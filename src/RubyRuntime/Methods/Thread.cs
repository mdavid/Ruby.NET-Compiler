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
    internal class rb_thread_s_new : VarArgMethodBody0 //author:war, status: not supported
    {
        internal static rb_thread_s_new singleton = new rb_thread_s_new();

        public override object Call(Class klass, object recv, Frame caller, Proc block, Array rest)
        {
            throw new NotImplementedError("rb_thread_s_new not supported").raise(caller);

            //Ruby.Thread th = new Thread();
            ////volatile VALUE *pos;

            ////pos = th->stk_pos;
            //Eval.CallPrivate(th, caller, "initialize", block, rest.value.ToArray());
            ////if (th->stk_pos == 0) {
            ////    rb_raise(rb_eThreadError, "uninitialized thread - check `%s#initialize'",
            ////        rb_class2name(klass));
            ////}

            //return th;
        }
    }

    
    internal class rb_thread_initialize : VarArgMethodBody0 //author:war, status: not supported
    {
        internal static rb_thread_initialize singleton = new rb_thread_initialize();

        public override object Call(Class last_class, object thread, Frame caller, Proc block, Array args)
        {
            throw new Ruby.NotImplementedError("rb_thread_initialize not supported").raise(caller);

            //if (block == null)
            //{
            //    throw new ThreadError("`initialize': must be called with a block (ThreadError)").raise(caller);
            //}


            ////this is naive
            //((Thread)thread).ThreadStart(caller, block, args);
            //return null;

            ////TODO: a lot of work is going on in here. 
            ////return rb_thread_start_0(rb_thread_yield, args, Thread.rb_thread_check(thread));

        }
    }


    //#define FOREACH_THREAD(x) FOREACH_THREAD_FROM(curr_thread,x)
    //#define END_FOREACH(x)    END_FOREACH_FROM(curr_thread,x)


    
    internal class rb_thread_start : VarArgMethodBody2 //author:war, status: not supported, comment: need to make sure that subclass isn't initialized() when Thread is subclassed.
    {
        internal static rb_thread_start singleton = new rb_thread_start();

        public override object Call(Class last_class, object recv, Frame caller, Proc block, object p1, object p2, Array rest)
        {
            throw new NotImplementedError("rb_thread_start not supported").raise(caller);

            //Ruby.Thread thread = new Thread();
            //if (block == null)
            //{
            //    throw new Ruby.ThreadError("`initialize': must be called with a block (ThreadError)").raise(caller);
            //}
            //thread.ThreadStart(caller, block, rest);

            ////  HOW DO I GENERATE THIS ERROR?
            ////    if (th->stk_pos == 0) {
            ////        rb_raise(rb_eThreadError, "uninitialized thread - check `%s#initialize'",
            ////            rb_class2name(klass));
            ////    }
            //return thread;
        }
    }


    
    internal class rb_thread_stop : VarArgMethodBody0 //status: not supported
    {
        internal static rb_thread_stop singleton = new rb_thread_stop();

        public override object Call(Class last_class, object recv, Frame caller, Proc block, Array rest)
        {
            throw new NotImplementedError("rb_thread_stop").raise(caller);
        }
    }


    
    internal class rb_thread_s_kill : MethodBody1 //author: war, status: done, comment: untested
    {
        internal static rb_thread_s_kill singleton = new rb_thread_s_kill();

        public override object Call1(Class last_class, object recv, Frame caller, Proc block, object th)
        {
            return Thread.rb_thread_kill(th, caller);
        }
    }


    
    internal class rb_thread_exit : MethodBody0 //author: war, status: done, comment: untested
    {
        internal static rb_thread_exit singleton = new rb_thread_exit();

        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {
            return Thread.rb_thread_kill(recv, caller);
        }
    }



    
    internal class rb_thread_pass : MethodBody0 //author: war, status: done
    {
        internal static rb_thread_pass singleton = new rb_thread_pass();

        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {
            //crude way in which to schedule another thread
            //wartag: the semantics of pass may force us to do some
            //scheduling on top of the .Net scheduler or just do it
            //the ruby way and have only one thread
            System.Threading.Thread.Sleep(5);
            return null;
        }
    }


    
    internal class rb_thread_current : MethodBody0 //status: not supported
    {
        internal static rb_thread_current singleton = new rb_thread_current();

        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {
            throw new Ruby.NotImplementedError("rb_thread_current not supported").raise(caller);

            ////wartag: this won't work as we have multiple threads running. 
            //return Eval.curr_thread;
        }
    }

    
    internal class rb_thread_main : MethodBody0 //author: war, status: done, comment: untested
    {
        internal static rb_thread_main singleton = new rb_thread_main();

        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {
            return Eval.main_thread;
        }
    }

    
    internal class rb_thread_list : MethodBody0 //author: war, status: done, comment: untested
    {
        internal static rb_thread_list singleton = new rb_thread_list();

        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {
            Array ary = new Array();
            foreach (Thread th in Thread.thread_list)
            {
                switch (th._status)
                {
                    case Thread.Thread_Status.THREAD_RUNNABLE:
                        goto case Thread.Thread_Status.THREAD_TO_KILL;
                    case Thread.Thread_Status.THREAD_STOPPED:
                        goto case Thread.Thread_Status.THREAD_TO_KILL;
                    case Thread.Thread_Status.THREAD_TO_KILL:
                        ary.Add(th);
                        break;
                    default:
                        // break; //TODO: this break was in the ruby code, 
                        // they must order their lists in some intelligent way?
                        break;
                }
            }
            return ary;
        }
    }


    
    internal class rb_thread_critical_get : MethodBody0 //author: war, status: not supported 
    {
        internal static rb_thread_critical_get singleton = new rb_thread_critical_get();

        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {
            throw new Ruby.NotImplementedError("rb_thread_critical_get not supported").raise(caller);

            ////this code is just checking a variable - nothing going on in the background yet
            //return Thread.rb_thread_critical ? true : false;
        }
    }


    
    internal class rb_thread_critical_set : MethodBody1 //author: war, status: not supported
    {
        internal static rb_thread_critical_set singleton = new rb_thread_critical_set();

        public override object Call1(Class last_class, object recv, Frame caller, Proc block, object val)
        {
            // BBTAG: for Singleton library...
            //throw new Ruby.NotImplementedError("rb_thread_critical_set not supported").raise(caller);

            ////this code is just setting a variable - nothing going on in the background yet
            //Thread.rb_thread_critical = Marshal.RTEST(val);
            //return val;
            Thread.rb_thread_critical = Marshal.RTEST(val);
            return val;
        }
    }

    
    internal class rb_thread_s_abort_exc : MethodBody0 //author: war, status: not supported, comment: background logic
    {
        internal static rb_thread_s_abort_exc singleton = new rb_thread_s_abort_exc();

        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {
            throw new Ruby.NotImplementedError("rb_thread_s_abort_exc not supported").raise(caller);

            //return Thread.ruby_thread_abort ? true : false;
        }
    }


    
    internal class rb_thread_s_abort_exc_set : MethodBody1 //author: war, status: not supported, comment: background logic
    {
        internal static rb_thread_s_abort_exc_set singleton = new rb_thread_s_abort_exc_set();

        public override object Call1(Class last_class, object recv, Frame caller, Proc block, object val)
        {
            throw new Ruby.NotImplementedError("rb_thread_s_abort_exc_set not supported").raise(caller);

            //Eval.rb_secure(4, caller);
            //Thread.ruby_thread_abort = Marshal.RTEST(val);
            //return val;
        }
    }

    

    internal class rb_thread_raise_m : VarArgMethodBody1 //status: not supported
    {
        internal static rb_thread_raise_m singleton = new rb_thread_raise_m();

        public override object Call(Class last_class, object recv, Frame caller, Proc block, object p1, Array rest)
        {
            throw new NotImplementedError("rb_thread_raise_m").raise(caller);
        }
    }

    
    internal class rb_thread_abort_exc : MethodBody0 //author: war, status: not supported, comment: background logic
    {
        internal static rb_thread_abort_exc singleton = new rb_thread_abort_exc();

        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {
            throw new Ruby.NotImplementedError("rb_thread_raise_m not supported").raise(caller);

            //return ((Thread)recv).abort;
        }
    }


    
    internal class rb_thread_abort_exc_set : MethodBody1 //author: war, status: not supported, comment: background logic
    {
        internal static rb_thread_abort_exc_set singleton = new rb_thread_abort_exc_set();

        public override object Call1(Class last_class, object recv, Frame caller, Proc block, object val)
        {
            throw new Ruby.NotImplementedError("rb_thread_abort_exc_set not supported").raise(caller);
            
            //Eval.rb_secure(4, caller);
            //((Thread)recv).abort = Marshal.RTEST(val);
            //return val;
        }
    }


    
    internal class rb_thread_priority_set : MethodBody1 //author: war, status: done
    {
        internal static rb_thread_priority_set singleton = new rb_thread_priority_set();

        public override object Call1(Class klass, object recv, Frame caller, Proc block, object prio)
        {
            Eval.rb_secure(4, caller);
            Thread th = ((Thread)recv);
            th._priority = (int)prio;
            th.AdjustPriorities();
            return prio;
        }
    }

    
    internal class rb_thread_safe_level : MethodBody0 //author: war, status: done
    {
        internal static rb_thread_safe_level singleton = new rb_thread_safe_level();

        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {
            Thread th = (Thread)recv;
            if (th == Eval.curr_thread)
            {
                return Eval.ruby_safe_level;
            }
            return th.safe;
        }
    }


    
    internal class rb_thread_group : MethodBody0 //status: not supported
    {
        internal static rb_thread_group singleton = new rb_thread_group();

        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {
            throw new NotImplementedError("rb_thread_group").raise(caller);
        }
    }

    
    internal class rb_thread_aref : MethodBody1 //author: war, status: done, comment: untested
    {
        internal static rb_thread_aref singleton = new rb_thread_aref();

        public override object Call1(Class last_class, object thread, Frame caller, Proc block, object id)
        {
            Thread th = (Thread)thread;
            return rb_hash_aref.singleton.Call1(last_class, th.locals, caller, null, id);
        }
    }

    
    internal class rb_thread_aset : MethodBody2 //author: war, status: done, comment: untested
    {
        internal static rb_thread_aset singleton = new rb_thread_aset();

        public override object Call2(Class klass, object thread, Frame caller, Proc block, object id, object val)
        {
            Thread th = (Thread)thread;

            if (th.locals == null)
                th.locals = new System.Collections.Generic.Dictionary<string,object>();
            return rb_hash_aset.singleton.Call2(klass, th.locals, caller, null, id, val);
        }
    }

    
    internal class rb_thread_key_p : MethodBody1 //author: war, status: done, comment: untested
    {
        internal static rb_thread_key_p singleton = new rb_thread_key_p();

        public override object Call1(Class klass, object thread, Frame caller, Proc block, object id)
        {
            Thread th = (Thread)thread;

            if (th.locals == null)
                return false;

            return Ruby.Methods.rb_hash_has_key.singleton.Call1(klass, th.locals, caller, null, id);
        }
    }


    
    internal class rb_thread_keys : MethodBody0 //author: war, status: done, comment: untested
    {
        internal static rb_thread_keys singleton = new rb_thread_keys();

        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {
            Thread th = ((Thread)recv);

            if (th.locals != null)
                return Ruby.Methods.rb_hash_keys.singleton.Call0(last_class, th.locals, caller, null);
            else
                return new Array();
        }
    }


    
    internal class rb_thread_inspect : MethodBody0 //status: not supported
    {
        internal static rb_thread_inspect singleton = new rb_thread_inspect();

        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {
            throw new NotImplementedError("rb_thread_inspect").raise(caller);
        }
    }


 
    internal class rb_thread_join_m : VarArgMethodBody0 //status: done
    {
        internal static rb_thread_join_m singleton = new rb_thread_join_m();

        public override object Call(Class last_class, object recv, Frame caller, Proc block, Array rest)
        {
            Class.rb_scan_args(caller, rest, 0, 1, false);
            if (rest.Count == 0)
            {
                ((Thread)recv).thread.Join();
            }
            else
            {
                double delay = Numeric.rb_num2dbl(rest[0], caller);
                ((Thread)recv).thread.Join((int)(delay / 1000));
            }

            return recv;
        }
    }

    
    internal class rb_thread_alive_p : MethodBody0 //status: done
    {
        internal static rb_thread_alive_p singleton = new rb_thread_alive_p();

        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {
            return ((Thread)recv).thread.IsAlive;
        }
    }

    

    internal class rb_thread_stop_p : MethodBody0 //status: done
    {
        internal static rb_thread_stop_p singleton = new rb_thread_stop_p();

        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {
            return ((Thread)recv).thread.ThreadState != System.Threading.ThreadState.Running;
        }
    }

    

    internal class rb_thread_kill : MethodBody0 //status: done
    {
        internal static rb_thread_kill singleton = new rb_thread_kill();

        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {
            ((Thread)recv).thread.Interrupt();
            return recv;
        }
    }
    
    internal class rb_thread_priority : MethodBody0 //author: war, status: done
    {
        internal static rb_thread_priority singleton = new rb_thread_priority();

        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {            
            return (int)((Thread)recv)._priority;
        }
    }


   
    internal class rb_thread_wakeup : MethodBody0 //status: not supported
    {
        public static rb_thread_wakeup singleton = new rb_thread_wakeup();

        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {
            throw new Ruby.NotImplementedError("rb_thread_wakeup not supported").raise(caller);

            //// fixme
            //return recv;
        }
    }





    internal class rb_thread_value : MethodBody0 //status: done
    {
        internal static rb_thread_value singleton = new rb_thread_value();

        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {
            Thread thread = (Thread)recv;
            thread.thread.Join();
            return thread.result;
        }
    }

      
    internal class rb_thread_status : MethodBody0 //status: not supported
    {
        public static rb_thread_status singleton = new rb_thread_status();

        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {
            throw new Ruby.NotImplementedError("rb_thread_status not supported").raise(caller);

            //// fixme, abort with exception?
            //// possibly need to store exception value in thread - like result

            //switch (((Thread)recv).thread.ThreadState)
            //{
            //    case System.Threading.ThreadState.StopRequested:
            //    case System.Threading.ThreadState.SuspendRequested:
            //    case System.Threading.ThreadState.Unstarted:
            //    case System.Threading.ThreadState.AbortRequested:
            //    case System.Threading.ThreadState.Background:
            //    case System.Threading.ThreadState.Running:
            //        return "run";
            //    case System.Threading.ThreadState.Aborted:
            //        return null;
            //    case System.Threading.ThreadState.Stopped:
            //        return false;
            //    case System.Threading.ThreadState.Suspended:
            //    case System.Threading.ThreadState.WaitSleepJoin:
            //    default:
            //        return "sleep";
            //}
        }
    }


    internal class rb_thread_run : MethodBody0 //status: done
    {
        internal static rb_thread_run singleton = new rb_thread_run();

        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {
            return recv;
        }
    }
}
