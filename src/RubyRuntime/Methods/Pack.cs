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

    
    internal class pack_pack : MethodBody1  //author: war, status: done
    {
        internal static pack_pack singleton = new pack_pack();

        public override object Call1(Class last_class, object ary, Frame caller, Proc block, object fmt)
        {

            int p, pend, idx;
            int len, items, ptr;
            string fmtString, fromString;
            char type;
            object from;
            int plen;
            System.Text.StringBuilder res;

            fmtString = String.StringValue(fmt, caller);
            p = 0;
            pend = fmtString.Length;
            res = new System.Text.StringBuilder();

            items = ((Array)ary).Count;
            idx = 0;

            while (p < pend)
            {
                if (fmtString.Length != pend)
                {
                    new RuntimeError("format string modified");
                }
                type = fmtString[p++];
                if (char.IsWhiteSpace(type)) //TEST: IsWhiteSpace() must be equivalent to isspace()
                {
                    continue;
                }
                if (type == '#') //eat comments
                {
                    while ((p < pend) && (fmtString[p] != '\n'))
                    {
                        p++;
                    }
                    continue;
                }
                if (p < pend && (fmtString[p] == '_' || fmtString[p] == '!')) //'_' and '!' can only come after "sSiIlL"
                {
                    const string natstr = "sSiIlL";

                    if (natstr.IndexOf(type) != -1)
                    {
                        p++;
                    }
                    else
                    {
                        throw new ArgumentError(string.Format("'{0}' allowed only after types {1}", fmtString[p], natstr)).raise(caller);
                    }
                }
                if (p < pend && fmtString[p] == '*') /* set data length */
                {
                    if ("@Xxu".IndexOf(type) != -1)
                    {
                        len = 0;
                    }
                    else
                    {
                        len = items;
                    }
                    p++;
                }
                else if (p < pend && char.IsDigit(fmtString[p]))
                {
                    int numStart = p;
                    int numEnd = p + 1;
                    while (numEnd < pend)
                    {
                        if (char.IsDigit(fmtString[numEnd]))
                        {
                            numEnd++;
                        }
                        else
                        {
                            break;
                        }
                    }
                    len = int.Parse(fmtString.Substring(numStart, numEnd - numStart));
                }
                else
                {
                    len = 1;
                }

                switch (type)
                {
                    case 'A':
                        goto case 'Z';
                    case 'a':
                        goto case 'Z';
                    case 'B':
                        goto case 'Z';
                    case 'b':
                        goto case 'Z';
                    case 'H':
                        goto case 'Z';
                    case 'h':
                        goto case 'Z';
                    case 'Z':
                        from = Pack.NEXTFROM(caller, ary, ref idx, ref items);


                        if (from == null)
                        {
                            fromString = "";
                            ptr = 0;
                            plen = 0;
                        }
                        else
                        {
                            fromString = String.StringValue(from, caller);
                            ptr = 0;
                            plen = fromString.Length;
                        }

                        if (fmtString[p - 1] == '*')
                        {
                            len = plen;
                        }
                        switch (type)
                        {
                            case 'a':        /* arbitrary binary string (null padded)  */
                                goto case 'Z';
                            case 'A':        /* ASCII string (space padded) */
                                goto case 'Z';
                            case 'Z':        /* null terminated ASCII string  */
                                if (plen >= len)
                                {
                                    res.Append(fromString.Substring(0, len));
                                    if (fmtString[p - 1] == '*' && type == 'Z')
                                    {
                                        res.Append("\0");
                                    }
                                }
                                else
                                {
                                    res.Append(fromString);
                                    len -= plen;
                                    if (type == 'A')
                                    {
                                        res.Append(' ', len);
                                    }
                                    else //type == 'Z' or type == 'a'
                                    {
                                        res.Append('\0', len);
                                    }
                                }
                                break;
                            case 'b':        /* bit string (ascending) */
                                {
                                    int aByte = 0;
                                    int i, j = 0;

                                    if (len > plen)
                                    {
                                        j = (len - plen + 1) / 2;
                                        len = plen;
                                    }
                                    for (i = 0; i++ < len; ptr++)
                                    {
                                        if ((fromString[ptr] & 1) > 0)
                                        {
                                            aByte |= 128;
                                        }
                                        if ((i & 7) > 0)
                                        {
                                            aByte >>= 1;
                                        }
                                        else
                                        {
                                            char c = (char)aByte;
                                            res.Append((char)c);
                                            aByte = 0;
                                        }
                                    }
                                    if ((len & 7) > 0)
                                    {
                                        char c;
                                        aByte >>= 7 - (len & 7);
                                        c = (char)aByte;
                                        res.Append(c);
                                    }
                                    len = j;
                                    res.Append('\0', len); //goto grow;                                     
                                }
                                break;
                            case 'B':        /* bit string (descending) */
                                {
                                    int aByte = 0;
                                    int i, j = 0;

                                    if (len > plen)
                                    {
                                        j = (len - plen + 1) / 2;
                                        len = plen;
                                    }
                                    for (i = 0; i++ < len; ptr++)
                                    {
                                        aByte |= fromString[ptr] & 1;
                                        if ((i & 7) > 0)
                                        {
                                            aByte <<= 1;
                                        }
                                        else
                                        {
                                            char c = (char)aByte;
                                            res.Append(c);
                                            aByte = 0;
                                        }
                                    }
                                    if ((len & 7) > 0)
                                    {
                                        char c;
                                        aByte <<= 7 - (len & 7);
                                        c = (char)aByte;
                                        res.Append(c);
                                    }
                                    len = j;
                                    res.Append('\0', len); //goto grow;     
                                }
                                break;
                            case 'h':        /* hex string (low nibble first) */
                                {
                                    int aByte = 0;
                                    int i, j = 0;


                                    if (len > plen)
                                    {
                                        j = (len - plen + 1) / 2;
                                        len = plen;
                                    }
                                    for (i = 0; i++ < len; ptr++)
                                    {
                                        if (char.IsLetter(fromString[ptr]))
                                        {
                                            aByte |= (((fromString[ptr] & 15) + 9) & 15) << 4;
                                        }
                                        else
                                        {
                                            aByte |= (fromString[ptr] & 15) << 4;
                                        }
                                        if ((i & 1) > 0)
                                        {
                                            aByte >>= 4;
                                        }
                                        else
                                        {
                                            char c = (char)aByte;
                                            res.Append(c);
                                            aByte = 0;
                                        }
                                    }
                                    if ((len & 1) > 0)
                                    {
                                        char c = (char)aByte;
                                        res.Append(c);
                                    }
                                    len = j;
                                    res.Append('\0', len); //goto grow;  
                                }
                                break;
                            case 'H':        /* hex string (high nibble first) */
                                {
                                    int aByte = 0;
                                    int i, j = 0;

                                    if (len > plen)
                                    {
                                        j = (len - plen + 1) / 2;
                                        len = plen;
                                    }
                                    for (i = 0; i++ < len; ptr++)
                                    {
                                        if (char.IsLetter(fromString[ptr]))
                                        {
                                            aByte |= ((fromString[ptr] & 15) + 9) & 15;
                                        }
                                        else
                                        {
                                            aByte |= fromString[ptr] & 15;
                                        }
                                        if ((i & 1) > 0)
                                        {
                                            aByte <<= 4;
                                        }
                                        else
                                        {
                                            char c = (char)aByte;
                                            res.Append(c);
                                            aByte = 0;
                                        }
                                    }
                                    if ((len & 1) > 0)
                                    {
                                        char c = (char)aByte;
                                        res.Append(c);
                                    }
                                    len = j;
                                    res.Append('\0', len);
                                }
                                break;
                        }
                        break;
                    case 'c': /* signed char */
                        goto case 'C';
                    case 'C': /* unsigned char */
                        while (len-- > 0)
                        {
                            char c;

                            from = Pack.NEXTFROM(caller, ary, ref idx, ref items);

                            c = (char)(Pack.num2i32(from, caller) & 0xFF);
                            res.Append(c);
                        }
                        break;
                    case 's':        /* signed short */
                        goto case 'S';
                    case 'S':        /* unsigned short */
                        while (len-- > 0)
                        {
                            ushort s;

                            from = Pack.NEXTFROM(caller, ary, ref idx, ref items);

                            s = (ushort)Pack.num2i32(from, caller);
                            byte[] b = System.BitConverter.GetBytes(s);
                            Pack.rb_str_buf_cat(res, b, 2);
                        }
                        break;
                    case 'i':        /* signed int */
                        goto case 'L';
                    case 'I':        /* unsigned int */
                        goto case 'L';
                    case 'l':        /* signed long */
                        goto case 'L';
                    case 'L':        /* unsigned long */
                        while (len-- > 0)
                        {
                            uint i;

                            from = Pack.NEXTFROM(caller, ary, ref idx, ref items);

                            i = Pack.num2i32(from, caller);
                            byte[] b = System.BitConverter.GetBytes(i);
                            Pack.rb_str_buf_cat(res, b, 4);
                        }
                        break;
                    case 'q':        /* signed quad (64bit) int */
                        goto case 'Q';
                    case 'Q':        /* unsigned quad (64bit) int */
                        while (len-- > 0)
                        {
                            ulong ul;

                            from = Pack.NEXTFROM(caller, ary, ref idx, ref items);
                            ul = Pack.num2i64(from, caller);
                            byte[] b = System.BitConverter.GetBytes(ul);
                            Pack.rb_str_buf_cat(res, b, 8);
                        }
                        break;
                    case 'n':        /* unsigned short (network byte-order)  */
                        while (len-- > 0)
                        {
                            ushort s;

                            from = Pack.NEXTFROM(caller, ary, ref idx, ref items);

                            s = (ushort)Pack.num2i32(from, caller);
                            byte[] b = System.BitConverter.GetBytes(s);
                            if (!Pack.isBigEndian())
                            {
                                Pack.swap(b);
                            }
                            Pack.rb_str_buf_cat(res, b, 2);
                        }
                        break;
                    case 'N':        /* unsigned long (network byte-order) */

                        while (len-- > 0)
                        {
                            uint l;

                            from = Pack.NEXTFROM(caller, ary, ref idx, ref items);

                            l = Pack.num2i32(from, caller);
                            byte[] b = System.BitConverter.GetBytes(l);
                            if (!Pack.isBigEndian())
                            {
                                Pack.swap(b);
                            }
                            Pack.rb_str_buf_cat(res, b, 4);
                        }

                        break;
                    case 'v':        /* unsigned short (VAX byte-order) */
                        while (len-- > 0)
                        {
                            from = Pack.NEXTFROM(caller, ary, ref idx, ref items);

                            ushort s;
                            s = (ushort)Pack.num2i32(from, caller);
                            byte[] b = System.BitConverter.GetBytes(s);
                            if (Pack.isBigEndian())
                            {
                                Pack.swap(b);
                            }
                            Pack.rb_str_buf_cat(res, b, 2);
                        }
                        break;
                    case 'V':        /* unsigned long (VAX byte-order) */
                        while (len-- > 0)
                        {
                            from = Pack.NEXTFROM(caller, ary, ref idx, ref items);

                            uint l;

                            l = Pack.num2i32(from, caller);
                            byte[] s = System.BitConverter.GetBytes(l);
                            if (Pack.isBigEndian())
                            {
                                Pack.swap(s);
                            }
                            Pack.rb_str_buf_cat(res, s, 4);
                        }
                        break;
                    case 'f':        /* single precision float in native format */
                        goto case 'F';
                    case 'F':        /* ditto */
                        while (len-- > 0)
                        {
                            float f;

                            from = Pack.NEXTFROM(caller, ary, ref idx, ref items);
                            f = (float)((Float)Float.rb_Float(from, caller)).value;
                            byte[] s = System.BitConverter.GetBytes(f);
                            Pack.rb_str_buf_cat(res, s, 4);
                        }
                        break;
                    case 'e':        /* single precision float in VAX byte-order */
                        while (len-- > 0)
                        {
                            float f;
                            byte[] ftmp;

                            from = Pack.NEXTFROM(caller, ary, ref idx, ref items);
                            f = (float)((Float)Float.rb_Float(from, caller)).value;
                            ftmp = System.BitConverter.GetBytes(f);
                            if (Pack.isBigEndian())
                            {
                                Pack.swap(ftmp);
                            }
                            Pack.rb_str_buf_cat(res, ftmp, 4);
                        }
                        break;
                    case 'E':        /* double precision float in VAX byte-order */
                        while (len-- > 0)
                        {
                            double d;
                            byte[] dtmp;

                            from = Pack.NEXTFROM(caller, ary, ref idx, ref items);
                            d = ((Float)Float.rb_Float(from, caller)).value;
                            dtmp = System.BitConverter.GetBytes(d);
                            if (Pack.isBigEndian())
                            {
                                Pack.swap(dtmp);
                            }
                            Pack.rb_str_buf_cat(res, dtmp, 8);
                        }
                        break;
                    case 'd':        /* double precision float in native format */
                        goto case 'D';
                    case 'D':        /* ditto */
                        while (len-- > 0)
                        {
                            double d;

                            from = Pack.NEXTFROM(caller, ary, ref idx, ref items);
                            d = ((Float)Float.rb_Float(from, caller)).value;
                            Pack.rb_str_buf_cat(res, System.BitConverter.GetBytes(d), 8);
                        }
                        break;
                    case 'g':       /* single precision float in network byte-order */
                        while (len-- > 0)
                        {
                            float f;
                            byte[] ftmp;

                            from = Pack.NEXTFROM(caller, ary, ref idx, ref items);
                            f = (float)((Float)Float.rb_Float(from, caller)).value;
                            ftmp = System.BitConverter.GetBytes(f);

                            if (!Pack.isBigEndian())
                            {
                                Pack.swap(ftmp);
                            }
                            Pack.rb_str_buf_cat(res, ftmp, 4);
                        }
                        break;

                    case 'G':        /* double precision float in network byte-order */

                        while (len-- > 0)
                        {
                            double d;
                            byte[] dtmp;

                            from = Pack.NEXTFROM(caller, ary, ref idx, ref items);
                            d = ((Float)Float.rb_Float(from, caller)).value;
                            dtmp = System.BitConverter.GetBytes(d);
                            if (!Pack.isBigEndian())
                            {
                                Pack.swap(dtmp);
                            }
                            Pack.rb_str_buf_cat(res, dtmp, 8);

                        }
                        break;
                    case 'x':        /* null byte */
                        //grow:
                        res.Append('\0', len);
                        break;
                    case 'X':        /* back up byte */
                        //shrink:
                        plen = res.Length;
                        if (plen < len)
                        {
                            throw new ArgumentError("X outside of string").raise(caller);
                        }
                        res.Length = plen - len;
                        break;
                    case '@':        /* null fill to absolute position */
                        len -= res.Length;
                        if (len > 0) res.Append('\0', len); //goto grow:
                        len = -len;
                        if (len > 0) goto case 'X';
                        break;
                    case '%':
                        throw new ArgumentError("% is not supported").raise(caller); //FIX when merge.
                    case 'U':        /* Unicode character */
                        while (len-- > 0)
                        {
                            int l;
                            char[] buf = new char[8];
                            int le;

                            from = Pack.NEXTFROM(caller, ary, ref idx, ref items);
                            from = Integer.rb_to_int(from, caller);
                            l = Numeric.rb_num2long(from, caller);
                            if (l < 0)
                            {
                                throw new RangeError("pack(U): value out of range").raise(caller);
                            }
                            le = Pack.uv_to_utf8(caller, buf, (uint)l);
                            Pack.rb_str_buf_cat(res, buf, le);
                        }
                        break;
                    case 'u':        /* uuencoded string */
                        goto case 'm';
                    case 'm':        /* base64 encoded string */
                        string frmString;
                        from = Pack.NEXTFROM(caller, ary, ref idx, ref items);
                        frmString = String.StringValue(from, caller);
                        ptr = 0;
                        plen = frmString.Length;

                        if (len <= 2)
                        {
                            len = 45;
                        }
                        else
                        {
                            len = len / 3 * 3;
                        }

                        while (plen > 0)
                        {
                            int todo;


                            if (plen > len)
                            {
                                todo = len;
                            }
                            else
                            {
                                todo = plen;
                            }
                            Pack.encodes(res, frmString.Substring(ptr), todo, type);
                            plen -= todo;
                            ptr += todo;
                        }
                        break;
                    case 'M': /* quoted-printable encoded string */
                        from = String.ObjectAsString(Pack.NEXTFROM(caller, ary, ref idx, ref items), caller);
                        if (len <= 1)
                        {
                            len = 72;
                        }
                        Pack.qpencodes(res, from, len);
                        break;
                    case 'P':        /* pointer to packed byte string */
                        throw new NotImplementedError("The pack(\"P\") method is not implemented on this platform").raise(caller);
                    case 'p':        /* pointer to string */
                        throw new NotImplementedError("The pack(\"p\") method is not implemented on this platform").raise(caller);
                    case 'w':        /* BER compressed integer  */
                        while (len-- > 0)
                        {
                            uint ul;
                            System.Text.StringBuilder buf = new System.Text.StringBuilder();
                            char c;
                            int bufs, bufe;
                            //char c, *bufs, *bufe;

                            from = Pack.NEXTFROM(caller, ary, ref idx, ref items);
                            if (from is Bignum)
                            {
                                object big128 = new Bignum(128);
                                while (from is Bignum)
                                {
                                    from = Ruby.Methods.rb_big_divmod.singleton.Call1(last_class, from, caller, null, big128);
                                    c = (char)(Numeric.rb_num2long(((Array)from).value[1], caller) | 0x80);
                                    Pack.rb_str_buf_cat(buf, new char[] { c }, 1);
                                    from = ((Array)from).value[0];
                                }
                            }

                            {
                                int l = Numeric.rb_num2long(from, caller);
                                if (l < 0)
                                {
                                    throw new ArgumentError("cannot compress negative numbers").raise(caller);
                                }
                                ul = (uint)l;
                            }

                            while (ul > 0)
                            {
                                c = (char)((ul & 0x7f) | 0x80);
                                Pack.rb_str_buf_cat(buf, new char[] { c }, 1);
                                ul >>= 7;
                            }

                            if (buf.Length > 0)
                            {
                                bufs = 0;
                                bufe = buf.Length - 1;
                                buf[bufs] = (char)(buf[bufs] & 0x7f); /* clear continue bit */
                                while (bufs < bufe) /* reverse */
                                {
                                    c = buf[bufs];
                                    buf[bufs++] = buf[bufe];
                                    buf[bufe--] = c;
                                }
                                Pack.rb_str_buf_cat(res, buf.ToString().ToCharArray(), buf.Length);
                            }
                            else
                            {
                                c = (char)0;
                                Pack.rb_str_buf_cat(res, new char[] { c }, 1);
                            }
                        }
                        break;

                    default:
                        break;
                }
            }

            return new String(res.ToString());
        }


    }

    
    internal class pack_unpack : MethodBody1 //author: war, status: done
    {
        internal static pack_unpack singleton = new pack_unpack();

        //unpack('m')
        private static int first = 1;
        private static int[] b64_xtable = new int[256];

        public override object Call1(Class last_class, object str, Frame caller, Proc block, object fmt)
        {
            string hexdigits = "0123456789abcdef0123456789ABCDEFx";
            string fmtString, strString;
            int s, send; //str
            int p, pend; //fmt
            object ary;
            char type;
            int len;
            bool star = false;
            int tmp;

            fmtString = String.StringValue(fmt, caller);
            strString = String.StringValue(str, caller);
            s = 0;
            send = strString.Length;
            p = 0;
            pend = fmtString.Length;

            ary = new Array();
            while (p < pend)
            {
                type = fmtString[p++];

                if (char.IsWhiteSpace(type)) continue;
                if (type == '#') //eat comments
                {
                    while ((p < pend) && (fmtString[p] != '\n'))
                    {
                        p++;
                    }
                    continue;
                }
                if (p < pend && (fmtString[p] == '_' || fmtString[p] == '!')) //'_' and '!' can only come after "sSiIlL"
                {
                    const string natstr = "sSiIlL";

                    if (natstr.IndexOf(type) != -1)
                    {
                        p++;
                    }
                    else
                    {
                        throw new ArgumentError(string.Format("'{0}' allowed only after types {1}", fmtString[p], natstr)).raise(caller);
                    }
                }
                if (p >= pend)
                {
                    len = 1;
                }
                else if (fmtString[p] == '*')
                {
                    star = true;
                    len = send - s;
                    p++;
                }
                else if (p < pend && char.IsDigit(fmtString[p]))
                {
                    int numStart = p;
                    int numEnd = p + 1;
                    while (numEnd < pend)
                    {
                        if (char.IsDigit(fmtString[numEnd]))
                        {
                            numEnd++;
                        }
                        else
                        {
                            break;
                        }
                    }
                    len = int.Parse(fmtString.Substring(numStart, numEnd - numStart));
                }
                else
                {
                    len = (type != '@') ? 1 : 0;
                }
                int t;
                switch (type)
                {
                    case '%':
                        throw new ArgumentError("% is not supported").raise(caller);
                    case 'A':
                        if (len > send - s)
                        {
                            len = send - s;
                        }
                        int end = len;
                        t = s + len - 1;
                        while (t >= s)
                        {
                            if (strString[t] != ' ' && strString[t] != '\0') break;
                            t--; len--;
                        }
                        ((Array)ary).value.Add(String.infected_str_new(s, len, strString));
                        s += end;
                        break;
                    case 'Z':
                        {
                            t = s;

                            if (len > send - s) len = send - s;
                            while (t < s + len && t <= send && strString[t] != '\0') t++;
                            ((Array)ary).value.Add(String.infected_str_new(s, t - s, strString));
                            if (t < send) t++;
                            s = star ? t : s + len;
                        }
                        break;
                    case 'a':
                        if (len > send - s) len = send - s;
                        ((Array)ary).value.Add(String.infected_str_new(s, len, strString));
                        s += len;
                        break;
                    case 'b':
                        {
                            System.Text.StringBuilder bitStrBuilder = new System.Text.StringBuilder();
                            object bitstr;
                            int bits;
                            int i;

                            if ((p > 0 && fmtString[p - 1] == '*') || len > (send - s) * 8)
                            {
                                len = (send - s) * 8;
                            }
                            bits = 0;
                            for (i = 0; i < len; i++)
                            {
                                if ((i & 7) > 0)
                                {
                                    bits >>= 1;
                                }
                                else
                                {
                                    bits = (byte)strString[s++];
                                }
                                bitStrBuilder.Append(((bits & 1) > 0) ? '1' : '0');
                            }
                            bitstr = new String(bitStrBuilder.ToString());
                            ((Array)ary).value.Add(bitstr);
                        }
                        break;
                    case 'B':
                        {
                            System.Text.StringBuilder bitStrBuilder = new System.Text.StringBuilder();
                            object bitstr;
                            int bits;
                            int i;

                            if ((p > 0 && fmtString[p - 1] == '*') || len > (send - s) * 8)
                            {
                                len = (send - s) * 8;
                            }
                            bits = 0;
                            for (i = 0; i < len; i++)
                            {
                                if ((i & 7) > 0)
                                {
                                    bits <<= 1;
                                }
                                else
                                {
                                    bits = strString[s++];
                                }
                                bitStrBuilder.Append(((bits & 128) > 0) ? '1' : '0');
                            }
                            bitstr = new String(bitStrBuilder.ToString());
                            ((Array)ary).value.Add(bitstr);
                        }
                        break;
                    case 'h':
                        {
                            System.Text.StringBuilder bitStrBuilder = new System.Text.StringBuilder();
                            object bitstr;
                            int bits;
                            int i;

                            if ((p > 0 && fmtString[p - 1] == '*') || len > (send - s) * 8)
                            {
                                len = (send - s) * 2;
                            }
                            bits = 0;

                            for (i = 0; i < len; i++)
                            {
                                if ((i & 1) > 0)
                                {
                                    bits >>= 4;
                                }
                                else
                                {
                                    bits = strString[s++];
                                }
                                bitStrBuilder.Append(hexdigits[bits & 15]);
                            }
                            bitstr = new String(bitStrBuilder.ToString());
                            ((Array)ary).value.Add(bitstr);
                        }
                        break;
                    case 'H':
                        {
                            System.Text.StringBuilder bitStrBuilder = new System.Text.StringBuilder();
                            object bitstr;
                            int bits;
                            int i;

                            if ((p > 0 && fmtString[p - 1] == '*') || len > (send - s) * 8)
                            {
                                len = (send - s) * 2;
                            }
                            bits = 0;
                            for (i = 0; i < len; i++)
                            {
                                if ((i & 1) > 0)
                                {
                                    bits <<= 4;
                                }
                                else
                                {
                                    bits = strString[s++];
                                }
                                bitStrBuilder.Append(hexdigits[(bits >> 4) & 15]);
                            }
                            bitstr = new String(bitStrBuilder.ToString());
                            ((Array)ary).value.Add(bitstr);
                        }
                        break;
                    case 'c':
                        tmp = Pack.PACK_LENGTH_ADJUST(s, send, ref len, star, 1);
                        while (len-- > 0)
                        {
                            int c = (byte)strString[s++];
                            if (c > 127) c -= 256; //integer is signed
                            ((Array)ary).value.Add(c);
                        }
                        tmp = Pack.PACK_ITEM_ADJUST(ary, tmp);
                        break;
                    case 'C':
                        tmp = Pack.PACK_LENGTH_ADJUST(s, send, ref len, star, 1);
                        while (len-- > 0)
                        {
                            int c = (byte)strString[s++];
                            ((Array)ary).value.Add(c);
                        }
                        tmp = Pack.PACK_ITEM_ADJUST(ary, tmp);
                        break;
                    case 's':
                        tmp = Pack.PACK_LENGTH_ADJUST(s, send, ref len, star, 2);
                        while (len-- > 0)
                        {
                            short sTmp;
                            sTmp = System.BitConverter.ToInt16(new byte[] { (byte)strString[s], (byte)strString[s + 1] }, 0);
                            s += 2;
                            ((Array)ary).value.Add((int)sTmp);
                        }
                        tmp = Pack.PACK_ITEM_ADJUST(ary, tmp);
                        break;
                    case 'S':
                        tmp = Pack.PACK_LENGTH_ADJUST(s, send, ref len, star, 2);
                        while (len-- > 0)
                        {
                            ushort sTmp;
                            sTmp = System.BitConverter.ToUInt16(new byte[] { (byte)strString[s], (byte)strString[s + 1] }, 0);
                            s += 2;
                            ((Array)ary).value.Add((int)sTmp);
                        }
                        tmp = Pack.PACK_ITEM_ADJUST(ary, tmp);
                        break;
                    case 'i':
                        tmp = Pack.PACK_LENGTH_ADJUST(s, send, ref len, star, 4);
                        while (len-- > 0)
                        {
                            int sTmp;
                            sTmp = System.BitConverter.ToInt32(new byte[] { (byte)strString[s], (byte)strString[s + 1], 
                                (byte)strString[s + 2], (byte)strString[s + 3]}, 0);
                            s += 4;
                            ((Array)ary).value.Add(sTmp);
                        }
                        tmp = Pack.PACK_ITEM_ADJUST(ary, tmp);
                        break;
                    case 'I':
                        tmp = Pack.PACK_LENGTH_ADJUST(s, send, ref len, star, 4);
                        while (len-- > 0)
                        {
                            uint sTmp;
                            sTmp = System.BitConverter.ToUInt32(new byte[] { (byte)strString[s], (byte)strString[s + 1], 
                                (byte)strString[s + 2], (byte)strString[s + 3]}, 0);
                            s += 4;
                            ((Array)ary).value.Add(Bignum.rb_uint2inum(sTmp, caller));
                        }
                        tmp = Pack.PACK_ITEM_ADJUST(ary, tmp);
                        break;
                    case 'l':
                        goto case 'i';
                    case 'L':
                        goto case 'I';
                    case 'q': //signed quadword 
                        tmp = Pack.PACK_LENGTH_ADJUST(s, send, ref len, star, 8);
                        while (len-- > 0)
                        {
                            long qTmp = System.BitConverter.ToInt64(new byte[]{(byte)strString[s],
                                (byte)strString[s + 1], (byte)strString[s + 2], (byte)strString[s + 3], (byte)strString[s + 4],
                                (byte)strString[s + 5], (byte)strString[s + 6], (byte)strString[s + 7]}, 0);
                            s += 8;
                            ((Array)ary).value.Add(Pack.i64_2_num(qTmp, caller));
                        }
                        tmp = Pack.PACK_ITEM_ADJUST(ary, tmp);
                        break;
                    case 'Q': //unsigned quadword 
                        tmp = Pack.PACK_LENGTH_ADJUST(s, send, ref len, star, 8);
                        while (len-- > 0)
                        {
                            ulong QTmp = System.BitConverter.ToUInt64(new byte[]{(byte)strString[s],
                                (byte)strString[s + 1], (byte)strString[s + 2], (byte)strString[s + 3], (byte)strString[s + 4],
                                (byte)strString[s + 5], (byte)strString[s + 6], (byte)strString[s + 7]}, 0);
                            s += 8;
                            ((Array)ary).value.Add(Pack.ui64_2_num(QTmp, caller));
                        }
                        break;
                    case 'n':
                        tmp = Pack.PACK_LENGTH_ADJUST(s, send, ref len, star, 2);
                        while (len-- > 0)
                        {
                            ushort nTmp = 0;
                            byte[] nTmpArray = new byte[] { (byte)strString[s], (byte)strString[s + 1] };
                            if (!Pack.isBigEndian())
                            {
                                Pack.swap(nTmpArray);
                            }
                            nTmp = System.BitConverter.ToUInt16(nTmpArray, 0);
                            s += 2;
                            ((Array)ary).value.Add((int)nTmp);
                        }
                        tmp = Pack.PACK_ITEM_ADJUST(ary, (int)tmp);
                        break;
                    case 'N':
                        tmp = Pack.PACK_LENGTH_ADJUST(s, send, ref len, star, 4);
                        while (len-- > 0)
                        {
                            uint NTmp = 0;
                            byte[] NTmpArray = new byte[] { (byte)strString[s], (byte)strString[s + 1],
                                (byte)strString[2], (byte)strString[3]};
                            if (!Pack.isBigEndian())
                            {
                                Pack.swap(NTmpArray);
                            }
                            NTmp = System.BitConverter.ToUInt32(NTmpArray, 0);
                            s += 4;
                            ((Array)ary).value.Add(Bignum.rb_uint2inum(NTmp, caller));
                        }
                        tmp = Pack.PACK_ITEM_ADJUST(ary, (int)tmp);
                        break;
                    case 'v':
                        tmp = Pack.PACK_LENGTH_ADJUST(s, send, ref len, star, 2);
                        while (len-- > 0)
                        {
                            ushort vTmp = 0;
                            byte[] vTmpArray = new byte[] { (byte)strString[s], (byte)strString[s + 1] };
                            if (Pack.isBigEndian())
                            {
                                Pack.swap(vTmpArray);
                            }
                            vTmp = System.BitConverter.ToUInt16(vTmpArray, 0);
                            s += 2;
                            ((Array)ary).value.Add((int)vTmp);
                        }
                        tmp = Pack.PACK_ITEM_ADJUST(ary, (int)tmp);
                        break;
                    case 'V':
                        tmp = Pack.PACK_LENGTH_ADJUST(s, send, ref len, star, 4);
                        while (len-- > 0)
                        {
                            uint VTmp = 0;
                            byte[] VTmpArray = new byte[] { (byte)strString[s], (byte)strString[s + 1], 
                                (byte)strString[s + 2], (byte)strString[s + 3]};
                            if (Pack.isBigEndian())
                            {
                                Pack.swap(VTmpArray);
                            }

                            VTmp = System.BitConverter.ToUInt32(VTmpArray, 0);
                            s += 4;
                            ((Array)ary).value.Add(Bignum.rb_uint2inum(VTmp, caller));
                        }
                        tmp = Pack.PACK_ITEM_ADJUST(ary, (int)tmp);
                        break;
                    case 'f':
                        goto case 'F';
                    case 'F':
                        tmp = Pack.PACK_LENGTH_ADJUST(s, send, ref len, star, 4);
                        while (len-- > 0)
                        {
                            float FTmp;
                            FTmp = System.BitConverter.ToSingle(new byte[] { (byte)strString[s], (byte)strString[s + 1], 
                                (byte)strString[s + 2], (byte)strString[s + 3] }, 0);
                            s += 4;
                            ((Array)ary).value.Add(new Float(FTmp)); 
                        }
                        tmp = Pack.PACK_ITEM_ADJUST(ary, (int)tmp);
                        break;
                    case 'e':
                        tmp = Pack.PACK_LENGTH_ADJUST(s, send, ref len, star, 4);
                        while (len-- > 0)
                        {
                            float eTmp;
                            byte[] eTmpArray = new byte[]{ (byte)strString[s], (byte)strString[s + 1],
                                (byte)strString[s + 2], (byte)strString[s + 3]};
                            if (Pack.isBigEndian())
                            {
                                Pack.swap(eTmpArray);
                            }
                            s += 4;
                            eTmp = System.BitConverter.ToSingle(eTmpArray, 0);
                            ((Array)ary).value.Add(new Float(eTmp));
                        }
                        tmp = Pack.PACK_ITEM_ADJUST(ary, (int)tmp);
                        break;
                    case 'E':
                        tmp = Pack.PACK_LENGTH_ADJUST(s, send, ref len, star, 8);
                        while (len-- > 0)
                        {
                            double ETmp;
                            byte[] ETmpArray = new byte[]{(byte)strString[s],
                                (byte)strString[s + 1], (byte)strString[s + 2], (byte)strString[s + 3], (byte)strString[s + 4],
                                (byte)strString[s + 5], (byte)strString[s + 6], (byte)strString[s + 7]};
                            if (Pack.isBigEndian())
                            {
                                Pack.swap(ETmpArray);
                            }
                            s += 8;
                            ETmp = System.BitConverter.ToDouble(ETmpArray, 0);
                            ((Array)ary).value.Add(new Float(ETmp));
                        }
                        tmp = Pack.PACK_ITEM_ADJUST(ary, (int)tmp);
                        break;
                    case 'D':
                        goto case 'd';
                    case 'd':
                        tmp = Pack.PACK_LENGTH_ADJUST(s, send, ref len, star, 8);
                        while (len-- > 0)
                        {
                            double dTmp;
                            byte[] dTmpArray = new byte[]{(byte)strString[s],
                                (byte)strString[s + 1], (byte)strString[s + 2], (byte)strString[s + 3], (byte)strString[s + 4],
                                (byte)strString[s + 5], (byte)strString[s + 6], (byte)strString[s + 7]};
                            s += 8;
                            dTmp = System.BitConverter.ToDouble(dTmpArray, 0);
                            ((Array)ary).value.Add(new Float(dTmp));
                        }
                        tmp = Pack.PACK_ITEM_ADJUST(ary, (int)tmp);
                        break;
                    case 'g':
                        tmp = Pack.PACK_LENGTH_ADJUST(s, send, ref len, star, 4);
                        while (len-- > 0)
                        {
                            float gTmp;
                            byte[] gTmpArray = new byte[]{ (byte)strString[s], (byte)strString[s + 1],
                                (byte)strString[s + 2], (byte)strString[s + 3]};
                            if (!Pack.isBigEndian())
                            {
                                Pack.swap(gTmpArray);
                            }
                            s += 4;
                            gTmp = System.BitConverter.ToSingle(gTmpArray, 0);
                            ((Array)ary).value.Add(new Float(gTmp));
                        }
                        tmp = Pack.PACK_ITEM_ADJUST(ary, (int)tmp);
                        break;
                    case 'G':
                        {
                            tmp = Pack.PACK_LENGTH_ADJUST(s, send, ref len, star, 8);
                            while (len-- > 0)
                            {
                                double GTmp;
                                byte[] GTmpArray = new byte[]{(byte)strString[s],
                                (byte)strString[s + 1], (byte)strString[s + 2], (byte)strString[s + 3], (byte)strString[s + 4],
                                (byte)strString[s + 5], (byte)strString[s + 6], (byte)strString[s + 7]};
                                if (!Pack.isBigEndian())
                                {
                                    Pack.swap(GTmpArray);
                                }
                                s += 8;
                                GTmp = System.BitConverter.ToDouble(GTmpArray, 0);
                                ((Array)ary).value.Add(new Float(GTmp));
                            }
                            tmp = Pack.PACK_ITEM_ADJUST(ary, (int)tmp);
                            break;
                        }
                    case 'U': 
                        if (len > send - s) len = send - s;
                        while (len > 0 && s < send)
                        {
                            int alen = send - s;
                            uint l;

                            l = (uint)Pack.utf8_to_uv(caller, strString,  s, ref alen);
                            s += alen; len--;
                            ((Array)ary).value.Add(Bignum.rb_uint2inum(l, caller));                           
                        }
                        break;
                    case 'u':
                        {


                            object buf = String.infected_str_new(0, (send - s) * 3/4, strString);
                            string ptrString = ((String)buf).value;
                            System.Text.StringBuilder ptrStringBuilder = new System.Text.StringBuilder(ptrString);
                            int ptr = 0;
                            int total = 0;

                            while (s < send && strString[s] > ' ' && strString[s] < 'a')
                            {

                                int a, b, c, d;
                                char[] hunk = new char[4];
                                
                                hunk[3] = '\0';

                                len = (strString[s++] - ' ') & 0x3f;
                                total += len;
                                if (total > ((String)buf).value.Length)
                                {
                                    len -= total - ((String)buf).value.Length;
                                    total = ((String)buf).value.Length;
                                }
                                while (len > 0)
                                {
                                    int mlen = len > 3 ? 3 : len;

                                    if (s < send && strString[s] >= ' ')
                                        a = (strString[s++] - ' ') & 0x3f;
                                    else
                                        a = 0;
                                    if (s < send && strString[s] >= ' ')
                                        b = (strString[s++] - ' ') & 0x3f;
                                    else
                                        b = 0;
                                    if (s < send && strString[s] >= ' ')
                                        c = (strString[s++] - ' ') & 0x3f;
                                    else
                                        c = 0;
                                    if (s < send && strString[s] >= ' ')
                                        d = (strString[s++] - ' ') & 0x3f;
                                    else
                                        d = 0;
                                    hunk[0] = (char)(byte)(a << 2 | b >> 4);
                                    hunk[1] = (char)(byte)(b << 4 | c >> 2);
                                    hunk[2] = (char)(byte)(c << 6 | d);
                                    for(int j = 0; j < mlen; j++){
                                        ptrStringBuilder[ptr + j] = hunk[j];
                                    }
                                    ptr += mlen;
                                    len -= mlen;                                    
                                }
                                if (s < send && strString[s] == '\r') s++;
                                if (s < send && strString[s] == '\n') s++;
                                else if(s < send && (s + 1 == send || strString[s + 1] == '\n')){
                                    s += 2; /* possible checksum byte */
                                }
                            }

                            ptrStringBuilder.Length = total;
                            ((String)buf).value = ptrStringBuilder.ToString();
                            ((Array)ary).value.Add(buf);
                        }
                        break;
                    case 'm':
                        {
                            object buf = String.infected_str_new(0, (send - s) * 3 / 4, strString);
                            System.Text.StringBuilder ptrString = new System.Text.StringBuilder(((String)buf).value);
                            int ptr = 0;
                            int a = -1, b = -1, c = 0, d = 0;

                            if (first > 0)
                            {
                                int i;
                                first = 0;

                                for (i = 0; i < 256; i++)
                                {
                                    b64_xtable[i] = -1;
                                }
                                for (i = 0; i < 64; i++)
                                {
                                    b64_xtable[(int)Pack.b64_table[i]] = i;
                                }
                            }
                            while (s < send)
                            {
                                while (strString[s] == '\r' || strString[s] == '\n') { s++; }
                                if ((a = b64_xtable[(int)strString[s]]) == -1) break;
                                if (!(s+1 < send) || (b = b64_xtable[(int)strString[s + 1]]) == -1) break;
                                if (!(s+2 < send) || (c = b64_xtable[(int)strString[s + 2]]) == -1) break;
                                if (!(s+3 < send) || (d = b64_xtable[(int)strString[s + 3]]) == -1) break;
                                ptrString[ptr++] = (char)(byte)(a << 2 | b >> 4);
                                ptrString[ptr++] = (char)(byte)(b << 4 | c >> 2);
                                ptrString[ptr++] = (char)(byte)(c << 6 | d);
                                s += 4;
                            }
                            if (a != -1 && b != -1)
                            {
                                if (s + 2 < send && strString[ptr++] == '=')
                                    ptrString[ptr++] = (char)(byte)(a << 2 | b >> 4);
                                if (c != -1 && s + 3 < send && strString[s + 3] == '=')
                                {
                                    ptrString[ptr++] = (char)(byte)(a << 2 | b >> 4);
                                    ptrString[ptr++] = (char)(byte)(b << 4 | c >> 2);
                                }
                            }
                            ptrString.Length = ptr;
                            ((String)buf).value = ptrString.ToString();
                            ((Array)ary).value.Add(buf);
                        }
                        break;
                    case 'M':
                        {
                            object buf = String.infected_str_new(0, send - s, strString);
                            System.Text.StringBuilder ptrString = new System.Text.StringBuilder(((String)buf).value);
                            int ptr = 0;
                            int c1, c2;

                            while(s < send){
                                if (strString[s] == '=')
                                {
                                    if (++s == send) break;
                                    if (strString[s] != '\n')
                                    {
                                        if ((c1 = Pack.hex2num(strString[s])) == -1) break;
                                        if (++s == send) break;
                                        if ((c2 = Pack.hex2num(strString[s])) == -1) break;
                                        ptrString[ptr++] = (char)(byte)(c1 << 4 | c2);
                                    }
                                }
                                else
                                {
                                    ptrString[ptr++] = strString[s];
                                }
                                s++;
                            }
                            ptrString.Length = ptr;
                            ((String)buf).value = ptrString.ToString();
                            ((Array)ary).value.Add(buf);
                        }
                        break;
                    case 'X':
                        if (len > s)
                            throw new ArgumentError("X outside of string").raise(caller);
                        s -= len;
                        break;
                    case 'x':
                        if (len > send - s)
                            throw new ArgumentError("X outside of string").raise(caller);
                        s += len;
                        break;
                    case 'P':        /* pointer to packed byte string */
                        throw new NotImplementedError("The pack(\"P\") method is not implemented on this platform").raise(caller);
                    case 'p':        /* pointer to string */
                        throw new NotImplementedError("The pack(\"p\") method is not implemented on this platform").raise(caller);
                    case 'w':
                        {
                            uint ul = 0;
                            uint ulmask = ((uint)0xfe) << 4 - 1 * 8;

                            while (len > 0 && s < send)
                            {
                                ul <<= 7;
                                ul |= (byte)(strString[s] & 0x7f);

                                if (!((strString[s++] & 0x80) > 0))
                                {
                                    ((Array)ary).value.Add(Bignum.rb_uint2inum(ul, caller));
                                    len--;
                                    ul = 0;
                                }
                                else if ((ul & ulmask) > 0) {
                                    object big = new Bignum(ul);
                                    object big128 = new Bignum(128);
                                    while (s < send)
                                    {
                                        big = Ruby.Methods.rb_big_mul.singleton.Call1(last_class, big, caller, null, big128);
                                        big = Ruby.Methods.rb_big_plus.singleton.Call1(last_class, big, caller, null, Bignum.rb_uint2inum((byte)(strString[s] & 0x7f), caller));
                                    }
                                    if (!((strString[s++] & 0x80) > 0))
                                    {
                                        ((Array)ary).value.Add(big);
                                        len--;
                                        ul = 0;
                                        break;
                                    }
                                }
                            }
                        }
                        break;
                    default:
                        break;
                }
            }
            return ary;
        }
    }
}
