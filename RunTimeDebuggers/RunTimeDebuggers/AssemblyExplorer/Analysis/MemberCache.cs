using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using RunTimeDebuggers.Helpers;

namespace RunTimeDebuggers.AssemblyExplorer
{
    public class MemberCache
    {
        public MemberInfo Member { get; private set; }

        public MemberCache(MemberInfo member)
        {
            this.Member = member;
            UsedBy = new HashSet<MemberLookupEntry>();
        }

        public HashSet<MemberLookupEntry> UsedBy { get; set; }

        private object usedByLock = new object();
        public void AddUsedBy(MemberInfo m, int offset)
        {
            lock (usedByLock) UsedBy.Add(new MemberLookupEntry(m, offset));
        }
        public void AddUsedBy(IEnumerable<MemberLookupEntry> m)
        {
            lock (usedByLock) UsedBy.AddRange(m);
        }
    }

    public struct MemberLookupEntry
    {
        private MemberInfo member;
        private int offset;

        public MemberLookupEntry(MemberInfo member, int offset)
        {
            this.member = member;
            this.offset = offset;
        }
        public MemberInfo Member { get { return member; } }
        public int Offset { get { return offset; } }

        public override int GetHashCode()
        {
            return member.GetHashCode();
        }


        public override bool Equals(object obj)
        {
            if (obj is MemberLookupEntry)
            {
                var other = (MemberLookupEntry)obj;
                return member.Equals(other.member);

            }
            else
                return false;
        }
    }



}
