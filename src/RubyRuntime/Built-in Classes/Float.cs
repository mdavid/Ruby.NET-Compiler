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
    public partial class Float : Numeric
    {
        internal double value;

        //-----------------------------------------------------------------
        
        [UsedByRubyCompiler]
        public Float(double value)
            : base(Ruby.Runtime.Init.rb_cFloat)
        {
            this.value = value;
        }

        protected Float(Class klass) : base(klass) 
        { 
        }


        //-----------------------------------------------------------------

        internal override object Inner()
        {
            return value;
        }

        public override string ToString() // status: done
        {
            return value.ToString(CultureInfo.InvariantCulture);
        }


        //-----------------------------------------------------------------


        internal static int rb_dbl_cmp(double a, double b) // status: done
        {
            return (a == b) ? 0 : (a > b) ? 1 : -1;
        }

        internal static Float rb_Float(object o, Frame caller)
        {
            if (o is int)
            {
                return new Float((double)(int)o);
            }
            else if (o is double)
            {
                return new Float((double)o);
            }
            else if (o is Bignum)
            {
                return new Float((double)((Bignum)o).value);
            }
            else if (o is String)
            {
                return new Float(String.rb_str_to_dbl(caller, (String)o, true));
            }
            else if (o == null)
            {
                throw new  TypeError("cannot convert nil into Float").raise(caller);
            }
            else
            {
                Float f = Object.Convert<Float>(o, "to_f", caller);
                if (System.Double.IsNaN(f.value))
                {
                    throw new ArgumentError("invalid value for Float()").raise(caller);
                }
                else
                {
                    return f;
                }
            }
        }
    }
}

