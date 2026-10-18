namespace Fuck
{
    internal class Program
    {
        public static InterpretResult Interpret(string source)
        {
            List<Token> tokens = new Lexer().Lex(source);
            TokenStream stream = new(tokens);
            bool success = new Compiler().Compile(stream, out Chunk chunk);
            if (!success)
                return InterpretResult.CompileError;

            Debug.DisassembleChunk(chunk, "main");

            return new VM().Interpret(chunk);
        }
        static void RunFile(string path)
        {
            string source = File.ReadAllText(path);
            InterpretResult result = Interpret(source);

            if (result == InterpretResult.CompileError) Environment.Exit(65);
            if (result == InterpretResult.RuntimeError) Environment.Exit(70);
        }
        static void Repl()
        {
            while(true)
            {
                Console.Write("> ");
                string line = Console.ReadLine()!;
                if (line == null)
                {
                    Console.WriteLine();
                    break;
                }

                Interpret(line);
            }
        }
        static void Main(string[] args)
        {
            if(args.Length == 0)
            {
                Repl();
            }
            else if (args.Length == 1)
            {
                RunFile(args[0]);
            }
            else
            {
                Environment.Exit(64);
            }
            

            //chunk.Write(OpCode.Return, 1);

            //VM vm = new();

            //Debug.DisassembleChunk(chunk, "main");
            //vm.Interpret(chunk);

        }
    }
}
