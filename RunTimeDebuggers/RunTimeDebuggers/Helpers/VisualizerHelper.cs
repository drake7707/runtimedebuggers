using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Reflection;
using System.Reflection.Emit;
using RunTimeDebuggers.AssemblyExplorer;
using FastColoredTextBoxNS;

namespace RunTimeDebuggers.Helpers
{

    public static class VisualizerHelper
    {

        public class CodeBlock : IComparable<CodeBlock>
        {
            public CodeBlock()
            {
                Clickable = false;
            }

            public CodeBlock(string text)
                : this()
            {
                this.Text = text;
            }

            public CodeBlock(int indenting, string text)
            {
                this.Text = new string(' ', indenting) + text;
            }

            public virtual string Text { get; set; }

            public virtual object Tag { get; set; }

            public virtual Place Start { get; set; }
            public virtual Place End { get; set; }

            public int CompareTo(CodeBlock other)
            {
                if (Start <= other.Start && End > other.End) return 0;
                if (Start <= other.Start) return -1;
                return 1;
            }

            public bool Clickable { get; set; }
        }

        public class TypeCodeBlock : CodeBlock
        {
            public TypeCodeBlock(Type t)
            {
                this.Text = t.ToSignatureString();
                this.Tag = t;
                Clickable = true;
            }
        }

        public class MemberCodeBlock : CodeBlock
        {
            public MemberCodeBlock(MemberInfo t)
            {
                this.Text = t.ToSignatureString();
                this.Tag = t;
                this.Clickable = true;
            }
        }

        
        public class ValueCodeBlock : CodeBlock
        {
            public ValueCodeBlock(object value)
            {
                if (value == null)
                    this.Text = "null";
                else if (value.GetType().IsEnum)
                    this.Text = value.GetType().ToSignatureString() + "." + value;
                if (value is string)
                    this.Text = "\"" + value + "\"";
                else
                    this.Text = value + "";
                this.Tag = value;
            }
        }

        public class InstructionOffsetCodeBlock : CodeBlock
        {
            public bool IsTarget { get; set; }
            public InstructionOffsetCodeBlock(int offset, bool isTarget)
            {
                this.IsTarget = isTarget;
                this.Text = offset.ToString("x4");
                this.Tag = offset;

                if(isTarget)
                    this.Clickable = true;
            }
        }

        public class InstructionOpCodeCodeBlock : CodeBlock
        {
            public InstructionOpCodeCodeBlock(ILInstruction instruction)
            {
                string opcode = instruction.Code.ToString();
                if (opcode.Length < 10)
                    opcode = opcode + new string(' ', 10 - opcode.Length);

                this.Text = opcode;
                this.Tag = instruction;
            }
        }

        public class ParameterInfoCodeBlock : CodeBlock
        {
            public ParameterInfoCodeBlock(ParameterInfo p)
            {
                if (p == null)
                    this.Text = "(this)";
                else
                    this.Text = "(" + p.Name + ")";

                this.Tag = p;
            }
        }

        public class ErrorCodeBlock : CodeBlock
        {
            public ErrorCodeBlock(Disassembler.Error err)
            {
                this.Text = "Error reading instruction at " + err.ErrorPosition.ToString("x4") + ":  " + err.Exception.ToExceptionString();
                this.Tag = err;
            }
        }



        private static Dictionary<OpCode, string> opcodeDescriptions;

        public static string RTFHeader = @"{\rtf1\ansi\ansicpg1252\deff0\deflang2067{\fonttbl{\f0\fnil\fcharset0 Tahoma;}}{\colortbl ;\red0\green77\blue187;\red0\green0\blue255;\red163\green21\blue21;\red255\green0\blue0;}\viewkind4\uc1\pard\f0\fs17@BODY@\par}";




        public static List<CodeBlock> GetCodeBlocks(this Disassembler reader)
        {
            List<CodeBlock> blocks = new List<CodeBlock>();
            if (reader.Instructions == null)
                return blocks;

            var locals = reader.Method.GetMethodBody().LocalVariables;

            if (locals.Count > 0)
            {
                blocks.Add(new CodeBlock(".locals {" + Environment.NewLine));
                foreach (var local in locals)
                {
                    blocks.Add(new CodeBlock("    [" + local.LocalIndex + "] "));
                    blocks.Add(new TypeCodeBlock(local.LocalType));
                    blocks.Add(new CodeBlock(Environment.NewLine));
                }
                blocks.Add(new CodeBlock("}" + Environment.NewLine));
            }

            var exceptionClauses = reader.Method.GetMethodBody().ExceptionHandlingClauses;

            var allTries = exceptionClauses.GroupBy(ex => ex.TryOffset + "_" + ex.TryLength)
                                                 .ToDictionary(g => g.Key, g => g.Last());

            // sort by trylength descending to define the outer try first!
            var tryClausesByOffset = allTries.Values.GroupBy(t => t.TryOffset)
                                                .ToDictionary(g => g.Key, g => g.OrderByDescending(ex => ex.TryLength).ToList());

            var tryClausesByEndOffset = tryClausesByOffset.Values.SelectMany(ex => ex)
                                                                 .GroupBy(ex => ex.TryOffset + ex.TryLength)
                                                                 .ToDictionary(g => g.Key, g => g.OrderByDescending(ex => ex.TryOffset).ToList());

            var catchClausesByOffset = exceptionClauses.Where(ex => ex.Flags == ExceptionHandlingClauseOptions.Clause)
                                                     .ToDictionary(ex => ex.HandlerOffset, ex => ex);

            var catchClausesByEndOffset = exceptionClauses.Where(ex => ex.Flags == ExceptionHandlingClauseOptions.Clause)
                                                     .ToDictionary(ex => ex.HandlerOffset + ex.HandlerLength, ex => ex);

            var faultClausesByOffset = exceptionClauses.Where(ex => ex.Flags == ExceptionHandlingClauseOptions.Fault)
                                         .ToDictionary(ex => ex.HandlerOffset, ex => ex);

            var faultClausesByEndOffset = exceptionClauses.Where(ex => ex.Flags == ExceptionHandlingClauseOptions.Fault)
                                         .ToDictionary(ex => ex.HandlerOffset + ex.HandlerLength, ex => ex);


            var finallyClausesByOffset = exceptionClauses.Where(ex => ex.Flags == ExceptionHandlingClauseOptions.Finally)
                                                     .ToDictionary(ex => ex.HandlerOffset, ex => ex);

            var finallyClausesByEndOffset = exceptionClauses.Where(ex => ex.Flags == ExceptionHandlingClauseOptions.Finally)
                                                    .ToDictionary(ex => ex.HandlerOffset + ex.HandlerLength, ex => ex);


            var filterClausesByFilterOffset = exceptionClauses.Where(ex => ex.Flags == ExceptionHandlingClauseOptions.Filter)
                                                  .ToDictionary(ex => ex.FilterOffset, ex => ex);

            var filterClausesByHandlerOffset = exceptionClauses.Where(ex => ex.Flags == ExceptionHandlingClauseOptions.Filter)
                                                  .ToDictionary(ex => ex.HandlerOffset, ex => ex);

            var filterClausesByEndHandlerOffset = exceptionClauses.Where(ex => ex.Flags == ExceptionHandlingClauseOptions.Filter)
                                                  .ToDictionary(ex => ex.HandlerOffset + ex.HandlerLength, ex => ex);


            int indenting = 0;
            foreach (var instruction in reader.Instructions)
            {
                List<ExceptionHandlingClause> exClauses;

                if (tryClausesByEndOffset.TryGetValue(instruction.Offset, out exClauses))
                {
                    for (int i = 0; i < exClauses.Count; i++)
                    {
                        indenting--;
                        blocks.Add(new CodeBlock(indenting, "}" + Environment.NewLine));
                    }
                }

                ExceptionHandlingClause ex;
                if (catchClausesByEndOffset.TryGetValue(instruction.Offset, out ex))
                {
                    indenting--;
                    blocks.Add(new CodeBlock(indenting, "}" + Environment.NewLine));
                }

                if (faultClausesByEndOffset.TryGetValue(instruction.Offset, out ex))
                {
                    indenting--;
                    blocks.Add(new CodeBlock(indenting, "}" + Environment.NewLine));
                }

                if (finallyClausesByEndOffset.TryGetValue(instruction.Offset, out ex))
                {
                    indenting--;
                    blocks.Add(new CodeBlock(indenting, "}" + Environment.NewLine));
                }

                if (filterClausesByEndHandlerOffset.TryGetValue(instruction.Offset, out ex))
                {
                    indenting--;
                    blocks.Add(new CodeBlock(indenting, "}" + Environment.NewLine));
                }


                if (tryClausesByOffset.TryGetValue(instruction.Offset, out exClauses))
                {
                    for (int i = 0; i < exClauses.Count; i++)
                    {
                        blocks.Add(new CodeBlock(indenting, ".try" + Environment.NewLine));
                        blocks.Add(new CodeBlock(indenting, "{" + Environment.NewLine));
                        indenting++;
                    }
                }

                if (catchClausesByOffset.TryGetValue(instruction.Offset, out ex))
                {
                    blocks.Add(new CodeBlock(indenting, "catch("));
                    blocks.Add(new TypeCodeBlock(ex.CatchType));
                    blocks.Add(new CodeBlock(")" + Environment.NewLine));
                    blocks.Add(new CodeBlock(indenting, "{" + Environment.NewLine));
                    indenting++;
                }

                if (faultClausesByOffset.TryGetValue(instruction.Offset, out ex))
                {
                    blocks.Add(new CodeBlock(indenting, "fault" + Environment.NewLine));
                    blocks.Add(new CodeBlock(indenting, "{" + Environment.NewLine));
                    indenting++;
                }

                if (finallyClausesByOffset.TryGetValue(instruction.Offset, out ex))
                {
                    blocks.Add(new CodeBlock(indenting, "finally" + Environment.NewLine));
                    blocks.Add(new CodeBlock(indenting, "{" + Environment.NewLine));
                    indenting++;
                }

                if (filterClausesByHandlerOffset.TryGetValue(instruction.Offset, out ex))
                {
                    indenting--;
                    blocks.Add(new CodeBlock(indenting, "}" + Environment.NewLine));
                    blocks.Add(new CodeBlock(indenting, "catch" + Environment.NewLine));
                    blocks.Add(new CodeBlock(indenting, "{" + Environment.NewLine));
                    indenting++;
                }


                if (filterClausesByFilterOffset.TryGetValue(instruction.Offset, out ex))
                {
                    blocks.Add(new CodeBlock(indenting, "filter" + Environment.NewLine));
                    blocks.Add(new CodeBlock(indenting, "{" + Environment.NewLine));
                    indenting++;
                }


                string opcode = instruction.Code.ToString();
                if (opcode.Length < 10)
                    opcode = opcode + new string(' ', 10 - opcode.Length);


                blocks.Add(new CodeBlock(indenting, ""));
                blocks.Add(new InstructionOffsetCodeBlock(instruction.Offset, false));
                blocks.Add(new CodeBlock(" "));
                blocks.Add(new InstructionOpCodeCodeBlock(instruction));
                blocks.Add(new CodeBlock("\t"));

                if (instruction.Code.FlowControl == FlowControl.Branch || instruction.Code.FlowControl == FlowControl.Cond_Branch)
                    if (instruction.Operand is Int32[])
                    {
                        var targetOffsets = ((Int32[])(instruction.Operand));
                        for (int i = 0; i < targetOffsets.Length; i++)
                        {
                            blocks.Add(new InstructionOffsetCodeBlock(targetOffsets[i], true));
                            if (i != targetOffsets.Length - 1)
                                blocks.Add(new CodeBlock(", "));
                        }
                    }
                    else
                    {
                        blocks.Add(new InstructionOffsetCodeBlock(Convert.ToInt32(instruction.Operand), true));
                    }
                else if (instruction.Operand is Type)
                {
                    blocks.Add(new TypeCodeBlock((Type)instruction.Operand));
                }
                else if (instruction.Operand is MemberInfo)
                {
                    blocks.Add(new TypeCodeBlock(((MemberInfo)instruction.Operand).DeclaringType));
                    blocks.Add(new CodeBlock("::"));
                    blocks.Add(new MemberCodeBlock((MemberInfo)instruction.Operand));
                }
                else if (instruction.Operand is string)
                {
                    blocks.Add(new ValueCodeBlock((string)instruction.Operand));
                }
                else if (instruction.Operand is Int32)
                {
                    blocks.Add(new ValueCodeBlock(instruction.Operand));
                }


                int paramIdx = -1;
                if (instruction.Code == OpCodes.Ldarg || instruction.Code == OpCodes.Ldarg_S ||
                    instruction.Code == OpCodes.Starg || instruction.Code == OpCodes.Starg_S)
                    paramIdx = Convert.ToInt32(instruction.Operand);
                else if (instruction.Code == OpCodes.Ldarg_0)
                    paramIdx = 0;
                else if (instruction.Code == OpCodes.Ldarg_1)
                    paramIdx = 1;
                else if (instruction.Code == OpCodes.Ldarg_2)
                    paramIdx = 2;
                else if (instruction.Code == OpCodes.Ldarg_3)
                    paramIdx = 3;
                else
                    paramIdx = -1;

                if (paramIdx >= 0)
                {
                    var parameterInfos = reader.Method.GetParameters();
                    if (reader.Method.IsStatic)
                        blocks.Add(new ParameterInfoCodeBlock(parameterInfos[paramIdx]));
                    else
                    {
                        if (paramIdx == 0)
                            blocks.Add(new ParameterInfoCodeBlock(null));
                        else
                            blocks.Add(new ParameterInfoCodeBlock(parameterInfos[paramIdx - 1]));
                    }
                }

                blocks.Add(new CodeBlock(Environment.NewLine));
            }

            foreach (var error in reader.Errors)
            {
                blocks.Add(new ErrorCodeBlock(error));
                blocks.Add(new CodeBlock(Environment.NewLine));

            }
            //CSharpILParser parser = new CSharpILParser(reader.Method, reader.Instructions);
            //string csharp = parser.Parse();
            //str.AppendLine(GetRtfUnicodeEscapedString(Environment.NewLine + Environment.NewLine + EscapeRTF(csharp).Replace(Environment.NewLine, @"\line ")));

            return blocks;
        }



        public static string ToRTFCode(this Disassembler reader)
        {
            if (reader.Instructions == null)
                return "";

            StringBuilder str = new StringBuilder();

            var locals = reader.Method.GetMethodBody().LocalVariables;

            if (locals.Count > 0)
            {
                str.Append(@".locals \{" + @"\line ");
                foreach (var local in locals)
                {
                    str.Append("    [" + local.LocalIndex + "] " + local.LocalType.ToSignatureString() + @"\line ");
                }
                str.Append(@"\}\line ");
            }

            //var tryClausesByOffset = exceptionClauses.GroupBy(ex => ex.TryOffset)
            //                                         .ToDictionary(g => g.Key, g => g.OrderBy(ex => ex.HandlerOffset + ex.HandlerLength).Last());

            //var tryClausesByEndOffset = tryClausesByOffset.Values
            //                                .GroupBy(ex => ex.HandlerOffset + ex.HandlerLength)
            //                                .ToDictionary(g => g.Key, g => g.OrderByDescending(ex => ex.TryOffset).ToList());

            var exceptionClauses = reader.Method.GetMethodBody().ExceptionHandlingClauses;

            var allTries = exceptionClauses.GroupBy(ex => ex.TryOffset + "_" + ex.TryLength)
                                                 .ToDictionary(g => g.Key, g => g.Last());

            // sort by trylength descending to define the outer try first!
            var tryClausesByOffset = allTries.Values.GroupBy(t => t.TryOffset)
                                                .ToDictionary(g => g.Key, g => g.OrderByDescending(ex => ex.TryLength).ToList());

            var tryClausesByEndOffset = tryClausesByOffset.Values.SelectMany(ex => ex)
                                                                 .GroupBy(ex => ex.TryOffset + ex.TryLength)
                                                                 .ToDictionary(g => g.Key, g => g.OrderByDescending(ex => ex.TryOffset).ToList());

            var catchClausesByOffset = exceptionClauses.Where(ex => ex.Flags == ExceptionHandlingClauseOptions.Clause)
                                                     .ToDictionary(ex => ex.HandlerOffset, ex => ex);

            var catchClausesByEndOffset = exceptionClauses.Where(ex => ex.Flags == ExceptionHandlingClauseOptions.Clause)
                                                     .ToDictionary(ex => ex.HandlerOffset + ex.HandlerLength, ex => ex);

            var faultClausesByOffset = exceptionClauses.Where(ex => ex.Flags == ExceptionHandlingClauseOptions.Fault)
                                         .ToDictionary(ex => ex.HandlerOffset, ex => ex);

            var faultClausesByEndOffset = exceptionClauses.Where(ex => ex.Flags == ExceptionHandlingClauseOptions.Fault)
                                         .ToDictionary(ex => ex.HandlerOffset + ex.HandlerLength, ex => ex);


            var finallyClausesByOffset = exceptionClauses.Where(ex => ex.Flags == ExceptionHandlingClauseOptions.Finally)
                                                     .ToDictionary(ex => ex.HandlerOffset, ex => ex);

            var finallyClausesByEndOffset = exceptionClauses.Where(ex => ex.Flags == ExceptionHandlingClauseOptions.Finally)
                                                    .ToDictionary(ex => ex.HandlerOffset + ex.HandlerLength, ex => ex);


            var filterClausesByFilterOffset = exceptionClauses.Where(ex => ex.Flags == ExceptionHandlingClauseOptions.Filter)
                                                  .ToDictionary(ex => ex.FilterOffset, ex => ex);

            var filterClausesByHandlerOffset = exceptionClauses.Where(ex => ex.Flags == ExceptionHandlingClauseOptions.Filter)
                                                  .ToDictionary(ex => ex.HandlerOffset, ex => ex);

            var filterClausesByEndHandlerOffset = exceptionClauses.Where(ex => ex.Flags == ExceptionHandlingClauseOptions.Filter)
                                                  .ToDictionary(ex => ex.HandlerOffset + ex.HandlerLength, ex => ex);


            int indenting = 0;
            foreach (var instruction in reader.Instructions)
            {
                List<ExceptionHandlingClause> exClauses;


                if (tryClausesByEndOffset.TryGetValue(instruction.Offset, out exClauses))
                {
                    for (int i = 0; i < exClauses.Count; i++)
                    {
                        indenting--;
                        str.AppendLine(indenting, @"\}");
                    }
                }

                ExceptionHandlingClause ex;
                if (catchClausesByEndOffset.TryGetValue(instruction.Offset, out ex))
                {
                    indenting--;
                    str.AppendLine(indenting, @"\}");
                }

                if (faultClausesByEndOffset.TryGetValue(instruction.Offset, out ex))
                {
                    indenting--;
                    str.AppendLine(indenting, @"\}");
                }

                if (finallyClausesByEndOffset.TryGetValue(instruction.Offset, out ex))
                {
                    indenting--;
                    str.AppendLine(indenting, @"\}");
                }

                if (filterClausesByEndHandlerOffset.TryGetValue(instruction.Offset, out ex))
                {
                    indenting--;
                    str.AppendLine(indenting, @"\}");
                }


                if (tryClausesByOffset.TryGetValue(instruction.Offset, out exClauses))
                {
                    for (int i = 0; i < exClauses.Count; i++)
                    {
                        str.AppendLine(indenting, @".try");
                        str.AppendLine(indenting, @"\{");
                        indenting++;
                    }
                }

                if (catchClausesByOffset.TryGetValue(instruction.Offset, out ex))
                {
                    str.AppendLine(indenting, @"catch(" + ex.CatchType.ToSignatureString() + ")");
                    str.AppendLine(indenting, @"\{");
                    indenting++;
                }

                if (faultClausesByOffset.TryGetValue(instruction.Offset, out ex))
                {
                    str.AppendLine(indenting, @"fault");
                    str.AppendLine(indenting, @"\{");
                    indenting++;
                }

                if (finallyClausesByOffset.TryGetValue(instruction.Offset, out ex))
                {
                    str.AppendLine(indenting, @"finally");
                    str.AppendLine(indenting, @"\{");
                    indenting++;
                }

                if (filterClausesByHandlerOffset.TryGetValue(instruction.Offset, out ex))
                {
                    indenting--;
                    str.AppendLine(indenting, @"\}");
                    str.AppendLine(indenting, @"catch");
                    str.AppendLine(indenting, @"\{");
                    indenting++;
                }


                if (filterClausesByFilterOffset.TryGetValue(instruction.Offset, out ex))
                {
                    str.AppendLine(indenting, @"filter");
                    str.AppendLine(indenting, @"\{");
                    indenting++;
                }


                string opcode = instruction.Code.ToString();
                if (opcode.Length < 10)
                    opcode = opcode + new string(' ', 10 - opcode.Length);

                string line = instruction.Offset.ToString("x4") + @" \b " + opcode + @"\b0 ";
                line += " \t";
                if (instruction.Code.FlowControl == FlowControl.Branch || instruction.Code.FlowControl == FlowControl.Cond_Branch)
                    if (instruction.Operand is Int32[])
                        line += string.Join(",", ((Int32[])(instruction.Operand)).Select(i => i.ToString("x4")).ToArray());
                    else
                    {
                        line += GetHyperlink((Convert.ToInt32(instruction.Operand)).ToString("x4"), "", 4);
                    }
                else if (instruction.Operand is Type)
                {
                    string sig = ((Type)instruction.Operand).ToSignatureString();
                    if (!string.IsNullOrEmpty(sig))
                        line += GetHyperlink(sig, "", 2);
                    else
                        line += instruction.Operand;
                }
                else if (instruction.Operand is MemberInfo)
                {
                    string sig = ((MemberInfo)instruction.Operand).DeclaringType.ToSignatureString() + "::" + ((MemberInfo)instruction.Operand).ToSignatureString();
                    if (!string.IsNullOrEmpty(sig))
                        line += GetHyperlink(sig, "", 1);
                    else
                        line += instruction.Operand;
                }
                else if (instruction.Operand is string)
                {
                    line += @"\cf3\" + "\"" + (string)(instruction.Operand) + @"\cf0\" + "\"";
                }
                else if (instruction.Operand is Int32)
                {
                    line += @"\cf3\" + "\"" + (instruction.Operand + "") + @"\cf0\" + "\"";
                }


                int paramIdx = -1;
                if (instruction.Code == OpCodes.Ldarg || instruction.Code == OpCodes.Ldarg_S ||
                    instruction.Code == OpCodes.Starg || instruction.Code == OpCodes.Starg_S)
                    paramIdx = Convert.ToInt32(instruction.Operand);
                else if (instruction.Code == OpCodes.Ldarg_0)
                    paramIdx = 0;
                else if (instruction.Code == OpCodes.Ldarg_1)
                    paramIdx = 1;
                else if (instruction.Code == OpCodes.Ldarg_2)
                    paramIdx = 2;
                else if (instruction.Code == OpCodes.Ldarg_3)
                    paramIdx = 3;
                else
                    paramIdx = -1;

                if (paramIdx >= 0)
                {
                    var parameterInfos = reader.Method.GetParameters();
                    if (reader.Method.IsStatic)
                        line += "(" + parameterInfos[paramIdx].Name + ")";
                    else
                    {
                        if (paramIdx == 0)
                            line += "(this)";
                        else
                            line += "(" + parameterInfos[paramIdx - 1].Name + ")";
                    }
                }


                str.AppendLine(indenting, line);
            }

            foreach (var error in reader.Errors)
            {
                str.AppendLine(EscapeRTF("Error reading instruction at " + error.ErrorPosition.ToString("x4") + ":  " + error.Exception.ToExceptionString()) + @"\line ");
            }
            //CSharpILParser parser = new CSharpILParser(reader.Method, reader.Instructions);
            //string csharp = parser.Parse();
            //str.AppendLine(GetRtfUnicodeEscapedString(Environment.NewLine + Environment.NewLine + EscapeRTF(csharp).Replace(Environment.NewLine, @"\line ")));

            return str.ToString();
        }

        private static void AppendLine(this StringBuilder str, int indenting, string s)
        {
            str.Append(new string(' ', indenting * 4) + s + @"\line ");
        }


        //"http://" + Uri.EscapeUriString(((MemberInfo)instruction.Operand).Module.FullyQualifiedName) + "#" + ((MemberInfo)instruction.Operand).MetadataToken
        private static string GetHyperlink(string text, string url, int colorIndex)
        {
            // use &nbsp; spaces between method for easier parsing
            return @"\cf" + colorIndex + @"\ul " + GetRtfUnicodeEscapedString(EscapeRTF(text)).Replace(' ', ' ') + @"\cf0\ulnone ";
            //return @"\cf1\ul\field{\*\fldinst{HYPERLINK " + "\"" + EscapeRT F(GetRtfUnicodeEscapedString(text)) + "\"}}" + @"{\fldrslt " + EscapeRTF(GetRtfUnicodeEscapedString(text)) + @"}\cf0\ulnone ";
        }

        static string GetRtfUnicodeEscapedString(string s)
        {
            var sb = new StringBuilder();
            foreach (var c in s)
            {
                if (c <= 0x7f)
                    sb.Append(c);
                else
                    sb.Append("\\u" + Convert.ToUInt32(c) + "?");
            }
            return sb.ToString();
        }

        private static string EscapeRTF(string text)
        {
            return text.Replace(@"\", @"\\").Replace(@"{", @"\{").Replace(@"}", @"\}");
        }


        public static string ToDescription(this OpCode code)
        {
            if (opcodeDescriptions == null)
                InitializeOpcodeDescriptions();
            string str;
            if (opcodeDescriptions.TryGetValue(code, out str))
                return str;
            else
                return "";
        }

        private static void InitializeOpcodeDescriptions()
        {
            opcodeDescriptions = new Dictionary<OpCode, string>();
            opcodeDescriptions.Add(OpCodes.Add, "Adds two values and pushes the result onto the evaluation stack. ");
            opcodeDescriptions.Add(OpCodes.Add_Ovf, "Adds two integers, performs an overflow check, and pushes the result onto the evaluation stack. ");
            opcodeDescriptions.Add(OpCodes.Add_Ovf_Un, "Adds two unsigned integer values, performs an overflow check, and pushes the result onto the evaluation stack. ");
            opcodeDescriptions.Add(OpCodes.And, "Computes the bitwise AND of two values and pushes the result onto the evaluation stack. ");
            opcodeDescriptions.Add(OpCodes.Arglist, "Returns an unmanaged pointer to the argument list of the current method. ");
            opcodeDescriptions.Add(OpCodes.Beq, "Transfers control to a target instruction if two values are equal. ");
            opcodeDescriptions.Add(OpCodes.Beq_S, "Transfers control to a target instruction (short form) if two values are equal. ");
            opcodeDescriptions.Add(OpCodes.Bge, "Transfers control to a target instruction if the first value is greater than or equal to the second value. ");
            opcodeDescriptions.Add(OpCodes.Bge_S, "Transfers control to a target instruction (short form) if the first value is greater than or equal to the second value. ");
            opcodeDescriptions.Add(OpCodes.Bge_Un, "Transfers control to a target instruction if the first value is greater than the second value, when comparing unsigned integer values or unordered float values. ");
            opcodeDescriptions.Add(OpCodes.Bge_Un_S, "Transfers control to a target instruction (short form) if the first value is greater than the second value, when comparing unsigned integer values or unordered float values. ");
            opcodeDescriptions.Add(OpCodes.Bgt, "Transfers control to a target instruction if the first value is greater than the second value. ");
            opcodeDescriptions.Add(OpCodes.Bgt_S, "Transfers control to a target instruction (short form) if the first value is greater than the second value. ");
            opcodeDescriptions.Add(OpCodes.Bgt_Un, "Transfers control to a target instruction if the first value is greater than the second value, when comparing unsigned integer values or unordered float values. ");
            opcodeDescriptions.Add(OpCodes.Bgt_Un_S, "Transfers control to a target instruction (short form) if the first value is greater than the second value, when comparing unsigned integer values or unordered float values. ");
            opcodeDescriptions.Add(OpCodes.Ble, "Transfers control to a target instruction if the first value is less than or equal to the second value. ");
            opcodeDescriptions.Add(OpCodes.Ble_S, "Transfers control to a target instruction (short form) if the first value is less than or equal to the second value. ");
            opcodeDescriptions.Add(OpCodes.Ble_Un, "Transfers control to a target instruction if the first value is less than or equal to the second value, when comparing unsigned integer values or unordered float values. ");
            opcodeDescriptions.Add(OpCodes.Ble_Un_S, "Transfers control to a target instruction (short form) if the first value is less than or equal to the second value, when comparing unsigned integer values or unordered float values. ");
            opcodeDescriptions.Add(OpCodes.Blt, "Transfers control to a target instruction if the first value is less than the second value. ");
            opcodeDescriptions.Add(OpCodes.Blt_S, "Transfers control to a target instruction (short form) if the first value is less than the second value. ");
            opcodeDescriptions.Add(OpCodes.Blt_Un, "Transfers control to a target instruction if the first value is less than the second value, when comparing unsigned integer values or unordered float values. ");
            opcodeDescriptions.Add(OpCodes.Blt_Un_S, "Transfers control to a target instruction (short form) if the first value is less than the second value, when comparing unsigned integer values or unordered float values. ");
            opcodeDescriptions.Add(OpCodes.Bne_Un, "Transfers control to a target instruction when two unsigned integer values or unordered float values are not equal. ");
            opcodeDescriptions.Add(OpCodes.Bne_Un_S, "Transfers control to a target instruction (short form) when two unsigned integer values or unordered float values are not equal. ");
            opcodeDescriptions.Add(OpCodes.Box, "Converts a value type to an object reference (type O). ");
            opcodeDescriptions.Add(OpCodes.Br, "Unconditionally transfers control to a target instruction. ");
            opcodeDescriptions.Add(OpCodes.Br_S, "Unconditionally transfers control to a target instruction (short form). ");
            opcodeDescriptions.Add(OpCodes.Break, "Signals the Common Language Infrastructure (CLI) to inform the debugger that a break point has been tripped. ");
            opcodeDescriptions.Add(OpCodes.Brfalse, "Transfers control to a target instruction if value is false, a null reference (Nothing in Visual Basic), or zero. ");
            opcodeDescriptions.Add(OpCodes.Brfalse_S, "Transfers control to a target instruction if value is false, a null reference, or zero. ");
            opcodeDescriptions.Add(OpCodes.Brtrue, "Transfers control to a target instruction if value is true, not null, or non-zero. ");
            opcodeDescriptions.Add(OpCodes.Brtrue_S, "Transfers control to a target instruction (short form) if value is true, not null, or non-zero. ");
            opcodeDescriptions.Add(OpCodes.Call, "Calls the method indicated by the passed method descriptor. ");
            opcodeDescriptions.Add(OpCodes.Calli, "Calls the method indicated on the evaluation stack (as a pointer to an entry point) with arguments described by a calling convention. ");
            opcodeDescriptions.Add(OpCodes.Callvirt, "Calls a late-bound method on an object, pushing the return value onto the evaluation stack. ");
            opcodeDescriptions.Add(OpCodes.Castclass, "Attempts to cast an object passed by reference to the specified class. ");
            opcodeDescriptions.Add(OpCodes.Ceq, "Compares two values. If they are equal, the integer value 1 (int32) is pushed onto the evaluation stack; otherwise 0 (int32) is pushed onto the evaluation stack. ");
            opcodeDescriptions.Add(OpCodes.Cgt, "Compares two values. If the first value is greater than the second, the integer value 1 (int32) is pushed onto the evaluation stack; otherwise 0 (int32) is pushed onto the evaluation stack. ");
            opcodeDescriptions.Add(OpCodes.Cgt_Un, "Compares two unsigned or unordered values. If the first value is greater than the second, the integer value 1 (int32) is pushed onto the evaluation stack; otherwise 0 (int32) is pushed onto the evaluation stack. ");
            opcodeDescriptions.Add(OpCodes.Ckfinite, "Throws ArithmeticException if value is not a finite number. ");
            opcodeDescriptions.Add(OpCodes.Clt, "Compares two values. If the first value is less than the second, the integer value 1 (int32) is pushed onto the evaluation stack; otherwise 0 (int32) is pushed onto the evaluation stack. ");
            opcodeDescriptions.Add(OpCodes.Clt_Un, "Compares the unsigned or unordered values value1 and value2. If value1 is less than value2, then the integer value 1 (int32) is pushed onto the evaluation stack; otherwise 0 (int32) is pushed onto the evaluation stack. ");
            opcodeDescriptions.Add(OpCodes.Constrained, "Constrains the type on which a virtual method call is made. ");
            opcodeDescriptions.Add(OpCodes.Conv_I, "Converts the value on top of the evaluation stack to native int. ");
            opcodeDescriptions.Add(OpCodes.Conv_I1, "Converts the value on top of the evaluation stack to int8, then extends (pads) it to int32. ");
            opcodeDescriptions.Add(OpCodes.Conv_I2, "Converts the value on top of the evaluation stack to int16, then extends (pads) it to int32. ");
            opcodeDescriptions.Add(OpCodes.Conv_I4, "Converts the value on top of the evaluation stack to int32. ");
            opcodeDescriptions.Add(OpCodes.Conv_I8, "Converts the value on top of the evaluation stack to int64. ");
            opcodeDescriptions.Add(OpCodes.Conv_Ovf_I, "Converts the signed value on top of the evaluation stack to signed native int, throwing OverflowException on overflow. ");
            opcodeDescriptions.Add(OpCodes.Conv_Ovf_I_Un, "Converts the unsigned value on top of the evaluation stack to signed native int, throwing OverflowException on overflow. ");
            opcodeDescriptions.Add(OpCodes.Conv_Ovf_I1, "Converts the signed value on top of the evaluation stack to signed int8 and extends it to int32, throwing OverflowException on overflow. ");
            opcodeDescriptions.Add(OpCodes.Conv_Ovf_I1_Un, "Converts the unsigned value on top of the evaluation stack to signed int8 and extends it to int32, throwing OverflowException on overflow. ");
            opcodeDescriptions.Add(OpCodes.Conv_Ovf_I2, "Converts the signed value on top of the evaluation stack to signed int16 and extending it to int32, throwing OverflowException on overflow. ");
            opcodeDescriptions.Add(OpCodes.Conv_Ovf_I2_Un, "Converts the unsigned value on top of the evaluation stack to signed int16 and extends it to int32, throwing OverflowException on overflow. ");
            opcodeDescriptions.Add(OpCodes.Conv_Ovf_I4, "Converts the signed value on top of the evaluation stack to signed int32, throwing OverflowException on overflow. ");
            opcodeDescriptions.Add(OpCodes.Conv_Ovf_I4_Un, "Converts the unsigned value on top of the evaluation stack to signed int32, throwing OverflowException on overflow. ");
            opcodeDescriptions.Add(OpCodes.Conv_Ovf_I8, "Converts the signed value on top of the evaluation stack to signed int64, throwing OverflowException on overflow. ");
            opcodeDescriptions.Add(OpCodes.Conv_Ovf_I8_Un, "Converts the unsigned value on top of the evaluation stack to signed int64, throwing OverflowException on overflow. ");
            opcodeDescriptions.Add(OpCodes.Conv_Ovf_U, "Converts the signed value on top of the evaluation stack to unsigned native int, throwing OverflowException on overflow. ");
            opcodeDescriptions.Add(OpCodes.Conv_Ovf_U_Un, "Converts the unsigned value on top of the evaluation stack to unsigned native int, throwing OverflowException on overflow. ");
            opcodeDescriptions.Add(OpCodes.Conv_Ovf_U1, "Converts the signed value on top of the evaluation stack to unsigned int8 and extends it to int32, throwing OverflowException on overflow. ");
            opcodeDescriptions.Add(OpCodes.Conv_Ovf_U1_Un, "Converts the unsigned value on top of the evaluation stack to unsigned int8 and extends it to int32, throwing OverflowException on overflow. ");
            opcodeDescriptions.Add(OpCodes.Conv_Ovf_U2, "Converts the signed value on top of the evaluation stack to unsigned int16 and extends it to int32, throwing OverflowException on overflow. ");
            opcodeDescriptions.Add(OpCodes.Conv_Ovf_U2_Un, "Converts the unsigned value on top of the evaluation stack to unsigned int16 and extends it to int32, throwing OverflowException on overflow. ");
            opcodeDescriptions.Add(OpCodes.Conv_Ovf_U4, "Converts the signed value on top of the evaluation stack to unsigned int32, throwing OverflowException on overflow. ");
            opcodeDescriptions.Add(OpCodes.Conv_Ovf_U4_Un, "Converts the unsigned value on top of the evaluation stack to unsigned int32, throwing OverflowException on overflow. ");
            opcodeDescriptions.Add(OpCodes.Conv_Ovf_U8, "Converts the signed value on top of the evaluation stack to unsigned int64, throwing OverflowException on overflow. ");
            opcodeDescriptions.Add(OpCodes.Conv_Ovf_U8_Un, "Converts the unsigned value on top of the evaluation stack to unsigned int64, throwing OverflowException on overflow. ");
            opcodeDescriptions.Add(OpCodes.Conv_R_Un, "Converts the unsigned integer value on top of the evaluation stack to float32. ");
            opcodeDescriptions.Add(OpCodes.Conv_R4, "Converts the value on top of the evaluation stack to float32. ");
            opcodeDescriptions.Add(OpCodes.Conv_R8, "Converts the value on top of the evaluation stack to float64. ");
            opcodeDescriptions.Add(OpCodes.Conv_U, "Converts the value on top of the evaluation stack to unsigned native int, and extends it to native int. ");
            opcodeDescriptions.Add(OpCodes.Conv_U1, "Converts the value on top of the evaluation stack to unsigned int8, and extends it to int32. ");
            opcodeDescriptions.Add(OpCodes.Conv_U2, "Converts the value on top of the evaluation stack to unsigned int16, and extends it to int32. ");
            opcodeDescriptions.Add(OpCodes.Conv_U4, "Converts the value on top of the evaluation stack to unsigned int32, and extends it to int32. ");
            opcodeDescriptions.Add(OpCodes.Conv_U8, "Converts the value on top of the evaluation stack to unsigned int64, and extends it to int64. ");
            opcodeDescriptions.Add(OpCodes.Cpblk, "Copies a specified number bytes from a source address to a destination address. ");
            opcodeDescriptions.Add(OpCodes.Cpobj, "Copies the value type located at the address of an object (type &, * or native int) to the address of the destination object (type &, * or native int). ");
            opcodeDescriptions.Add(OpCodes.Div, "Divides two values and pushes the result as a floating-point (type F) or quotient (type int32) onto the evaluation stack. ");
            opcodeDescriptions.Add(OpCodes.Div_Un, "Divides two unsigned integer values and pushes the result (int32) onto the evaluation stack. ");
            opcodeDescriptions.Add(OpCodes.Dup, "Copies the current topmost value on the evaluation stack, and then pushes the copy onto the evaluation stack. ");
            opcodeDescriptions.Add(OpCodes.Endfilter, "Transfers control from the filter clause of an exception back to the Common Language Infrastructure (CLI) exception handler. ");
            opcodeDescriptions.Add(OpCodes.Endfinally, "Transfers control from the fault or finally clause of an exception block back to the Common Language Infrastructure (CLI) exception handler. ");
            opcodeDescriptions.Add(OpCodes.Initblk, "Initializes a specified block of memory at a specific address to a given size and initial value. ");
            opcodeDescriptions.Add(OpCodes.Initobj, "Initializes each field of the value type at a specified address to a null reference or a 0 of the appropriate primitive type. ");
            opcodeDescriptions.Add(OpCodes.Isinst, "Tests whether an object reference (type O) is an instance of a particular class. ");
            opcodeDescriptions.Add(OpCodes.Jmp, "Exits current method and jumps to specified method. ");
            opcodeDescriptions.Add(OpCodes.Ldarg, "Loads an argument (referenced by a specified index value) onto the stack. ");
            opcodeDescriptions.Add(OpCodes.Ldarg_0, "Loads the argument at index 0 onto the evaluation stack. ");
            opcodeDescriptions.Add(OpCodes.Ldarg_1, "Loads the argument at index 1 onto the evaluation stack. ");
            opcodeDescriptions.Add(OpCodes.Ldarg_2, "Loads the argument at index 2 onto the evaluation stack. ");
            opcodeDescriptions.Add(OpCodes.Ldarg_3, "Loads the argument at index 3 onto the evaluation stack. ");
            opcodeDescriptions.Add(OpCodes.Ldarg_S, "Loads the argument (referenced by a specified short form index) onto the evaluation stack. ");
            opcodeDescriptions.Add(OpCodes.Ldarga, "Load an argument address onto the evaluation stack. ");
            opcodeDescriptions.Add(OpCodes.Ldarga_S, "Load an argument address, in short form, onto the evaluation stack. ");
            opcodeDescriptions.Add(OpCodes.Ldc_I4, "Pushes a supplied value of type int32 onto the evaluation stack as an int32. ");
            opcodeDescriptions.Add(OpCodes.Ldc_I4_0, "Pushes the integer value of 0 onto the evaluation stack as an int32. ");
            opcodeDescriptions.Add(OpCodes.Ldc_I4_1, "Pushes the integer value of 1 onto the evaluation stack as an int32. ");
            opcodeDescriptions.Add(OpCodes.Ldc_I4_2, "Pushes the integer value of 2 onto the evaluation stack as an int32. ");
            opcodeDescriptions.Add(OpCodes.Ldc_I4_3, "Pushes the integer value of 3 onto the evaluation stack as an int32. ");
            opcodeDescriptions.Add(OpCodes.Ldc_I4_4, "Pushes the integer value of 4 onto the evaluation stack as an int32. ");
            opcodeDescriptions.Add(OpCodes.Ldc_I4_5, "Pushes the integer value of 5 onto the evaluation stack as an int32. ");
            opcodeDescriptions.Add(OpCodes.Ldc_I4_6, "Pushes the integer value of 6 onto the evaluation stack as an int32. ");
            opcodeDescriptions.Add(OpCodes.Ldc_I4_7, "Pushes the integer value of 7 onto the evaluation stack as an int32. ");
            opcodeDescriptions.Add(OpCodes.Ldc_I4_8, "Pushes the integer value of 8 onto the evaluation stack as an int32. ");
            opcodeDescriptions.Add(OpCodes.Ldc_I4_M1, "Pushes the integer value of -1 onto the evaluation stack as an int32. ");
            opcodeDescriptions.Add(OpCodes.Ldc_I4_S, "Pushes the supplied int8 value onto the evaluation stack as an int32, short form. ");
            opcodeDescriptions.Add(OpCodes.Ldc_I8, "Pushes a supplied value of type int64 onto the evaluation stack as an int64. ");
            opcodeDescriptions.Add(OpCodes.Ldc_R4, "Pushes a supplied value of type float32 onto the evaluation stack as type F (float). ");
            opcodeDescriptions.Add(OpCodes.Ldc_R8, "Pushes a supplied value of type float64 onto the evaluation stack as type F (float). ");
            opcodeDescriptions.Add(OpCodes.Ldelem, "Loads the element at a specified array index onto the top of the evaluation stack as the type specified in the instruction. ");
            opcodeDescriptions.Add(OpCodes.Ldelem_I, "Loads the element with type native int at a specified array index onto the top of the evaluation stack as a native int. ");
            opcodeDescriptions.Add(OpCodes.Ldelem_I1, "Loads the element with type int8 at a specified array index onto the top of the evaluation stack as an int32. ");
            opcodeDescriptions.Add(OpCodes.Ldelem_I2, "Loads the element with type int16 at a specified array index onto the top of the evaluation stack as an int32. ");
            opcodeDescriptions.Add(OpCodes.Ldelem_I4, "Loads the element with type int32 at a specified array index onto the top of the evaluation stack as an int32. ");
            opcodeDescriptions.Add(OpCodes.Ldelem_I8, "Loads the element with type int64 at a specified array index onto the top of the evaluation stack as an int64. ");
            opcodeDescriptions.Add(OpCodes.Ldelem_R4, "Loads the element with type float32 at a specified array index onto the top of the evaluation stack as type F (float). ");
            opcodeDescriptions.Add(OpCodes.Ldelem_R8, "Loads the element with type float64 at a specified array index onto the top of the evaluation stack as type F (float). ");
            opcodeDescriptions.Add(OpCodes.Ldelem_Ref, "Loads the element containing an object reference at a specified array index onto the top of the evaluation stack as type O (object reference). ");
            opcodeDescriptions.Add(OpCodes.Ldelem_U1, "Loads the element with type unsigned int8 at a specified array index onto the top of the evaluation stack as an int32. ");
            opcodeDescriptions.Add(OpCodes.Ldelem_U2, "Loads the element with type unsigned int16 at a specified array index onto the top of the evaluation stack as an int32. ");
            opcodeDescriptions.Add(OpCodes.Ldelem_U4, "Loads the element with type unsigned int32 at a specified array index onto the top of the evaluation stack as an int32. ");
            opcodeDescriptions.Add(OpCodes.Ldelema, "Loads the address of the array element at a specified array index onto the top of the evaluation stack as type & (managed pointer). ");
            opcodeDescriptions.Add(OpCodes.Ldfld, "Finds the value of a field in the object whose reference is currently on the evaluation stack. ");
            opcodeDescriptions.Add(OpCodes.Ldflda, "Finds the address of a field in the object whose reference is currently on the evaluation stack. ");
            opcodeDescriptions.Add(OpCodes.Ldftn, "Pushes an unmanaged pointer (type native int) to the native code implementing a specific method onto the evaluation stack. ");
            opcodeDescriptions.Add(OpCodes.Ldind_I, "Loads a value of type native int as a native int onto the evaluation stack indirectly. ");
            opcodeDescriptions.Add(OpCodes.Ldind_I1, "Loads a value of type int8 as an int32 onto the evaluation stack indirectly. ");
            opcodeDescriptions.Add(OpCodes.Ldind_I2, "Loads a value of type int16 as an int32 onto the evaluation stack indirectly. ");
            opcodeDescriptions.Add(OpCodes.Ldind_I4, "Loads a value of type int32 as an int32 onto the evaluation stack indirectly. ");
            opcodeDescriptions.Add(OpCodes.Ldind_I8, "Loads a value of type int64 as an int64 onto the evaluation stack indirectly. ");
            opcodeDescriptions.Add(OpCodes.Ldind_R4, "Loads a value of type float32 as a type F (float) onto the evaluation stack indirectly. ");
            opcodeDescriptions.Add(OpCodes.Ldind_R8, "Loads a value of type float64 as a type F (float) onto the evaluation stack indirectly. ");
            opcodeDescriptions.Add(OpCodes.Ldind_Ref, "Loads an object reference as a type O (object reference) onto the evaluation stack indirectly. ");
            opcodeDescriptions.Add(OpCodes.Ldind_U1, "Loads a value of type unsigned int8 as an int32 onto the evaluation stack indirectly. ");
            opcodeDescriptions.Add(OpCodes.Ldind_U2, "Loads a value of type unsigned int16 as an int32 onto the evaluation stack indirectly. ");
            opcodeDescriptions.Add(OpCodes.Ldind_U4, "Loads a value of type unsigned int32 as an int32 onto the evaluation stack indirectly. ");
            opcodeDescriptions.Add(OpCodes.Ldlen, "Pushes the number of elements of a zero-based, one-dimensional array onto the evaluation stack. ");
            opcodeDescriptions.Add(OpCodes.Ldloc, "Loads the local variable at a specific index onto the evaluation stack. ");
            opcodeDescriptions.Add(OpCodes.Ldloc_0, "Loads the local variable at index 0 onto the evaluation stack. ");
            opcodeDescriptions.Add(OpCodes.Ldloc_1, "Loads the local variable at index 1 onto the evaluation stack. ");
            opcodeDescriptions.Add(OpCodes.Ldloc_2, "Loads the local variable at index 2 onto the evaluation stack. ");
            opcodeDescriptions.Add(OpCodes.Ldloc_3, "Loads the local variable at index 3 onto the evaluation stack. ");
            opcodeDescriptions.Add(OpCodes.Ldloc_S, "Loads the local variable at a specific index onto the evaluation stack, short form. ");
            opcodeDescriptions.Add(OpCodes.Ldloca, "Loads the address of the local variable at a specific index onto the evaluation stack. ");
            opcodeDescriptions.Add(OpCodes.Ldloca_S, "Loads the address of the local variable at a specific index onto the evaluation stack, short form. ");
            opcodeDescriptions.Add(OpCodes.Ldnull, "Pushes a null reference (type O) onto the evaluation stack. ");
            opcodeDescriptions.Add(OpCodes.Ldobj, "Copies the value type object pointed to by an address to the top of the evaluation stack. ");
            opcodeDescriptions.Add(OpCodes.Ldsfld, "Pushes the value of a static field onto the evaluation stack. ");
            opcodeDescriptions.Add(OpCodes.Ldsflda, "Pushes the address of a static field onto the evaluation stack. ");
            opcodeDescriptions.Add(OpCodes.Ldstr, "Pushes a new object reference to a string literal stored in the metadata. ");
            opcodeDescriptions.Add(OpCodes.Ldtoken, "Converts a metadata token to its runtime representation, pushing it onto the evaluation stack. ");
            opcodeDescriptions.Add(OpCodes.Ldvirtftn, "Pushes an unmanaged pointer (type native int) to the native code implementing a particular virtual method associated with a specified object onto the evaluation stack. ");
            opcodeDescriptions.Add(OpCodes.Leave, "Exits a protected region of code, unconditionally transferring control to a specific target instruction. ");
            opcodeDescriptions.Add(OpCodes.Leave_S, "Exits a protected region of code, unconditionally transferring control to a target instruction (short form). ");
            opcodeDescriptions.Add(OpCodes.Localloc, "Allocates a certain number of bytes from the local dynamic memory pool and pushes the address (a transient pointer, type *) of the first allocated byte onto the evaluation stack. ");
            opcodeDescriptions.Add(OpCodes.Mkrefany, "Pushes a typed reference to an instance of a specific type onto the evaluation stack. ");
            opcodeDescriptions.Add(OpCodes.Mul, "Multiplies two values and pushes the result on the evaluation stack. ");
            opcodeDescriptions.Add(OpCodes.Mul_Ovf, "Multiplies two integer values, performs an overflow check, and pushes the result onto the evaluation stack. ");
            opcodeDescriptions.Add(OpCodes.Mul_Ovf_Un, "Multiplies two unsigned integer values, performs an overflow check, and pushes the result onto the evaluation stack. ");
            opcodeDescriptions.Add(OpCodes.Neg, "Negates a value and pushes the result onto the evaluation stack. ");
            opcodeDescriptions.Add(OpCodes.Newarr, "Pushes an object reference to a new zero-based, one-dimensional array whose elements are of a specific type onto the evaluation stack. ");
            opcodeDescriptions.Add(OpCodes.Newobj, "Creates a new object or a new instance of a value type, pushing an object reference (type O) onto the evaluation stack. ");
            opcodeDescriptions.Add(OpCodes.Nop, "Fills space if opcodes are patched. No meaningful operation is performed although a processing cycle can be consumed. ");
            opcodeDescriptions.Add(OpCodes.Not, "Computes the bitwise complement of the integer value on top of the stack and pushes the result onto the evaluation stack as the same type. ");
            opcodeDescriptions.Add(OpCodes.Or, "Compute the bitwise complement of the two integer values on top of the stack and pushes the result onto the evaluation stack. ");
            opcodeDescriptions.Add(OpCodes.Pop, "Removes the value currently on top of the evaluation stack. ");
            opcodeDescriptions.Add(OpCodes.Prefix1, "Infrastructure. This is a reserved instruction.");
            opcodeDescriptions.Add(OpCodes.Prefix2, "Infrastructure. This is a reserved instruction.");
            opcodeDescriptions.Add(OpCodes.Prefix3, "Infrastructure. This is a reserved instruction.");
            opcodeDescriptions.Add(OpCodes.Prefix4, "Infrastructure. This is a reserved instruction.");
            opcodeDescriptions.Add(OpCodes.Prefix5, "Infrastructure. This is a reserved instruction.");
            opcodeDescriptions.Add(OpCodes.Prefix6, "Infrastructure. This is a reserved instruction.");
            opcodeDescriptions.Add(OpCodes.Prefix7, "Infrastructure. This is a reserved instruction.");
            opcodeDescriptions.Add(OpCodes.Prefixref, "Infrastructure. This is a reserved instruction.");
            opcodeDescriptions.Add(OpCodes.Readonly, "Specifies that the subsequent array address operation performs no type check at run time, and that it returns a managed pointer whose mutability is restricted. ");
            opcodeDescriptions.Add(OpCodes.Refanytype, "Retrieves the type token embedded in a typed reference. ");
            opcodeDescriptions.Add(OpCodes.Refanyval, "Retrieves the address (type &) embedded in a typed reference. ");
            opcodeDescriptions.Add(OpCodes.Rem, "Divides two values and pushes the remainder onto the evaluation stack. ");
            opcodeDescriptions.Add(OpCodes.Rem_Un, "Divides two unsigned values and pushes the remainder onto the evaluation stack. ");
            opcodeDescriptions.Add(OpCodes.Ret, "Returns from the current method, pushing a return value (if present) from the callee's evaluation stack onto the caller's evaluation stack. ");
            opcodeDescriptions.Add(OpCodes.Rethrow, "Rethrows the current exception. ");
            opcodeDescriptions.Add(OpCodes.Shl, "Shifts an integer value to the left (in zeroes) by a specified number of bits, pushing the result onto the evaluation stack. ");
            opcodeDescriptions.Add(OpCodes.Shr, "Shifts an integer value (in sign) to the right by a specified number of bits, pushing the result onto the evaluation stack. ");
            opcodeDescriptions.Add(OpCodes.Shr_Un, "Shifts an unsigned integer value (in zeroes) to the right by a specified number of bits, pushing the result onto the evaluation stack. ");
            opcodeDescriptions.Add(OpCodes.Sizeof, "Pushes the size, in bytes, of a supplied value type onto the evaluation stack. ");
            opcodeDescriptions.Add(OpCodes.Starg, "Stores the value on top of the evaluation stack in the argument slot at a specified index. ");
            opcodeDescriptions.Add(OpCodes.Starg_S, "Stores the value on top of the evaluation stack in the argument slot at a specified index, short form. ");
            opcodeDescriptions.Add(OpCodes.Stelem, "Replaces the array element at a given index with the value on the evaluation stack, whose type is specified in the instruction. ");
            opcodeDescriptions.Add(OpCodes.Stelem_I, "Replaces the array element at a given index with the native int value on the evaluation stack. ");
            opcodeDescriptions.Add(OpCodes.Stelem_I1, "Replaces the array element at a given index with the int8 value on the evaluation stack. ");
            opcodeDescriptions.Add(OpCodes.Stelem_I2, "Replaces the array element at a given index with the int16 value on the evaluation stack. ");
            opcodeDescriptions.Add(OpCodes.Stelem_I4, "Replaces the array element at a given index with the int32 value on the evaluation stack. ");
            opcodeDescriptions.Add(OpCodes.Stelem_I8, "Replaces the array element at a given index with the int64 value on the evaluation stack. ");
            opcodeDescriptions.Add(OpCodes.Stelem_R4, "Replaces the array element at a given index with the float32 value on the evaluation stack. ");
            opcodeDescriptions.Add(OpCodes.Stelem_R8, "Replaces the array element at a given index with the float64 value on the evaluation stack. ");
            opcodeDescriptions.Add(OpCodes.Stelem_Ref, "Replaces the array element at a given index with the object ref value (type O) on the evaluation stack. ");
            opcodeDescriptions.Add(OpCodes.Stfld, "Replaces the value stored in the field of an object reference or pointer with a new value. ");
            opcodeDescriptions.Add(OpCodes.Stind_I, "Stores a value of type native int at a supplied address. ");
            opcodeDescriptions.Add(OpCodes.Stind_I1, "Stores a value of type int8 at a supplied address. ");
            opcodeDescriptions.Add(OpCodes.Stind_I2, "Stores a value of type int16 at a supplied address. ");
            opcodeDescriptions.Add(OpCodes.Stind_I4, "Stores a value of type int32 at a supplied address. ");
            opcodeDescriptions.Add(OpCodes.Stind_I8, "Stores a value of type int64 at a supplied address. ");
            opcodeDescriptions.Add(OpCodes.Stind_R4, "Stores a value of type float32 at a supplied address. ");
            opcodeDescriptions.Add(OpCodes.Stind_R8, "Stores a value of type float64 at a supplied address. ");
            opcodeDescriptions.Add(OpCodes.Stind_Ref, "Stores a object reference value at a supplied address. ");
            opcodeDescriptions.Add(OpCodes.Stloc, "Pops the current value from the top of the evaluation stack and stores it in a the local variable list at a specified index. ");
            opcodeDescriptions.Add(OpCodes.Stloc_0, "Pops the current value from the top of the evaluation stack and stores it in a the local variable list at index 0. ");
            opcodeDescriptions.Add(OpCodes.Stloc_1, "Pops the current value from the top of the evaluation stack and stores it in a the local variable list at index 1. ");
            opcodeDescriptions.Add(OpCodes.Stloc_2, "Pops the current value from the top of the evaluation stack and stores it in a the local variable list at index 2. ");
            opcodeDescriptions.Add(OpCodes.Stloc_3, "Pops the current value from the top of the evaluation stack and stores it in a the local variable list at index 3. ");
            opcodeDescriptions.Add(OpCodes.Stloc_S, "Pops the current value from the top of the evaluation stack and stores it in a the local variable list at index (short form). ");
            opcodeDescriptions.Add(OpCodes.Stobj, "Copies a value of a specified type from the evaluation stack into a supplied memory address. ");
            opcodeDescriptions.Add(OpCodes.Stsfld, "Replaces the value of a static field with a value from the evaluation stack. ");
            opcodeDescriptions.Add(OpCodes.Sub, "Subtracts one value from another and pushes the result onto the evaluation stack. ");
            opcodeDescriptions.Add(OpCodes.Sub_Ovf, "Subtracts one integer value from another, performs an overflow check, and pushes the result onto the evaluation stack. ");
            opcodeDescriptions.Add(OpCodes.Sub_Ovf_Un, "Subtracts one unsigned integer value from another, performs an overflow check, and pushes the result onto the evaluation stack. ");
            opcodeDescriptions.Add(OpCodes.Switch, "Implements a jump table. ");
            opcodeDescriptions.Add(OpCodes.Tailcall, "Performs a postfixed method call instruction such that the current method's stack frame is removed before the actual call instruction is executed. ");
            opcodeDescriptions.Add(OpCodes.Throw, "Throws the exception object currently on the evaluation stack. ");
            opcodeDescriptions.Add(OpCodes.Unaligned, "Indicates that an address currently atop the evaluation stack might not be aligned to the natural size of the immediately following ldind, stind, ldfld, stfld, ldobj, stobj, initblk, or cpblk instruction. ");
            opcodeDescriptions.Add(OpCodes.Unbox, "Converts the boxed representation of a value type to its unboxed form. ");
            opcodeDescriptions.Add(OpCodes.Unbox_Any, "Converts the boxed representation of a type specified in the instruction to its unboxed form. ");
            opcodeDescriptions.Add(OpCodes.Volatile, "Specifies that an address currently atop the evaluation stack might be volatile, and the results of reading that location cannot be cached or that multiple stores to that location cannot be suppressed. ");
            opcodeDescriptions.Add(OpCodes.Xor, "Computes the bitwise XOR of the top two values on the evaluation stack, pushing the result onto the evaluation stack.");
        }


        private static string GetRTFForValue(object value, Type t)
        {
            if (value == null)
                return "null";

            if (t.IsEnum)
                return t.FullName + "." + Enum.GetName(t, value);
            else if (value is Type)
                return "typeof(" + ((Type)value).ToSignatureString() + ")";
            else if (value is string)
                return @"\cf3\" + "\"" + (string)(value) + @"\cf0\" + "\"";
            else
                return value + "";
        }

        public static string GetAttributesRTF(IList<CustomAttributeData> cads)
        {
            StringBuilder str = new StringBuilder();
            foreach (var cad in cads)
            {
                List<string> arguments = new List<string>();
                arguments.AddRange(cad.ConstructorArguments.Select(arg => GetRTFForValue(arg.Value, arg.ArgumentType)));
                arguments.AddRange(cad.NamedArguments.Select(arg => arg.MemberInfo.Name + "=" + GetRTFForValue(arg.TypedValue.Value, arg.TypedValue.ArgumentType)));

                string typeName = cad.Constructor.DeclaringType.ToSignatureString();
                str.Append("[" + @"\b " + typeName + @"\b0" + "(" + string.Join(", ", arguments.ToArray()) + ")]" + @"\line ");
            }

            return str.ToString();
        }

        public static List<CodeBlock> GetAttributesCodeBlocks(IList<CustomAttributeData> cads)
        {
            var blocks = new List<CodeBlock>();

            foreach (var cad in cads)
            {
                blocks.Add(new CodeBlock("["));
                blocks.Add(new TypeCodeBlock(cad.Constructor.DeclaringType));
                blocks.Add(new CodeBlock("("));

                for (int i = 0; i < cad.ConstructorArguments.Count; i++)
                {
                    var val = cad.ConstructorArguments[i].Value;
                    if (val != null)
                        val = val.CastTo(cad.ConstructorArguments[i].ArgumentType);
                    blocks.Add(new ValueCodeBlock(val));

                    if (i != cad.ConstructorArguments.Count - 1)
                        blocks.Add(new CodeBlock(", "));
                }

                if(cad.ConstructorArguments.Count > 0 && cad.NamedArguments.Count > 0)
                    blocks.Add(new CodeBlock(", "));

                for (int i = 0; i < cad.NamedArguments.Count; i++)
                {
                    var val = cad.NamedArguments[i].TypedValue.Value;
                    if (val != null)
                        val = val.CastTo(cad.NamedArguments[i].TypedValue.ArgumentType);

                    blocks.Add(new CodeBlock(cad.NamedArguments[i].MemberInfo.Name + " = "));
                    blocks.Add(new ValueCodeBlock(val));

                    if (i != cad.NamedArguments.Count - 1)
                        blocks.Add(new CodeBlock(", "));
                }
                blocks.Add(new CodeBlock(")]" + Environment.NewLine));
            }

            return blocks;

        }

        public static List<CodeBlock> GetAssemblyVisualization(this Assembly a)
        {
            var blocks = new List<CodeBlock>();
            blocks.AddRange(GetAttributesCodeBlocks(a.GetCustomAttributesDataInclSecurity()));

            blocks.Add(new CodeBlock(Environment.NewLine));
            blocks.Add(new CodeBlock(AliasManager.Instance.GetFullNameWithAlias(a, a.GetName().Name)));
            return blocks;
        }

        public static List<CodeBlock> GetMemberVisualization(this MemberInfo m)
        {
            var blocks = new List<CodeBlock>();
            blocks.AddRange(GetAttributesCodeBlocks(m.GetCustomAttributesDataInclSecurity()));

            blocks.Add(new CodeBlock(Environment.NewLine));
            blocks.Add(new CodeBlock(m.ToSignatureString()));
            return blocks;
        }

        public static List<CodeBlock> GetTypeVisualization(this Type m)
        {
            var blocks = new List<CodeBlock>();
            blocks.AddRange(GetAttributesCodeBlocks(m.GetCustomAttributesDataInclSecurity()));

            blocks.Add(new CodeBlock(Environment.NewLine));
            blocks.Add(new CodeBlock(m.ToSignatureString()));
            return blocks;
        }


        //public static string GetAssemblyVisualization(this Assembly a)
        //{
        //    string attrs = GetAttributesRTF(a.GetCustomAttributesDataInclSecurity());
        //    string text = AliasManager.Instance.GetFullNameWithAlias(a, a.GetName().Name);

        //    return VisualizerHelper.RTFHeader.Replace("@BODY@", attrs + @"\line " + text);
        //}

        //public static string GetMemberVisualization(this MemberInfo m)
        //{
        //    string attrs = GetAttributesRTF(m.GetCustomAttributesDataInclSecurity());
        //    string text = m.ToSignatureString();

        //    return VisualizerHelper.RTFHeader.Replace("@BODY@", attrs + @"\line " + text);
        //}

        //public static string GetTypeVisualization(this Type m)
        //{
        //    string attrs = GetAttributesRTF(m.GetCustomAttributesDataInclSecurity());
        //    string text = m.ToSignatureString(true);

        //    return VisualizerHelper.RTFHeader.Replace("@BODY@", attrs + @"\line " + text);
        //}
    }
}
