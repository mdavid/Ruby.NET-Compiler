using Ruby.Runtime;
using Ruby.Interop;

namespace Ruby
{
    public partial class UnboundMethod
    {
         [InteropMethod("arity")]
        public object arity()
        {
            return Eval.Call0(this, "arity");
        }

         [InteropMethod("bind")]
        public object bind(object p1)
        {
            return Eval.Call1(this, "bind", p1);
        }

    }
}

