using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace RunTimeDebuggers.Helpers
{
    public static class Extensions
    {

        public static void AddRange<T>(this HashSet<T> h, IEnumerable<T> objs)
        {
            foreach (var o in objs)
                h.Add(o);
        }

        public static IEnumerable<TreeNode> GetAllNodes(this TreeNode n)
        {
            foreach (TreeNode subn in n.Nodes)
            {
                yield return subn;
                foreach (TreeNode subsubn in GetAllNodes(subn))
                    yield return subsubn;
            }
        }

        public static IEnumerable<TreeNode> GetAllNodes(this TreeView n)
        {
            foreach (TreeNode subn in n.Nodes)
            {
                yield return subn;
                foreach (TreeNode subsubn in GetAllNodes(subn))
                    yield return subsubn;
            }
        }
    }
}
