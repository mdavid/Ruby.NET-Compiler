using Ruby.Runtime;
using Ruby.Interop;

namespace Ruby
{
    public partial class Signal
    {
         [InteropMethod("list")]
        public static object list()
        {
            return Eval.Call0(Ruby.Runtime.Init.rb_mSignal, "list");
        }

         [InteropMethod("trap")]
        public static object trap(params object[] args)
        {
            return Eval.Call(Ruby.Runtime.Init.rb_mSignal, "trap", args);
        }

    }
}

