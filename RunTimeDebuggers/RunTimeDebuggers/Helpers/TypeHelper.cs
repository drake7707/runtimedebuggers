using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Runtime.CompilerServices;
using RunTimeDebuggers.LocalsDebugger;

using System.Reflection.Emit;
using RunTimeDebuggers.AssemblyExplorer;

namespace RunTimeDebuggers.Helpers
{
    public static class TypeHelper
    {

        //public static Type GetTypeFromName(string typename, List<Type> genericTypes)
        //{
        //    if (typename == "string")
        //        return typeof(string);
        //    else if (typename == "int")
        //        return typeof(int);
        //    else if (typename == "long")
        //        return typeof(long);
        //    else if (typename == "short")
        //        return typeof(short);
        //    else if (typename == "bool")
        //        return typeof(bool);
        //    else if (typename == "float")
        //        return typeof(float);
        //    else if (typename == "double")
        //        return typeof(double);
        //    else if (typename == "byte")
        //        return typeof(byte);
        //    else if (typename == "object")
        //        return typeof(object);
        //    else if (typename == "ushort")
        //        return typeof(ushort);
        //    else if (typename == "uint")
        //        return typeof(uint);
        //    else if (typename == "ulong")
        //        return typeof(ulong);
        //    else if (typename == "sbyte")
        //        return typeof(sbyte);


        //    Type t = Type.GetType(typename);

        //    if (t == null)
        //    {
        //        foreach (Assembly a in AppDomain.CurrentDomain.GetAssemblies())
        //        {
        //            foreach (Type typ in a.GetTypesSafe())
        //            {
        //                if (typ.Name.Contains('`'))
        //                {
        //                    if (typ.Name.Substring(0, typ.Name.IndexOf('`')) == typename)
        //                        return typ;
        //                }
        //                else
        //                {
        //                    if (typ.Name == typename || typ.FullName == typename)
        //                    {
        //                        var genericArguments = typ.GetGenericArguments();
        //                        if (typ.IsGenericType && genericArguments != null)
        //                        {
        //                            if (genericTypes.Count == genericArguments.Length)
        //                            {
        //                                bool match = true;
        //                                for (int i = 0; i < genericArguments.Length; i++)
        //                                {
        //                                    if (!genericArguments[i].IsAssignableFrom(genericTypes[i]))
        //                                    {
        //                                        match = false;
        //                                        break;
        //                                    }
        //                                }
        //                                if (match)
        //                                    return typ;
        //                            }
        //                        }
        //                        else
        //                            return typ;
        //                    }
        //                }
        //            }
        //        }
        //    }
        //    else
        //        return t;

        //    return null;
        //}

        public static bool IsField(this Type t, string name)
        {
            var field = GetFieldsOfType(t, true, true).Where(f => f.Name == name || AliasManager.Instance.GetAlias(f) == name).FirstOrDefault();
            return field != null;
        }
        public static bool IsProperty(this Type t, string name)
        {
            var prop = GetPropertiesOfType(t, true, true)
                          .Where(f => f.Name == name || AliasManager.Instance.GetAlias(f) == name).FirstOrDefault();
            return prop != null;
        }
        public static bool IsMethod(this Type t, TypeStore typeStore, string name)
        {
            var method = GetMethodsOfType(t, true, true).Concat(typeStore.GetExtensionMethodsOf(t))
                            .Where(f => (f.Name == name || AliasManager.Instance.GetAlias(f) == name) && !f.IsSpecialName)
                            .FirstOrDefault();
            return method != null;
        }

        public static IEnumerable<Type> GetTypesSafe(this Assembly assembly)
        {
            try
            {
                return assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException ex)
            {
                return ex.Types.Where(x => x != null);
            }
        }

        public static IEnumerable<Type> GetTypesSafe(this Module module)
        {
            try
            {
                return module.GetTypes();
            }
            catch (ReflectionTypeLoadException ex)
            {
                return ex.Types.Where(x => x != null);
            }
        }


        public static List<PropertyInfo> GetPropertiesOfType(this Type t, bool includeBaseProperties, bool inclStatic)
        {

            if (includeBaseProperties)
            {
                var flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
                if (inclStatic)
                    flags |= BindingFlags.Static;

                if (includeBaseProperties)
                    flags |= BindingFlags.FlattenHierarchy;

                var props = t.GetProperties(flags);
                return props.ToList();
            }
            else
                return GetPropertiesOfTypeWithoutBase(t, inclStatic).ToList();
        }
        private static IEnumerable<PropertyInfo> GetPropertiesOfTypeWithoutBase(Type t, bool inclStatic)
        {
            var flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
            if (inclStatic)
                flags |= BindingFlags.Static;

            return t.GetProperties(flags).Where(p => p.DeclaringType == t);

            var props = t.GetProperties(flags)
                         .GroupBy(p => p.MetadataToken)
                         .ToDictionary(g => g.Key, g => g.First());


            if (t.BaseType != null)
            {
                var baseProps = t.BaseType.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                foreach (var baseProp in baseProps)
                {

                    bool removeProp = true;

                    PropertyInfo initialProp;
                    if (props.TryGetValue(baseProp.MetadataToken, out initialProp))
                    {
                        var getMethod = initialProp.GetGetMethod(true);
                        if (getMethod != null && getMethod.DeclaringType == t)
                        {
                            removeProp = false;

                            //if ((getMethod.Attributes & MethodAttributes.Virtual) != 0 &&
                            //   (getMethod.Attributes & MethodAttributes.NewSlot) == 0)
                            //{
                            //    // the property's 'get' method is an override
                            //    // don't remove it
                            //    removeProp = false;

                            //}
                        }
                    }

                    if (removeProp)
                        props.Remove(baseProp.MetadataToken);
                }
            }
            return props.Values.ToList();
        }

        public static List<Type> GetInterfacesOfType(this Type t, bool includeBaseProperties)
        {

            if (includeBaseProperties)
            {
                return t.GetInterfaces().ToList();
            }
            else
                return GetInterfacesWithoutBase(t).ToList();
        }
        private static IEnumerable<Type> GetInterfacesWithoutBase(Type t)
        {

            var interfaces = t.GetInterfaces()
                         .GroupBy(p => p.GUID)
                         .ToDictionary(g => g.Key, g => g.First());


            if (t.BaseType != null)
            {
                var baseInterfaces = t.BaseType.GetInterfaces().ToList();
                baseInterfaces.AddRange(t.GetInterfaces().SelectMany(i => i.GetInterfaces()));

                foreach (var baseInterface in baseInterfaces)
                {
                    Type initialType;
                    if (interfaces.TryGetValue(baseInterface.GUID, out initialType))
                    {
                        interfaces.Remove(baseInterface.GUID);
                    }
                }
            }
            return interfaces.Values.ToList();
        }


        public static List<MethodInfo> GetMethodsOfType(this Type t, bool includeBase, bool inclStatic)
        {
            return GetMethodsOfType(t, includeBase, inclStatic, false);
        }

        public static List<MethodInfo> GetMethodsOfType(this Type t, bool includeBase, bool inclStatic, bool inclSpecialName)
        {
            if (includeBase)
            {
                List<MethodInfo> methods = new List<MethodInfo>();

                var flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
                if (includeBase)
                    flags |= BindingFlags.FlattenHierarchy;

                if (inclStatic)
                    flags |= BindingFlags.Static;

                // all instance methods

                if (inclSpecialName)
                    methods.AddRange(t.GetMethods(flags));
                else
                    methods.AddRange(t.GetMethods(flags).Where(m => !m.IsSpecialName || m.Name.ToLower().StartsWith("op_")));


                // all static methods of base types
                Type cur = t.BaseType;
                while (cur != null)
                {
                    var baseFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static;

                    if (inclSpecialName)
                        methods.AddRange(cur.GetMethods(baseFlags));
                    else
                        methods.AddRange(cur.GetMethods(baseFlags).Where(m => !m.IsSpecialName || m.Name.ToLower().StartsWith("op_")));


                    if (!includeBase)
                        cur = null;
                    else
                        cur = cur.BaseType;
                }

                return methods;
            }
            else
                return GetMethodsOfTypeWithoutBase(t, inclStatic, inclSpecialName).ToList();
        }

        private static IEnumerable<MethodInfo> GetMethodsOfTypeWithoutBase(Type t, bool inclStatic, bool inclSpecialName)
        {
            var flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
            if (inclStatic)
                flags |= BindingFlags.Static;

            if (inclSpecialName)
            {
                return t.GetMethods(flags).Where(m => m.DeclaringType == t);
            }
            else
            {
                return t.GetMethods(flags).Where(m => (!m.IsSpecialName || m.Name.ToLower().StartsWith("op_")) &&
                                                  m.DeclaringType == t);

            }


            //var methods = t.GetMethods(flags)
            //             .Where(m => inclSpecialName || !m.IsSpecialName)
            //             .GroupBy(m => m.MetadataToken)
            //             .ToDictionary(g => g.Key, g => g.First());


            //if (t.BaseType != null)
            //{
            //    var baseMethods = t.BaseType.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).Where(m => inclSpecialName || !m.IsSpecialName);
            //    foreach (var baseMethod in baseMethods)
            //    {
            //        MethodInfo initialMethod;
            //        if (methods.TryGetValue(baseMethod.MetadataToken, out initialMethod))
            //        {
            //            methods.Remove(baseMethod.MetadataToken);
            //        }
            //    }
            //}
            //return methods.Values.ToList();
        }


        public static List<EventInfo> GetEventsOfType(this Type t, bool includeBase, bool inclStatic)
        {
            if (includeBase)
            {
                List<EventInfo> events = new List<EventInfo>();

                var flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
                if (includeBase)
                    flags |= BindingFlags.FlattenHierarchy;
                if (inclStatic)
                    flags |= BindingFlags.Static;

                events.AddRange(t.GetEvents(flags));

                return events;
            }
            else
                return GetEventsOfTypeWithoutBase(t, inclStatic).ToList();
        }

        private static IEnumerable<EventInfo> GetEventsOfTypeWithoutBase(Type t, bool inclStatic)
        {
            var flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
            if (inclStatic)
                flags |= BindingFlags.Static;

            var methods = t.GetEvents(flags)
                         .GroupBy(m => m.MetadataToken)
                         .ToDictionary(g => g.Key, g => g.First());


            if (t.BaseType != null)
            {
                var baseEvents = t.BaseType.GetEvents(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                foreach (var baseEvent in baseEvents)
                {
                    EventInfo initialEvent;
                    if (methods.TryGetValue(baseEvent.MetadataToken, out initialEvent))
                    {
                        methods.Remove(baseEvent.MetadataToken);
                    }
                }
            }
            return methods.Values.ToList();
        }

        public static IEnumerable<MethodInfo> GetSubscribedMethods(this EventInfo ev, object instance)
        {
            var flags = BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.GetField;
            if (instance == null)
                flags |= BindingFlags.Static;

            FieldInfo f = ev.DeclaringType.GetField(ev.Name, flags);
            if (f != null)
            {
                var d = (System.Delegate)f.GetValue(instance);
                return d.GetInvocationList().Select(s => s.Method);
            }
            else
            {
                // it's a custom implementation of event handler, handle the most used cases
                if (instance is System.ComponentModel.Component)
                {
                    var c = (System.ComponentModel.Component)instance;

                    var evList = (System.ComponentModel.EventHandlerList)c.GetType().GetProperty("Events", BindingFlags.Instance | BindingFlags.NonPublic)
                                                                                    .GetValue(instance, null);

                    // magic scanner here that will probably not work all the time
                    // in System.Windows.Forms the Events are stored in an EventHandlerList
                    // the keys of those events are static fields defined, but no standard naming is used
                    // in Control it's all Event<EventName>, in Form it's EVENT_<EVENTNAME>

                    // best way to determine actual field is to inspect the IL of the add method, find the static private field info that
                    // is used

                    FieldInfo eventFieldKey = null;

                    MethodInfo addMethod = ev.GetAddMethod();
                    while (addMethod != null)
                    {
                        var instructions = addMethod.GetILInstructions();

                        eventFieldKey = instructions.Where(i => i.Code == OpCodes.Ldsfld && ((FieldInfo)i.Operand).Name.ToLower().StartsWith("event"))
                                              .Select(i => (FieldInfo)i.Operand)
                                              .FirstOrDefault();

                        if (eventFieldKey != null)
                        {
                            Delegate d = evList[eventFieldKey.GetValue(instance)];
                            if (d != null)
                            {
                                return d.GetInvocationList().Select(s => s.Method);
                            }
                            else
                                return Enumerable.Empty<MethodInfo>();
                        }
                        else
                        {
                            // it's possible that it's defined in base.addMethod
                            // go look for the base call
                            var baseMethod = instructions.Where(i => i.Code == OpCodes.Call && ((MethodInfo)i.Operand).Name == addMethod.Name)
                                                                                           .Select(i => (MethodInfo)i.Operand)
                                                                                           .FirstOrDefault();

                            addMethod = baseMethod;
                        }
                    }
                }

                else if (instance is System.Windows.UIElement)
                {
                    var c = (System.Windows.UIElement)instance;

                    var evStore = c.GetType().GetProperty("EventHandlersStore", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.FlattenHierarchy)
                                             .GetValue(c, null);

                    if (evStore != null)
                    {
                        MethodInfo getRoutedEventHandlers = evStore.GetType().GetMethod("GetRoutedEventHandlers", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

                        MethodInfo addMethod = ev.GetAddMethod();
                        while (addMethod != null)
                        {
                            var instructions = addMethod.GetILInstructions();
                            FieldInfo eventFieldKey = null;

                            // ClickEvent, SubmenuClosedEvent, etc..
                            eventFieldKey = instructions.Where(i => i.Code == OpCodes.Ldsfld && ((FieldInfo)i.Operand).Name.ToLower().EndsWith("event"))
                                                  .Select(i => (FieldInfo)i.Operand)
                                                  .FirstOrDefault();
                            
                            if (eventFieldKey != null)
                            {
                                System.Windows.RoutedEvent re = (System.Windows.RoutedEvent)eventFieldKey.GetValue(null);

                                var eventhandlers = (System.Windows.RoutedEventHandlerInfo[])getRoutedEventHandlers.Invoke(evStore, new object[] { re });

                                List<MethodInfo> methods = new List<MethodInfo>();
                                foreach (var evhandler in eventhandlers)
                                {
                                    Delegate d = evhandler.Handler;
                                    if (d != null)
                                        methods.Add(d.Method);
                                }
                                return methods;
                            }
                            else
                            {
                                // it's possible that it's defined in base.addMethod
                                // go look for the base call
                                var baseMethod = instructions.Where(i => i.Code == OpCodes.Call && ((MethodInfo)i.Operand).Name == addMethod.Name)
                                                                                               .Select(i => (MethodInfo)i.Operand)
                                                                                               .FirstOrDefault();
                                addMethod = baseMethod;
                            }
                        }
                    }
                }
            }

            throw new Exception("Unable to evaluate event, custom event wiring is used");
        }


        public static List<ConstructorInfo> GetConstructorsOfType(this Type t, bool isStatic)
        {
            List<ConstructorInfo> constructors = new List<ConstructorInfo>();

            BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.FlattenHierarchy;
            if (isStatic)
                flags |= BindingFlags.Static;

            constructors.AddRange(t.GetConstructors(flags));
            return constructors;
        }

        public static List<FieldInfo> GetFieldsOfType(this Type t, bool includeBaseTypeFields, bool includeStatic)
        {
            List<FieldInfo> fields = new List<FieldInfo>();


            var flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
            if (includeStatic)
                flags |= BindingFlags.Static;

            Type cur = t;
            while (cur != null)
            {

                fields.AddRange(cur.GetFields(flags));

                if (!includeBaseTypeFields)
                    cur = null;
                else
                    cur = cur.BaseType;
            }
            return fields;
        }

        public static List<ILInstruction> GetILInstructions(this MethodBase m)
        {
            Disassembler dis = new Disassembler(m);
            dis.BuildInstructions();

            if (dis.Instructions == null)
                return new List<ILInstruction>();
            else
                return dis.Instructions;
        }

        public static bool IsUnsafe(this MethodBase m)
        {
            var returnType = m.GetReturnType();
            if (returnType != null && returnType.IsPointer)
                return true;

            if (m.GetParameters().Any(p => p.ParameterType.IsPointer))
                return true;

            var body = m.GetMethodBody();
            if (body == null)
                return false;
            else
            {
                bool anyPinned = body.LocalVariables.Any(p => p.IsPinned);
                if (anyPinned)
                    return true;
                else
                {
                    var usedMembers = m.GetILInstructions().Where(il => il.Operand is MemberInfo).Select(il => (MemberInfo)il.Operand);
                    foreach (var member in usedMembers)
                    {
                        returnType = member.GetReturnType();
                        if (returnType != null && returnType.IsPointer)
                            return true;

                        if (member is MethodBase)
                        {
                            bool anyParameterIsPointer = ((MethodBase)member).GetParameters().Any(p => p.ParameterType.IsPointer);
                            if (anyParameterIsPointer)
                                return true;
                        }
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Casts the given object to the given type
        /// </summary>
        /// <param name="t">The type to cast the object to</param>
        /// <param name="o">The object to be casted</param>
        /// <returns>The casted object</returns>
        public static object CastTo(this object o, Type t)
        {
            try
            {
                return typeof(TypeHelper).GetMethod("CastGeneric", BindingFlags.Static | BindingFlags.NonPublic)
                                         .MakeGenericMethod(t).Invoke(null, new object[] { o });
            }
            catch (Exception ex)
            {
                // throw actual exception
                throw ex.InnerException;
            }
        }

        /// <summary>
        /// Casts the given object to the given type
        /// </summary>
        /// <typeparam name="T">The type to cast the object to</typeparam>
        /// <param name="o">The object to be casted</param>
        /// <returns>The casted object</returns>
        private static T CastGeneric<T>(object o)
        {
            return (T)o;
        }

        public static IEnumerable<MemberInfo> GetAllMembers(this Type t)
        {
            List<MemberInfo> members = new List<MemberInfo>();
            members.AddRange(GetFieldsOfType(t, true, true).ToArray());
            members.AddRange(GetPropertiesOfType(t, true, true).ToArray());
            members.AddRange(GetMethodsOfType(t, true, true).ToArray());

            return members;
        }

        public static List<Type> GetNestedTypesOfType(this Type t)
        {
            List<Type> nestedTypes = new List<Type>();
            nestedTypes.AddRange(t.GetNestedTypes(BindingFlags.Public | BindingFlags.NonPublic));
            return nestedTypes;
        }

        public static IEnumerable<KeyValuePair<Type, MethodInfo>> GetExtensionMethodsDefinedInType(this Type t)
        {
            if (!t.IsSealed || t.IsGenericType || t.IsNested)
                return Enumerable.Empty<KeyValuePair<Type, MethodInfo>>();

            var methods = t.GetMethods(BindingFlags.Public | BindingFlags.Static)
                           .Where(m => m.IsDefined(typeof(ExtensionAttribute), false));

            List<KeyValuePair<Type, MethodInfo>> pairs = new List<KeyValuePair<Type, MethodInfo>>();
            foreach (var m in methods)
            {
                var parameters = m.GetParameters();
                if (parameters.Length > 0)
                {
                    if (parameters[0].ParameterType.IsGenericParameter)
                    {
                        if (m.ContainsGenericParameters)
                        {
                            var genericParameters = m.GetGenericArguments();
                            Type genericParam = genericParameters[parameters[0].ParameterType.GenericParameterPosition];
                            foreach (var constraint in genericParam.GetGenericParameterConstraints())
                                pairs.Add(new KeyValuePair<Type, MethodInfo>(parameters[0].ParameterType, m));
                        }
                    }
                    else
                        pairs.Add(new KeyValuePair<Type, MethodInfo>(parameters[0].ParameterType, m));
                }
            }

            return pairs;
        }

        public static bool IsStatic(this MemberInfo member)
        {
            if (member is FieldInfo)
                return ((FieldInfo)member).IsStatic;
            else if (member is PropertyInfo)
            {
                var prop = ((PropertyInfo)member);
                return prop.CanRead && prop.GetGetMethod(true).IsStatic;
            }
            else if (member is MethodInfo)
                return ((MethodInfo)member).IsStatic;
            else if (member is EventInfo)
            {
                var addMethod = ((EventInfo)member).GetAddMethod();
                if (addMethod != null)
                    return addMethod.IsStatic;
            }
            return false;
        }

        public static Type GetReturnType(this MemberInfo member)
        {
            if (member is FieldInfo)
                return ((FieldInfo)member).FieldType;
            else if (member is PropertyInfo)
                return ((PropertyInfo)member).PropertyType;
            else if (member is MethodInfo)
                return ((MethodInfo)member).ReturnType;
            else if (member is EventInfo)
                return ((EventInfo)member).EventHandlerType;

            return null;
        }

        public static MethodInfo GetTopLevelBaseDefinition(this MethodInfo mb)
        {
            MethodInfo topLevel = mb;
            while (topLevel.GetBaseDefinition() != topLevel)
                topLevel = topLevel.GetBaseDefinition();

            return topLevel;
        }
        public static Type GetMostPrecision(this Type t, Type t2)
        {
            var leftTypeCode = Type.GetTypeCode(t);
            var rightTypeCode = Type.GetTypeCode(t2);

            if (leftTypeCode == rightTypeCode)
                return t;

            if (leftTypeCode > rightTypeCode)
                return t;
            else
                return t2;
        }

        /// <summary>
        /// Gets the default value of any type
        /// </summary>
        /// <param name="t">The type to get a default value of</param>
        /// <returns></returns>
        public static object GetDefault(this Type t)
        {
            if (t.IsByRef)
                return null;
            else
                return typeof(TypeHelper).GetMethod("GetDefaultGeneric", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Static)
                                     .MakeGenericMethod(t).Invoke(null, null);
        }

        /// <summary>
        /// Returns a default value of the given type t
        /// </summary>
        /// <typeparam name="T">The type to get a default value of</typeparam>
        /// <returns>The default value of given type</returns>
        private static T GetDefaultGeneric<T>()
        {
            return default(T);
        }

        public static bool IsEqual(this MemberInfo m1, MemberInfo m2)
        {
            return m1.Module == m2.Module && m1.MetadataToken == m2.MetadataToken;
        }

        public static string GetName(this MemberInfo m, bool prefixDeclaredType)
        {
            if (prefixDeclaredType && m.DeclaringType != null)
                return m.DeclaringType.ToSignatureString() + "::" + AliasManager.Instance.GetFullNameWithAlias(m, m.Name);
            else
                return AliasManager.Instance.GetFullNameWithAlias(m, m.Name);
        }

        public static string ToSignatureString(this FieldInfo f)
        {
            return f.FieldType.ToSignatureString() + " " + AliasManager.Instance.GetFullNameWithAlias(f, f.Name);
        }

        public static string ToSignatureString(this PropertyInfo p)
        {
            return p.PropertyType.ToSignatureString() + " " + AliasManager.Instance.GetFullNameWithAlias(p, p.Name);
        }

        public static string ToSignatureString(this MemberInfo m)
        {
            if (m is FieldInfo)
                return ToSignatureString((FieldInfo)m);
            else if (m is PropertyInfo)
                return ToSignatureString((PropertyInfo)m);
            else if (m is MethodInfo)
                return ToSignatureString((MethodInfo)m);
            else if (m is ConstructorInfo)
                return ToSignatureString((ConstructorInfo)m);
            else
                return "";
        }


        public static string ToSignatureString(this Type t)
        {
            return ToSignatureString(t, true);
        }

        public static string ToSignatureString(this Type t, bool appendWithAlias)
        {
            if (t == null)
                return "";

            string name = GetSignatureStringOfSpecialType(t);
            if (name == null)
            {
                string typeName;
                if (t.IsGenericType)
                    typeName = TrimGenericCounterFromTypeName(t.Name) + "<" + string.Join(",", t.GetGenericArguments().Select(typ => typ.ToSignatureString(appendWithAlias)).ToArray()) + ">";
                else
                    typeName = t.Name;

                if (appendWithAlias)
                    return AliasManager.Instance.GetFullNameWithAlias(t, typeName);
                else
                    return typeName;
            }
            else
                return name;
        }

        public static string TrimGenericCounterFromTypeName(string typeName)
        {
            string name;
            if (typeName.Contains('`'))
                name = typeName.Substring(0, typeName.IndexOf('`'));
            else
                name = typeName;

            return name;
        }

        private static string GetSignatureStringOfSpecialType(Type t)
        {
            if (t == typeof(string))
                return "string";
            if (t == typeof(int))
                return "int";
            if (t == typeof(long))
                return "long";
            if (t == typeof(short))
                return "short";
            if (t == typeof(float))
                return "float";
            if (t == typeof(double))
                return "double";
            if (t == typeof(bool))
                return "bool";
            if (t == typeof(object))
                return "object";
            if (t == typeof(ushort))
                return "ushort";
            if (t == typeof(uint))
                return "uint";
            if (t == typeof(ulong))
                return "ulong";
            if (t == typeof(sbyte))
                return "sbyte";
            if (t == typeof(byte))
                return "byte";
            if (t == typeof(void))
                return "void";

            return null;
        }

        public static string ToExceptionString(this Exception ex)
        {
            StringBuilder str = new StringBuilder();

            Exception e = ex;
            while (e != null)
            {
                str.AppendLine(e.GetType().FullName + " - " + e.Message);
                e = e.InnerException;
            }
            return str.ToString();
        }

        public static string ToSignatureString(this MethodBase c)
        {
            string sig = "";

            sig += AliasManager.Instance.GetFullNameWithAlias(c, c.Name);

            if (c.DeclaringType != null && !c.DeclaringType.IsAbstract)
            {
                if (c.ContainsGenericParameters)
                {
                    try
                    {
                        var genericParams = c.GetGenericArguments();
                        sig += "<" + string.Join(",", genericParams.Select(t => t.ToSignatureString()).ToArray()) + ">";
                    }
                    catch (Exception)
                    {

                    }
                }
            }

            sig += "(" + string.Join(",", c.GetParameters().Select(p =>
            {
                string par = "";
                if (p.IsOut)
                    par += "out ";

                if (p.ParameterType.IsByRef)
                    par += "ref ";

                par += p.ParameterType.ToSignatureString() + " " + p.Name;

                if (p.IsOptional)
                    return "[" + par + "]";

                return par;
            }).ToArray()) + ")";

            return sig;
        }


        public static string ToSignatureString(this MethodInfo m)
        {
            string sig = "";

            sig += m.ReturnType.ToSignatureString() + " ";

            sig += ToSignatureString((MethodBase)m);

            return sig;
        }


        public static IList<CustomAttributeData> GetCustomAttributesDataInclSecurity(this Assembly t)
        {
            var customAttributeData = CustomAttributeData.GetCustomAttributes(t).Where(ca => !ca.Constructor.DeclaringType.IsSecurityAttribute()).ToList();
            var securityAttributes = t.GetCustomAttributes(false).Where(a => a.GetType().IsSecurityAttribute()).Cast<System.Security.Permissions.SecurityAttribute>().ToList();

            foreach (var secAttr in securityAttributes)
                customAttributeData.Add(CreateCustomAttributeDataFromSecurityAttribute(secAttr));

            return customAttributeData;
        }

        public static IList<CustomAttributeData> GetCustomAttributesDataInclSecurity(this Module t)
        {
            var customAttributeData = CustomAttributeData.GetCustomAttributes(t).Where(ca => !ca.Constructor.DeclaringType.IsSecurityAttribute()).ToList();
            var securityAttributes = t.GetCustomAttributes(false).Where(a => a.GetType().IsSecurityAttribute()).Cast<System.Security.Permissions.SecurityAttribute>().ToList();

            foreach (var secAttr in securityAttributes)
                customAttributeData.Add(CreateCustomAttributeDataFromSecurityAttribute(secAttr));

            return customAttributeData;
        }

        public static IList<CustomAttributeData> GetCustomAttributesDataInclSecurity(this MemberInfo t)
        {
            var customAttributeData = CustomAttributeData.GetCustomAttributes(t).Where(ca => !ca.Constructor.DeclaringType.IsSecurityAttribute()).ToList();
            var securityAttributes = t.GetCustomAttributes(false).Where(a => a.GetType().IsSecurityAttribute()).Cast<System.Security.Permissions.SecurityAttribute>().ToList();

            foreach (var secAttr in securityAttributes)
                customAttributeData.Add(CreateCustomAttributeDataFromSecurityAttribute(secAttr));

            return customAttributeData;
        }

        public static IList<CustomAttributeData> GetCustomAttributesDataInclSecurity(this ParameterInfo t)
        {
            var customAttributeData = CustomAttributeData.GetCustomAttributes(t).Where(ca => !ca.Constructor.DeclaringType.IsSecurityAttribute()).ToList();
            var securityAttributes = t.GetCustomAttributes(false).Where(a => a.GetType().IsSecurityAttribute()).Cast<System.Security.Permissions.SecurityAttribute>().ToList();

            foreach (var secAttr in securityAttributes)
                customAttributeData.Add(CreateCustomAttributeDataFromSecurityAttribute(secAttr));

            return customAttributeData;
        }




        private static CustomAttributeData CreateCustomAttributeDataFromSecurityAttribute(System.Security.Permissions.SecurityAttribute secAttr)
        {
            // find the constructor with the least amount of parameters, the rest will be set via Named arguments
            var ctors = secAttr.GetType().GetConstructorsOfType(false);
            ConstructorInfo ctor = ctors.First();
            foreach (var ct in ctors.Skip(1))
            {
                if (ct.GetParameters().Length < ctor.GetParameters().Length)
                    ctor = ct;
            }

            System.Security.Permissions.SecurityAttribute defaultSecAttr = null;


            List<CustomAttributeTypedArgument> constructorArguments = new List<CustomAttributeTypedArgument>();
            if (ctor.GetParameters().Length == 1 && ctor.GetParameters().First().ParameterType == typeof(System.Security.Permissions.SecurityAction))
            {
                // most security permissions have a constructor with only SecurityAction as parameter                
                constructorArguments.Add(CreateTypedArgument(secAttr.Action));

                defaultSecAttr = (System.Security.Permissions.SecurityAttribute)ctor.Invoke(new object[] { secAttr.Action });
            }
            else
            {
                // TODO i guess?
                // analyze the IL of the ctor to determine which parameter of the ctor goes to which field
                //Disassembler dis = new Disassembler(ctor);

            }

            List<CustomAttributeNamedArgument> namedArguments = new List<CustomAttributeNamedArgument>();
            foreach (var f in secAttr.GetType().GetFieldsOfType(true, false).Where(fld => fld.IsPublic))
            {
                object value = f.GetValue(secAttr);
                if (defaultSecAttr != null)
                {
                    // check if it differs from a default instance
                    object defaultValue = f.GetValue(defaultSecAttr);
                    if (object.Equals(value, defaultValue))
                    {
                        // it's the same, named argument is not necessary
                    }
                    else
                        namedArguments.Add(CreateNamedArgument(f, value));
                }
            }

            foreach (var prop in secAttr.GetType().GetPropertiesOfType(true, false).Where(prop => prop.CanRead && prop.CanWrite && prop.GetGetMethod().IsPublic))
            {
                object value = prop.GetValue(secAttr, null);
                if (defaultSecAttr != null)
                {
                    // check if it differs from a default instance
                    object defaultValue = prop.GetValue(defaultSecAttr, null);
                    if (object.Equals(value, defaultValue))
                    {
                        // it's the same, named argument is not necessary
                    }
                    else
                        namedArguments.Add(CreateNamedArgument(prop, value));
                }
            }

            var cad = (CustomAttributeData)System.Runtime.Serialization.FormatterServices.GetSafeUninitializedObject(typeof(CustomAttributeData));
            typeof(CustomAttributeData).GetField("m_ctor", BindingFlags.NonPublic | BindingFlags.Instance)
                                       .SetValue(cad, ctor);

            typeof(CustomAttributeData).GetField("m_namedArgs", BindingFlags.NonPublic | BindingFlags.Instance)
                                       .SetValue(cad, (IList<CustomAttributeNamedArgument>)namedArguments);

            typeof(CustomAttributeData).GetField("m_typedCtorArgs", BindingFlags.NonPublic | BindingFlags.Instance)
                                   .SetValue(cad, (IList<CustomAttributeTypedArgument>)constructorArguments);

            // eh that'll do

            //private ConstructorInfo m_ctor;
            //private Module m_scope;
            //private MemberInfo[] m_members;
            //private CustomAttributeCtorParameter[] m_ctorParams;
            //private CustomAttributeNamedParameter[] m_namedParams;
            //private IList<CustomAttributeTypedArgument> m_typedCtorArgs;
            //private IList<CustomAttributeNamedArgument> m_namedArgs;
            return cad;
        }

        private static CustomAttributeTypedArgument CreateTypedArgument(object value)
        {
            var ctor = typeof(CustomAttributeTypedArgument).GetConstructorsOfType(false)
                            .Where(ct => ct.GetParameters().Length == 1).First();

            return (CustomAttributeTypedArgument)ctor.Invoke(new object[] { value });
        }

        private static CustomAttributeNamedArgument CreateNamedArgument(MemberInfo member, object value)
        {
            var ctor = typeof(CustomAttributeNamedArgument).GetConstructorsOfType(false)
                                .Where(ct => ct.GetParameters().Length == 2 &&
                                       ct.GetParameters()[0].ParameterType == typeof(MemberInfo) &&
                                       ct.GetParameters()[1].ParameterType == typeof(object)).First();

            return (CustomAttributeNamedArgument)ctor.Invoke(new object[] { member, value });
        }


        public static bool IsSecurityAttribute(this Type type)
        {
            return type == typeof(System.Security.Permissions.SecurityAttribute) || type.IsSubclassOf(typeof(System.Security.Permissions.SecurityAttribute));
        }
    }
}
