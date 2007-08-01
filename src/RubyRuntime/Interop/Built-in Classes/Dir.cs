using Ruby.Runtime;
using Ruby.Interop;

namespace Ruby
{
    public partial class Dir
    {
         [InteropMethod("[]")]
        public static object indexer(object p1)
        {
            return Eval.Call1(Ruby.Runtime.Init.rb_cDir, "[]", p1);
        }

         [InteropMethod("chdir")]
        public static object chdir(params object[] args)
        {
            return Eval.Call(Ruby.Runtime.Init.rb_cDir, "chdir", args);
        }

         [InteropMethod("chroot")]
        public static object chroot(object p1)
        {
            return Eval.Call1(Ruby.Runtime.Init.rb_cDir, "chroot", p1);
        }

         [InteropMethod("delete")]
        public static object delete(object p1)
        {
            return Eval.Call1(Ruby.Runtime.Init.rb_cDir, "delete", p1);
        }

         [InteropMethod("foreach")]
        public static object ForEach(object p1)
        {
            return Eval.Call1(Ruby.Runtime.Init.rb_cDir, "foreach", p1);
        }

         [InteropMethod("getwd")]
        public static object getwd()
        {
            return Eval.Call0(Ruby.Runtime.Init.rb_cDir, "getwd");
        }

         [InteropMethod("glob")]
        public static object glob(params object[] args)
        {
            return Eval.Call(Ruby.Runtime.Init.rb_cDir, "glob", args);
        }

         [InteropMethod("mkdir")]
        public static object mkdir(params object[] args)
        {
            return Eval.Call(Ruby.Runtime.Init.rb_cDir, "mkdir", args);
        }

         [InteropMethod("open")]
        public static object open(object p1)
        {
            return Eval.Call1(Ruby.Runtime.Init.rb_cDir, "open", p1);
        }

         [InteropMethod("pwd")]
        public static object pwd()
        {
            return Eval.Call0(Ruby.Runtime.Init.rb_cDir, "pwd");
        }

         [InteropMethod("rmdir")]
        public static object rmdir(object p1)
        {
            return Eval.Call1(Ruby.Runtime.Init.rb_cDir, "rmdir", p1);
        }

         [InteropMethod("unlink")]
        public static object unlink(object p1)
        {
            return Eval.Call1(Ruby.Runtime.Init.rb_cDir, "unlink", p1);
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

         [InteropMethod("close")]
        public object close()
        {
            return Eval.Call0(this, "close");
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

         [InteropMethod("path")]
        public object path()
        {
            return Eval.Call0(this, "path");
        }

         [InteropMethod("pos")]
        public object pos()
        {
            return Eval.Call0(this, "pos");
        }

         [InteropMethod("pos=")]
        public object set_pos(object p1)
        {
            return Eval.Call1(this, "pos=", p1);
        }

         [InteropMethod("read")]
        public object read()
        {
            return Eval.Call0(this, "read");
        }

         [InteropMethod("reject")]
        public object reject()
        {
            return Eval.Call0(this, "reject");
        }

         [InteropMethod("rewind")]
        public object rewind()
        {
            return Eval.Call0(this, "rewind");
        }

         [InteropMethod("seek")]
        public object seek(object p1)
        {
            return Eval.Call1(this, "seek", p1);
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

         [InteropMethod("tell")]
        public object tell()
        {
            return Eval.Call0(this, "tell");
        }

         [InteropMethod("zip")]
        public object zip(params object[] args)
        {
            return Eval.Call(this, "zip", args);
        }

    }
}

