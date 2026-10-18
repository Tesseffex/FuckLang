using System;
using System.Collections.Generic;
using System.Text;

namespace Fuck;

public enum OpCode : byte
{
    LoadConstant,
    LoadNil,
    LoadTrue,
    LoadFalse,

    Add,
    Subtract,
    Multiply,
    Divide,

    Negate,

    Return,

}