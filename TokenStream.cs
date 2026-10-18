using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace Fuck;

public class TokenStream(List<Token> list)
{
    private readonly List<Token> _list = list;
    public int Position { get; protected set; } = 0;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Token Peek(int offset = 0) => _list[Position + offset];

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Token Advance() => _list[Position++];

    public Token Current => _list[Position];

    public Token this[int position] => _list[position];
}
