#nullable disable
using System.Diagnostics.CodeAnalysis;


namespace Fuck;

public enum Precedence {
	None,
	Assignment,
	Or,
	And,
	Equality,
	Comparison,
	Term,
	Factor,
	Unary,
	Call,
	Primary
}

public delegate void ParseFunction();
public struct ParseRule(ParseFunction prefix, ParseFunction infix, Precedence prec)
{
	public ParseFunction Prefix = prefix;
	public ParseFunction Infix = infix;
	public Precedence Precedence = prec;
}

public class Compiler
{
	public ParseRule GetRule(TokenType type)
	{
		//you should look at how this is implemented in
		//crafting interpreters lol
		return type switch
		{
			TokenType.OpenParen	=> new(Grouping, null, Precedence.None),

			TokenType.Minus		=> new(Unary, Binary, Precedence.Term),
			TokenType.Plus		=> new(null, Binary, Precedence.Term),

			TokenType.Slash		=> new(null, Binary, Precedence.Factor),
            TokenType.Star		=> new(null, Binary, Precedence.Factor),
			
            TokenType.False		=> new(Literal, null, Precedence.None),
            TokenType.True		=> new(Literal, null, Precedence.None),
            TokenType.Nil		=> new(Literal, null, Precedence.None),

            TokenType.Number	=> new(Number, null, Precedence.None),
			_					=> new(null, null, Precedence.None)
		};
	}

	public void Literal()
	{
		switch (Previous.Type)
        {
            case (TokenType.False): Emit(OpCode.LoadFalse); break;
            case (TokenType.True): Emit(OpCode.LoadTrue); break;
            case (TokenType.Nil): Emit(OpCode.LoadNil); break;
        }
	}

	public TokenStream TokenStream = null!;
	public Token Previous = new(); //kys

	private Chunk Chunk = null!;

	private bool HadError = false;
	private bool PanicMode = true;

	private void ErrorAt(Token token, string message)
	{
		if (PanicMode) return;
		PanicMode = true;
		
		Console.Error.Write($"[line {token.Line}] Error");

		if (token.Type == TokenType.EOF)
		{
			Console.Error.Write(" at end");
		}
		else if(token.Type != TokenType.Error)
		{
			Console.Error.Write($" at '{token}'");
		}

		Console.Error.WriteLine($": {message}");
		HadError = true;
	}

	private void Error(string message)
	{
		ErrorAt(Previous, message);
	}

	private void ErrorAtCurrent(string message)
	{
		ErrorAt(TokenStream.Current, message);
	}

	private Token Peek(int offset = 0) => TokenStream.Peek(offset);

	private void Advance()
	{
		Previous = TokenStream.Current;
		while (true)
		{
			Token current = TokenStream.Advance();
			if(current.Type != TokenType.Error)
                break;
			ErrorAtCurrent(current.Value);
		}
	}
	private void Consume(TokenType type, string error_message)
	{
		if(TokenStream.Current.Type == type)
		{
			Advance();
			return;
		}

		ErrorAtCurrent(error_message);
	}

	private byte MakeConstant(Value value)
	{
		int c = Chunk.PushConstant(value);
		if(c > byte.MaxValue)
		{
			Error("Too many constants in chunk.");
			return 0;
		}

		return (byte)c;
	}

	private void EmitConstant(Value value) => EmitPair(OpCode.LoadConstant, MakeConstant(value));

	private void Grouping()
	{
		Expression();
		Consume(TokenType.CloseParen, "Expected ')' after expression.");
	}

	private void Number()
	{
		double value = double.Parse(Previous.Value);
		EmitConstant(new(value));
	}

	private void Binary()
	{
		TokenType op = Previous.Type;
		ParseRule rule = GetRule(op);
		ParsePrecedence((Precedence)(rule.Precedence + 1));

		switch (op)
		{
			case TokenType.Plus:	Emit(OpCode.Add); break;
			case TokenType.Minus:	Emit(OpCode.Subtract); break;
			case TokenType.Star:	Emit(OpCode.Multiply); break;
			case TokenType.Slash:	Emit(OpCode.Divide); break;
			default: return; //Unreachable
		}
	}

	private void Unary()
	{
		TokenType op = Previous.Type;

		ParsePrecedence(Precedence.Assignment);

		switch (op)
		{
			case TokenType.Minus:
				Emit(OpCode.Negate);
				break;
			default:
				return; //Unreachable
		}
	}

	private void Expression()
	{
		ParsePrecedence(Precedence.Assignment);
	}

	private void ParsePrecedence(Precedence prec)
	{
		Advance();
		ParseFunction prefix_rule = GetRule(Previous.Type).Prefix;
		if(prefix_rule == null)
		{
			Error("Expected expression.");
			return;
		}

		prefix_rule();

		while(prec <= GetRule(TokenStream.Current.Type).Precedence)
		{
			Advance();
			ParseFunction infix_rule = GetRule(Previous.Type).Infix;
			infix_rule();
		}
	}

	private void EmitReturn() => Emit(OpCode.Return);

	private void EmitPair(OpCode code, byte b)
	{
		Emit(code);
		Emit(b);
	}

	private void Emit(byte b) => Chunk.Write(b, Previous.Line);
	private void Emit(OpCode code) => Emit((byte)code);

	public bool Compile(TokenStream tokens, [NotNullWhen(true)] out Chunk chunk)
	{
		TokenStream = tokens;
		PanicMode = false;
		HadError = false;
		Chunk = new();

		//Advance();
		Expression();
		Consume(TokenType.EOF, "Expected end of file.");
		Emit(OpCode.Return);

		chunk = HadError ? null! : Chunk;
		return !HadError;
	}
}
