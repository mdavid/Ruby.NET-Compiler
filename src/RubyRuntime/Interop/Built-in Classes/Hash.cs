using Ruby.Runtime;
using Ruby.Interop;

namespace Ruby
{
    public partial class Hash
    {
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

         [InteropMethod("default")]
        public object Default(params object[] args)
        {
            return Eval.Call(this, "default", args);
        }

         [InteropMethod("default=")]
        public object set_default(object p1)
        {
            return Eval.Call1(this, "default=", p1);
        }

         [InteropMethod("default_proc")]
        public object default_proc()
        {
            return Eval.Call0(this, "default_proc");
        }

         [InteropMethod("delete")]
        public object delete(object p1)
        {
            return Eval.Call1(this, "delete", p1);
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

         [InteropMethod("each_key")]
        public object each_key()
        {
            return Eval.Call0(this, "each_key");
        }

         [InteropMethod("each_pair")]
        public object each_pair()
        {
            return Eval.Call0(this, "each_pair");
        }

         [InteropMethod("each_value")]
        public object each_value()
        {
            return Eval.Call0(this, "each_value");
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

         [InteropMethod("has_key?")]
        public object is_has_key(object p1)
        {
            return Eval.Call1(this, "has_key?", p1);
        }

         [InteropMethod("has_value?")]
        public object is_has_value(object p1)
        {
            return Eval.Call1(this, "has_value?", p1);
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

         [InteropMethod("invert")]
        public object invert()
        {
            return Eval.Call0(this, "invert");
        }

         [InteropMethod("key?")]
        public object is_key(object p1)
        {
            return Eval.Call1(this, "key?", p1);
        }

         [InteropMethod("keys")]
        public object keys()
        {
            return Eval.Call0(this, "keys");
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

         [InteropMethod("merge")]
        public object merge(object p1)
        {
            return Eval.Call1(this, "merge", p1);
        }

         [InteropMethod("merge!")]
        public object do_merge(object p1)
        {
            return Eval.Call1(this, "merge!", p1);
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

         [InteropMethod("rehash")]
        public object rehash()
        {
            return Eval.Call0(this, "rehash");
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

         [InteropMethod("select")]
        public object select(params object[] args)
        {
            return Eval.Call(this, "select", args);
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

         [InteropMethod("store")]
        public object store(object p1, object p2)
        {
            return Eval.Call2(this, "store", p1, p2);
        }

         [InteropMethod("to_hash")]
        public object to_hash()
        {
            return Eval.Call0(this, "to_hash");
        }

         [InteropMethod("update")]
        public object update(object p1)
        {
            return Eval.Call1(this, "update", p1);
        }

         [InteropMethod("value?")]
        public object is_value(object p1)
        {
            return Eval.Call1(this, "value?", p1);
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

