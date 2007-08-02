using Ruby.Runtime;
using Ruby.Interop;

namespace Ruby
{
    public partial class Symbol
    {
         [InteropMethod("all_symbols")]
        public static object all_symbols()
        {
            return Eval.Call0(Ruby.Runtime.Init.rb_cSymbol, "all_symbols");
        }

         [InteropMethod("id2name")]
        public object id2name()
        {
            return Eval.Call0(this, "id2name");
        }

         [InteropMethod("to_i")]
        public object to_i()
        {
            return Eval.Call0(this, "to_i");
        }

         [InteropMethod("to_int")]
        public object to_int()
        {
            return Eval.Call0(this, "to_int");
        }

         [InteropMethod("to_sym")]
        public object to_sym()
        {
            return Eval.Call0(this, "to_sym");
        }

    }
}

