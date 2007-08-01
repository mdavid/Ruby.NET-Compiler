/**********************************************************************

  Ruby.NET Runtime Library
  Originally developed at Queensland University of Technology
 
  Some sections of this C# code mirror the structure of the C code in the
  Ruby 1.8.2 Interpreter Copyright (C) 1993-2003 Yukihiro Matsumoto, et.al.
  
**********************************************************************/

using Ruby;
using Ruby.Runtime;

namespace Ruby.Methods
{
    
    internal class dir_s_alloc : MethodBody0 // author: cjs, status: done
    {
        internal static dir_s_alloc singleton = new dir_s_alloc();

        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {
            return new Dir((Class)recv);
        }
    }

    
    internal class dir_s_open : MethodBody1 // author: cjs, status: done
    {
        internal static dir_s_open singleton = new dir_s_open();

        public override object Call1(Class last_class, object recv, Frame caller, Proc block, object p1)
        {
            string dirname = String.StringValue(p1, caller);

            Dir dir = new Dir((Class)recv);
            Dir.dir_initialize(caller, dir, dirname);

            if (block == null)
                return dir;
            else
            {
                object result = null;
                try
                {
                    result = Proc.rb_yield(block, caller, new object[] { dir });
                }
                finally
                {
                    Eval.CallPrivate(dir, caller, "close", null, new object[] { });
                }

                return result;
            }
        }
    }

    
    internal class dir_foreach : MethodBody1 // author: cjs, status: done
    {
        internal static dir_foreach singleton = new dir_foreach();

        public override object Call1(Class last_class, object recv, Frame caller, Proc block, object p1)
        {
            Dir dir = Dir.dir_open_dir(caller, p1);

            try
            {
                dir_each.singleton.Call0(last_class, dir, caller, block);
            }
            finally
            {
                dir_close.singleton.Call0(last_class, dir, caller, null);
            }

            return null;
        }
    }

    
    internal class dir_entries : MethodBody1 // author: cjs, status: done
    {
        internal static dir_entries singleton = new dir_entries();

        public override object Call1(Class last_class, object recv, Frame caller, Proc block, object p1)
        {
            Dir dir = Dir.dir_open_dir(caller, p1);

            object result = null;
            try
            {
                result = Array.rb_Array(dir, caller);
            }
            finally
            {
                dir_close.singleton.Call0(last_class, dir, caller, null);
            }

            return result;
        }
    }

    
    internal class dir_initialize : MethodBody1 // author: cjs, status: done
    {
        internal static dir_initialize singleton = new dir_initialize();

        public override object Call1(Class last_class, object recv, Frame caller, Proc block, object p1)
        {
            string path = String.StringValue(p1, caller);
            Dir.dir_initialize(caller, recv, path);

            return recv;
        }
    }

    
    internal class dir_path : MethodBody0 // author: cjs, status: done
    {
        internal static dir_path singleton = new dir_path();

        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {
            Dir dir = Dir.GetDIR(caller, recv);

            return new String(dir._path);
        }
    }

    
    internal class dir_read : MethodBody0 // author: cjs, status: done
    {
        internal static dir_read singleton = new dir_read();

        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {
            Dir dir = Dir.GetDIR(caller, recv);

            if (dir._pos < (dir._entries.Count - 1))
                return dir._entries[dir._pos++];
            else
                return null;
        }
    }

    
    internal class dir_each : MethodBody0 // author: cjs, status: done
    {
        internal static dir_each singleton = new dir_each();

        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {
            Dir dir = Dir.GetDIR(caller, recv);

            if (block != null)
                foreach (String dp_name in dir._entries)
                {
                    dp_name.Tainted = true;
                    Proc.rb_yield(block, caller, new object[] { dp_name });
                    if (dir._dir == null)
                        Dir.dir_closed(caller);
                }
            else
                throw new LocalJumpError("no block given").raise(caller);

            return dir;
        }
    }

    
    internal class dir_rewind : MethodBody0 // author: cjs, status: done
    {
        internal static dir_rewind singleton = new dir_rewind();

        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {
            Dir dir = Dir.GetDIR(caller, recv);

            dir._pos = 0;

            return null;
        }
    }

    
    internal class dir_seek : MethodBody1 // author: cjs, status: done
    {
        internal static dir_seek singleton = new dir_seek();

        public override object Call1(Class last_class, object recv, Frame caller, Proc block, object p1)
        {
            Dir dir = Dir.GetDIR(caller, recv);

            throw NotImplementedError.rb_notimplement(caller, "seek").raise(caller);
        }
    }

    
    internal class dir_tell : MethodBody0 // author: cjs, status: done
    {
        internal static dir_tell singleton = new dir_tell();

        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {
            throw NotImplementedError.rb_notimplement(caller, "tell").raise(caller);
        }
    }

    
    internal class dir_set_pos : MethodBody1 // author: cjs, status: done
    {
        internal static dir_set_pos singleton = new dir_set_pos();

        public override object Call1(Class last_class, object recv, Frame caller, Proc block, object p1)
        {
            Dir dir = Dir.GetDIR(caller, recv);

            throw NotImplementedError.rb_notimplement(caller, "pos=").raise(caller);
        }
    }

    
    internal class dir_close : MethodBody0 // author: cjs, status: done
    {
        internal static dir_close singleton = new dir_close();

        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {
            Dir dir = Dir.GetDIR(caller, recv);

            dir._dir = null;
            dir._path = null;
            dir._entries = null;
            dir._pos = 0;

            return null;
        }
    }

    
    internal class dir_s_chdir : VarArgMethodBody0 // author: cjs, status: done
    {
        internal static dir_s_chdir singleton = new dir_s_chdir();

        static void dir_chdir(Frame caller, object p)
        {
            string path = String.StringValue(p, caller);
            try
            {
                System.Environment.CurrentDirectory = path;
            }
            catch (System.Exception e)
            {
                throw SystemCallError.rb_sys_fail(path, e, caller).raise(caller);
            }
        }

        static int chdir_blocking = 0;

        class chdir_data
        {
            internal String old_path, new_path;
            internal bool done;
        }

        static object chdir_yield(Frame caller, Proc block, chdir_data args)
        {
            dir_chdir(caller, args.new_path);
            args.done = true;
            chdir_blocking++;
            return Proc.rb_yield(block, caller, new object[] { args.new_path });
        }

        static object chdir_restore(Frame caller, chdir_data args)
        {
            if (args.done)
            {
                chdir_blocking--;
                dir_chdir(caller, args.old_path);
            }
            return null;
        }

        public override object Call(Class last_class, object recv, Frame caller, Proc block, Array rest)
        {
            String path;

            Eval.rb_secure(2, caller);
            if (Class.rb_scan_args(caller, rest, 0, 1, false) == 1)
                path = String.RStringValue(rest[0], caller);
            else
            {
                string dist = System.Environment.GetEnvironmentVariable("HOME");
                if (dist == null || dist.Length == 0)
                    dist = System.Environment.GetEnvironmentVariable("LOGDIR");
                if (dist == null || dist.Length == 0)
                    throw new ArgumentError("HOME/LOGDIR not set").raise(caller);
                path = new String(dist);
            }

            if (chdir_blocking > 0)
            {
                if (block != null)
                    Errors.rb_warn("conflicting chdir during another chdir block");
            }

            if (block != null)
            {
                string cwd = System.Environment.CurrentDirectory;

                chdir_data args = new chdir_data();

                args.old_path = new String(cwd);
                args.old_path.Tainted = true;
                args.new_path = path;
                args.done = false;

                object result = null;
                try
                {
                    result = chdir_yield(caller, block, args);
                }
                finally
                {
                    chdir_restore(caller, args);
                }

                return result;
            }
            else
            {
                dir_chdir(caller, path);
                return 0;
            }
        }
    }

    
    internal class dir_s_getwd : MethodBody0 // author: cjs, status: done
    {
        internal static dir_s_getwd singleton = new dir_s_getwd();

        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {
            Eval.rb_secure(4, caller);
            String cwd = new String(System.Environment.CurrentDirectory);
            cwd.Tainted = true;
            return cwd;
        }
    }

    
    internal class dir_s_chroot : MethodBody1 // author: cjs, status: done
    {
        internal static dir_s_chroot singleton = new dir_s_chroot();

        public override object Call1(Class last_class, object recv, Frame caller, Proc block, object p1)
        {
            throw NotImplementedError.rb_notimplement(caller, "chroot").raise(caller);
        }
    }

    
    internal class dir_s_mkdir : VarArgMethodBody1 // author: cjs, status: done
    {
        internal static dir_s_mkdir singleton = new dir_s_mkdir();

        public override object Call(Class last_class, object recv, Frame caller, Proc block, object p1, Array rest)
        {
            if (rest.Count > 1)
                throw new ArgumentError(string.Format("wrong number of arguments ({0} for {1})", new object[] { rest.Count, 2 })).raise(caller);
            // rest[0] posix permissions, ignored here

            string path = String.StringValue(p1, caller);

            Eval.rb_secure(2, caller); //check_dirname
            try
            {
                System.IO.Directory.CreateDirectory(path);
            }
            catch (System.IO.IOException ioe)
            {
                throw SystemCallError.rb_sys_fail(path, ioe, caller).raise(caller);
            }

            return 0; //success
        }
    }

    
    internal class dir_s_rmdir : MethodBody1 // author: cjs, status: done
    {
        internal static dir_s_rmdir singleton = new dir_s_rmdir();

        public override object Call1(Class last_class, object recv, Frame caller, Proc block, object p1)
        {
            string path = String.StringValue(p1, caller);

            Eval.rb_secure(2, caller); //check_dirname
            try
            {
                System.IO.Directory.Delete(path);
            }
            catch (System.IO.IOException ioe)
            {
                throw SystemCallError.rb_sys_fail(path, ioe, caller).raise(caller);
            }

            return null;
        }
    }

    
    internal class dir_s_glob : MethodBody // author: cjs, status: done
    {
        internal static dir_s_glob singleton = new dir_s_glob();

        public override object Calln(Class last_class, object recv, Frame caller, ArgList args)
        {
            object str;
            int flags;

            if (Class.rb_scan_args(caller, args.ToRubyArray(), 1, 1, false) == 2)
                flags = Numeric.rb_num2long(args[1], caller);
            else
                flags = 0;
            str = args[0];

            return Dir.rb_push_glob(caller, args.block, str, flags);
        }
    }

    
    internal class dir_s_aref : MethodBody1 // author: cjs, status: done
    {
        internal static dir_s_aref singleton = new dir_s_aref();

        public override object Call1(Class last_class, object recv, Frame caller, Proc block, object p1)
        {
            return Dir.rb_push_glob(caller, block, p1, 0);
        }
    }
}
