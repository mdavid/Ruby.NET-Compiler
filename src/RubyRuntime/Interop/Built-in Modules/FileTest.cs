using Ruby.Runtime;
using Ruby.Interop;

namespace Ruby
{
    public partial class FileTest
    {
         [InteropMethod("blockdev?")]
        public static object is_blockdev(object p1)
        {
            return Eval.Call1(Ruby.Runtime.Init.rb_mFileTest, "blockdev?", p1);
        }

         [InteropMethod("chardev?")]
        public static object is_chardev(object p1)
        {
            return Eval.Call1(Ruby.Runtime.Init.rb_mFileTest, "chardev?", p1);
        }

         [InteropMethod("directory?")]
        public static object is_directory(object p1)
        {
            return Eval.Call1(Ruby.Runtime.Init.rb_mFileTest, "directory?", p1);
        }

         [InteropMethod("executable?")]
        public static object is_executable(object p1)
        {
            return Eval.Call1(Ruby.Runtime.Init.rb_mFileTest, "executable?", p1);
        }

         [InteropMethod("executable_real?")]
        public static object is_executable_real(object p1)
        {
            return Eval.Call1(Ruby.Runtime.Init.rb_mFileTest, "executable_real?", p1);
        }

         [InteropMethod("exist?")]
        public static object is_exist(object p1)
        {
            return Eval.Call1(Ruby.Runtime.Init.rb_mFileTest, "exist?", p1);
        }

         [InteropMethod("exists?")]
        public static object is_exists(object p1)
        {
            return Eval.Call1(Ruby.Runtime.Init.rb_mFileTest, "exists?", p1);
        }

         [InteropMethod("file?")]
        public static object is_file(object p1)
        {
            return Eval.Call1(Ruby.Runtime.Init.rb_mFileTest, "file?", p1);
        }

         [InteropMethod("grpowned?")]
        public static object is_grpowned(object p1)
        {
            return Eval.Call1(Ruby.Runtime.Init.rb_mFileTest, "grpowned?", p1);
        }

         [InteropMethod("identical?")]
        public static object is_identical(object p1, object p2)
        {
            return Eval.Call2(Ruby.Runtime.Init.rb_mFileTest, "identical?", p1, p2);
        }

         [InteropMethod("owned?")]
        public static object is_owned(object p1)
        {
            return Eval.Call1(Ruby.Runtime.Init.rb_mFileTest, "owned?", p1);
        }

         [InteropMethod("pipe?")]
        public static object is_pipe(object p1)
        {
            return Eval.Call1(Ruby.Runtime.Init.rb_mFileTest, "pipe?", p1);
        }

         [InteropMethod("readable?")]
        public static object is_readable(object p1)
        {
            return Eval.Call1(Ruby.Runtime.Init.rb_mFileTest, "readable?", p1);
        }

         [InteropMethod("readable_real?")]
        public static object is_readable_real(object p1)
        {
            return Eval.Call1(Ruby.Runtime.Init.rb_mFileTest, "readable_real?", p1);
        }

         [InteropMethod("setgid?")]
        public static object is_setgid(object p1)
        {
            return Eval.Call1(Ruby.Runtime.Init.rb_mFileTest, "setgid?", p1);
        }

         [InteropMethod("setuid?")]
        public static object is_setuid(object p1)
        {
            return Eval.Call1(Ruby.Runtime.Init.rb_mFileTest, "setuid?", p1);
        }

         [InteropMethod("size")]
        public static object size(object p1)
        {
            return Eval.Call1(Ruby.Runtime.Init.rb_mFileTest, "size", p1);
        }

         [InteropMethod("size?")]
        public static object is_size(object p1)
        {
            return Eval.Call1(Ruby.Runtime.Init.rb_mFileTest, "size?", p1);
        }

         [InteropMethod("socket?")]
        public static object is_socket(object p1)
        {
            return Eval.Call1(Ruby.Runtime.Init.rb_mFileTest, "socket?", p1);
        }

         [InteropMethod("sticky?")]
        public static object is_sticky(object p1)
        {
            return Eval.Call1(Ruby.Runtime.Init.rb_mFileTest, "sticky?", p1);
        }

         [InteropMethod("symlink?")]
        public static object is_symlink(object p1)
        {
            return Eval.Call1(Ruby.Runtime.Init.rb_mFileTest, "symlink?", p1);
        }

         [InteropMethod("writable?")]
        public static object is_writable(object p1)
        {
            return Eval.Call1(Ruby.Runtime.Init.rb_mFileTest, "writable?", p1);
        }

         [InteropMethod("writable_real?")]
        public static object is_writable_real(object p1)
        {
            return Eval.Call1(Ruby.Runtime.Init.rb_mFileTest, "writable_real?", p1);
        }

         [InteropMethod("zero?")]
        public static object is_zero(object p1)
        {
            return Eval.Call1(Ruby.Runtime.Init.rb_mFileTest, "zero?", p1);
        }

    }
}

