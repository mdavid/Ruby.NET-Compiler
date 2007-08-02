/**********************************************************************

  Ruby.NET Runtime Library
  Originally developed at Queensland University of Technology
 
  Some sections of this C# code mirror the structure of the C code in the
  Ruby 1.8.2 Interpreter Copyright (C) 1993-2003 Yukihiro Matsumoto, et.al.
  
**********************************************************************/

using Ruby.Runtime;
using System;
using System.Collections.Generic;
using System.Text;

namespace Ruby
{

    public partial class Pack
    {
        internal static char[] uu_table = "`!\"#$%&'()*+,-./0123456789:;<=>?@ABCDEFGHIJKLMNOPQRSTUVWXYZ[\\]^_".ToCharArray();
        internal static char[] b64_table = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/".ToCharArray();
        internal static char[] hex_table = "0123456789ABCDEF".ToCharArray();
        internal static int EOF = -1;
        internal static long[] utf8_limits = {
            0x0,                /* 1 */
            0x80,                /* 2 */
            0x800,                /* 3 */
            0x10000,            /* 4 */
            0x200000,            /* 5 */
            0x4000000,            /* 6 */
            0x80000000,            /* 7 */
        };

        // -----------------------------------------------------------------------------

        internal static object THISFROM(object ary, int idx)
        {
            return ((Array)ary).value[idx];
        }

        internal static object NEXTFROM(Frame caller, object ary, ref int idx, ref int items)
        {
            object from;
            if (items-- > 0)
            {
                from = ((Array)ary).value[idx++];
            }
            else
            {
                throw new ArgumentError("too few arguments").raise(caller);
            }
            return from;
        }

        internal static int PACK_ITEM_ADJUST(object ary, int tmp)
        {
            while (tmp-- > 0)
            {
                ((Array)ary).value.Add(null);
            }
            return tmp;
        }

        internal static int PACK_LENGTH_ADJUST(int s, int send, ref int len, bool star, int sz)
        {
            int tmp;
            tmp = 0;
            if (len > (send - s) / sz)
            {
                if (!star)
                {
                    tmp = len - (send - s) / sz;
                }
                len = (send - s) / sz;
            }
            return tmp;
        }

        internal static void qpencodes(System.Text.StringBuilder res, object from, int len)
        {
            char[] buff = new char[1024];
            int i = 0, n = 0, prev = EOF;
            string s = ((String)from).value;
            int sIndex = 0;
            int send = s.Length;

            while (sIndex < send)
            {

                if ((s[sIndex] > 126) ||
                    (s[sIndex] < 32 && s[sIndex] != '\n' && s[sIndex] != '\t') ||
                    (s[sIndex] == '='))
                {
                    buff[i++] = '=';
                    buff[i++] = hex_table[s[sIndex] >> 4];
                    buff[i++] = hex_table[s[sIndex] & 0x0f];
                    n += 3;
                    prev = EOF;
                }
                else if (s[sIndex] == '\n')
                {
                    if (prev == ' ' || prev == '\t')
                    {
                        buff[i++] = '=';
                        buff[i++] = s[sIndex];
                    }
                    buff[i++] = s[sIndex];
                    n = 0;
                    prev = s[sIndex];
                }
                else
                {
                    buff[i++] = s[sIndex];
                    n++;
                    prev = s[sIndex];
                }
                if (n > len)
                {
                    buff[i++] = '=';
                    buff[i++] = '\n';
                    n = 0;
                    prev = '\n';
                }
                if (i > 1024 - 5)
                {
                    rb_str_buf_cat(res, buff, i);
                    i = 0;
                }
                sIndex++;
            }
            if (n > 0)
            {
                buff[i++] = '=';
                buff[i++] = '\n';
            }
            if (i > 0)
            {
                rb_str_buf_cat(res, buff, i);
            }
        }

        internal static void encodes(System.Text.StringBuilder res, string s, int len, int type)
        {
            char[] buff = new char[len * 4 / 3 + 6];
            int i = 0;
            int sIndex = 0;
            char[] trans = type == 'u' ? uu_table : b64_table;
            int padding;

            if (type == 'u')
            {
                buff[i++] = (char)(len + ' ');
                padding = '`';
            }
            else
            {
                padding = '=';
            }
            while (len >= 3)
            {
                buff[i++] = trans[0x3F & (s[sIndex] >> 2)];
                buff[i++] = trans[0x3F & (((s[sIndex] << 4) & 0x30) | ((s[1 + sIndex] >> 4) & 0xF))];
                buff[i++] = trans[0x3F & (((s[1 + sIndex] << 2) & 0x3C) | ((s[2 + sIndex] >> 6) & 0x3))];
                buff[i++] = trans[0x3F & s[2 + sIndex]];
                sIndex += 3;
                len -= 3;
            }
            if (len == 2)
            {
                buff[i++] = trans[0x3F & (s[sIndex] >> 2)];
                buff[i++] = trans[0x3F & (((s[sIndex] << 4) & 0x30) | ((s[1 + sIndex] >> 4) & 0xF))];
                buff[i++] = trans[0x3F & (((s[1 + sIndex] << 2) & 0x3C) | (('\0' >> 6) & 0x3))];
                buff[i++] = (char)padding;
            }
            else if (len == 1)
            {
                buff[i++] = trans[0x3F & (s[sIndex] >> 2)];
                buff[i++] = trans[0x3F & (((s[sIndex] << 4) & 0x30) | (('\0' >> 4) & 0xF))];
                buff[i++] = (char)padding;
                buff[i++] = (char)padding;
            }
            buff[i++] = '\n';
            rb_str_buf_cat(res, buff, i);
        }

        internal static int uv_to_utf8(Frame caller, char[] buf, uint uv)
        {
            if (uv <= 0x7f)
            { //127
                buf[0] = (char)uv;
                return 1;
            }
            if (uv <= 0x7ff)
            {  //2047
                buf[0] = (char)(((uv >> 6) & 0xff) | 0xc0);
                buf[1] = (char)(((uv & 0x3f) | 0x80));
                return 2;
            }
            if (uv <= 0xffff)
            { //65535
                buf[0] = (char)(((uv >> 12) & 0xff) | 0xe0);
                buf[1] = (char)(((uv >> 6) & 0x3f) | 0x80);
                buf[2] = (char)((uv & 0x3f) | 0x80);
                return 3;
            }
            if (uv <= 0x1fffff)
            { //2097151
                buf[0] = (char)(((uv >> 18) & 0xff) | 0xf0);
                buf[1] = (char)(((uv >> 12) & 0x3f) | 0x80);
                buf[2] = (char)(((uv >> 6) & 0x3f) | 0x80);
                buf[3] = (char)((uv & 0x3f) | 0x80);
                return 4;
            }
            if (uv <= 0x3ffffff)
            { //67108863
                buf[0] = (char)(((uv >> 24) & 0xff) | 0xf8);
                buf[1] = (char)(((uv >> 18) & 0x3f) | 0x80);
                buf[2] = (char)(((uv >> 12) & 0x3f) | 0x80);
                buf[3] = (char)(((uv >> 6) & 0x3f) | 0x80);
                buf[4] = (char)((uv & 0x3f) | 0x80);
                return 5;
            }
            if (uv <= 0x7fffffff)
            { //2147483647
                buf[0] = (char)(((uv >> 30) & 0xff) | 0xfc);
                buf[1] = (char)(((uv >> 24) & 0x3f) | 0x80);
                buf[2] = (char)(((uv >> 18) & 0x3f) | 0x80);
                buf[3] = (char)(((uv >> 12) & 0x3f) | 0x80);
                buf[4] = (char)(((uv >> 6) & 0x3f) | 0x80);
                buf[5] = (char)((uv & 0x3f) | 0x80);
                return 6;
            }
            throw new RangeError("pack(U): value out of range").raise(caller);
        }

        internal static long utf8_to_uv(Frame caller, string pString, int p, ref int lenp)
        {
            int c = pString[p++] & 0xff;
            uint uv = (uint)c;
            int n;

            if (!((uv & 0x80) > 0))
            {
                lenp = 1;
                return uv;
            }
            if (!((uv & 0x40) > 0))
            {
                lenp = 1;
                throw new ArgumentError("malformed UTF-8 character").raise(caller);
            }
            if (!((uv & 0x20) > 0)) { n = 2; uv &= 0x1f; }
            else if (!((uv & 0x10) > 0)) { n = 3; uv &= 0x0f; }
            else if (!((uv & 0x08) > 0)) { n = 4; uv &= 0x07; }
            else if (!((uv & 0x04) > 0)) { n = 5; uv &= 0x03; }
            else if (!((uv & 0x02) > 0)) { n = 6; uv &= 0x01; }
            else
            {
                lenp = 1;
                throw new ArgumentError("malformed UTF-8 character").raise(caller);
            }
            if (n > lenp)
            {
                throw new ArgumentError(string.Format("malformed UTF-8 character (expected {0} bytes, given {1} bytes)", n, lenp)).raise(caller);
            }
            lenp = n--;
            if (n != 0)
            {
                while (n-- > 0)
                {
                    c = pString[p++] & 0xff;
                    if ((c & 0xc0) != 0x80)
                    {
                        lenp -= n + 1;
                        throw new ArgumentError("malformed UTF-8 character").raise(caller);
                    }
                    else
                    {
                        c &= 0x3f;
                        uv = uv << 6 | (uint)c;
                    }

                }
            }
            n = lenp - 1;
            if (uv < utf8_limits[n])
            {
                throw new ArgumentError("redundant UTF-8 sequence").raise(caller);
            }

            return uv;
        }

        internal static void rb_str_buf_cat(System.Text.StringBuilder res, byte[] byteArray, int length)
        {
            for (int i = 0; i < length; i++)
            {
                res.Append((char)byteArray[i]);
            }
        }

        internal static void rb_str_buf_cat(System.Text.StringBuilder res, char[] charArray, int length)
        {
            for (int i = 0; i < length; i++)
            {
                res.Append(charArray[i]);
            }
        }

        internal static void swap(byte[] swapArray)
        {
            byte temp;

            switch (swapArray.Length)
            {
                case 2:
                    temp = swapArray[0];
                    swapArray[0] = swapArray[1];
                    swapArray[1] = temp;
                    break;
                case 4:
                    temp = swapArray[0];
                    swapArray[0] = swapArray[3];
                    swapArray[3] = temp;

                    temp = swapArray[1];
                    swapArray[1] = swapArray[2];
                    swapArray[2] = temp;
                    break;
                case 8:
                    temp = swapArray[0];
                    swapArray[0] = swapArray[7];
                    swapArray[7] = temp;

                    temp = swapArray[1];
                    swapArray[1] = swapArray[6];
                    swapArray[6] = temp;

                    temp = swapArray[2];
                    swapArray[2] = swapArray[5];
                    swapArray[5] = temp;

                    temp = swapArray[3];
                    swapArray[3] = swapArray[4];
                    swapArray[4] = temp;
                    break;
                default:
                    break;
            }
        }

        internal static bool isBigEndian()
        {

            return !System.BitConverter.IsLittleEndian;
        }

        internal static uint num2i32(object x, Frame caller)
        {
            x = Integer.rb_to_int(x, caller);

            if (x is int) return (uint)((int)x & 0xFFFFFFFF);
            if (x is Bignum)
            {
                return Bignum.rb_big2ulong_pack((Bignum)x, caller);
            }
            throw new TypeError(string.Format("cannot convert {0} to `integer'", Class.CLASS_OF(x)._name)).raise(caller);
        }

        internal static ulong num2i64(object x, Frame caller)
        {
            x = Integer.rb_to_int(x, caller);

            if (x is int)
            {
                byte[] b = BitConverter.GetBytes((long)(int)x);                
                return BitConverter.ToUInt64(b, 0);
            }             
            if (x is Bignum)
            {
                return Bignum.rb_big2uquad_pack((Bignum)x, caller);
            }
            throw new TypeError(string.Format("cannot convert {0} to `integer'", Class.CLASS_OF(x)._name)).raise(caller);
        }

        internal static object i64_2_num(long x, Frame caller)
        {
            if (x >= int.MinValue && x <= int.MaxValue)
            {
                return (int)x;
            }
            else
            {
                return new Bignum(x);
            }
        }

        internal static object ui64_2_num(ulong x, Frame caller)
        {
            if (x <= int.MaxValue)
            {
                return (int)x;
            }
            else
            {
                return new Bignum(IronMath.integer.make(x));
            }
        }

        internal static int hex2num(char c)
        {

            switch (c)
            {
                case '0':
                    goto case '9';
                case '1':
                    goto case '9';
                case '2':
                    goto case '9';
                case '3':
                    goto case '9';
                case '4':
                    goto case '9';
                case '5':
                    goto case '9';
                case '6':
                    goto case '9';
                case '7':
                    goto case '9';
                case '8':
                    goto case '9';
                case '9':
                    return c - '0';
                case 'a':
                    goto case 'f';
                case 'b':
                    goto case 'f';
                case 'c':
                    goto case 'f';
                case 'd':
                    goto case 'f';
                case 'e':
                    goto case 'f';
                case 'f':
                    return c - 'a' + 10;
                case 'A':
                    goto case 'F';
                case 'B':
                    goto case 'F';
                case 'C':
                    goto case 'F';
                case 'D':
                    goto case 'F';
                case 'E':
                    goto case 'F';
                case 'F':
                    return c - 'A' + 10;
                default:
                    return -1;
            }
        }
    }    
}
