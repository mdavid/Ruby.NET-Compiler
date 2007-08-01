using Ruby.Runtime;
using Ruby.Interop;

namespace Ruby
{
    public partial class SystemExit
    {
         [InteropMethod("status")]
        public object status()
        {
            return Eval.Call0(this, "status");
        }

         [InteropMethod("success?")]
        public object is_success()
        {
            return Eval.Call0(this, "success?");
        }

    }
}

