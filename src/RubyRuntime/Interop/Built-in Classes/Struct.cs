using Ruby.Runtime;
using Ruby.Interop;

namespace Ruby
{
    public partial class Struct
    {
         [InteropMethod("new")]
        public static object new_instance(params object[] args)
        {
            return Eval.Call(Ruby.Runtime.Init.rb_cStruct, "new", args);
        }

         [InteropMethod("[]")]
        public object indexer(object p1)
        {
            return Eval.Call1(this, "[]", p1);
        }

         [InteropMethod("[]=")]
        public object setitem(object p1, object p2)
        {
            return Eval.Call2(this, "[]=", p1, p2);
        }

         [InteropMethod("all?")]
        public object is_all()
        {
            return Eval.Call0(this, "all?");
        }

         [InteropMethod("any?")]
        public object is_any()
        {
            return Eval.Call0(this, "any?");
        }

         [InteropMethod("collect")]
        public object collect()
        {
            return Eval.Call0(this, "collect");
        }

         [InteropMethod("detect")]
        public object detect(params object[] args)
        {
            return Eval.Call(this, "detect", args);
        }

         [InteropMethod("each")]
        public object each()
        {
            return Eval.Call0(this, "each");
        }

         [InteropMethod("each_pair")]
        public object each_pair()
        {
            return Eval.Call0(this, "each_pair");
        }

         [InteropMethod("each_with_index")]
        public object each_with_index()
        {
            return Eval.Call0(this, "each_with_index");
        }

         [InteropMethod("entries")]
        public object entries()
        {
            return Eval.Call0(this, "entries");
        }

         [InteropMethod("find")]
        public object find(params object[] args)
        {
            return Eval.Call(this, "find", args);
        }

         [InteropMethod("find_all")]
        public object find_all()
        {
            return Eval.Call0(this, "find_all");
        }

         [InteropMethod("grep")]
        public object grep(object p1)
        {
            return Eval.Call1(this, "grep", p1);
        }

         [InteropMethod("include?")]
        public object is_include(object p1)
        {
            return Eval.Call1(this, "include?", p1);
        }

         [InteropMethod("inject")]
        public object inject(params object[] args)
        {
            return Eval.Call(this, "inject", args);
        }

         [InteropMethod("length")]
        public object length()
        {
            return Eval.Call0(this, "length");
        }

         [InteropMethod("map")]
        public object map()
        {
            return Eval.Call0(this, "map");
        }

         [InteropMethod("max")]
        public object max()
        {
            return Eval.Call0(this, "max");
        }

         [InteropMethod("member?")]
        public object is_member(object p1)
        {
            return Eval.Call1(this, "member?", p1);
        }

         [InteropMethod("members")]
        public object members()
        {
            return Eval.Call0(this, "members");
        }

         [InteropMethod("min")]
        public object min()
        {
            return Eval.Call0(this, "min");
        }

         [InteropMethod("partition")]
        public object partition()
        {
            return Eval.Call0(this, "partition");
        }

         [InteropMethod("reject")]
        public object reject()
        {
            return Eval.Call0(this, "reject");
        }

         [InteropMethod("select")]
        public object select(params object[] args)
        {
            return Eval.Call(this, "select", args);
        }

         [InteropMethod("size")]
        public object size()
        {
            return Eval.Call0(this, "size");
        }

         [InteropMethod("sort")]
        public object sort()
        {
            return Eval.Call0(this, "sort");
        }

         [InteropMethod("sort_by")]
        public object sort_by()
        {
            return Eval.Call0(this, "sort_by");
        }

         [InteropMethod("values")]
        public object values()
        {
            return Eval.Call0(this, "values");
        }

         [InteropMethod("values_at")]
        public object values_at(params object[] args)
        {
            return Eval.Call(this, "values_at", args);
        }

         [InteropMethod("zip")]
        public object zip(params object[] args)
        {
            return Eval.Call(this, "zip", args);
        }

    }
}

