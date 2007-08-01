using Ruby.Runtime;
using Ruby.Interop;

namespace Ruby
{
    public partial class Marshal
    {
         [InteropMethod("dump")]
        public static object dump(params object[] args)
        {
            return Eval.Call(Ruby.Runtime.Init.rb_mMarshal, "dump", args);
        }

         [InteropMethod("load")]
        public static object load(params object[] args)
        {
            return Eval.Call(Ruby.Runtime.Init.rb_mMarshal, "load", args);
        }

         [InteropMethod("restore")]
        public static object restore(params object[] args)
        {
            return Eval.Call(Ruby.Runtime.Init.rb_mMarshal, "restore", args);
        }

    }
}

