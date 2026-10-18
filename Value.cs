using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace Fuck;

public enum ValueType : byte
{
    Nil,
    Boolean,
    Number
}

[StructLayout(LayoutKind.Explicit)]
public struct Value
{
    public Value(bool value)
    {
        Type = ValueType.Boolean;
        AsBoolean = value;
    }
    public Value(double number)
    {
        Type = ValueType.Number;
        AsNumber = number;
    }
    [FieldOffset(0)]
    public ValueType Type;
    [FieldOffset(1)]
    public double AsNumber;
    [FieldOffset(1)]
    public bool AsBoolean;

    public static Value Nil => new() { Type = ValueType.Nil, AsBoolean = false };

    public readonly bool Is(ValueType type) => Type == type;
    public readonly override string ToString() => Type switch
    {
        ValueType.Nil => "nil",
        ValueType.Number => AsNumber.ToString(),
        ValueType.Boolean => AsBoolean.ToString(),
        _ => "<invalid type>"
    };
}
