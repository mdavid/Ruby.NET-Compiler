/**********************************************************************

  Ruby.NET Runtime Library
  Originally developed at Queensland University of Technology
 
  Some sections of this C# code mirror the structure of the C code in the
  Ruby 1.8.2 Interpreter Copyright (C) 1993-2003 Yukihiro Matsumoto, et.al.
  
**********************************************************************/

using Ruby.Runtime;
using Ruby;
using System.Globalization;

namespace Ruby
{

    public partial class Process
    {
        private class get_pid_global : readonly_global
        {
            internal override object getter(string id, Frame caller)
            {
                return System.Diagnostics.Process.GetCurrentProcess().Id;
            }
        }

        internal static readonly_global get_pid = new get_pid_global();            // (get_pid)
        internal static readonly_global rb_last_status = new readonly_global();   // (readonly)

        internal const int P_WAIT        = 0;
        internal const int P_NOWAIT      = 1;
        internal const int OLD_P_OVERLAY = 2;
        internal const int P_OVERLAY     = 2;
        internal const int P_NOWAITO     = 3;
        internal const int P_DETACH      = 4;

        internal const int _WAIT_CHILD      = 0;
        internal const int _WAIT_GRANDCHILD = 1;

        internal const int EXIT_SUCCESS = 0;
        internal const int EXIT_FAILURE = 1;

        internal static int MaxGroups = 32;

        internal static bool under_uid_switch = false;
        internal static void check_uid_switch(Frame caller)
        {
            Eval.rb_secure(2, caller);
            if (under_uid_switch)
            {
                throw new RuntimeError("can't handle UID during evaluating the block given to the Process::UID.switch method").raise(caller);
            }
        }

        internal static bool under_gid_switch = false;
        internal static void check_gid_switch(Frame caller)
        {
            Eval.rb_secure(2, caller);
            if (under_gid_switch)
            {
                throw new RuntimeError("can't handle GID during evaluating the block given to the Process::UID.switch method").raise(caller);
            }
        }

        internal static int getuid()
        {
            return 0;
        }

        internal static int geteuid()
        {
            return 0;
        }

        internal static int getgid()
        {
            return 0;
        }

        internal static int getegid()
        {
            return 0;
        }

        internal static int do_spawn(int process_status, string command)
        {
            command = command.Trim();
            switch (process_status)
            {
                case P_OVERLAY:
                    // FIXME: this doesn't actually work like EXECV should.
                    // simply creates another process using the existing resource. It doesn't overlay
                    System.Diagnostics.Process exec = System.Diagnostics.Process.GetCurrentProcess();

                    exec.StartInfo.UseShellExecute = false;

                    if (command.StartsWith("\""))
                    {
                        int split = command.IndexOf('\"', 1);
                        exec.StartInfo.FileName = command.Substring(1, split);
                        if (command.Length > split + 1)
                            exec.StartInfo.Arguments = command.Substring(split + 1);
                    }
                    else if (command.StartsWith("\'"))
                    {
                        int split = command.IndexOf('\'', 1);
                        exec.StartInfo.FileName = command.Substring(1, split);
                        if (command.Length > split + 1)
                            exec.StartInfo.Arguments = command.Substring(split + 1);
                    }
                    else if (command.IndexOf(' ') >= 0)
                    {
                        int split = command.IndexOf(' ');
                        exec.StartInfo.FileName = command.Substring(0, split);
                        if (command.Length > split + 1)
                            exec.StartInfo.Arguments = command.Substring(split + 1);
                    }
                    else
                        exec.StartInfo.FileName = command;

                    if (exec.Start()) // FIXME: at this point all ruby processing should stop (if we had a multithreaded situation)
                        exec.WaitForExit();

                    System.Environment.Exit(exec.ExitCode);
                    
                    exec.Dispose();

                    return EXIT_SUCCESS;
                case P_WAIT:
                default:
                    System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo("cmd", "/C " + command);
                    startInfo.UseShellExecute = false;

                    System.Diagnostics.Process p = System.Diagnostics.Process.Start(startInfo);

                    if (p == null)
                        return -1;

                    p.WaitForExit();

                    int result = p.ExitCode;

                    p.Dispose();

                    return result;
            }
        }

        internal static int do_aspawn(int process_status, string command, string[] arguments)
        {
            return do_spawn(process_status, string.Format(CultureInfo.InvariantCulture, "{0} {1}", command, string.Join(" ", arguments)));
        }

        internal static int proc_spawn_n(Array argv, string prog, Frame caller)
        {
            string[] args = new string[argv.Count];
            for (int i = 0; i < argv.Count; i++)
                args[i] = String.StringValue(argv[i], caller);

            return do_aspawn(P_WAIT, prog, args);
        }
        
        internal static void rb_proc_exec(string cmd)
        {
            do_spawn(P_OVERLAY, cmd);
        }

        internal static void proc_exec_n(Array argv, string prog, Frame caller)
        {
            string[] args = new string[argv.Count];
            for (int i = 0; i < argv.Count; i++)
                args[i] = String.StringValue(argv[i], caller);

            do_aspawn(P_OVERLAY, prog, args);
        }

        internal static int rb_waitpid(Frame caller, int pid, out int status, ref int flags)
        {
            System.Diagnostics.Process p;
            if (pid == -1)
                p = null; //System.Diagnostics.Process.GetCurrentProcess(); // current process will block itself! instead of child processes?
            else
                p = System.Diagnostics.Process.GetProcessById(pid);

            if (p != null)
            {
                rb_last_status.value = new ProcessStatus(p);
                p.WaitForExit();

                status = 0;
                return pid;
            }

            if (flags != 0)
            {
                throw new ArgumentError("can't do waitpid with flags").raise(caller);
            }

            status = 0;
            int result = 0;
            //for (; ; )
            //{
            //    TRAP_BEG;
            //    result = wait(st);
            //    TRAP_END;
            //    if (result < 0)
            //    {
            //        if (errno == EINTR)
            //        {
            //            rb_thread_schedule();
            //            continue;
            //        }
            //        return -1;
            //    }
            //    if (result == pid)
            //    {
            //        break;
            //    }
            //    if (!pid_tbl)
            //        pid_tbl = st_init_numtable();
            //    st_insert(pid_tbl, pid, st);
            //    if (!rb_thread_alone()) 
            //        rb_thread_schedule();
            //}

            if (result > 0)
            {
                rb_last_status.value = new ProcessStatus(p);
            }
            return result;
        }

        //private static void detach_process_watcer(object sender, EventArgs e)
        //{
        //    return;
        //}

        internal static int rb_detach_process(int pid)
        {
            // FIXME: Is reaping automatically done by windows OS anyway?
            //System.Diagnostics.Process p = System.Diagnostics.Process.GetProcessById(pid);
            //if (p != null)
            //    p.Exited += new EventHandler(detach_process_watcer);

            return 1;
        }
    }
}
