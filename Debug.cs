using System;
using System.Collections.Generic;
using System.Text;

namespace Fuck;

public class Debug
{
    private static int SimpleInstruction(string instruction, int offset)
    {
        Console.WriteLine(instruction);
        return offset + 1;
    }
    private static int ConstantInstruction(string instruction, Chunk chunk, int offset)
    {
        byte constant = chunk.Code[offset + 1];
        Console.WriteLine($"{instruction, -16} {constant, 4} '{chunk.Constants[constant].ToString()}'");
        return offset + 2;
    }

    public static int DisassembleInstruction(Chunk chunk, int offset)
    {
        Console.Write($"{offset:D4} ");
        if (offset > 0 && chunk.Lines[offset] == chunk.Lines[offset - 1])
        {
            Console.Write("   | ");
        }
        else {
            Console.Write($"{chunk.Lines[offset], 4} ");
        }

        OpCode instruction = (OpCode)chunk.Code[offset];
        switch(instruction)
        {
            case OpCode.LoadConstant:
                return ConstantInstruction("LOAD_CONST", chunk, offset);
            case OpCode.Add:
                return SimpleInstruction("ADD", offset);
            case OpCode.Subtract:
                return SimpleInstruction("SUBTRACT", offset);
            case OpCode.Multiply:
                return SimpleInstruction("MULTIPLY", offset);
            case OpCode.Divide:
                return SimpleInstruction("DIVIDE", offset);
            case OpCode.Negate:
                return SimpleInstruction("NEGATE", offset);
            case OpCode.Return:
                return SimpleInstruction("RETURN", offset);
            default:
                Console.WriteLine($"<unknown opcode: {instruction}>");
                return offset + 1;
        }
    }

    public static void DisassembleChunk(Chunk chunk, string name)
    {
        Console.WriteLine($"== {name} ==");
        for(int offset = 0; offset < chunk.Count;)
        {
            offset = DisassembleInstruction(chunk, offset);
        }
    }
}
