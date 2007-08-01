using Ruby.Runtime;
using Ruby.Interop;

namespace Ruby
{
    public partial class String
    {
         [InteropMethod("%")]
        public static object operator%(String p1, object p2)
        {
            return Eval.Call1(p1, "%", p2);
        }

         [InteropMethod("*")]
        public static object operator*(String p1, object p2)
        {
            return Eval.Call1(p1, "*", p2);
        }

         [InteropMethod("+")]
        public static object operator+(String p1, object p2)
        {
            return Eval.Call1(p1, "+", p2);
        }

         [InteropMethod("<")]
        public static object operator<(String p1, object p2)
        {
            return Eval.Call1(p1, "<", p2);
        }

         [InteropMethod("<<")]
        public object lshift(object p1)
        {
            return Eval.Call1(this, "<<", p1);
        }

         [InteropMethod("<=")]
        public static object operator<=(String p1, object p2)
        {
            return Eval.Call1(p1, "<=", p2);
        }

         [InteropMethod("<=>")]
        public object cmp(object p1)
        {
            return Eval.Call1(this, "<=>", p1);
        }

         [InteropMethod(">")]
        public static object operator>(String p1, object p2)
        {
            return Eval.Call1(p1, ">", p2);
        }

         [InteropMethod(">=")]
        public static object operator>=(String p1, object p2)
        {
            return Eval.Call1(p1, ">=", p2);
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

         [InteropMethod("between?")]
        public object is_between(object p1, object p2)
        {
            return Eval.Call2(this, "between?", p1, p2);
        }

         [InteropMethod("capitalize")]
        public object capitalize()
        {
            return Eval.Call0(this, "capitalize");
        }

         [InteropMethod("capitalize!")]
        public object do_capitalize()
        {
            return Eval.Call0(this, "capitalize!");
        }

         [InteropMethod("casecmp")]
        public object casecmp(object p1)
        {
            return Eval.Call1(this, "casecmp", p1);
        }

         [InteropMethod("center")]
        public object center(params object[] args)
        {
            return Eval.Call(this, "center", args);
        }

         [InteropMethod("chomp")]
        public object chomp(params object[] args)
        {
            return Eval.Call(this, "chomp", args);
        }

         [InteropMethod("chomp!")]
        public object do_chomp(params object[] args)
        {
            return Eval.Call(this, "chomp!", args);
        }

         [InteropMethod("chop")]
        public object chop()
        {
            return Eval.Call0(this, "chop");
        }

         [InteropMethod("chop!")]
        public object do_chop()
        {
            return Eval.Call0(this, "chop!");
        }

         [InteropMethod("collect")]
        public object collect()
        {
            return Eval.Call0(this, "collect");
        }

         [InteropMethod("concat")]
        public object concat(object p1)
        {
            return Eval.Call1(this, "concat", p1);
        }

         [InteropMethod("count")]
        public object count(params object[] args)
        {
            return Eval.Call(this, "count", args);
        }

         [InteropMethod("crypt")]
        public object crypt(object p1)
        {
            return Eval.Call1(this, "crypt", p1);
        }

         [InteropMethod("delete")]
        public object delete(params object[] args)
        {
            return Eval.Call(this, "delete", args);
        }

         [InteropMethod("delete!")]
        public object do_delete(params object[] args)
        {
            return Eval.Call(this, "delete!", args);
        }

         [InteropMethod("detect")]
        public object detect(params object[] args)
        {
            return Eval.Call(this, "detect", args);
        }

         [InteropMethod("downcase")]
        public object downcase()
        {
            return Eval.Call0(this, "downcase");
        }

         [InteropMethod("downcase!")]
        public object do_downcase()
        {
            return Eval.Call0(this, "downcase!");
        }

         [InteropMethod("dump")]
        public object dump()
        {
            return Eval.Call0(this, "dump");
        }

         [InteropMethod("each")]
        public object each(params object[] args)
        {
            return Eval.Call(this, "each", args);
        }

         [InteropMethod("each_byte")]
        public object each_byte()
        {
            return Eval.Call0(this, "each_byte");
        }

         [InteropMethod("each_line")]
        public object each_line(params object[] args)
        {
            return Eval.Call(this, "each_line", args);
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

         [InteropMethod("gsub")]
        public object gsub(params object[] args)
        {
            return Eval.Call(this, "gsub", args);
        }

         [InteropMethod("gsub!")]
        public object do_gsub(params object[] args)
        {
            return Eval.Call(this, "gsub!", args);
        }

         [InteropMethod("hex")]
        public object hex()
        {
            return Eval.Call0(this, "hex");
        }

         [InteropMethod("include?")]
        public object is_include(object p1)
        {
            return Eval.Call1(this, "include?", p1);
        }

         [InteropMethod("index")]
        public object index(params object[] args)
        {
            return Eval.Call(this, "index", args);
        }

         [InteropMethod("inject")]
        public object inject(params object[] args)
        {
            return Eval.Call(this, "inject", args);
        }

         [InteropMethod("insert")]
        public object insert(object p1, object p2)
        {
            return Eval.Call2(this, "insert", p1, p2);
        }

         [InteropMethod("intern")]
        public object intern()
        {
            return Eval.Call0(this, "intern");
        }

         [InteropMethod("length")]
        public object length()
        {
            return Eval.Call0(this, "length");
        }

         [InteropMethod("ljust")]
        public object ljust(params object[] args)
        {
            return Eval.Call(this, "ljust", args);
        }

         [InteropMethod("lstrip")]
        public object lstrip()
        {
            return Eval.Call0(this, "lstrip");
        }

         [InteropMethod("lstrip!")]
        public object do_lstrip()
        {
            return Eval.Call0(this, "lstrip!");
        }

         [InteropMethod("map")]
        public object map()
        {
            return Eval.Call0(this, "map");
        }

         [InteropMethod("match")]
        public object match(object p1)
        {
            return Eval.Call1(this, "match", p1);
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

         [InteropMethod("next")]
        public object next()
        {
            return Eval.Call0(this, "next");
        }

         [InteropMethod("next!")]
        public object do_next()
        {
            return Eval.Call0(this, "next!");
        }

         [InteropMethod("oct")]
        public object oct()
        {
            return Eval.Call0(this, "oct");
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

         [InteropMethod("rindex")]
        public object rindex(params object[] args)
        {
            return Eval.Call(this, "rindex", args);
        }

         [InteropMethod("rjust")]
        public object rjust(params object[] args)
        {
            return Eval.Call(this, "rjust", args);
        }

         [InteropMethod("rstrip")]
        public object rstrip()
        {
            return Eval.Call0(this, "rstrip");
        }

         [InteropMethod("rstrip!")]
        public object do_rstrip()
        {
            return Eval.Call0(this, "rstrip!");
        }

         [InteropMethod("scan")]
        public object scan(object p1)
        {
            return Eval.Call1(this, "scan", p1);
        }

         [InteropMethod("select")]
        public object select()
        {
            return Eval.Call0(this, "select");
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

         [InteropMethod("sort_by")]
        public object sort_by()
        {
            return Eval.Call0(this, "sort_by");
        }

         [InteropMethod("split")]
        public object split(params object[] args)
        {
            return Eval.Call(this, "split", args);
        }

         [InteropMethod("squeeze")]
        public object squeeze(params object[] args)
        {
            return Eval.Call(this, "squeeze", args);
        }

         [InteropMethod("squeeze!")]
        public object do_squeeze(params object[] args)
        {
            return Eval.Call(this, "squeeze!", args);
        }

         [InteropMethod("strip")]
        public object strip()
        {
            return Eval.Call0(this, "strip");
        }

         [InteropMethod("strip!")]
        public object do_strip()
        {
            return Eval.Call0(this, "strip!");
        }

         [InteropMethod("sub")]
        public object sub(params object[] args)
        {
            return Eval.Call(this, "sub", args);
        }

         [InteropMethod("sub!")]
        public object do_sub(params object[] args)
        {
            return Eval.Call(this, "sub!", args);
        }

         [InteropMethod("succ")]
        public object succ()
        {
            return Eval.Call0(this, "succ");
        }

         [InteropMethod("succ!")]
        public object do_succ()
        {
            return Eval.Call0(this, "succ!");
        }

         [InteropMethod("sum")]
        public object sum(params object[] args)
        {
            return Eval.Call(this, "sum", args);
        }

         [InteropMethod("swapcase")]
        public object swapcase()
        {
            return Eval.Call0(this, "swapcase");
        }

         [InteropMethod("swapcase!")]
        public object do_swapcase()
        {
            return Eval.Call0(this, "swapcase!");
        }

         [InteropMethod("to_f")]
        public object to_f()
        {
            return Eval.Call0(this, "to_f");
        }

         [InteropMethod("to_i")]
        public object to_i(params object[] args)
        {
            return Eval.Call(this, "to_i", args);
        }

         [InteropMethod("to_str")]
        public object to_str()
        {
            return Eval.Call0(this, "to_str");
        }

         [InteropMethod("to_sym")]
        public object to_sym()
        {
            return Eval.Call0(this, "to_sym");
        }

         [InteropMethod("tr")]
        public object tr(object p1, object p2)
        {
            return Eval.Call2(this, "tr", p1, p2);
        }

         [InteropMethod("tr!")]
        public object do_tr(object p1, object p2)
        {
            return Eval.Call2(this, "tr!", p1, p2);
        }

         [InteropMethod("tr_s")]
        public object tr_s(object p1, object p2)
        {
            return Eval.Call2(this, "tr_s", p1, p2);
        }

         [InteropMethod("tr_s!")]
        public object do_tr_s(object p1, object p2)
        {
            return Eval.Call2(this, "tr_s!", p1, p2);
        }

         [InteropMethod("unpack")]
        public object unpack(object p1)
        {
            return Eval.Call1(this, "unpack", p1);
        }

         [InteropMethod("upcase")]
        public object upcase()
        {
            return Eval.Call0(this, "upcase");
        }

         [InteropMethod("upcase!")]
        public object do_upcase()
        {
            return Eval.Call0(this, "upcase!");
        }

         [InteropMethod("upto")]
        public object upto(object p1)
        {
            return Eval.Call1(this, "upto", p1);
        }

         [InteropMethod("zip")]
        public object zip(params object[] args)
        {
            return Eval.Call(this, "zip", args);
        }

    }
}

