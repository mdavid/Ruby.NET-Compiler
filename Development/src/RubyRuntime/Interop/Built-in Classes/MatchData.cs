using Ruby.Runtime;
using Ruby.Interop;

namespace Ruby
{
    public partial class MatchData
    {
         [InteropMethod("[]")]
        public object indexer(params object[] args)
        {
            return Eval.Call(this, "[]", args);
        }

         [InteropMethod("begin")]
        public object begin(object p1)
        {
            return Eval.Call1(this, "begin", p1);
        }

         [InteropMethod("captures")]
        public object captures()
        {
            return Eval.Call0(this, "captures");
        }

         [InteropMethod("end")]
        public object end(object p1)
        {
            return Eval.Call1(this, "end", p1);
        }

         [InteropMethod("length")]
        public object length()
        {
            return Eval.Call0(this, "length");
        }

         [InteropMethod("offset")]
        public object offset(object p1)
        {
            return Eval.Call1(this, "offset", p1);
        }

         [InteropMethod("post_match")]
        public object post_match()
        {
            return Eval.Call0(this, "post_match");
        }

         [InteropMethod("pre_match")]
        public object pre_match()
        {
            return Eval.Call0(this, "pre_match");
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

         [InteropMethod("string")]
        public object String()
        {
            return Eval.Call0(this, "string");
        }

         [InteropMethod("values_at")]
        public object values_at(params object[] args)
        {
            return Eval.Call(this, "values_at", args);
        }

    }
}

