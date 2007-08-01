/**********************************************************************

  Ruby.NET Runtime Library
  Originally developed at Queensland University of Technology
 
**********************************************************************/


using System.Collections.Generic;
using System.Text;
using PERWAPI;


namespace Ruby.Compiler.AST
{

    internal class VALUE : Node, ISimple
    {
        internal object value;


        internal VALUE(object value, YYLTYPE location)
            : base(location)
        {
            this.value = value;
        }

        public override string ToString()
        {
            return value.ToString();
        }

        public void GenSimple(CodeGenContext context)
        {
            if (value is string)// T_STRING,
            {
                context.ldstr((string)(value));
                context.newobj(Runtime.String.ctor);
                return;
            }
            if (value is int)   // T_FIXNUM
            {
                context.ldc_i4((int)(value));
                context.box(PrimitiveType.Int32);
                return;
            }
            if (value is double)// T_FLOAT
            {
                context.ldc_r8((double)(value));
                context.newobj(Runtime.Float.ctor);
                return;
            }
            if (value is ID)    // T_SYMBOL
            {
                context.ldstr(((ID)value).ToString());
                context.newobj(Runtime.Symbol.ctor);
                return;
            }
            if (value is BigNum)
            {
                BigNum num = (BigNum) value;
                context.ldc_i4(num.sign);
                context.ldstr(num.ToString());
                context.ldc_i4(num.bas);
                context.newobj(Runtime.Bignum.ctor);
                return;
            }

            throw new System.NotImplementedException("VALUE " + value.GetType().ToString());
        }



        internal static string RSTRING(VALUE v)
        {
            return (string)v.value;
        }


        internal static VALUE str_new(string str, int length, YYLTYPE location)
        {
            return new VALUE(str.Substring(0, length), location);
        }


        internal static VALUE str_cat(VALUE str, string ptr, int len, YYLTYPE location)
        {
            return new VALUE((string)str.value + ptr.Substring(0, (int)len), location);
        }


        internal static VALUE StringToNumber(string buffer, int bas, YYLTYPE location)
        {
            int str = 0;
            while (char.IsWhiteSpace(buffer[str]) || buffer[str] == '_')
                str++;

            int sign = 1;

            if (buffer[str] == '+')
            {
                str++;
            }
            else if (buffer[str] == '-')
            {
                str++;
                sign = -1;
            }

            switch (bas)
            {
                case 2:
                    if (buffer[str] == '0' && str + 1 < buffer.Length && (buffer[str + 1] == 'b' || buffer[str + 1] == 'B'))
                        str += 2;
                    break;
                case 8:
                    if (buffer[str] == '0' && str + 1 < buffer.Length && (buffer[str + 1] == 'o' || buffer[str + 1] == 'O'))
                        str += 2;
                    break;
                case 10:
                    if (buffer[str] == '0' && str + 1 < buffer.Length && (buffer[str + 1] == 'd' || buffer[str + 1] == 'D'))
                        str += 2;
                    break;
                case 16:
                    if (buffer[str] == '0' && str + 1 < buffer.Length && (buffer[str + 1] == 'x' || buffer[str + 1] == 'X'))
                        str += 2;
                    break;
            }

            List<uint> data = new List<uint>();
            while (str < buffer.Length)
            {
                char c = buffer[str++];

                uint d;
                if (char.IsDigit(c))
                    d = (uint)(c - '0');
                else if (char.IsLower(c))
                    d = (uint)(c - 'a') + 10;
                else if (char.IsUpper(c))
                    d = (uint)(c - 'A') + 10;
                else
                    break;

                if (d >= bas)
                    break;
                else
                    data.Add(d);
            }
            
            try
            {
                int num = 0;

                foreach (int d in data)
                {
                    checked {num = num * bas + d; }
                }
                            
                return new VALUE(sign * num, location);
            }
            catch (System.OverflowException)
            {
                return new VALUE(new BigNum(sign, data, bas), location);
            }
        }
    }


    
    internal class BigNum
    {
        internal int sign;
        internal int bas;
        internal List<uint> data;

        internal BigNum(int sign, List<uint> data, int bas)
        {
            this.sign = sign;
            this.data = data;
            this.bas = bas;
        }

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();
            foreach (uint d in data)
                builder.Append((char)(d < 10 ? '0' + d : 'A' + d - 10));

            return builder.ToString();
        }
    }
}
