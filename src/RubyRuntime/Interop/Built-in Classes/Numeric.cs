using Ruby.Runtime;
using Ruby.Interop;

namespace Ruby
{
    public partial class Numeric
    {
         [InteropMethod("+@")]
        public static object operator+(Numeric p1)
        {
            return Eval.Call0(p1, "+@");
        }

         [InteropMethod("-@")]
        public static object operator-(Numeric p1)
        {
            return Eval.Call0(p1, "-@");
        }

         [InteropMethod("<")]
        public static object operator<(Numeric p1, object p2)
        {
            return Eval.Call1(p1, "<", p2);
        }

         [InteropMethod("<=")]
        public static object operator<=(Numeric p1, object p2)
        {
            return Eval.Call1(p1, "<=", p2);
        }

         [InteropMethod("<=>")]
        public object cmp(object p1)
        {
            return Eval.Call1(this, "<=>", p1);
        }

         [InteropMethod(">")]
        public static object operator>(Numeric p1, object p2)
        {
            return Eval.Call1(p1, ">", p2);
        }

         [InteropMethod(">=")]
        public static object operator>=(Numeric p1, object p2)
        {
            return Eval.Call1(p1, ">=", p2);
        }

         [InteropMethod("abs")]
        public object abs()
        {
            return Eval.Call0(this, "abs");
        }

         [InteropMethod("between?")]
        public object is_between(object p1, object p2)
        {
            return Eval.Call2(this, "between?", p1, p2);
        }

         [InteropMethod("ceil")]
        public object ceil()
        {
            return Eval.Call0(this, "ceil");
        }

         [InteropMethod("coerce")]
        public object coerce(object p1)
        {
            return Eval.Call1(this, "coerce", p1);
        }

         [InteropMethod("div")]
        public object div(object p1)
        {
            return Eval.Call1(this, "div", p1);
        }

         [InteropMethod("divmod")]
        public object divmod(object p1)
        {
            return Eval.Call1(this, "divmod", p1);
        }

         [InteropMethod("floor")]
        public object floor()
        {
            return Eval.Call0(this, "floor");
        }

         [InteropMethod("integer?")]
        public object is_integer()
        {
            return Eval.Call0(this, "integer?");
        }

         [InteropMethod("modulo")]
        public object modulo(object p1)
        {
            return Eval.Call1(this, "modulo", p1);
        }

         [InteropMethod("nonzero?")]
        public object is_nonzero()
        {
            return Eval.Call0(this, "nonzero?");
        }

         [InteropMethod("quo")]
        public object quo(object p1)
        {
            return Eval.Call1(this, "quo", p1);
        }

         [InteropMethod("remainder")]
        public object remainder(object p1)
        {
            return Eval.Call1(this, "remainder", p1);
        }

         [InteropMethod("round")]
        public object round()
        {
            return Eval.Call0(this, "round");
        }

         [InteropMethod("singleton_method_added")]
        public object singleton_method_added(object p1)
        {
            return Eval.Call1(this, "singleton_method_added", p1);
        }

         [InteropMethod("step")]
        public object step(params object[] args)
        {
            return Eval.Call(this, "step", args);
        }

         [InteropMethod("to_int")]
        public object to_int()
        {
            return Eval.Call0(this, "to_int");
        }

         [InteropMethod("truncate")]
        public object truncate()
        {
            return Eval.Call0(this, "truncate");
        }

         [InteropMethod("zero?")]
        public object is_zero()
        {
            return Eval.Call0(this, "zero?");
        }

    }
}

