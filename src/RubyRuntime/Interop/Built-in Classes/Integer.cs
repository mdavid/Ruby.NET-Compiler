using Ruby.Runtime;
using Ruby.Interop;

namespace Ruby
{
    public partial class Integer
    {
         [InteropMethod("induced_from")]
        public static object induced_from(object p1)
        {
            return Eval.Call1(Ruby.Runtime.Init.rb_cInteger, "induced_from", p1);
        }

         [InteropMethod("chr")]
        public object chr()
        {
            return Eval.Call0(this, "chr");
        }

         [InteropMethod("downto")]
        public object downto(object p1)
        {
            return Eval.Call1(this, "downto", p1);
        }

         [InteropMethod("next")]
        public object next()
        {
            return Eval.Call0(this, "next");
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

         [InteropMethod("succ")]
        public object succ()
        {
            return Eval.Call0(this, "succ");
        }

         [InteropMethod("times")]
        public object times()
        {
            return Eval.Call0(this, "times");
        }

         [InteropMethod("to_i")]
        public object to_i()
        {
            return Eval.Call0(this, "to_i");
        }

         [InteropMethod("upto")]
        public object upto(object p1)
        {
            return Eval.Call1(this, "upto", p1);
        }

    }
}

