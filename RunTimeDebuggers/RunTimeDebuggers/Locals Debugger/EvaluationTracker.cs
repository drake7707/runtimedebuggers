using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RunTimeDebuggers.LocalsDebugger
{
    class EvaluationTracker
    {
        public EvaluationTracker()
        {
            EvaluationResults = new List<CSharpExpressionEvaluation.EvaluateResult>();
        }

        public List<CSharpExpressionEvaluation.EvaluateResult> EvaluationResults { get; set; }
    }
}
