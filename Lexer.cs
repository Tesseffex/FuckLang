using System;
using System.Collections.Generic;
using System.Text;

namespace Fuck;

public enum TokenType
{
    //Single char tokens
    OpenParen, CloseParen,
    OpenBrace, CloseBrace,
    Comma, Dot, Minus, Plus,
    Semicolon, Slash, Star,

    //One/two char tokens
    Exclaim, ExEqual,
    Equal, EqualEqual,

    //Comparison
    Greater, Lesser,
    GreaterEqual, LesserEqual,

    //Literals
    Identifier, String, Number,

    //Keywords
    True, False, And, Or, //boolean logic
    If, Else, For, While, //control logic
    Class, Super, This,   //OOP
    Var, Func, Print, Nil, //other
    Return, //return

    Error, EOF
}

public struct Token
{
    public TokenType Type;
    public string Value;
    public int Line;

    public readonly bool HasValue => Value != null;

    public readonly override string ToString() => HasValue ? Value : Type.ToString();
}

public class Lexer
{
    public string Source = null!;
    public int Start = 0;
    public int Current = 0;
    public int Line = 1;

    private bool IsAtEnd => Current >= Source.Length;

    public Token MakeToken(TokenType type, bool put_value = false)
    {
        return new Token
        {
            Type = type,
            Value = put_value ? Source.Substring(Start, Current - Start) : null!,
            Line = Line
        };
    }

    public Token ErrorToken(string message)
    {
        return new Token
        {
            Type = TokenType.Error,
            Value = message,
            Line = Line
        };
    }

    private char Advance() => Source[Current++];

    private bool Match(char expect)
    {
        if (IsAtEnd) return false;
        if (Source[Current] != expect) return false;

        Current++;
        return true;
    }

    private char Peek(int offset = 0) => Current >= Source.Length ? '\0' : Source[Current + offset];

    private void SkipWhitespace()
    {
        while (true)
        {
            char c = Peek();
            switch (c)
            {
                case ' ' or '\r' or '\t':
                    Advance();
                    continue;
                case '\n':
                    Line++;
                    Advance();
                    continue;

                case '/': //comment
                    if(Peek(1) == '/')
                    {
                        while (Peek() != '\n' && !IsAtEnd) Advance();
                    }
                    else
                    {
                        return;
                    }
                    continue;
            }
            break;
        }
    }

    public Token ReadString()
    {
        while(Peek() != '"' && !IsAtEnd)
        {
            if (Peek() == '\n') Line++;
            Advance();
        }

        if (IsAtEnd) return ErrorToken("Unterminated string");

        Advance(); // absorb closing quote

        return MakeToken(TokenType.String, true);
    }

    private bool IsDigit(char c) => !IsAtEnd && char.IsAsciiDigit(c);

    public Token ReadNumber()
    {
        while (IsDigit(Peek())) Advance();

        if(Peek() == '.' && char.IsAscii(Peek(1)))
        {
            //Absorb .
            Advance();

            while (IsDigit(Peek())) Advance();
        }

        return MakeToken(TokenType.Number, true);
    }

    private TokenType CheckKeyword(int start, string rest, TokenType type)
    {
        if (Current - Start == start + rest.Length && rest.Equals(Source.AsSpan(Start + start, rest.Length), StringComparison.Ordinal))
            return type;

        return TokenType.Identifier;
    }

    private TokenType IdentifierType()
    {
        switch (Source[Start])
        {
            case 'a': return CheckKeyword(1, "nd", TokenType.And);
            case 'c': return CheckKeyword(1, "lass", TokenType.Class);
            case 'e': return CheckKeyword(1, "lse", TokenType.Else);
            case 'i': return CheckKeyword(1, "f", TokenType.If);
            case 'n': return CheckKeyword(1, "il", TokenType.Nil);
            case 'o': return CheckKeyword(1, "r", TokenType.Or);
            case 'p': return CheckKeyword(1, "rint", TokenType.Print);
            case 'r': return CheckKeyword(1, "eturn", TokenType.Return);
            case 's': return CheckKeyword(1, "uper", TokenType.Super);
            case 'v': return CheckKeyword(1, "ar", TokenType.Var);
            case 'w': return CheckKeyword(1, "while", TokenType.While);

            case 'f':
                if(Current - Start > 1)
                {
                    switch (Source[Start + 1])
                    {
                        case 'a': return CheckKeyword(2, "lse", TokenType.False);
                        case 'o': return CheckKeyword(2, "r", TokenType.Or);
                        case 'u': return CheckKeyword(2, "nc", TokenType.Func);
                    }
                }
                break;

            case 't':
                if(Current - Start > 1)
                {
                    switch (Source[Start + 1])
                    {
                        case 'r': return CheckKeyword(2, "ue", TokenType.True);
                        case 'h': return CheckKeyword(2, "is", TokenType.This);
                    }
                }
                break;
        }
        return TokenType.Identifier;
    }

    public Token ReadIdentifier()
    {
        while (IsAlpha(Peek()) || char.IsAsciiDigit(Peek())) Advance();
        TokenType type = IdentifierType();
        return MakeToken(type, type == TokenType.Identifier);
    }

    private static bool IsAlpha(char c) => char.IsAsciiLetter(c) || c == '_';

    public Token ScanToken()
    {
        SkipWhitespace();
        Start = Current;
        if (IsAtEnd) return MakeToken(TokenType.EOF);

        char c = Advance();

        if (IsAlpha(c)) return ReadIdentifier();
        if (char.IsAsciiDigit(c)) return ReadNumber();

        switch (c)
        {
            case '(': return MakeToken(TokenType.OpenParen);
            case ')': return MakeToken(TokenType.CloseParen);
            case '{': return MakeToken(TokenType.OpenBrace);
            case '}': return MakeToken(TokenType.CloseBrace);

            case ';': return MakeToken(TokenType.Semicolon);
            case ',': return MakeToken(TokenType.Comma);
            case '.': return MakeToken(TokenType.Dot);

            case '+': return MakeToken(TokenType.Plus);
            case '-': return MakeToken(TokenType.Minus);
            case '*': return MakeToken(TokenType.Star);
            case '/': return MakeToken(TokenType.Slash);

            case '!': return MakeToken(Match('=') ? TokenType.ExEqual : TokenType.Exclaim);
            case '=': return MakeToken(Match('=') ? TokenType.EqualEqual : TokenType.Equal);
            case '<': return MakeToken(Match('=') ? TokenType.LesserEqual : TokenType.Lesser);
            case '>': return MakeToken(Match('=') ? TokenType.GreaterEqual : TokenType.Greater);
            case '"': return ReadString();
        }

        return ErrorToken("Unexpected character.");
    }

    public List<Token> Lex(string source)
    {
        Source = source;
        Start = 0;
        Current = 0;
        List<Token> tokens = [];

        while (true)
        {
            Token tk = ScanToken();
            tokens.Add(tk);
            if (tk.Type == TokenType.EOF) break;
        }

        return tokens;
    }
}
