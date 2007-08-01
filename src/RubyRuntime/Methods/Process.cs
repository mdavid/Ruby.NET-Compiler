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
    

    internal class p_uid_change_privilege : MethodBody1 // author: cjs, status: done
    {
        internal static p_uid_change_privilege singleton = new p_uid_change_privilege();

        public override object Call1(Class last_class, object recv, Frame caller, Proc block, object p1)
        {
            Process.check_uid_switch(caller);

            int uid = Numeric.rb_num2long(p1, caller);

            if (Process.geteuid() == 0) /* root-user */
                throw NotImplementedError.rb_notimplement(caller, "change_privilege").raise(caller);
            else
                throw NotImplementedError.rb_notimplement(caller, "change_privilege").raise(caller);
        }
    }

    
    internal class p_uid_grant_privilege : MethodBody1 // author: cjs, status: done
    {
        internal static p_uid_grant_privilege singleton = new p_uid_grant_privilege();

        public override object Call1(Class last_class, object recv, Frame caller, Proc block, object p1)
        {
            int id = Numeric.rb_num2long(p1, caller);

            Process.check_uid_switch(caller);

            int uid = Process.getuid();

            throw NotImplementedError.rb_notimplement(caller, "grant_privilege").raise(caller);
        }
    }

    
    internal class p_uid_exchange : MethodBody0 // author: cjs, status: done
    {
        internal static p_uid_exchange singleton = new p_uid_exchange();

        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {
            Process.check_uid_switch(caller);

            int uid = Process.getuid();
            int euid = Process.geteuid();
            
            throw NotImplementedError.rb_notimplement(caller, "re_exchange").raise(caller);
        }
    }

    
    internal class p_uid_exchangeable : MethodBody0 // author: cjs, status: done
    {
        internal static p_uid_exchangeable singleton = new p_uid_exchangeable();

        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {
            return false;
        }
    }

    
    internal class p_uid_have_saved_id : MethodBody0 // author: cjs, status: done
    {
        internal static p_uid_have_saved_id singleton = new p_uid_have_saved_id();

        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {
            return false;
        }
    }

    
    internal class p_uid_switch : MethodBody0 // author: cjs, status: done
    {
        internal static p_uid_switch singleton = new p_uid_switch();

        private object p_uid_sw_ensure(object obj, Frame caller)
        {
            Process.under_uid_switch = false;
            return p_uid_exchange.singleton.Call0(null, obj, caller, null);
        }

        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {
            int uid, euid;

            Process.check_uid_switch(caller);

            uid = Process.getuid();
            euid = Process.geteuid();

            if (uid == euid)
            {
                Errno.errno = Errno.EPERM;
                throw SystemCallError.rb_sys_fail(null, caller).raise(caller);
            }
            p_uid_exchange.singleton.Call0(last_class, recv, caller, block);
            if (block != null)
            {
                Process.under_uid_switch = true;

                object result = null;
                try
                {
                    result = Proc.rb_yield(block, caller, new ArgList());
                }
                finally
                {
                    p_uid_sw_ensure(recv, caller);
                }
                return result;
            }
            else
            {
                return euid;
            }
        }
    }



    
    internal class p_gid_change_privilege : MethodBody1 // author: cjs, status: done
    {
        internal static p_gid_change_privilege singleton = new p_gid_change_privilege();

        public override object Call1(Class last_class, object recv, Frame caller, Proc block, object p1)
        {
            Process.check_gid_switch(caller);

            int gid = Numeric.rb_num2long(p1, caller);

            if (Process.geteuid() == 0) /* root-user */
                throw NotImplementedError.rb_notimplement(caller, "change_privilege").raise(caller);
            else
                throw NotImplementedError.rb_notimplement(caller, "change_privilege").raise(caller);
        }
    }

    
    internal class p_gid_grant_privilege : MethodBody1 // author: cjs, status: done
    {
        internal static p_gid_grant_privilege singleton = new p_gid_grant_privilege();

        public override object Call1(Class last_class, object recv, Frame caller, Proc block, object p1)
        {
            int id = Numeric.rb_num2long(p1, caller);

            Process.check_gid_switch(caller);

            int gid = Process.getgid();
            
            throw NotImplementedError.rb_notimplement(caller, "grant_privilege").raise(caller);
        } 
    }

    
    internal class p_gid_exchange : MethodBody0 // author: cjs, status: done
    {
        internal static p_gid_exchange singleton = new p_gid_exchange();

        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {
            Process.check_gid_switch(caller);

            int gid = Process.getgid();
            int egid = Process.getegid(); 
            
            throw NotImplementedError.rb_notimplement(caller, "re_exchange").raise(caller);
        }
    }

    
    internal class p_gid_exchangeable : MethodBody0 // author: cjs, status: done
    {
        internal static p_gid_exchangeable singleton = new p_gid_exchangeable();

        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {
            return false;
        }
    }

    
    internal class p_gid_have_saved_id : MethodBody0 // author: cjs, status: done
    {
        internal static p_gid_have_saved_id singleton = new p_gid_have_saved_id();

        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {
            return false;
        }
    }

    
    internal class p_gid_switch : MethodBody0 // author: cjs, status: done
    {
        internal static p_gid_switch singleton = new p_gid_switch();

        private object p_gid_sw_ensure(object obj, Frame caller)
        {
            Process.under_gid_switch = false;
            return p_gid_exchange.singleton.Call0(null, obj, caller, null);
        }

        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {
            int gid, egid;

            Process.check_gid_switch(caller);

            gid = Process.getgid();
            egid = Process.getegid();

            if (gid == egid)
            {
                Errno.errno = Errno.EPERM;
                throw SystemCallError.rb_sys_fail(null, caller).raise(caller);
            }
            p_gid_exchange.singleton.Call0(last_class, recv, caller, block);
            if (block != null)
            {
                Process.under_gid_switch = true;
                object result = null;
                try
                {
                    result = Proc.rb_yield(block, caller, new ArgList());
                }
                finally
                {
                    p_gid_sw_ensure(recv, caller);
                }
                return result;
            }
            else
            {
                return egid;
            }
        }
    }


    
    internal class p_sys_setuid : MethodBody1 // author: cjs, status: done
    {
        internal static p_sys_setuid singleton = new p_sys_setuid();

        public override object Call1(Class last_class, object recv, Frame caller, Proc block, object p1)
        {
            throw NotImplementedError.rb_notimplement(caller, "setuid").raise(caller);
        }
    }

    
    internal class p_sys_setgid : MethodBody1 // author: cjs, status: done
    {
        internal static p_sys_setgid singleton = new p_sys_setgid();

        public override object Call1(Class last_class, object recv, Frame caller, Proc block, object p1)
        {
            throw NotImplementedError.rb_notimplement(caller, "setgid").raise(caller);
        }
    }

    
    internal class p_sys_setruid : MethodBody1 // author: cjs, status: done
    {
        internal static p_sys_setruid singleton = new p_sys_setruid();

        public override object Call1(Class last_class, object recv, Frame caller, Proc block, object p1)
        {
            throw NotImplementedError.rb_notimplement(caller, "setruid").raise(caller);
        }
    }

    
    internal class p_sys_setrgid : MethodBody1 // author: cjs, status: done
    {
        internal static p_sys_setrgid singleton = new p_sys_setrgid();

        public override object Call1(Class last_class, object recv, Frame caller, Proc block, object p1)
        {
            throw NotImplementedError.rb_notimplement(caller, "setrgid").raise(caller);
        }
    }

    
    internal class p_sys_seteuid : MethodBody1 // author: cjs, status: done
    {
        internal static p_sys_seteuid singleton = new p_sys_seteuid();

        public override object Call1(Class last_class, object recv, Frame caller, Proc block, object p1)
        {
            throw NotImplementedError.rb_notimplement(caller, "seteuid").raise(caller);
        }
    }

    
    internal class p_sys_setegid : MethodBody1 // author: cjs, status: done
    {
        internal static p_sys_setegid singleton = new p_sys_setegid();

        public override object Call1(Class last_class, object recv, Frame caller, Proc block, object p1)
        {
            throw NotImplementedError.rb_notimplement(caller, "setegid").raise(caller);
        }
    }

    
    internal class p_sys_setreuid : MethodBody2 // author: cjs, status: done
    {
        internal static p_sys_setreuid singleton = new p_sys_setreuid();

        public override object Call2(Class last_class, object recv, Frame caller, Proc block, object p1, object p2)
        {
            throw NotImplementedError.rb_notimplement(caller, "setreuid").raise(caller);
        }
    }

    
    internal class p_sys_setregid : MethodBody2 // author: cjs, status: done
    {
        internal static p_sys_setregid singleton = new p_sys_setregid();

        public override object Call2(Class last_class, object recv, Frame caller, Proc block, object p1, object p2)
        {
            throw NotImplementedError.rb_notimplement(caller, "setregid").raise(caller);
        }
    }

    
    internal class p_sys_setresuid : MethodBody3 // author: cjs, status: done
    {
        internal static p_sys_setresuid singleton = new p_sys_setresuid();

        public override object Call3(Class last_class, object recv, Frame caller, Proc block, object p1, object p2, object p3)
        {
            throw NotImplementedError.rb_notimplement(caller, "setresuid").raise(caller);
        }
    }

    
    internal class p_sys_setresgid : MethodBody3 // author: cjs, status: done
    {
        internal static p_sys_setresgid singleton = new p_sys_setresgid();

        public override object Call3(Class last_class, object recv, Frame caller, Proc block, object p1, object p2, object p3)
        {
            throw NotImplementedError.rb_notimplement(caller, "setresgid").raise(caller);
        }
    }

    
    internal class p_sys_issetugid : MethodBody0 // author: cjs, status: done
    {
        internal static p_sys_issetugid singleton = new p_sys_issetugid();

        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {
            throw NotImplementedError.rb_notimplement(caller, "issetugid").raise(caller);
        }
    }


    
    internal class rb_proc_times : MethodBody0 // author: war, status: done
    {
        internal static rb_proc_times singleton = new rb_proc_times();

        public override object Call0(Class last_klass, object recv, Frame caller, Proc block)
        {
            
                //(RubyRuntime.S_Tms);
            System.Diagnostics.Process process = System.Diagnostics.Process.GetCurrentProcess();

            System.TimeSpan systemTime = process.PrivilegedProcessorTime;
            System.TimeSpan userTime = process.UserProcessorTime;

            return Struct.rb_struct_new(last_klass, Ruby.Runtime.Init.S_Tms, caller, block,
                new Float(userTime.TotalSeconds),
                new Float(systemTime.TotalSeconds),
                new Float(0),
                new Float(0));
        }
    }

    
    internal class get_pid : MethodBody0 // author: cjs, status: done
    {
        internal static get_pid singleton = new get_pid();

        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {
            Eval.rb_secure(2, caller);
            return System.Diagnostics.Process.GetCurrentProcess().Id;
        }
    }

    
    internal class rb_f_exec : VarArgMethodBody0 // author: cjs, status: done
    { 
        internal static rb_f_exec singleton = new rb_f_exec();

        public override object Call(Class last_class, object recv, Frame caller, Proc block, Array rest)
        {
            string prog = null;

            if (rest.Count == 0)
            {
                throw new ArgumentError("wrong number of arguments").raise(caller);
            }

            Array ary = Object.CheckConvert<Array>(rest[0], "to_ary", caller);
            if (ary != null)
            {
                if (ary.Count != 2)
                {
                    throw new ArgumentError("wrong first argument").raise(caller);
                }
                prog = String.StringValue(ary[0], caller);
                rest[0] = ary[1];
            }
            try
            {
                if (rest.Count == 1 && prog == null)
                    Process.rb_proc_exec(String.StringValue(rest[0], caller));
                else
                    Process.proc_exec_n(rest, prog, caller);
            }
            catch (System.Exception e)
            {
                throw SystemCallError.rb_sys_fail(String.StringValue(rest[0], caller), e, caller).raise(caller);
            }
            return null;
        }
    }

    
    internal class rb_f_fork : MethodBody0 // author: cjs, status: done
    {
        internal static rb_f_fork singleton = new rb_f_fork();

        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {
            throw NotImplementedError.rb_notimplement(caller, "fork").raise(caller);
        }
    }

    
    internal class rb_f_exit_bang : VarArgMethodBody0 // author: cjs, status: done
    {
        internal static rb_f_exit_bang singleton = new rb_f_exit_bang();

        public override object Call(Class last_class, object recv, Frame caller, Proc block, Array rest)
        {
            int istatus;

            Eval.rb_secure(4, caller);
            if (Class.rb_scan_args(caller, rest, 0, 1, false) == 1)
            {
                object status = rest[0];
                if (status is bool)
                {
                    istatus = (bool)status == true ? Process.EXIT_SUCCESS : Process.EXIT_FAILURE;
                }
                else
                    istatus = Numeric.rb_num2long(status, caller);
            }
            else
                istatus = Process.EXIT_FAILURE;

            System.Environment.Exit(istatus);

            return null; /* not reached */
        }
    }

    
    internal class rb_f_system : VarArgMethodBody0 // author: cjs, status: done
    {
        internal static rb_f_system singleton = new rb_f_system();

        public override object Call(Class last_class, object recv, Frame caller, Proc block, Array rest)
        {
            System.Console.Out.Flush();
            System.Console.Error.Flush();
            if (rest.Count == 0)
            {
                Process.rb_last_status.value = null;
                throw new ArgumentError("wrong number of arguments").raise(caller);
            }

            string prog = null;
            if (rest[0] is Array)
            {
                Array ary = (Array)rest[0];
                if (ary.Count != 2)
                {
                    throw new ArgumentError("wrong first argument").raise(caller);
                }
                prog = String.StringValue(ary[0], caller);
                rest[0] = ary[1];
            }

            int status;
            if (rest.Count == 1 && prog == null)
                status = Process.do_spawn(Process.P_WAIT, String.StringValue(rest[0], caller));
            else
                status = Process.proc_spawn_n(rest, prog, caller);

            return (status == Process.EXIT_SUCCESS);
        }
    }

    
    internal class rb_f_sleep : VarArgMethodBody0 // author: cjs, status: done
    {
        internal static rb_f_sleep singleton = new rb_f_sleep();

        public override object Call(Class last_class, object recv, Frame caller, Proc block, Array rest)
        {
            int argc = rest.Count;
            System.DateTime start;

            start = System.DateTime.Now;
            if (argc == 0)
            {
                System.Threading.Thread.Sleep(System.Threading.Timeout.Infinite);
            }
            else if (argc == 1)
            {
                System.Threading.Thread.Sleep((int)(Numeric.rb_num2dbl(rest[0], caller) * 1000));
            }
            else
            {
                throw new ArgumentError("wrong number of arguments").raise(caller);
            }

            return (int)System.Math.Round(System.DateTime.Now.Subtract(start).TotalSeconds);
        }
    }

    
    internal class rb_f_kill : VarArgMethodBody1 //author: cjs, status: done
    {
        internal static rb_f_kill singleton = new rb_f_kill();

        public override object Call(Class last_class, object recv, Frame caller, Proc block, object sig, Array rest)
        {
            Eval.rb_secure(2, caller);

            if (rest.Count == 0)
                throw new ArgumentError("wrong number of arguments -- kill(sig, pid...)").raise(caller);

            int signal;
            if (sig is int)
                signal = (int)sig;
            else 
            {
                string sigstr;
                if (sig is Symbol)
                    if (((Symbol)sig).id_s == null)
                        throw new ArgumentError("bad signal").raise(caller);
                    else
                        sigstr = ((Symbol)sig).id_s;
                else if (sig is String)
                    sigstr = ((String)sig).value;
                else
                {
                    sigstr = String.rb_check_string_type(sig, caller).value;
                    if (sigstr == null)
                        throw new ArgumentError(string.Format("bad signal type {0}", ((Basic)sig).my_class._name)).raise(caller);
                }

                if (sigstr.StartsWith("SIG"))
                    sigstr = sigstr.Substring(3);
                if (!Signal.siglist.TryGetValue(sigstr, out signal))
                    throw new ArgumentError(string.Format("unsupported name `SIG{0}'", sigstr)).raise(caller);
            }

            if (signal < 0)
                signal = -signal;

            foreach (int process in rest)
            {
                if ((int)process == System.Diagnostics.Process.GetCurrentProcess().Id)
                    return Stdlib.raise(signal);
                else
                    throw new NotImplementedError("rb_f_kill: process other than current").raise(caller);
            }

            return rest.Count;
        }
    }

    
    internal class proc_wait : VarArgMethodBody0 // author: cjs, status: done
    {
        internal static proc_wait singleton = new proc_wait();

        public override object Call(Class last_class, object recv, Frame caller, Proc block, Array rest)
        {
            int pid, flags, status;

            Eval.rb_secure(2, caller);
            
            flags = 0;
            Class.rb_scan_args(caller, rest, 0, 2, false);
            if (rest.Count == 0)
            {
                pid = -1;
            }
            else
            {
                pid = Numeric.rb_num2long(rest[0], caller);
                if (rest.Count == 2 && rest[1] != null)
                {
                    flags = Numeric.rb_num2long(rest[1], caller);
                }
            }

            if ((pid = Process.rb_waitpid(caller, pid, out status, ref flags)) < 0)
                throw new SystemCallError(0).raise(caller);
            
            if (pid == 0)
                return Process.rb_last_status.value = null;
            else
                return pid;
        }
    }

    
    internal class proc_wait2 : VarArgMethodBody0 // author: cjs, status: done
    {
        internal static proc_wait2 singleton = new proc_wait2();

        public override object Call(Class last_class, object recv, Frame caller, Proc block, Array rest)
        {
            object pid = proc_wait.singleton.Call(last_class, recv, caller, block, rest);
            if (pid == null)
                return null;
            else
                return new Array(new object[] { pid, Process.rb_last_status.value });
        }
    }

    
    internal class proc_waitall : MethodBody0 // author: cjs, status: partial, comment: threads
    {
        internal static proc_waitall singleton = new proc_waitall();

        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {
            object result;

            Eval.rb_secure(2, caller);
            result = new Array();

            return result;
        }
    }

    
    internal class proc_detach : MethodBody1 // author: cjs, status: done
    {
        internal static proc_detach singleton = new proc_detach();

        public override object Call1(Class last_class, object recv, Frame caller, Proc block, object p1)
        {
            Eval.rb_secure(2, caller);
            return Process.rb_detach_process(Numeric.rb_num2long(p1, caller));
        }
    }

    
    internal class get_ppid : MethodBody0 // author: cjs, status: done
    {
        internal static get_ppid singleton = new get_ppid();

        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {
            Eval.rb_secure(2, caller);
            return 0;
        }
    }


    
    internal class proc_getpgrp : MethodBody0 // author: cjs, status: done
    {
        internal static proc_getpgrp singleton = new proc_getpgrp();

        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {
            Eval.rb_secure(2, caller);
            throw NotImplementedError.rb_notimplement(caller, "getpgrp").raise(caller);
        }
    }

    
    internal class proc_setpgrp : MethodBody0 // author: cjs, status: done
    {
        internal static proc_setpgrp singleton = new proc_setpgrp();

        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {
            Eval.rb_secure(2, caller);
            throw NotImplementedError.rb_notimplement(caller, "setpgrp").raise(caller);
        }
    }

    
    internal class proc_getpgid : MethodBody1 // author: cjs, status: done
    {
        internal static proc_getpgid singleton = new proc_getpgid();

        public override object Call1(Class last_class, object recv, Frame caller, Proc block, object p1)
        {
            throw NotImplementedError.rb_notimplement(caller, "getpgid").raise(caller);
        }
    }

    
    internal class proc_setpgid : MethodBody2 // author: cjs, status: done
    {
        internal static proc_setpgid singleton = new proc_setpgid();

        public override object Call2(Class last_class, object recv, Frame caller, Proc block, object p1, object p2)
        {
            throw NotImplementedError.rb_notimplement(caller, "setpgid").raise(caller);
        }
    }

    
    internal class proc_setsid : MethodBody0 // author: cjs, status: done
    {
        internal static proc_setsid singleton = new proc_setsid();

        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {
            throw NotImplementedError.rb_notimplement(caller, "setsid").raise(caller);
        }
    }

    
    internal class proc_getpriority : MethodBody2 // author: cjs, status: done
    {
        internal static proc_getpriority singleton = new proc_getpriority();

        public override object Call2(Class last_class, object recv, Frame caller, Proc block, object p1, object p2)
        {
            throw NotImplementedError.rb_notimplement(caller, "getpriority").raise(caller);
        }
    }

    
    internal class proc_setpriority : MethodBody3 // author: cjs, status: done
    {
        internal static proc_setpriority singleton = new proc_setpriority();

        public override object Call3(Class last_class, object recv, Frame caller, Proc block, object p1, object p2, object p3)
        {
            throw NotImplementedError.rb_notimplement(caller, "setpriority").raise(caller);
        }
    }

    
    internal class proc_getuid : MethodBody0 // author: cjs, status: done
    {
        internal static proc_getuid singleton = new proc_getuid();

        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {
            int uid = Process.getuid();
            return uid;
        }
    }

    
    internal class proc_setuid : MethodBody1 // author: cjs, status: done
    {
        internal static proc_setuid singleton = new proc_setuid();

        public override object Call1(Class last_class, object recv, Frame caller, Proc block, object p1)
        {
            int uid = Numeric.rb_num2long(p1, caller);

            Process.check_uid_switch(caller);

            throw NotImplementedError.rb_notimplement(caller, "uid=").raise(caller);
        }
    }

    
    internal class proc_getgid : MethodBody0 // author: cjs, status: done
    {
        internal static proc_getgid singleton = new proc_getgid();

        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {
            int gid = Process.getgid();
            return gid;
        }
    }

    
    internal class proc_setgid : MethodBody1 // author: cjs, status: done
    {
        internal static proc_setgid singleton = new proc_setgid();

        public override object Call1(Class last_class, object recv, Frame caller, Proc block, object p1)
        {
            int gid = Numeric.rb_num2long(p1, caller);

            Process.check_gid_switch(caller);
            
            throw NotImplementedError.rb_notimplement(caller, "gid=").raise(caller);
        }
    }

    
    internal class proc_geteuid : MethodBody0 // author: cjs, status: done
    {
        internal static proc_geteuid singleton = new proc_geteuid();

        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {
            int euid = Process.geteuid();
            return euid;
        }
    }

    
    internal class proc_seteuid : MethodBody1 // author: cjs, status: done
    {
        internal static proc_seteuid singleton = new proc_seteuid();

        public override object Call1(Class last_class, object recv, Frame caller, Proc block, object p1)
        {
            Process.check_uid_switch(caller);
            throw NotImplementedError.rb_notimplement(caller, "euid=").raise(caller);
        }
    }

    
    internal class proc_getegid : MethodBody0 // author: cjs, status: done
    {
        internal static proc_getegid singleton = new proc_getegid();

        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {
            int egid = Process.getegid();

            return egid;
        }
    }

    
    internal class proc_setegid : MethodBody1 // author: cjs, status: done
    {
        internal static proc_setegid singleton = new proc_setegid();

        public override object Call1(Class last_class, object recv, Frame caller, Proc block, object p1)
        {
            Process.check_gid_switch(caller);
            throw NotImplementedError.rb_notimplement(caller, "egid=").raise(caller);
        }
    }

    
    internal class proc_initgroups : MethodBody2 // author: cjs, status: done
    {
        internal static proc_initgroups singleton = new proc_initgroups();

        public override object Call2(Class last_class, object recv, Frame caller, Proc block, object p1, object p2)
        {
            throw NotImplementedError.rb_notimplement(caller, "initgroups").raise(caller);
        }
    }

    
    internal class proc_getgroups : MethodBody0 // author: cjs, status: done
    {
        internal static proc_getgroups singleton = new proc_getgroups();

        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {
            throw NotImplementedError.rb_notimplement(caller, "getgroups").raise(caller);
        }
    }

    
    internal class proc_setgroups : MethodBody1 // author: cjs, status: done
    {
        internal static proc_setgroups singleton = new proc_setgroups();

        public override object Call1(Class last_class, object recv, Frame caller, Proc block, object p1)
        {
            throw NotImplementedError.rb_notimplement(caller, "setgroups").raise(caller);
        }
    }

    
    internal class proc_getmaxgroups : MethodBody0 // author: cjs, status: done
    {
        internal static proc_getmaxgroups singleton = new proc_getmaxgroups();

        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {
            return Process.MaxGroups;
        }
    }

    
    internal class proc_setmaxgroups : MethodBody1 // author: cjs, status: done
    {
        internal static proc_setmaxgroups singleton = new proc_setmaxgroups();

        public override object Call1(Class last_class, object recv, Frame caller, Proc block, object p1)
        {
            int ngroups = Numeric.rb_num2long(p1, caller);

            if (ngroups > 4096)
                ngroups = 4096;

            Process.MaxGroups = ngroups;

            return Process.MaxGroups;
        }
    }
}
