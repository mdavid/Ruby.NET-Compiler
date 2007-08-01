using Ruby.Runtime;
using Ruby.Interop;

namespace Ruby
{
    public partial class ThreadGroup
    {
         [InteropMethod("add")]
        public object add(object p1)
        {
            return Eval.Call1(this, "add", p1);
        }

         [InteropMethod("enclose")]
        public object enclose()
        {
            return Eval.Call0(this, "enclose");
        }

         [InteropMethod("enclosed?")]
        public object is_enclosed()
        {
            return Eval.Call0(this, "enclosed?");
        }

         [InteropMethod("list")]
        public object list()
        {
            return Eval.Call0(this, "list");
        }

    }
}

