using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NodeControl;
using System.Drawing;
using NodeControl.Nodes;
using System.Reflection.Emit;

namespace RunTimeDebuggers.AssemblyExplorer
{
    class ILNodeDiagram : NodeDiagram
    {

        public ILNodeDiagram()
        {
            Factories.Clear();
        }

        public void LoadIL(List<ILInstruction> instructions)
        {
            Nodes.Clear();

            StartNode sn = new StartNode(this);
            Nodes.Add(sn);


            Dictionary<int, Node> nodesByILOffset = new Dictionary<int, Node>();

            foreach (var instruction in instructions)
            {
                var iNode = GetNodeFromInstruction(instruction);
                Nodes.Add((Node)iNode);
                nodesByILOffset.Add(instruction.Offset, (Node)iNode);
            }

            var nodesToLink = nodesByILOffset.OrderBy(p => p.Key).Select(p => p.Value).ToArray();
            for (int i = 0; i < nodesToLink.Length; i++)
            {
                var node = nodesToLink[i];

                if (i == 0)
                    sn.LinksTo = node;

                if (node is InstructionNode)
                {
                    if (i + 1 < nodesToLink.Length && ContinuesToNextInstruction(((IInstructionNode)node).Instruction))
                        ((InstructionNode)node).LinksTo = nodesToLink[i + 1];
                }
                else if (node is UnconditionalBranchNode)
                {
                    ((UnconditionalBranchNode)node).LinksTo = nodesByILOffset[(int)((IInstructionNode)node).Instruction.Operand];
                }
                else if (node is BranchNode)
                {
                    if (i + 1 < nodesToLink.Length)
                        ((BranchNode)node).AddContinueLinksTo(nodesToLink[i + 1]);

                    if (((IInstructionNode)node).Instruction.Operand is int)
                    {
                        int offset = (int)((IInstructionNode)node).Instruction.Operand;
                        ((BranchNode)node).AddJumpLinksTo(nodesByILOffset[offset]);
                    }
                    else if (((IInstructionNode)node).Instruction.Operand is int[])
                    {
                        // TODO multibranch
                        //int[] offsets = (int[])((IInstructionNode)node).Instruction.Operand;
                        //foreach (var offset in offsets)
                        //    ((BranchNode)node).AddJumpLinksTo(nodesByILOffset[offset]);
                    }
                }
            }

            AutoLayout(false);
            Redraw();
        }

        private bool ContinuesToNextInstruction(ILInstruction instruction)
        {
            return instruction.Code.FlowControl == FlowControl.Next ||
                   instruction.Code.FlowControl == FlowControl.Call;
        }

        private IInstructionNode GetNodeFromInstruction(ILInstruction instruction)
        {

            Node n;

            if (instruction.Code.FlowControl == FlowControl.Branch)
                n = new UnconditionalBranchNode(this, instruction);
            else if (instruction.Code.FlowControl == FlowControl.Cond_Branch)
                n = new BranchNode(this, instruction);
            else
            {
                n = new InstructionNode(this, instruction);
            }

            n.Direction = Node.DirectionEnum.Vertical;
            return (IInstructionNode)n;
        }


        interface IInstructionNode
        {
            ILInstruction Instruction { get; }
        }

        class InstructionNode : NodeControl.Nodes.TextNode, IInstructionNode
        {
            public ILInstruction Instruction { get; set; }
            public InstructionNode(NodeDiagram parent, ILInstruction instruction)
                : base(parent)
            {
                this.Instruction = instruction;
                string operand = instruction.GetOperandString();
                Text = instruction.Offset.ToString("x4") + Environment.NewLine + instruction.Code.ToString() + (string.IsNullOrEmpty(operand) ? "" : Environment.NewLine + operand);

                using (System.Drawing.Graphics g = parent.CreateGraphics())
                {
                    SizeF s = g.MeasureString(Text, parent.Font);

                    nodeSize = base.NodeSize;

                    if (s.Width > base.NodeSize.Width)
                        nodeSize = new Size((int)s.Width, nodeSize.Height);

                    if (s.Height > base.NodeSize.Height)
                        nodeSize = new Size((int)nodeSize.Width, (int)s.Height);
                }
            }

            private Size nodeSize;
            public override Size NodeSize { get { return nodeSize; } }

            protected override bool OpenEditor()
            {
                return true;
            }
        }

        class StatementNode : NodeControl.Nodes.TextNode
        {
            public Statement Statement { get; set; }
            public StatementNode(NodeDiagram parent, Statement statement)
                : base(parent)
            {
                this.Statement = statement;
                Text = statement.Expression;

                using (System.Drawing.Graphics g = parent.CreateGraphics())
                {
                    SizeF s = g.MeasureString(Text, parent.Font);

                    nodeSize = base.NodeSize;

                    if (s.Width > base.NodeSize.Width)
                        nodeSize = new Size((int)s.Width, nodeSize.Height);

                    if (s.Height > base.NodeSize.Height)
                        nodeSize = new Size((int)nodeSize.Width, (int)s.Height);
                }
            }

            private Size nodeSize;
            public override Size NodeSize { get { return nodeSize; } }

            protected override bool OpenEditor()
            {
                return true;
            }
        }


        public class UnconditionalBranchNode : NodeControl.Nodes.TextNode, IInstructionNode
        {
            public ILInstruction Instruction { get; set; }
            public UnconditionalBranchNode(NodeDiagram parent, ILInstruction instruction)
                : base(parent)
            {
                this.Instruction = instruction;
                string operand = instruction.GetOperandString();
                Text = instruction.Offset.ToString("x4") + Environment.NewLine + instruction.Code.ToString() + (string.IsNullOrEmpty(operand) ? "" : Environment.NewLine + operand);

                using (System.Drawing.Graphics g = parent.CreateGraphics())
                {
                    SizeF s = g.MeasureString(Text, parent.Font);

                    nodeSize = base.NodeSize;

                    if (s.Width > base.NodeSize.Width)
                        nodeSize = new Size((int)s.Width, nodeSize.Height);

                    if (s.Height > base.NodeSize.Height)
                        nodeSize = new Size((int)nodeSize.Width, (int)s.Height);
                }
            }

            private Size nodeSize;
            public override Size NodeSize { get { return nodeSize; } }

            protected override bool OpenEditor()
            {
                return true;
            }
        }

        public class BranchNode : NodeControl.Nodes.ConditionNode, IInstructionNode
        {
            public ILInstruction Instruction { get; set; }
            public BranchNode(NodeDiagram parent, ILInstruction instruction)
                : base(parent)
            {
                this.Instruction = instruction;
                string operand = instruction.GetOperandString();
                Text = instruction.Offset.ToString("x4") + Environment.NewLine + instruction.Code.ToString() + (string.IsNullOrEmpty(operand) ? "" : Environment.NewLine + operand);

                using (System.Drawing.Graphics g = parent.CreateGraphics())
                {
                    SizeF s = g.MeasureString(Text, parent.Font);

                    nodeSize = base.NodeSize;

                    if (s.Width > base.NodeSize.Width)
                        nodeSize = new Size((int)s.Width, nodeSize.Height);

                    if (s.Height > base.NodeSize.Height)
                        nodeSize = new Size((int)nodeSize.Width, (int)s.Height);
                }


                if (instruction.Code == OpCodes.Beq || instruction.Code == OpCodes.Beq_S)
                {
                    jumpExpression = "==";
                    continueExpression = "!=";
                }
                else if (instruction.Code == OpCodes.Bge || instruction.Code == OpCodes.Bge_S ||
                     instruction.Code == OpCodes.Bge_Un || instruction.Code == OpCodes.Bge_Un_S)
                {
                    jumpExpression = ">=";
                    continueExpression = "<";
                }
                else if (instruction.Code == OpCodes.Bgt || instruction.Code == OpCodes.Bgt_S ||
                     instruction.Code == OpCodes.Bgt_Un || instruction.Code == OpCodes.Bgt_Un_S)
                {
                    jumpExpression = ">";
                    continueExpression = "<=";
                }
                else if (instruction.Code == OpCodes.Ble || instruction.Code == OpCodes.Ble_S ||
                     instruction.Code == OpCodes.Ble_Un || instruction.Code == OpCodes.Ble_Un_S)
                {
                    jumpExpression = "<=";
                    continueExpression = ">";
                }
                else if (instruction.Code == OpCodes.Blt || instruction.Code == OpCodes.Blt_S ||
                     instruction.Code == OpCodes.Blt_Un || instruction.Code == OpCodes.Blt_Un_S)
                {
                    jumpExpression = "<";
                    continueExpression = ">=";
                }
                else if (instruction.Code == OpCodes.Bne_Un || instruction.Code == OpCodes.Bne_Un_S)
                {
                    jumpExpression = "!=";
                    continueExpression = "==";
                }
                else if (instruction.Code == OpCodes.Brfalse || instruction.Code == OpCodes.Brfalse_S)
                {
                    jumpExpression = "false";
                    continueExpression = "true";
                }
                else if (instruction.Code == OpCodes.Brtrue || instruction.Code == OpCodes.Brtrue_S)
                {
                    jumpExpression = "true";
                    continueExpression = "false";
                }
                LinksTo.Add(new Condition() { Text = continueExpression });
                LinksTo.Add(new Condition() { Text = jumpExpression });

            }

            private string jumpExpression;
            private string continueExpression;

            public void AddJumpLinksTo(Node n)
            {
                LinksTo[1] = new Condition() { Text = jumpExpression, LinksTo = n };
            }
            public void AddContinueLinksTo(Node n)
            {
                LinksTo[0] = new Condition() { Text = continueExpression, LinksTo = n };
            }

            public Condition ContinueCondition { get { return this.LinksTo[0]; } }
            public Condition JumpCondition { get { return this.LinksTo[1]; } }

            private Size nodeSize;
            public override Size NodeSize { get { return nodeSize; } }

            protected override bool OpenEditor()
            {
                return true;
            }
        }


        private class Statement
        {
            public Statement()
            {
                Nodes = new List<InstructionNode>();
            }
            public string Expression { get; set; }
            public List<InstructionNode> Nodes { get; set; }
        }

        //    public List<Node> SimplifyToStatements(List<IInstructionNode> nodes)
        //    {
        //        List<Node> newNodeList = new List<Node>();

        //        Stack<Statement> statements = new Stack<Statement>();

        //        HashSet<IInstructionNode> nodesVisited = new HashSet<IInstructionNode>();


        //        foreach (var n in nodes)
        //        {
        //            if (!nodesVisited.Contains(n))
        //            {
        //                IInstructionNode currentNode = n;
        //                while (currentNode != null)
        //                {
        //                    nodesVisited.Add(currentNode);

        //                    if (n is InstructionNode)
        //                    {
        //                        Statement currentStatement = null;
        //                        if (n.Instruction.Code == OpCodes.Ldc_I4)
        //                        {
        //                            currentStatement = new Statement() { Expression = n.Instruction.GetOperandString() };
        //                            currentStatement.Nodes.Add((InstructionNode)n);
        //                            statements.Push(currentStatement);
        //                        }
        //                        else if (n.Instruction.Code == OpCodes.Stloc || n.Instruction.Code == OpCodes.Stloc_S)
        //                        {
        //                            var statement = statements.Pop();
        //                            currentStatement = new Statement() { Expression = "local_" + n.Instruction.GetOperandString() + " = " + statement.Expression };
        //                            currentStatement.Nodes.AddRange(statement.Nodes);
        //                            currentStatement.Nodes.Add((InstructionNode)n);
        //                        }

        //                        if (currentStatement != null && statements.Count == 0)
        //                        {
        //                            // statement is complete

        //                        }

        //                        var linksTo = ((InstructionNode)n).LinksTo;
        //                        if (linksTo != null && linksTo is IInstructionNode)
        //                            currentNode = (InstructionNode)linksTo;
        //                        else
        //                            currentNode = null;
        //                    }
        //                    else if (n is BranchNode)
        //                    {

        //                    }
        //                }
        //            }
        //        }



        //        return newNodeList;
        //    }
    }


}
