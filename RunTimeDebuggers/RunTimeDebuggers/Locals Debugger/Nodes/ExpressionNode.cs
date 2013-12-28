using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RunTimeDebuggers.Helpers;

namespace RunTimeDebuggers.LocalsDebugger
{
    class ExpressionNode : MemberNode, ILocalsNode
    {

        public Object ThisObject { get; private set; }
        public string Expression { get; private set; }

        
        public ExpressionNode(object thisObject, string expression)
        {
            this.ThisObject = thisObject;
            this.Expression = expression;

            this[0] = expression;
            Evaluate();
            Changed = false;
        }

        
        public override void Evaluate()
        {
            var oldType = ObjectType;
            var oldObject = Object;

            try
            {
                var result = StatementParser.EvaluateStatement(Expression, ThisObject, true, true, false);
                Object = result.Result;
                ObjectType = result.ResultType;

                if (Object != null)
                    this[1] = Object;
                else
                    this[1] = "null";

                if (ObjectType != null)
                    this[2] = ObjectType.ToSignatureString();
                else
                    this[2] = "";

            }
            catch (Exception ex)
            {
                this[1] = "(ERROR: " + ex.GetType().Name + " - " + ex.Message + ")";
            }

            SetChanged(oldObject, oldType);
        }
    }
}
