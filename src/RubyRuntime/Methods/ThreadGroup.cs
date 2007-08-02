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
    internal class thgroup_s_alloc : MethodBody0 //author:war, status: done, comment: untested
    {
        internal static thgroup_s_alloc singleton = new thgroup_s_alloc();

        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {
            ThreadGroup group = new ThreadGroup((Class)recv);

            group.enclosed = 0;
            group.group = group; //self reference

            return group;            
        }
    }

    
    internal class thgroup_list : MethodBody0 //author:war, status: done, comment: untested
    {
        internal static thgroup_list singleton = new thgroup_list();

        public override object Call0(Class last_class, object group, Frame caller, Proc block)
        {
            ThreadGroup data;            
            Array ary;

            data = (ThreadGroup)group;
            ary = new Array();

            foreach(Thread th in Thread.thread_list){
                if (th.thGroup == data.group)
                {
                    ary.Add(th.thread);
                }
            }

            return ary;
        }
    }

    
    internal class thgroup_enclose : MethodBody0 //author:war, status: done, comment: untested
    {
        internal static thgroup_enclose singleton = new thgroup_enclose();

        public override object Call0(Class last_class, object group, Frame caller, Proc block)
        {
            ThreadGroup data;

            data = (ThreadGroup)group;
            data.enclosed = 1;

            return group; 
        }
    }

    
    internal class thgroup_enclosed_p : MethodBody0 //author:war, status: done, comment: untested
    {
        internal static thgroup_enclosed_p singleton = new thgroup_enclosed_p();

        public override object Call0(Class last_class, object group, Frame caller, Proc block)
        {
            ThreadGroup data;

            data = (ThreadGroup)group;
            if (data.enclosed > 0)
            {
                return true;
            }
            return false;           
        }
    }

    
    internal class thgroup_add : MethodBody1 //author:war, status: not supported, comment: untested
    {
        internal static thgroup_add singleton = new thgroup_add();

        public override object Call1(Class last_class, object group, Frame caller, Proc block, object thread)
        {
            throw new Ruby.NotImplementedError("thgroup_add not supported").raise(caller);

            //Thread th;
            //ThreadGroup data;

            //Eval.rb_secure(4, caller);
            //th = Thread.rb_thread_check(thread, caller);

            ////TODO: Why is this significant?
            ////if (!th->next || !th->prev) {
            ////    rb_raise(rb_eTypeError, "wrong argument type %s (expected Thread)",
            ////        rb_obj_classname(thread));
            ////}

            //if (((Object)group).Frozen)
            //{
            //    throw new ThreadError("can't move to the frozen thread group").raise(caller);
            //}
            
            //data = (ThreadGroup)group;
            //if (data.enclosed > 0)
            //{
            //    throw new ThreadError("can't move to the enclosed thread group").raise(caller);
            //}

            //if (th.thGroup == null) //not sure how this could happen yet?
            //{
            //    return null;
            //}
            //if (th.thGroup.Frozen)
            //{
            //    throw new ThreadError("can't move from the frozen thread group").raise(caller);
            //}
            //data = th.thGroup;
            //if (data.enclosed > 0)
            //{
            //    throw new ThreadError("can't move from the enclosed thread group").raise(caller);
            //}
            //th.thGroup = (ThreadGroup)group;

            //return group;
        } 
    }
}
