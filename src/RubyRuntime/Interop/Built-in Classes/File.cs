using Ruby.Runtime;
using Ruby.Interop;

namespace Ruby
{
    public partial class File
    {
         [InteropMethod("basename")]
        public static object basename(params object[] args)
        {
            return Eval.Call(Ruby.Runtime.Init.rb_cFile, "basename", args);
        }

         [InteropMethod("blockdev?")]
        public static object is_blockdev(object p1)
        {
            return Eval.Call1(Ruby.Runtime.Init.rb_cFile, "blockdev?", p1);
        }

         [InteropMethod("chardev?")]
        public static object is_chardev(object p1)
        {
            return Eval.Call1(Ruby.Runtime.Init.rb_cFile, "chardev?", p1);
        }

         [InteropMethod("delete")]
        public static object delete(params object[] args)
        {
            return Eval.Call(Ruby.Runtime.Init.rb_cFile, "delete", args);
        }

         [InteropMethod("directory?")]
        public static object is_directory(object p1)
        {
            return Eval.Call1(Ruby.Runtime.Init.rb_cFile, "directory?", p1);
        }

         [InteropMethod("dirname")]
        public static object dirname(object p1)
        {
            return Eval.Call1(Ruby.Runtime.Init.rb_cFile, "dirname", p1);
        }

         [InteropMethod("executable?")]
        public static object is_executable(object p1)
        {
            return Eval.Call1(Ruby.Runtime.Init.rb_cFile, "executable?", p1);
        }

         [InteropMethod("executable_real?")]
        public static object is_executable_real(object p1)
        {
            return Eval.Call1(Ruby.Runtime.Init.rb_cFile, "executable_real?", p1);
        }

         [InteropMethod("exist?")]
        public static object is_exist(object p1)
        {
            return Eval.Call1(Ruby.Runtime.Init.rb_cFile, "exist?", p1);
        }

         [InteropMethod("exists?")]
        public static object is_exists(object p1)
        {
            return Eval.Call1(Ruby.Runtime.Init.rb_cFile, "exists?", p1);
        }

         [InteropMethod("expand_path")]
        public static object expand_path(params object[] args)
        {
            return Eval.Call(Ruby.Runtime.Init.rb_cFile, "expand_path", args);
        }

         [InteropMethod("extname")]
        public static object extname(object p1)
        {
            return Eval.Call1(Ruby.Runtime.Init.rb_cFile, "extname", p1);
        }

         [InteropMethod("file?")]
        public static object is_file(object p1)
        {
            return Eval.Call1(Ruby.Runtime.Init.rb_cFile, "file?", p1);
        }

         [InteropMethod("fnmatch")]
        public static object fnmatch(params object[] args)
        {
            return Eval.Call(Ruby.Runtime.Init.rb_cFile, "fnmatch", args);
        }

         [InteropMethod("fnmatch?")]
        public static object is_fnmatch(params object[] args)
        {
            return Eval.Call(Ruby.Runtime.Init.rb_cFile, "fnmatch?", args);
        }

         [InteropMethod("ftype")]
        public static object ftype(object p1)
        {
            return Eval.Call1(Ruby.Runtime.Init.rb_cFile, "ftype", p1);
        }

         [InteropMethod("grpowned?")]
        public static object is_grpowned(object p1)
        {
            return Eval.Call1(Ruby.Runtime.Init.rb_cFile, "grpowned?", p1);
        }

         [InteropMethod("identical?")]
        public static object is_identical(object p1, object p2)
        {
            return Eval.Call2(Ruby.Runtime.Init.rb_cFile, "identical?", p1, p2);
        }

         [InteropMethod("join")]
        public static object join(params object[] args)
        {
            return Eval.Call(Ruby.Runtime.Init.rb_cFile, "join", args);
        }

         [InteropMethod("lchmod")]
        public static object lchmod(params object[] args)
        {
            return Eval.Call(Ruby.Runtime.Init.rb_cFile, "lchmod", args);
        }

         [InteropMethod("lchown")]
        public static object lchown(params object[] args)
        {
            return Eval.Call(Ruby.Runtime.Init.rb_cFile, "lchown", args);
        }

         [InteropMethod("link")]
        public static object link(object p1, object p2)
        {
            return Eval.Call2(Ruby.Runtime.Init.rb_cFile, "link", p1, p2);
        }

         [InteropMethod("owned?")]
        public static object is_owned(object p1)
        {
            return Eval.Call1(Ruby.Runtime.Init.rb_cFile, "owned?", p1);
        }

         [InteropMethod("pipe?")]
        public static object is_pipe(object p1)
        {
            return Eval.Call1(Ruby.Runtime.Init.rb_cFile, "pipe?", p1);
        }

         [InteropMethod("readable?")]
        public static object is_readable(object p1)
        {
            return Eval.Call1(Ruby.Runtime.Init.rb_cFile, "readable?", p1);
        }

         [InteropMethod("readable_real?")]
        public static object is_readable_real(object p1)
        {
            return Eval.Call1(Ruby.Runtime.Init.rb_cFile, "readable_real?", p1);
        }

         [InteropMethod("readlink")]
        public static object readlink(object p1)
        {
            return Eval.Call1(Ruby.Runtime.Init.rb_cFile, "readlink", p1);
        }

         [InteropMethod("rename")]
        public static object rename(object p1, object p2)
        {
            return Eval.Call2(Ruby.Runtime.Init.rb_cFile, "rename", p1, p2);
        }

         [InteropMethod("setgid?")]
        public static object is_setgid(object p1)
        {
            return Eval.Call1(Ruby.Runtime.Init.rb_cFile, "setgid?", p1);
        }

         [InteropMethod("setuid?")]
        public static object is_setuid(object p1)
        {
            return Eval.Call1(Ruby.Runtime.Init.rb_cFile, "setuid?", p1);
        }

         [InteropMethod("size")]
        public static object size(object p1)
        {
            return Eval.Call1(Ruby.Runtime.Init.rb_cFile, "size", p1);
        }

         [InteropMethod("size?")]
        public static object is_size(object p1)
        {
            return Eval.Call1(Ruby.Runtime.Init.rb_cFile, "size?", p1);
        }

         [InteropMethod("socket?")]
        public static object is_socket(object p1)
        {
            return Eval.Call1(Ruby.Runtime.Init.rb_cFile, "socket?", p1);
        }

         [InteropMethod("split")]
        public static object split(object p1)
        {
            return Eval.Call1(Ruby.Runtime.Init.rb_cFile, "split", p1);
        }

         [InteropMethod("sticky?")]
        public static object is_sticky(object p1)
        {
            return Eval.Call1(Ruby.Runtime.Init.rb_cFile, "sticky?", p1);
        }

         [InteropMethod("symlink")]
        public static object symlink(object p1, object p2)
        {
            return Eval.Call2(Ruby.Runtime.Init.rb_cFile, "symlink", p1, p2);
        }

         [InteropMethod("symlink?")]
        public static object is_symlink(object p1)
        {
            return Eval.Call1(Ruby.Runtime.Init.rb_cFile, "symlink?", p1);
        }

         [InteropMethod("umask")]
        public static object umask(params object[] args)
        {
            return Eval.Call(Ruby.Runtime.Init.rb_cFile, "umask", args);
        }

         [InteropMethod("unlink")]
        public static object unlink(params object[] args)
        {
            return Eval.Call(Ruby.Runtime.Init.rb_cFile, "unlink", args);
        }

         [InteropMethod("utime")]
        public static object utime(params object[] args)
        {
            return Eval.Call(Ruby.Runtime.Init.rb_cFile, "utime", args);
        }

         [InteropMethod("writable?")]
        public static object is_writable(object p1)
        {
            return Eval.Call1(Ruby.Runtime.Init.rb_cFile, "writable?", p1);
        }

         [InteropMethod("writable_real?")]
        public static object is_writable_real(object p1)
        {
            return Eval.Call1(Ruby.Runtime.Init.rb_cFile, "writable_real?", p1);
        }

         [InteropMethod("zero?")]
        public static object is_zero(object p1)
        {
            return Eval.Call1(Ruby.Runtime.Init.rb_cFile, "zero?", p1);
        }

         [InteropMethod("atime")]
        public object atime()
        {
            return Eval.Call0(this, "atime");
        }

         [InteropMethod("chmod")]
        public object chmod(object p1)
        {
            return Eval.Call1(this, "chmod", p1);
        }

         [InteropMethod("chown")]
        public object chown(object p1, object p2)
        {
            return Eval.Call2(this, "chown", p1, p2);
        }

         [InteropMethod("ctime")]
        public object ctime()
        {
            return Eval.Call0(this, "ctime");
        }

         [InteropMethod("flock")]
        public object flock(object p1)
        {
            return Eval.Call1(this, "flock", p1);
        }

         [InteropMethod("lstat")]
        public object lstat()
        {
            return Eval.Call0(this, "lstat");
        }

         [InteropMethod("mtime")]
        public object mtime()
        {
            return Eval.Call0(this, "mtime");
        }

         [InteropMethod("path")]
        public object path()
        {
            return Eval.Call0(this, "path");
        }

         [InteropMethod("truncate")]
        public object truncate(object p1)
        {
            return Eval.Call1(this, "truncate", p1);
        }

    }
}

