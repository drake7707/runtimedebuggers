using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Reflection;
using System.Windows.Forms;

namespace RunTimeDebuggers.AssemblyExplorer
{
    interface IAssemblyBrowser
    {
        void OnNodeRightClicked(AbstractAssemblyNode node, Point pos, bool isFromBrowser);

        void SelectMember(MemberInfo member, int offset = -1);

        void SelectType(Type t);

        ImageList GetNodeImageList();

        void AddTab(Control c, string tabName);

        void GoBack();

        void GoForward();

        void SetStatusText(string str);
    }
}
