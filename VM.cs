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
    public Chunk Chunk;
    public bool HadError;
    public int InstructionPointer = 0;
    public Value[] Stack = new Value[MAX_STACK];
    public int StackTop;

    public void ResetStack()
    {
        StackTop = 0;
    }

    public void Push(Value value)
    {
        Stack[StackTop++] = value;
    }

    public Value Pop() => Stack[--StackTop];
    public Value Peek(int offset = 0) => Stack[StackTop - 1 - offset];

    public void RuntimeError(string message)
    {
        HadError = true;
        Console.Error.WriteLine(message);
        int line = Chunk.Lines[InstructionPointer - 1];
        Console.WriteLine($"at line {line} in script");
        ResetStack();
    }

    public bool StackExpect(ValueType type, int offset = 0) => Peek(offset).Is(type);

    public InterpretResult Run()
    {
        ResetStack();
        Value _read_constant() => Chunk.Constants[Chunk.Code[InstructionPointer++]];

        while(true)
        {
            /*
            Console.WriteLine();
            Console.Write("STACK: ");
            if (StackTop > 0)
            {
                for (int i = 0; i < StackTop; i++)
                    Console.Write($"[{Stack[i]}] ");
                Console.WriteLine();
            }
            else
            {
                Console.WriteLine("<EMPTY>");
            }
            Debug.DisassembleInstruction(Chunk, InstructionPointer);
            */


            OpCode instruction;
            switch(instruction = (OpCode)Chunk.Code[InstructionPointer++])
            {
                case OpCode.Return:
                    Console.WriteLine($"Returned: {Pop()}");
                    return InterpretResult.Ok;
                case OpCode.Add:
                    {
                        if (
                            !StackExpect(ValueType.Number, 0)
                            || !StackExpect(ValueType.Number, 1)
                        )
                        {
                            RuntimeError("Operands must be numbers.");
                            break;
                        }
                        Value b = Pop();
                        Value a = Pop();
                        Push(new(a.AsNumber + b.AsNumber));
                        break;
                    }
                case OpCode.Subtract:
                    {
                        if (
                            !StackExpect(ValueType.Number, 0)
                            || !StackExpect(ValueType.Number, 1)
                        )
                        {
                            RuntimeError("Operands must be numbers.");
                            break;
                        }
                        Value b = Pop();
                        Value a = Pop();
                        Push(new(a.AsNumber - b.AsNumber));
                        break;
                    }
                case OpCode.Multiply:
                    {
                        if (
                            !StackExpect(ValueType.Number, 0)
                            || !StackExpect(ValueType.Number, 1)
                        )
                        {
                            RuntimeError("Operands must be numbers.");
                            break;
                        }
                        Value b = Pop();
                        Value a = Pop();
                        Push(new(a.AsNumber * b.AsNumber));
                        break;
                    }
                case OpCode.Divide:
                    {
                        if (
                            !StackExpect(ValueType.Number, 0)
                            || !StackExpect(ValueType.Number, 1)
                        )
                        {
                            RuntimeError("Operands must be numbers.");
                            break;
                        }
                        Value b = Pop();
                        Value a = Pop();
                        Push(new(a.AsNumber / b.AsNumber));
                        break;
                    }
                case OpCode.Negate:
                    if (!StackExpect(ValueType.Number))
                    {
                        RuntimeError("Operand must be a number.");
                        break;
                    }

                    Push(new(-Pop().AsNumber));
                    break;
                case OpCode.LoadConstant:
                    Push(_read_constant());
                    break;

                case OpCode.LoadNil:
                    Push(Value.Nil);
                    break;

                case OpCode.LoadTrue:
                    Push(new(true));
                    break;
                case OpCode.LoadFalse:
                    Push(new(false));
                    break;
            }

            if (HadError)
                return InterpretResult.RuntimeError;
        }
    }

    public InterpretResult Interpret(Chunk chunk)
    {
        Chunk = chunk;
        InstructionPointer = 0;
        return Run();
    }
}
