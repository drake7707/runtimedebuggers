using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Reflection.Emit;
using System.IO;
using RunTimeDebuggers.Helpers;
using System.Runtime.InteropServices;
using System.Resources;

namespace RunTimeDebuggers.AssemblyExplorer
{
    public class DumpManager
    {

        public DumpManager()
        {

        }

        private Dictionary<Assembly, AssemblyBuilder> assemblyMapping = new Dictionary<Assembly, AssemblyBuilder>();
        private Dictionary<Module, ModuleBuilder> moduleMapping = new Dictionary<Module, ModuleBuilder>();
        private Dictionary<Type, Type> typeMapping = new Dictionary<Type, Type>();
        private Dictionary<FieldInfo, FieldBuilder> fieldMapping = new Dictionary<FieldInfo, FieldBuilder>();
        private Dictionary<PropertyInfo, PropertyBuilder> propertyMapping = new Dictionary<PropertyInfo, PropertyBuilder>();
        private Dictionary<MethodInfo, MethodBuilder> methodMapping = new Dictionary<MethodInfo, MethodBuilder>();
        private Dictionary<Type, GenericTypeParameterBuilder> genericTypeParameterMapping = new Dictionary<Type, GenericTypeParameterBuilder>();


        private Dictionary<ParameterInfo, ParameterBuilder> parameterMapping = new Dictionary<ParameterInfo, ParameterBuilder>();

        private Dictionary<EventInfo, EventBuilder> eventMapping = new Dictionary<EventInfo, EventBuilder>();
        private Dictionary<ConstructorInfo, ConstructorBuilder> constructorMapping = new Dictionary<ConstructorInfo, ConstructorBuilder>();


        private Dictionary<Type, Type> finishedTypes = new Dictionary<Type, Type>();

        public void Dump(Assembly assembly, string path)
        {


            CreateAssembly(assembly, path);

            CreateTypeAssociations(assembly);

            BuildAssembly(assembly);

            foreach (var m in methodMapping)
                BuildMethodBody(m.Key, m.Value);

            foreach (var c in constructorMapping)
                BuildConstructorBody(c.Key, c.Value);

            FinishAssembly(assembly);


        }

        private void CreateAssembly(Assembly a, string path)
        {
            Console.WriteLine("Creating assembly: " + a.FullName);
            AssemblyBuilder abuilder = AppDomain.CurrentDomain.DefineDynamicAssembly(new AssemblyName(a.FullName),
                                                                         AssemblyBuilderAccess.RunAndSave,
                                                                         path);
            assemblyMapping.Add(a, abuilder);

            bool resourcesDumped = false;
            foreach (Module m in a.GetModules())
            {
                CreateModule(m, a, abuilder, path, !resourcesDumped);
                resourcesDumped = true;
            }
        }

        private void CreateModule(Module m, Assembly a, AssemblyBuilder abuilder, string path, bool dumpResources)
        {
            Console.WriteLine("Creating module: " + m.Name);
            ModuleBuilder mbuilder = abuilder.DefineDynamicModule(m.Name, m.Name, true);
            moduleMapping.Add(m, mbuilder);

            if (dumpResources)
            {
                foreach (string resourceName in a.GetManifestResourceNames())
                {
                    Console.WriteLine("Dumping resources: " + resourceName);
                    ManifestResourceInfo info = a.GetManifestResourceInfo(resourceName);
                    if (resourceName.ToLower().EndsWith(".resources"))
                    {
                        var resourceBuilder = mbuilder.DefineResource(resourceName, resourceName, ResourceAttributes.Public);
                        ResourceReader reader = new ResourceReader(a.GetManifestResourceStream(resourceName));
                        var readerEnumerator = reader.GetEnumerator();
                        while (readerEnumerator.MoveNext())
                            resourceBuilder.AddResource((string)readerEnumerator.Key, readerEnumerator.Value);
                    }
                    else
                    {

                        mbuilder.DefineManifestResource(resourceName, a.GetManifestResourceStream(resourceName), ResourceAttributes.Public);
                        //var resourceBuilder = mbuilder.DefineResource(resourceName, resourceName, ResourceAttributes.Public);

                        //using (Stream s = a.GetManifestResourceStream(resourceName))
                        //{
                        //    byte[] bytes = s.ReadToEnd();

                        //    string resPath = Path.Combine(path, Path.GetRandomFileName());
                        //    File.WriteAllBytes(resPath, bytes);
                        //    abuilder.AddResourceFile(resourceName, Path.GetFileName(resPath));
                        //}
                    }
                }
            }


            foreach (Type t in m.GetTypesSafe().Where(t => !t.IsNested))
                CreateType(t, mbuilder, null);
        }

        private void CreateType(Type t, ModuleBuilder mbuilder, TypeBuilder parentType)
        {

            TypeBuilder tbuilder;
            if (parentType != null)
            {

                tbuilder = parentType.DefineNestedType("" + t.Name, t.Attributes);
            }
            else
            {
                string fullname = string.IsNullOrEmpty(t.Namespace) ? "" + t.Name : t.Namespace + "." + "" + t.Name;
                tbuilder = mbuilder.DefineType(fullname , t.Attributes);
            }

            typeMapping.Add(t, tbuilder);
            Console.WriteLine("Creating type: " + tbuilder.FullName);

            var sourceGenericArgs = t.GetGenericArguments();
            if (sourceGenericArgs.Length > 0)
            {
                var genericNames = sourceGenericArgs.Select(gt => gt.Name).ToArray();
                GenericTypeParameterBuilder[] gtpbuilder = tbuilder.DefineGenericParameters(genericNames);
                UpdateGenericTypeBuilder(sourceGenericArgs, gtpbuilder);
            }

            foreach (Type nestedType in t.GetNestedTypesOfType())
                CreateType(nestedType, mbuilder, tbuilder);
        }

        private void CreateTypeAssociations(Assembly a)
        {
            foreach (Module m in a.GetModules())
            {
                foreach (Type t in m.GetTypesSafe())
                {
                    TypeBuilder tbuilder = (TypeBuilder)typeMapping[t]; // always a type builder for types defined from module

                    if (t.BaseType != null) // base type requires all types have their generic types set!
                    {
                        if (t.BaseType.FullName != "System.__ComObject") // no com objects
                        {
                            Type targetBaseType = GetMatchingType(t.BaseType);
                            tbuilder.SetParent(targetBaseType);
                            Console.WriteLine("Assigning type association: " + tbuilder.FullName + " inherits from " + targetBaseType.FullName);
                        }
                    }

                    foreach (var iface in t.GetInterfacesOfType(false))
                    {
                        Type targetInterface = GetMatchingType(iface);
                        tbuilder.AddInterfaceImplementation(targetInterface);
                        Console.WriteLine("Assigning type association: " + tbuilder.FullName + " implements " + targetInterface.FullName);
                    }
                }
            }
        }

        private void BuildAssembly(Assembly a)
        {
            AssemblyBuilder abuilder = assemblyMapping[a];
            Console.WriteLine("Building assembly: " + abuilder.FullName);

            foreach (Module m in a.GetModules())
                BuildModule(m, abuilder);

            if (a.EntryPoint != null)
                abuilder.SetEntryPoint(methodMapping[a.EntryPoint]);
        }

        private void BuildModule(Module m, AssemblyBuilder abuilder)
        {
            ModuleBuilder mbuilder = moduleMapping[m];

            Console.WriteLine("Building module: " + mbuilder.Name);
            foreach (Type t in m.GetTypesSafe())
                BuildType(t, mbuilder);
        }

        private void BuildType(Type t, ModuleBuilder mbuilder)
        {
            TypeBuilder tbuilder = (TypeBuilder)typeMapping[t]; // always a type builder for types defined from module
            Console.WriteLine("Building type: " + tbuilder.FullName);

            // ensure all type inheritance is set before building methods, passing parameters byval or byref is based
            // on the inheritance chain of ValueType
            foreach (FieldInfo f in t.GetFieldsOfType(false, true))
                BuildField(f, tbuilder);



            foreach (MethodInfo m in t.GetMethodsOfType(false, true, true))
                BuildMethod(m, tbuilder);

            foreach (ConstructorInfo c in t.GetConstructorsOfType(true))
                BuildConstructor(c, tbuilder);

            foreach (PropertyInfo p in t.GetPropertiesOfType(false, true))
                BuildProperty(p, tbuilder);

        }

        private void BuildField(FieldInfo f, TypeBuilder tbuilder)
        {
            FieldBuilder fbuilder = tbuilder.DefineField(f.Name, GetMatchingType(f.FieldType), f.Attributes);
            if (f.IsLiteral)
                fbuilder.SetConstant(f.GetRawConstantValue());

            fieldMapping.Add(f, fbuilder);
            Console.WriteLine("Defining field: " + fbuilder.Name);
        }

        private void BuildProperty(PropertyInfo p, TypeBuilder tbuilder)
        {
            Type[] parameterTypes = p.GetIndexParameters().Select(par => GetMatchingType(par.ParameterType)).ToArray();
            PropertyBuilder pbuilder = tbuilder.DefineProperty(p.Name, p.Attributes, GetMatchingType(p.PropertyType), parameterTypes);
            propertyMapping.Add(p, pbuilder);
            Console.WriteLine("Defining property: " + pbuilder.Name);

            var sourceGetMethod = p.GetGetMethod(true);
            if (p.CanRead && sourceGetMethod != null)
                pbuilder.SetGetMethod(methodMapping[sourceGetMethod]);

            var sourceSetMethod = p.GetSetMethod(true);
            if (p.CanWrite && sourceSetMethod != null)
                pbuilder.SetSetMethod(methodMapping[sourceSetMethod]);
        }



        private void BuildPInvokeMethod(MethodInfo m, TypeBuilder tbuilder)
        {
            ParameterInfo[] sourceParameters = m.GetParameters();

            Type targetReturnType = GetMatchingType(m.ReturnType);
            Type[] targetParameterTypes = sourceParameters.Select(par => GetMatchingType(par.ParameterType)).ToArray();

            var dllImportAttribute = (DllImportAttribute)m.GetCustomAttributes(typeof(DllImportAttribute), true).FirstOrDefault();
            MethodBuilder mbuilder = tbuilder.DefinePInvokeMethod(m.Name, dllImportAttribute.Value, m.Attributes, m.CallingConvention, targetReturnType, targetParameterTypes, dllImportAttribute.CallingConvention, dllImportAttribute.CharSet);
            mbuilder.SetImplementationFlags(m.GetMethodImplementationFlags());

            var sourceGenericArgs = m.GetGenericArguments();
            if (sourceGenericArgs.Length > 0)
            {
                var genericNames = sourceGenericArgs.Select(t => t.Name).ToArray();
                GenericTypeParameterBuilder[] gtpbuilder = mbuilder.DefineGenericParameters(genericNames);

                //Dictionary<Type, GenericTypeParameterBuilder> genericParamsMapping = new Dictionary<Type, GenericTypeParameterBuilder>();
                UpdateGenericTypeBuilder(sourceGenericArgs, gtpbuilder);
            }

            mbuilder.SetSignature(targetReturnType, null, null, targetParameterTypes, null, null);

            foreach (ParameterInfo p in sourceParameters)
            {
                BuildParameter(p, mbuilder);
            }

            methodMapping.Add(m, mbuilder);

            Console.WriteLine("Defining P/Invoke method: " + mbuilder.Name);
            //MethodBuilder mbuilder = tbuilder.DefinePInvokeMethod();
            //System.Diagnostics.Debugger.Break();
        }

        private void BuildMethod(MethodInfo m, TypeBuilder tbuilder)
        {
            if ((m.Attributes & MethodAttributes.PinvokeImpl) == MethodAttributes.PinvokeImpl)
            {
                BuildPInvokeMethod(m, tbuilder);
                return;
            }

            MethodBuilder mbuilder = tbuilder.DefineMethod(m.Name, m.Attributes, m.CallingConvention);

            //Type targetReturnType = GetType(m.ReturnType);
            //Type[] targetParameterTypes = m.GetParameters().Select(par => GetType(par.ParameterType)).ToArray();



            ParameterInfo[] sourceParameters = m.GetParameters();

            Type targetReturnType;
            Type[] targetParameterTypes = new Type[sourceParameters.Length];


            var sourceGenericArgs = m.GetGenericArguments();
            if (sourceGenericArgs.Length > 0)
            {
                var genericNames = sourceGenericArgs.Select(t => t.Name).ToArray();
                GenericTypeParameterBuilder[] gtpbuilder = mbuilder.DefineGenericParameters(genericNames);

                //Dictionary<Type, GenericTypeParameterBuilder> genericParamsMapping = new Dictionary<Type, GenericTypeParameterBuilder>();
                UpdateGenericTypeBuilder(sourceGenericArgs, gtpbuilder);
            }

            targetReturnType = GetMatchingType(m.ReturnType);
            targetParameterTypes = sourceParameters.Select(par => GetMatchingType(par.ParameterType)).ToArray();

            mbuilder.SetSignature(targetReturnType, null, null, targetParameterTypes, null, null);
            mbuilder.SetImplementationFlags(m.GetMethodImplementationFlags());

            foreach (ParameterInfo p in sourceParameters)
            {
                BuildParameter(p, mbuilder);
            }

            methodMapping.Add(m, mbuilder);
            Console.WriteLine("Defining method: " + mbuilder.Name);
        }

        private void BuildConstructor(ConstructorInfo c, TypeBuilder tbuilder)
        {

            ParameterInfo[] sourceParameters = c.GetParameters();
            Type[] targetParameterTypes = sourceParameters.Select(par => GetMatchingType(par.ParameterType)).ToArray();

            ConstructorBuilder cbuilder = tbuilder.DefineConstructor(c.Attributes, c.CallingConvention, targetParameterTypes);

            foreach (ParameterInfo p in sourceParameters)
            {
                BuildParameter(p, cbuilder);
            }

            cbuilder.SetImplementationFlags(c.GetMethodImplementationFlags());
            constructorMapping.Add(c, cbuilder);

            Console.WriteLine("Defining constructor: " + cbuilder.Name);
        }


        private void UpdateGenericTypeBuilder(Type[] sourceGenericArgs, GenericTypeParameterBuilder[] gtpbuilder)
        {
            for (int i = 0; i < sourceGenericArgs.Length; i++)
            {
                var genericParameterConstraints = sourceGenericArgs[i].GetGenericParameterConstraints();

                Type baseTypeConstraint = genericParameterConstraints.Where(t => !t.IsInterface).FirstOrDefault();
                if (baseTypeConstraint != null)
                    gtpbuilder[i].SetBaseTypeConstraint(baseTypeConstraint);

                gtpbuilder[i].SetInterfaceConstraints(genericParameterConstraints.Where(t => t.IsInterface).ToArray());
                gtpbuilder[i].SetGenericParameterAttributes(sourceGenericArgs[i].GenericParameterAttributes);

                //genericParamsMapping.Add(sourceGenericArgs[i], gtpbuilder[i]);
                genericTypeParameterMapping.Add(sourceGenericArgs[i], gtpbuilder[i]);
            }
        }


        private void BuildConstructorBody(ConstructorInfo c, ConstructorBuilder cbuilder)
        {
            try
            {
                if (c.GetMethodBody() != null)
                {
                    ILGenerator ilGen = cbuilder.GetILGenerator();
                    BuildBody(c, ilGen);
                }
            }
            catch (Exception)
            {

                throw;
            }
        }


        private void BuildMethodBody(MethodInfo m, MethodBuilder mbuilder)
        {
            try
            {
                if ((m.Attributes & MethodAttributes.PinvokeImpl) == MethodAttributes.PinvokeImpl)
                {
                    // p invoke method has no body
                }
                else
                {
                    if (m.GetMethodBody() != null)
                    {
                        ILGenerator ilGen = mbuilder.GetILGenerator();
                        BuildBody(m, ilGen);
                    }
                }
            }
            catch (Exception)
            {

                throw;
            }
        }

        private void BuildBody(MethodBase m, ILGenerator ilGen)
        {
            Console.WriteLine("Building method body for " + m.Name);

            List<ILInstruction> instructions = m.GetILInstructions();

            foreach (var local in m.GetMethodBody().LocalVariables
                                                   .OrderBy(l => l.LocalIndex))
            {

                Type targetLocalType = GetMatchingType(local.LocalType);
                ilGen.DeclareLocal(targetLocalType, local.IsPinned);
            }

            Dictionary<int, Label> labelsByOffset = instructions.Where(i => (i.Code.FlowControl == FlowControl.Branch || i.Code.FlowControl == FlowControl.Cond_Branch) &&
                                                                            (i.Code.OperandType != OperandType.InlineSwitch))
                                                                .GroupBy(i => Convert.ToInt32(i.Operand))
                                                                .ToDictionary(g => g.Key, g => ilGen.DefineLabel());

            var labelOffsetsOfInlineSwitches = instructions.Where(i => (i.Code.FlowControl == FlowControl.Branch || i.Code.FlowControl == FlowControl.Cond_Branch) &&
                                                                 (i.Code.OperandType == OperandType.InlineSwitch))
                                                     .SelectMany(i => (int[])(i.Operand))
                                                     .GroupBy(i => i)
                                                     .Select(g => g.First())
                                                     .ToList();

            foreach (var lblOffset in labelOffsetsOfInlineSwitches)
            {
                if (!labelsByOffset.ContainsKey(lblOffset))
                    labelsByOffset[lblOffset] = ilGen.DefineLabel();
            }

            var exceptionClauses = m.GetMethodBody().ExceptionHandlingClauses;

            //var tryClausesByOffset = exceptionClauses.GroupBy(ex => ex.TryOffset)
            //                                         .ToDictionary(g => g.Key, g => g.OrderBy(ex => ex.HandlerOffset + ex.HandlerLength).Last());

            //var tryClausesByEndOffset = tryClausesByOffset.Values
            //                                .GroupBy(ex => ex.HandlerOffset + ex.HandlerLength)
            //                                .ToDictionary(g => g.Key, g => g.OrderByDescending(ex => ex.TryOffset).ToList());

            var allTries = exceptionClauses.GroupBy(ex => ex.TryOffset + "_" + ex.TryLength)
                                                 .ToDictionary(g => g.Key, g => g.Last());

            // sort by trylength descending to define the outer try first!
            var tryClausesByOffset = allTries.Values.GroupBy(t => t.TryOffset)
                                                .ToDictionary(g => g.Key, g => g.OrderByDescending(ex => ex.TryLength).ToList());


            //Dictionary<int, List<ExceptionHandlingClause>> tryClauses = new Dictionary<int, List<ExceptionHandlingClause>>();
            //HashSet<string> clausesDone = new HashSet<string>();
            //foreach (var ex in exceptionClauses)
            //{
            //    if (!clausesDone.Contains(ex.TryOffset + "_" + ex.TryLength))
            //    {
            //        List<ExceptionHandlingClause> lst;
            //        if (!tryClauses.TryGetValue(ex.TryOffset, out lst))
            //            tryClauses.Add(ex.TryOffset, lst = new List<ExceptionHandlingClause>());
            //        lst.Add(ex);
            //        clausesDone.Add(ex.TryOffset + "_" + ex.TryLength);
            //    }
            //}

            var tryClausesByEndOffset = tryClausesByOffset.Values.SelectMany(ex => ex)
                                                                 .GroupBy(ex => ex.HandlerOffset + ex.HandlerLength)
                                                                 .ToDictionary(g => g.Key, g => g.OrderByDescending(ex => ex.TryOffset).ToList());
            //exceptionClauses
            //    .GroupBy(ex => ex.HandlerOffset + ex.HandlerLength)
            //    .ToDictionary(g => g.Key, g => g.OrderByDescending(ex => ex.TryOffset).ToList());

            var catchClausesByOffset = exceptionClauses.Where(ex => ex.Flags == ExceptionHandlingClauseOptions.Clause ||
                                                                    ex.Flags == ExceptionHandlingClauseOptions.Filter)
                                                     .ToDictionary(ex => ex.HandlerOffset, ex => ex);

            var faultClausesByOffset = exceptionClauses.Where(ex => ex.Flags == ExceptionHandlingClauseOptions.Fault)
                                         .ToDictionary(ex => ex.HandlerOffset, ex => ex);

            var finallyClausesByOffset = exceptionClauses.Where(ex => ex.Flags == ExceptionHandlingClauseOptions.Finally)
                                                     .ToDictionary(ex => ex.HandlerOffset, ex => ex);

            var filterClausesByOffset = exceptionClauses.Where(ex => ex.Flags == ExceptionHandlingClauseOptions.Filter)
                                                  .ToDictionary(ex => ex.FilterOffset, ex => ex);



            foreach (ILInstruction instruction in instructions)
            {
                OpCode code = instruction.Code; //  GetLongForm(instruction);

                int curOffset = (int)ilGen.GetType().GetField("m_length", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(ilGen);
                if (curOffset != instruction.Offset && System.Diagnostics.Debugger.IsAttached)
                    System.Diagnostics.Debugger.Break();

                // always end exceptions first before starting new ones on same offset line
                List<ExceptionHandlingClause> endEx;
                if (tryClausesByEndOffset.TryGetValue(instruction.Offset, out endEx))
                {
                    foreach (var exc in endEx)
                        EndException(ilGen, exc);
                    //ilGen.EndExceptionBlock();
                }

                ExceptionHandlingClause ex;
                if (faultClausesByOffset.TryGetValue(instruction.Offset, out ex))
                    BeginFault(ilGen, ex);
                //ilGen.BeginFaultBlock();

                if (catchClausesByOffset.TryGetValue(instruction.Offset, out ex))
                    BeginCatch(ilGen, ex);
                //ilGen.BeginCatchBlock(GetMatchingType(ex.CatchType));

                if (finallyClausesByOffset.TryGetValue(instruction.Offset, out ex))
                    BeginFinally(ilGen, ex);
                //ilGen.BeginFinallyBlock();

                if (filterClausesByOffset.TryGetValue(instruction.Offset, out ex))
                    BeginFilter(ilGen, ex);
                //ilGen.BeginFilterBlock();


                // do start of try after all other clauses, start of try and start of clause -> clause is still
                // clause of previous try and start of try is inside clause

                //ExceptionHandlingClause ex;
                //if (tryClausesByOffset.TryGetValue(instruction.Offset, out ex))
                //    BeginTry(ilGen, ex);

                List<ExceptionHandlingClause> beginEx;
                if (tryClausesByOffset.TryGetValue(instruction.Offset, out beginEx))
                {
                    foreach (var exc in beginEx)
                        BeginTry(ilGen, exc);
                }
                //ilGen.BeginExceptionBlock();

                Label l;
                if (labelsByOffset.TryGetValue(instruction.Offset, out l))
                    ilGen.MarkLabel(l);

                if (instruction.Code.FlowControl == FlowControl.Branch || instruction.Code.FlowControl == FlowControl.Cond_Branch)
                {
                    if (instruction.Code.OperandType == OperandType.InlineSwitch)
                    {
                        Label[] labels = ((int[])instruction.Operand).Select(i => labelsByOffset[i])
                                                                     .ToArray();

                        ilGen.Emit(code, labels);
                    }
                    else
                    {

                        ilGen.Emit(code, labelsByOffset[Convert.ToInt32(instruction.Operand)]);
                    }
                }
                else
                {
                    if (instruction.Operand == null)
                        ilGen.Emit(code);
                    else if (instruction.Operand is byte)
                        ilGen.Emit(code, (byte)instruction.Operand);
                    else if (instruction.Operand is sbyte)
                        ilGen.Emit(code, (sbyte)instruction.Operand);
                    else if (instruction.Operand is short)
                        ilGen.Emit(code, (short)instruction.Operand);
                    else if (instruction.Operand is int)
                        ilGen.Emit(code, (int)instruction.Operand);
                    else if (instruction.Operand is FieldInfo)
                    {
                        var targetOperandField = GetMatchingField(m, (FieldInfo)instruction.Operand);
                        ilGen.Emit(code, targetOperandField);
                    }
                    else if (instruction.Operand is MethodInfo)
                    {
                        var targetOperandMethod = GetMatchingMethod(m, (MethodInfo)instruction.Operand);
                        ilGen.Emit(code, targetOperandMethod);
                    }
                    else if (instruction.Operand is ConstructorInfo)
                    {
                        var targetOperandConstructor = GetMatchingConstructor(m, (ConstructorInfo)instruction.Operand);
                        ilGen.Emit(code, targetOperandConstructor);
                    }
                    else if (instruction.Operand is Type)
                    {
                        Type sourceType = (Type)instruction.Operand;
                        Type targetType = GetMatchingType(sourceType);
                        ilGen.Emit(code, targetType);
                    }
                    else if (instruction.Operand is long)
                        ilGen.Emit(code, (long)instruction.Operand);
                    else if (instruction.Operand is float)
                        ilGen.Emit(code, (float)instruction.Operand);
                    else if (instruction.Operand is double)
                        ilGen.Emit(code, (double)instruction.Operand);
                    else if (instruction.Operand is string)
                        ilGen.Emit(code, (string)instruction.Operand);

                }



            }
            try
            {
                ilGen.GetType().GetMethod("BakeByteArray", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(ilGen, null);
            }
            catch (Exception)
            {

                throw;
            }
        }

        private static void BeginTry(ILGenerator ilGen, ExceptionHandlingClause ex)
        {
            var m_exceptionCount = ilGen.GetField<int>("m_exceptionCount");
            var m_currExcStackCount = ilGen.GetField<int>("m_currExcStackCount");
            Type exceptionType = Assembly.GetAssembly(ilGen.GetType()).GetType("System.Reflection.Emit.__ExceptionInfo");
            Array m_exceptions = ilGen.GetField<Array>("m_exceptions");
            Array m_currExcStack = ilGen.GetField<Array>("m_currExcStack");
            Type ilGenType = ilGen.GetType();
            MethodInfo enlargeArray = ilGenType.GetMethod("EnlargeArray", BindingFlags.NonPublic | BindingFlags.Static, null, new Type[] { exceptionType.MakeArrayType() }, null);
            ConstructorInfo exceptionInfoCtor = exceptionType.GetConstructor(BindingFlags.NonPublic | BindingFlags.Instance, null, new Type[] { typeof(int), typeof(Label) }, null);

            // begin of method similar to ILGenerator BeginExceptionBlock

            //////if (this.m_exceptions == null)
            //////    this.m_exceptions = new __ExceptionInfo[8];
            if (m_exceptions == null)
            {
                m_exceptions = Array.CreateInstance(exceptionType, 8);
                ilGen.SetField("m_exceptions", m_exceptions);
            }

            //////if (this.m_currExcStack == null)
            //////    this.m_currExcStack = new __ExceptionInfo[8];
            if (m_currExcStack == null)
            {
                m_currExcStack = Array.CreateInstance(exceptionType, 8);
                ilGen.SetField("m_currExcStack", m_currExcStack);
            }

            //////if (this.m_exceptionCount >= this.m_exceptions.Length)
            //////    this.m_exceptions = ILGenerator.EnlargeArray(this.m_exceptions);
            if (m_exceptionCount >= m_exceptions.Length)
            {
                m_exceptions = (Array)enlargeArray.Invoke(null, new object[] { m_exceptions });
                ilGen.SetField("m_exceptions", m_exceptions);
            }

            //////if (this.m_currExcStackCount >= this.m_currExcStack.Length)
            //////    this.m_currExcStack = ILGenerator.EnlargeArray(this.m_currExcStack);
            if (m_exceptionCount >= m_currExcStack.Length)
            {
                m_currExcStack = (Array)enlargeArray.Invoke(null, new object[] { m_currExcStack });
                ilGen.SetField("m_currExcStack", m_currExcStack);
            }

            //////__ExceptionInfo _ExceptionInfo = new __ExceptionInfo(this.m_length, label);
            object exceptionInfo = exceptionInfoCtor.Invoke(new object[] { ex.TryOffset, default(Label) });

            //////this.m_exceptions[this.m_exceptionCount++] = _ExceptionInfo;
            m_exceptions.SetValue(exceptionInfo, m_exceptionCount);
            m_exceptionCount++;
            ilGen.SetField("m_exceptionCount", m_exceptionCount);

            //////this.m_currExcStack[this.m_currExcStackCount++] = _ExceptionInfo;
            m_currExcStack.SetValue(exceptionInfo, m_currExcStackCount);
            m_currExcStackCount++;
            ilGen.SetField("m_currExcStackCount", m_currExcStackCount);
        }

        private static void BeginCatch(ILGenerator ilGen, ExceptionHandlingClause ex)
        {
            var m_currExcStackCount = ilGen.GetField<int>("m_currExcStackCount");
            Type exceptionType = Assembly.GetAssembly(ilGen.GetType()).GetType("System.Reflection.Emit.__ExceptionInfo");
            Array m_currExcStack = ilGen.GetField<Array>("m_currExcStack");
            MethodInfo getCurrentState = exceptionType.GetMethod("GetCurrentState", BindingFlags.NonPublic | BindingFlags.Instance);
            MethodInfo markCatchAddr = exceptionType.GetMethod("MarkCatchAddr", BindingFlags.NonPublic | BindingFlags.Instance);

            //////if (this.m_currExcStackCount == 0)
            //////    throw new NotSupportedException(Environment.GetResourceString("Argument_NotInExceptionBlock"));
            if (m_currExcStackCount == 0)
                throw new NotSupportedException("Not in exception block");

            //////__ExceptionInfo _ExceptionInfo = this.m_currExcStack[this.m_currExcStackCount - 1];
            object exceptionInfo = m_currExcStack.GetValue(m_currExcStackCount - 1);


            Type catchType = (ex.Flags == ExceptionHandlingClauseOptions.Clause ? ex.CatchType : null);

            //////if (_ExceptionInfo.GetCurrentState() == 1)
            if ((int)getCurrentState.Invoke(exceptionInfo, null) == 1) // filter
            {
                //////if (exceptionType != null)
                //////    throw new ArgumentException(Environment.GetResourceString("Argument_ShouldNotSpecifyExceptionType"));
                //////this.Emit(OpCodes.Endfilter);
                if (catchType != null)
                    throw new ArgumentException("Exception type should not be specified");

                // don't emit!
            }
            else
            {
                //////if (exceptionType == null)
                //////    throw new ArgumentNullException("exceptionType");

                //////Label endLabel = _ExceptionInfo.GetEndLabel();
                //////this.Emit(OpCodes.Leave, endLabel);
                if (catchType == null)
                    throw new ArgumentNullException("exceptionType");

                // don't emit!
            }

            //////_ExceptionInfo.MarkCatchAddr(this.m_length, exceptionType);
            markCatchAddr.Invoke(exceptionInfo, new object[] { ex.HandlerOffset, catchType });
        }

        private static void BeginFault(ILGenerator ilGen, ExceptionHandlingClause ex)
        {
            var m_currExcStackCount = ilGen.GetField<int>("m_currExcStackCount");
            Type exceptionType = Assembly.GetAssembly(ilGen.GetType()).GetType("System.Reflection.Emit.__ExceptionInfo");
            Array m_currExcStack = ilGen.GetField<Array>("m_currExcStack");
            MethodInfo getCurrentState = exceptionType.GetMethod("GetCurrentState", BindingFlags.NonPublic | BindingFlags.Instance);
            MethodInfo markFaultAddr = exceptionType.GetMethod("MarkFaultAddr", BindingFlags.NonPublic | BindingFlags.Instance);

            //////if (this.m_currExcStackCount == 0)
            //////    throw new NotSupportedException(Environment.GetResourceString("Argument_NotInExceptionBlock"));
            if (m_currExcStackCount == 0)
                throw new NotSupportedException("Not in exception block");

            //////__ExceptionInfo _ExceptionInfo = this.m_currExcStack[this.m_currExcStackCount - 1];
            object exceptionInfo = m_currExcStack.GetValue(m_currExcStackCount - 1);

            //////Label endLabel = _ExceptionInfo.GetEndLabel();
            //////this.Emit(OpCodes.Leave, endLabel);

            // don't emit!

            //////_ExceptionInfo.MarkFaultAddr(this.m_length);
            markFaultAddr.Invoke(exceptionInfo, new object[] { ex.HandlerOffset });
        }

        private static void BeginFinally(ILGenerator ilGen, ExceptionHandlingClause ex)
        {
            var m_currExcStackCount = ilGen.GetField<int>("m_currExcStackCount");
            Type exceptionType = Assembly.GetAssembly(ilGen.GetType()).GetType("System.Reflection.Emit.__ExceptionInfo");
            Array m_currExcStack = ilGen.GetField<Array>("m_currExcStack");
            MethodInfo getCurrentState = exceptionType.GetMethod("GetCurrentState", BindingFlags.NonPublic | BindingFlags.Instance);
            MethodInfo markFinallyAddr = exceptionType.GetMethod("MarkFinallyAddr", BindingFlags.NonPublic | BindingFlags.Instance);

            //////if (this.m_currExcStackCount == 0)
            //////    throw new NotSupportedException(Environment.GetResourceString("Argument_NotInExceptionBlock"));
            if (m_currExcStackCount == 0)
                throw new NotSupportedException("Not in exception block");

            //////__ExceptionInfo _ExceptionInfo = this.m_currExcStack[this.m_currExcStackCount - 1];
            object exceptionInfo = m_currExcStack.GetValue(m_currExcStackCount - 1);

            //////int currentState = _ExceptionInfo.GetCurrentState();
            int currentState = (int)getCurrentState.Invoke(exceptionInfo, null);

            //////int num = 0;
            //////if (currentState != 0)
            //////{
            //////    this.Emit(OpCodes.Leave, endLabel);
            //////    num = this.m_length;
            //////}
            int num = 0;
            if (currentState != 0) // State_Try
            {
                // don't emit!
                num = ex.HandlerOffset;
            }

            //////this.MarkLabel(endLabel);
            //////Label label = this.DefineLabel();
            //////_ExceptionInfo.SetFinallyEndLabel(label);
            //////this.Emit(OpCodes.Leave, label);
            // don't emit!

            //////if (num == 0)
            //////    num = this.m_length;
            if (num == 0)
                num = ex.HandlerOffset; // well still same offset, because it doesn't emit

            //////_ExceptionInfo.MarkFinallyAddr(this.m_length, num);
            markFinallyAddr.Invoke(exceptionInfo, new object[] { ex.HandlerOffset, num }); // TODO , might be incorrect

        }

        private static void BeginFilter(ILGenerator ilGen, ExceptionHandlingClause ex)
        {
            var m_currExcStackCount = ilGen.GetField<int>("m_currExcStackCount");
            Type exceptionType = Assembly.GetAssembly(ilGen.GetType()).GetType("System.Reflection.Emit.__ExceptionInfo");
            Array m_currExcStack = ilGen.GetField<Array>("m_currExcStack");
            MethodInfo getCurrentState = exceptionType.GetMethod("GetCurrentState", BindingFlags.NonPublic | BindingFlags.Instance);
            MethodInfo markFilterAddr = exceptionType.GetMethod("MarkFilterAddr", BindingFlags.NonPublic | BindingFlags.Instance);

            //////if (this.m_currExcStackCount == 0)
            //////    throw new NotSupportedException(Environment.GetResourceString("Argument_NotInExceptionBlock"));
            if (m_currExcStackCount == 0)
                throw new NotSupportedException("Not in exception block");

            //////__ExceptionInfo _ExceptionInfo = this.m_currExcStack[this.m_currExcStackCount - 1];
            object exceptionInfo = m_currExcStack.GetValue(m_currExcStackCount - 1);

            //////Label endLabel = _ExceptionInfo.GetEndLabel();
            //////this.Emit(OpCodes.Leave, endLabel);
            // don't emit


            //////_ExceptionInfo.MarkFilterAddr(this.m_length);
            markFilterAddr.Invoke(exceptionInfo, new object[] { ex.FilterOffset });
        }

        private static void EndException(ILGenerator ilGen, ExceptionHandlingClause ex)
        {
            var m_currExcStackCount = ilGen.GetField<int>("m_currExcStackCount");
            Type exceptionType = Assembly.GetAssembly(ilGen.GetType()).GetType("System.Reflection.Emit.__ExceptionInfo");
            Array m_currExcStack = ilGen.GetField<Array>("m_currExcStack");
            MethodInfo getCurrentState = exceptionType.GetMethod("GetCurrentState", BindingFlags.NonPublic | BindingFlags.Instance);
            MethodInfo done = exceptionType.GetMethod("Done", BindingFlags.NonPublic | BindingFlags.Instance);

            //////if (this.m_currExcStackCount == 0)
            //////    throw new NotSupportedException(Environment.GetResourceString("Argument_NotInExceptionBlock"));
            if (m_currExcStackCount == 0)
                throw new NotSupportedException("Not in exception block");

            //////__ExceptionInfo _ExceptionInfo = this.m_currExcStack[this.m_currExcStackCount - 1];
            object exceptionInfo = m_currExcStack.GetValue(m_currExcStackCount - 1);

            //////this.m_currExcStack[this.m_currExcStackCount - 1] = null;
            m_currExcStack.SetValue(null, m_currExcStackCount - 1);

            //////this.m_currExcStackCount--;
            m_currExcStackCount--;
            ilGen.SetField("m_currExcStackCount", m_currExcStackCount);


            //////Label endLabel = _ExceptionInfo.GetEndLabel();
            //////int currentState = _ExceptionInfo.GetCurrentState();
            int currentState = (int)getCurrentState.Invoke(exceptionInfo, null);

            //////if (currentState == 1 || currentState == 0)
            //////    throw new InvalidOperationException(Environment.GetResourceString("Argument_BadExceptionCodeGen"));
            if (currentState == 1 || currentState == 0)
                throw new InvalidOperationException("Bad code generation");


            //////if (currentState == 2)
            //////    this.Emit(OpCodes.Leave, endLabel);
            //////else
            //////{
            //////    if (currentState == 3 || currentState == 4)
            //////        this.Emit(OpCodes.Endfinally);
            //////}
            if (currentState == 2)
            {
                // don't emit
            }
            else
            {
                if (currentState == 3 || currentState == 4)
                {
                    // don't emit
                }
            }

            //////if (this.m_labelList[endLabel.GetLabelValue()] == -1)
            //////    this.MarkLabel(endLabel);
            //////else
            //////    this.MarkLabel(_ExceptionInfo.GetFinallyEndLabel());
            // don't label anything

            //////_ExceptionInfo.Done(this.m_length);
            done.Invoke(exceptionInfo, new object[] { ex.HandlerOffset + ex.HandlerLength });
        }

        private static OpCode GetLongForm(ILInstruction instruction)
        {
            OpCode longFormCode = instruction.Code;

            if (instruction.Code == OpCodes.Br_S)
                longFormCode = OpCodes.Br;
            else if (instruction.Code == OpCodes.Brfalse_S)
                longFormCode = OpCodes.Brfalse;
            else if (instruction.Code == OpCodes.Brtrue_S)
                longFormCode = OpCodes.Brtrue;
            else if (instruction.Code == OpCodes.Ble_S)
                longFormCode = OpCodes.Ble;
            else if (instruction.Code == OpCodes.Ble_Un_S)
                longFormCode = OpCodes.Ble_Un;
            else if (instruction.Code == OpCodes.Blt_S)
                longFormCode = OpCodes.Blt;
            else if (instruction.Code == OpCodes.Blt_Un_S)
                longFormCode = OpCodes.Blt_Un;
            else if (instruction.Code == OpCodes.Bge_S)
                longFormCode = OpCodes.Bge;
            else if (instruction.Code == OpCodes.Bge_Un_S)
                longFormCode = OpCodes.Bge_Un;
            else if (instruction.Code == OpCodes.Bgt_S)
                longFormCode = OpCodes.Bgt;
            else if (instruction.Code == OpCodes.Bgt_Un_S)
                longFormCode = OpCodes.Bgt_Un;
            else if (instruction.Code == OpCodes.Bne_Un_S)
                longFormCode = OpCodes.Bne_Un;
            else if (instruction.Code == OpCodes.Beq_S)
                longFormCode = OpCodes.Beq;
            else if (instruction.Code == OpCodes.Ldarg_S)
                longFormCode = OpCodes.Ldarg;
            else if (instruction.Code == OpCodes.Starg_S)
                longFormCode = OpCodes.Starg;
            else if (instruction.Code == OpCodes.Ldloca_S)
                longFormCode = OpCodes.Ldloca;
            else if (instruction.Code == OpCodes.Ldloc_S)
                instruction.Code = OpCodes.Ldloc;
            else if (instruction.Code == OpCodes.Stloc_S)
                instruction.Code = OpCodes.Stloc;
            else if (instruction.Code == OpCodes.Ldarga_S)
                instruction.Code = OpCodes.Ldarga;
            else if (instruction.Code == OpCodes.Ldc_I4_S)
                instruction.Code = OpCodes.Ldc_I4;
            else if (instruction.Code == OpCodes.Leave_S)
                instruction.Code = OpCodes.Leave;
            else
                longFormCode = instruction.Code;
            return longFormCode;
        }

        private void BuildParameter(ParameterInfo p, MethodBuilder mbuilder)
        {
            ParameterBuilder pbuilder = mbuilder.DefineParameter(p.Position, p.Attributes, p.Name);
            parameterMapping.Add(p, pbuilder);
        }

        private void BuildParameter(ParameterInfo p, ConstructorBuilder cbuilder)
        {
            ParameterBuilder pbuilder = cbuilder.DefineParameter(p.Position, p.Attributes, p.Name);
            parameterMapping.Add(p, pbuilder);
        }

        private void FinishAssembly(Assembly a)
        {


            AssemblyBuilder abuilder = assemblyMapping[a];
            Console.WriteLine("Finishing assembly: " + abuilder.FullName);
            foreach (Module m in a.GetModules())
                FinishModule(m, abuilder);

            abuilder.Save(Path.GetFileName(a.Location));
        }

        private void FinishModule(Module m, AssemblyBuilder abuilder)
        {
            ModuleBuilder mbuilder = moduleMapping[m];
            Console.WriteLine("Finishing module: " + mbuilder.Name);
            foreach (Type t in m.GetTypesSafe())
                FinishType(t, new HashSet<Type>());


            mbuilder.CreateGlobalFunctions();
        }

        private void FinishType(Type t, HashSet<Type> trackedTypes)
        {
            if (finishedTypes.ContainsKey(t))
                return;

            if (trackedTypes.Contains(t))
                return;

            trackedTypes.Add(t);

            Type targetType;
            if (typeMapping.TryGetValue(t, out targetType) && targetType is TypeBuilder)
            {
                TypeBuilder tbuilder = ((TypeBuilder)targetType);

                Console.WriteLine("Finishing type: " + targetType.Name);

                HashSet<InterfaceMapping> implementedInterfaceMethods = new HashSet<InterfaceMapping>(t.GetInterfacesOfType(true)
                                                                            .Select(i => t.GetInterfaceMap(i)));

                foreach (var imapping in implementedInterfaceMethods)
                {
                    for (int i = 0; i < imapping.TargetMethods.Length; i++)
                    {
                        var method = imapping.TargetMethods[i];
                        var interfaceMethod = GetMatchingMethod(null, imapping.InterfaceMethods[i]);
                        if (method.DeclaringType == t)
                            tbuilder.DefineMethodOverride(GetMatchingMethod(null, method), interfaceMethod);
                    }
                }

                // create base type first
                if (t.BaseType != null)
                    FinishType(t.BaseType, trackedTypes);

                // create interfaces first
                foreach (var iface in t.GetInterfacesOfType(true))
                    FinishType(iface, trackedTypes);

                // nested value types need to be created first, see remarks on

                // http://msdn.microsoft.com/en-us/library/system.reflection.emit.typebuilder.createtype(v=vs.95).aspx
                foreach (var f in t.GetFieldsOfType(false, true).Where(f => f.FieldType.IsValueType))
                {
                    var fieldType = f.FieldType.IsGenericType && !f.FieldType.IsGenericTypeDefinition ? f.FieldType.GetGenericTypeDefinition() : f.FieldType;

                    if (assemblyMapping.ContainsKey(fieldType.Assembly))
                        FinishType(fieldType, trackedTypes);
                }

                var finishedType = tbuilder.CreateType();
                finishedTypes.Add(t, finishedType);
            }
            else
            {
                // it's a type from another assembly, but it may contain generic parameters that are from the building assembly
                if (t.IsGenericType && !t.IsGenericTypeDefinition)
                {
                    foreach (var genType in t.GetGenericArguments())
                        FinishType(genType, trackedTypes);
                }
            }
        }

        private FieldInfo GetMatchingField(MethodBase parentMethod, FieldInfo sourceField)
        {
            if (assemblyMapping.ContainsKey(sourceField.DeclaringType.Assembly))
            {
                // it's a ctor defined in the creating assembly

                if (sourceField.DeclaringType.IsGenericType && !sourceField.DeclaringType.IsGenericTypeDefinition)
                {
                    Type targetType = GetMatchingType(sourceField.DeclaringType);
                    FieldInfo nonGenericFieldFromSourceType = (FieldInfo)sourceField.Module.ResolveMember(sourceField.MetadataToken,
                                             sourceField.DeclaringType.GetGenericArguments(),
                                             parentMethod == null || parentMethod is ConstructorInfo ? null : parentMethod.GetGenericArguments());

                    FieldInfo nonGenericFieldFromTargetType = GetMatchingField(parentMethod, nonGenericFieldFromSourceType);

                    FieldInfo fieldWithGenericTypesFilledInFromTargetType = TypeBuilder.GetField(targetType, nonGenericFieldFromTargetType);

                    return fieldWithGenericTypesFilledInFromTargetType;
                }
                else
                    // it's a field defined in the creating assembly
                    return fieldMapping[sourceField];
            }
            else
            {
                // it's a field in an assembly that's referenced
                Type targetType = GetMatchingType(sourceField.DeclaringType);
                if (targetType != sourceField.DeclaringType)
                {
                    var fieldOfTargetGenericTypeDefinition = (FieldInfo)sourceField.Module.ResolveMember(sourceField.MetadataToken,
                                                                                                      sourceField.DeclaringType.GetGenericArguments(),
                                                                                                      parentMethod == null || parentMethod is ConstructorInfo ? null : parentMethod.GetGenericArguments());

                    var field = TypeBuilder.GetField(targetType, fieldOfTargetGenericTypeDefinition);
                    return field;


                }
                else
                    return sourceField;
            }
        }

        /// <summary>
        /// 
        /// Here be dragon level reaching dangerous levels in here! Beware!
        /// </summary>
        /// <param name="parentMethod"></param>
        /// <param name="sourceMethod"></param>
        /// <returns></returns>
        private MethodInfo GetMatchingMethod(MethodBase parentMethod, MethodInfo sourceMethod)
        {
            if (sourceMethod.DeclaringType.IsGenericType && !sourceMethod.DeclaringType.IsGenericTypeDefinition)
            {
                Type targetType = GetMatchingType(sourceMethod.DeclaringType);
                MethodInfo nonGenericMethodFromSourceType = (MethodInfo)sourceMethod.Module.ResolveMember(sourceMethod.MetadataToken,
                                         sourceMethod.DeclaringType.GetGenericArguments(),
                                         parentMethod == null || parentMethod is ConstructorInfo ? null : parentMethod.GetGenericArguments());

                MethodInfo nonGenericMethodFromTargetType = GetMatchingMethod(parentMethod, nonGenericMethodFromSourceType);
                MethodInfo nonGenericMethodWithGenericTypesFilledInFromTargetType;
                if (targetType is TypeBuilder || targetType.GetType().Name == "TypeBuilderInstantiation")
                {
                    nonGenericMethodWithGenericTypesFilledInFromTargetType = TypeBuilder.GetMethod(targetType, nonGenericMethodFromTargetType);
                }
                else
                {
                    nonGenericMethodWithGenericTypesFilledInFromTargetType = targetType.GetMethods()
                                                                                       .Where(m => m.MetadataToken == nonGenericMethodFromTargetType.MetadataToken).FirstOrDefault();
                }


                if (sourceMethod.IsGenericMethod && !sourceMethod.IsGenericMethodDefinition)
                {
                    // map all the generic types from the source method to the target method
                    var targetGenericMethodDefinition = GetMatchingMethod(parentMethod, sourceMethod.GetGenericMethodDefinition());

                    var targetGenericTypes = sourceMethod.GetGenericArguments()
                                                         .Select(gt => GetMatchingType(gt))
                                                         .ToArray();
                    var targetGenericMethod = nonGenericMethodWithGenericTypesFilledInFromTargetType.MakeGenericMethod(targetGenericTypes);
                    return targetGenericMethod;
                }
                else
                    return nonGenericMethodWithGenericTypesFilledInFromTargetType;
            }
            else
            {
                if (sourceMethod.IsGenericMethod && !sourceMethod.IsGenericMethodDefinition)
                {
                    // map all the generic types from the source method to the target method
                    var targetGenericMethodDefinition = GetMatchingMethod(parentMethod, sourceMethod.GetGenericMethodDefinition());

                    var targetGenericTypes = sourceMethod.GetGenericArguments()
                                                         .Select(gt => GetMatchingType(gt))
                                                         .ToArray();
                    var targetGenericMethod = targetGenericMethodDefinition.MakeGenericMethod(targetGenericTypes);
                    return targetGenericMethod;
                }
                else
                {
                    if (assemblyMapping.ContainsKey(sourceMethod.DeclaringType.Assembly))
                        return methodMapping[sourceMethod];
                    else
                        return sourceMethod;

                }
            }


            // OLD CODE

            if (assemblyMapping.ContainsKey(sourceMethod.DeclaringType.Assembly))
            {
                // it's a method defined in a type in the creating assembly

                if (sourceMethod.DeclaringType.IsGenericType && !sourceMethod.DeclaringType.IsGenericTypeDefinition)
                {
                    Type targetType = GetMatchingType(sourceMethod.DeclaringType);
                    MethodInfo nonGenericMethodFromSourceType = (MethodInfo)sourceMethod.Module.ResolveMember(sourceMethod.MetadataToken,
                                             sourceMethod.DeclaringType.GetGenericArguments(),
                                             parentMethod == null || parentMethod is ConstructorInfo ? null : parentMethod.GetGenericArguments());

                    MethodInfo nonGenericMethodFromTargetType = GetMatchingMethod(parentMethod, nonGenericMethodFromSourceType);

                    MethodInfo nonGenericMethodWithGenericTypesFilledInFromTargetType = TypeBuilder.GetMethod(targetType, nonGenericMethodFromTargetType);

                    if (sourceMethod.IsGenericMethod && !sourceMethod.IsGenericMethodDefinition)
                    {
                        // map all the generic types from the source method to the target method
                        var targetGenericMethodDefinition = GetMatchingMethod(parentMethod, sourceMethod.GetGenericMethodDefinition());

                        var targetGenericTypes = sourceMethod.GetGenericArguments()
                                                             .Select(gt => GetMatchingType(gt))
                                                             .ToArray();
                        var targetGenericMethod = nonGenericMethodWithGenericTypesFilledInFromTargetType.MakeGenericMethod(targetGenericTypes);
                        return targetGenericMethod;
                    }
                    else
                        return nonGenericMethodWithGenericTypesFilledInFromTargetType;
                }
                else
                {
                    if (sourceMethod.IsGenericMethod && !sourceMethod.IsGenericMethodDefinition)
                    {
                        // map all the generic types from the source method to the target method
                        var targetGenericMethodDefinition = GetMatchingMethod(parentMethod, sourceMethod.GetGenericMethodDefinition());

                        var targetGenericTypes = sourceMethod.GetGenericArguments()
                                                             .Select(gt => GetMatchingType(gt))
                                                             .ToArray();
                        var targetGenericMethod = targetGenericMethodDefinition.MakeGenericMethod(targetGenericTypes);
                        return targetGenericMethod;
                    }
                    else
                        return methodMapping[sourceMethod];


                }
            }
            else
            {
                // it's a method in a type of an assembly that is referenced.
                // it's still possible the type has a generic parameter of the current type

                if (sourceMethod.DeclaringType.IsGenericType && !sourceMethod.DeclaringType.IsGenericTypeDefinition)
                {
                    Type targetType = GetMatchingType(sourceMethod.DeclaringType);
                    MethodInfo nonGenericMethodFromSourceType = (MethodInfo)sourceMethod.Module.ResolveMember(sourceMethod.MetadataToken,
                                             sourceMethod.DeclaringType.GetGenericArguments(),
                                             parentMethod == null || parentMethod is ConstructorInfo ? null : parentMethod.GetGenericArguments());


                    MethodInfo nonGenericMethodFromTargetType = GetMatchingMethod(parentMethod, nonGenericMethodFromSourceType);

                    MethodInfo nonGenericMethodWithGenericTypesFilledInFromTargetType;
                    if (targetType is TypeBuilder || targetType.GetType().Name == "TypeBuilderInstantiation")
                    {
                        nonGenericMethodWithGenericTypesFilledInFromTargetType = TypeBuilder.GetMethod(targetType, nonGenericMethodFromTargetType);
                    }
                    else
                    {
                        nonGenericMethodWithGenericTypesFilledInFromTargetType = targetType.GetMethods()
                                                                                           .Where(m => m.MetadataToken == nonGenericMethodFromTargetType.MetadataToken).FirstOrDefault();
                    }


                    if (sourceMethod.IsGenericMethod && !sourceMethod.IsGenericMethodDefinition)
                    {
                        // map all the generic types from the source method to the target method
                        var targetGenericMethodDefinition = GetMatchingMethod(parentMethod, sourceMethod.GetGenericMethodDefinition());

                        var targetGenericTypes = sourceMethod.GetGenericArguments()
                                                             .Select(gt => GetMatchingType(gt))
                                                             .ToArray();
                        var targetGenericMethod = nonGenericMethodWithGenericTypesFilledInFromTargetType.MakeGenericMethod(targetGenericTypes);
                        return targetGenericMethod;
                    }
                    else
                        return nonGenericMethodWithGenericTypesFilledInFromTargetType;
                }
                else
                {
                    if (sourceMethod.IsGenericMethod && !sourceMethod.IsGenericMethodDefinition)
                    {
                        // map all the generic types from the source method to the target method
                        var targetGenericMethodDefinition = GetMatchingMethod(parentMethod, sourceMethod.GetGenericMethodDefinition());

                        var targetGenericTypes = sourceMethod.GetGenericArguments()
                                                             .Select(gt => GetMatchingType(gt))
                                                             .ToArray();
                        var targetGenericMethod = targetGenericMethodDefinition.MakeGenericMethod(targetGenericTypes);
                        return targetGenericMethod;
                    }
                    else
                        return sourceMethod; // not defined in generic type, can give source method as reference
                    //else
                    //Type targetType = GetMatchingType(sourceMethod.DeclaringType);
                    //if (targetType != sourceMethod.DeclaringType)
                    //{
                    //    // type contains generic stuff from this assembly, need to return 
                    //    // the corresponding method of that type

                    //    var methodOfGenericTypeDefinition = (MethodInfo)sourceMethod.Module.ResolveMember(sourceMethod.MetadataToken,
                    //                                                                                      sourceMethod.DeclaringType.GetGenericArguments(),
                    //                                                                                      parentMethod == null || parentMethod is ConstructorInfo ? null : parentMethod.GetGenericArguments());

                    //    var method = TypeBuilder.GetMethod(targetType, methodOfGenericTypeDefinition);
                    //    return method;

                    //}
                    //else
                    //return sourceMethod; // not defined in generic type, can give source method as reference
                }
            }
        }

        private ConstructorInfo GetMatchingConstructor(MethodBase parentMethod, ConstructorInfo sourceConstructor)
        {
            if (assemblyMapping.ContainsKey(sourceConstructor.DeclaringType.Assembly))
            {
                // it's a ctor defined in the creating assembly

                if (sourceConstructor.DeclaringType.IsGenericType && !sourceConstructor.DeclaringType.IsGenericTypeDefinition)
                {
                    Type targetType = GetMatchingType(sourceConstructor.DeclaringType);
                    ConstructorInfo nonGenericConstructorFromSourceType = (ConstructorInfo)sourceConstructor.Module.ResolveMember(sourceConstructor.MetadataToken,
                                             sourceConstructor.DeclaringType.GetGenericArguments(),
                                             parentMethod == null || parentMethod is ConstructorInfo ? null : parentMethod.GetGenericArguments());

                    ConstructorInfo nonGenericConstructorFromTargetType = GetMatchingConstructor(parentMethod, nonGenericConstructorFromSourceType);

                    ConstructorInfo constructorWithGenericTypesFilledInFromTargetType = TypeBuilder.GetConstructor(targetType, nonGenericConstructorFromTargetType);

                    return constructorWithGenericTypesFilledInFromTargetType;
                }
                else
                    return constructorMapping[sourceConstructor];
            }
            else
            {
                // it's a ctor in an assembly that's referenced
                Type targetType = GetMatchingType(sourceConstructor.DeclaringType);
                if (targetType != sourceConstructor.DeclaringType)
                {
                    // type is generic
                    var constructorOfTargetGenericTypeDefinition = (ConstructorInfo)sourceConstructor.Module.ResolveMember(sourceConstructor.MetadataToken,
                                                                                                      sourceConstructor.DeclaringType.GetGenericArguments(),
                                                                                                      parentMethod == null || parentMethod is ConstructorInfo ? null : parentMethod.GetGenericArguments());

                    var field = TypeBuilder.GetConstructor(targetType, constructorOfTargetGenericTypeDefinition);
                    return field;


                }
                else
                    return sourceConstructor;
            }
        }

        private Type GetMatchingType(Type sourceType)
        {
            if (sourceType.IsGenericTypeDefinition)
            {
                // typename<>
                return GetType(sourceType);
            }
            else if (sourceType.IsGenericType)
            {
                var targetGenericTypeDefinition = GetMatchingType(sourceType.GetGenericTypeDefinition());

                var targetGenericTypes = sourceType.GetGenericArguments()
                                                   .Select(gt => GetMatchingType(gt))
                                                   .ToArray();
                var targetGenericType = targetGenericTypeDefinition.MakeGenericType(targetGenericTypes);

                return targetGenericType;
            }
            else if (sourceType.IsGenericParameter)
            {
                // get matching generic parameter
                return genericTypeParameterMapping[sourceType];
            }
            else if (sourceType.IsArray)
            {
                var targetBaseType = GetMatchingType(sourceType.GetElementType());
                return targetBaseType.MakeArrayType();
            }
            else if (sourceType.IsByRef)
            {
                var targetBaseType = GetMatchingType(sourceType.GetElementType());
                return targetBaseType.MakeByRefType();
            }
            else
                return GetType(sourceType);
        }


        // TODO , make sure GetType returns matching generic types too!
        private Type GetType(Type t)
        {
            Type tb;
            if (typeMapping.TryGetValue(t, out tb))
                return tb;
            else
            {
                if (t.Assembly.FullName.ToLower().Contains("runtimedebug"))
                    System.Diagnostics.Debugger.Break();

                if (assemblyMapping.ContainsKey(t.Assembly))
                    throw new Exception("Type needs to be mapped, but isn't available in the mapping");
                else
                    return t;
            }
        }
    }
}
