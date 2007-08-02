using Ruby.Runtime;
using Ruby.Interop;

namespace Ruby
{
    public partial class Math
    {
         [InteropMethod("acos")]
        public static object acos(object p1)
        {
            return Eval.Call1(Ruby.Runtime.Init.rb_mMath, "acos", p1);
        }

         [InteropMethod("acosh")]
        public static object acosh(object p1)
        {
            return Eval.Call1(Ruby.Runtime.Init.rb_mMath, "acosh", p1);
        }

         [InteropMethod("asin")]
        public static object asin(object p1)
        {
            return Eval.Call1(Ruby.Runtime.Init.rb_mMath, "asin", p1);
        }

         [InteropMethod("asinh")]
        public static object asinh(object p1)
        {
            return Eval.Call1(Ruby.Runtime.Init.rb_mMath, "asinh", p1);
        }

         [InteropMethod("atan")]
        public static object atan(object p1)
        {
            return Eval.Call1(Ruby.Runtime.Init.rb_mMath, "atan", p1);
        }

         [InteropMethod("atan2")]
        public static object atan2(object p1, object p2)
        {
            return Eval.Call2(Ruby.Runtime.Init.rb_mMath, "atan2", p1, p2);
        }

         [InteropMethod("atanh")]
        public static object atanh(object p1)
        {
            return Eval.Call1(Ruby.Runtime.Init.rb_mMath, "atanh", p1);
        }

         [InteropMethod("cos")]
        public static object cos(object p1)
        {
            return Eval.Call1(Ruby.Runtime.Init.rb_mMath, "cos", p1);
        }

         [InteropMethod("cosh")]
        public static object cosh(object p1)
        {
            return Eval.Call1(Ruby.Runtime.Init.rb_mMath, "cosh", p1);
        }

         [InteropMethod("erf")]
        public static object erf(object p1)
        {
            return Eval.Call1(Ruby.Runtime.Init.rb_mMath, "erf", p1);
        }

         [InteropMethod("erfc")]
        public static object erfc(object p1)
        {
            return Eval.Call1(Ruby.Runtime.Init.rb_mMath, "erfc", p1);
        }

         [InteropMethod("exp")]
        public static object exp(object p1)
        {
            return Eval.Call1(Ruby.Runtime.Init.rb_mMath, "exp", p1);
        }

         [InteropMethod("frexp")]
        public static object frexp(object p1)
        {
            return Eval.Call1(Ruby.Runtime.Init.rb_mMath, "frexp", p1);
        }

         [InteropMethod("hypot")]
        public static object hypot(object p1, object p2)
        {
            return Eval.Call2(Ruby.Runtime.Init.rb_mMath, "hypot", p1, p2);
        }

         [InteropMethod("ldexp")]
        public static object ldexp(object p1, object p2)
        {
            return Eval.Call2(Ruby.Runtime.Init.rb_mMath, "ldexp", p1, p2);
        }

         [InteropMethod("log")]
        public static object log(object p1)
        {
            return Eval.Call1(Ruby.Runtime.Init.rb_mMath, "log", p1);
        }

         [InteropMethod("log10")]
        public static object log10(object p1)
        {
            return Eval.Call1(Ruby.Runtime.Init.rb_mMath, "log10", p1);
        }

         [InteropMethod("sin")]
        public static object sin(object p1)
        {
            return Eval.Call1(Ruby.Runtime.Init.rb_mMath, "sin", p1);
        }

         [InteropMethod("sinh")]
        public static object sinh(object p1)
        {
            return Eval.Call1(Ruby.Runtime.Init.rb_mMath, "sinh", p1);
        }

         [InteropMethod("sqrt")]
        public static object sqrt(object p1)
        {
            return Eval.Call1(Ruby.Runtime.Init.rb_mMath, "sqrt", p1);
        }

         [InteropMethod("tan")]
        public static object tan(object p1)
        {
            return Eval.Call1(Ruby.Runtime.Init.rb_mMath, "tan", p1);
        }

         [InteropMethod("tanh")]
        public static object tanh(object p1)
        {
            return Eval.Call1(Ruby.Runtime.Init.rb_mMath, "tanh", p1);
        }

    }
}

