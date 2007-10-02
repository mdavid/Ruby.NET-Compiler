class RubyMain
{
    public static void Main(string[] args)
    {
        Ruby.Compiler.Compiler.InteropWarnings = false;
        Ruby.Compiler.RubyEntry.Process(args);
    }
}
