public class RubyCompiler
{
    public static void Main(string[] args)
    {
        Ruby.Compiler.Compiler.InteropWarnings = true;
        Ruby.Compiler.Compiler.Process(args);
    }
}
