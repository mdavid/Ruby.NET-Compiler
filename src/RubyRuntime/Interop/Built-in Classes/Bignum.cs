using Ruby.Runtime;
using Ruby.Interop;

namespace Ruby
{
    public partial class Bignum
    {
         [InteropMethod("%")]
        public static object operator%(Bignum p1, object p2)
        {
            return Eval.Call1(p1, "%", p2);
        }

         [InteropMethod("&")]
        public static object operator&(Bignum p1, object p2)
        {
            return Eval.Call1(p1, "&", p2);
        }

         [InteropMethod("*")]
        public static object operator*(Bignum p1, object p2)
        {
            return Eval.Call1(p1, "*", p2);
        }

         [InteropMethod("**")]
        public object pow(object p1)
        {
            return Eval.Call1(this, "**", p1);
        }

         [InteropMethod("+")]
        public static object operator+(Bignum p1, object p2)
        {
            return Eval.Call1(p1, "+", p2);
        }

         [InteropMethod("-")]
        public static object operator-(Bignum p1, object p2)
        {
            return Eval.Call1(p1, "-", p2);
        }

         [InteropMethod("/")]
        public static object operator/(Bignum p1, object p2)
        {
            return Eval.Call1(p1, "/", p2);
        }

         [InteropMethod("<<")]
        public object lshift(object p1)
        {
            return Eval.Call1(this, "<<", p1);
        }

         [InteropMethod(">>")]
        public object rshift(object p1)
        {
            return Eval.Call1(this, ">>", p1);
        }

         [InteropMethod("[]")]
        public object indexer(object p1)
        {
            return Eval.Call1(this, "[]", p1);
        }

         [InteropMethod("^")]
        public static object operator^(Bignum p1, object p2)
        {
            return Eval.Call1(p1, "^", p2);
        }

         [InteropMethod("size")]
        public object size()
        {
            return Eval.Call0(this, "size");
        }

         [InteropMethod("to_f")]
        public object to_f()
        {
            return Eval.Call0(this, "to_f");
        }

         [InteropMethod("|")]
        public static object operator|(Bignum p1, object p2)
        {
            return Eval.Call1(p1, "|", p2);
        }

         [InteropMethod("~")]
        public static object operator~(Bignum p1)
        {
            return Eval.Call0(p1, "~");
        }

    }
}

