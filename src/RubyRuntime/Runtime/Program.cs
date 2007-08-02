using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Ruby;


namespace Ruby.Runtime
{
    [UsedByRubyCompiler]
    public class Program
    {
        internal static Dictionary<string, System.Type> programs = new Dictionary<string, System.Type>();
        internal static Stack<Proc> end_procs = new Stack<Proc>();

        [UsedByRubyCompiler]
        public static void AddProgram(string name, System.Type type)
        {
            if (!programs.ContainsKey(name))
                programs.Add(name, type);
        }

        [UsedByRubyCompiler]
        public static void ruby_stop()
        {
            while (end_procs.Count > 0)
            {
                Proc end = end_procs.Pop();
                try
                {
                    Proc.rb_yield(end, null, new ArgList());
                    //end.yield(null, new ArgList());
                }
                catch (RubyException e)
                {
                    e.Report();
                }
            }
        }

        [UsedByRubyCompiler]
        public static object End(Proc block)
        {
            foreach (Proc p in end_procs)
                if (p.body.GetType() == block.body.GetType())
                    return null;

            end_procs.Push(block);
            return null;
        }


    }
}
