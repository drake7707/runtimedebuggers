using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using RunTimeDebuggers.Helpers;
using System.Reflection;

namespace RunTimeDebuggers.AssemblyExplorer
{
    partial class DecryptStrings : TreeNodeControl
    {
        private MethodInfo decryptionMethod;
        private MethodBase specificMethod;

        public DecryptStrings(IAssemblyBrowser browser, MethodInfo decryptionMethod)
            : this(browser, decryptionMethod, null)
        { }

        public DecryptStrings(IAssemblyBrowser browser, MethodInfo decryptionMethod, MethodBase specificMethod)
            : base(browser)
        {
            this.browser = browser;
            this.decryptionMethod = decryptionMethod;
            this.specificMethod = specificMethod;

            InitializeComponent();

            Initialize(tvSearchResults);

            BuildDecryptedStrings();
        }



        private void txtFilter_TextChanged(object sender, EventArgs e)
        {
            tmrFilter.Enabled = false;
            tmrFilter.Enabled = true;
        }

        private void BuildDecryptedStrings()
        {

            pbar.Visible = true;
            var ui = WindowsFormsSynchronizationContext.Current;

            TaskFactory f = new TaskFactory(1);
            f.StartTask<MethodInfo, Dictionary<MethodBase, List<DecryptedString>>>(dm => GetDecryptedStrings(dm), decryptionMethod, strings =>
            {
                decryptedStrings = strings;
                ui.Send(o =>
                {
                    ((DecryptStrings)o).pbar.Visible = false;

                    Filter();
                    ((DecryptStrings)o).tvSearchResults.ExpandAll();
                }, this);
            });

        }

        private Dictionary<MethodBase, List<DecryptedString>> decryptedStrings;

        private void BuildNodes(System.Threading.SynchronizationContext ui, string filterstring)
        {
            if (decryptedStrings == null)
                return;

            foreach (var pair in decryptedStrings)
            {
                MemberNode n = null;
                foreach (DecryptedString ds in pair.Value)
                {
                    if (string.IsNullOrEmpty(filterstring) || pair.Key.Name.ToLower().Contains(filterstring) ||
                        (ds != null && ds.Decrypted.ToLower().Contains(filterstring)))
                    {
                        if (n == null)
                            n = MemberNode.GetNodeOfMember(pair.Key, true);
                        n.Nodes.Add(ds.ILOffset.ToString("x4") + ": '" + ds.Decrypted + "' (=" + ds.Encrypted + ")");
                    }
                }

                if (n != null)
                {
                    ui.Send(o =>
                    {
                        ((DecryptStrings)o).tvSearchResults.Nodes.Add(n);
                    }, this);
                }
            }
        }

        private class DecryptedString
        {
            public string Encrypted { get; set; }
            public string Decrypted { get; set; }
            public int ILOffset { get; set; }
        }
        private Dictionary<MethodBase, List<DecryptedString>> GetDecryptedStrings(MethodInfo decryptionMethod)
        {
            ParameterInfo[] decryptionParameters = decryptionMethod.GetParameters();

            Dictionary<MethodBase, List<DecryptedString>> stringsPerMethod = new Dictionary<MethodBase, List<DecryptedString>>();

            var mc = AnalysisManager.Instance.GetMemberCache(decryptionMethod);

            IEnumerable<MemberInfo> methodsToInspect;
            if (specificMethod != null)
                methodsToInspect = new MemberInfo[] { specificMethod };
            else
                methodsToInspect = mc.UsedBy.Select(entry => entry.Member);

            foreach (MemberInfo mUsing in methodsToInspect)
            {
                if (mUsing is MethodBase)
                {
                    List<DecryptedString> decryptedStrings = new List<DecryptedString>();

                    try
                    {
                        var instructions = ((MethodBase)mUsing).GetILInstructions();

                        var instructionsCallingDecrypt = instructions.Where(i => object.Equals(i.Operand, decryptionMethod));

                        foreach (var call in instructionsCallingDecrypt)
                        {
                            object[] parameterValues = new object[decryptionParameters.Length];
                            var curInstruction = call.Previous;
                            int curParamIndex = decryptionParameters.Length - 1;
                            while (curInstruction != null && curParamIndex >= 0)
                            {
                                object valuePutOnStackForInstruction = curInstruction.ConstantValuePutOnStack;
                                if (valuePutOnStackForInstruction != null &&
                                   decryptionParameters[curParamIndex].ParameterType.IsAssignableFrom(valuePutOnStackForInstruction.GetType()))
                                {
                                    parameterValues[curParamIndex] = valuePutOnStackForInstruction;
                                    curParamIndex--;
                                }
                                curInstruction = curInstruction.Previous;
                            }

                            if (curParamIndex < 0)
                            {
                                string decryptedString = (string)decryptionMethod.Invoke(null, parameterValues);

                                DecryptedString ds = new DecryptedString()
                                {
                                    Decrypted = decryptedString,
                                    Encrypted = parameterValues.Where(p => p is string).FirstOrDefault() + "",
                                    ILOffset = call.Offset
                                };
                                decryptedStrings.Add(ds);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        decryptedStrings.Add(new DecryptedString() { Decrypted = "#ERROR: Unable to decrypt strings, " + ex.GetType().FullName + " - " + ex.Message, Encrypted = "", ILOffset = 0 });
                    }
                    stringsPerMethod.Add((MethodBase)mUsing, decryptedStrings);
                }
            }

            return stringsPerMethod;
        }

        private void Filter()
        {
            // if data already available
            if (decryptedStrings == null)
                return;

            tvSearchResults.Nodes.Clear();

            pbar.Visible = true;

            var ui = WindowsFormsSynchronizationContext.Current;

            string filterstring = txtFilter.Text.ToLower();
            TaskFactory f = new TaskFactory(1);
            f.StartTask(() => BuildNodes(ui, filterstring), () =>
            {
                ui.Send(o =>
                {
                    ((DecryptStrings)o).pbar.Visible = false;
                    ((DecryptStrings)o).tvSearchResults.ExpandAll();
                }, this);
            });
        }



        private void tmrFilter_Tick(object sender, EventArgs e)
        {
            tmrFilter.Enabled = false;
            Filter();
        }
    }
}
