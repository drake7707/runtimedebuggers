using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using RunTimeDebuggers.Helpers;

namespace RunTimeDebuggers.AssemblyExplorer
{

    public class TypeCache
    {
        private Type type;
        public TypeCache(Type type)
        {
            this.type = type;
            UsedBy = new HashSet<MemberInfo>();
            CreatedBy = new HashSet<MemberInfo>();
            DerivedBy = new HashSet<Type>();
            ImplementedBy = new HashSet<Type>();
        }

        public HashSet<MemberInfo> UsedBy { get; set; }
        public HashSet<MemberInfo> CreatedBy { get; set; }
        public HashSet<Type> DerivedBy { get; set; }
        public  HashSet<Type> ImplementedBy { get; set; }

        private object usedByLock = new object();
        public void AddUsedBy(MemberInfo m)
        {
            lock (usedByLock) UsedBy.Add(m);
        }
        public void AddUsedBy(IEnumerable<MemberInfo> m)
        {
            lock (usedByLock) UsedBy.AddRange(m);
        }

        private object createdByLock = new object();
        public void AddCreatedBy(MemberInfo m)
        {
            lock (createdByLock) CreatedBy.Add(m);
        }
        public void AddCreatedBy(IEnumerable<MemberInfo> m)
        {
            lock (createdByLock) CreatedBy.AddRange(m);
        }

        private object derivedByLock = new object();
        public void AddDerivedBy(Type m)
        {
            lock (derivedByLock) DerivedBy.Add(m);
        }
        public void AddDerivedBy(IEnumerable<Type> m)
        {
            lock (derivedByLock) DerivedBy.AddRange(m);
        }


        private object implementedByLock = new object();
        public void AddImplementedBy(Type m)
        {
            lock (implementedByLock) ImplementedBy.Add(m);
        }
        public void AddImplementedBy(IEnumerable<Type> m)
        {
            lock (implementedByLock) ImplementedBy.AddRange(m);
        }
    }
}
