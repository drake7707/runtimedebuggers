using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;

namespace RunTimeDebuggers.LocalsDebugger
{
    internal interface ILocalsNode
    {
        void Evaluate();

        bool Changed { get; set; }
    }



    internal class LocalsHelper
    {

        private static object[] AsObjectArrayFromGenericIenumerable(object obj)
        {
            var castToObjects = typeof(System.Linq.Enumerable).GetMethod("Cast").MakeGenericMethod(typeof(object)).Invoke(null, new object[] { obj });
            object[] items = (object[])typeof(System.Linq.Enumerable).GetMethod("ToArray").MakeGenericMethod(typeof(object)).Invoke(null, new object[] { castToObjects });
            return items;
        }

        private static HashSet<Guid> typeBlacklist = new HashSet<Guid>();

        public static object[] AsObjectArrayFromIenumerable(object obj)
        {
            if (typeBlacklist.Contains(obj.GetType().GUID)) // screw it!
                return null;

            List<object> objects;
            if (obj is IEnumerable)
            {
                objects = new List<object>();

                try
                {
                    foreach (var o in (IEnumerable)obj)
                        objects.Add(o);

                }
                catch (NullReferenceException)
                {
                    // if .NET can't provide me with a goddamn decent implementation of the enumerator then blacklist it
                    typeBlacklist.Add(obj.GetType().GUID);
                }

                return objects.ToArray();
            }
            else if (obj is ICollection)
            {
                objects = new List<object>();
                try
                {
                    foreach (var o in (ICollection)obj)
                        objects.Add(o);

                }
                catch (NullReferenceException)
                {
                    // if .NET can't provide me with a goddamn decent implementation of the enumerator then blacklist it
                    typeBlacklist.Add(obj.GetType().GUID);
                }
                return objects.ToArray();
            }
            else
            {
                var ienumerableInterface = obj.GetType().GetInterfaces().Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEnumerable<>)).FirstOrDefault();
                if (ienumerableInterface != null)
                    return AsObjectArrayFromGenericIenumerable(obj);
            }

            return null;
        }

    }


}
