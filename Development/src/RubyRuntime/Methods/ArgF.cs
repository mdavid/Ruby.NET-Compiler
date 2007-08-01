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
    
    internal class argf_binmode : MethodBody0 // author: cjs, status: done
    {
        internal static argf_binmode singleton = new argf_binmode();

        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {
            IO.argf_binmode = true;
            IO.next_argv(caller);
            IO.ARGF_FORWARD(caller);
            rb_io_binmode.singleton.Call0(last_class, IO.current_file, caller, null);
            return IO.argf;
        }
    }


    
    internal class argf_close_m : MethodBody0 // author: cjs, status: done
    {
        internal static argf_close_m singleton = new argf_close_m();

        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {
            IO.next_argv(caller);
            IO.argf_close(IO.current_file, caller);
            if (IO.next_p != -1)
                IO.next_p = 1;
            IO.gets_lineno = 0;
            return IO.argf;
        }
    }


    
    internal class argf_closed : MethodBody0 // author: cjs, status: done
    {
        internal static argf_closed singleton = new argf_closed();

        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {
            IO.next_argv(caller);
            IO.ARGF_FORWARD(caller);
            return IO.rb_io_closed(caller, IO.current_file);
        }
    }


    
    internal class argf_each_byte : MethodBody0 // author: cjs, status: done
    {
        internal static argf_each_byte singleton = new argf_each_byte();

        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {
            object byt;

            while ((byt = argf_getc.singleton.Call0(last_class, recv, caller, null)) != null)
            {
                Proc.rb_yield(block, caller, new object[] { byt });
            }
            return IO.argf;
        }
    }


    
    internal class argf_each_line : VarArgMethodBody0 // author: cjs, status: done
    {
        internal static argf_each_line singleton = new argf_each_line();

        public override object Call(Class last_class, object recv, Frame caller, Proc block, Array rest)
        {
            String str;

            if (!IO.next_argv(caller))
                return null;

            if (IO.current_file is File)
            {
                for (; ; )
                {
                    if (!IO.next_argv(caller))
                        return IO.argf;
                    Eval.CallPrivate(IO.current_file, caller, "each", block, new object[] { 0 });
                    IO.next_p = 1;
                }
            }
            while ((str = IO.argf_getline(rest.value.ToArray(), caller))!=null)
            {
                Proc.rb_yield(block, caller, new object[] { str });
            }
            return IO.argf;
        }
    }


    
    internal class argf_eof : MethodBody0 // author: cjs, status: done
    {
        internal static argf_eof singleton = new argf_eof();

        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {
            if (IO.current_file != null)
            {
                if (IO.init_p == 0)
                    return true;
                IO.ARGF_FORWARD(caller);
                if (Eval.Test(rb_io_eof.singleton.Call0(last_class, IO.current_file, caller, null)))
                    return true;
            }
            return false;
        }
    }


    
    internal class argf_file : MethodBody0 // author: cjs, status: done
    {
        internal static argf_file singleton = new argf_file();

        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {
            IO.next_argv(caller);
            return IO.current_file;
        }
    }


    
    internal class argf_filename : MethodBody0 // author: cjs, status: done
    {
        internal static argf_filename singleton = new argf_filename();

        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {
            IO.next_argv(caller);
            return IO.filename.value;
        }
    }


    
    internal class argf_fileno : MethodBody0 // author: cjs, status: done
    {
        internal static argf_fileno singleton = new argf_fileno();

        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {
            if (!IO.next_argv(caller))
                throw new ArgumentError("no stream").raise(caller);
            IO.ARGF_FORWARD(caller);
            
            return rb_io_fileno.singleton.Call0(last_class, IO.current_file, caller, null);
        }
    }


    
    internal class argf_getc : MethodBody0 // author: cjs, status: done
    {
        internal static argf_getc singleton = new argf_getc();

        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {
            if (!IO.next_argv(caller))
                return null;

            object bite; //byte
            while (true)
            {
                if (!(IO.current_file is File))
                    bite = Eval.CallPrivate(IO.current_file, caller, "getc", null, new object[] { });
                else
                    bite = rb_io_getc.singleton.Call0(last_class, IO.current_file, caller, null);

                if (bite == null && IO.next_p != -1)
                {
                    IO.argf_close(IO.current_file, caller);
                    IO.next_p = 1;
                    continue;
                }
                break;
            }

            return bite;
        }
    }


    
    internal class argf_lineno : MethodBody0 // author: cjs, status: done
    {
        internal static argf_lineno singleton = new argf_lineno();

        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {
            return IO.lineno_global.value;
        }
    }


    
    internal class argf_read : VarArgMethodBody0 // author: cjs, status: done
    {
        internal static argf_read singleton = new argf_read();

        public override object Call(Class last_class, object recv, Frame caller, Proc block, Array argv)
        {
            object tmp = null, length = null;
            String str = null;
            long len = 0;
            int argc = argv.Count;

            Class.rb_scan_args(caller, argv, 0, 2, false);

            if (argc > 0)
            {
                length = argv[0];
                len = Numeric.rb_num2long(length, caller);
            }
            if (argc > 1)
            {
                str = (String)argv[1];
                str.value = "";
                argv[1] = null;
            }

        retry:
            if (!IO.next_argv(caller))
            {
                return str;
            }
            
            if (IO.current_file is File)
            {
                tmp = IO.argf_forward(caller);
            }
            else
            {
                tmp = io_read.singleton.Calln(last_class, IO.current_file, caller, new ArgList(null, argv.value.ToArray()));
            }
            
            if (str == null)
                str = (String)tmp;
            else if (tmp != null)
                str.value += tmp;

            if (tmp == null || length == null)
            {
                if (IO.next_p != -1)
                {
                    IO.argf_close(IO.current_file, caller);
                    IO.next_p = 1;
                    goto retry;
                }
            }
            else if (argc >= 1)
            {
                if (str.value.Length < len)
                {
                    len -= str.value.Length;
                    argv[0] = len;
                    goto retry;
                }
            }

            return str;
        }
    }


    
    internal class argf_readchar : MethodBody0 // author: cjs, status: done
    {
        internal static argf_readchar singleton = new argf_readchar();

        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {
            IO.NEXT_ARGF_FORWARD(caller);
            object c = argf_getc.singleton.Call0(last_class, recv, caller, null);

            if (c == null)
                throw EOFError.rb_eof_error().raise(caller);

            return c;
        }
    }


    
    internal class argf_rewind : MethodBody0 // author: cjs, status: done
    {
        internal static argf_rewind singleton = new argf_rewind();

        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {
            if (!IO.next_argv(caller))
                throw new ArgumentError("no stream to rewind").raise(caller);
            IO.ARGF_FORWARD(caller);
            return rb_io_rewind.singleton.Call0(last_class, IO.current_file, caller, null);
        }
    }


    
    internal class argf_seek_m : VarArgMethodBody0 // author: cjs, status: done
    {
        internal static argf_seek_m singleton = new argf_seek_m();

        public override object Call(Class last_class, object recv, Frame caller, Proc block, Array rest)
        {
            if (!IO.next_argv(caller))
                throw new ArgumentError("no stream to seek").raise(caller);
            IO.ARGF_FORWARD(caller);

            return rb_io_seek_m.singleton.Call(last_class, IO.current_file, caller, null, rest); 
        }
    }


    
    internal class argf_set_lineno : MethodBody1 // author: cjs, status: done
    {
        internal static argf_set_lineno singleton = new argf_set_lineno();

        public override object Call1(Class last_class, object recv, Frame caller, Proc block, object p1)
        {
            IO.gets_lineno = Integer.rb_num2long(p1, caller);
            IO.lineno_global.value = IO.gets_lineno;
            return null;
        }
    }


    
    internal class argf_set_pos : MethodBody1 // author: cjs, status: done
    {
        internal static argf_set_pos singleton = new argf_set_pos();

        public override object Call1(Class last_class, object recv, Frame caller, Proc block, object p1)
        {
            if (!IO.next_argv(caller))
                throw new ArgumentError("no stream to set position").raise(caller);
            IO.ARGF_FORWARD(caller);

            return rb_io_set_pos.singleton.Call1(last_class, IO.current_file, caller, null, p1);
        }
    }


    
    internal class argf_skip : MethodBody0 // author: cjs, status: done
    {
        internal static argf_skip singleton = new argf_skip();

        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {
            if (IO.next_p != -1)
            {
                IO.argf_close(IO.current_file, caller);
                IO.next_p = 1;
            }
            return IO.argf;
        }
    }


    
    internal class argf_tell : MethodBody0 // author: cjs, status: done
    {
        internal static argf_tell singleton = new argf_tell();

        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {
            if (!IO.next_argv(caller))
                throw new ArgumentError("no stream to tell").raise(caller);
            IO.ARGF_FORWARD(caller);

            return rb_io_tell.singleton.Call0(last_class, IO.current_file, caller, null);
        }
    }


    
    internal class argf_to_io : MethodBody0 // author: cjs, status: done
    {
        internal static argf_to_io singleton = new argf_to_io();

        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {
            IO.next_argv(caller);
            IO.ARGF_FORWARD(caller);
            return IO.current_file;
        }
    }


    
    internal class argf_to_s : MethodBody0 // author: cjs, status: done
    {
        internal static argf_to_s singleton = new argf_to_s();

        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {
            return new String("ARGF");
        }
    }
}
