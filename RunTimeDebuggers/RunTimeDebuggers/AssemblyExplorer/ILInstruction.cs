using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection.Emit;
using System.Linq;
using RunTimeDebuggers.Helpers;
using System.Reflection;

namespace RunTimeDebuggers.AssemblyExplorer
{
    public class ILInstruction
    {

        public OpCode Code { get; set; }
        public object Operand { get; set; }

        public int Offset { get; set; }
        public int Size { get; set; }



        public override bool Equals(object obj)
        {
            ILInstruction il = obj as ILInstruction;
            if (il == null)
                return false;
            else
                return this.Code == il.Code && this.Offset == il.Offset && this.Operand == il.Operand;
        }

        public override int GetHashCode()
        {
            return this.Code.GetHashCode() ^ this.Offset.GetHashCode() ^ this.Operand.GetHashCode();
        }


        public ILInstruction Previous { get; set; }

        public ILInstruction Next { get; set; }

        public int GetEndOffset()
        {
            return Offset + Size;
        }


        /// <summary>
        /// Returns the value pushed onto the stack if the value is a constant, otherwise null
        /// </summary>
        public object ConstantValuePutOnStack
        {
            get
            {
                if (Code == OpCodes.Ldstr || Code == OpCodes.Ldtoken ||
                    Code == OpCodes.Ldc_I4 || Code == OpCodes.Ldc_I4_S ||
                    Code == OpCodes.Ldc_R4 || Code == OpCodes.Ldc_I8 ||
                    Code == OpCodes.Ldc_R8 || Code == OpCodes.Ldnull)
                    return Operand;
                else if (Code == OpCodes.Ldc_I4_0)
                    return 0;
                else if (Code == OpCodes.Ldc_I4_1)
                    return 1;
                else if (Code == OpCodes.Ldc_I4_2)
                    return 2;
                else if (Code == OpCodes.Ldc_I4_3)
                    return 3;
                else if (Code == OpCodes.Ldc_I4_4)
                    return 4;
                else if (Code == OpCodes.Ldc_I4_5)
                    return 5;
                else if (Code == OpCodes.Ldc_I4_6)
                    return 6;
                else if (Code == OpCodes.Ldc_I4_7)
                    return 7;
                else if (Code == OpCodes.Ldc_I4_8)
                    return 8;
                else if (Code == OpCodes.Ldc_I4_M1)
                    return -1;
                return null;
            }
        }

        public string GetOperandString()
        {
            if (Code.FlowControl == FlowControl.Branch || Code.FlowControl == FlowControl.Cond_Branch)
                if (Operand is Int32[])
                    return string.Join(",", ((Int32[])(Operand)).Select(i => i.ToString("x4")).ToArray());
                else
                    return Convert.ToInt32(Operand).ToString("x4");

            else if (Operand is Type)

                return ((Type)Operand).ToSignatureString();

            else if (Operand is MemberInfo)
                return ((MemberInfo)Operand).DeclaringType.ToSignatureString() + "::" + ((MemberInfo)Operand).ToSignatureString();

            else if (Operand is string)
                return Operand + "";
            return "";
        }

        public override string ToString()
        {
            return Code.ToString();
        }
    }
}
