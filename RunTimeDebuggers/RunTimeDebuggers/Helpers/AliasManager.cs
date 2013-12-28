using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RunTimeDebuggers.Controls;
using System.Reflection;
using System.Xml;
using RunTimeDebuggers.AssemblyExplorer;
using System.Reflection.Emit;

namespace RunTimeDebuggers.Helpers
{
    public class AliasManager
    {


        private static AliasManager instance;

        public static AliasManager Instance
        {
            get
            {
                if (instance == null)
                    instance = new AliasManager();

                return instance;
            }
        }

        public AliasManager()
        {
            this.Aliases = new Dictionary<object, string>();
        }

        
        private bool hideNameIfAliasIsPresent;

        public bool HideNameIfAliasIsPresent
        {
            get { return hideNameIfAliasIsPresent; }
            set
            {
                if (hideNameIfAliasIsPresent != value)
                {
                    hideNameIfAliasIsPresent = value;

                    foreach (var a in Aliases )
                        OnAliasChanged(a.Key, a.Value);
                }
            }
        }
        

        public Dictionary<object, string> Aliases { get; private set; }

        internal object GetObjectFromAlias(string alias)
        {
            return Aliases.Where(p => p.Value == alias)
                          .Select(p => p.Key)
                          .FirstOrDefault();
        }

        internal string GetAlias(object obj)
        {
            string str;
            if (Aliases.TryGetValue(obj, out str))
                return str;
            else
                return "";
        }

        internal string GetFullNameWithAlias(object obj, string objNameRepresentation)
        {
            string alias = GetAlias(obj);
            if (string.IsNullOrEmpty(alias))
                return objNameRepresentation;
            else
            {
                if (HideNameIfAliasIsPresent)
                    return alias;
                else
                    return "#" + alias + "#" + objNameRepresentation;
            }
        }

        internal bool FilterOnAliasName(object obj, string filterstring)
        {
            string str;
            if (Aliases.TryGetValue(obj, out str))
            {
                if (str.ToLower().Contains(filterstring))
                    return true;
            }
            return false;
        }

        internal void SetAlias(object obj, string alias)
        {
            bool changed = false;
            if (string.IsNullOrEmpty(alias))
            {
                if (Aliases.ContainsKey(obj))
                {
                    changed = true;
                    Aliases.Remove(obj);
                }
            }
            else
            {
                string previousAlias;
                if (Aliases.TryGetValue(obj, out previousAlias))
                {
                    if (previousAlias != alias)
                    {
                        changed = true;
                        Aliases[obj] = alias;
                    }
                }
                else
                {
                    changed = true;
                    Aliases[obj] = alias;
                }
            }

            if (changed)
            {
                OnAliasChanged(obj, alias);
            }
        }

        private void OnAliasChanged(object obj, string alias)
        {
            AliasChangedHandler temp = AliasChanged;
            if (temp != null)
                temp(obj, alias);
        }

        public bool PromptAlias(System.Windows.Forms.IWin32Window parent, object obj)
        {
            if (obj != null)
            {
                using (InputBox dlg = new InputBox() { DialogText = "Change alias", Value = AliasManager.Instance.GetAlias(obj) })
                {
                    if (dlg.ShowDialog(parent) == System.Windows.Forms.DialogResult.OK)
                    {
                        AliasManager.Instance.SetAlias(obj, dlg.Value);
                        return true;
                    }
                    else
                        return false;
                }
            }
            return false;
        }

        public event AliasChangedHandler AliasChanged;
        public delegate void AliasChangedHandler(object obj, string alias);


        private string GetPrefixOfType(Type t)
        {
            if (typeof(System.Windows.Forms.Button).IsAssignableFrom(t))
                return "btn";
            else if (typeof(System.Windows.Forms.CheckBox).IsAssignableFrom(t))
                return "chk";
            else if (typeof(System.Windows.Forms.ListBox).IsAssignableFrom(t))
                return "lst";
            else if (typeof(System.Windows.Forms.ComboBox).IsAssignableFrom(t))
                return "cmb";
            else if (typeof(System.Windows.Forms.RadioButton).IsAssignableFrom(t))
                return "rdb";
            else if (typeof(System.Windows.Forms.MenuItem).IsAssignableFrom(t))
                return "mnu";
            else if (typeof(System.Windows.Forms.TextBox).IsAssignableFrom(t))
                return "txt";
            else if (typeof(System.Windows.Forms.TreeView).IsAssignableFrom(t))
                return "tv";
            else if (typeof(System.Windows.Forms.ToolStripMenuItem).IsAssignableFrom(t))
                return "mnu";
            else if (typeof(System.Windows.Forms.TabPage).IsAssignableFrom(t))
                return "tp";

            return "";
        }


        public void AutoAliasAssembly(Assembly a, bool nonPublicOnly)
        {
            Random rnd = new Random();
            string[] nouns = RunTimeDebuggers.Properties.Resources.nounlist.Split('\n').OrderBy(n => rnd.Next()).ToArray();

            int nounIdx = 0;
            foreach (var t in a.GetTypesSafe())
            {
                if (!nonPublicOnly || (nonPublicOnly && t.IsNotPublic))
                {
                    string noun = nouns[(nounIdx + 1 >= nouns.Length) ? (nounIdx = 0) : nounIdx++];
                    if(typeof(System.Windows.Forms.Form).IsAssignableFrom(t))
                        SetAlias(t, "frm" + noun.Substring(0,1).ToUpper() + noun.Substring(1));
                    else
                        SetAlias(t, noun);
                }
                foreach (var f in t.GetFieldsOfType(false, true))
                {
                    if (!nonPublicOnly || (nonPublicOnly && !f.IsPublic))
                    {
                        string noun = nouns[(nounIdx + 1 >= nouns.Length) ? (nounIdx = 0) : nounIdx++];
                        string prefix = GetPrefixOfType(f.FieldType);
                        if(!string.IsNullOrEmpty(prefix))
                            SetAlias(f, prefix + noun.Substring(0, 1).ToUpper() + noun.Substring(1));
                        else
                            SetAlias(f, noun);
                    }
                     
                }

                foreach (var p in t.GetPropertiesOfType(false, true))
                {
                    if (!nonPublicOnly || (nonPublicOnly && (p.CanRead && !p.GetGetMethod(true).IsPublic)))
                        SetAlias(p, nouns[(nounIdx + 1 >= nouns.Length) ? (nounIdx = 0) : nounIdx++]);
                }
            }

            foreach (var t in a.GetTypesSafe())
            {
                string eventName = "";
                foreach (var m in t.GetMethodsOfType(false, true))
                {


                    if (m.GetParameters().Where(p => p.ParameterType.Name.Contains("EventArgs")).Any())
                    {
                        // it's probably an event handler
                        // go go gadget scan where it might be used to wire the event
                        var cache = AnalysisManager.Instance.GetMemberCache(m);
                        if (((MethodBaseCache)cache).WiredForEvent.Count > 0 && ((MethodBaseCache)cache).WiredForEvent.First().Source != null)
                        {
                            var sourceName = ((MethodBaseCache)cache).WiredForEvent.First().Source.GetName(false);
                            var eventMethod = ((MethodBaseCache)cache).WiredForEvent.First().EventMethod;
                            var eventMethodCache = ((MethodBaseCache)AnalysisManager.Instance.GetMemberCache(eventMethod));
                            if (eventMethodCache.SpecialReference != null)
                                eventName = sourceName + "_" + eventMethodCache.SpecialReference.GetName(false);
                            else
                                eventName = sourceName + "_" + eventMethod.GetName(false).Replace("add_", "").Replace("Handler", "");
                            
                        }
                        else
                            eventName = "";
                    }
                    else
                        eventName = "";

                    if (!nonPublicOnly || (nonPublicOnly && !m.IsPublic))
                    {
                        if (!string.IsNullOrEmpty(eventName))
                            SetAlias(m, eventName + "Handler");
                        else
                            SetAlias(m, nouns[(nounIdx + 1 >= nouns.Length) ? (nounIdx = 0) : nounIdx++]);
                    }
                }
            }
        }

        private static string GetEventWire(MemberCache cache)
        {
            var ctorOfEventHandler = typeof(EventHandler).GetConstructorsOfType(false).First();
            foreach (var use in cache.UsedBy)
            {
                if (use.Member is MethodInfo)
                {
                    var usedInMethod = (MethodInfo)use.Member;
                    var ilinstructions = usedInMethod.GetILInstructions();

                    string eventName = "";
                    var instructionUsed = ilinstructions.Where(il => il.Offset == use.Offset).FirstOrDefault();
                    if (instructionUsed != null)
                    {
                        if (instructionUsed.Code == OpCodes.Ldftn &&
                           instructionUsed.Next != null && instructionUsed.Next.Code == OpCodes.Newobj && (ConstructorInfo)instructionUsed.Next.Operand == ctorOfEventHandler)
                        {
                            // it's about to get registered to an event
                            var wireInstruction = instructionUsed.Next.Next;
                            if (wireInstruction != null && wireInstruction.Code.FlowControl == FlowControl.Call && wireInstruction.Operand is MethodInfo)
                            {
                                // this should be the eventname if it isn't obfuscated
                                eventName = ((MethodInfo)(wireInstruction.Operand)).Name.Replace("add_", "");

                                // search for the field that has the event
                                if (instructionUsed.Previous != null && instructionUsed.Previous.Previous != null)
                                {
                                    var loadFieldInstruction = instructionUsed.Previous.Previous;
                                    if (loadFieldInstruction.Code == OpCodes.Ldfld || loadFieldInstruction.Code == OpCodes.Ldflda)
                                    {
                                        eventName = ((FieldInfo)loadFieldInstruction.Operand).GetName(false) + "_" + eventName;
                                    }
                                }

                                return eventName;
                            }
                        }
                    }


                }
            }
            return null;
        }

        public void SaveAliases(string path)
        {
            var writer = System.Xml.XmlWriter.Create(path);

            writer.WriteStartDocument();
            writer.WriteStartElement("Aliases");

            foreach (var pair in Aliases)
            {
                writer.WriteStartElement("Alias");

                if (pair.Key is Type)
                {
                    Type t = (Type)pair.Key;

                    writer.WriteAttributeString("AliasType", "Type");
                    writer.WriteAttributeString("Name", t.AssemblyQualifiedName);
                    writer.WriteAttributeString("Token", t.MetadataToken.ToString());
                    writer.WriteAttributeString("ModuleToken", t.Module.MetadataToken.ToString());
                    writer.WriteAttributeString("ModuleName", t.Module.FullyQualifiedName.ToString());
                    writer.WriteAttributeString("Assembly", t.Module.Assembly.FullName);
                    writer.WriteAttributeString("As", pair.Value);
                }
                else if (pair.Key is MemberInfo)
                {
                    MemberInfo m = (MemberInfo)pair.Key;

                    writer.WriteAttributeString("AliasType", "Member");
                    writer.WriteAttributeString("Name", m.Name);

                    if (m.DeclaringType != null)
                    {
                        writer.WriteAttributeString("DeclaringType", m.DeclaringType.AssemblyQualifiedName);
                        writer.WriteAttributeString("DeclaringTypeToken", m.DeclaringType.MetadataToken.ToString());
                    }
                    else
                    {
                        writer.WriteAttributeString("DeclaringType", "");
                        writer.WriteAttributeString("DeclaringTypeToken", "");
                    }
                    writer.WriteAttributeString("ModuleToken", m.Module.MetadataToken.ToString());
                    writer.WriteAttributeString("ModuleName", m.Module.FullyQualifiedName.ToString());

                    writer.WriteAttributeString("Assembly", m.Module.Assembly.FullName);
                    writer.WriteAttributeString("Token", m.MetadataToken.ToString());
                    writer.WriteAttributeString("As", pair.Value);
                }
                writer.WriteEndElement();
            }


            writer.WriteEndElement();
            writer.WriteEndDocument();

            writer.Close();
        }

        public void LoadAliases(string path)
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(path);

            foreach (XmlNode n in doc.SelectNodes("//Alias"))
            {
                if (n.Attributes["AliasType"].Value == "Type")
                {
                    string typeFullyQualifiedName = n.Attributes["Name"].Value;
                    // attempt to resolve type
                    Type t;
                    try
                    {
                        t = Type.GetType(typeFullyQualifiedName, false);
                    }
                    catch (Exception)
                    {
                        Console.WriteLine("Unable to resolve type " + typeFullyQualifiedName + " while loading aliases");
                        t = null;
                    }

                    if (t == null)
                    {
                        // attempt to find by assembly, module and token
                        var assemblies = AppDomain.CurrentDomain.GetAssemblies().Where(a => a.FullName == n.Attributes["Assembly"].Value);
                        int moduleToken = int.Parse(n.Attributes["ModuleToken"].Value);
                        int typeToken = int.Parse(n.Attributes["Token"].Value);
                        t = ResolveType(typeFullyQualifiedName, assemblies, moduleToken, typeToken);
                    }
                    if (t != null)
                        SetAlias(t, n.Attributes["As"].Value);
                }
                else if (n.Attributes["AliasType"].Value == "Member")
                {
                    if (!string.IsNullOrEmpty(n.Attributes["DeclaringType"].Value))
                    {
                        var assemblies = AppDomain.CurrentDomain.GetAssemblies().Where(a => a.FullName == n.Attributes["Assembly"].Value);
                        int moduleToken = int.Parse(n.Attributes["ModuleToken"].Value);
                        int memberToken = int.Parse(n.Attributes["Token"].Value);
                        string memberName = n.Attributes["Name"].Value;

                        MemberInfo member = null;

                        foreach (var ass in assemblies)
                        {
                            if (member != null)
                                break;

                            var module = ass.GetModules().Where(m => m.MetadataToken == moduleToken).FirstOrDefault();
                            if (module != null)
                            {
                                try
                                {
                                    member = module.ResolveMember(memberToken);
                                }
                                catch (Exception)
                                {
                                    member = null;
                                }
                                if (member != null && member.Name == memberName)
                                {
                                    // OK
                                }
                                else
                                    member = null;
                            }
                        }

                        if (member != null)
                            SetAlias(member, n.Attributes["As"].Value);
                    }
                }
            }

        }

        private static Type ResolveType(string typeFullyQualifiedName, IEnumerable<Assembly> assemblies, int moduleToken, int typeToken)
        {
            Type t = null;
            foreach (var ass in assemblies)
            {
                if (t != null)
                    break;

                var module = ass.GetModules().Where(m => m.MetadataToken == moduleToken).FirstOrDefault();
                if (module != null)
                {
                    try
                    {
                        t = module.ResolveType(typeToken);
                    }
                    catch (Exception)
                    {
                        t = null;
                    }
                    if (t != null && t.AssemblyQualifiedName == typeFullyQualifiedName)
                    {
                        // OK
                    }
                    else
                        t = null;
                }
            }
            return t;
        }
    }
}
