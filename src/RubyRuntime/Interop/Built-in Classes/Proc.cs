using Ruby.Runtime;
using Ruby.Interop;

namespace Ruby
{
    public partial class Proc
    {
         [InteropMethod("new")]
        public static object new_instance(params object[] args)
        {
            return Eval.Call(Ruby.Runtime.Init.rb_cProc, "new", args);
        }

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

         [InteropMethod("binding")]
        public object binding()
        {
            return Eval.Call0(this, "binding");
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

    }
}

