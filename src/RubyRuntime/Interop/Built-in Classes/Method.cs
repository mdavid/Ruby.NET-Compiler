using Ruby.Runtime;
using Ruby.Interop;

namespace Ruby
{
    public partial class Method
    {
         [InteropMethod("[]")]
        public object indexer(params object[] args)
        {
            return Eval.Call(this, "[]", args);
        }

         [InteropMethod("arity")]
        public object arity()
        {
            return Eval.Call0(this, "arity");
        }

         [InteropMethod("call")]
        public object call(params object[] args)
        {
            return Eval.Call(this, "call", args);
        }

         [InteropMethod("to_proc")]
        public object to_proc()
        {
            return Eval.Call0(this, "to_proc");
        }

         [InteropMethod("unbind")]
        public object unbind()
        {
            return Eval.Call0(this, "unbind");
        }

    }
}

