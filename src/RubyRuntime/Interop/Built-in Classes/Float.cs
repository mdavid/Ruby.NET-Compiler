using Ruby.Runtime;
using Ruby.Interop;

namespace Ruby
{
    public partial class Float
    {
         [InteropMethod("induced_from")]
        public static object induced_from(object p1)
        {
            return Eval.Call1(Ruby.Runtime.Init.rb_cFloat, "induced_from", p1);
        }

         [InteropMethod("%")]
        public static object operator%(Float p1, object p2)
        {
            return Eval.Call1(p1, "%", p2);
        }

         [InteropMethod("*")]
        public static object operator*(Float p1, object p2)
        {
            return Eval.Call1(p1, "*", p2);
        }

         [InteropMethod("**")]
        public object pow(object p1)
        {
            return Eval.Call1(this, "**", p1);
        }

         [InteropMethod("+")]
        public static object operator+(Float p1, object p2)
        {
            return Eval.Call1(p1, "+", p2);
        }

         [InteropMethod("-")]
        public static object operator-(Float p1, object p2)
        {
            return Eval.Call1(p1, "-", p2);
        }

         [InteropMethod("/")]
        public static object operator/(Float p1, object p2)
        {
            return Eval.Call1(p1, "/", p2);
        }

         [InteropMethod("finite?")]
        public object is_finite()
        {
            return Eval.Call0(this, "finite?");
        }

         [InteropMethod("infinite?")]
        public object is_infinite()
        {
            return Eval.Call0(this, "infinite?");
        }

         [InteropMethod("nan?")]
        public object is_nan()
        {
            return Eval.Call0(this, "nan?");
        }

         [InteropMethod("prec")]
        public object prec(object p1)
        {
            return Eval.Call1(this, "prec", p1);
        }

         [InteropMethod("prec_f")]
        public object prec_f()
        {
            return Eval.Call0(this, "prec_f");
        }

         [InteropMethod("prec_i")]
        public object prec_i()
        {
            return Eval.Call0(this, "prec_i");
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

    }
}

