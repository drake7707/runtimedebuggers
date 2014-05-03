using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using RunTimeDebuggers.Helpers;

namespace RunTimeDebuggers.AssemblyExplorer
{

    public class FieldCache : MemberCache
    {
        public FieldCache(MemberInfo member)
            : base(member)
        {
            AssignedBy = new HashSet<MemberInfo>();
            ReadBy = new HashSet<MemberInfo>();
        }

        public HashSet<MemberInfo> AssignedBy { get; set; }
        public HashSet<MemberInfo> ReadBy { get; set; }

        private object assignedByLock = new object();
        public void AddAssignedBy(MemberInfo m)
        {
            lock (assignedByLock) AssignedBy.Add(m);
        }
        public void AddAssignedBy(IEnumerable<MemberInfo> m)
        {
            lock (assignedByLock) AssignedBy.AddRange(m);
        }

        private object readByLock = new object();
        public void AddReadBy(MemberInfo m)
        {
            lock (readByLock) ReadBy.Add(m);
        }
        public void AddReadBy(IEnumerable<MemberInfo> m)
        {
            lock (readByLock) ReadBy.AddRange(m);
        }

    }
}
