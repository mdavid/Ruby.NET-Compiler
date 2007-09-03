/**********************************************************************

  Ruby.NET Runtime Library
  Originally developed at Queensland University of Technology
 
  Some sections of this C# code mirror the structure of the C code in the
  Ruby 1.8.2 Interpreter Copyright (C) 1993-2003 Yukihiro Matsumoto, et.al.
  
**********************************************************************/

using Ruby.Runtime;
using System.Globalization;

namespace Ruby
{
    [UsedByRubyCompiler]
    public partial class Range : Object
    {
        internal object Begin
        {
            get { return instance_variable_get("begin");}
            set { instance_variable_set("begin", value);}
        }
        internal object End
        {
            get { return instance_variable_get("end");}
            set { instance_variable_set("end", value);}
        }
        internal bool excl
        {
            get { return Eval.Test(instance_variable_get("excl"));}
            set { instance_variable_set("excl", value);}
        }

        // ----------------------------------------------------------------------------


        internal Range(): base(Ruby.Runtime.Init.rb_cRange) 
        { 
        }

        public Range(Class klass)
            : base(klass) 
        { 
        }

        [UsedByRubyCompiler]
        public Range(object begin, object end, bool exclude_end)
            : this()
        {
            Begin = begin;
            End = end;
            excl = exclude_end;
        }

        // ----------------------------------------------------------------------------

        internal bool Excl
        {
            get { return excl; }
        }


        //-----------------------------------------------------------------


        internal static bool LE(object x, object y, Frame caller)
        {
            object result = Eval.CallPrivate(x, caller, "<=>", null, y);
            if (result is int)
                return 0 >= (int)result;
            else
                return false;
        }

        internal static bool LT(object x, object y, Frame caller)
        {
            object result = Eval.CallPrivate(x, caller, "<=>", null, y);
            if (result is int)
                return 0 < (int)result;
            else
                return false;
        }


        // This method returns Qfalse, Qtrue and Nil. Calling methods sometimes take different
        // action based on the return type. 
        // rb_range_beg_len
        internal static object MapToLength(object recv, int len, bool raiseException, bool trimToMax, out int begp, out int lenp, Frame caller)
        {
            int beg, end, b, e;
            begp = 0;
            lenp = 0;

            if (!Object.rb_obj_is_kind_of(recv, Ruby.Runtime.Init.rb_cRange)) return false;

            Range range = (Range)recv;

            beg = b =  Numeric.rb_num2long(range.Begin, caller);
            end = e = Numeric.rb_num2long(range.End, caller);

            if (beg < 0)
            {
                beg += len;
                if (beg < 0) goto out_of_range;
            }

            if (raiseException)
            {
                if (beg > len) goto out_of_range;
                if (end > len) end = len;
            }

            if (end < 0) end += len;
            if (!range.excl) end++;
            len = end - beg;
            if (len < 0) len = 0;

            begp = beg;
            lenp = len;
            return true;

            out_of_range:            
            if (raiseException)
                throw new RangeError(string.Format(CultureInfo.InvariantCulture, "{0}..{1}{2} out of range", range.Begin, range.Excl ? "." : "", range.End)).raise(caller);

            return null;
        }
    }
}
