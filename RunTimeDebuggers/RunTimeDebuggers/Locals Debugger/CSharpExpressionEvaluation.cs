using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using RunTimeDebuggers.Helpers;
using RunTimeDebuggers.AssemblyExplorer;
using System.Windows.Forms;

namespace RunTimeDebuggers.LocalsDebugger
{
    /// <summary>
    /// Evaluates an c# expression
    /// </summary>
    class CSharpExpressionEvaluation
    {
        /// <summary>
        /// The tokens of the expression
        /// </summary>
        public List<Token> Tokens { get; private set; }

        /// <summary>
        /// The current index of the tokens
        /// </summary>
        private int currentTokenIndex;

        /// <summary>
        /// The recognized variables
        /// </summary>
        private Dictionary<string, Variable> variables;

        public struct Variable
        {
            public object Value { get; set; }
            public Type Type { get; set; }
        }

        private Type targetType;

        /// <summary>
        /// Does the value have to be evaluated
        /// </summary>
        private bool evaluateValue;

        /// <summary>
        /// The current value of the expression evaluated to the current token index
        /// </summary>
        public Object CurrentValue { get; private set; }


        /// <summary>
        /// The current type of the value of the expression evaluated to the current token index
        /// </summary>
        public Type CurrentType { get; private set; }

        /// <summary>
        /// The current member evaluated
        /// </summary>
        public MemberInfo CurrentMember { get; private set; }

        /// <summary>
        /// The instance used on the current member to get the current value
        /// </summary>
        public Object CurrentMemberInstance { get; set; }

        /// <summary>
        /// Was the last evaluated member a type, meaning that the fields, properties or methods have to be evaluated in a static manner
        /// </summary>
        private bool isStatic;

        /// <summary>
        /// The generic types of the current type that's been evaluated
        /// </summary>
        private List<Type> genericTypesOfCurrentType = new List<Type>();

        /// <summary>
        /// Is the output of the expression at current token index valid
        /// </summary>
        public bool IsValidOutput { get; private set; }

        /// <summary>
        /// The casts that have to be applied after the expression has been evaluated
        /// </summary>
        private Stack<Type> pendingCasts = new Stack<Type>();

        /// <summary>
        /// The type store containing information about all types
        /// </summary>
        private TypeStore typeStore;

        /// <summary>
        /// Keeps track of all evaluation results
        /// </summary>
        private EvaluationTracker evaluationTracker;

        /// <summary>
        /// The offset to use in the FromToken and ToToken of the results added to the evaluation tracker
        /// </summary>
        private int trackerOffset;

        private bool useDebugger;

        /// <summary>
        /// Creates a new expression to evaluate
        /// </summary>
        /// <param name="expression">The tokens of the expression to evaluate</param>
        /// <param name="variables">The variables that are used in the statement</param>
        /// <param name="typeStore">The type store containing type information</param>
        /// <param name="evaluateValue">True if the value has to be evaluated, otherwise only the current type will be evaluated</param>
        /// <param name="targetType">Optional target type if known, necessary when required to infer value</param>
        public CSharpExpressionEvaluation(List<Token> expression, Dictionary<string, Variable> variables, TypeStore typeStore, bool evaluateValue, EvaluationTracker evaluationTracker, Type targetType = null, bool useDebugger = false)
            : this(expression, variables, typeStore, evaluateValue, evaluationTracker, 0, targetType, useDebugger)
        {
        }

        private CSharpExpressionEvaluation(List<Token> expression, Dictionary<string, Variable> variables, TypeStore typeStore, bool evaluateValue, EvaluationTracker evaluationTracker, int trackerOffset, Type targetType = null, bool useDebugger = false)
        {
            this.Tokens = expression;
            this.variables = variables;
            this.evaluateValue = evaluateValue;
            this.typeStore = typeStore;
            this.targetType = targetType;
            this.evaluationTracker = evaluationTracker;
            this.trackerOffset = trackerOffset;
            this.useDebugger = useDebugger;

            Variable thisVariable;
            if (!variables.TryGetValue("this", out thisVariable))
                throw new ArgumentException("The 'this' variable is not present in the variable list");

            CurrentValue = thisVariable.Value;
            CurrentType = thisVariable.Type;
            CurrentMember = null;
            CurrentMemberInstance = null;
        }

        /// <summary>
        /// Evalutes the expression
        /// </summary>
        public void EvaluateExpression()
        {
            // keep stack of operator and left and right hand statements for deferred execution if the
            // priority of the operator that follows is higher and thus needs to be evaluated first
            Stack<Token> operatorStack = new Stack<Token>();
            Stack<EvaluateResult> statementStack = new Stack<EvaluateResult>();
            Stack<object> valueStack = new Stack<object>();
            Stack<Type> typeStack = new Stack<Type>();

            while (currentTokenIndex < Tokens.Count)
            {
                string current = Tokens[currentTokenIndex].Text;

                switch (Tokens[currentTokenIndex].Type)
                {
                    case TokenType.Operator:
                        if (operatorStack.Count > 0)
                        {
                            if (operatorStack.Peek().Priority < Tokens[currentTokenIndex].Priority)
                            {
                                // eg + < *
                                // priority of current operation is higher, don't execute yet
                            }
                            else
                            {
                                // eg * >= + or + >= +
                                // execute previous statement
                                EvaluateExpressionStack(operatorStack, statementStack);
                            }
                        }
                        operatorStack.Push(Tokens[currentTokenIndex]);
                        currentTokenIndex++;
                        break;

                    default:

                        int curIdx = currentTokenIndex;
                        var splitResult = SplitIntoParameters(Tokens, ref curIdx, (tokens, idx) => tokens[idx].Text == "=>", false);
                        if (splitResult.Count == 2)
                        {
                            // lambda expression
                            EvaluateLambda(splitResult[0], splitResult[1]);
                            currentTokenIndex = curIdx;
                        }
                        else
                        {
                            // it's not an operator, so it's a statement between 2 operators, evaluate the statement and push the 
                            // result in the stack
                            int indexBeforeStatement = currentTokenIndex;
                            EvaluateStatement();
                            ApplyPendingCasts();

                            var result = new EvaluateResult()
                            {
                                FromToken = trackerOffset + indexBeforeStatement,
                                ToToken = trackerOffset + currentTokenIndex - 1,
                                Member = CurrentMember,
                                MemberInstance = CurrentMemberInstance,
                                Result = evaluateValue ? CurrentValue : CurrentType.GetDefault(),
                                ResultType = CurrentType
                            };
                            evaluationTracker.EvaluationResults.Add(result);
                            statementStack.Push(result);
                        }
                        break;
                }
            }
            // while there are still pending operators, pop & evaluate
            while (operatorStack.Count > 0)
                EvaluateExpressionStack(operatorStack, statementStack);
        }

        private void EvaluateLambda(List<Token> parameters, List<Token> statement)
        {
            List<Type> lambdaParameterTypes = new List<Type>();
            if (targetType != null && targetType.ContainsGenericParameters)
                lambdaParameterTypes = targetType.GetMethod("Invoke").GetParameters().Select(p => p.ParameterType).ToList();
            else
                lambdaParameterTypes = parameters.Select(o => typeof(object)).ToList();



            List<ParameterExpression> parameterExpressions = new List<ParameterExpression>();

            var lambdaEvaluator = new Func<object[], object>(objects =>
                {
                    Dictionary<string, Variable> lambdaParameters = new Dictionary<string, Variable>(variables);
                    for (int i = 0; i < parameters.Count; i++)
                        lambdaParameters.Add(parameters[i].Text, new Variable() { Value = objects[i], Type = lambdaParameterTypes[i] });


                    CSharpExpressionEvaluation eval = new CSharpExpressionEvaluation(statement, lambdaParameters, typeStore, true, evaluationTracker, useDebugger: useDebugger);
                    eval.EvaluateExpression();
                    eval.ApplyPendingCasts();

                    return eval.CurrentValue;
                });

            for (int i = 0; i < parameters.Count; i++)
            {
                parameterExpressions.Add(Expression.Parameter(lambdaParameterTypes[i], parameters[i].Text));
            }

            var objectsExpression = Expression.NewArrayInit(typeof(object), parameterExpressions.ToArray());

            var lambdaEvalExpr = Expression.Call(Expression.Constant(lambdaEvaluator), "Invoke", null, objectsExpression);

            LambdaExpression lambdaExpr;
            if (targetType == null)
                lambdaExpr = Expression.Lambda(lambdaEvalExpr, parameterExpressions.ToArray());
            else
                lambdaExpr = Expression.Lambda(targetType, lambdaEvalExpr, parameterExpressions.ToArray());

            Expression<Func<string, bool, bool>> meh = (s, b) => (bool)lambdaEvaluator(new object[] { s, b });

            CurrentValue = lambdaExpr.Compile();
            CurrentType = CurrentValue.GetType();
            IsValidOutput = true;
        }



        /// <summary>
        /// Evaluates the top operator from the operator stack, with the values of the statements
        /// </summary>
        /// <param name="operatorStack">The stack of operators to execute</param>
        /// <param name="statementStack">The evaluated statements</param>
        private void EvaluateExpressionStack(Stack<Token> operatorStack, Stack<EvaluateResult> statementStack)
        {
            // pop operator
            var op = operatorStack.Pop();

            // pop the right statement
            EvaluateResult right = statementStack.Pop();

            // if the operator requires a left statement too, pop that one as well
            EvaluateResult left;

            if (op.GetRequiredStatementCount() > 1)
                left = statementStack.Pop();
            else
                left = null;

            // evaluate the operator
            EvaluateExpression(left, right, op);

            // push the result back in the stack

            var result = new EvaluateResult()
            {
                FromToken = trackerOffset + (left != null ? Math.Min(left.FromToken, right.FromToken) : right.FromToken),
                ToToken = trackerOffset + (left != null ? Math.Max(left.ToToken, right.ToToken) : right.ToToken),
                Member = CurrentMember,
                MemberInstance = CurrentMemberInstance,
                Result = evaluateValue ? CurrentValue : CurrentType.GetDefault(),
                ResultType = CurrentType
            };
            evaluationTracker.EvaluationResults.Add(result);
            statementStack.Push(result);
        }

        /// <summary>
        /// Evaluates an operator and left or left and right statements
        /// </summary>
        /// <param name="left">The left statement</param>
        /// <param name="right">The right statement</param>
        /// <param name="op">The operator to evaluate</param>
        private void EvaluateExpression(EvaluateResult left, EvaluateResult right, Token op)
        {
            if (op.Text == "+" && op.Priority == TokenPriority.PlusMinus)
            {
                // special case, if any of the statements is a string, use string.Concat!
                if (typeof(string).IsAssignableFrom(left.ResultType) || typeof(string).IsAssignableFrom(right.ResultType))
                {
                    CurrentType = typeof(string);
                    CurrentMember = null;
                    CurrentMemberInstance = null;
                    if (evaluateValue)
                    {
                        // leftValue + rightValue, both as strings
                        CurrentValue = left.Result + "" + right.Result;
                        IsValidOutput = true;
                    }
                    return;
                }
            }
            else if (op.Text == "=" && op.Priority == TokenPriority.Assign)
            {
                // assign to left instead
                if (left.Member is FieldInfo)
                    ((FieldInfo)left.Member).SetValue(left.MemberInstance, right.Result);
                else if (left.Member is PropertyInfo)
                    ((PropertyInfo)left.Member).SetValue(left.MemberInstance, right.Result, null);
                else
                    throw new InvalidOperationException("Unable to assign the right statement, the left statement is not a field or property");

                CurrentType = right.ResultType;
                CurrentValue = right.Result;
                CurrentMember = null;
                CurrentMemberInstance = null;

                return;
            }

            Expression exprOperator = null;

            // the right expression made from the right statement value & type
            Expression rightExpr = Expression.Constant(right.Result, right.ResultType);

            // the left expression made from the left statement if present
            Expression leftExpr;
            if (left != null)
            {
                leftExpr = Expression.Constant(left.Result, left.ResultType);
                ConvertToMostPrecisionIfRequired(ref leftExpr, ref rightExpr);
            }
            else
                leftExpr = null;

            // translate the operator to the correct expression
            if (op.Text == "+" && op.Priority == TokenPriority.PlusMinus)
                exprOperator = Expression.Add(leftExpr, rightExpr);
            else if (op.Text == "+" && op.Priority == TokenPriority.UnaryMinus)
                exprOperator = Expression.UnaryPlus(rightExpr);
            else if (op.Text == "-" && op.Priority == TokenPriority.PlusMinus)
                exprOperator = Expression.Subtract(leftExpr, rightExpr);
            else if (op.Text == "-" && op.Priority == TokenPriority.UnaryMinus)
                exprOperator = Expression.Negate(rightExpr);
            else if (op.Text == "*")
                exprOperator = Expression.Multiply(leftExpr, rightExpr);
            else if (op.Text == "/")
                exprOperator = Expression.Divide(leftExpr, rightExpr);
            else if (op.Text == "%")
                exprOperator = Expression.Modulo(leftExpr, rightExpr);

            else if (op.Text == ">")
                exprOperator = Expression.GreaterThan(leftExpr, rightExpr);
            else if (op.Text == "<")
                exprOperator = Expression.LessThan(leftExpr, rightExpr);
            else if (op.Text == ">=")
                exprOperator = Expression.GreaterThanOrEqual(leftExpr, rightExpr);
            else if (op.Text == "<=")
                exprOperator = Expression.LessThanOrEqual(leftExpr, rightExpr);
            else if (op.Text == "==")
                exprOperator = Expression.Equal(leftExpr, rightExpr);
            else if (op.Text == "!=")
                exprOperator = Expression.NotEqual(leftExpr, rightExpr);

            else if (op.Text == "&&")
                exprOperator = Expression.AndAlso(leftExpr, rightExpr);
            else if (op.Text == "&")
                exprOperator = Expression.And(leftExpr, rightExpr);
            else if (op.Text == "!")
                exprOperator = Expression.Not(rightExpr);
            else if (op.Text == "||")
                exprOperator = Expression.OrElse(leftExpr, rightExpr);
            else if (op.Text == "|")
                exprOperator = Expression.Or(leftExpr, rightExpr);
            else if (op.Text == "^")
                exprOperator = Expression.Power(leftExpr, rightExpr);

            else if (op.Text == ">>")
                exprOperator = Expression.RightShift(leftExpr, rightExpr);
            else if (op.Text == "<<")
                exprOperator = Expression.LeftShift(leftExpr, rightExpr);

            if (exprOperator == null)
                throw new ArgumentException("Unrecognized operator " + op.Text);

            // compile & execute the expression (if the value needs to be evaluated)
            var expr = Expression.Lambda(exprOperator).Compile();

            CurrentType = expr.Method.ReturnType;
            CurrentMember = expr.Method;
            CurrentMemberInstance = null;
            if (evaluateValue)
            {
                CurrentValue = expr.DynamicInvoke();
                IsValidOutput = true;
            }
        }

        /// <summary>
        /// Generalizes 2 expressions to the same common compatible type
        /// e.g int, double becomes double,double
        /// </summary>
        /// <param name="left">The left expression</param>
        /// <param name="right">The right expression</param>
        private static void ConvertToMostPrecisionIfRequired(ref Expression left, ref Expression right)
        {
            var leftTypeCode = Type.GetTypeCode(left.Type);
            var rightTypeCode = Type.GetTypeCode(right.Type);

            if (leftTypeCode == rightTypeCode)
                return;

            if (leftTypeCode > rightTypeCode)
                right = Expression.Convert(right, left.Type);
            else
                left = Expression.Convert(left, right.Type);
        }

        /// <summary>
        /// Evaluates a statement. Casts are only applied to inner statements, outer casts
        /// of the statement is stored in the pending casts
        /// </summary>
        private void EvaluateStatement()
        {
            // first check if there are any casts
            bool stopCheckForCast = false;
            while (currentTokenIndex < Tokens.Count && !stopCheckForCast)
            {
                switch (Tokens[currentTokenIndex].Type)
                {
                    case TokenType.OpenParens:
                        var allStatementBetweenParens = SplitIntoParameters(Tokens, ref currentTokenIndex,
                                                           (t, i) => false, true);

                        var statementBetweenParens = allStatementBetweenParens[0];

                        // if what follows is an identifier, another open ( or primitive, it's a cast
                        if (currentTokenIndex < Tokens.Count &&
                            (Tokens[currentTokenIndex].Type == TokenType.Identifier ||
                             Tokens[currentTokenIndex].Type == TokenType.OpenParens ||
                             Tokens[currentTokenIndex].Type == TokenType.Primitive))
                        {
                            // format:  (.....)identifier
                            //          (.....)(statement)
                            //          (.....)double
                            // this is a cast, .. right?
                            var resolveTypeResult = ResolveType(statementBetweenParens, typeStore);
                            if (resolveTypeResult.ResolvedType != null)
                            {
                                // push cast into pending casts
                                pendingCasts.Push(resolveTypeResult.ResolvedType);
                            }
                            else
                                throw new ArgumentException("Unable to resolve type " + string.Join("", statementBetweenParens.Select(tn => tn.Text).ToArray()));
                        }
                        else
                        {
                            var tokenIndexOfStatement = Tokens.IndexOf(statementBetweenParens.First());
                            // expression wrapped in ( ), evaluate it without ( )
                            CSharpExpressionEvaluation eval = new CSharpExpressionEvaluation(statementBetweenParens, variables, typeStore, evaluateValue, evaluationTracker, tokenIndexOfStatement, useDebugger: useDebugger);
                            try
                            {
                                eval.EvaluateExpression();
                                // apply any casts of the inner expression
                                eval.ApplyPendingCasts();
                            }
                            finally
                            {
                                // even if it fails, copy the results because intellisense can use that
                                CurrentType = eval.CurrentType;
                                CurrentValue = eval.CurrentValue;
                                CurrentMember = eval.CurrentMember;
                                CurrentMemberInstance = eval.CurrentMemberInstance;
                                IsValidOutput = eval.IsValidOutput;
                            }
                        }
                        break;

                    default:
                        stopCheckForCast = true;
                        break;
                }
            }

            // now that ( ) are out of the way, evaluate the statement
            bool endOfStatement = false;
            while (currentTokenIndex < Tokens.Count && !endOfStatement)
            {
                string current = Tokens[currentTokenIndex].Text;

                // invoke the method that corresponds with the current thing to evaluate
                switch (Tokens[currentTokenIndex].Type)
                {
                    case TokenType.Identifier:
                        if (variables.ContainsKey(Tokens[currentTokenIndex].Text))
                            EvaluateVariable();
                        else if (Tokens[currentTokenIndex].Text == "new")
                            EvaluateNew();
                        else if (Tokens[currentTokenIndex].Text == "typeof")
                            EvaluateTypeOf();
                        else if (Tokens[currentTokenIndex].Text == "default")
                            EvaluateDefault();
                        else if (CurrentType != null && CurrentType.IsField(Tokens[currentTokenIndex].Text))
                            EvaluateField();
                        else if (CurrentType != null && CurrentType.IsProperty(Tokens[currentTokenIndex].Text) && (currentTokenIndex + 1 >= Tokens.Count || !IsStartOfArguments(Tokens, currentTokenIndex + 1)))
                            EvaluateProperty();
                        else if (CurrentType != null && CurrentType.IsMethod(typeStore, Tokens[currentTokenIndex].Text))
                            EvaluateMethod();
                        else
                            EvaluateStaticType();
                        break;

                    case TokenType.Dot:
                        // skip dots
                        currentTokenIndex++;
                        IsValidOutput = false;
                        break;

                    case TokenType.Primitive:
                        EvaluatePrimitive();
                        break;

                    case TokenType.OpenParens:
                        throw new ArgumentException("Token '" + current + "' not expected now");

                    case TokenType.CloseParens:
                        throw new ArgumentException("Token '" + current + "' not expected now");

                    case TokenType.OpenBracket:
                        // it's an open bracket, it's an indexer of a property, evaluate the default indexer
                        EvaluateIndexer();
                        break;

                    case TokenType.Operator:
                        // it's an operator, means the end of the statement
                        endOfStatement = true;
                        break;

                    default:
                        throw new ArgumentException("Identifier not recognized " + current);
                }
            }

            if (evaluateValue)
            {
                if (!IsValidOutput)
                    throw new ArgumentException("Expected statement");
            }
        }

        /// <summary>
        /// Applies the pending casts to the evaluated statement
        /// </summary>
        public void ApplyPendingCasts()
        {
            while (pendingCasts.Count > 0)
            {
                Type pendingCast = pendingCasts.Pop();

                CurrentType = pendingCast;
                CurrentValue = Cast(CurrentType, CurrentValue);

            }
        }

        /// <summary>
        /// Casts the given object to the given type
        /// </summary>
        /// <param name="t">The type to cast the object to</param>
        /// <param name="o">The object to be casted</param>
        /// <returns>The casted object</returns>
        public object Cast(Type t, object o)
        {
            try
            {
                return this.GetType().GetMethod("CastGeneric", BindingFlags.Instance | BindingFlags.NonPublic)
                                         .MakeGenericMethod(t).Invoke(this, new object[] { o });
            }
            catch (Exception ex)
            {
                // throw actual exception
                throw ex.InnerException;
            }
        }

        /// <summary>
        /// Casts the given object to the given type
        /// </summary>
        /// <typeparam name="T">The type to cast the object to</typeparam>
        /// <param name="o">The object to be casted</param>
        /// <returns>The casted object</returns>
        private T CastGeneric<T>(object o)
        {
            return (T)o;
        }

        /// <summary>
        /// Evaluates a variable
        /// </summary>
        private void EvaluateVariable()
        {
            Variable var;
            if (variables.TryGetValue(Tokens[currentTokenIndex].Text, out var))
                // use thisObject given in the constructor
                if (evaluateValue)
                    CurrentValue = var.Value;
            CurrentType = var.Type;
            CurrentMember = null;
            CurrentMemberInstance = null;

            IsValidOutput = true;
            currentTokenIndex++;
        }

        /// <summary>
        /// Evaluate the new operator
        /// </summary>
        private void EvaluateNew()
        {
            currentTokenIndex++;

            List<Token> typeTokens = new List<Token>();

            // read all tokens until the start of the constructor method arguments
            // those are the tokens of the type to instantiate
            while (!IsStartOfArguments(Tokens, currentTokenIndex))
            {
                typeTokens.Add(Tokens[currentTokenIndex]);
                currentTokenIndex++;
            }

            // resolve the type to instantiate
            TypeResolveResult resolvedTypeResult = ResolveType(typeTokens, typeStore);
            if (resolvedTypeResult != null)
            {
                Type typeToConstruct = resolvedTypeResult.ResolvedType;
                // look up all constructors of the resolved type
                var constructors = typeToConstruct.GetConstructorsOfType(false);

                // also take all generic arguments of the type
                genericTypesOfCurrentType = resolvedTypeResult.GenericTypes;

                if (constructors.Count() > 0)
                {
                    // there is a constructor
                    if (IsStartOfArguments(Tokens, currentTokenIndex))
                    {
                        // resolve the arguments of the constructor
                        List<EvaluateResult> argumentValues = ResolveArguments();

                        // take the type of the arguments
                        var argumentTypes = argumentValues.Select(a => a.ResultType).ToList();

                        // filter the constructor based on the argument types
                        constructors = constructors.Where(m =>
                        {
                            var parameters = m.GetParameters();
                            bool check = parameters.Length == argumentValues.Count;
                            if (!check)
                                return false;

                            return MatchesParameters(genericTypesOfCurrentType, null, argumentTypes, parameters);
                        }).ToList();


                        if (constructors.Count() > 1)
                            throw new ArgumentException("Multiple matches found for constructor of " + typeToConstruct.Name);
                        else if (constructors.Count() == 1)
                        {
                            // there is exactly 1 constructor that matches

                            var constructor = constructors.FirstOrDefault();

                            try
                            {
                                // invoke the constructor if we need to evaluate
                                if (evaluateValue)
                                {
                                    CurrentValue = constructor.Invoke(argumentValues.Select(a => a.Result).ToArray());
                                    CurrentType = CurrentValue == null ? typeToConstruct : CurrentValue.GetType();
                                }
                                else // the type of the value returned will be the type to construct
                                    CurrentType = typeToConstruct;

                                CurrentMember = constructor;
                                CurrentMemberInstance = null;

                                IsValidOutput = true;
                            }
                            catch (TargetInvocationException ex) // unwrap
                            {
                                throw ex.InnerException;
                            }

                            currentTokenIndex++;
                        }
                        else
                            throw new ArgumentException("No overload for constructor of " + typeToConstruct.Name + " has " + argumentValues.Count + " parameters");
                    }
                    else
                        throw new ArgumentException("Expected arguments for constructor");
                }
                else
                    throw new ArgumentException("No suitable constructor found for " + typeToConstruct.Name);
            }
            else
                throw new ArgumentException("Unable to resolve type " + string.Join("", typeTokens.Select(tn => tn.Text).ToArray()));
        }

        /// <summary>
        /// Evaluate the typeof operator
        /// </summary>
        private void EvaluateTypeOf()
        {
            currentTokenIndex++;

            // get the tokens between the ( ) for the type
            List<List<Token>> typeTokens = SplitIntoParameters(Tokens, ref currentTokenIndex,
                (t, i) => t[i].Type == TokenType.Comma, true);

            if (typeTokens.Count > 0)
            {
                // resolve the type
                var resolveResult = ResolveType(typeTokens[0], typeStore);
                if (resolveResult.ResolvedType != null)
                {
                    // if need to evaluate, the value will be the type specified
                    if (evaluateValue)
                        CurrentValue = resolveResult.ResolvedType;

                    // typeof always returns a value of type Type
                    CurrentType = typeof(Type);
                    CurrentMember = null;
                    CurrentMemberInstance = null;
                    IsValidOutput = true;
                }
                else
                    throw new ArgumentException("Unable to resolve type " + string.Join("", typeTokens[0].Select(tn => tn.Text).ToArray()));
            }
            else
                throw new ArgumentException("Expected type to evaluate for typeof");
        }

        /// <summary>
        /// Evaluates the default operator
        /// </summary>
        private void EvaluateDefault()
        {
            currentTokenIndex++;
            // get the tokens between the ( ) for the type
            List<List<Token>> typeTokens = SplitIntoParameters(Tokens, ref currentTokenIndex,
                (t, i) => t[i].Type == TokenType.Comma, true);

            if (typeTokens.Count > 0)
            {
                // resolve the type
                var resolveResult = ResolveType(typeTokens[0], typeStore);
                if (resolveResult.ResolvedType != null)
                {
                    // if need to evaluate, the value will be the default value of the type specified
                    if (evaluateValue)
                        CurrentValue = resolveResult.ResolvedType.GetDefault();

                    // the value of the default will always be the type itself
                    CurrentType = resolveResult.ResolvedType;
                    CurrentMember = null;
                    CurrentMemberInstance = null;
                    IsValidOutput = true;
                }
                else
                    throw new ArgumentException("Unable to resolve type " + string.Join("", typeTokens[0].Select(tn => tn.Text).ToArray()));
            }
            else
                throw new ArgumentException("Expected type to evaluate for default");
        }



        /// <summary>
        /// Evaluates a field
        /// </summary>
        private void EvaluateField()
        {
            string current = Tokens[currentTokenIndex].Text;

            // evaluate field
            var fields = CurrentType.GetFieldsOfType(true, true).Where(f => f.Name == current || AliasManager.Instance.GetAlias(f) == current).ToList();
            if (fields.Count > 1)
                throw new ArgumentException("Multiple matches found for field " + current);
            else if (fields.Count == 1)
            {
                var field = fields.First();
                try
                {
                    // if need to evaluate, invoke the field
                    if (evaluateValue)
                    {
                        if (CurrentValue == null && !isStatic)
                            throw new NullReferenceException("Unable to evaluate " + current + ", object instance is null");

                        CurrentMemberInstance = CurrentValue;
                        CurrentValue = field.GetValue(CurrentValue);
                        CurrentType = CurrentValue == null ? field.FieldType : CurrentValue.GetType();
                    }
                    else
                        CurrentType = field.FieldType;

                    isStatic = false;
                    CurrentMember = field;

                    IsValidOutput = true;
                }
                catch (TargetInvocationException ex) // unwrap
                {
                    throw ex.InnerException;
                }
            }
            else
                throw new ArgumentException("No field found with the name " + current);

            currentTokenIndex++;
        }

        /// <summary>
        /// Evaluate the default indexer of a type
        /// </summary>
        private void EvaluateIndexer()
        {
            // search for the properties that have indexers
            var props = CurrentType.GetPropertiesOfType(true, true)
                                 .Where(f => f.GetIndexParameters().Length > 0);
            currentTokenIndex--; // one before the [

            // evaluate the properties that matches the arguments
            EvaluateProperty("indexer", props);
        }

        /// <summary>
        /// Evaluates a property
        /// </summary>
        private void EvaluateProperty()
        {
            string current = Tokens[currentTokenIndex].Text;

            // get all properties that match the name
            var props = CurrentType.GetPropertiesOfType(true, true)
                                   .Where(f => f.Name == current || AliasManager.Instance.GetAlias(f) == current);

            // evaluate the properties that match the indexer arguments
            EvaluateProperty(current, props);
        }

        /// <summary>
        /// Evaluates a property, taken from the given list of possible properties
        /// </summary>
        /// <param name="current"></param>
        /// <param name="props"></param>
        private void EvaluateProperty(string current, IEnumerable<PropertyInfo> props)
        {
            // check if there is indexer
            List<EvaluateResult> indexerResolveResults = new List<EvaluateResult>();
            if (currentTokenIndex + 1 < Tokens.Count && IsStartOfIndexer(Tokens, currentTokenIndex + 1))
            {
                currentTokenIndex++;

                // resolve the indexer
                indexerResolveResults = ResolveIndexers();

                // get the types of the indexer arguments
                var indexerTypes = indexerResolveResults.Select(iv => iv.ResultType).ToList();

                // filter the properties to those that match the indexer arguments
                props = props.Where(m =>
                {
                    var indexedParameters = m.GetIndexParameters();
                    bool check = indexedParameters.Length == indexerResolveResults.Count;
                    if (!check)
                        return false;

                    return MatchesParameters(genericTypesOfCurrentType, new List<Type>(), indexerTypes, indexedParameters);
                }).ToList();
            }

            //if (props.Count() > 1) --> due to explicit interface implementations of list 'Item' has 2 same indexer properties
            //{
            //    throw new ArgumentException("Multiple matches found " + current);
            //}
            if (props.Count() >= 1)
            {
                var prop = props.ElementAt(0);
                try
                {

                    // if need to evaluate, invoke the property
                    if (evaluateValue)
                    {
                        if (CurrentValue == null && !isStatic)
                            throw new NullReferenceException("Unable to evaluate " + current + ", object instance is null");

                        CurrentMemberInstance = CurrentValue;
                        if (prop.CanRead && prop.GetGetMethod() != null)
                            CurrentValue = prop.GetValue(CurrentValue, indexerResolveResults.Select(i => i.Result).ToArray());
                        else
                        {
                            // don't attempt to read property without get method, but don't throw exception either
                            // it can still be used in assignments
                            CurrentValue = null;
                            CurrentType = prop.PropertyType;
                        }
                        CurrentType = CurrentValue == null ? prop.PropertyType : CurrentValue.GetType();
                    }
                    else
                        CurrentType = prop.PropertyType;

                    isStatic = false;
                    CurrentMember = prop;
                    IsValidOutput = true;
                }
                catch (TargetInvocationException ex) // unwrap
                {
                    throw ex.InnerException;
                }
            }
            else
            {
                throw new ArgumentException("No overload for property " + current + " has " + indexerResolveResults.Count + " parameters with given signature");
            }

            // there wasn't an indexer, the current token is not on the next token yet
            if (indexerResolveResults.Count == 0)
                currentTokenIndex++;
        }

        /// <summary>
        /// Evaluate a method
        /// </summary>
        private void EvaluateMethod()
        {
            string current = Tokens[currentTokenIndex].Text;

            // get all methods & extension methods of the current type that match the name
            var methods = CurrentType.GetMethodsOfType(true, true).Concat(typeStore.GetExtensionMethodsOf(CurrentType))
                                              .Where(f => f.Name == current || AliasManager.Instance.GetAlias(f) == current)
                                              .ToList();

            // read generic type tokens

            List<Type> genericTypesOfMethod;
            if (currentTokenIndex + 1 < Tokens.Count && IsStartOfGenericTypes(Tokens, currentTokenIndex + 1))
            {
                // has generic parameters;
                currentTokenIndex++;
                genericTypesOfMethod = ResolveGenericTypes(Tokens, ref currentTokenIndex, typeStore);

            }
            else
                genericTypesOfMethod = new List<Type>();

            // filter available methods based on given generic types
            if (genericTypesOfMethod.Count > 0)
            {
                methods = methods.Where(m =>
                {
                    var genericArguments = m.GetGenericArguments();
                    return m.ContainsGenericParameters && genericArguments.Length == genericTypesOfMethod.Count;
                }).ToList();
            }


            if (methods.Count() > 0)
            {
                // if there was no generic type definition, go on to the ( token
                if (genericTypesOfMethod.Count == 0)
                    currentTokenIndex++;

                if (IsStartOfArguments(Tokens, currentTokenIndex))
                {
                    // resolve the arguments
                    List<EvaluateResult> argumentValues = ResolveArguments();

                    // filter methods based on parameters
                    methods = methods.Where(m =>
                    {
                        // determine actual generic types of the method. There could be generic parameters that
                        // need to be inferred
                        List<Type> actualGenericTypesOfMethod;
                        if (genericTypesOfMethod.Count == 0 && m.ContainsGenericParameters)
                            actualGenericTypesOfMethod = m.GetGenericArguments().ToList();
                        else
                            actualGenericTypesOfMethod = genericTypesOfMethod;

                        var parameters = m.GetParameters();

                        // get argument types
                        var argumentTypes = argumentValues.Select(a => a.ResultType).ToList();

                        // if the method is a static extension method, the first parameter is the current type
                        if (argumentTypes.Count == parameters.Length - 1)
                        {
                            if (m.IsDefined(typeof(ExtensionAttribute), false))
                                argumentTypes.Insert(0, CurrentType);
                        }

                        bool check = parameters.Length == argumentTypes.Count;
                        if (!check)
                            return false;

                        return MatchesParameters(genericTypesOfCurrentType, actualGenericTypesOfMethod, argumentTypes, parameters);
                    }).ToList();

                    if (methods.Count() > 1)
                    {
                        // take most precise method
                        // eg Console.WriteLine(object) and Console.WriteLine(string)
                        // TODO
                        throw new ArgumentException("Multiple matches found " + current);
                    }
                    else if (methods.Count() == 1)
                    {
                        var method = methods.FirstOrDefault();
                        // check if method is extension method
                        var isExtensionMethod = method.IsDefined(typeof(ExtensionAttribute), false);

                        // make method generic if it has generic types defined
                        // from the type list or from the inferred types
                        if (genericTypesOfMethod.Count > 0)
                            method = method.MakeGenericMethod(genericTypesOfMethod.ToArray());
                        else
                        {
                            if (method.ContainsGenericParameters)
                            {
                                var argumentTypes = argumentValues.Select(a => a.ResultType).ToList();
                                if (isExtensionMethod && argumentValues.Count == method.GetParameters().Length - 1)
                                    argumentTypes.Insert(0, CurrentType);

                                // types were inferred, determine the generic types from the arguments
                                Type[] genericTypesToDetermine = GetGenericTypesFromDefinitionAndParameters(method, argumentTypes);
                                method = method.MakeGenericMethod(genericTypesToDetermine);
                            }
                        }

                        try
                        {
                            // if need to evaluate, invoke method
                            if (evaluateValue)
                            {
                                if (CurrentValue == null && !isStatic)
                                    throw new NullReferenceException("Unable to evaluate " + current + ", object instance is null");

                                // if the method is an extension method
                                if (isExtensionMethod && argumentValues.Count == method.GetParameters().Length - 1)
                                {
                                    // insert the current value as first argument & invoke as static
                                    List<object> argValues = argumentValues.Select(a => a.Result).ToList();
                                    argValues.Insert(0, CurrentValue);

                                    if (useDebugger)
                                    {
                                        ILDebugManager.Instance.Debugger = new ILDebugger(method, null, argValues.ToArray());
                                        ILDebugManager.Instance.Run();
                                        // wait until debugger is finished, but don't block user input for breakpoints etc
                                        while (!ILDebugManager.Instance.Debugger.Returned)
                                        {
                                            Application.DoEvents();
                                            System.Threading.Thread.Sleep(25);
                                        }

                                        CurrentValue = ILDebugManager.Instance.Debugger.ReturnValue;
                                    }
                                    else
                                    {
                                        CurrentMemberInstance = null;
                                        CurrentValue = method.Invoke(null, argValues.ToArray());
                                    }
                                }
                                else
                                {
                                    if (useDebugger)
                                    {
                                        ILDebugManager.Instance.Debugger = new ILDebugger(method, CurrentValue, argumentValues.Select(a => a.Result).ToArray());
                                        ILDebugManager.Instance.Run();
                                        // wait until debugger is finished, but don't block user input for breakpoints etc
                                        while (!ILDebugManager.Instance.Debugger.Returned)
                                        {
                                            Application.DoEvents();
                                            System.Threading.Thread.Sleep(25);
                                        }

                                        CurrentValue = ILDebugManager.Instance.Debugger.ReturnValue;
                                    }
                                    else
                                    {
                                        CurrentMemberInstance = CurrentValue;
                                        CurrentValue = method.Invoke(CurrentValue, argumentValues.Select(a => a.Result).ToArray());
                                    }
                                }
                            }
                            isStatic = false;
                            // type is always return type of method
                            CurrentType = method.ReturnType;
                            CurrentMember = method;
                            IsValidOutput = true;
                        }
                        catch (TargetInvocationException ex) // unwrap
                        {
                            throw ex.InnerException;
                        }

                        currentTokenIndex++;
                    }
                    else
                        throw new ArgumentException("No overload for method " + current + " has " + argumentValues.Count + " parameters");
                }
                else
                    throw new ArgumentException("Expected arguments for method");
            }
            else
                throw new ArgumentException("No suitable method found for " + current);
        }

        /// <summary>
        /// Gets an array of types that match the generic type definition of the method
        /// </summary>
        /// <param name="method">The generic method</param>
        /// <param name="argumentTypes">The types of the arguments specified</param>
        /// <returns>An array of types that match the generic type definition</returns>
        private Type[] GetGenericTypesFromDefinitionAndParameters(MethodInfo method, List<Type> argumentTypes)
        {
            Type[] genericTypes = new Type[method.GetGenericArguments().Length];

            // determine generic parameter from argument
            var parameters = method.GetParameters();
            for (int i = 0; i < parameters.Length; i++)
            {
                Type pType = parameters[i].ParameterType;
                Type actualType = argumentTypes[i];

                // resolve the generic type by inspecting the parameter type and it's corresponding type from the argument
                ResolveGenericType(genericTypes, pType, actualType);
            }
            return genericTypes;
        }

        /// <summary>
        /// Resolve the generic type by inspecting the given parameter type and it's corresponding type from the argument and
        /// store it in the correct position in the array
        /// </summary>
        /// <param name="genericTypes">The generic type array for the method</param>
        /// <param name="pType">The parameter type</param>
        /// <param name="actualType">The actual type</param>
        private static void ResolveGenericType(Type[] genericTypes, Type pType, Type actualType)
        {
            // if the parameter type itself is the generic parameter, store it in the array
            if (pType.IsGenericParameter)
                genericTypes[pType.GenericParameterPosition] = actualType;
            else
            {
                // if the parameter type has generic parameters (e.g IEnumerable<T>)
                if (pType.ContainsGenericParameters)
                {
                    // inspect all the generic type of the parameter type itself (recursively)
                    var genericArgumentsOfPType = pType.GetGenericArguments();
                    var genericArgumentsOfActualType = actualType.GetGenericArguments();
                    for (int j = 0; j < genericArgumentsOfPType.Length; j++)
                        ResolveGenericType(genericTypes, genericArgumentsOfPType[j], genericArgumentsOfActualType[j]);
                }
            }
        }

        /// <summary>
        /// Evaluate the current token as a static class
        /// </summary>
        private void EvaluateStaticType()
        {
            // not a method, field or property
            //part of namespace?

            string current = Tokens[currentTokenIndex].Text;


            Type resolvedType = null;
            string fullTypeName = "";

            object aliasObj = AliasManager.Instance.GetObjectFromAlias(current);
            if (aliasObj is Type)
                resolvedType = (Type)aliasObj;

            else
            {
                if (isStatic && CurrentType != null)
                {
                    // the type was resolved (maybe because it had the same name as an assembly)
                    // but no property,field or method was found that matches it

                    // continue looking for type with previous part
                    fullTypeName = CurrentType.Name + ".";
                }

                // while it does not match a type and the tokens read are a chain of identifiers and dots
                while (resolvedType == null && currentTokenIndex < Tokens.Count &&
                      (Tokens[currentTokenIndex].Type == TokenType.Dot ||
                       Tokens[currentTokenIndex].Type == TokenType.Identifier))
                {
                    fullTypeName += Tokens[currentTokenIndex].Text;

                    if (Tokens[currentTokenIndex].Type == TokenType.Identifier)
                    {
                        // does the type have generic parameters
                        if (currentTokenIndex + 1 < Tokens.Count && IsStartOfGenericTypes(Tokens, currentTokenIndex + 1))
                        {
                            // has generic parameters;
                            currentTokenIndex++;

                            // resolve the generic types
                            var genericTypes = ResolveGenericTypes(Tokens, ref currentTokenIndex, typeStore);

                            // find all matching types based on name and resolved generic types
                            List<Type> possibleTypes = typeStore.FindMatchingTypes(fullTypeName, genericTypes);

                            if (possibleTypes.Count > 1)
                                throw new ArgumentException("Ambigious match for type '" + fullTypeName + "'");
                            //else if (possibleTypes.Count == 0)
                            //    throw new ArgumentException("Unable to resolve type " + fullTypeName);

                            if (possibleTypes.Count == 1)
                            {
                                resolvedType = possibleTypes[0];

                                // make type generic if there were generic types defined
                                if (genericTypes != null)
                                    resolvedType = resolvedType.MakeGenericType(genericTypes.ToArray());

                                genericTypesOfCurrentType = genericTypes;
                            }
                            else
                                currentTokenIndex++;
                        }
                        else
                        {
                            // find matching types based on name
                            List<Type> possibleTypes = typeStore.FindMatchingTypes(fullTypeName, null);

                            if (possibleTypes.Count > 1)
                                throw new ArgumentException("Ambigious match for type '" + fullTypeName + "'");
                            //else if (possibleTypes.Count == 0)
                            //    throw new ArgumentException("Unable to resolve type " + fullTypeName);

                            if (possibleTypes.Count == 1)
                                resolvedType = possibleTypes[0];

                            currentTokenIndex++;

                        }
                    }
                    else
                        currentTokenIndex++;

                }
            }

            isStatic = true;

            // type was resolved
            if (resolvedType != null)
            {
                // if need to evaluate, the current value of a static type is always null
                if (evaluateValue)
                {
                    CurrentValue = null;
                }

                CurrentType = resolvedType;
                CurrentMember = null;
                CurrentMemberInstance = null;
                // output of type is not a valid output
                IsValidOutput = false;
                currentTokenIndex++;
            }
            else
                throw new ArgumentException("Unrecognized identifier: " + fullTypeName);
        }

        /// <summary>
        /// Evaluates a primitive
        /// </summary>
        private void EvaluatePrimitive()
        {
            // if need to evaluate
            if (evaluateValue)
            {
                CurrentValue = Tokens[currentTokenIndex].ParsedObject;
            }

            CurrentType = Tokens[currentTokenIndex].ParsedObject == null ? null : Tokens[currentTokenIndex].ParsedObject.GetType();
            CurrentMember = null;

            IsValidOutput = true;
            currentTokenIndex++;
        }

        /// <summary>
        /// Represents a result of a type resolve
        /// </summary>
        private class TypeResolveResult
        {
            /// <summary>
            /// The resolved type
            /// </summary>
            public Type ResolvedType { get; set; }
            /// <summary>
            /// The generic types of the resolved type
            /// </summary>
            public List<Type> GenericTypes { get; set; }
        }

        /// <summary>
        /// Represents a result of an expression evaluation
        /// </summary>
        public class EvaluateResult
        {
            /// <summary>
            /// The evaluation result is the result from the tokens starting FromToken
            /// </summary>
            public int FromToken { get; set; }

            /// <summary>
            /// The evaluation result is the result from the tokens ending at ToToken
            /// </summary>
            public int ToToken { get; set; }

            /// <summary>
            /// The value of the expression evaluated
            /// </summary>
            public Object Result { get; set; }
            /// <summary>
            ///  The type of the expression evaluated
            /// </summary>
            public Type ResultType { get; set; }

            /// <summary>
            /// The last member evaluated
            /// </summary>
            public MemberInfo Member { get; set; }

            /// <summary>
            /// The instance used to evaluate the last member
            /// </summary>
            public object MemberInstance { get; set; }
        }

        /// <summary>
        /// Resolve arguments from current token index
        /// </summary>
        /// <returns>A list of argument resolve results (containing type & value (if evaluating))</returns>
        private List<EvaluateResult> ResolveArguments()
        {

            // get the tokens of each argument
            List<List<Token>> arguments = SplitIntoParameters(Tokens, ref currentTokenIndex,
                                                              (t, i) => t[i].Type == TokenType.Comma, true);

            List<EvaluateResult> argumentValues = new List<EvaluateResult>();
            for (int i = 0; i < arguments.Count; i++)
            {
                var arg = arguments[i];

                var tokenIndexOfArgument = Tokens.IndexOf(arg.First());

                // evaluate each argument as an expression
                CSharpExpressionEvaluation eval = new CSharpExpressionEvaluation(arg, variables, typeStore, evaluateValue, evaluationTracker, tokenIndexOfArgument);
                try
                {
                    eval.EvaluateExpression();
                    eval.ApplyPendingCasts();
                }
                finally
                {
                    // store even when error, intellisense can use it
                    // and store it in the list
                    var result = new EvaluateResult()
                    {
                        FromToken = trackerOffset + tokenIndexOfArgument,
                        ToToken = trackerOffset + Tokens.IndexOf(arg.Last()),
                        Result = eval.CurrentValue,
                        ResultType = eval.CurrentType,
                        Member = eval.CurrentMember,
                        MemberInstance = eval.CurrentMemberInstance
                    };
                    evaluationTracker.EvaluationResults.Add(result);
                    argumentValues.Add(result);
                }
            }
            return argumentValues;
        }

        /// <summary>
        /// Resolve generic types from the given list of tokens at given token index
        /// </summary>
        /// <param name="tokens">Tokens to parse</param>
        /// <param name="currentTokenIndex">The offset in the token list</param>
        /// <param name="typeStore">The type store containing type information</param>
        /// <returns>A list of resolved types</returns>
        private static List<Type> ResolveGenericTypes(List<Token> tokens, ref int currentTokenIndex, TypeStore typeStore)
        {
            List<Type> genericTypes;
            genericTypes = new List<Type>();

            // get the tokens for each generic type
            List<List<Token>> genericTypeNames = SplitIntoParameters(tokens, ref currentTokenIndex,
                                                                    (t, i) => t[i].Type == TokenType.Comma, true);


            foreach (var typeName in genericTypeNames)
            {
                // resolve type for each generic type
                TypeResolveResult typeResolveInfo = ResolveType(typeName, typeStore);

                if (typeResolveInfo != null)
                {
                    // add the resolved type to the list
                    Type genericType = typeResolveInfo.ResolvedType;
                    genericTypes.Add(genericType);
                }
                else
                    throw new ArgumentException("Unable to resolve type " + string.Join("", typeName.Select(tn => tn.Text).ToArray()));
            }
            return genericTypes;
        }

        /// <summary>
        /// Resolve a type from the given list of tokens
        /// </summary>
        /// <param name="tokens">Tokens to parse</param>
        /// <param name="typeStore">The type store containing type information</param>
        /// <returns>The resolved type result</returns>
        private static TypeResolveResult ResolveType(List<Token> tokens, TypeStore typeStore)
        {
            int currentTokenIndex = 0;
            string fullTypeName = "";

            List<Type> genericTypes = null;

            while (currentTokenIndex < tokens.Count)
            {
                // if current token index is start of generic types, resolve the generic types
                if (IsStartOfGenericTypes(tokens, currentTokenIndex))
                    genericTypes = ResolveGenericTypes(tokens, ref currentTokenIndex, typeStore);
                else // otherwise append to type name
                    fullTypeName += tokens[currentTokenIndex].Text;

                currentTokenIndex++;
            }

            Type t;
            // find match for (possibly partial) name with given generic types
            List<Type> possibleTypes = typeStore.FindMatchingTypes(fullTypeName, genericTypes);
            if (possibleTypes.Count > 1)
                throw new ArgumentException("Ambigious match for type '" + fullTypeName + "'");

            if (possibleTypes.Count == 0)
            {
                // try alias manager
                object aliasObj = AliasManager.Instance.GetObjectFromAlias(fullTypeName);
                if (aliasObj is Type)
                    t = (Type)aliasObj;
                else
                    throw new ArgumentException("Unable to resolve type " + fullTypeName);
            }
            else
                t = possibleTypes[0];

            if (t != null)
            {
                // make type generic if there were generic types defined
                if (genericTypes != null)
                    t = t.MakeGenericType(genericTypes.ToArray());
                return new TypeResolveResult() { ResolvedType = t, GenericTypes = genericTypes };
            }
            else
                throw new ArgumentException("Unable to resolve type " + fullTypeName);
        }

        /// <summary>
        /// Resolve indexer arguments of a property
        /// </summary>
        /// <returns>A list of results (containing type and value (if evaluating))</returns>
        private List<EvaluateResult> ResolveIndexers()
        {
            // get tokens for each argument of the indexer
            List<List<Token>> indexers = SplitIntoParameters(Tokens, ref currentTokenIndex,
                                                             (t, i) => t[i].Type == TokenType.Comma, true);


            List<EvaluateResult> indexerValues = new List<EvaluateResult>();
            for (int i = 0; i < indexers.Count; i++)
            {
                var arg = indexers[i];

                var tokenIndexOfArgument = Tokens.IndexOf(arg.First());

                // evaluate each argument of the indexer as an expression
                CSharpExpressionEvaluation eval = new CSharpExpressionEvaluation(arg, variables, typeStore, evaluateValue, evaluationTracker, tokenIndexOfArgument);
                eval.EvaluateExpression();
                eval.ApplyPendingCasts();

                // add the result to the list
                var result = new EvaluateResult()
                {
                    FromToken = trackerOffset + tokenIndexOfArgument,
                    ToToken = trackerOffset + Tokens.IndexOf(arg.Last()),
                    Result = eval.CurrentValue,
                    ResultType = eval.CurrentType,
                    Member = eval.CurrentMember,
                    MemberInstance = eval.CurrentMemberInstance
                };
                evaluationTracker.EvaluationResults.Add(result);
                indexerValues.Add(result);
            }

            return indexerValues;
        }

        /// <summary>
        /// Checks if the parameters match the arguments given (for a method, constructor or property indexer)
        /// </summary>
        /// <param name="genericTypesOfClass">The generic types defined at the class level</param>
        /// <param name="genericTypesOfMethod">The generic types defined at the method level</param>
        /// <param name="argumentTypes">The types of the arguments</param>
        /// <param name="parameters">The parameters of the method,constructor or property indexer</param>
        /// <returns></returns>
        private static bool MatchesParameters(List<Type> genericTypesOfClass, List<Type> genericTypesOfMethod, List<Type> argumentTypes, ParameterInfo[] parameters)
        {
            for (int j = 0; j < argumentTypes.Count; j++)
            {
                var argType = argumentTypes[j];
                if (argType != null)
                {
                    var parType = parameters[j].ParameterType;

                    // check if the argument matches the corresponding parameter
                    bool match = MatchesParameter(genericTypesOfClass, genericTypesOfMethod, argType, parType);
                    if (!match)
                        return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Checks if an argument type matches the parameter type
        /// </summary>
        /// <param name="genericTypesOfClass">The generic types defined at the class level</param>
        /// <param name="genericTypesOfMethod">The generic types defined at the method level</param>
        /// <param name="argType">The type of the argument</param>
        /// <param name="parType">The type of the parameter</param>
        /// <returns></returns>
        private static bool MatchesParameter(List<Type> genericTypesOfClass, List<Type> genericTypesOfMethod, Type argType, Type parType)
        {
            // if the parameter type is a generic parameter
            if (parType.IsGenericParameter)
            {
                List<Type> genericTypes;
                // determine where the generic type is defined based on the declaring method property
                if (parType.DeclaringMethod == null)
                    genericTypes = genericTypesOfClass;
                else
                    genericTypes = genericTypesOfMethod;

                // check the constraints of the generic type
                foreach (var constraint in genericTypes[parType.GenericParameterPosition].GetGenericParameterConstraints())
                {
                    if (!constraint.IsAssignableFrom(argType))
                        return false;
                }
            }
            else
            {
                // parameter type isn't a generic parameter, but it contains generic parameters
                if (parType.ContainsGenericParameters)
                {
                    // get inner generic types of parameter and argument
                    var genericTypesOfPar = parType.GetGenericArguments();
                    var genericTypesOfArg = argType.GetGenericArguments();

                    if (genericTypesOfPar.Length != genericTypesOfArg.Length)
                        return false;

                    // and compare each one of them (recursively)
                    for (int i = 0; i < genericTypesOfPar.Length; i++)
                        MatchesParameter(genericTypesOfClass, genericTypesOfMethod, genericTypesOfArg[i], genericTypesOfPar[i]);
                }
                else
                    // parameter type does not contain a generic type, check if it's assignable from the corresponding argument type
                    if (!parType.IsAssignableFrom(argType))
                        return false;
            }

            return true;
        }

        /// <summary>
        /// Checks whether the token at given index the start of a generic type list
        /// </summary>
        /// <param name="tokens">A list of tokens to check in</param>
        /// <param name="idx">The index of the token</param>
        /// <returns>True if the token is the start of a generic type list</returns>
        private static bool IsStartOfGenericTypes(List<Token> tokens, int idx)
        {
            return idx < tokens.Count && tokens[idx].Type == TokenType.Operator && tokens[idx].Text == "<";
        }
        /// <summary>
        /// Checks whether the token at given index the end of a generic type list
        /// </summary>
        /// <param name="tokens">A list of tokens to check in</param>
        /// <param name="idx">The index of the token</param>
        /// <returns>True if the token is the end of a generic type list</returns>
        private static bool IsEndOfGenericTypes(List<Token> tokens, int idx)
        {
            return idx < tokens.Count && tokens[idx].Type == TokenType.Operator && tokens[idx].Text == ">";
        }

        /// <summary>
        /// Checks whether the token at given index the start of an argument list
        /// </summary>
        /// <param name="tokens">A list of tokens to check in</param>
        /// <param name="idx">The index of the token</param>
        /// <returns>True if the token is the start of an argument list</returns>
        private static bool IsStartOfArguments(List<Token> tokens, int idx)
        {
            return idx < tokens.Count && tokens[idx].Type == TokenType.OpenParens;
        }

        /// <summary>
        /// Checks whether the token at given index the end of an argument list
        /// </summary>
        /// <param name="tokens">A list of tokens to check in</param>
        /// <param name="idx">The index of the token</param>
        /// <returns>True if the token is the end of an argument list</returns>
        private static bool IsEndOfArguments(List<Token> tokens, int idx)
        {
            return idx < tokens.Count && tokens[idx].Type == TokenType.CloseParens;
        }

        /// <summary>
        /// Checks whether the token at given index the start of an indexer argument list
        /// </summary>
        /// <param name="tokens">A list of tokens to check in</param>
        /// <param name="idx">The index of the token</param>
        /// <returns>True if the token is the start of an indexer argument list</returns>
        private static bool IsStartOfIndexer(List<Token> tokens, int idx)
        {
            return idx < tokens.Count && tokens[idx].Type == TokenType.OpenBracket;
        }

        /// <summary>
        /// Checks whether the token at given index the end of an indexer argument list
        /// </summary>
        /// <param name="tokens">A list of tokens to check in</param>
        /// <param name="idx">The index of the token</param>
        /// <returns>True if the token is the end of an indexer argument list</returns>
        private static bool IsEndOfIndexer(List<Token> tokens, int idx)
        {
            return idx < tokens.Count && tokens[idx].Type == TokenType.CloseBracket;
        }

        /// <summary>
        /// Split the following tokens by the given seperator marker into smaller set of tokens
        /// e.g  ( ... ,.... , ... )
        /// or   [ ... , ... ]
        /// </summary>
        /// <param name="tokens">The tokens to split</param>
        /// <param name="currentTokenIndex">The offset in the tokens to start splitting</param>
        /// <param name="seperatorMarker">The seperator marker</param>
        /// <param name="surroundedByParens">Indicates that the tokens to split are wrapped by peranthesis</param>
        /// <returns></returns>
        private static List<List<Token>> SplitIntoParameters(List<Token> tokens, ref int currentTokenIndex, Func<List<Token>, int, bool> seperatorMarker, bool surroundedByParens)
        {
            List<List<Token>> statements = new List<List<Token>>();

            int openParenthesisCount = 0;

            if (surroundedByParens)
            {
                if (!IsStartOfArguments(tokens, currentTokenIndex) &&
                       !IsStartOfGenericTypes(tokens, currentTokenIndex) &&
                       !IsStartOfIndexer(tokens, currentTokenIndex))
                    throw new ArgumentException("The index is not positioned on the start marker to split into statements");

                openParenthesisCount++;
                currentTokenIndex++;
            }
            else
            {
                openParenthesisCount++;
            }




            List<Token> statement = new List<Token>();
            while (openParenthesisCount > 0 && currentTokenIndex < tokens.Count)
            {
                if (IsStartOfArguments(tokens, currentTokenIndex) ||
                    IsStartOfGenericTypes(tokens, currentTokenIndex) ||
                    IsStartOfIndexer(tokens, currentTokenIndex))// startMarker(tokens, currentTokenIndex))
                {
                    // it's a open parens, count it
                    openParenthesisCount++;

                    // and add it to the statement if it belongs to the sub statement
                    if (openParenthesisCount > 1)
                        statement.Add(tokens[currentTokenIndex]);
                    currentTokenIndex++;
                }
                else if (IsEndOfArguments(tokens, currentTokenIndex) ||
                         IsEndOfGenericTypes(tokens, currentTokenIndex) ||
                         IsEndOfIndexer(tokens, currentTokenIndex)) // endMarker(tokens, currentTokenIndex))
                {
                    openParenthesisCount--;

                    if (openParenthesisCount >= 1)
                        statement.Add(tokens[currentTokenIndex]);

                    // if the open parens count is 0 and there was a statement,
                    // add the statement
                    if (openParenthesisCount == 0 && statement.Count > 0)
                    {
                        statements.Add(statement);
                        statement = new List<Token>();
                    }
                    currentTokenIndex++;
                }
                else if (seperatorMarker(tokens, currentTokenIndex))
                {
                    if (openParenthesisCount > 1)
                        statement.Add(tokens[currentTokenIndex]);

                    if (openParenthesisCount == 1)
                    {  // only go to next argument if it's the argument of the method we're parsing
                        // and not a sub method as argument  which is the case when the current open parens count is 1
                        // (which is the initial open parenthesis)
                        statements.Add(statement);
                        statement = new List<Token>();
                    }

                    currentTokenIndex++;
                }
                else
                {
                    statement.Add(tokens[currentTokenIndex]);
                    currentTokenIndex++;
                }
            }

            if (statement.Count > 0)
                statements.Add(statement); // add partial statement

            return statements;
        }

    }
}
