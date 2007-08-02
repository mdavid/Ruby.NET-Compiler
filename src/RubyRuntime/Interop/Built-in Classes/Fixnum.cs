using Ruby.Runtime;
using Ruby.Interop;

namespace Ruby
{
    public partial class Fixnum
    {
         [InteropMethod("%")]
        public static object operator%(Fixnum p1, object p2)
        {
            return Eval.Call1(p1, "%", p2);
        }

         [InteropMethod("&")]
        public static object operator&(Fixnum p1, object p2)
        {
            return Eval.Call1(p1, "&", p2);
        }

         [InteropMethod("*")]
        public static object operator*(Fixnum p1, object p2)
        {
            return Eval.Call1(p1, "*", p2);
        }

         [InteropMethod("**")]
        public object pow(object p1)
        {
            return Eval.Call1(this, "**", p1);
        }

         [InteropMethod("+")]
        public static object operator+(Fixnum p1, object p2)
        {
            return Eval.Call1(p1, "+", p2);
        }

         [InteropMethod("-")]
        public static object operator-(Fixnum p1, object p2)
        {
            return Eval.Call1(p1, "-", p2);
        }

         [InteropMethod("/")]
        public static object operator/(Fixnum p1, object p2)
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
        public static object operator^(Fixnum p1, object p2)
        {
            return Eval.Call1(p1, "^", p2);
        }

         [InteropMethod("id2name")]
        public object id2name()
        {
            return Eval.Call0(this, "id2name");
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

         [InteropMethod("to_sym")]
        public object to_sym()
        {
            return Eval.Call0(this, "to_sym");
        }

         [InteropMethod("|")]
        public static object operator|(Fixnum p1, object p2)
        {
            return Eval.Call1(p1, "|", p2);
        }

         [InteropMethod("~")]
        public static object operator~(Fixnum p1)
        {
            return Eval.Call0(p1, "~");
        }

    }
}

