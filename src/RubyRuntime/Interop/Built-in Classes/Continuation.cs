using Ruby.Runtime;
using Ruby.Interop;

namespace Ruby
{
    public partial class Continuation
    {
         [InteropMethod("[]")]
        public object indexer(params object[] args)
        {
            return Eval.Call(this, "[]", args);
        }

         [InteropMethod("call")]
        public object call(params object[] args)
        {
            return Eval.Call(this, "call", args);
        }

    }
}

