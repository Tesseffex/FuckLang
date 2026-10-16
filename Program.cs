namespace Fuck
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Chunk chunk = new();

            chunk.PushConstant(new Value(1.2));


            chunk.Write(OpCode.LoadConstant, 1);
            chunk.Write((byte)0, 1);
            chunk.Write(OpCode.Negate, 1);
            chunk.Write(OpCode.Return, 1);

            VM vm = new();

            Console.WriteLine(Debug.DisassembleChunk(chunk, "main"));
            vm.Interpret(chunk);

        }
    }
}
