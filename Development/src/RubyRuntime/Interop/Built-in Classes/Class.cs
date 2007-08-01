using Ruby.Runtime;
using Ruby.Interop;

namespace Ruby
{
    public partial class Class
    {
         [InteropMethod("allocate")]
        public object allocate()
        {
            return Eval.Call0(this, "allocate");
        }

         [InteropMethod("new")]
        public object new_instance(params object[] args)
        {
            return Eval.Call(this, "new", args);
        }

         [InteropMethod("superclass")]
        public object superclass()
        {
            return Eval.Call0(this, "superclass");
        }

    }
}

