using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;
using System.Reflection;
using RunTimeDebuggers.Helpers;

namespace RunTimeDebuggers.AssemblyExplorer
{
    partial class Bookmarks : TreeNodeControl
    {
        public Bookmarks(IAssemblyBrowser browser)
            : base(browser)
        {
            InitializeComponent();

            Initialize(tvNodes);

            Fill();

            BookmarkManager.Instance.BookmarkAdded += new BookmarkManager.BookmarkHandler(BookmarkManager_BookmarkAdded);
            BookmarkManager.Instance.BookmarkRemoved += new BookmarkManager.BookmarkHandler(BookmarkManager_BookmarkRemoved);
        }


        void BookmarkManager_BookmarkAdded(object bookmark)
        {
            AddNode(bookmark);
        }

        void BookmarkManager_BookmarkRemoved(object obj)
        {
            bool hasNode = false;
            foreach (TreeNode n in tvNodes.Nodes)
            {
                if ((obj is Type && n is TypeNode && ((Type)obj).GUID == ((TypeNode)n).Type.GUID) ||
                    (obj is MemberInfo && n is MemberNode && ((MemberInfo)obj).IsEqual(((MemberNode)n).Member)))
                {
                    n.Remove();
                }
            }

            if (!hasNode)
                AddNode(obj);
        }

        public void Fill()
        {

            tvNodes.BeginUpdate();
            foreach (var bookmark in BookmarkManager.Instance.GetBookMarks())
            {
                AddNode(bookmark);
            }

            tvNodes.EndUpdate();
        }

        private void AddNode(object obj)
        {
            if (obj is Type)
            {
                var tn = new TypeNode((Type)obj);
                tn.Nodes.Clear();
                tvNodes.Nodes.Add(tn);
            }
            else if (obj is MemberInfo)
                tvNodes.Nodes.Add(MemberNode.GetNodeOfMember((MemberInfo)obj, true));
        }

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {

            if (disposing && (components != null))
            {
                components.Dispose();
            }
            if (disposing)
            {
                BookmarkManager.Instance.BookmarkAdded -= new BookmarkManager.BookmarkHandler(BookmarkManager_BookmarkAdded);
                BookmarkManager.Instance.BookmarkRemoved -= new BookmarkManager.BookmarkHandler(BookmarkManager_BookmarkRemoved);
            }
            base.Dispose(disposing);
        }

    }
}
