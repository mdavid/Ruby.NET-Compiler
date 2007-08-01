using Ruby.Runtime;
using Ruby.Interop;

namespace Ruby
{
    public partial class Range
    {
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

         [InteropMethod("begin")]
        public object begin()
        {
            return Eval.Call0(this, "begin");
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

         [InteropMethod("each_with_index")]
        public object each_with_index()
        {
            return Eval.Call0(this, "each_with_index");
        }

         [InteropMethod("end")]
        public object end()
        {
            return Eval.Call0(this, "end");
        }

         [InteropMethod("entries")]
        public object entries()
        {
            return Eval.Call0(this, "entries");
        }

         [InteropMethod("exclude_end?")]
        public object is_exclude_end()
        {
            return Eval.Call0(this, "exclude_end?");
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

         [InteropMethod("first")]
        public object first()
        {
            return Eval.Call0(this, "first");
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

         [InteropMethod("last")]
        public object last()
        {
            return Eval.Call0(this, "last");
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
        public object select()
        {
            return Eval.Call0(this, "select");
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

         [InteropMethod("step")]
        public object step(params object[] args)
        {
            return Eval.Call(this, "step", args);
        }

         [InteropMethod("zip")]
        public object zip(params object[] args)
        {
            return Eval.Call(this, "zip", args);
        }

    }
}

