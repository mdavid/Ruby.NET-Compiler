using Ruby.Runtime;
using Ruby.Interop;

namespace Ruby
{
    public partial class Array
    {
         [InteropMethod("&")]
        public static object operator&(Array p1, object p2)
        {
            return Eval.Call1(p1, "&", p2);
        }

         [InteropMethod("*")]
        public static object operator*(Array p1, object p2)
        {
            return Eval.Call1(p1, "*", p2);
        }

         [InteropMethod("+")]
        public static object operator+(Array p1, object p2)
        {
            return Eval.Call1(p1, "+", p2);
        }

         [InteropMethod("-")]
        public static object operator-(Array p1, object p2)
        {
            return Eval.Call1(p1, "-", p2);
        }

         [InteropMethod("<<")]
        public object lshift(object p1)
        {
            return Eval.Call1(this, "<<", p1);
        }

         [InteropMethod("<=>")]
        public object cmp(object p1)
        {
            return Eval.Call1(this, "<=>", p1);
        }

         [InteropMethod("[]")]
        public object indexer(params object[] args)
        {
            return Eval.Call(this, "[]", args);
        }

         [InteropMethod("[]=")]
        public object setitem(params object[] args)
        {
            return Eval.Call(this, "[]=", args);
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

         [InteropMethod("assoc")]
        public object assoc(object p1)
        {
            return Eval.Call1(this, "assoc", p1);
        }

         [InteropMethod("at")]
        public object at(object p1)
        {
            return Eval.Call1(this, "at", p1);
        }

         [InteropMethod("clear")]
        public object clear()
        {
            return Eval.Call0(this, "clear");
        }

         [InteropMethod("collect")]
        public object collect()
        {
            return Eval.Call0(this, "collect");
        }

         [InteropMethod("collect!")]
        public object do_collect()
        {
            return Eval.Call0(this, "collect!");
        }

         [InteropMethod("compact")]
        public object compact()
        {
            return Eval.Call0(this, "compact");
        }

         [InteropMethod("compact!")]
        public object do_compact()
        {
            return Eval.Call0(this, "compact!");
        }

         [InteropMethod("concat")]
        public object concat(object p1)
        {
            return Eval.Call1(this, "concat", p1);
        }

         [InteropMethod("delete")]
        public object delete(object p1)
        {
            return Eval.Call1(this, "delete", p1);
        }

         [InteropMethod("delete_at")]
        public object delete_at(object p1)
        {
            return Eval.Call1(this, "delete_at", p1);
        }

         [InteropMethod("delete_if")]
        public object delete_if()
        {
            return Eval.Call0(this, "delete_if");
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

         [InteropMethod("each_index")]
        public object each_index()
        {
            return Eval.Call0(this, "each_index");
        }

         [InteropMethod("each_with_index")]
        public object each_with_index()
        {
            return Eval.Call0(this, "each_with_index");
        }

         [InteropMethod("empty?")]
        public object is_empty()
        {
            return Eval.Call0(this, "empty?");
        }

         [InteropMethod("entries")]
        public object entries()
        {
            return Eval.Call0(this, "entries");
        }

         [InteropMethod("fetch")]
        public object fetch(params object[] args)
        {
            return Eval.Call(this, "fetch", args);
        }

         [InteropMethod("fill")]
        public object fill(params object[] args)
        {
            return Eval.Call(this, "fill", args);
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
        public object first(params object[] args)
        {
            return Eval.Call(this, "first", args);
        }

         [InteropMethod("flatten")]
        public object flatten()
        {
            return Eval.Call0(this, "flatten");
        }

         [InteropMethod("flatten!")]
        public object do_flatten()
        {
            return Eval.Call0(this, "flatten!");
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

         [InteropMethod("index")]
        public object index(object p1)
        {
            return Eval.Call1(this, "index", p1);
        }

         [InteropMethod("indexes")]
        public object indexes(params object[] args)
        {
            return Eval.Call(this, "indexes", args);
        }

         [InteropMethod("indices")]
        public object indices(params object[] args)
        {
            return Eval.Call(this, "indices", args);
        }

         [InteropMethod("inject")]
        public object inject(params object[] args)
        {
            return Eval.Call(this, "inject", args);
        }

         [InteropMethod("insert")]
        public object insert(params object[] args)
        {
            return Eval.Call(this, "insert", args);
        }

         [InteropMethod("join")]
        public object join(params object[] args)
        {
            return Eval.Call(this, "join", args);
        }

         [InteropMethod("last")]
        public object last(params object[] args)
        {
            return Eval.Call(this, "last", args);
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

         [InteropMethod("map!")]
        public object do_map()
        {
            return Eval.Call0(this, "map!");
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

         [InteropMethod("nitems")]
        public object nitems()
        {
            return Eval.Call0(this, "nitems");
        }

         [InteropMethod("pack")]
        public object pack(object p1)
        {
            return Eval.Call1(this, "pack", p1);
        }

         [InteropMethod("partition")]
        public object partition()
        {
            return Eval.Call0(this, "partition");
        }

         [InteropMethod("pop")]
        public object pop()
        {
            return Eval.Call0(this, "pop");
        }

         [InteropMethod("push")]
        public object push(params object[] args)
        {
            return Eval.Call(this, "push", args);
        }

         [InteropMethod("rassoc")]
        public object rassoc(object p1)
        {
            return Eval.Call1(this, "rassoc", p1);
        }

         [InteropMethod("reject")]
        public object reject()
        {
            return Eval.Call0(this, "reject");
        }

         [InteropMethod("reject!")]
        public object do_reject()
        {
            return Eval.Call0(this, "reject!");
        }

         [InteropMethod("replace")]
        public object replace(object p1)
        {
            return Eval.Call1(this, "replace", p1);
        }

         [InteropMethod("reverse")]
        public object reverse()
        {
            return Eval.Call0(this, "reverse");
        }

         [InteropMethod("reverse!")]
        public object do_reverse()
        {
            return Eval.Call0(this, "reverse!");
        }

         [InteropMethod("reverse_each")]
        public object reverse_each()
        {
            return Eval.Call0(this, "reverse_each");
        }

         [InteropMethod("rindex")]
        public object rindex(object p1)
        {
            return Eval.Call1(this, "rindex", p1);
        }

         [InteropMethod("select")]
        public object select()
        {
            return Eval.Call0(this, "select");
        }

         [InteropMethod("shift")]
        public object shift()
        {
            return Eval.Call0(this, "shift");
        }

         [InteropMethod("size")]
        public object size()
        {
            return Eval.Call0(this, "size");
        }

         [InteropMethod("slice")]
        public object slice(params object[] args)
        {
            return Eval.Call(this, "slice", args);
        }

         [InteropMethod("slice!")]
        public object do_slice(params object[] args)
        {
            return Eval.Call(this, "slice!", args);
        }

         [InteropMethod("sort")]
        public object sort()
        {
            return Eval.Call0(this, "sort");
        }

         [InteropMethod("sort!")]
        public object do_sort()
        {
            return Eval.Call0(this, "sort!");
        }

         [InteropMethod("sort_by")]
        public object sort_by()
        {
            return Eval.Call0(this, "sort_by");
        }

         [InteropMethod("to_ary")]
        public object to_ary()
        {
            return Eval.Call0(this, "to_ary");
        }

         [InteropMethod("transpose")]
        public object transpose()
        {
            return Eval.Call0(this, "transpose");
        }

         [InteropMethod("uniq")]
        public object uniq()
        {
            return Eval.Call0(this, "uniq");
        }

         [InteropMethod("uniq!")]
        public object do_uniq()
        {
            return Eval.Call0(this, "uniq!");
        }

         [InteropMethod("unshift")]
        public object unshift(params object[] args)
        {
            return Eval.Call(this, "unshift", args);
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

         [InteropMethod("|")]
        public static object operator|(Array p1, object p2)
        {
            return Eval.Call1(p1, "|", p2);
        }

    }
}

