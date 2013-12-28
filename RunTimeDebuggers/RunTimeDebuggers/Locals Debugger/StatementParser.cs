using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using RunTimeDebuggers.Helpers;

namespace RunTimeDebuggers.LocalsDebugger
{
    internal static class StatementParser
    {
        private static TypeStore typeStore;
        internal static TypeStore Store
        {
            get
            {
                if (typeStore == null)
                    typeStore = new TypeStore();

                return typeStore;
            }
        }
        public static CSharpExpressionEvaluation.EvaluateResult EvaluateStatement(string statement, object thisObject, bool evaluateValue, bool applyCasts, bool useDebugger)
        {
            List<Token> tokens = GetTokensOfStatement(statement);

            var variables = new Dictionary<string, CSharpExpressionEvaluation.Variable>() { { "this", new CSharpExpressionEvaluation.Variable() { Value = thisObject, Type = thisObject == null ? null : thisObject.GetType() } } };

            var tracker = new EvaluationTracker();
            CSharpExpressionEvaluation eval = new CSharpExpressionEvaluation(tokens, variables, Store, evaluateValue, tracker, useDebugger:useDebugger);
            //eval.EvaluateStatement();
            eval.EvaluateExpression();

            if (applyCasts)
                eval.ApplyPendingCasts();

            return new CSharpExpressionEvaluation.EvaluateResult() { Result = eval.CurrentValue, ResultType = eval.CurrentType };

            //EvaluateStatement(tokens, thisObject, evaluateValue);
        }

        private static List<Token> GetTokensOfStatement(string statement)
        {
            Tokenizer t = new Tokenizer(statement);
            if (t.IsInvalid)
                return new List<Token>();
            //    throw new Exception("Invalid statement");

            List<Token> tokens = new List<Token>();
            while (t.GetNextToken().Type != TokenType.NotAToken)
                tokens.Add(t.Current);

            return tokens;
        }


        private static int GetTokenIndexFromCharIndex(string statement, int charPos)
        {
            List<Token> partTokens = GetTokensOfStatement(statement.Substring(0, charPos + 1));
            return partTokens.Count - 1;
        }
        public static KeyValuePair<Type, string> GetTypeOfCurrentStatement(string statement, int pos, object thisObject, bool applyCasts)
        {
            var beforeAndAfterPos = GetBeforeAndAfterPosition(statement, pos);

            var variables = new Dictionary<string, CSharpExpressionEvaluation.Variable>() { { "this", new CSharpExpressionEvaluation.Variable() { Value = thisObject, Type = thisObject == null ? null : thisObject.GetType() } } };


            var leadingTokens = GetTokensOfStatement(beforeAndAfterPos.Key);

            var tracker = new EvaluationTracker();
            //var tokens = GetTokensOfStatement(statement);

            //var tokenAtCurPos = GetTokenIndexFromCharIndex(statement, pos);

            //CSharpExpressionEvaluation eval = new CSharpExpressionEvaluation(leadingTokens, variables, Store, false, tracker);
            CSharpExpressionEvaluation eval = new CSharpExpressionEvaluation(leadingTokens, variables, Store, false, tracker);

            try
            {
                //eval.EvaluateStatement();
                eval.EvaluateExpression();

                if (applyCasts)
                    eval.ApplyPendingCasts();

            }
            catch (Exception)
            {

            }


            var result = new CSharpExpressionEvaluation.EvaluateResult() { Result = eval.CurrentValue, ResultType = eval.CurrentType };

            //EvaluateResult result = EvaluateStatement(tokensAndAfter.Item1, thisObject, false);

            return new KeyValuePair<Type, string>(result.ResultType, beforeAndAfterPos.Value);
        }

        public static List<MethodInfo> GetCurrentMethodOverloads(string statement, int pos, object thisObject)
        {
            var beforeAndAfterPos = GetBeforeAndAfterPosition(statement, pos);

            int prevStatementEndIdx = pos - (beforeAndAfterPos.Key.Length + beforeAndAfterPos.Value.Length);
            prevStatementEndIdx--;

            if (prevStatementEndIdx >= 0)
            {
                var statementBeforeParts = GetBeforeAndAfterPosition(statement, prevStatementEndIdx);
                var encapsulatingStatement = GetTokensOfStatement(statementBeforeParts.Key);

                var variables = new Dictionary<string, CSharpExpressionEvaluation.Variable>() { { "this", new CSharpExpressionEvaluation.Variable() { Value = thisObject, Type = thisObject.GetType() } } };

                var tracker = new EvaluationTracker();
                CSharpExpressionEvaluation eval = new CSharpExpressionEvaluation(encapsulatingStatement, variables, Store, false, tracker);
                var tokens = GetTokensOfStatement(statement);

                //eval.EvaluateStatement();
                eval.EvaluateExpression();

                if (eval.CurrentType != null)
                {
                    string methodName = statementBeforeParts.Value.Replace("(", "");
                    methodName = System.Text.RegularExpressions.Regex.Replace(methodName, "<.*>", "");

                    return eval.CurrentType.GetMethodsOfType(true, true)
                                    .Where(m => m.Name == methodName)
                                    .ToList();
                }
            }
            return new List<MethodInfo>();
        }

        private static KeyValuePair<string, string> GetBeforeAndAfterPosition(string statement, int pos)
        {
            if (string.IsNullOrEmpty(statement))
                return new KeyValuePair<string, string>("", "");

            string before = "";
            string after = "";

            int parens = 0;
            int brackets = 0;
            int genericParens = 0;

            int afterOffset = pos;

            int i = pos;
            bool dotEncountered = false;
            bool endOfStatementEncountered = false;

            char curChar = statement[i];


            char[] endofStatements = new char[] { ',', '+', '-', '/', '*', '%' };

            while (i >= 0 && parens >= 0 && brackets >= 0 && genericParens >= 0 && !endOfStatementEncountered)
            {
                curChar = statement[i];

                if (curChar == '(') parens--;
                if (curChar == ')') parens++;
                if (curChar == '[') brackets--;
                if (curChar == ']') brackets++;
                if (curChar == '<') genericParens--;
                if (curChar == '>') genericParens++;

                if (parens == 0 && brackets == 0 && genericParens == 0)
                    if (endofStatements.Contains(curChar)) endOfStatementEncountered = true;

                if (!dotEncountered)
                {
                    if (parens == 0 && brackets == 0 && genericParens == 0 && curChar == '.')
                    {
                        dotEncountered = true;
                        afterOffset = i + 1;
                    }
                }
                else
                {
                    if (!endOfStatementEncountered)
                        before = curChar + before;
                }
                i--;
            }
            if (string.IsNullOrEmpty(before))
            {
                if (i == -1)
                    afterOffset = 0;
                else
                    afterOffset = i + 2;
            }

            if (afterOffset >= statement.Length)
                after = "";
            else
            {
                i = afterOffset;
                curChar = statement[i];
                while (i < statement.Length && curChar != '(' && curChar != '[')
                {
                    after = after + curChar;
                    i++;
                    if (i < statement.Length)
                        curChar = statement[i];
                }
            }

            return new KeyValuePair<string, string>(before, after);
        }

        public static List<string> GetStringsNotToAutocompleteOn()
        {
            return new List<string>() { "new", "this", "typeof" };
        }

        //private class TypeResolveResult
        //{
        //    public Type ResolvedType { get; set; }
        //    public List<Type> GenericTypes { get; set; }
        //}

        //public class EvaluateResult
        //{
        //    public Object Result { get; set; }
        //    public Type ResultType { get; set; }
        //}

        //public static EvaluateResult EvaluateStatement(List<Token> tokens, object thisObject, bool evaluateValue)
        //{
        //    int currentTokenIndex = 0;
        //    object currentValue = thisObject;
        //    Type currentType = thisObject.GetType();
        //    List<Type> genericTypesOfCurrentType = new List<Type>();
        //    bool isValidOutput = false;

        //    while (currentTokenIndex < tokens.Count)
        //    {
        //        string current = tokens[currentTokenIndex].Text;

        //        if (tokens[currentTokenIndex].Type == TokenType.Identifier)
        //        {
        //            if (tokens[currentTokenIndex].Text == "this")
        //            {
        //                if (evaluateValue)
        //                    currentValue = thisObject;
        //                currentType = thisObject.GetType();
        //                isValidOutput = true;
        //                currentTokenIndex++;
        //            }
        //            else if (tokens[currentTokenIndex].Text == "new")
        //            {
        //                currentTokenIndex++;

        //                List<Token> typeTokens = new List<Token>();
        //                while (!IsStartOfArguments(tokens, currentTokenIndex))
        //                {
        //                    typeTokens.Add(tokens[currentTokenIndex]);
        //                    currentTokenIndex++;
        //                }

        //                TypeResolveResult typeInfoToConstruct = ResolveType(typeTokens);
        //                if (typeInfoToConstruct != null)
        //                {
        //                    Type typeToConstruct = typeInfoToConstruct.ResolvedType;
        //                    var constructors = GetConstructorsOfType(typeToConstruct);
        //                    genericTypesOfCurrentType = typeInfoToConstruct.GenericTypes;

        //                    if (constructors.Count() > 0)
        //                    {
        //                        if (IsStartOfArguments(tokens, currentTokenIndex))
        //                        {
        //                            List<EvaluateResult> argumentValues = ResolveArguments(tokens, thisObject, ref currentTokenIndex, evaluateValue);

        //                            // filter methods based on parameters
        //                            constructors = constructors.Where(m =>
        //                            {
        //                                var parameters = m.GetParameters();
        //                                bool check = parameters.Length == argumentValues.Count;
        //                                if (!check)
        //                                    return false;

        //                                return MatchesParameters(genericTypesOfCurrentType, null, argumentValues, parameters);
        //                            }).ToList();

        //                            if (constructors.Count() > 1)
        //                                throw new ArgumentException("Multiple matches found " + current);
        //                            else if (constructors.Count() == 1)
        //                            {
        //                                var constructor = constructors.FirstOrDefault();

        //                                try
        //                                {
        //                                    if (evaluateValue)
        //                                    {
        //                                        currentValue = constructor.Invoke(argumentValues.Select(a => a.Result).ToArray());
        //                                        currentType = currentValue == null ? typeToConstruct : currentValue.GetType();
        //                                    }
        //                                    else
        //                                        currentType = typeToConstruct;

        //                                    isValidOutput = true;
        //                                }
        //                                catch (TargetInvocationException ex) // unwrap
        //                                {
        //                                    throw ex.InnerException;
        //                                }

        //                                currentTokenIndex++;
        //                            }
        //                            else
        //                                throw new ArgumentException("No overload for constructor " + current + " has " + argumentValues.Count + " parameters");
        //                        }
        //                        else
        //                            throw new ArgumentException("Expected arguments for constructor");
        //                    }
        //                    else
        //                        throw new ArgumentException("No suitable constructor found for " + current);
        //                }
        //                else
        //                    throw new ArgumentException("Unable to resolve type " + string.Join("", typeTokens.Select(tn => tn.Text)));
        //            }
        //            else if (tokens[currentTokenIndex].Text == "typeof")
        //            {
        //                currentTokenIndex++;
        //                List<List<Token>> typeTokens = SplitIntoParameters(tokens, ref currentTokenIndex,
        //                    IsStartOfArguments,
        //                    IsEndOfArguments,
        //                    (t, i) => t[i].Type == TokenType.Comma);

        //                if (typeTokens.Count > 0)
        //                {
        //                    var resolveResult = ResolveType(typeTokens[0]);
        //                    if (resolveResult.ResolvedType != null)
        //                    {
        //                        currentValue = resolveResult.ResolvedType;
        //                        currentType = typeof(Type);
        //                        isValidOutput = true;
        //                    }
        //                    else
        //                        throw new ArgumentException("Unable to resolve type " + string.Join("", typeTokens[0].Select(tn => tn.Text)));
        //                }
        //                else
        //                    throw new ArgumentException("Expected type to evaluate for typeof");

        //            }
        //            else if (IsFieldOf(currentType, tokens[currentTokenIndex].Text))
        //            {
        //                // evaluate field
        //                var field = GetFieldsOfType(currentType).Where(f => f.Name == current).FirstOrDefault();
        //                try
        //                {
        //                    if (evaluateValue)
        //                    {
        //                        currentValue = field.GetValue(currentValue);
        //                        currentType = currentValue == null ? field.FieldType : currentValue.GetType();
        //                    }
        //                    else
        //                        currentType = field.FieldType;

        //                    isValidOutput = true;
        //                }
        //                catch (TargetInvocationException ex) // unwrap
        //                {
        //                    throw ex.InnerException;
        //                }
        //                currentTokenIndex++;
        //            }
        //            else if (IsPropertyOf(currentType, tokens[currentTokenIndex].Text))
        //            {
        //                var prop = GetPropertiesOfType(currentType)
        //                            .Where(f => f.Name == current).FirstOrDefault();

        //                // check if there is indexer
        //                List<EvaluateResult> indexerValues = new List<EvaluateResult>();
        //                if (currentTokenIndex + 1 < tokens.Count && IsStartOfIndexer(tokens, currentTokenIndex + 1))
        //                {
        //                    currentTokenIndex++;
        //                    indexerValues = ResolveIndexers(tokens, thisObject, ref currentTokenIndex, evaluateValue);
        //                }

        //                try
        //                {
        //                    if (evaluateValue)
        //                    {
        //                        currentValue = prop.GetValue(currentValue, indexerValues.Select(i => i.Result).ToArray());
        //                        currentType = currentValue == null ? prop.PropertyType : currentValue.GetType();
        //                    }
        //                    else
        //                        currentType = prop.PropertyType;

        //                    isValidOutput = true;
        //                }
        //                catch (TargetInvocationException ex) // unwrap
        //                {
        //                    throw ex.InnerException;
        //                }

        //                if (indexerValues.Count == 0)
        //                    currentTokenIndex++;
        //            }
        //            else if (IsMethodOf(currentType, tokens[currentTokenIndex].Text))
        //            {
        //                var methods = GetMethodsOfType(currentType)
        //                              .Where(f => f.Name == current)
        //                              .ToList();

        //                // read generic type tokens

        //                List<Type> genericTypesOfMethod;
        //                if (currentTokenIndex + 1 < tokens.Count && IsStartOfGenericTypes(tokens, currentTokenIndex + 1))
        //                {
        //                    // has generic parameters;
        //                    currentTokenIndex++;
        //                    genericTypesOfMethod = ResolveGenericTypes(tokens, ref currentTokenIndex);

        //                }
        //                else
        //                    genericTypesOfMethod = new List<Type>();

        //                // filter available methods based on given generic types
        //                if (genericTypesOfMethod.Count > 0)
        //                {
        //                    methods = methods.Where(m =>
        //                        {
        //                            var genericArguments = m.GetGenericArguments();
        //                            return m.ContainsGenericParameters && genericArguments.Length == genericTypesOfMethod.Count;
        //                        }).ToList();
        //                }

        //                if (methods.Count() > 0)
        //                {
        //                    if (genericTypesOfMethod.Count == 0)
        //                        currentTokenIndex++;

        //                    if (IsStartOfArguments(tokens, currentTokenIndex))
        //                    {
        //                        List<EvaluateResult> argumentValues = ResolveArguments(tokens, thisObject, ref currentTokenIndex, evaluateValue);

        //                        // filter methods based on parameters
        //                        methods = methods.Where(m =>
        //                                        {
        //                                            var parameters = m.GetParameters();
        //                                            bool check = parameters.Length == argumentValues.Count;
        //                                            if (!check)
        //                                                return false;

        //                                            return MatchesParameters(genericTypesOfCurrentType, genericTypesOfMethod, argumentValues, parameters);
        //                                        }).ToList();

        //                        if (methods.Count() > 1)
        //                        {
        //                            throw new ArgumentException("Multiple matches found " + current);
        //                        }
        //                        else if (methods.Count() == 1)
        //                        {
        //                            var method = methods.FirstOrDefault();

        //                            if (genericTypesOfMethod.Count > 0)
        //                                method = method.MakeGenericMethod(genericTypesOfMethod.ToArray());

        //                            try
        //                            {
        //                                if (evaluateValue)
        //                                {
        //                                    currentValue = method.Invoke(currentValue, argumentValues.Select(a => a.Result).ToArray());
        //                                    currentType = currentValue == null ? method.ReturnType : currentValue.GetType();
        //                                }
        //                                else
        //                                    currentType = method.ReturnType;

        //                                isValidOutput = true;
        //                            }
        //                            catch (TargetInvocationException ex) // unwrap
        //                            {
        //                                throw ex.InnerException;
        //                            }

        //                            currentTokenIndex++;
        //                        }
        //                        else
        //                            throw new ArgumentException("No overload for method " + current + " has " + argumentValues.Count + " parameters");
        //                    }
        //                    else
        //                        throw new ArgumentException("Expected arguments for method");
        //                }
        //                else
        //                    throw new ArgumentException("No suitable method found for " + current);
        //            }
        //            else
        //            {
        //                // not a method, field or property
        //                //part of namespace?

        //                Type resolvedType = null;
        //                string fullTypeName = "";
        //                while (resolvedType == null && currentTokenIndex < tokens.Count &&
        //                      (tokens[currentTokenIndex].Type == TokenType.Dot ||
        //                       tokens[currentTokenIndex].Type == TokenType.Identifier))
        //                {
        //                    fullTypeName += tokens[currentTokenIndex].Text;

        //                    if (tokens[currentTokenIndex].Type == TokenType.Identifier)
        //                    {
        //                        if (currentTokenIndex + 1 < tokens.Count && IsStartOfGenericTypes(tokens, currentTokenIndex + 1))
        //                        {
        //                            // has generic parameters;
        //                            currentTokenIndex++;
        //                            var genericTypes = ResolveGenericTypes(tokens, ref currentTokenIndex);

        //                            resolvedType = GetTypeFromName(fullTypeName, genericTypes);
        //                            if (resolvedType != null)
        //                            {
        //                                if (genericTypes != null)
        //                                    resolvedType = resolvedType.MakeGenericType(genericTypes.ToArray());
        //                            }
        //                            else
        //                                throw new ArgumentException("Unable to resolve type " + fullTypeName);

        //                            genericTypesOfCurrentType = genericTypes;
        //                        }
        //                        else
        //                        {
        //                            resolvedType = GetTypeFromName(fullTypeName, null);
        //                            currentTokenIndex++;
        //                        }
        //                    }
        //                    else
        //                        currentTokenIndex++;

        //                }

        //                if (resolvedType != null)
        //                {
        //                    currentType = resolvedType;
        //                    if (evaluateValue)
        //                        currentValue = null;
        //                    // output of type is not a valid output
        //                    isValidOutput = false;
        //                    currentTokenIndex++;
        //                }
        //                else
        //                    throw new ArgumentException("Unrecognized identifier: " + current);
        //            }
        //        }
        //        else if (tokens[currentTokenIndex].Type == TokenType.Dot)
        //        {
        //            currentTokenIndex++;
        //        }
        //        else if (tokens[currentTokenIndex].Type == TokenType.Primitive)
        //        {
        //            if (evaluateValue)
        //            {
        //                currentValue = tokens[currentTokenIndex].ParsedObject;
        //                currentType = currentValue == null ? null : currentValue.GetType();
        //            }
        //            else
        //                currentType = tokens[currentTokenIndex].ParsedObject == null ? null : tokens[currentTokenIndex].ParsedObject.GetType();

        //            isValidOutput = true;
        //            currentTokenIndex++;
        //        }
        //        else
        //        {
        //            throw new ArgumentException("Identifier not recognized " + current);
        //            //currentTokenIndex++;
        //        }
        //    }

        //    if (evaluateValue)
        //    {
        //        if (!isValidOutput)
        //            throw new ArgumentException("Expected statement");
        //    }

        //    return new EvaluateResult() { Result = currentValue, ResultType = currentType };
        //}

        //private static List<EvaluateResult> ResolveArguments(List<Token> tokens, object thisObject, ref int currentTokenIndex, bool evaluateValue)
        //{
        //    //List<List<Token>> arguments = GetArgumentsOfMethod(tokens, ref currentTokenIndex);
        //    List<List<Token>> arguments = SplitIntoParameters(tokens, ref currentTokenIndex,
        //                                                      IsStartOfArguments,
        //                                                      IsEndOfArguments,
        //                                                      (t, i) => t[i].Type == TokenType.Comma);

        //    List<EvaluateResult> argumentValues = new List<EvaluateResult>();
        //    foreach (var arg in arguments)
        //        argumentValues.Add(EvaluateStatement(arg, thisObject, evaluateValue));

        //    return argumentValues;
        //}

        //private static List<Type> ResolveGenericTypes(List<Token> tokens, ref int currentTokenIndex)
        //{
        //    List<Type> genericTypes;
        //    genericTypes = new List<Type>();

        //    List<List<Token>> genericTypeNames = SplitIntoParameters(tokens, ref currentTokenIndex,
        //                                                            IsStartOfGenericTypes,
        //                                                            IsEndOfGenericTypes,
        //                                                            (t, i) => t[i].Type == TokenType.Comma);


        //    foreach (var typeName in genericTypeNames)
        //    {
        //        TypeResolveResult typeResolveInfo = ResolveType(typeName);

        //        if (typeResolveInfo != null)
        //        {
        //            Type genericType = typeResolveInfo.ResolvedType;
        //            genericTypes.Add(genericType);
        //        }
        //        else
        //            throw new ArgumentException("Unable to resolve type " + string.Join("", typeName.Select(tn => tn.Text)));
        //    }
        //    return genericTypes;
        //}

        //private static TypeResolveResult ResolveType(List<Token> tokens)
        //{
        //    int currentTokenIndex = 0;
        //    string fullTypeName = "";

        //    List<Type> genericTypes = null;

        //    while (currentTokenIndex < tokens.Count)
        //    {
        //        if (IsStartOfGenericTypes(tokens, currentTokenIndex))
        //        {
        //            genericTypes = ResolveGenericTypes(tokens, ref currentTokenIndex);
        //        }
        //        else
        //            fullTypeName += tokens[currentTokenIndex].Text;

        //        currentTokenIndex++;
        //    }

        //    Type t = GetTypeFromName(fullTypeName, genericTypes);

        //    if (t != null)
        //    {
        //        if (genericTypes != null)
        //            t = t.MakeGenericType(genericTypes.ToArray());
        //        return new TypeResolveResult() { ResolvedType = t, GenericTypes = genericTypes };
        //    }
        //    else
        //        throw new ArgumentException("Unable to resolve type " + fullTypeName);
        //}

        //private static List<EvaluateResult> ResolveIndexers(List<Token> tokens, object thisObject, ref int currentTokenIndex, bool evaluateValue)
        //{
        //    //List<List<Token>> indexers = GetIndexersOfProperty(tokens, ref currentTokenIndex);
        //    List<List<Token>> indexers = SplitIntoParameters(tokens, ref currentTokenIndex,
        //                                                     IsStartOfIndexer,
        //                                                     IsEndOfIndexer,
        //                                                     (t, i) => t[i].Type == TokenType.Comma);


        //    List<EvaluateResult> indexerValues = new List<EvaluateResult>();
        //    foreach (var arg in indexers)
        //        indexerValues.Add(EvaluateStatement(arg, thisObject, evaluateValue));

        //    return indexerValues;
        //}

        //private static bool MatchesParameters(List<Type> genericTypesOfClass, List<Type> genericTypesOfMethod, List<EvaluateResult> argumentValues, ParameterInfo[] parameters)
        //{
        //    for (int j = 0; j < argumentValues.Count; j++)
        //    {
        //        if (argumentValues[j].ResultType != null)
        //        {
        //            var par = parameters[j].ParameterType;
        //            if (par.IsGenericParameter)
        //            {
        //                List<Type> genericTypes;
        //                if (par.DeclaringMethod == null)
        //                    genericTypes = genericTypesOfClass;
        //                else
        //                    genericTypes = genericTypesOfMethod;

        //                // it's a generic parameter, check the generic type instead
        //                if (!genericTypes[par.GenericParameterPosition].IsAssignableFrom(argumentValues[j].ResultType))
        //                    return false;
        //            }
        //            else
        //            {
        //                if (!par.IsAssignableFrom(argumentValues[j].ResultType))
        //                    return false;
        //            }
        //        }
        //    }

        //    return true;
        //}

        //private static bool IsStartOfGenericTypes(List<Token> tokens, int idx)
        //{
        //    return idx < tokens.Count && tokens[idx].Type == TokenType.Operator && tokens[idx].Text == "<";
        //}

        //private static bool IsEndOfGenericTypes(List<Token> tokens, int idx)
        //{
        //    return idx < tokens.Count && tokens[idx].Type == TokenType.Operator && tokens[idx].Text == ">";
        //}

        //private static bool IsStartOfArguments(List<Token> tokens, int idx)
        //{
        //    return idx < tokens.Count && tokens[idx].Type == TokenType.OpenParens;
        //}

        //private static bool IsEndOfArguments(List<Token> tokens, int idx)
        //{
        //    return idx < tokens.Count && tokens[idx].Type == TokenType.CloseParens;
        //}

        //private static bool IsStartOfIndexer(List<Token> tokens, int idx)
        //{
        //    return idx < tokens.Count && tokens[idx].Type == TokenType.OpenBracket;
        //}

        //private static bool IsEndOfIndexer(List<Token> tokens, int idx)
        //{
        //    return idx < tokens.Count && tokens[idx].Type == TokenType.CloseBracket;
        //}

        //private static List<List<Token>> SplitIntoParameters(List<Token> tokens, ref int currentTokenIndex, Func<List<Token>, int, bool> startMarker, Func<List<Token>, int, bool> endMarker, Func<List<Token>, int, bool> seperatorMarker)
        //{
        //    List<List<Token>> statements = new List<List<Token>>();

        //    if (!startMarker(tokens, currentTokenIndex))
        //        throw new ArgumentException("The index is not positioned on the start marker to split into statements");

        //    int openParenthesisCount = 0;
        //    openParenthesisCount++;
        //    currentTokenIndex++;

        //    List<Token> statement = new List<Token>();
        //    while (openParenthesisCount > 0 && currentTokenIndex < tokens.Count)
        //    {
        //        if (startMarker(tokens, currentTokenIndex))
        //        {
        //            openParenthesisCount++;
        //            if (openParenthesisCount > 1)
        //                statement.Add(tokens[currentTokenIndex]);
        //            currentTokenIndex++;
        //        }
        //        else if (endMarker(tokens, currentTokenIndex))
        //        {
        //            openParenthesisCount--;

        //            if (openParenthesisCount >= 1)
        //                statement.Add(tokens[currentTokenIndex]);

        //            if (openParenthesisCount == 0 && statement.Count > 0)
        //            {
        //                statements.Add(statement);
        //                statement = new List<Token>();
        //            }
        //            currentTokenIndex++;
        //        }
        //        else if (seperatorMarker(tokens, currentTokenIndex))
        //        {
        //            if (openParenthesisCount > 1)
        //                statement.Add(tokens[currentTokenIndex]);

        //            if (openParenthesisCount == 1)
        //            { // only go to next argument if it's the argument of the method we're parsing
        //                // and not a sub method as argument
        //                statements.Add(statement);
        //                statement = new List<Token>();
        //            }

        //            currentTokenIndex++;
        //        }
        //        else
        //        {
        //            statement.Add(tokens[currentTokenIndex]);
        //            currentTokenIndex++;
        //        }
        //    }
        //    return statements;
        //}

        //private static Type GetTypeFromName(string typename, List<Type> genericTypes)
        //{
        //    if (typename == "string")
        //        return typeof(string);
        //    else if (typename == "int")
        //        return typeof(int);
        //    else if (typename == "long")
        //        return typeof(long);
        //    else if (typename == "short")
        //        return typeof(short);
        //    else if (typename == "bool")
        //        return typeof(bool);
        //    else if (typename == "float")
        //        return typeof(float);
        //    else if (typename == "double")
        //        return typeof(double);
        //    else if (typename == "byte")
        //        return typeof(byte);
        //    else if (typename == "object")
        //        return typeof(object);
        //    else if (typename == "ushort")
        //        return typeof(ushort);
        //    else if (typename == "uint")
        //        return typeof(uint);
        //    else if (typename == "ulong")
        //        return typeof(ulong);
        //    else if (typename == "sbyte")
        //        return typeof(sbyte);


        //    Type t = Type.GetType(typename);

        //    if (t == null)
        //    {
        //        foreach (Assembly a in AppDomain.CurrentDomain.GetAssemblies())
        //        {
        //            foreach (Type typ in a.GetTypesSafe())
        //            {
        //                if (typ.Name.Contains('`'))
        //                {
        //                    if (typ.Name.Substring(0, typ.Name.IndexOf('`')) == typename)
        //                        return typ;
        //                }
        //                else
        //                {
        //                    if (typ.Name == typename || typ.FullName == typename)
        //                    {
        //                        var genericArguments = typ.GetGenericArguments();
        //                        if (typ.IsGenericType && genericArguments != null)
        //                        {
        //                            if (genericTypes.Count == genericArguments.Length)
        //                            {
        //                                bool match = true;
        //                                for (int i = 0; i < genericArguments.Length; i++)
        //                                {
        //                                    if (!genericArguments[i].IsAssignableFrom(genericTypes[i]))
        //                                    {
        //                                        match = false;
        //                                        break;
        //                                    }
        //                                }
        //                                if (match)
        //                                    return typ;
        //                            }
        //                        }
        //                        else
        //                            return typ;
        //                    }
        //                }
        //            }
        //        }
        //    }
        //    else
        //        return t;

        //    return null;
        //}


        //private static bool IsFieldOf(Type t, string name)
        //{
        //    var field = GetFieldsOfType(t).Where(f => f.Name == name).FirstOrDefault();
        //    return field != null;
        //}

        //private static bool IsPropertyOf(Type t, string name)
        //{
        //    var prop = GetPropertiesOfType(t)
        //                  .Where(f => f.Name == name).FirstOrDefault();
        //    return prop != null;
        //}

        //private static bool IsMethodOf(Type t, string name)
        //{
        //    var method = GetMethodsOfType(t)
        //                    .Where(f => f.Name == name && !f.IsSpecialName).FirstOrDefault();
        //    return method != null;
        //}


        //private static IEnumerable<PropertyInfo> GetPropertiesOfType(Type t)
        //{
        //    var props = t.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance | BindingFlags.FlattenHierarchy);
        //    return props;
        //}

        //private static IEnumerable<MethodInfo> GetMethodsOfType(Type t)
        //{
        //    List<MethodInfo> methods = new List<MethodInfo>();

        //    // all instance methods
        //    methods.AddRange(t.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.FlattenHierarchy)
        //                      .Where(m => !m.IsSpecialName));

        //    // all static methods
        //    Type cur = t;
        //    while (cur != null)
        //    {
        //        methods.AddRange(cur.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static)
        //                            .Where(m => !m.IsSpecialName));

        //        var events = GetEventsOfType(cur);

        //        cur = cur.BaseType;
        //    }

        //    var props = GetPropertiesOfType(t);

        //    return methods;
        //}

        //private static IEnumerable<EventInfo> GetEventsOfType(Type t)
        //{
        //    List<EventInfo> events = new List<EventInfo>();

        //    events.AddRange(t.GetEvents(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.FlattenHierarchy));


        //    return events;

        //}

        //private static IEnumerable<ConstructorInfo> GetConstructorsOfType(Type t)
        //{
        //    List<ConstructorInfo> constructors = new List<ConstructorInfo>();
        //    constructors.AddRange(t.GetConstructors(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.FlattenHierarchy));
        //    return constructors;
        //}

        //private static IEnumerable<FieldInfo> GetFieldsOfType(Type t)
        //{
        //    List<FieldInfo> fields = new List<FieldInfo>();

        //    Type cur = t;
        //    while (cur != null)
        //    {
        //        fields.AddRange(cur.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance | BindingFlags.FlattenHierarchy));
        //        cur = cur.BaseType;
        //    }
        //    return fields;
        //}


        //public static IEnumerable<MemberInfo> GetAllMembers(Type t)
        //{
        //    List<MemberInfo> members = new List<MemberInfo>();
        //    members.AddRange(GetFieldsOfType(t));
        //    members.AddRange(GetPropertiesOfType(t));
        //    members.AddRange(GetMethodsOfType(t));

        //    return members;
        //}

    }
}
