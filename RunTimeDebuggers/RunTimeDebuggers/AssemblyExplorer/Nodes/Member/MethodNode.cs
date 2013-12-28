using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Reflection;
using RunTimeDebuggers.Helpers;

namespace RunTimeDebuggers.AssemblyExplorer
{
    class MethodNode : MemberNode
    {
        public MethodInfo Method { get; private set; }

        public MethodNode(MethodInfo method, bool prefixDeclaringType)
            : base(prefixDeclaringType)
        {
            this.Method = method;

            UpdateText(false);
            int iconIdx = (int)method.GetIcon();
            this.ImageIndex = iconIdx;
            this.SelectedImageIndex = iconIdx;
        }

        public override void Populate(string filterstring)
        {
        }

        internal static string GetMethodName(MethodInfo m, bool prefixDeclaredType)
        {
            string sig = "";


            sig += m.GetName(prefixDeclaredType);

            if (m.ContainsGenericParameters)
            {
                var genericParams = m.GetGenericArguments();
                sig += "<" + string.Join(",", genericParams.Select(t => t.ToSignatureString()).ToArray()) + ">";
            }

            sig += "(" + string.Join(",", m.GetParameters().Select(p =>
            {
                string par = "";
                if (p.IsOut)
                    par += "out ";

                if (p.ParameterType.IsByRef)
                    par += "ref ";

                par += p.ParameterType.ToSignatureString() + " " + p.Name;

                if (p.IsOptional)
                    return "[" + par + "]";

                return par;
            }).ToArray()) + ")";

            sig += " : " + m.ReturnType.ToSignatureString();

            return sig;
        }

        public override void UpdateText(bool recursive)
        {
            base.UpdateText(recursive);
            string str = "";

            str += GetMethodName(Method, prefixDeclaringType);

            if (this.Text != str)
                this.Text = str;

            this.StatusText = GetMethodName(Method, false) +" - Token: " + Method.MetadataToken + " - Alias: " + AliasManager.Instance.GetAlias(Method);

        }

        internal override void OnAliasChanged(object obj, string alias)
        {
            if (obj is Type) // can be any parameter or return type
                UpdateText(false);
            else if (obj is MethodInfo && (MethodInfo)obj == Method)
                UpdateText(false);

            base.OnAliasChanged(obj, alias);
        }

        public override string Visualization
        {
            get
            {
                return GetVisualisation(this, Method);
            }
        }

        public static string RTFHeader = @"{\rtf1\ansi\ansicpg1252\deff0\deflang2067{\fonttbl{\f0\fnil\fcharset0 Tahoma;}}{\colortbl ;\red0\green77\blue187;\red0\green0\blue255;\red163\green21\blue21;\red255\green0\blue0;}\viewkind4\uc1\pard\f0\fs17@BODY@\par}";


        public static string GetVisualisation(AbstractAssemblyNode node, MethodBase m)
        {
            string il;
            try
            {
                Disassembler disAss = new Disassembler(m);
                disAss.BuildInstructions();
                //MethodBodyReader reader = new MethodBodyReader(m);
                il = disAss.ToRTFCode();
            }
            catch (Exception ex)
            {
                il = "Error reading IL: " + ex.GetType().FullName + " - " + ex.Message;
            }

            return RTFHeader.Replace("@BODY@", node.Text + @"\line " + il);
        }


        public override MemberInfo Member
        {
            get { return Method; }
        }
    }
}
