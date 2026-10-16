using System;
using System.Collections.Generic;
using System.Text;

namespace Fuck;
public class Chunk
{
    public List<byte> Code = [];
    public List<Value> Constants = [];
    public List<int> Lines = [];

    public int Count => Code.Count;

    public void Write(byte b, int line)
    {
        Code.Add(b);
        Lines.Add(line);
    }
    public void Write(OpCode code, int line) => Write((byte)code, line);

    public int PushConstant(Value value)
    {
        Constants.Add(value);
        return Constants.Count - 1;
    }
}
