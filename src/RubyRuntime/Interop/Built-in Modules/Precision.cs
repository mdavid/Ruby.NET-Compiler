using Ruby.Runtime;
using Ruby.Interop;

namespace Ruby
{
    public partial class Precision
    {
         [InteropMethod("included")]
        public static object included(object p1)
        {
            return Eval.Call1(Ruby.Runtime.Init.rb_mPrecision, "included", p1);
        }

    }
}

