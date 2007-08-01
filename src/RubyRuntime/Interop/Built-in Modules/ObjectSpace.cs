using Ruby.Runtime;
using Ruby.Interop;

namespace Ruby
{
    public partial class ObjectSpace
    {
         [InteropMethod("_id2ref")]
        public static object _id2ref(object p1)
        {
            return Eval.Call1(Ruby.Runtime.Init.rb_mObjectSpace, "_id2ref", p1);
        }

         [InteropMethod("add_finalizer")]
        public static object add_finalizer(object p1)
        {
            return Eval.Call1(Ruby.Runtime.Init.rb_mObjectSpace, "add_finalizer", p1);
        }

         [InteropMethod("call_finalizer")]
        public static object call_finalizer(object p1)
        {
            return Eval.Call1(Ruby.Runtime.Init.rb_mObjectSpace, "call_finalizer", p1);
        }

         [InteropMethod("define_finalizer")]
        public static object define_finalizer(params object[] args)
        {
            return Eval.Call(Ruby.Runtime.Init.rb_mObjectSpace, "define_finalizer", args);
        }

         [InteropMethod("each_object")]
        public static object each_object(params object[] args)
        {
            return Eval.Call(Ruby.Runtime.Init.rb_mObjectSpace, "each_object", args);
        }

         [InteropMethod("finalizers")]
        public static object finalizers()
        {
            return Eval.Call0(Ruby.Runtime.Init.rb_mObjectSpace, "finalizers");
        }

         [InteropMethod("garbage_collect")]
        public static object garbage_collect()
        {
            return Eval.Call0(Ruby.Runtime.Init.rb_mObjectSpace, "garbage_collect");
        }

         [InteropMethod("remove_finalizer")]
        public static object remove_finalizer(object p1)
        {
            return Eval.Call1(Ruby.Runtime.Init.rb_mObjectSpace, "remove_finalizer", p1);
        }

         [InteropMethod("undefine_finalizer")]
        public static object undefine_finalizer(object p1)
        {
            return Eval.Call1(Ruby.Runtime.Init.rb_mObjectSpace, "undefine_finalizer", p1);
        }

    }
}

