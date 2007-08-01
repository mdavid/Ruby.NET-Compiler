/**********************************************************************

  Ruby.NET Runtime Library
  Originally developed at Queensland University of Technology
 
  Some sections of this C# code mirror the structure of the C code in the
  Ruby 1.8.2 Interpreter Copyright (C) 1993-2003 Yukihiro Matsumoto, et.al.
  
**********************************************************************/

using Ruby.Runtime;
using Ruby;
using System.Runtime.InteropServices;
using System.Collections.Generic;

namespace Ruby.Methods
{

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void sighandler(int value);


    internal class Stdlib
    {
        [DllImport("msvcrt")]
        internal static extern System.IntPtr signal(int signum, sighandler handler);

        [DllImport("msvcrt")]
        internal static extern int raise(int signum);
    }
   
    internal class sig_trap : MethodBody1 //status: partial
    {
        internal static sig_trap singleton = new sig_trap();

        public override object Call1(Class last_class, object recv, Frame caller, Proc block, object sig)
        {
            return trap(sig, block);
        }

        public override object Call2(Class last_class, object recv, Frame caller, Proc block, object sig, object handler)
        {
            return trap(sig, handler);
        }

        static List<sighandler> handlers = new List<sighandler>();

        internal object trap(object sig, object arg)
        {
            int signal = 0;
            switch (((String)sig).value)
            {
                case "SIGINT":
                    signal = 2; 
                    break;
                case "SIGILL":
                    signal = 4;
                    break;
                case "SIGFPE":
                    signal = 8;
                    break;
                case "SIGSEGV":
                    signal = 11;
                    break;
                case "SIGTERM":
                    signal = 15;
                    break;
                case "SIGBREAK":
                    signal = 21;
                    break;
                case "SIGABRT":
                    signal = 22;
                    break;
            }

            sighandler handler;
            if (arg is Proc)
                handler = new sighandler(new Handler((Proc)arg).HandleSignal);
            else if (arg is sighandler)
                handler = (sighandler)arg;
            else
                handler = null;

            handlers.Add(handler); // (avoid garbage collection of unmanaged handlers)

            return Stdlib.signal(signal, handler).ToInt32();
        }

        
        private class Handler
        {
            private Proc block;

            internal Handler(Proc block)
            {
                this.block = block;
            }

            internal void HandleSignal(int value)
            {
                Proc.rb_yield(block, null, value);
            }
        }
    }

    
    internal class sig_list : MethodBody0 // author: cjs, status: done
    {
        internal static sig_list singleton = new sig_list();

        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {
            Hash h = new Hash();

            foreach (string key in Signal.siglist.Keys)
                h.Add(new String(key), Signal.siglist[key]);

            return h;
        }
    }
}
