using System;
using System.Collections.Generic;
using System.Text;

namespace Fuck;

public enum InterpretResult
{
    Ok,
    CompileError,
    RuntimeError,
}

public class VM
{
    public const int MAX_STACK = 256;
    public Chunk Chunk = null!;
    public int InstructionPointer = 0;
    public Value[] Stack = new Value[MAX_STACK];
    public int StackTop;

    public void ResetStack()
    {
        StackTop = -1;
    }

    public void Push(Value value)
    {
        Stack[++StackTop] = value;
    }

    public Value Pop() => Stack[StackTop--];

    public InterpretResult Run()
    {
        ResetStack();
        Value _read_constant() => Chunk.Constants[Chunk.Code[InstructionPointer++]];

        while(true)
        {
            /*
            Console.Write("STACK: ");
            if (StackTop >= 0)
            {
                for (int i = 0; i <= StackTop; i++)
                    Console.Write($"[{Stack[StackTop]}] ");
                Console.WriteLine();
            }
            else
            {
                Console.WriteLine("<EMPTY>");
            }
            */

            OpCode instruction;
            switch(instruction = (OpCode)Chunk.Code[InstructionPointer++])
            {
                case OpCode.Return:
                    Console.WriteLine($"Returned: {Pop()}");
                    return InterpretResult.Ok;
                case OpCode.Negate:
                    Push(new(-Pop().V));
                    break;
                case OpCode.LoadConstant:
                    Push(_read_constant());
                    break;
            }
        }
    }

    public InterpretResult Interpret(Chunk chunk)
    {
        Chunk = chunk;
        InstructionPointer = 0;
        return Run();
    }
}
