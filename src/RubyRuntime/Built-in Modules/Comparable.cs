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

    public partial class Comparable
    {
        internal static int Compare(object a, object b, Frame caller) //status: done
        {
            object value = Eval.CallPrivate(a, caller, "<=>", null, b);
            if (value == null)
            {
                throw new ArgumentError(string.Format(CultureInfo.InvariantCulture, "comparison of {0} with {1} failed", Class.rb_obj_classname(a), Class.rb_obj_classname(b))).raise(caller);
            }
            else if (value is int)
            {
                return (int)value;
            }
            else if (value is Bignum)
            {
                return ((Bignum)value).value >= 0 ? 1 : -1;
            }
            else if ((bool)Eval.CallPrivate(value, caller, ">", null, 0))
            {
                return 1;
            }
            else if ((bool)Eval.CallPrivate(value, caller, "<", null, 0))
            {
                return -1;
            }
            else
            {
                return 0;
            }
        }
    }
}
