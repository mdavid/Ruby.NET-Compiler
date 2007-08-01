using Ruby.Runtime;
using Ruby.Interop;

namespace Ruby
{
    public partial class Exception
    {
         [InteropMethod("backtrace")]
        public object backtrace()
        {
            return Eval.Call0(this, "backtrace");
        }

         [InteropMethod("exception")]
        public object exception(params object[] args)
        {
            return Eval.Call(this, "exception", args);
        }

         [InteropMethod("message")]
        public object message()
        {
            return Eval.Call0(this, "message");
        }

         [InteropMethod("set_backtrace")]
        public object set_backtrace(object p1)
        {
            return Eval.Call1(this, "set_backtrace", p1);
        }

         [InteropMethod("to_str")]
        public object to_str()
        {
            return Eval.Call0(this, "to_str");
        }

    }
}

