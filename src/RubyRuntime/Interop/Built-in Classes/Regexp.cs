using Ruby.Runtime;
using Ruby.Interop;

namespace Ruby
{
    public partial class Regexp
    {
         [InteropMethod("compile")]
        public static object compile(params object[] args)
        {
            return Eval.Call(Ruby.Runtime.Init.rb_cRegexp, "compile", args);
        }

         [InteropMethod("escape")]
        public static object escape(params object[] args)
        {
            return Eval.Call(Ruby.Runtime.Init.rb_cRegexp, "escape", args);
        }

         [InteropMethod("last_match")]
        public static object last_match(params object[] args)
        {
            return Eval.Call(Ruby.Runtime.Init.rb_cRegexp, "last_match", args);
        }

         [InteropMethod("quote")]
        public static object quote(params object[] args)
        {
            return Eval.Call(Ruby.Runtime.Init.rb_cRegexp, "quote", args);
        }

         [InteropMethod("union")]
        public static object union(params object[] args)
        {
            return Eval.Call(Ruby.Runtime.Init.rb_cRegexp, "union", args);
        }

         [InteropMethod("casefold?")]
        public object is_casefold()
        {
            return Eval.Call0(this, "casefold?");
        }

         [InteropMethod("kcode")]
        public object kcode()
        {
            return Eval.Call0(this, "kcode");
        }

         [InteropMethod("match")]
        public object match(object p1)
        {
            return Eval.Call1(this, "match", p1);
        }

         [InteropMethod("options")]
        public object options()
        {
            return Eval.Call0(this, "options");
        }

         [InteropMethod("source")]
        public object source()
        {
            return Eval.Call0(this, "source");
        }

         [InteropMethod("~")]
        public static object operator~(Regexp p1)
        {
            return Eval.Call0(p1, "~");
        }

    }
}

