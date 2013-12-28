using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace RunTimeDebuggers.AssemblyExplorer
{
    public class MethodBaseCache : MemberCache
    {
        public MethodBaseCache(MemberInfo member)
            : base(member)
        {
            Uses = new HashSet<MemberLookupEntry>();
            CalledBy = new HashSet<MemberInfo>();
            UsedStrings = new HashSet<string>();

            WiredForEvent = new HashSet<WiredLookupEntry>();
            WiresEvent = new HashSet<WiredLookupEntry>();
        }

        public SpecialMethodEnum MethodType { get; set; }
        public enum SpecialMethodEnum
        {
            None,
            PropertyGet,
            PropertySet,
            EventAdd,
            EventRemove,
            EventRaise
        }
        public MemberInfo SpecialReference { get; set; }


        public HashSet<MemberLookupEntry> Uses { get; set; }
        private object usesLock = new object();
        public void AddUses(MemberInfo m, int offset)
        {
            lock (usesLock) Uses.Add(new MemberLookupEntry(m, offset));
        }
        public void AddUses(IEnumerable<MemberLookupEntry> m)
        {
            lock (usesLock) Uses.AddRange(m);
        }


        public HashSet<MemberInfo> CalledBy { get; set; }

        private object calledByLock = new object();
        public void AddCalledBy(MemberInfo m)
        {
            lock (calledByLock) CalledBy.Add(m);
        }
        public void AddCalledBy(IEnumerable<MemberInfo> m)
        {
            lock (calledByLock) CalledBy.AddRange(m);
        }

        public HashSet<string> UsedStrings { get; set; }

        private object usedStringsLock = new object();
        public void AddUsedString(string s)
        {
            lock (calledByLock) UsedStrings.Add(s);
        }
        public void AddUsedString(IEnumerable<string> s)
        {
            lock (usedStringsLock) UsedStrings.AddRange(s);
        }



        public HashSet<WiredLookupEntry> WiredForEvent { get; set; }

        private object wiredForEventLock = new object();
        public void AddWiredForEvent(WiredLookupEntry m)
        {
            lock (wiredForEventLock) WiredForEvent.Add(m);
        }
        public void AddWiredForEvent(IEnumerable<WiredLookupEntry> m)
        {
            lock (wiredForEventLock) WiredForEvent.AddRange(m);
        }


        public HashSet<WiredLookupEntry> WiresEvent { get; set; }

        private object wiresEventLock = new object();
        public void AddWiresEvent(WiredLookupEntry m)
        {
            lock (wiresEventLock) WiresEvent.Add(m);
        }
        public void AddWiresEvent(IEnumerable<WiredLookupEntry> m)
        {
            lock (wiresEventLock) WiresEvent.AddRange(m);
        }
    }

    public struct WiredLookupEntry
    {
        private MethodInfo eventMethod;
        private MemberInfo source;
        public WiredLookupEntry(MethodInfo eventMethod, MemberInfo source)
        {
            this.eventMethod = eventMethod;
            this.source = source;
        }
        public MethodInfo EventMethod { get { return eventMethod; } }
        public MemberInfo Source { get { return source; } }

        public override int GetHashCode()
        {
            if(source == null)
                return eventMethod.GetHashCode();
            else
                return eventMethod.GetHashCode() ^ source.GetHashCode();
        }
    }

}
