/**********************************************************************

  Ruby.NET Runtime Library
  Originally developed at Queensland University of Technology
 
  Some sections of this C# code mirror the structure of the C code in the
  Ruby 1.8.2 Interpreter Copyright (C) 1993-2003 Yukihiro Matsumoto, et.al.
  
**********************************************************************/

using Ruby.Runtime;
using Ruby;
using System.Globalization;


namespace Ruby.Methods
{

    
    internal class marshal_dump : VarArgMethodBody0 // author: war, status: done, comment: minor issues in Marshal helper methods. 
    {

        internal static marshal_dump singleton = new marshal_dump();

        //WARTAG: minor issues in Marshal helper methods that need testing 
        public override object Call(Class last_class, object recv, Frame caller, Proc block, Array args) // author: war, status: done 
        {
            object obj;
            object a1;
            object a2;
            object port;
            int limit = -1;

            Marshal.dump_arg arg = new Marshal.dump_arg();
            Marshal.dump_call_arg c_arg = new Marshal.dump_call_arg();

            port = null;
            int argc = Class.rb_scan_args(caller, args, 1, 2, false);
            if (argc == 3)
            {
                obj = args[0];
                a1 = args[1];
                a2 = args[2];
                if (a2 != null) limit = Numeric.rb_num2long(a2, caller);
                if (a1 == null) goto type_error;
                port = a1;
            }
            else if (argc == 2)
            {
                obj = args[0];
                a1 = args[1];
                if (a1 is int) limit = (int)a1;
                else if (a1 == null) goto type_error;
                else port = a1;
            }
            else
            {
                obj = args[0];
            }
            arg.dest = null;
            if (port != null)
            {
                if (!Eval.RespondTo(port, Marshal.s_write))
                {
                    goto type_error;
                }
                arg.str = new String();
                arg.dest = port;
                if (Eval.RespondTo(port, Marshal.s_binmode))
                {
                    Eval.CallPrivate0(port, caller, Marshal.s_binmode, null);
                }
            }
            else
            {
                port = new String();
                arg.str = port;
            }
      
            arg.taint = false;
            c_arg.obj = obj;
            c_arg.arg = arg;
            c_arg.limit = limit;

            Marshal.w_byte((char)Marshal.MARSHAL_MAJOR, arg, caller);
            Marshal.w_byte((char)Marshal.MARSHAL_MINOR, arg, caller);

            try
            {
                Marshal.dump(c_arg, caller);
            }
            finally
            {
                Marshal.dump_ensure(arg);
            }

            return port;

        type_error:
            throw new TypeError("instance of IO needed").raise(caller);
        }
    }

    //WARTAG: minor issues in Marshal helper methods that need testing
    
    internal class marshal_load : VarArgMethodBody0 // author: war, status: done    
    {
        internal static marshal_load singleton = new marshal_load();

        //load( from [, aProc ] ) -> anObject 
        public override object Call(Class last_class, object recv, Frame caller, Proc block, Array argv)
        {
            object port = null;
            object proc = null;
            int major, minor;
            object v = null;
            Marshal.load_arg arg = new Marshal.load_arg();
            string portString;

            int argc = Class.rb_scan_args(caller, argv, 1, 1, false);
            if (argc == 1)
            {
                port = argv[0];
            }
            else
            {
                proc = argv[1];
            }
            if (Eval.RespondTo(port, "to_str")) //TEST: make sure this code works if port is unassigned
            {                
                arg.taint = ((Basic)port).Tainted;                
                portString = String.StringValue(port, caller);
            }
            else if (Eval.RespondTo(port, Marshal.s_getc) && Eval.RespondTo(port, Marshal.s_read))
            {
                if (Eval.RespondTo(port, Marshal.s_binmode))
                {
                    Eval.CallPrivate0(port, caller, Marshal.s_binmode, null);
                }
                arg.taint = true;
            }
            else
            {
                throw new TypeError("instance of IO needed").raise(caller);
            }
            arg.src = port;
            arg.offset = 0;

            major = Marshal.r_byte(caller, arg);
            minor = Marshal.r_byte(caller, arg);

            if (major != Marshal.MARSHAL_MAJOR || minor > Marshal.MARSHAL_MINOR)
            {
                string errorString = "incompatible marshal file format (can't be read)\n" +
                    "\tformat version {0}.{1} required; {2}.{3} given";
                throw new TypeError(string.Format(CultureInfo.InvariantCulture, errorString, Marshal.MARSHAL_MAJOR, Marshal.MARSHAL_MINOR, major, minor)).raise(caller);
            }
            if (Eval.Test(Options.ruby_verbose) && minor != Marshal.MARSHAL_MINOR)
            {

                string errorString = "incompatible marshal file format (can be read)\n" +
                    "\tformat version {0}.{1} required; {2}.{3} given";
                Errors.rb_warn(string.Format(CultureInfo.InvariantCulture, errorString, Marshal.MARSHAL_MAJOR, Marshal.MARSHAL_MINOR, major, minor));
            }

            arg.data   = new Hash();

            if (proc == null) arg.proc = null;
            else arg.proc = proc;
            
            try
            {
                v = Marshal.load(caller, arg);
            }
            finally
            {
                Marshal.load_ensure(arg); 
            }

            return v;
        }
    }
}
