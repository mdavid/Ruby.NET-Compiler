/**********************************************************************

  Ruby.NET Runtime Library
  Originally developed at Queensland University of Technology
 
  Some sections of this C# code mirror the structure of the C code in the
  Ruby 1.8.2 Interpreter Copyright (C) 1993-2003 Yukihiro Matsumoto, et.al.
  
**********************************************************************/

using Ruby.Runtime;
using Ruby;

namespace Ruby.Methods
{
    
    internal class pst_equal : MethodBody1 // author: cjs, status: done
    {
        internal static pst_equal singleton = new pst_equal();

        public override object Call1(Class last_class, object recv, Frame caller, Proc block, object p1)
        {
            if (recv == p1)
                return true;
            else
                return Object.Equals(((ProcessStatus)recv).st, p1);
        }
    }

    
    internal class pst_bitand : MethodBody1 // author: cjs, status: done
    {
        internal static pst_bitand singleton = new pst_bitand();

        public override object Call1(Class last_class, object recv, Frame caller, Proc block, object p1)
        {
            return ((ProcessStatus)recv).st & Numeric.rb_num2long(p1, caller);
        }
    }

    
    internal class pst_rshift : MethodBody1 // author: cjs, status: done
    {
        internal static pst_rshift singleton = new pst_rshift();

        public override object Call1(Class last_class, object recv, Frame caller, Proc block, object p1)
        {
            return ((ProcessStatus)recv).st >> Numeric.rb_num2long(p1, caller);
        }
    }

    
    internal class pst_to_i : MethodBody0 // author: cjs, status: done
    {
        internal static pst_to_i singleton = new pst_to_i();

        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {
            return ((ProcessStatus)recv).st;
        }
    }

    
    internal class pst_to_s : MethodBody0 // author: cjs, status: done
    {
        internal static pst_to_s singleton = new pst_to_s();

        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {
            return new String(pst_to_i.singleton.Call0(last_class, recv, caller, null).ToString());
        }
    }

    
    internal class pst_inspect : MethodBody0 // author: cjs, status: done
    {
        internal static pst_inspect singleton = new pst_inspect();

        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {
            ProcessStatus status = ((ProcessStatus)recv);

            System.Text.StringBuilder buf = new System.Text.StringBuilder();

            buf.Append(string.Format("#<{0}: pid={1}", status.my_class._name, status.pid));
            if (status.WIFSTOPPED())
            {
                int stopsig = status.WSTOPSIG();
                string signame = Signal.ruby_signal_name(stopsig);
                if (signame != null)
                    buf.Append(string.Format(",stopped(SIG{0}={1})", signame, stopsig));
                else
                    buf.Append(string.Format(",stopped({0})", stopsig));
            }
            if (status.WIFSIGNALED())
            {
                int termsig = status.WTERMSIG();
                string signame = Signal.ruby_signal_name(termsig);
                if (signame != null)
                    buf.Append(string.Format(",signaled(SIG{0}={1})", signame, termsig));
                else
                    buf.Append(string.Format(",signaled({0})", termsig));
            }
            if (status.WIFEXITED())
            {
                buf.Append(string.Format(",exited({0})", status.WEXITSTATUS()));
            }
            buf.Append(">");

            return new String(buf.ToString());
        }
    }

    
    internal class pst_pid : MethodBody0 // author: cjs, status: done
    {
        internal static pst_pid singleton = new pst_pid();

        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {
            return ((ProcessStatus)recv).pid;
        }
    }

    
    internal class pst_wifstopped : MethodBody0 // author: cjs, status: done
    {
        internal static pst_wifstopped singleton = new pst_wifstopped();

        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {
            ProcessStatus status = ((ProcessStatus)recv);

            return (status.WIFSTOPPED());
        }
    }

    
    internal class pst_wstopsig : MethodBody0 // author: cjs, status: done
    {
        internal static pst_wstopsig singleton = new pst_wstopsig();

        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {
            ProcessStatus status = ((ProcessStatus)recv);

            if (status.WIFSTOPPED())
                return (status.WSTOPSIG());
            return null;
        }
    }


    
    internal class pst_wifsignaled : MethodBody0 // author: cjs, status: done
    {
        internal static pst_wifsignaled singleton = new pst_wifsignaled();

        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {
            ProcessStatus status = ((ProcessStatus)recv);

            return (status.WIFSIGNALED());
        }
    }


    
    internal class pst_wtermsig : MethodBody0 // author: cjs, status: done
    {
        internal static pst_wtermsig singleton = new pst_wtermsig();

        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {
            ProcessStatus status = ((ProcessStatus)recv);

            if (status.WIFSIGNALED())
                return status.WTERMSIG();
            return null;
        }
    }

    
    internal class pst_wifexited : MethodBody0 // author: cjs, status: done
    {
        internal static pst_wifexited singleton = new pst_wifexited();

        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {
            ProcessStatus status = ((ProcessStatus)recv);

            return (status.WIFEXITED());
        }
    }

    
    internal class pst_wexitstatus : MethodBody0 // author: cjs, status: done
    {
        internal static pst_wexitstatus singleton = new pst_wexitstatus();

        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {
            ProcessStatus status = ((ProcessStatus)recv);

            if (status.WIFEXITED())
                return status.WEXITSTATUS();
            return null;
        }
    }

    
    internal class pst_success_p : MethodBody0 // author: cjs, status: done
    {
        internal static pst_success_p singleton = new pst_success_p();

        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {
            ProcessStatus status = ((ProcessStatus)recv);

            if (!status.WIFEXITED())
                return null;
            return status.WEXITSTATUS() == Process.EXIT_SUCCESS;
        }

    }

    
    internal class pst_wcoredump : MethodBody0 // author: cjs, status: done
    {
        internal static pst_wcoredump singleton = new pst_wcoredump();

        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {
            return false;
        }
    }
}
