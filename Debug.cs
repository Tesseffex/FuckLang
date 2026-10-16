using System;
using System.Collections.Generic;
using System.Text;

namespace Fuck;

public class Debug
{
    private static int SimpleInstruction(string instruction, StringBuilder sb, int offset)
    {
        sb.AppendLine(instruction);
        return offset + 1;
    }
    private static int ConstantInstruction(string instruction, Chunk chunk, StringBuilder sb, int offset)
    {
        byte constant = chunk.Code[offset + 1];
        sb.AppendLine($"{instruction, -16} {constant, 4} '{chunk.Constants[constant].ToString()}'");
        return offset + 2;
    }

    public static int DisassembleInstruction(Chunk chunk, StringBuilder sb, int offset)
    {
        sb.Append($"{offset:D4} ");
        if (offset > 0 && chunk.Lines[offset] == chunk.Lines[offset - 1])
        {
            sb.Append("   | ");
        }
        else {
            sb.Append($"{chunk.Lines[offset], 4} ");
        }

        OpCode instruction = (OpCode)chunk.Code[offset];
        switch(instruction)
        {
            case OpCode.LoadConstant:
                return ConstantInstruction("LOAD_CONST", chunk, sb, offset);
            case OpCode.Return:
                return SimpleInstruction("RETURN", sb, offset);
            case OpCode.Negate:
                return SimpleInstruction("NEGATE", sb, offset);
            default:
                sb.Append($"<unknown opcode: {instruction}>");
                return offset + 1;
        }
    }

    public static string DisassembleChunk(Chunk chunk, string name)
    {
        StringBuilder sb = new();
        sb.AppendLine($"== {name} ==");
        for(int offset = 0; offset < chunk.Count;)
        {
            offset = DisassembleInstruction(chunk, sb, offset);
        }

        return sb.ToString();
    }
}
