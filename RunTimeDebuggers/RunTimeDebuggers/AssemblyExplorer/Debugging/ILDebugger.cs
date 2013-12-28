using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using RunTimeDebuggers.Helpers;
using System.Reflection.Emit;
using System.Linq.Expressions;

namespace RunTimeDebuggers.AssemblyExplorer
{
 public   class ILDebugger
    {
        private List<ILInstruction> instructions;
        private int curInstructionIndex;


        private MethodBase method;

        public MethodBase Method
        {
            get { return method; }
        }

        private Stack<StackEntry> stack = new Stack<StackEntry>();
        public Stack<StackEntry> Stack { get { return stack; } }

        private Stack<ProtectedBlockExecution> protectedBlockExecution = new Stack<ProtectedBlockExecution>();


        public class ProtectedBlockExecution
        {
            public ProtectedBlockExecution()
            {
                CurrentProtectedBlocks = new Stack<ProtectedBlock>();
            }
            public Stack<ProtectedBlock> CurrentProtectedBlocks { get; set; }
            public Exception CurrentException { get; set; }
        }
        private Dictionary<int, List<ProtectedBlock>> availableProtectedBlocksByOffset = new Dictionary<int, List<ProtectedBlock>>();


        private object[] locals;
        private IList<LocalVariableInfo> localVariableInfos;

        private object thisObject;

        private object[] parameters;
        private ParameterInfo[] parameterInfos;

        public ILDebugger(MethodBase mb, object thisObject, object[] parameters)
        {
            this.method = mb;
            this.thisObject = thisObject;

            // add current execution stack of protected blocks
            protectedBlockExecution.Push(new ProtectedBlockExecution());

            this.parameters = parameters;
            parameterInfos = mb.GetParameters();

            this.instructions = mb.GetILInstructions();
            curInstructionIndex = 0;


            localVariableInfos = mb.GetMethodBody().LocalVariables;
            locals = new object[localVariableInfos.Count];
            foreach (var local in localVariableInfos)
                locals[local.LocalIndex] = local.LocalType.GetDefault();



            var exceptionClauses = mb.GetMethodBody().ExceptionHandlingClauses
                                     .GroupBy(e => e.TryOffset)
                                     .ToDictionary(g => g.Key, g => g.GroupBy(ee => ee.TryOffset + ee.TryLength).ToDictionary(gg => gg.Key, gg => gg.ToList()));

            var instructionsByOffset = instructions.GroupBy(il => il.Offset)
                                                  .ToDictionary(g => g.Key, g => g.First());

            foreach (var pair in exceptionClauses.OrderBy(p => p.Key))
            {

                foreach (var spair in pair.Value.OrderByDescending(p => p.Key))
                {
                    List<ProtectedBlock> blocks;
                    if (!availableProtectedBlocksByOffset.TryGetValue(pair.Key, out blocks))
                        availableProtectedBlocksByOffset[pair.Key] = blocks = new List<ProtectedBlock>();

                    ProtectedBlock pb = new ProtectedBlock(spair.Value, instructionsByOffset);
                    blocks.Add(pb);
                }
            }

        }

        public int CurrentInstructionIndex
        {
            get { return curInstructionIndex; }
            set
            {
                curInstructionIndex = value;
            }
        }

        public ILInstruction CurrentInstruction
        {
            get
            {
                if (methodDebugger == null)
                    if (curInstructionIndex < instructions.Count)
                        return instructions[curInstructionIndex];
                    else
                        return null;
                else
                    return methodDebugger.CurrentInstruction;
            }
        }
        public MethodBase CurrentMethod
        {
            get
            {
                if (methodDebugger == null)
                    return method;
                else
                    return methodDebugger.CurrentMethod;
            }
        }

        public bool IsTopOfStack
        {
            get
            {
                return (methodDebugger == null);
            }
        }
        private ILDebugger methodDebugger;

        internal ILDebugger MethodDebugger
        {
            get { return methodDebugger; }
        }

        public bool Returned { get; set; }
        public bool HasReturnValue { get; set; }
        public object ReturnValue { get; set; }


        public enum StepEnum
        {
            StepInto,
            StepOver,
            StepOut
        }
        public void Next(StepEnum stepType)
        {
            if (Returned)
                throw new InvalidOperationException("The method has returned");

            try
            {
                if (methodDebugger != null)
                {
                    methodDebugger.Next(stepType);

                    if (methodDebugger.Returned)
                    {
                        if (methodDebugger.HasReturnValue)
                        {
                            stack.Push(new StackEntry()
                            {
                                Type = methodDebugger.Method.GetReturnType(),
                                Value = methodDebugger.ReturnValue
                            });
                        }
                        methodDebugger = null;
                    }
                }
                else
                {
                    List<ProtectedBlock> enteringInProtectedBlocks;
                    if (availableProtectedBlocksByOffset.TryGetValue(curInstructionIndex, out enteringInProtectedBlocks))
                    {
                        var curPbExecution = protectedBlockExecution.Peek();
                        var pbs = curPbExecution.CurrentProtectedBlocks;
                        foreach (var pb in enteringInProtectedBlocks)
                            pbs.Push(pb);
                    }

                    ILInstruction instruction = instructions[curInstructionIndex];

                    if (DoNop(instruction)) { }
                    else if (DoPop(instruction)) { }
                    else if (DoArithmicOperator(instruction)) { }
                    else if (DoBooleanOperator(instruction)) { }
                    else if (DoShiftOperator(instruction)) { }

                    else if (DoLoadObj(instruction)) { }
                    else if (DoIsInstance(instruction)) { }

                    else if (DoLoadArgument(instruction)) { }
                    else if (DoStoreArgument(instruction)) { }

                    else if (DoLoadIndirect(instruction)) { }
                    else if (DoStoreIndirect(instruction)) { }

                    else if (DoLoadLocal(instruction)) { }
                    else if (DoStoreLocal(instruction)) { }

                    else if (DoLoadField(instruction)) { }
                    else if (DoStoreField(instruction)) { }

                    else if (DoLoadElement(instruction)) { }
                    else if (DoStoreElement(instruction)) { }

                    else if (DoConversion(instruction)) { }

                    else if (DoLoadConstant(instruction)) { }

                    else if (DoBranching(instruction)) { }
                    else if (DoCompare(instruction)) { }

                    else if (DoCall(instruction, stepType)) { }
                    else if (DoNewObj(instruction, stepType)) { }

                    else if (DoNewArr(instruction)) { }
                    else if (DoLength(instruction)) { }

                    else if (DoDuplication(instruction)) { }

                    else if (DoCastClass(instruction)) { }

                    else if (DoReturn(instruction)) { }

                    else if (DoVolatile(instruction)) { }

                    else if (DoBoxing(instruction)) { }

                    else if (DoThrow(instruction)) { }
                    else if (DoRethrow(instruction)) { }
                    else if (DoLeave(instruction)) { }
                    else
                        throw new DebuggerInstructionNotImplementedException("Opcode '" + instruction.Code + "' is not implemented yet");

                    curInstructionIndex++;
                }
            }
            catch (Exception ex)
            {
                // check if there is an exception handler for the current instruction
                // todo!
                if (ex is DebuggerInstructionNotImplementedException) // debugger can't handle
                    throw;
                else
                {
                    HandleException(ex);
                    // no protected block handles it
                }
            }

        }

        private void HandleException(Exception ex)
        {
            // check the protected blocks
            var currentExecutionProtectedBlock = protectedBlockExecution.Peek();

            if (currentExecutionProtectedBlock.CurrentProtectedBlocks.Count > 0)
            {
                var pb = currentExecutionProtectedBlock.CurrentProtectedBlocks.Peek();
                ProtectedBlock.CatchBlock catchBlock = pb.CatchBlocks.Where(cb => cb.ExceptionType.IsInstanceOfType(ex)).FirstOrDefault();
                if (catchBlock != null)
                {
                    // jump to catch block
                    // push new stack of protected blocks
                    protectedBlockExecution.Push(new ProtectedBlockExecution() { CurrentException = ex });
                    CurrentInstructionIndex = catchBlock.Start.Offset;
                }
            }
        }

        private bool DoThrow(ILInstruction instruction)
        {
            if (instruction.Code == OpCodes.Throw)
            {
                var entry = stack.Pop();
                throw (Exception)entry.Value;
            }

            return false;
        }

        private bool DoRethrow(ILInstruction instruction)
        {
            if (instruction.Code == OpCodes.Rethrow)
            {
                var currentExecutionProtectedBlock = protectedBlockExecution.Peek();
                // TODO directly go to the handle because this will wipe the call stack
                // thus not emulating the behaviour 100%
                throw currentExecutionProtectedBlock.CurrentException;
            }

            return false;
        }

        private bool DoLeave(ILInstruction instruction)
        {
            if (instruction.Code == OpCodes.Leave ||instruction.Code == OpCodes.Leave_S)
            {

                int oldInstructionIndex = curInstructionIndex;

                int targetOffset = Convert.ToInt32(instruction.Operand);
                ILInstruction targetInstruction = instructions.Where(il => il.Offset == targetOffset).FirstOrDefault();
                curInstructionIndex = instructions.IndexOf(targetInstruction);
                curInstructionIndex--;

                protectedBlockExecution.Pop(); // jump out of protected block
                var curExecutionContext = protectedBlockExecution.Peek();
                curExecutionContext.CurrentProtectedBlocks.Pop(); // done with the handler


                if (curExecutionContext.CurrentProtectedBlocks.Count > 0)
                {
                    // check if there are finallies to be executed

                }    

                
                return true;
            }

            return false;
        }

        private bool DoBoxing(ILInstruction instruction)
        {
            if (instruction.Code == OpCodes.Box)
            {
                return true;
            }
            else if (instruction.Code == OpCodes.Unbox)
            {
                return true;
            }

            return false;
        }

        private bool DoNop(ILInstruction instruction)
        {
            if (instruction.Code == OpCodes.Nop)
                return true;

            return false;
        }

        private bool DoPop(ILInstruction instruction)
        {
            if (instruction.Code == OpCodes.Pop)
            {
                stack.Pop(); // bye stack value
                return true;
            }

            return false;
        }

        private bool DoIsInstance(ILInstruction instruction)
        {
            if (instruction.Code == OpCodes.Isinst)
            {
                var entry = stack.Pop();

                if (entry.Value == null)
                {
                    stack.Push(new StackEntry()
                    {
                        Value = null,
                        Type = null,
                    });
                }
                else
                {
                    if (entry.Value.GetType().IsInstanceOfType((Type)instruction.Operand))
                    {
                        stack.Push(new StackEntry()
                        {
                            Value = entry.Value,
                            Type = (Type)instruction.Operand,
                        });
                    }
                    else
                    {
                        stack.Push(new StackEntry()
                        {
                            Value = null,
                            Type = null,
                        });
                    }
                }

                return true;
            }

            return false;
        }


        private bool DoVolatile(ILInstruction instruction)
        {
            // is there actually something that we have to do here?
            // the debugger isn't really made for multithreaded debugging, there's no way to even debug on a seperate
            // thread without being an actual debugger, so I'm going with no for now
            if (instruction.Code == OpCodes.Volatile)
                return true;

            return false;
        }

        private bool DoCastClass(ILInstruction instruction)
        {
            if (instruction.Code == OpCodes.Castclass)
            {
                var entry = stack.Pop();
                if (entry.Value == null)
                {
                    stack.Push(new StackEntry() { ByRef = false, Value = null, Type = null });
                    return true;
                }
                else
                {
                    Type targetType = (Type)instruction.Operand;
                    stack.Push(new StackEntry() { ByRef = false, Value = Cast(targetType, entry.Value), Type = targetType });
                }
                return true;
            }
            return false;
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

        private bool DoNewArr(ILInstruction instruction)
        {
            if (instruction.Code == OpCodes.Newarr)
            {
                var nrOfElements = stack.Pop();
                Type t = (Type)instruction.Operand;
                object array = Array.CreateInstance(t, Convert.ToInt32(nrOfElements.Value));

                stack.Push(new StackEntry()
                {
                    Type = array.GetType(),
                    Value = array
                });
                return true;
            }
            return false;
        }

        private bool DoLength(ILInstruction instruction)
        {
            if (instruction.Code == OpCodes.Ldlen)
            {
                var array = stack.Pop();
                Type t = (Type)instruction.Operand;

                int nrOfItems = (array.Value as Array).Length;

                stack.Push(new StackEntry()
                {
                    Type = nrOfItems.GetType(),
                    Value = nrOfItems
                });
                return true;
            }
            return false;
        }

        private bool DoDuplication(ILInstruction instruction)
        {
            if (instruction.Code == OpCodes.Dup)
            {
                var value = stack.Pop();

                var duplicate = new StackEntry()
                {
                    Type = value.Type,
                    Value = value.Value,
                    ByRef = value.ByRef
                };

                stack.Push(value);
                stack.Push(duplicate);
                return true;
            }
            return false;
        }

        private bool DoBooleanOperator(ILInstruction instruction)
        {
            if (instruction.Code == OpCodes.And || instruction.Code == OpCodes.Or)
            {
                var right = stack.Pop();
                var left = stack.Pop();

                var func = CreateArithmicOpCodeFunc(instruction.Code, left.Type, right.Type);
                object result = func.Invoke(null, new object[] { left.Value, right.Value });
                stack.Push(new StackEntry()
                {
                    Value = result,
                    Type = func.ReturnType
                });

                return true;
            }

            return false;
        }

        private bool DoShiftOperator(ILInstruction instruction)
        {
            if (instruction.Code == OpCodes.Shr || instruction.Code == OpCodes.Shr_Un ||
                instruction.Code == OpCodes.Shl)
            {
                var right = stack.Pop();
                var left = stack.Pop();

                var func = CreateArithmicOpCodeFunc(instruction.Code, left.Type, right.Type);
                object result = func.Invoke(null, new object[] { left.Value, right.Value });
                stack.Push(new StackEntry()
                {
                    Value = result,
                    Type = func.ReturnType
                });

                return true;
            }

            return false;
        }

        private bool DoArithmicOperator(ILInstruction instruction)
        {
            if (instruction.Code == OpCodes.Add || instruction.Code == OpCodes.Add_Ovf || instruction.Code == OpCodes.Add_Ovf_Un ||
                instruction.Code == OpCodes.Sub || instruction.Code == OpCodes.Sub_Ovf || instruction.Code == OpCodes.Sub_Ovf_Un ||
                instruction.Code == OpCodes.Mul || instruction.Code == OpCodes.Mul_Ovf || instruction.Code == OpCodes.Mul_Ovf_Un ||
                instruction.Code == OpCodes.Div || instruction.Code == OpCodes.Div_Un)
            {
                var right = stack.Pop();
                var left = stack.Pop();

                var func = CreateArithmicOpCodeFunc(instruction.Code, left.Type, right.Type);
                object result = func.Invoke(null, new object[] { left.Value, right.Value });
                stack.Push(new StackEntry()
                {
                    Value = result,
                    Type = func.ReturnType
                });

                return true;
            }

            return false;
        }

        private bool DoLoadArgument(ILInstruction instruction)
        {
            bool isLoadArgument = false;
            int paramIdx = 0;
            bool isByRef = false;
            if (instruction.Code == OpCodes.Ldarg || instruction.Code == OpCodes.Ldarg_S)
            {
                paramIdx = Convert.ToInt32(instruction.Operand);
                isLoadArgument = true;
            }
            else if (instruction.Code == OpCodes.Ldarga || instruction.Code == OpCodes.Ldarga_S)
            {
                paramIdx = Convert.ToInt32(instruction.Operand);
                isLoadArgument = true;
                isByRef = true;
            }
            else if (instruction.Code == OpCodes.Ldarg_0)
            {
                paramIdx = 0;
                isLoadArgument = true;
            }
            else if (instruction.Code == OpCodes.Ldarg_1)
            {
                paramIdx = 1;
                isLoadArgument = true;
            }
            else if (instruction.Code == OpCodes.Ldarg_2)
            {
                paramIdx = 2;
                isLoadArgument = true;
            }
            else if (instruction.Code == OpCodes.Ldarg_3)
            {
                paramIdx = 3;
                isLoadArgument = true;
            }

            if (isLoadArgument)
            {
                if (method.IsStatic)
                {
                    stack.Push(new StackEntry()
                    {
                        Type = parameterInfos[paramIdx].ParameterType,
                        Value = parameters[paramIdx],
                        ByRef = isByRef
                    });
                }
                else
                {
                    if (paramIdx == 0)
                    {
                        stack.Push(new StackEntry()
                        {
                            Type = thisObject.GetType(),
                            Value = thisObject,
                            ByRef = isByRef
                        });
                    }
                    else
                    {
                        stack.Push(new StackEntry()
                        {
                            Type = parameterInfos[paramIdx - 1].ParameterType,
                            Value = parameters[paramIdx - 1],
                            ByRef = isByRef
                        });
                    }
                }
                return true;
            }
            return false;
        }

        private bool DoStoreArgument(ILInstruction instruction)
        {
            bool isStoreArgument = false;
            int paramIdx = 0;

            if (instruction.Code == OpCodes.Starg || instruction.Code == OpCodes.Starg_S)
            {
                if (method.IsStatic)
                {
                    paramIdx = Convert.ToInt32(instruction.Operand);
                    isStoreArgument = true;
                }
                else
                {
                    paramIdx = Convert.ToInt32(instruction.Operand) - 1;
                    isStoreArgument = true;
                }
            }

            if (isStoreArgument)
            {
                var entry = stack.Pop();

                parameters[paramIdx] = entry.Value;
                return true;
            }
            return false;
        }

        private bool DoLoadObj(ILInstruction instruction)
        {
            if (instruction.Code == OpCodes.Ldobj)
            {
                var entry = stack.Pop();
                //if (entry.ByRef)
                //{
                stack.Push(new StackEntry()
                {
                    Type = entry.Type,
                    Value = entry.Value,
                    ByRef = false
                });
                //}
                //else
                //    throw new Exception("Can't load an object from the current value on the stack. It was not passed by reference");

                return true;
            }

            return false;
        }


        private bool DoLoadLocal(ILInstruction instruction)
        {
            bool isLoadLocal = false;
            int localIdx = 0;

            if (instruction.Code == OpCodes.Ldloc || instruction.Code == OpCodes.Ldloc_S || instruction.Code == OpCodes.Ldloca || instruction.Code == OpCodes.Ldloca_S)
            {
                localIdx = Convert.ToInt32(instruction.Operand);
                isLoadLocal = true;
            }
            else if (instruction.Code == OpCodes.Ldloc_0)
            {
                localIdx = 0;
                isLoadLocal = true;
            }
            else if (instruction.Code == OpCodes.Ldloc_1)
            {
                localIdx = 1;
                isLoadLocal = true;
            }
            else if (instruction.Code == OpCodes.Ldloc_2)
            {
                localIdx = 2;
                isLoadLocal = true;
            }
            else if (instruction.Code == OpCodes.Ldloc_3)
            {
                localIdx = 3;
                isLoadLocal = true;
            }

            if (isLoadLocal)
            {
                stack.Push(new StackEntry()
                {
                    Type = localVariableInfos[localIdx].LocalType,
                    Value = locals[localIdx],
                    ByRef = instruction.Code == OpCodes.Ldloca || instruction.Code == OpCodes.Ldloca_S
                });
                return true;
            }
            return false;
        }


        private bool DoLoadIndirect(ILInstruction instruction)
        {
            if (instruction.Code == OpCodes.Ldind_I || instruction.Code == OpCodes.Ldind_I1 || instruction.Code == OpCodes.Ldind_I2 || instruction.Code == OpCodes.Ldind_I4 ||
                instruction.Code == OpCodes.Ldind_I8 || instruction.Code == OpCodes.Ldind_R4 || instruction.Code == OpCodes.Ldind_R8 || instruction.Code == OpCodes.Ldind_Ref ||
                instruction.Code == OpCodes.Ldind_U1 || instruction.Code == OpCodes.Ldind_U2 || instruction.Code == OpCodes.Ldind_U4)
            {
                var addressStack = stack.Pop();
                stack.Push(new StackEntry()
                {
                    Value = addressStack.Value,
                    Type = addressStack.Type,
                    ByRef = false,
                });

                return true;
            }

            return false;
        }

        private bool DoStoreIndirect(ILInstruction instruction)
        {
            if (instruction.Code == OpCodes.Stind_I || instruction.Code == OpCodes.Stind_I1 || instruction.Code == OpCodes.Stind_I2 || instruction.Code == OpCodes.Ldind_I4 ||
                instruction.Code == OpCodes.Stind_I8 || instruction.Code == OpCodes.Stind_R4 || instruction.Code == OpCodes.Stind_R8 || instruction.Code == OpCodes.Stind_Ref)
            {
                var addressStack = stack.Pop();
                stack.Push(new StackEntry()
                {
                    Value = addressStack.Value,
                    Type = addressStack.Type,
                    ByRef = true,
                });

                return true;
            }

            return false;
        }

        private bool DoStoreLocal(ILInstruction instruction)
        {
            bool isStoreLocal = false;
            int localIdx = 0;

            if (instruction.Code == OpCodes.Stloc || instruction.Code == OpCodes.Stloc_S)
            {
                localIdx = Convert.ToInt32(instruction.Operand);
                isStoreLocal = true;
            }
            else if (instruction.Code == OpCodes.Stloc_0)
            {
                localIdx = 0;
                isStoreLocal = true;
            }
            else if (instruction.Code == OpCodes.Stloc_1)
            {
                localIdx = 1;
                isStoreLocal = true;
            }
            else if (instruction.Code == OpCodes.Stloc_2)
            {
                localIdx = 2;
                isStoreLocal = true;
            }
            else if (instruction.Code == OpCodes.Stloc_3)
            {
                localIdx = 3;
                isStoreLocal = true;
            }

            if (isStoreLocal)
            {
                var entry = stack.Pop();
                locals[localIdx] = entry.Value;
                return true;
            }
            return false;
        }

        private bool DoLoadField(ILInstruction instruction)
        {
            if (instruction.Code == OpCodes.Ldfld || instruction.Code == OpCodes.Ldflda)
            {
                FieldInfo fld = (FieldInfo)instruction.Operand;

                var fieldOwner = stack.Pop();

                var value = fld.GetValue(fieldOwner.Value);

                stack.Push(new StackEntry()
                {
                    Type = fld.FieldType,
                    Value = value
                });
                return true;
            }
            else if (instruction.Code == OpCodes.Ldsfld || instruction.Code == OpCodes.Ldsflda)
            {
                FieldInfo fld = (FieldInfo)instruction.Operand;
                var value = fld.GetValue(null);

                stack.Push(new StackEntry()
                {
                    Type = fld.FieldType,
                    Value = value
                });
                return true;
            }

            return false;
        }

        private bool DoStoreField(ILInstruction instruction)
        {
            if (instruction.Code == OpCodes.Stfld)
            {
                FieldInfo fld = (FieldInfo)instruction.Operand;

                var newValue = stack.Pop();
                var fieldOwner = stack.Pop();

                object value = newValue.Value;
                if (fld.FieldType == typeof(bool))
                    value = Convert.ToBoolean(value);
                else if (fld.FieldType == typeof(char))
                    value = Convert.ToChar(value);

                fld.SetValue(fieldOwner.Value, value);
                return true;
            }
            else if (instruction.Code == OpCodes.Stsfld)
            {
                FieldInfo fld = (FieldInfo)instruction.Operand;
                var newValue = stack.Pop();


                object value = newValue.Value;
                if (fld.FieldType == typeof(bool))
                    value = Convert.ToBoolean(value);
                else if (fld.FieldType == typeof(char))
                    value = Convert.ToChar(value);

                fld.SetValue(null, value);
                return true;
            }

            return false;
        }


        private bool DoLoadElement(ILInstruction instruction)
        {

            if (instruction.Code == OpCodes.Ldelem || instruction.Code == OpCodes.Ldelem_I ||
                instruction.Code == OpCodes.Ldelem_I1 || instruction.Code == OpCodes.Ldelem_I2 ||
                instruction.Code == OpCodes.Ldelem_I4 || instruction.Code == OpCodes.Ldelem_I8 ||
                instruction.Code == OpCodes.Ldelem_R4 || instruction.Code == OpCodes.Ldelem_R8 ||
                instruction.Code == OpCodes.Ldelem_Ref || instruction.Code == OpCodes.Ldelem_U1 ||
                instruction.Code == OpCodes.Ldelem_U2 || instruction.Code == OpCodes.Ldelem_U4 ||
                instruction.Code == OpCodes.Ldelema)
            {
                var index = stack.Pop();
                var array = stack.Pop();

                var value = (array.Value as Array).GetValue((int)index.Value);

                //var func = CreateLoadElementOpCodeFunc(instruction.Code, instruction.Operand, array.Type, index.Type);

                //var value = func.Invoke(null, new object[] { array.Value, index.Value });

                //var value = ((Array)array.Value).GetValue(Convert.ToInt64(index));

                stack.Push(new StackEntry()
                {
                    Value = value,
                    Type = value == null ? ((Array)array.Value).GetType().GetElementType() : value.GetType(),
                    ByRef = (instruction.Code == OpCodes.Ldelema)
                });

                return true;
            }
            return false;
        }

        private bool DoStoreElement(ILInstruction instruction)
        {
            if (instruction.Code == OpCodes.Stelem || instruction.Code == OpCodes.Stelem_I ||
                instruction.Code == OpCodes.Stelem_I1 || instruction.Code == OpCodes.Stelem_I2 ||
                instruction.Code == OpCodes.Stelem_I4 || instruction.Code == OpCodes.Stelem_I8 ||
                instruction.Code == OpCodes.Stelem_R4 || instruction.Code == OpCodes.Stelem_R8 ||
                instruction.Code == OpCodes.Stelem_Ref)
            {
                var objValue = stack.Pop();
                var index = stack.Pop();
                var array = stack.Pop();

                object value = objValue.Value;

                if (instruction.Code == OpCodes.Stelem_I)
                    value = Convert.ToInt32(value);
                else if (instruction.Code == OpCodes.Stelem_I1)
                    value = Convert.ToByte(value);
                else if (instruction.Code == OpCodes.Stelem_I2)
                    value = Convert.ToInt16(value);
                else if (instruction.Code == OpCodes.Stelem_I4)
                    value = Convert.ToInt32(value);
                else if (instruction.Code == OpCodes.Stelem_I8)
                    value = Convert.ToInt64(value);
                else if (instruction.Code == OpCodes.Stelem_R4)
                    value = Convert.ToSingle(value);
                else if (instruction.Code == OpCodes.Stelem_R8)
                    value = Convert.ToDouble(value);

                if (((Array)array.Value).GetType().GetElementType() == typeof(bool))
                    value = Convert.ToBoolean(value);
                else if (((Array)array.Value).GetType().GetElementType() == typeof(char))
                    value = Convert.ToChar(value);

                (array.Value as Array).SetValue(value, (int)index.Value);

                //var func = CreateStoreElementOpCodeFunc(instruction.Code, instruction.Operand, array.Type, index.Type);

                //var value = func.Invoke(null, new object[] { array.Value, index.Value });

                ////var value = ((Array)array.Value).GetValue(Convert.ToInt64(index));

                //stack.Push(new StackEntry()
                //{
                //    Value = value,
                //    Type = func.ReturnType
                //});

                return true;
            }

            return false;
        }

        private bool DoCompare(ILInstruction instruction)
        {

            if (instruction.Code == OpCodes.Ceq ||
                instruction.Code == OpCodes.Clt || instruction.Code == OpCodes.Clt_Un ||
                instruction.Code == OpCodes.Cgt || instruction.Code == OpCodes.Cgt_Un)
            {
                var right = stack.Pop();
                var left = stack.Pop();

                var func = CreateBooleanOpCodeFunc(instruction.Code, left.Type, left.ByRef, right.Type, right.ByRef);
                object result = func.Invoke(null, new object[] { left.Value, right.Value });

                stack.Push(new StackEntry()
                {
                    Value = result,
                    Type = result.GetType(),
                });

                return true;
            }

            return false;
        }


        private bool DoBranching(ILInstruction instruction)
        {
            bool doJump = false;
            bool isBranch = false;
            if (instruction.Code == OpCodes.Br || instruction.Code == OpCodes.Br_S)
            {
                doJump = true;
                isBranch = true;
            }
            else if (instruction.Code == OpCodes.Brtrue || instruction.Code == OpCodes.Brtrue_S)
            {
                var entry = stack.Pop();
                if (entry.Value == null)
                    doJump = false;
                else
                {
                    if (!entry.Type.IsPrimitive)
                    {
                        // type o, if obj != null then jump
                        doJump = true;
                    }
                    else
                    {
                        // primitive
                        doJump = Convert.ToInt32(entry.Value) != 0;
                    }
                }

                isBranch = true;
            }
            else if (instruction.Code == OpCodes.Brfalse || instruction.Code == OpCodes.Brfalse_S)
            {
                var entry = stack.Pop();
                if (entry.Value == null)
                {
                    doJump = true;
                }
                else
                {
                    if (!entry.Type.IsPrimitive)
                    {
                        // type o, if(!(obj != null)) then jump
                        doJump = false;
                    }
                    else
                    {
                        doJump = Convert.ToInt32(entry.Value) == 0;
                    }
                }

                isBranch = true;
            }
            else if (instruction.Code == OpCodes.Beq || instruction.Code == OpCodes.Beq_S)
            {
                var right = stack.Pop();
                var left = stack.Pop();

                var func = CreateBooleanOpCodeFunc(OpCodes.Ceq, left.Type, left.ByRef, right.Type, right.ByRef);
                bool result = (bool)func.Invoke(null, new object[] { left.Value, right.Value });
                doJump = result;
                isBranch = true;
            }
            else if (instruction.Code == OpCodes.Bge || instruction.Code == OpCodes.Bge_S)
            {
                var right = stack.Pop();
                var left = stack.Pop();

                var func = CreateBooleanOpCodeFunc(OpCodes.Clt, left.Type, left.ByRef, right.Type, right.ByRef);
                bool result = (bool)func.Invoke(null, new object[] { left.Value, right.Value });
                doJump = !result;
                isBranch = true;
            }
            else if (instruction.Code == OpCodes.Bge_Un || instruction.Code == OpCodes.Bge_Un_S)
            {
                var right = stack.Pop();
                var left = stack.Pop();

                var func = CreateBooleanOpCodeFunc(OpCodes.Clt_Un, left.Type, left.ByRef, right.Type, right.ByRef);
                bool result = (bool)func.Invoke(null, new object[] { left.Value, right.Value });
                doJump = !result;
                isBranch = true;
            }
            else if (instruction.Code == OpCodes.Bgt || instruction.Code == OpCodes.Bgt_S)
            {
                var right = stack.Pop();
                var left = stack.Pop();

                var func = CreateBooleanOpCodeFunc(OpCodes.Cgt, left.Type, left.ByRef, right.Type, right.ByRef);
                bool result = (bool)func.Invoke(null, new object[] { left.Value, right.Value });
                doJump = result;
                isBranch = true;
            }
            else if (instruction.Code == OpCodes.Bgt_Un || instruction.Code == OpCodes.Bgt_Un_S)
            {
                var right = stack.Pop();
                var left = stack.Pop();

                var func = CreateBooleanOpCodeFunc(OpCodes.Cgt_Un, left.Type, left.ByRef, right.Type, right.ByRef);
                bool result = (bool)func.Invoke(null, new object[] { left.Value, right.Value });
                doJump = result;
                isBranch = true;
            }
            else if (instruction.Code == OpCodes.Ble || instruction.Code == OpCodes.Ble_S)
            {
                var right = stack.Pop();
                var left = stack.Pop();

                var func = CreateBooleanOpCodeFunc(OpCodes.Cgt, left.Type, left.ByRef, right.Type, right.ByRef);
                bool result = (bool)func.Invoke(null, new object[] { left.Value, right.Value });
                doJump = !result;
                isBranch = true;
            }
            else if (instruction.Code == OpCodes.Ble_Un || instruction.Code == OpCodes.Ble_Un_S)
            {
                var right = stack.Pop();
                var left = stack.Pop();

                var func = CreateBooleanOpCodeFunc(OpCodes.Cgt_Un, left.Type, left.ByRef, right.Type, right.ByRef);
                bool result = (bool)func.Invoke(null, new object[] { left.Value, right.Value });
                doJump = !result;
                isBranch = true;
            }
            else if (instruction.Code == OpCodes.Blt || instruction.Code == OpCodes.Blt_S)
            {
                var right = stack.Pop();
                var left = stack.Pop();

                var func = CreateBooleanOpCodeFunc(OpCodes.Clt, left.Type, left.ByRef, right.Type, right.ByRef);
                bool result = (bool)func.Invoke(null, new object[] { left.Value, right.Value });
                doJump = result;
                isBranch = true;
            }
            else if (instruction.Code == OpCodes.Blt_Un || instruction.Code == OpCodes.Blt_Un_S)
            {
                var right = stack.Pop();
                var left = stack.Pop();

                var func = CreateBooleanOpCodeFunc(OpCodes.Clt_Un, left.Type, left.ByRef, right.Type, right.ByRef);
                bool result = (bool)func.Invoke(null, new object[] { left.Value, right.Value });
                doJump = result;
                isBranch = true;
            }
            else if (instruction.Code == OpCodes.Bne_Un || instruction.Code == OpCodes.Bne_Un_S)
            {
                var right = stack.Pop();
                var left = stack.Pop();

                var func = CreateBooleanOpCodeFunc(OpCodes.Ceq, left.Type, left.ByRef, right.Type, right.ByRef);
                bool result = (bool)func.Invoke(null, new object[] { left.Value, right.Value });
                doJump = !result;
                isBranch = true;
            }

            if (isBranch)
            {
                if (doJump)
                {
                    int targetOffset = Convert.ToInt32(instruction.Operand);
                    ILInstruction targetInstruction = instructions.Where(il => il.Offset == targetOffset).FirstOrDefault();
                    curInstructionIndex = instructions.IndexOf(targetInstruction);

                    curInstructionIndex--;
                }
                return true;
            }

            return false;
        }

        private bool DoReturn(ILInstruction instruction)
        {
            if (instruction.Code == OpCodes.Ret)
            {
                Returned = true;
                if (stack.Count > 0)
                {
                    var val = stack.Pop();
                    ReturnValue = val.Value;
                    HasReturnValue = true;
                }
                else
                    HasReturnValue = false;

                return true;
            }

            return false;
        }

        private bool DoConversion(ILInstruction instruction)
        {
            OpCode code = instruction.Code;

            if (code == OpCodes.Conv_I || code == OpCodes.Conv_Ovf_I || code == OpCodes.Conv_Ovf_I_Un
                ||
                code == OpCodes.Conv_I1 || code == OpCodes.Conv_Ovf_I1 || code == OpCodes.Conv_Ovf_I1_Un ||
                code == OpCodes.Conv_I2 || code == OpCodes.Conv_Ovf_I2 || code == OpCodes.Conv_Ovf_I2_Un ||
                code == OpCodes.Conv_I4 || code == OpCodes.Conv_Ovf_I4 || code == OpCodes.Conv_Ovf_I4_Un
                ||
                code == OpCodes.Conv_I8 || code == OpCodes.Conv_Ovf_I8 || code == OpCodes.Conv_Ovf_I8_Un
                ||
                code == OpCodes.Conv_U || code == OpCodes.Conv_Ovf_U || code == OpCodes.Conv_Ovf_U_Un
                ||
                code == OpCodes.Conv_U1 || code == OpCodes.Conv_Ovf_U1 || code == OpCodes.Conv_Ovf_U1_Un ||
                code == OpCodes.Conv_U2 || code == OpCodes.Conv_Ovf_U2 || code == OpCodes.Conv_Ovf_U2_Un ||
                code == OpCodes.Conv_U4 || code == OpCodes.Conv_Ovf_U4 || code == OpCodes.Conv_Ovf_U4_Un
                ||
                code == OpCodes.Conv_U8 || code == OpCodes.Conv_Ovf_U8 || code == OpCodes.Conv_Ovf_U8_Un
                ||
                code == OpCodes.Conv_R_Un || code == OpCodes.Conv_R4
                ||
                code == OpCodes.Conv_R8)
            {
                var right = stack.Pop();

                object result;

                //if (code == OpCodes.Conv_U)
                //{
                //    result = new UIntPtr(Convert.ToUInt32(right.Value));
                //}
                //else

                var func = CreateConvOpCodeFunc(instruction.Code, right.Type);
                result = func.Invoke(null, new object[] { right.Value });
                stack.Push(new StackEntry()
                {
                    Value = result,
                    Type = func.ReturnType
                });

                return true;
            }
            return false;
        }
        private bool DoCall(ILInstruction instruction, StepEnum stepType)
        {
            if (instruction.Code == OpCodes.Call || instruction.Code == OpCodes.Callvirt)
            {
                MethodBase mb = (MethodBase)instruction.Operand;

                var instructions = mb.GetILInstructions();

                List<object> parameters = new List<object>();
                foreach (var p in mb.GetParameters())
                {
                    var paramValue = stack.Pop();
                    parameters.Insert(0, paramValue.Value);
                }

                object thisObject;
                if (mb.IsStatic)
                    thisObject = null;
                else
                    thisObject = stack.Pop().Value;

                ExecuteMethod(mb, instructions, parameters, thisObject, instruction.Code == OpCodes.Callvirt);
                return true;
            }

            return false;
        }

        private void ExecuteMethod(MethodBase mb, List<ILInstruction> instructions, List<object> parameters, object thisObject, bool isCallVirt)
        {
            if (instructions != null && instructions.Count > 0 && !mb.IsUnsafe())
            {
                try
                {
                    // if it's a virtual method and the method is called by callvirt, evaluate the most overriden
                    // method first
                    if (mb.IsVirtual && mb is MethodInfo && isCallVirt)
                    {
                        MethodInfo m = (MethodInfo)mb;
                        // needs to invoke the overriden method !
                        var topLevelBaseDefinition = m.GetTopLevelBaseDefinition();


                        MethodInfo correspondingMethodInType = null;
                        Type curType = thisObject.GetType();
                        while (correspondingMethodInType == null)
                        {
                            correspondingMethodInType = curType.GetMethodsOfType(false, false, true).Where(me => me.GetTopLevelBaseDefinition() == topLevelBaseDefinition).FirstOrDefault();
                            curType = curType.BaseType;
                        }

                        methodDebugger = new ILDebugger(correspondingMethodInType, thisObject, parameters.ToArray());
                    }
                    else
                        methodDebugger = new ILDebugger(mb, thisObject, parameters.ToArray());

                }
                catch (BadImageFormatException)
                {
                    // some references in the method are not CLR compliant
                    // invoke with reflection
                    ExecuteMethodWithReflection(mb, parameters, thisObject);
                }
            }
            else
            {
                ExecuteMethodWithReflection(mb, parameters, thisObject);
            }
        }

        private void ExecuteMethodWithReflection(MethodBase mb, List<object> parameters, object thisObject)
        {
            var paramInfoOfMethod = mb.GetParameters();
            // in IL an int is a bool or char, but in c# this conversion will fail
            object[] convertedParameters = new object[parameters.Count];
            for (int i = 0; i < parameters.Count; i++)
            {
                if (paramInfoOfMethod[i].ParameterType == typeof(bool))
                    convertedParameters[i] = Convert.ToBoolean(parameters[i]);
                else if (paramInfoOfMethod[i].ParameterType == typeof(char))
                    convertedParameters[i] = Convert.ToChar(parameters[i]);
                else
                    convertedParameters[i] = parameters[i];
            }

            object value = mb.Invoke(mb.IsStatic ? null : thisObject, convertedParameters.ToArray());
            if (mb.GetReturnType() != typeof(void) && mb.GetReturnType() != null)
            {
                stack.Push(new StackEntry()
                {
                    Type = mb.GetReturnType(),
                    Value = value
                });
            }
        }

        private bool DoNewObj(ILInstruction instruction, StepEnum stepType)
        {
            if (instruction.Code == OpCodes.Newobj)
            {
                ConstructorInfo ctor = (ConstructorInfo)instruction.Operand;
                object instance = System.Runtime.Serialization.FormatterServices.GetUninitializedObject(ctor.DeclaringType);



                MethodBase mb = (MethodBase)instruction.Operand;

                var instructions = mb.GetILInstructions();

                // pop parameters from stack
                List<object> parameters = new List<object>();
                foreach (var p in mb.GetParameters())
                {
                    var paramValue = stack.Pop();
                    parameters.Insert(0, paramValue.Value);
                }

                // push new obj on the stack
                stack.Push(new StackEntry()
                {
                    Type = ctor.DeclaringType,
                    Value = instance
                });

                object thisObject;
                if (mb.IsStatic)
                    thisObject = null;
                else
                    thisObject = instance;

                ExecuteMethod(mb, instructions, parameters, thisObject, instruction.Code == OpCodes.Callvirt);
                return true;
            }
            return false;
        }


        private bool DoLoadConstant(ILInstruction instruction)
        {
            object value = null;
            Type type = null;
            bool isLoadConstant = false;

            if (instruction.Code == OpCodes.Ldstr)
            {
                value = instruction.Operand;
                type = typeof(string);
                isLoadConstant = true;
            }
            else if (instruction.Code == OpCodes.Ldtoken)
            {
                value = instruction.Operand;
                type = typeof(Type);
                isLoadConstant = true;
            }
            else if (instruction.Code == OpCodes.Ldc_I4 || instruction.Code == OpCodes.Ldc_I4_S)
            {
                value = instruction.Operand;
                type = typeof(Int32);
                isLoadConstant = true;
            }
            else if (instruction.Code == OpCodes.Ldc_R4)
            {
                value = instruction.Operand;
                type = typeof(float);
                isLoadConstant = true;
            }
            else if (instruction.Code == OpCodes.Ldc_I8)
            {
                value = instruction.Operand;
                type = typeof(long);
                isLoadConstant = true;
            }
            else if (instruction.Code == OpCodes.Ldc_R8)
            {
                value = instruction.Operand;
                type = typeof(double);
                isLoadConstant = true;
            }
            else if (instruction.Code == OpCodes.Ldnull)
            {
                value = null;
                type = null;
                isLoadConstant = true;
            }
            else if (instruction.Code == OpCodes.Ldc_I4_0)
            {
                value = 0;
                type = typeof(Int32);
                isLoadConstant = true;
            }
            else if (instruction.Code == OpCodes.Ldc_I4_1)
            {
                value = 1;
                type = typeof(Int32);
                isLoadConstant = true;
            }
            else if (instruction.Code == OpCodes.Ldc_I4_2)
            {
                value = 2;
                type = typeof(Int32);
                isLoadConstant = true;
            }
            else if (instruction.Code == OpCodes.Ldc_I4_3)
            {
                value = 3;
                type = typeof(Int32);
                isLoadConstant = true;
            }
            else if (instruction.Code == OpCodes.Ldc_I4_4)
            {
                value = 4;
                type = typeof(Int32);
                isLoadConstant = true;
            }
            else if (instruction.Code == OpCodes.Ldc_I4_5)
            {
                value = 5;
                type = typeof(Int32);
                isLoadConstant = true;
            }
            else if (instruction.Code == OpCodes.Ldc_I4_6)
            {
                value = 6;
                type = typeof(Int32);
                isLoadConstant = true;
            }
            else if (instruction.Code == OpCodes.Ldc_I4_7)
            {
                value = 7;
                type = typeof(Int32);
                isLoadConstant = true;
            }
            else if (instruction.Code == OpCodes.Ldc_I4_8)
            {
                value = 8;
                type = typeof(Int32);
                isLoadConstant = true;
            }
            else if (instruction.Code == OpCodes.Ldc_I4_M1)
            {
                value = -1;
                type = typeof(Int32);
                isLoadConstant = true;
            }


            if (isLoadConstant)
            {
                stack.Push(new StackEntry()
                {
                    Type = type,
                    Value = value
                });
                return true;
            }
            return false;
        }
        private static DynamicMethod CreateArithmicOpCodeFunc(OpCode code, Type left, Type right)
        {
            Type resultType = left.GetMostPrecision(right);

            DynamicMethod dm = new DynamicMethod("OP_" + code.Name, resultType, new Type[] { left, right });
            ILGenerator gen = dm.GetILGenerator();
            gen.Emit(OpCodes.Ldarg_0);
            gen.Emit(OpCodes.Ldarg_1);
            gen.Emit(code);
            gen.Emit(OpCodes.Ret);
            return dm;
        }

        private static DynamicMethod CreateBooleanOpCodeFunc(OpCode code, Type left, bool leftByRef, Type right, bool rightByRef)
        {
            Type resultType = typeof(bool);

            DynamicMethod dm = new DynamicMethod("OP_" + code.Name, resultType, new Type[] { left, right });
            ILGenerator gen = dm.GetILGenerator();
            if (leftByRef)
                gen.Emit(OpCodes.Ldarga, 0);
            else
                gen.Emit(OpCodes.Ldarg_0);

            if (rightByRef)
                gen.Emit(OpCodes.Ldarga, 1);
            else
                gen.Emit(OpCodes.Ldarg_1);
            gen.Emit(code);
            gen.Emit(OpCodes.Ret);
            return dm;
        }



        private static DynamicMethod CreateLoadElementOpCodeFunc(OpCode code, object operand, Type arrayType, Type indexType)
        {
            Type returnType = null;

            if (code == OpCodes.Ldelem)
                returnType = (Type)operand;
            else if (code == OpCodes.Ldelem_I)
                returnType = typeof(IntPtr);
            else if (code == OpCodes.Ldelem_I1)
                returnType = typeof(sbyte);
            else if (code == OpCodes.Ldelem_I2)
                returnType = typeof(Int16);
            else if (code == OpCodes.Ldelem_I4)
                returnType = typeof(Int32);
            else if (code == OpCodes.Ldelem_I8)
                returnType = typeof(Int64);
            else if (code == OpCodes.Ldelem_R4)
                returnType = typeof(float);
            else if (code == OpCodes.Ldelem_R8)
                returnType = typeof(double);
            else if (code == OpCodes.Ldelem_Ref)
                returnType = typeof(IntPtr);
            else if (code == OpCodes.Ldelem_U1)
                returnType = typeof(byte);
            else if (code == OpCodes.Ldelem_U2)
                returnType = typeof(UInt16);
            else if (code == OpCodes.Ldelem_U4)
                returnType = typeof(UInt32);
            else if (code == OpCodes.Ldelema)
                returnType = typeof(IntPtr);


            DynamicMethod dm = new DynamicMethod("OP_" + code.Name, returnType, new Type[] { arrayType, indexType });
            ILGenerator gen = dm.GetILGenerator();
            gen.Emit(OpCodes.Ldarg_0);
            gen.Emit(OpCodes.Ldarg_1);
            gen.Emit(code);
            gen.Emit(OpCodes.Ret);
            return dm;
        }


        private static DynamicMethod CreateStoreElementOpCodeFunc(OpCode code, object operand, Type arrayType, Type indexType)
        {

            Type objType = null;
            if (code == OpCodes.Stelem)
                objType = (Type)operand;
            else if (code == OpCodes.Stelem_I)
                objType = typeof(IntPtr);
            else if (code == OpCodes.Stelem_I1)
                objType = typeof(byte);
            else if (code == OpCodes.Stelem_I2)
                objType = typeof(Int16);
            else if (code == OpCodes.Stelem_I4)
                objType = typeof(Int32);
            else if (code == OpCodes.Stelem_I8)
                objType = typeof(Int64);
            else if (code == OpCodes.Stelem_R4)
                objType = typeof(float);
            else if (code == OpCodes.Stelem_R8)
                objType = typeof(double);
            else if (code == OpCodes.Stelem_Ref)
                objType = typeof(IntPtr);

            DynamicMethod dm = new DynamicMethod("OP_" + code.Name, null, new Type[] { arrayType, indexType, objType });
            ILGenerator gen = dm.GetILGenerator();
            gen.Emit(OpCodes.Ldarg_0);
            gen.Emit(OpCodes.Ldarg_1);
            gen.Emit(code);
            gen.Emit(OpCodes.Ret);
            return dm;
        }


        private static DynamicMethod CreateConvOpCodeFunc(OpCode code, Type valType)
        {
            Type resultType;
            if (code == OpCodes.Conv_I || code == OpCodes.Conv_Ovf_I || code == OpCodes.Conv_Ovf_I_Un)
                resultType = typeof(IntPtr);
            else if (code == OpCodes.Conv_I1 || code == OpCodes.Conv_Ovf_I1 || code == OpCodes.Conv_Ovf_I1_Un)
                resultType = typeof(sbyte);
            else if (code == OpCodes.Conv_I2 || code == OpCodes.Conv_Ovf_I2 || code == OpCodes.Conv_Ovf_I2_Un)
                resultType = typeof(Int16);
            else if (code == OpCodes.Conv_I4 || code == OpCodes.Conv_Ovf_I4 || code == OpCodes.Conv_Ovf_I4_Un)
                resultType = typeof(Int32);
            else if (code == OpCodes.Conv_I8 || code == OpCodes.Conv_Ovf_I8 || code == OpCodes.Conv_Ovf_I8_Un)
                resultType = typeof(Int64);
            else if (code == OpCodes.Conv_U || code == OpCodes.Conv_Ovf_U || code == OpCodes.Conv_Ovf_U_Un)
                resultType = typeof(UIntPtr);
            else if (code == OpCodes.Conv_U1 || code == OpCodes.Conv_Ovf_U1 || code == OpCodes.Conv_Ovf_U1_Un)
                resultType = typeof(byte);
            else if (code == OpCodes.Conv_U2 || code == OpCodes.Conv_Ovf_U2 || code == OpCodes.Conv_Ovf_U2_Un)
                resultType = typeof(UInt16);
            else if (code == OpCodes.Conv_U4 || code == OpCodes.Conv_Ovf_U4 || code == OpCodes.Conv_Ovf_U4_Un)
                resultType = typeof(UInt32);
            else if (code == OpCodes.Conv_U8 || code == OpCodes.Conv_Ovf_U8 || code == OpCodes.Conv_Ovf_U8_Un)
                resultType = typeof(UInt64);
            else if (code == OpCodes.Conv_R_Un || code == OpCodes.Conv_R4)
                resultType = typeof(float);
            else if (code == OpCodes.Conv_R8)
                resultType = typeof(double);
            else
                resultType = null;

            DynamicMethod dm = new DynamicMethod("OP_" + code.Name, resultType, new Type[] { valType });
            ILGenerator gen = dm.GetILGenerator();
            gen.Emit(OpCodes.Ldarg_0);
            gen.Emit(code);
            gen.Emit(OpCodes.Ret);
            return dm;
        }

        public class StackEntry
        {
            public object Value { get; set; }
            public Type Type { get; set; }
            public bool ByRef { get; set; }

        }

        public class ProtectedBlock
        {
            public ExceptionHandlingClause Clause { get; set; }

            public ProtectedBlock(List<ExceptionHandlingClause> ecs, Dictionary<int, ILInstruction> instructionsByOffset)
            {
                CatchBlocks = new List<CatchBlock>();

                foreach (var ehc in ecs)
                {
                    if (ehc.Flags == ExceptionHandlingClauseOptions.Clause)
                    {
                        // actual try { } block
                        StartInstruction = instructionsByOffset[ehc.TryOffset];
                        EndInstruction = instructionsByOffset[ehc.TryOffset + ehc.TryLength];

                        // catch block
                        CatchBlocks.Add(new CatchBlock()
                        {
                            Start = instructionsByOffset[ehc.HandlerOffset],
                            End = instructionsByOffset[ehc.HandlerOffset + ehc.HandlerLength],
                            ExceptionType = ehc.CatchType
                        });
                    }
                    else if (ehc.Flags == ExceptionHandlingClauseOptions.Fault)
                    {
                        //TODO
                    }
                    else if (ehc.Flags == ExceptionHandlingClauseOptions.Filter)
                    {
                        // TODO
                    }
                    else if (ehc.Flags == ExceptionHandlingClauseOptions.Finally)
                    {
                        StartFinally = instructionsByOffset[ehc.HandlerOffset];
                        EndFinally = instructionsByOffset[ehc.HandlerOffset + ehc.HandlerLength];
                    }
                }
            }

            public ILInstruction StartInstruction { get; set; }
            public ILInstruction EndInstruction { get; set; }


            public List<CatchBlock> CatchBlocks { get; set; }

            public ILInstruction StartFinally { get; set; }
            public ILInstruction EndFinally { get; set; }


            public class CatchBlock
            {
                public Type ExceptionType { get; set; }
                public ILInstruction Start { get; set; }
                public ILInstruction End { get; set; }
            }
        }



        public class DebuggerInstructionNotImplementedException : NotImplementedException
        {
            public DebuggerInstructionNotImplementedException(string message)
                : base(message)
            {
            }
        }

    }


}
