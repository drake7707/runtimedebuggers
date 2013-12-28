using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using RunTimeDebuggers.Helpers;
using System.Collections;
using RunTimeDebuggers.LocalsDebugger;

namespace RunTimeDebuggers.AssemblyExplorer
{
    class InstanceFinderManager
    {

        private Type type;
        public InstanceFinderManager(Type type)
        {
            this.type = type;
        }


        //private Dictionary<Type, List<FieldInfo>> fields;

        //private void BuildFieldInfoList()
        //{
        //    fields = new Dictionary<Type, List<FieldInfo>>();
        //    foreach (var ass in AppDomain.CurrentDomain.GetAssemblies())
        //    {
        //        foreach (var t in ass.GetTypesSafe())
        //        {
        //            var fieldsOfTypeThatMatchInstanceType = t.GetFieldsOfType(true, true);
        //            if (fieldsOfTypeThatMatchInstanceType.Count > 0)
        //                fields[t] = fieldsOfTypeThatMatchInstanceType;
        //        }
        //    }
        //}


        public class InstanceResult
        {
            public object Instance { get; set; }
            public FieldInfo Origin { get; set; }

            public override int GetHashCode()
            {
                return Instance.GetType().GUID.GetHashCode() ^ Instance.GetHashCode();
            }
            public override bool Equals(object obj)
            {
                if (!(obj is InstanceResult))
                    return false;

                return object.Equals(Instance, ((InstanceResult)obj).Instance);
            }
        }

        public void Find(int depthLimit)
        {
            objectsAlreadyEvaluated.Clear();
            instances.Clear();
            Cancelled = false;


            for (int i = 0; i < depthLimit; i++)
            {
                FindInstances(i);    
            }
        }

        public void Cancel()
        {
            Cancelled = true;
        }

        private bool cancel;
        public bool Cancelled
        {
            get { lock (cancelLock) return cancel; }
            private set { lock (cancelLock) cancel = value; }
        }
        private object cancelLock = new object();


        private HashSet<InstanceResult> instances = new HashSet<InstanceResult>();

        private HashSet<object> objectsAlreadyEvaluated = new HashSet<object>();

        public IEnumerable<InstanceResult> Instances
        {
            get
            {
                return instances.ToArray();
            }
        }

        public string Status { get; private set; }


        private void FindInstances(int depthLimit)
        {
            // search in same and entry assembly first

            var entryAssembly = Assembly.GetEntryAssembly();
            List<Assembly> assemblies = new List<Assembly>();
            assemblies.Add(type.Assembly);

            if(!assemblies.Contains(entryAssembly))
                assemblies.Add(entryAssembly);

            // don't add this assembly
            var thisAssembly = Assembly.GetAssembly(typeof(InstanceFinderManager));
            assemblies.AddRange(AppDomain.CurrentDomain.GetAssemblies().Where(a => a != type.Assembly && a != entryAssembly && a != thisAssembly));

            foreach (var ass in assemblies)
            {
                if (Cancelled)
                    return;

                foreach (var t in ass.GetTypesSafe())
                {
                    if (Cancelled)
                        return;

                    //TODO add way to iterate over all static fields of generic classes
                    var staticFields = t.GetFieldsOfType(true, true).Where(f => f.IsStatic && !f.DeclaringType.ContainsGenericParameters);

                    foreach (var f in staticFields)
                    {
                        count++;
                        try
                        {
                            object fieldValue = f.GetValue(null);
                            FindInField(depthLimit, f, fieldValue, "");
                        }
                        catch (Exception)
                        {
                        }
                        
                    }
                }
            }
        }
        private int count = 0;


        private void FindInField(int depthLimit, FieldInfo f, object fieldValue, string path)
        {
            FindInField(depthLimit, f, -1, fieldValue, path);
        }

        private void FindInField(int depthLimit, FieldInfo f, int arrayIdx, object fieldValue, string path)
        {
            if (depthLimit <= 0)
                return;

            if (fieldValue != null)
            {
                if (type.IsAssignableFrom(fieldValue.GetType()))
                {
                    AddResult(instances, f, fieldValue);
                }
                else
                {
                    string name = f.GetName(true) + (arrayIdx == -1 ? "" : "[" + arrayIdx + "]");
                    var instancesOfFieldValue = FindInstancesIn(fieldValue, depthLimit - 1, path + " -> " + name);
                    if (instancesOfFieldValue != null)
                        instances.AddRange(instances);
                }

                object[] array = LocalsHelper.AsObjectArrayFromIenumerable(fieldValue);
                if (array != null)
                {
                    // check items
                    for (int i = 0; i < array.Length; i++)
                        FindInField(depthLimit-1, f, i, array[i], path);
                }
            }
        }

        public delegate void InstanceFoundHandler(InstanceResult result);
        public event InstanceFoundHandler InstanceFound;
        protected void OnInstanceFound(InstanceResult result)
        {
            InstanceFoundHandler temp = InstanceFound;
            if (temp != null)
                temp(result);
        }

        private HashSet<InstanceResult> FindInstancesIn(object obj, int depthLimit, string path)
        {
            if (depthLimit <= 0)
                return null;

            if (obj == null)
                return null;

            // don't go looking in primitives
            if (obj is string || obj is char || obj is int || obj is long || obj is short || obj is bool ||
                obj is byte || obj is uint || obj is ushort || obj is ulong || obj is sbyte || obj is float || obj is double)
                return null;


            //// prevent circular evaluations
            //if (!(obj is ValueType) && objectsAlreadyEvaluated.Contains(obj))
            //    return null;

            if (Cancelled)
                return null;

            HashSet<InstanceResult> instances = new HashSet<InstanceResult>();

            Status = "Scanning for instances in " + path;

            foreach (var f in obj.GetType().GetFieldsOfType(true, false))
            {
                if (Cancelled)
                    return null;

                var fieldValue = f.GetValue(obj);
                FindInField(depthLimit, f, fieldValue, path);
            }

            //if(!(obj is ValueType))
            //    objectsAlreadyEvaluated.Add(obj);

            return instances;
        }

        private void AddResult(HashSet<InstanceResult> instances, FieldInfo f, object fieldValue)
        {
            var result = new InstanceResult()
            {
                Instance = fieldValue,
                Origin = f
            };

            if (!instances.Contains(result))
            {
                // instance found
                instances.Add(result);
                OnInstanceFound(result);
            }
        }
    }
}
