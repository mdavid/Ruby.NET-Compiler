/**********************************************************************

  Ruby.NET Runtime Library
  Originally developed at Queensland University of Technology
 
  Some sections of this C# code mirror the structure of the C code in the
  Ruby 1.8.2 Interpreter Copyright (C) 1993-2003 Yukihiro Matsumoto, et.al.
  
**********************************************************************/

using Ruby.Runtime;
using Ruby;
using System.Globalization;

namespace Ruby.Methods
{
    
    internal class rb_file_initialize : VarArgMethodBody0  // author: cjs, status: done
    {
        internal static rb_file_initialize singleton = new rb_file_initialize();

        public override object Call(Class last_class, object recv, Frame caller, Proc block, Array rest)
        {
            if (((IO)recv).f != null)
                throw new RuntimeError("reinitializing File").raise(caller);

            int fd;
            if (0 < rest.Count && rest.Count < 3)
            {
                fd = Object.CheckConvert<int>(rest[0], "to_int", caller);
                if (fd > 0)
                    return rb_io_initialize.singleton.Call(last_class, recv, caller, block, fd, rest);
            }

            return IO.rb_open_file((IO)recv, caller, rest);
        }
    }

    
    internal class rb_file_s_stat : MethodBody1 // author: cjs, status: done
    {
        internal static rb_file_s_stat singleton = new rb_file_s_stat();

        public override object Call1(Class last_class, object recv, Frame caller, Proc block, object p1)
        {
            string path = String.StringValue(p1, caller);

            Eval.rb_secure(2, caller);
            return new FileStat(path);
        }
    }

    
    internal class rb_file_s_lstat : MethodBody1 // author: cjs, status: done
    {
        internal static rb_file_s_lstat singleton = new rb_file_s_lstat();

        public override object Call1(Class last_class, object recv, Frame caller, Proc block, object p1)
        {
            return rb_file_s_stat.singleton.Call1(last_class, recv, caller, null, p1);
        }
    }

    
    internal class rb_file_s_ftype : MethodBody1  // author: cjs, status: done
    {
        internal static rb_file_s_ftype singleton = new rb_file_s_ftype();

        public override object Call1(Class last_class, object recv, Frame caller, Proc block, object p1)
        {
            string path = String.StringValue(p1, caller);

            FileStat st = new FileStat(path);

            return File.rb_file_ftype(st.stat);
        }
    }

    
    internal class rb_file_s_atime : MethodBody1  // author: cjs, status: done
    {
        internal static rb_file_s_atime singleton = new rb_file_s_atime();

        public override object Call1(Class last_class, object recv, Frame caller, Proc block, object p1)
        {
            string path = String.StringValue(p1, caller);

            FileStat st = new FileStat(path);

            return new Time(st.stat.st_atime);
        }
    }

    
    internal class rb_file_s_mtime : MethodBody1  // author: cjs, status: done
    {
        internal static rb_file_s_mtime singleton = new rb_file_s_mtime();

        public override object Call1(Class last_class, object recv, Frame caller, Proc block, object p1)
        {
            string path = String.StringValue(p1, caller);

            FileStat st = new FileStat(path);

            return new Time(st.stat.st_mtime);
        }
    }

    
    internal class rb_file_s_ctime : MethodBody1 // author: cjs, status: done
    {
        internal static rb_file_s_ctime singleton = new rb_file_s_ctime();

        public override object Call1(Class last_class, object recv, Frame caller, Proc block, object p1)
        {
            string path = String.StringValue(p1, caller);

            FileStat st = new FileStat(path);

            return new Time(st.stat.st_ctime);
        }
    }

    
    internal class rb_file_s_utime : VarArgMethodBody0  // author: cjs, status: done
    {
        internal static rb_file_s_utime singleton = new rb_file_s_utime();

        public override object Call(Class last_class, object recv, Frame caller, Proc block, Array rest)
        {
            Class.rb_scan_args(caller, rest, 2, 0, true);

            Time atime = (Time)rest[0];
            Time mtime = (Time)rest[1];

            for (int i = 2; i < rest.Count; i++)
                if (rest[i] != null)
                {
                    string path = String.StringValue(rest[i], caller);
                    System.IO.File.SetLastAccessTime(path, atime.value);
                    System.IO.File.SetLastWriteTime(path, mtime.value);
                }

            return rest.Count - 2;
        }
    }

 
    internal class rb_file_s_chmod : VarArgMethodBody1 // author: cjs/war, status: done
    {
        internal static rb_file_s_chmod singleton = new rb_file_s_chmod();

        public override object Call(Class last_class, object recv, Frame caller, Proc block, object p1, Array rest)
        {
            //wartag: The owner field is the only relevant field on windows, and within that field the write 
            //permission is the only relevant option. The file is read-only if the write permission is not set;
            //file is always readable.  

            Eval.rb_secure(2, caller);
            Class.rb_scan_args(caller, rest, 0, 0, true);
            
            int mode = Numeric.rb_num2long(p1, caller);

            foreach (object o in rest)
            {
                string filename = String.StringValue(o, caller);
                Eval.rb_check_safe_obj(caller, o);
                File.chmod_internal(filename, mode, caller);
            }

            return rest.Count;
        }
    }

    
    internal class rb_file_s_chown : VarArgMethodBody2 // author: cjs, status: done
    {
        internal static rb_file_s_chown singleton = new rb_file_s_chown();

        public override object Call(Class last_class, object recv, Frame caller, Proc block, object p1, object p2, Array rest)
        {
            Eval.rb_secure(2, caller);

            Class.rb_scan_args(caller, rest, 0, 0, true);

            //chown does nothing on win32

            return rest.Count;
        }
    }

    
    internal class rb_file_s_lchmod : VarArgMethodBody1 // author: cjs, status: done
    {
        internal static rb_file_s_lchmod singleton = new rb_file_s_lchmod();

        public override object Call(Class last_class, object recv, Frame caller, Proc block, object p1, Array rest)
        {
            throw NotImplementedError.rb_notimplement(caller, "lchmod").raise(caller);
        }
    }

    
    internal class rb_file_s_lchown : VarArgMethodBody1  // author: cjs, status: done
    {
        internal static rb_file_s_lchown singleton = new rb_file_s_lchown();

        public override object Call(Class last_class, object recv, Frame caller, Proc block, object p1, Array rest)
        {
            throw NotImplementedError.rb_notimplement(caller, "lchown").raise(caller);
        }
    }


    
    internal class rb_file_s_link : MethodBody2  // author: cjs, status: done
    {
        internal static rb_file_s_link singleton = new rb_file_s_link();

        public override object Call2(Class last_class, object recv, Frame caller, Proc block, object p1, object p2)
        {
            return 0; // no symbolic links in win32
        }
    }


    
    internal class rb_file_s_symlink : MethodBody2  // author: cjs, status: done
    {
        internal static rb_file_s_symlink singleton = new rb_file_s_symlink();

        public override object Call2(Class last_class, object recv, Frame caller, Proc block, object p1, object p2)
        {
            throw NotImplementedError.rb_notimplement(caller, "symlink").raise(caller);
        }
    }

    
    internal class rb_file_s_readlink : MethodBody1 // author: cjs, status: done
    {
        internal static rb_file_s_readlink singleton = new rb_file_s_readlink();

        public override object Call1(Class last_class, object recv, Frame caller, Proc block, object p1)
        {
            throw NotImplementedError.rb_notimplement(caller, "readlink").raise(caller);
        }
    }

    
    internal class rb_file_s_unlink : VarArgMethodBody0 //status: done
    {
        internal static rb_file_s_unlink singleton = new rb_file_s_unlink();

        public override object Call(Class last_class, object recv, Frame caller, Proc block, Array files)
        {
            Eval.rb_secure(2, caller);

            foreach (String file in files)
                System.IO.File.Delete(file.value);
            return files.Count;
        }
    }

    
    internal class rb_file_s_rename : MethodBody2 // author: cjs, status: done
    {
        internal static rb_file_s_rename singleton = new rb_file_s_rename();

        public override object Call2(Class last_class, object recv, Frame caller, Proc block, object p1, object p2)
        {
            System.IO.FileInfo f = new System.IO.FileInfo(String.StringValue(p1, caller));
            f.MoveTo(String.StringValue(p2, caller));

            return 0;
        }
    }

    
    internal class rb_file_s_umask : VarArgMethodBody0 // author: cjs, status: done, comment: umask
    {
        internal static rb_file_s_umask singleton = new rb_file_s_umask();

        public override object Call(Class last_class, object recv, Frame caller, Proc block, Array rest)
        {
            Eval.rb_secure(2, caller);

            if (rest.Count > 1)
            {
                throw new ArgumentError("wrong number of arguments").raise(caller);
            }            

            //int omask = 888 - 644;
            //return omask;

            //wartag: tested this function on winxp and it always returns 0. 
            return 0;
        }
    }

    
    internal class rb_file_s_truncate : MethodBody2 // author: cjs, status: done
    {
        internal static rb_file_s_truncate singleton = new rb_file_s_truncate();

        public override object Call2(Class last_class, object recv, Frame caller, Proc block, object p1, object p2)
        {
            Eval.rb_secure(2, caller);
            throw NotImplementedError.rb_notimplement(caller, "truncate").raise(caller);
        }
    }

    
    internal class rb_file_s_expand_path : VarArgMethodBody0 // author: cjs, status: done
    {
        internal static rb_file_s_expand_path singleton = new rb_file_s_expand_path();

        public override object Call(Class last_class, object recv, Frame caller, Proc block, Array rest)
        {
            if (rest.Count == 1)
                return File.rb_file_expand_path(caller, rest[0], null);

            Class.rb_scan_args(caller, rest, 1, 1, false);

            return File.rb_file_expand_path(caller, rest[0], rest[1]);
        }
    }

    
    internal class rb_file_s_basename : VarArgMethodBody1 // author: cjs, status: done
    {
        internal static rb_file_s_basename singleton = new rb_file_s_basename();

        public override object Call(Class last_class, object recv, Frame caller, Proc block, object p1, Array rest)
        {
            String fname;
            string fext;
            if (Class.rb_scan_args(caller, rest, 0, 1, false) == 1)
                fext = String.StringValue(rest[0], caller);
            else
                fext = null;
            fname = String.RStringValue(p1, caller);

            string name;
            if ((name = fname.value) == null || fname.value.Length == 0)
                return fname;

            string p;
            int f;

            if ((name = File.skiproot(name)) == null || name.Length == 0)
            {
                if (fname.value.EndsWith(":"))
                {
                    p = "";
                    f = 0;
                }
                else
                {
                    p = fname.value.Substring(fname.value.Length - 1);
                    f = 1;
                }
            }
            else if ((p = File.strrdirsep(name)) == null || p.Length == 0)
            {
                if (fext == null || (f = File.rmext(name, fext)) == 0)
                {
                    f = File.chompdirsep(name);
                    if (f == fname.value.Length)
                        return fname;
                }
                p = name;
            }
            else
            {
                while (File.isdirsep(p))
                    p = p.Substring(1);
                if (fext == null || (f = File.rmext(p, fext)) == 0)
                    f = File.chompdirsep(p);
            }

            String basename = new String(p.Substring(0, f));
            basename.Tainted = fname.Tainted;
            return basename;
        }
    }

    
    internal class rb_file_s_dirname : MethodBody1  //author: cjs, status: done
    {
        internal static rb_file_s_dirname singleton = new rb_file_s_dirname();

        public override object Call1(Class last_class, object recv, Frame caller, Proc block, object p1)
        {
            String fname = String.RStringValue(p1, caller);
            string name = fname.value;
            string root = File.skiproot(name);

            if (name.IndexOf(root) > 1 && File.isdirsep(name))
                name = name.Substring(name.IndexOf(root) - 2);

            string p = File.rb_path_last_separator(root);
            if (p == null || p.Length == 0)
                p = root;

            if (p == name)
                return new String(".");

            String dirname = new String(name.Substring(0, name.Length - p.Length));

            if (name.Length > 1 && root.Equals(name.Substring(2)) && name.Substring(1, 1) == ":")
                dirname.value += ".";

            dirname.Tainted = fname.Tainted;
            return dirname;
        }
    }

    
    internal class rb_file_s_extname : MethodBody1 // author: cjs, status: done
    {
        internal static rb_file_s_extname singleton = new rb_file_s_extname();

        public override object Call1(Class last_class, object recv, Frame caller, Proc block, object p1)
        {
            String fname = String.RStringValue(p1, caller);
            string name = fname.value;
            string p = File.rb_path_last_separator(name); /* get the last path component */

            if (p == null || p.Length == 0)
                p = name;
            else
                p = p.Substring(1);

            int e = p.LastIndexOf('.'); /* get the last dot of the last component */
            if (e <= 0) /* no dot, or the only dot is first? */
                return new String("");

            String extname = new String(p.Substring(e));
            extname.Tainted = fname.Tainted;

            return extname;
        }
    }

    
    internal class rb_file_s_split : MethodBody1 // author: cjs, status: done
    {
        internal static rb_file_s_split singleton = new rb_file_s_split();

        public override object Call1(Class last_class, object recv, Frame caller, Proc block, object p1)
        {
            return new Array(new object[] { rb_file_s_dirname.singleton.Call1(last_class, recv, caller, null, p1), rb_file_s_basename.singleton.Call(last_class, recv, caller, null, p1, new Array()) });
        }
    }

    
    internal class rb_file_s_join : VarArgMethodBody0 // author: cjs, status: done
    {
        internal static rb_file_s_join singleton = new rb_file_s_join();

        public override object Call(Class last_class, object recv, Frame caller, Proc block, Array rest)
        {
            String sep = String.RStringValue(File.separator, caller);

            bool taint = (rest.Tainted || sep.Tainted);

            string[] names = new string[rest.Count];
            for (int i = 0; i < rest.Count; i++)
                if (rest[i] is String)
                    names[i] = ((String)rest[i]).value;
                else if (rest[i] is Array)
                {
                    Array ary = (Array)rest[i];
                    if (Array.IsInspecting(ary))
                        names[i] = "[...]";
                    else
                    {
                        try
                        {
                            Array.StartInspect(ary);
                            names[i] = ((String)rb_file_s_join.singleton.Call(last_class, recv, caller, block, new Array(new object[] { ary, sep }))).value;
                        }
                        finally
                        {
                            Array.EndInspect(ary);
                        }
                    }
                }
                else
                    names[i] = String.ObjectAsString(rest[i], caller).value;

            String result = new String(string.Join(sep.value, names));
            if (taint)
                result.Tainted = true;
            return result;
        }
    }

    
    internal class rb_file_lstat : MethodBody0 // author: cjs, status: done
    {
        internal static rb_file_lstat singleton = new rb_file_lstat();

        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {
            return rb_io_stat.singleton.Call0(last_class, recv, caller, null);
        }
    }

    
    internal class rb_file_atime : MethodBody0 // author: cjs, status: done
    {
        internal static rb_file_atime singleton = new rb_file_atime();

        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {
            string path = ((File)recv)._path;

            if (!System.IO.File.Exists(path))
                throw SystemCallError.rb_sys_fail(path, new System.IO.FileNotFoundException(null, path), caller).raise(caller);

            return new Time(System.IO.File.GetLastAccessTime(path));
        }
    }

    
    internal class rb_file_mtime : MethodBody0 // author: cjs, status: done
    {
        internal static rb_file_mtime singleton = new rb_file_mtime();

        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {
            string path = ((File)recv)._path;

            if (!System.IO.File.Exists(path))
                throw SystemCallError.rb_sys_fail(path, new System.IO.FileNotFoundException(null, path), caller).raise(caller);

            return new Time(System.IO.File.GetLastWriteTime(path));
        }
    }

    
    internal class rb_file_ctime : MethodBody0 // author: cjs, status: done
    {
        internal static rb_file_ctime singleton = new rb_file_ctime();

        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {
            string path = ((File)recv)._path;

            if (!System.IO.File.Exists(path))
                throw SystemCallError.rb_sys_fail(path, new System.IO.FileNotFoundException(null, path), caller).raise(caller);

            return new Time(System.IO.File.GetCreationTime(path));
        }
    }

    
    internal class rb_file_chmod : MethodBody1 // author: cjs/war, status: done, comment: chmod
    {
        internal static rb_file_chmod singleton = new rb_file_chmod();

        public override object Call1(Class last_class, object recv, Frame caller, Proc block, object p1)
        {
            Eval.rb_secure(2, caller);

            int mode = Numeric.rb_num2long(p1, caller);
            IO fptr = IO.GetOpenFile(caller, recv);

            if (fptr._path == null)
                return null;
            //if (chmod(fptr->path, mode) == -1)
            //    throw SystemCallError.rb_sys_fail(fptr.path, new System.IO.IOException(), caller).raise(caller);

            return 0;
        }
    }

    
    internal class rb_file_chown : MethodBody2 // author: cjs/war, status: done, comment: chown
    {
        internal static rb_file_chown singleton = new rb_file_chown();

        public override object Call2(Class last_class, object recv, Frame caller, Proc block, object p1, object p2)
        {
            Eval.rb_secure(2, caller);

            int o = Numeric.rb_num2long(p1, caller);
            int g = Numeric.rb_num2long(p2, caller);
            IO fptr = IO.GetOpenFile(caller, recv);

            if (fptr == null)
            {
                return null; 
            }
            //wartag: chown always returns 0, see /win32/win32.c

            return 0;
        }
    }

    
    internal class rb_file_truncate : MethodBody1 // author: cjs, status: done
    {
        internal static rb_file_truncate singleton = new rb_file_truncate();

        public override object Call1(Class last_class, object recv, Frame caller, Proc block, object p1)
        {
            Eval.rb_secure(2, caller);
            int pos = Numeric.rb_num2long(p1, caller);

            IO fptr = IO.GetOpenFile(caller, recv);
            if (!((fptr.mode & IO.FMODE_WRITABLE) > 0))
                throw new IOError("not opened for writing").raise(caller);

            System.IO.Stream f = IO.GetWriteFile(fptr);
            f.Flush();
            f.Seek(0, System.IO.SeekOrigin.Begin);

            throw NotImplementedError.rb_notimplement(caller, "truncate").raise(caller);
        }
    }

    
    internal class rb_file_flock : MethodBody1 // author: cjs, status: partial, comment: Ruby has 4 lock types, .NET only 2
    {
        internal static rb_file_flock singleton = new rb_file_flock();

        public override object Call1(Class last_class, object recv, Frame caller, Proc block, object p1)
        {
            IO fptr;
            int op;

            Eval.rb_secure(2, caller);
            op = Numeric.rb_num2long(p1, caller);
            fptr = File.GetOpenFile(caller, recv);

            if ((fptr.mode & File.FMODE_WRITABLE) > 0)
            {
                fptr.f.Flush();
            }

            try
            {
                switch (op)
                {
                    case File.LOCK_EX:
                    case File.LOCK_NB:
                    case File.LOCK_SH:
                        ((System.IO.FileStream)fptr.f).Lock(0, fptr.f.Length - 1);
                        break;
                    case File.LOCK_UN:
                        ((System.IO.FileStream)fptr.f).Unlock(0, fptr.f.Length - 1);
                        break;
                }
            }
            catch (System.IO.IOException ioe)
            {
                throw SystemCallError.rb_sys_fail(fptr._path, ioe, caller).raise(caller);
            }

            return 0;
        }
    }

    
    internal class rb_file_s_size : MethodBody1 // author: cjs, status: done
    {
        internal static rb_file_s_size singleton = new rb_file_s_size();

        public override object Call1(Class last_class, object recv, Frame caller, Proc block, object p1)
        {
            string path = String.StringValue(p1, caller);

            if (!System.IO.File.Exists(path))
                throw SystemCallError.rb_sys_fail(path, new System.IO.FileNotFoundException(null, path), caller).raise(caller);

            return new System.IO.FileInfo(path).Length;
        }
    }

    
    internal class rb_file_path : MethodBody0 // author: cjs, status: done
    {
        internal static rb_file_path singleton = new rb_file_path();

        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {
            File file = (File)recv;

            File.rb_io_check_initialized(caller, file);

            if (file._path == null)
                return null;
            else
            {
                String result = new String(file._path);
                result.Tainted = true;
                return result;
            }
        }
    }
    
    internal class rb_f_test : VarArgMethodBody0 // author: cjs, status: done
    {
        internal static rb_f_test singleton = new rb_f_test();

        public override object Call(Class last_class, object recv, Frame caller, Proc block, Array rest)
        {
            if (rest.Count == 0)
                throw new ArgumentError("wrong number of arguments").raise(caller);

            char cmd = (char)Integer.rb_num2long(rest[0], caller);

            if (cmd == 0)
                return false;

            if (File.strchr("bcdefgGkloOprRsSuwWxXz", cmd))
            {
                File.test_check(1, rest, caller);

                switch (cmd)
                {
                    case 'b':
                        return test_b.singleton.Call1(last_class, null, caller, null, rest[1]);
                    case 'c':
                        return test_c.singleton.Call1(last_class, null, caller, null, rest[1]);
                    case 'd':
                        return test_d.singleton.Call1(last_class, null, caller, null, rest[1]);
                    case 'a':
                    case 'e':
                        return test_e.singleton.Call1(last_class, null, caller, null, rest[1]);
                    case 'f':
                        return test_f.singleton.Call1(last_class, null, caller, null, rest[1]);
                    case 'g':
                        return test_sgid.singleton.Call1(last_class, null, caller, null, rest[1]);
                    case 'G':
                        return test_grpowned.singleton.Call1(last_class, null, caller, null, rest[1]);
                    case 'k':
                        return test_sticky.singleton.Call1(last_class, null, caller, null, rest[1]);
                    case 'l':
                        return test_l.singleton.Call1(last_class, null, caller, null, rest[1]);
                    case 'o':
                        return test_owned.singleton.Call1(last_class, null, caller, null, rest[1]);
                    case 'O':
                        return test_owned.singleton.Call1(last_class, null, caller, null, rest[1]);
                        //return test_rowned.singleton.Call1(last_class, null, caller, null, rest[1]);
                    case 'p':
                        return test_p.singleton.Call1(last_class, null, caller, null, rest[1]);
                    case 'r':
                        return test_r.singleton.Call1(last_class, null, caller, null, rest[1]);
                    case 'R':
                        return test_R.singleton.Call1(last_class, null, caller, null, rest[1]);
                    case 's':
                        return test_s.singleton.Call1(last_class, null, caller, null, rest[1]);
                    case 'S':
                        return test_S.singleton.Call1(last_class, null, caller, null, rest[1]);
                    case 'u':
                        return test_suid.singleton.Call1(last_class, null, caller, null, rest[1]);
                    case 'w':
                        return test_w.singleton.Call1(last_class, null, caller, null, rest[1]);
                    case 'W':
                        return test_W.singleton.Call1(last_class, null, caller, null, rest[1]);
                    case 'x':
                        return test_x.singleton.Call1(last_class, null, caller, null, rest[1]);
                    case 'X':
                        return test_X.singleton.Call1(last_class, null, caller, null, rest[1]);
                    case 'z':
                        return test_z.singleton.Call1(last_class, null, caller, null, rest[1]);
                }
            }

            if (File.strchr("MAC", cmd))
            {
                File.test_check(1, rest, caller);

                FileStat st = new FileStat(String.StringValue(rest[1], caller));

                switch (cmd)
                {
                    case 'A':
                        return new Time(st.stat.st_atime);
                    case 'M':
                        return new Time(st.stat.st_mtime);
                    case 'C':
                        return new Time(st.stat.st_ctime);
                }
            }

            if (File.strchr("-=<>", cmd))
            {
                File.test_check(2, rest, caller);

                FileStat st1 = new FileStat(String.StringValue(rest[1], caller));
                FileStat st2 = new FileStat(String.StringValue(rest[1], caller));

                switch (cmd)
                {
                    case '-':
                        return (st1.stat.st_dev == st2.stat.st_dev && st1.stat.st_ino == st2.stat.st_ino);
                    case '=':
                        return (st1.stat.st_mtime == st2.stat.st_mtime);
                    case '>':
                        return (st1.stat.st_mtime > st2.stat.st_mtime);
                    case '<':
                        return (st1.stat.st_mtime < st2.stat.st_mtime);
                }
            }

            throw new ArgumentError(string.Format(CultureInfo.InvariantCulture, "unknown command ?{0}", cmd)).raise(caller);
        }
    }

    
    internal class file_s_fnmatch : MethodBody // author: cjs, status: done
    {
        internal static file_s_fnmatch singleton = new file_s_fnmatch();

        public override object Calln(Class last_class, object recv, Frame caller, ArgList args)
        {
            int flags;
            if (Class.rb_scan_args(caller, args.ToRubyArray(), 2, 1, false) == 3)
                flags = Numeric.rb_num2long(args[2], caller);
            else
                flags = 0;

            string pattern = String.StringValue(args[0], caller);
            string path = String.StringValue(args[1], caller);

            return (File.fnmatch(pattern, path, flags) == 0);
        }
    }
}
