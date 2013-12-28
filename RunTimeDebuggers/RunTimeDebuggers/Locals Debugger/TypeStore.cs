using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using RunTimeDebuggers.Helpers;

namespace RunTimeDebuggers.LocalsDebugger
{
    public class TypeStore
    {

        private TypeTree tree = new TypeTree();
        private Dictionary<string, List<Type>> typeByName = new Dictionary<string, List<Type>>();

        private Dictionary<Guid, TypeInfo> typeInfo = new Dictionary<Guid, TypeInfo>();

        public class TypeInfo
        {
            public TypeInfo()
            {
                ExtensionMethods = new List<MethodInfo>();
            }

            //public List<ConstructorInfo> Constructors { get; set; }

            //public List<FieldInfo> Fields { get; set; }
            //public List<PropertyInfo> Properties { get; set; }
            //public List<MethodInfo> Methods { get; set; }

            public List<MethodInfo> ExtensionMethods { get; set; }
        }

        public TypeStore()
        {
            AppDomain.CurrentDomain.AssemblyLoad += new AssemblyLoadEventHandler(CurrentDomain_AssemblyLoad);

            foreach (Assembly a in AppDomain.CurrentDomain.GetAssemblies())
                AddTypesOfAssembly(a);
        }

        void CurrentDomain_AssemblyLoad(object sender, AssemblyLoadEventArgs args)
        {
            AddTypesOfAssembly(args.LoadedAssembly);
            
        }


        private void AddTypesOfAssembly(Assembly a)
        {
            foreach (Type typ in a.GetTypesSafe())
            {
                if (!typ.FullName.Contains('<'))
                { // skip the compiler generated <PrivateImplementationDetails>.....
                    Add(typ);

                    foreach (var pair in typ.GetExtensionMethodsDefinedInType())
                    {
                        TypeInfo tInfo;
                        if (!typeInfo.TryGetValue(pair.Key.GUID, out tInfo))
                            typeInfo.Add(pair.Key.GUID, tInfo = new TypeInfo());

                        tInfo.ExtensionMethods.Add(pair.Value);
                    }
                }
            }
        }

        private void Add(Type t)
        {
            tree.Add(t);

            string name;
            if (t.Name.Contains('`'))
                name = t.Name.Substring(0, t.Name.IndexOf('`'));
            else
                name = t.Name;

            List<Type> types;
            if (!typeByName.TryGetValue(name, out types))
                typeByName.Add(name, types = new List<Type>());
            types.Add(t);


            TypeInfo tInfo;
            if (!typeInfo.TryGetValue(t.GUID, out tInfo))
                typeInfo.Add(t.GUID, tInfo = new TypeInfo());

            //tInfo.Constructors = t.GetConstructorsOfType();
            //tInfo.Fields = t.GetFieldsOfType();
            //tInfo.Properties = t.GetPropertiesOfType();
            //tInfo.Methods = t.GetMethodsOfType();
        }

        public List<MethodInfo> GetExtensionMethodsOf(Type t)
        {
            List<MethodInfo> methods = new List<MethodInfo>();
            Type cur = t;
            while (cur != null)
            {

                TypeInfo tInfo;
                if (typeInfo.TryGetValue(cur.GUID, out tInfo))
                    methods.AddRange(tInfo.ExtensionMethods);


                foreach (var iface in cur.GetInterfaces())
                {
                    if (typeInfo.TryGetValue(iface.GUID, out tInfo))
                        methods.AddRange(tInfo.ExtensionMethods);
                }

                cur = cur.BaseType;
            }
            return methods;
        }


        public List<Type> GetTypesByName(string name)
        {
            Type t = GetSpecialTypeName(name);
            if (t != null)
                return new List<Type>() { t };

            List<Type> types;
            if (typeByName.TryGetValue(name, out types))
                return types;
            else
                return new List<Type>();
        }

        private Type GetSpecialTypeName(string typename)
        {
            if (typename == "string")
                return typeof(string);
            else if (typename == "int")
                return typeof(int);
            else if (typename == "long")
                return typeof(long);
            else if (typename == "short")
                return typeof(short);
            else if (typename == "bool")
                return typeof(bool);
            else if (typename == "float")
                return typeof(float);
            else if (typename == "double")
                return typeof(double);
            else if (typename == "byte")
                return typeof(byte);
            else if (typename == "object")
                return typeof(object);
            else if (typename == "ushort")
                return typeof(ushort);
            else if (typename == "uint")
                return typeof(uint);
            else if (typename == "ulong")
                return typeof(ulong);
            else if (typename == "sbyte")
                return typeof(sbyte);

            return null;
        }



        public List<Type> FindMatchingTypes(string name, List<Type> genericTypes)
        {
            Type typ = GetSpecialTypeName(name);
            if (typ != null)
                return new List<Type>() { typ };

            string nameOnly;
            if (name.Contains('.'))
                nameOnly = name.Split('.').Last();
            else
                nameOnly = name;

            nameOnly = TypeHelper.TrimGenericCounterFromTypeName(nameOnly);

            List<Type> types;
            if (!typeByName.TryGetValue(nameOnly, out types))
                return new List<Type>();

            types = types.Where(t =>
                                {
                                    bool check = TypeHelper.TrimGenericCounterFromTypeName(t.FullName).EndsWith(name);
                                    if (!check)
                                        return false;

                                    var genericArguments = t.GetGenericArguments();
                                    if (t.IsGenericType && genericArguments != null)
                                    {
                                        if (genericTypes.Count == genericArguments.Length)
                                        {
                                            for (int i = 0; i < genericArguments.Length; i++)
                                            {
                                                // check constraints
                                                foreach (var constraint in genericArguments[i].GetGenericParameterConstraints())
                                                {
                                                    if (!constraint.IsAssignableFrom(genericTypes[i]))
                                                        return false;
                                                }
                                            }
                                        }
                                        else
                                            return false;
                                    }

                                    return true;
                                    
                                }).ToList();

            return types;
        }

    


    }

    public class TypeTree
    {
        private TypeNode Root { get; set; }

        public TypeTree()
        {
            Root = new TypeNode();
        }

        public void Add(Type t)
        {
            Root.Add(t);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="fullname"></param>
        /// <returns>1 or more types with specified full name (types can have the same name but with multiple generic parameters)</returns>
        public IEnumerable<Type> GetByFullName(string fullname)
        {
            return Root.GetByFullName(fullname);
        }

        public IEnumerable<Type> GetByPartialName(string fullname)
        {
            return Root.GetByPartialName(fullname);
        }

        private class TypeNode
        {
            private Dictionary<string, TypeNode> nodes;
            public List<Type> possibleTypes = new List<Type>();

            public void Add(Type t)
            {
                string fullname = TypeHelper.TrimGenericCounterFromTypeName(t.FullName);

                string[] fullNameParts = fullname.Split('.');
                Add(fullNameParts, 0, t);
            }

            private void Add(string[] fullNameParts, int idx, Type t)
            {
                possibleTypes.Add(t);

                if (idx < fullNameParts.Length)
                {
                    if (nodes == null)
                        nodes = new Dictionary<string, TypeNode>();

                    TypeNode n;
                    if (!nodes.TryGetValue(fullNameParts[idx], out n))
                    {
                        n = new TypeNode();
                        nodes.Add(fullNameParts[idx], n);
                    }
                    n.Add(fullNameParts, idx + 1, t);
                }
            }

            public IEnumerable<Type> GetByFullName(string fullname)
            {
                string[] parts = fullname.Split('.');

                TypeNode n = this;
                for (int i = 0; i < parts.Length - 1; i++)
                {
                    if (!n.nodes.TryGetValue(parts[i], out n))
                        return null;
                }

                // n is last node
                return n.possibleTypes.Where(typ => TypeHelper.TrimGenericCounterFromTypeName(typ.FullName) == fullname)
                                      .ToList();
            }

            public IEnumerable<Type> GetByPartialName(string fullname)
            {
                string[] parts = fullname.Split('.');

                TypeNode n = this;
                for (int i = 0; i < parts.Length - 1; i++)
                {
                    if (!n.nodes.TryGetValue(parts[i], out n))
                        return null;
                }

                return n.possibleTypes.Where(t => t.Name.StartsWith(parts.Last()))
                                      .ToList();
            }
        }
    }
}
