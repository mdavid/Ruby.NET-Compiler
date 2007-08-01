using Ruby.Runtime;
using Ruby.Interop;

namespace Ruby
{
    public partial class TrueClass
    {
         [InteropMethod("&")]
        public static object operator&(TrueClass p1, object p2)
        {
            return Eval.Call1(p1, "&", p2);
        }

         [InteropMethod("^")]
        public static object operator^(TrueClass p1, object p2)
        {
            return Eval.Call1(p1, "^", p2);
        }

         [InteropMethod("|")]
        public static object operator|(TrueClass p1, object p2)
        {
            return Eval.Call1(p1, "|", p2);
        }

    }
}

