using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Diagnostics;

namespace Injector
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            

            TreeNode n1 = new TreeNode("root");

            TreeNode n11 = new TreeNode("child1");
            TreeNode n12 = new TreeNode("child2");
            TreeNode n13 = new TreeNode("child3");

            TreeNode n111 = new TreeNode("childofchild1");
            TreeNode n112 = new TreeNode("childofchild2");
            TreeNode n113 = new TreeNode("childofchild3");

            TreeNode n121 = new TreeNode("child2ofchild1");
            TreeNode n122 = new TreeNode("child2ofchild2");

            n11.Nodes.Add(n111);
            n11.Nodes.Add(n112);
            n11.Nodes.Add(n113);

            n12.Nodes.Add(n121);
            n12.Nodes.Add(n122);

            n1.Nodes.Add(n11);
            n1.Nodes.Add(n12);
            n1.Nodes.Add(n13);

            DoIterative(new ObjWithIndex<TreeNode>(n1, 0),
                              node => Trace.WriteLine(node.Object != null ? new string(' ', node.Index) + node.Object.Text : ""),
                              node => false,
                              node => node.Object.Nodes.Cast<TreeNode>().Select(n => new ObjWithIndex<TreeNode>(n, node.Index + 1)));

            //ILDebugManager.Instance.Debugger = new ILDebugger(typeof(Program).GetMethod("TestMethod",  System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static), null, new object[] { });
            //ILDebugManager.Instance.Run();


            Application.Run(new Form1());
        }
        public static void TestMethod()
        {

            try
            {
                MessageBox.Show("Throwing error");
                throw new ArgumentException("An error");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Exception: " + ex.Message);
            }
            finally
            {
                MessageBox.Show("Finalizing..");
            }
        }


        private struct ObjWithIndex<T>
        {
            public ObjWithIndex(T obj, int idx)
            {
                Object = obj;
                Index = idx;
            }

            public T Object;
            public int Index;
        }


        public static void DoIterative<T>(T n, Action<T> doSomething, Func<T, bool> recursiveBreakCondition, Func<T, IEnumerable<T>> recursiveAction)
        {
            Stack<T> previous = new Stack<T>();
            Stack<IEnumerator<T>> enumerators = new Stack<IEnumerator<T>>();

            T current = n;

            IEnumerator<T> enumerator = recursiveAction(current).GetEnumerator();

            bool hasElements = true;
            do
            {
                while (hasElements && !recursiveBreakCondition(current))
                {
                    doSomething(current);

                    enumerator = recursiveAction(current).GetEnumerator();
                    if (enumerator.MoveNext())
                    {
                        previous.Push(current);
                        enumerators.Push(enumerator);
                        current = enumerator.Current;
                        hasElements = true;
                    }
                    else
                        hasElements = false;
                }

                if (previous.Count > 0)
                {
                    do
                    {
                        var topEnumerator = enumerators.Peek();
                        if (topEnumerator.MoveNext())
                        {
                            current = topEnumerator.Current;
                            hasElements = true;
                        }
                        else
                        {
                            current = previous.Pop();
                            enumerator = enumerators.Pop();

                            hasElements = false;
                        }
                    }
                    while (!hasElements && previous.Count > 0);
                }
            } while (previous.Count > 0);
        }

    }
}
