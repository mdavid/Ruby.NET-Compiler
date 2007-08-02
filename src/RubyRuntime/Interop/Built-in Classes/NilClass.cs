using Ruby.Runtime;
using Ruby.Interop;

namespace Ruby
{
    public partial class NilClass
    {
         [InteropMethod("&")]
        public static object operator&(NilClass p1, object p2)
        {
            return Eval.Call1(p1, "&", p2);
        }

         [InteropMethod("^")]
        public static object operator^(NilClass p1, object p2)
        {
            return Eval.Call1(p1, "^", p2);
        }

         [InteropMethod("to_f")]
        public object to_f()
        {
            return Eval.Call0(this, "to_f");
        }

         [InteropMethod("to_i")]
        public object to_i()
        {
            return Eval.Call0(this, "to_i");
        }

         [InteropMethod("|")]
        public static object operator|(NilClass p1, object p2)
        {
            return Eval.Call1(p1, "|", p2);
        }

    }
}

