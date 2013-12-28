using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Reflection.Emit;


namespace RunTimeDebuggers.AssemblyExplorer
{
    public class Disassembler
    {
        static Dictionary<short, OpCode> _opcodes = new Dictionary<short, OpCode>();
        static Disassembler()
        {
            Dictionary<short, OpCode> opcodes = new Dictionary<short, OpCode>();
            foreach (FieldInfo fi in typeof(OpCodes).GetFields
                                     (BindingFlags.Public | BindingFlags.Static))
                if (typeof(OpCode).IsAssignableFrom(fi.FieldType))
                {
                    OpCode code = (OpCode)fi.GetValue(null);   // Get field's value
                    if (code.OpCodeType != OpCodeType.Nternal)
                        _opcodes.Add(code.Value, code);
                }
        }

        private int _pos;
        private byte[] il;
        private Module module;
        private MethodBase method;

        public MethodBase Method
        {
            get { return method; }
        }

        private List<ILInstruction> instructions;

        public List<ILInstruction> Instructions
        {
            get { return instructions; }
        }

        public Disassembler(MethodBase m)
        {
            this.method = m;
            this.module = m.Module;
            
        }

        public void BuildInstructions()
        {
            MethodBody body = method.GetMethodBody();
            if (body != null)
            {
                Errors = new List<Error>();
                instructions = new List<ILInstruction>();
                _pos = 0;
                il = body.GetILAsByteArray();

                DisassemblyInstructionsAtCurrentPosition();

                while(validLandingZones.Count > 0)
                {
                    _pos = validLandingZones.Pop();
                    DisassemblyInstructionsAtCurrentPosition();
                }

                // order by offset
                instructions = instructions.OrderBy(i => i.Offset).ToList();

                // set up prev-next
                for (int i = 1; i < instructions.Count; i++)
                {
                    instructions[i - 1].Next = instructions[i];
                    instructions[i].Previous = instructions[i - 1];
                }
            }
        }

        private void DisassemblyInstructionsAtCurrentPosition()
        {
            while (_pos < il.Length)
            {
                if (positions.Contains(_pos)) // already done this position
                    break;

                DisassembleNextInstruction();
            }
        }

        public class Error
        {
            public int ErrorPosition { get; set; }
            public Exception Exception { get; set; }
        }
        public List<Error> Errors { get; private set; }




        private Stack<int> validLandingZones = new Stack<int>();
        private HashSet<int> positions = new HashSet<int>();

        void DisassembleNextInstruction()
        {
            int opStart = _pos;


            if (positions.Contains(opStart))
                return;


            try
            {
                AttemptDisassembleInstruction();
            }
            catch (Exception ex)
            {
                
                Errors.Add(new Error()
                {
                    ErrorPosition = opStart,
                    Exception = ex
                });

                if (validLandingZones.Count > 0)
                {
                    _pos = validLandingZones.Pop();
                    DisassembleNextInstruction();
                }
            }
        }

        private void AttemptDisassembleInstruction()
        {
            int opStart = _pos;
            OpCode code = ReadOpCode();
            object operand = ReadOperand(code);

            ILInstruction instruction = new ILInstruction()
            {
                Code = code,
                Offset = opStart,
                Operand = operand,

                Previous = instructions.LastOrDefault(),
                Size = _pos - opStart
            };

            if (instruction.Code.FlowControl == FlowControl.Branch || instruction.Code.FlowControl == FlowControl.Cond_Branch)
            {
                if (instruction.Code.OperandType != OperandType.InlineSwitch)
                {
                    int landingZone = (int)instruction.Operand;
                    if (!positions.Contains(landingZone))
                    {
                        validLandingZones.Push(landingZone);
                    }
                }
                else if (instruction.Code.OperandType == OperandType.InlineSwitch)
                {
                    foreach (int landingZone in (int[])instruction.Operand)
                    {
                        if (!positions.Contains(landingZone))
                        {
                            validLandingZones.Push(landingZone);
                        }
                    }
                }
            }

            positions.Add(instruction.Offset);
            instructions.Add(instruction);
        }

        private OpCode ReadOpCode()
        {
            byte byteCode = il[_pos++];
            if (_opcodes.ContainsKey(byteCode)) return _opcodes[byteCode];
            if (_pos == il.Length) throw new Exception("Unexpected end of IL");
            short shortCode = (short)(byteCode * 256 + il[_pos++]);
            if (!_opcodes.ContainsKey(shortCode))
                throw new Exception("Cannot find opcode " + shortCode);
            return _opcodes[shortCode];
        }

        private object ReadOperand(OpCode c)
        {
            int operandLength =
              c.OperandType == OperandType.InlineNone
                ? 0 :
              c.OperandType == OperandType.ShortInlineBrTarget ||
              c.OperandType == OperandType.ShortInlineI ||
              c.OperandType == OperandType.ShortInlineVar
                ? 1 :
              c.OperandType == OperandType.InlineVar
                ? 2 :
              c.OperandType == OperandType.InlineI8 ||
              c.OperandType == OperandType.InlineR
                ? 8 :
              c.OperandType == OperandType.InlineSwitch
                ? 4 * (BitConverter.ToInt32(il, _pos) + 1) :
                4;  // All others are 4 bytes
            if (_pos + operandLength > il.Length)
                throw new Exception("Unexpected end of IL");

            var result = GetOperand(c, operandLength);
            _pos += operandLength;
            return result;
        }

        private object GetOperand(OpCode c, int operandLength)
        {
            if (c.OperandType == OperandType.ShortInlineBrTarget)
                return GetShortRelativeTarget();
            else if (c.OperandType == OperandType.InlineSwitch)
                return GetSwitchTarget(operandLength);
            else if (operandLength == 0)
                return null;
            else if (operandLength == 1)
                return Get1ByteOperand(c);
            else if (operandLength == 2)
                return Get2ByteOperand(c);
            else if (operandLength == 4)
                return Get4ByteOperand(c);
            else if (operandLength == 8)
                return Get8ByteOperand(c);
            else
                return null;
        }

        private object Get1ByteOperand(OpCode c)
        {
            return il[_pos];
        }


        private object Get2ByteOperand(OpCode c)
        {
            short intOp = BitConverter.ToInt16(il, _pos);
            return intOp;
        }

        private object Get4ByteOperand(OpCode c)
        {
            int intOp = BitConverter.ToInt32(il, _pos);
            switch (c.OperandType)
            {
                case OperandType.InlineTok:
                case OperandType.InlineMethod:
                case OperandType.InlineField:
                case OperandType.InlineType:
                    MemberInfo mi;
                    try
                    {
                        Type[] genericTypeArguments = method.DeclaringType.ContainsGenericParameters ? method.DeclaringType.GetGenericArguments() : Type.EmptyTypes;
                        Type[] genericMethodArguments;
                        try
                        {
                            genericMethodArguments = method.IsGenericMethod && method.ContainsGenericParameters ? method.GetGenericArguments() : Type.EmptyTypes;
                        }
                        catch (Exception)
                        {
                            genericMethodArguments = Type.EmptyTypes;
                        }
                        mi = module.ResolveMember(intOp, genericTypeArguments, genericMethodArguments);
                    }
                    catch
                    {
                        return null;
                    }
                    return mi;

                case OperandType.InlineString:
                    string s = module.ResolveString(intOp);
                    return s;

                case OperandType.InlineBrTarget:
                    return (_pos + intOp + 4);
                default:
                    return intOp;
            }
        }

        private object Get8ByteOperand(OpCode c)
        {
            if (c.OperandType == OperandType.InlineI8)
                return BitConverter.ToInt64(il, _pos);
            else if (c.OperandType == OperandType.InlineR)
                return BitConverter.ToDouble(il, _pos);
            else
                return null;
        }


        private int GetShortRelativeTarget()
        {
            int absoluteTarget = _pos + (sbyte)il[_pos] + 1;
            return absoluteTarget;
        }

        private int[] GetSwitchTarget(int operandLength)
        {
            int targetCount = BitConverter.ToInt32(il, _pos);
            int[] targets = new int[targetCount];
            for (int i = 0; i < targetCount; i++)
            {
                int ilTarget = BitConverter.ToInt32(il, _pos + (i + 1) * 4);
                targets[i] = (_pos + ilTarget + operandLength);
            }
            return targets;
        }

    }
}
