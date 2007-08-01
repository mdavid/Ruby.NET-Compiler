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
    
    internal class test_d : MethodBody1 // author: cjs, status: done
    {
        internal static test_d singleton = new test_d();

        public override object Call1(Class last_class, object recv, Frame caller, Proc block, object p1)
        {
            return System.IO.Directory.Exists(((String)p1).value);
        }
    }

    
    internal class test_e : MethodBody1 // author: cjs, status: done
    {
        internal static test_e singleton = new test_e();

        public override object Call1(Class last_class, object recv, Frame caller, Proc block, object p1)
        {
            return System.IO.Directory.Exists(((String)p1).value) || System.IO.File.Exists(((String)p1).value);
        }
    }

    
    internal class test_r : MethodBody1 // author: cjs, status: done
    {
        internal static test_r singleton = new test_r();

        public override object Call1(Class last_class, object recv, Frame caller, Proc block, object p1)
        {
            return FileTest.access(((String)p1).value, System.Security.AccessControl.FileSystemRights.ReadData);
        }
    }

    
    internal class test_R : MethodBody1 // author: cjs, status: done
    {
        internal static test_R singleton = new test_R();

        public override object Call1(Class last_class, object recv, Frame caller, Proc block, object p1)
        {
            return test_r.singleton.Call1(last_class, recv, caller, block, p1);
        }
    }

    
    internal class test_w : MethodBody1 // author: cjs, status: done
    {
        internal static test_w singleton = new test_w();

        public override object Call1(Class last_class, object recv, Frame caller, Proc block, object p1)
        {
            return FileTest.access(((String)p1).value, System.Security.AccessControl.FileSystemRights.WriteData);
        }
    }

    
    internal class test_W : MethodBody1 // author: cjs, status: done
    {
        internal static test_W singleton = new test_W();

        public override object Call1(Class last_class, object recv, Frame caller, Proc block, object p1)
        {
            return test_w.singleton.Call1(last_class, recv, caller, block, p1);
        }
    }

    
    internal class test_x : MethodBody1 // author: cjs, status: done
    {
        internal static test_x singleton = new test_x();

        public override object Call1(Class last_class, object recv, Frame caller, Proc block, object p1)
        {
            return FileTest.access(((String)p1).value, System.Security.AccessControl.FileSystemRights.ExecuteFile);
        }
    }

    
    internal class test_X : MethodBody1 // author: cjs, status: done
    {
        internal static test_X singleton = new test_X();

        public override object Call1(Class last_class, object recv, Frame caller, Proc block, object p1)
        {
            return test_x.singleton.Call1(last_class, recv, caller, block, p1);
        }
    }

    
    internal class test_f : MethodBody1 // author: cjs, status: done
    {
        internal static test_f singleton = new test_f();

        public override object Call1(Class last_class, object recv, Frame caller, Proc block, object p1)
        {
            return System.IO.File.Exists(((String)p1).value);
        }
    }

    
    internal class test_z : MethodBody1 // author: cjs, status: done
    {
        internal static test_z singleton = new test_z();

        public override object Call1(Class last_class, object recv, Frame caller, Proc block, object p1)
        {
            return System.IO.File.Exists(((String)p1).value) && (new System.IO.FileInfo(((String)p1).value).Length == 0);
        }
    }

    
    internal class test_s : MethodBody1 // author: cjs, status: done
    {
        internal static test_s singleton = new test_s();

        public override object Call1(Class last_class, object recv, Frame caller, Proc block, object p1)
        {
            if (!System.IO.File.Exists(((String)p1).value))
                return null;

            System.IO.FileInfo file = new System.IO.FileInfo(((String)p1).value);
            if (file.Length == 0)
                return null;
            else
                return file.Length;
        }
    }

    
    internal class test_owned : MethodBody1 // author: cjs, status: done
    {
        internal static test_owned singleton = new test_owned();

        public override object Call1(Class last_class, object recv, Frame caller, Proc block, object p1)
        {
            return FileTest.owned(((String)p1).value);
        }
    }

    
    internal class test_grpowned : MethodBody1 // author: cjs, status: done
    {
        internal static test_grpowned singleton = new test_grpowned();

        public override object Call1(Class last_class, object recv, Frame caller, Proc block, object p1)
        {
            return false;
        }
    }

    
    internal class test_p : MethodBody1 // author: cjs, status: done
    {
        internal static test_p singleton = new test_p();

        public override object Call1(Class last_class, object recv, Frame caller, Proc block, object p1)
        {
            return false;
        }
    }

    
    internal class test_l : MethodBody1 // author: cjs, status: done
    {
        internal static test_l singleton = new test_l();

        public override object Call1(Class last_class, object recv, Frame caller, Proc block, object p1)
        {
            return false;
        }
    }

    
    internal class test_S : MethodBody1 // author: cjs, status: done
    {
        internal static test_S singleton = new test_S();

        public override object Call1(Class last_class, object recv, Frame caller, Proc block, object p1)
        {
            return false;
        }
    }

    
    internal class test_b : MethodBody1 // author: cjs, status: done
    {
        internal static test_b singleton = new test_b();

        public override object Call1(Class last_class, object recv, Frame caller, Proc block, object p1)
        {
            return false;
        }
    }

    
    internal class test_c : MethodBody1 // author: cjs, status: done
    {
        internal static test_c singleton = new test_c();

        public override object Call1(Class last_class, object recv, Frame caller, Proc block, object p1)
        {
            if (recv is IO)
                return !(((IO)recv).f is System.IO.FileStream);
            else
                return false;
        }
    }

    
    internal class test_suid : MethodBody1 // author: cjs, status: done
    {
        internal static test_suid singleton = new test_suid();

        public override object Call1(Class last_class, object recv, Frame caller, Proc block, object p1)
        {
            return false;
        }
    }

    
    internal class test_sgid : MethodBody1 // author: cjs, status: done
    {
        internal static test_sgid singleton = new test_sgid();

        public override object Call1(Class last_class, object recv, Frame caller, Proc block, object p1)
        {
            return false;
        }
    }

    
    internal class test_sticky : MethodBody1 // author: cjs, status: done
    {
        internal static test_sticky singleton = new test_sticky();

        public override object Call1(Class last_class, object recv, Frame caller, Proc block, object p1)
        {
            return false;
        }
    }
}
