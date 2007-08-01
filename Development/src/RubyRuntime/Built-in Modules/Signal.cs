/**********************************************************************

  Ruby.NET Runtime Library
  Originally developed at Queensland University of Technology
 
  Some sections of this C# code mirror the structure of the C code in the
  Ruby 1.8.2 Interpreter Copyright (C) 1993-2003 Yukihiro Matsumoto, et.al.
  
**********************************************************************/


using Ruby;
using System.Runtime.InteropServices;

namespace Ruby
{

    public partial class Signal
    {

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate void signalDelegate(int value);


        internal class Stdlib
        {
            [DllImport("msvcrt")]
            public static extern System.IntPtr signal(int signum, signalDelegate handler);

            [DllImport("msvcrt")]
            public static extern int raise(int signum);
        }


        internal const int SIGINT  = 2;
        internal const int SIGILL  = 4;
        internal const int SIGABRT = 22;
        internal const int SIGFPE  = 8;
        internal const int SIGKILL = 9;
        internal const int SIGSEGV = 11;
        internal const int SIGTERM = 15;
        private const int NSIG = 23;

        //------------------------------------------------------------------------

        private static System.Collections.Generic.Dictionary<string, int> _siglist = null;

        internal static System.Collections.Generic.Dictionary<string, int> siglist
        {
            get
            {
                if (_siglist == null)
                    init_siglist();

                return _siglist;
            }
        }

        internal static void init_siglist()
        {
            if (_siglist == null)
            {
                _siglist = new System.Collections.Generic.Dictionary<string, int>();
                _siglist.Add("INT", SIGINT);
                _siglist.Add("ILL", SIGILL);
                _siglist.Add("ABRT", SIGABRT);
                _siglist.Add("FPE", SIGFPE);
                _siglist.Add("KILL", SIGKILL);
                _siglist.Add("SEGV", SIGSEGV);
                _siglist.Add("TERM", SIGTERM);
            }
        }

        //------------------------------------------------------------------------

        internal delegate void SigHandler(int sig);

        [DllImport("msvcrt.dll")]
        private static extern SigHandler signal(int sig, SigHandler func);

        internal static void install_sighandler(int sig, SigHandler handler)
        {
            SigHandler old = signal(sig, handler);

            if (old != null)
            {
                signal(sig, old);
            }
        }

        internal static void sighandler(int sig) // author: cjs, status: partial
        {
            if (sig >= NSIG)
            {
                //FIXME: caller
                Exception.rb_bug(string.Format("trap_handler: Bad signal {0}", sig), null);
            }

            signal(sig, sighandler);

            //if (ATOMIC_TEST(rb_trap_immediate))
            //{
            //    IN_MAIN_CONTEXT(signal_exec, sig);
            //    ATOMIC_SET(rb_trap_immediate, 1);
            //}
            //else
            //{
            //    ATOMIC_INC(rb_trap_pending);
            //    ATOMIC_INC(trap_pending_list[sig]);
            //}
        }

        internal static void sigsegv(int sig) // author: cjs, status: partial
        {
            //FIXME: caller
            Exception.rb_bug("Segmentation fault", null);
        }

        internal static void init_sigchld(int sig) // author: cjs, status: partial
        {
            SigHandler old = signal(sig, null);
            if (old != null)
            {
                signal(sig, old);
            }
            else
            {
                //FIXME: trap_list
                //trap_list[sig].cmd = 0;
            }
        }

        //------------------------------------------------------------------------

        internal static string ruby_signal_name(int no)
        {
            if (siglist.ContainsValue(no))
            {
                foreach (string key in siglist.Keys)
                    if (siglist[key] == no)
                        return key;
            }

            return null;
        }
    }
}
