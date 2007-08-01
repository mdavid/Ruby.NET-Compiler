/**********************************************************************

  Ruby.NET Runtime Library
  Originally developed at Queensland University of Technology
 
  Some sections of this C# code mirror the structure of the C code in the
  Ruby 1.8.2 Interpreter Copyright (C) 1993-2003 Yukihiro Matsumoto, et.al.
  
**********************************************************************/

using Ruby.Runtime;

namespace Ruby
{

    public partial class Thread : Object
    {

        //instance 
        internal System.Threading.Thread thread;
        internal object result = null;
        internal int safe = 0;
        internal int _priority = 0;
        internal Thread_Status _status = Thread_Status.THREAD_RUNNABLE;
        internal System.Collections.Generic.Dictionary<string, object> locals;
        internal bool abort = false;
        internal ThreadGroup thGroup;
        
        //static 
        internal static System.Collections.Generic.LinkedList<Thread> thread_list; //TODO: Actually write the code to add new threads to the list
        internal static bool rb_thread_critical = false; //TODO: Not done yet - if we kept multiple threads then working out the sematics of this 
                                                                  //could be difficult.
        internal static bool ruby_thread_abort = false; //TODO: Not done yet - back ground logic!!
        //needs a default group

        //-----------------------------------------------------------------

        internal Thread()
            : base(Ruby.Runtime.Init.rb_cThread)
        {
            if (thread_list == null)
            {
                //create the thread_list and add the main thread
                thread_list = new System.Collections.Generic.LinkedList<Thread>();
                thread_list.AddLast(Eval.main_thread);
            }
            thGroup = Eval.thgroup_default;
        }

        protected Thread(Class klass)
            : base(klass)
        {
        }

        //-----------------------------------------------------------------

        internal enum Thread_Status {
            THREAD_TO_KILL, 
            THREAD_RUNNABLE, 
            THREAD_STOPPED, 
            THREAD_KILLED
        };

        //-----------------------------------------------------------------

        internal override object Inner()
        {
            return thread;
        }

        internal Frame caller;
        internal Proc block;
        internal Array args;
        
        internal void DoWork()
        {
            try
            {
                result = Proc.rb_yield(block, caller, args);
            }
            catch
            {

            }
        }

        internal void ThreadStart(Frame caller, Proc block, Array args)
        {
            this.caller = caller;
            this.block = block;
            this.args = args;

            thread = new System.Threading.Thread(this.DoWork);         
            thread.Start();
        }

        //-----------------------------------------------------------------

        //TODO: I didn't put this check in a lot of methods, check all Thread methods and insert it.         
        internal static Thread rb_thread_check(object data, Frame caller)
        {
            if (data == null || !(data is Thread))
            {
                throw new TypeError(string.Format("wrong argument type {0} (expected Thread)", Class.rb_obj_classname(data))).raise(caller);
            }
            return (Thread)data;
        }
        
        //TODO: map the 5 .net threading priorities to the 'integer' priority system for Ruby
        //need to think carefully on this to get it to run like ruby does.
        internal void AdjustPriorities()
        {
            lock (this)
            {

            }
        }
        
        internal static void rb_thread_ready(Thread th)
        {
            // TODO
            // th->wait_for = 0;

            if (th._status != Thread.Thread_Status.THREAD_TO_KILL)
            {
                th._status = Thread.Thread_Status.THREAD_RUNNABLE;
            }
        }

        //TODO: more than one thread can access concurrently - consider locking
        internal static object rb_thread_kill(object thread, Frame caller)
        {
            Thread th = (Thread)thread;

            if (th != Eval.curr_thread && th.safe < 4)
            {
                Eval.rb_secure(4, caller);
            }
            if (th._status == Thread.Thread_Status.THREAD_TO_KILL || th._status == Thread.Thread_Status.THREAD_KILLED)
            {
                return thread;
            }

            //  TODO: undecided whether or not to implement the linked list of 
            //  threads yet?
            //    if (th == th->next || th == main_thread) rb_exit(EXIT_SUCCESS);

            rb_thread_ready(th);

            th._status = Thread.Thread_Status.THREAD_TO_KILL;

            //  TODO: haven't added the thread critical stuff yet
            //    if (!rb_thread_critical) rb_thread_schedule();

            return thread;
        }

        public static object rb_thread_local_aset(object thread, string id, object val, Frame caller)
        {
            Thread th = (Thread)thread;

            if (Eval.ruby_safe_level >= 4 && th != Eval.curr_thread)
            {
                throw new SecurityError("Insecure: can't modify thread locals").raise(caller);
            }
            if (((Thread)thread).Frozen)
            {
                TypeError.rb_error_frozen(caller, "thread locals");
            }
            if (th.locals == null)
            {
                th.locals = new System.Collections.Generic.Dictionary<string, object>();
            }
            if (val == null)
            {
                th.locals.Remove((string)id);
                return null;
            }
            th.locals.Add((string)id, val);
            return val;
        }

        public static object rb_thread_local_aref(object thread, string id, Frame caller)
        {
            Thread th = (Thread)thread;
            object val;
            if (Eval.ruby_safe_level >= 4 && th != Eval.curr_thread)
            {
                throw new SecurityError("Insecure: thread locals").raise(caller);
            }

            if (th.locals == null)
                return null;
            if (th.locals.TryGetValue(id, out val))
            {
                return val;
            }
            return null;
        }
    }
}