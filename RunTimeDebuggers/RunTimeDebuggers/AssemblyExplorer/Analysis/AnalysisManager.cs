using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using RunTimeDebuggers.Helpers;
using System.Reflection.Emit;

namespace RunTimeDebuggers.AssemblyExplorer
{
    class AnalysisManager
    {

        private static AnalysisManager instance;
        public static AnalysisManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new AnalysisManager();
                }
                return instance;
            }
        }


        private AnalysisManager()
        {

        }

        private struct Member
        {
            public Member(MemberInfo m)
            {
                this.moduleToken = m.Module.MetadataToken;
                this.token = m.MetadataToken;
                this.assemblyHashCode = m.Module.Assembly.GetHashCode();
            }

            private int assemblyHashCode;

            private int moduleToken;
            //public int Module { get { return moduleToken; } }
            private int token;
            //public int MetadataToken { get { return token; } }

            //public override bool Equals(object obj)
            //{
            //    if(!(obj is Member))
            //        return false;

            //    return token == ((Member)obj).token && moduleToken  == ((Member)obj).moduleToken;
            //}
        }
        private Dictionary<Member, MemberCache> memberCache = new Dictionary<Member, MemberCache>();
        private Dictionary<Guid, TypeCache> typeCache = new Dictionary<Guid, TypeCache>();





        private object lockTypeCache = new object();
        private object lockMethodCache = new object();
        public TypeCache GetTypeCache(Type t)
        {
            lock (lockTypeCache)
            {
                TypeCache c;
                if (!typeCache.TryGetValue(t.GUID, out c))
                    typeCache.Add(t.GUID, c = new TypeCache(t));
                return c;
            }
        }

        public MemberCache GetMemberCache(MemberInfo member)
        {
            lock (lockMethodCache)
            {
                MemberCache c;
                if (!memberCache.TryGetValue(new Member(member), out c))
                {
                    if (member is FieldInfo)
                        memberCache.Add(new Member(member), c = new FieldCache(member));
                    else if (member is MethodBase)
                        memberCache.Add(new Member(member), c = new MethodBaseCache(member));
                    else
                        memberCache.Add(new Member(member), c = new MemberCache(member));
                }
                return c;
            }
        }


        public IEnumerable<TypeCache> TypeCaches
        {
            get
            {
                return typeCache.Values;
            }
        }

        public IEnumerable<MemberCache> MemberCaches
        {
            get
            {
                return memberCache.Values;
            }
        }

        void CurrentDomain_AssemblyLoad(object sender, AssemblyLoadEventArgs args)
        {
            BuildCacheForAssembly(args.LoadedAssembly);
        }

        private bool cacheIsBuilding;

        public double CacheBuildProgress { get; private set; }

        public void BuildCache()
        {
            if (CacheLoaded || cacheIsBuilding)
                return;

            cacheIsBuilding = true;
            TaskFactory taskfactory = new TaskFactory(4);

            foreach (var ass in AppDomain.CurrentDomain.GetAssemblies())
            {
                var t = taskfactory.StartTask<Assembly>(a =>
                {
                    BuildCacheForAssembly(a);
                }, ass, () => CacheBuildProgress = taskfactory.Progress);

            }

            taskfactory.WaitAll();
            cacheIsBuilding = false;
            CacheLoaded = true;

            AppDomain.CurrentDomain.AssemblyLoad += new AssemblyLoadEventHandler(CurrentDomain_AssemblyLoad);
        }

        private void BuildCacheForAssembly(Assembly a)
        {
            Type[] types;
            try
            {
                types = a.GetTypesSafe().ToArray();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Unable to fetch types info for " + a.FullName + ", error: " + ex.ToExceptionString());
                types = Type.EmptyTypes;
            }

            foreach (var t in types)
            {
                //           Console.WriteLine("Analyzing type: " + t.FullName);

                if (t.BaseType != null)
                    GetTypeCache(t.BaseType).AddDerivedBy(t);

                foreach (var iface in t.GetInterfacesOfType(false))
                    GetTypeCache(iface).AddImplementedBy(t);

                try
                {
                    foreach (var f in t.GetFieldsOfType(false, true))
                        GetTypeCache(f.FieldType).AddUsedBy(f);
                }
                catch (Exception ex)
                {
                    // don't crash if unable to fetch all field info
                    Console.WriteLine("Unable to fetch field info for " + t.Name + ", error: " + ex.ToExceptionString());
                }

                try
                {
                    foreach (var prop in t.GetPropertiesOfType(false, true))
                    {
                        if (prop.CanRead)
                        {
                            var getMethod = prop.GetGetMethod(true);
                            if (getMethod != null)
                                BuildCacheFor(getMethod, MethodBaseCache.SpecialMethodEnum.PropertyGet, prop, getMethod);
                        }
                        if (prop.CanWrite)
                        {
                            var setMethod = prop.GetSetMethod(true);
                            if (setMethod != null)
                                BuildCacheFor(setMethod, MethodBaseCache.SpecialMethodEnum.PropertySet, prop, setMethod);
                        }
                    }
                }
                catch (Exception ex)
                {
                    // don't crash if unable to fetch all property info
                    Console.WriteLine("Unable to fetch property info for " + t.Name + ", error: " + ex.ToExceptionString());
                }

                try
                {
                    foreach (var m in t.GetMethodsOfType(false, true))
                        BuildCacheFor(m, MethodBaseCache.SpecialMethodEnum.None, m);
                }
                catch (Exception ex)
                {
                    // don't crash if unable to fetch all method info
                    Console.WriteLine("Unable to fetch method info for " + t.Name + ", error: " + ex.ToExceptionString());
                }

                try
                {
                    foreach (var c in t.GetConstructorsOfType(true))
                        BuildCacheFor(c, MethodBaseCache.SpecialMethodEnum.None, c);
                }
                catch (Exception ex)
                {
                    // don't crash if unable to fetch all constructor info
                    Console.WriteLine("Unable to fetch constructor info for " + t.Name + ", error: " + ex.ToExceptionString());
                }

                try
                {
                    foreach (var ev in t.GetEventsOfType(false, true))
                    {
                        var addMethod = ev.GetAddMethod(true);
                        if (addMethod != null)
                            BuildCacheFor(addMethod, MethodBaseCache.SpecialMethodEnum.EventAdd, ev, addMethod);
                        var removeMethod = ev.GetRemoveMethod(true);
                        if (removeMethod != null)
                            BuildCacheFor(removeMethod, MethodBaseCache.SpecialMethodEnum.EventRemove, ev, removeMethod);
                        var raiseMethod = ev.GetRaiseMethod(true);
                        if (raiseMethod != null)
                            BuildCacheFor(raiseMethod, MethodBaseCache.SpecialMethodEnum.EventRaise, ev, raiseMethod);
                    }

                }
                catch (Exception ex)
                {
                    // don't crash if unable to fetch all event info
                    Console.WriteLine("Unable to fetch event info for " + t.Name + ", error: " + ex.ToExceptionString());
                }
            }
        }


        public bool CacheLoaded { get; private set; }



        private void BuildCacheFor(MethodBase m, MethodBaseCache.SpecialMethodEnum methodType, PropertyInfo prop, params MemberInfo[] addFor)
        {
            MethodBaseCache cacheForM = ((MethodBaseCache)GetMemberCache(m));

            // check if method is used in a special context (property/event)
            if (cacheForM.MethodType == MethodBaseCache.SpecialMethodEnum.None && methodType != MethodBaseCache.SpecialMethodEnum.None)
                cacheForM.MethodType = methodType;
            cacheForM.SpecialReference = prop;

            BuildCacheFor(m, methodType, addFor);
        }

        private void BuildCacheFor(MethodBase m, MethodBaseCache.SpecialMethodEnum methodType, EventInfo ev, params MemberInfo[] addFor)
        {
            MethodBaseCache cacheForM = ((MethodBaseCache)GetMemberCache(m));

            // check if method is used in a special context (property/event)
            if (cacheForM.MethodType == MethodBaseCache.SpecialMethodEnum.None && methodType != MethodBaseCache.SpecialMethodEnum.None)
                cacheForM.MethodType = methodType;
            cacheForM.SpecialReference = ev;

            BuildCacheFor(m, methodType, addFor);
        }


        private void BuildCacheFor(MethodBase m, MethodBaseCache.SpecialMethodEnum methodType, params MemberInfo[] addFor)
        {
            MethodBaseCache cacheForM = ((MethodBaseCache)GetMemberCache(m));

            // check if method is used in a special context (property/event)
            if (cacheForM.MethodType == MethodBaseCache.SpecialMethodEnum.None && methodType != MethodBaseCache.SpecialMethodEnum.None)
                cacheForM.MethodType = methodType;

            foreach (var p in m.GetParameters())
            {
                if (!p.ParameterType.IsGenericParameter)
                    GetTypeCache(p.ParameterType).AddUsedBy(addFor);
            }

            if (m is MethodInfo)
                GetTypeCache(((MethodInfo)m).ReturnType).AddUsedBy(addFor);

            try
            {
                foreach (var i in m.GetILInstructions())
                {
                    if (i.Operand is MemberInfo)
                    {
                        var cache = GetMemberCache((MemberInfo)i.Operand);

                        foreach (var memberToAddFor in addFor)
                            cache.AddUsedBy(memberToAddFor, i.Offset);

                        cacheForM.AddUses((MemberInfo)i.Operand, i.Offset);

                        if (i.Code == OpCodes.Call || i.Code == OpCodes.Callvirt)
                            ((MethodBaseCache)cache).AddCalledBy(addFor);
                        else if (i.Code == OpCodes.Stfld)
                            ((FieldCache)cache).AddAssignedBy(addFor);
                        else if (i.Code == OpCodes.Ldfld)
                            ((FieldCache)cache).AddReadBy(addFor);
                        else if (i.Code == OpCodes.Ldftn) // event wiring in c# uses ldftn to pass methods to the handler delegate constructor
                        {
                            // read as pointer, possibly for event
                            if (i.Operand is MethodBase && i.Next.Code == OpCodes.Newobj && (i.Next.Next.Code == OpCodes.Callvirt || i.Next.Next.Code == OpCodes.Call))
                            {
                                var addMethod = (MethodInfo)i.Next.Next.Operand;
                                var addMethodCache = GetMemberCache(addMethod);

                                MemberInfo source = null;
                                if (i.Previous.Previous.Code == OpCodes.Ldfld)
                                    source = (FieldInfo)i.Previous.Previous.Operand;
                                else if (i.Previous.Previous.Code == OpCodes.Call || i.Previous.Previous.Code == OpCodes.Callvirt)
                                    source = (MethodInfo)i.Previous.Previous.Operand;

                                if (source != null)
                                {
                                    // todo add event info to source
                                }
                                ((MethodBaseCache)cache).AddWiredForEvent(new WiredLookupEntry(addMethod, source));
                                ((MethodBaseCache)addMethodCache).AddWiresEvent(new WiredLookupEntry((MethodInfo)i.Operand, source));
                            }
                        }
                        else if (i.Code == OpCodes.Ldvirtftn) // VB.NET uses event wiring in a property setter
                        {

                            if (i.Operand is MethodBase && i.Next.Code == OpCodes.Newobj)
                            {
                                var nextnext = i.Next.Next;

                                int idxOfEventLocal = -1;
                                if (nextnext.Code == OpCodes.Stloc || nextnext.Code == OpCodes.Stloc_S)
                                    idxOfEventLocal = (int)nextnext.Operand;
                                else if (nextnext.Code == OpCodes.Stloc_0)
                                    idxOfEventLocal = 0;
                                else if (nextnext.Code == OpCodes.Stloc_1)
                                    idxOfEventLocal = 1;
                                else if (nextnext.Code == OpCodes.Stloc_2)
                                    idxOfEventLocal = 2;
                                else if (nextnext.Code == OpCodes.Stloc_3)
                                    idxOfEventLocal = 3;

                                if (idxOfEventLocal != -1)
                                {
                                    // go find the ldloc

                                    ILInstruction curInstr = i;
                                    while (curInstr != null)
                                    {
                                        if ((((curInstr.Code == OpCodes.Ldloc || curInstr.Code == OpCodes.Ldloc_S) && (int)curInstr.Operand == idxOfEventLocal) ||
                                            (curInstr.Code == OpCodes.Ldloc_0 && idxOfEventLocal == 0) ||
                                            (curInstr.Code == OpCodes.Ldloc_1 && idxOfEventLocal == 1) ||
                                            (curInstr.Code == OpCodes.Ldloc_2 && idxOfEventLocal == 2) ||
                                            (curInstr.Code == OpCodes.Ldloc_3 && idxOfEventLocal == 3))
                                            &&
                                            (curInstr.Next.Code == OpCodes.Call || curInstr.Next.Code == OpCodes.Callvirt) &&
                                            curInstr.Previous.Code == OpCodes.Ldfld &&
                                            ((MethodInfo)curInstr.Next.Operand).Name.StartsWith("add_"))
                                        {
                                            MemberInfo source = (MemberInfo)curInstr.Previous.Operand;
                                            var addMethod = (MethodInfo)curInstr.Next.Operand;
                                            var addMethodCache = GetMemberCache(addMethod);


                                            if (source != null)
                                            {
                                                // todo add event info to source
                                            }
                                            ((MethodBaseCache)cache).AddWiredForEvent(new WiredLookupEntry(addMethod, source));
                                            ((MethodBaseCache)addMethodCache).AddWiresEvent(new WiredLookupEntry((MethodInfo)i.Operand, source));
                                            break;
                                        }

                                        curInstr = curInstr.Next;
                                    }
                                }
                            }


                        }
                        else if (i.Code == OpCodes.Newobj)
                            GetTypeCache(((ConstructorInfo)i.Operand).DeclaringType).AddCreatedBy(addFor);
                    }

                    if (i.Code == OpCodes.Ldstr)
                    {
                        cacheForM.AddUsedString((string)i.Operand);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Unable to build analysis cache for " + m.ToSignatureString() + ", error: " + ex.GetType().FullName + " - " + ex.Message);
            }
        }



    }

    static class Extensions
    {
        public static void AddRange<T>(this HashSet<T> h, IEnumerable<T> objs)
        {
            foreach (var o in objs)
                h.Add(o);
        }
    }
}
