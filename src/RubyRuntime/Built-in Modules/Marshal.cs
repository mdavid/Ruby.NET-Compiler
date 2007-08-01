/**********************************************************************

  Ruby.NET Runtime Library
  Originally developed at Queensland University of Technology
 
  Some sections of this C# code mirror the structure of the C code in the
  Ruby 1.8.2 Interpreter Copyright (C) 1993-2003 Yukihiro Matsumoto, et.al.
  
**********************************************************************/

using Ruby.Runtime;


namespace Ruby
{

    using HashKeyValuePair = System.Collections.Generic.KeyValuePair<Dictionary.Key, object>;

    public partial class Marshal //author: war
    {

        //from stdio.h
        internal static int BUFSIZ = 512;

        internal static int FLOAT_DIG = Numeric.DBL_DIG + 2;
        internal static int MANT_BITS = 32;
        internal static int DECIMAL_MANT = (53 - 16);    /* from IEEE754 double precision */
        internal static int BITSPERSHORT = 16;
        internal static int SIZEOF_BDIGITS = 4;
        internal static int SIZEOF_SHORT = 2;
        internal static int SIZEOF_LONG = 4; //long is 4 bytes in length in ruby
        internal static int SHORTMASK = 0xFFFF;

        internal const int MARSHAL_MAJOR = 4;
        internal const int MARSHAL_MINOR = 8;

        internal const char TYPE_NIL = '0';
        internal const char TYPE_TRUE = 'T';
        internal const char TYPE_FALSE = 'F';
        internal const char TYPE_FIXNUM = 'i';
        internal const char TYPE_IVAR = 'I';
        internal const char TYPE_LINK = '@';

        internal const char TYPE_EXTENDED = 'e';
        internal const char TYPE_UCLASS = 'C';
        internal const char TYPE_OBJECT = 'o';
        internal const char TYPE_DATA = 'd';
        internal const char TYPE_USERDEF = 'u';
        internal const char TYPE_USRMARSHAL = 'U';
        internal const char TYPE_FLOAT = 'f';
        internal const char TYPE_BIGNUM = 'l';
        internal const char TYPE_STRING = '"';
        internal const char TYPE_REGEXP = '/';
        internal const char TYPE_ARRAY = '[';
        internal const char TYPE_HASH = '{';
        internal const char TYPE_HASH_DEF = '}';
        internal const char TYPE_STRUCT = 'S';
        internal const char TYPE_MODULE_OLD = 'M';
        internal const char TYPE_CLASS = 'c';
        internal const char TYPE_MODULE = 'm';
        internal const char TYPE_SYMBOL = ':';
        internal const char TYPE_SYMLINK = ';';

        internal const string s_dump = "_dump";
        internal const string s_load = "_load";
        internal const string s_mdump = "marshal_dump";
        internal const string s_mload = "marshal_load";
        internal const string s_dump_data = "_dump_data";
        internal const string s_load_data = "_load_data";
        internal const string s_alloc = "_alloc";
        internal const string s_getc = "getc";
        internal const string s_read = "read";
        internal const string s_write = "write";
        internal const string s_binmode = "binmode";

        internal static bool type_data_warn = false;


        //----------------------------------------------------------------------------------
        // Dump
        //----------------------------------------------------------------------------------

        internal class dump_arg
        {
            internal object obj;
            internal object str;
            internal object dest;
            internal System.Collections.Generic.Dictionary<int, int> symbols;
            internal System.Collections.Generic.Dictionary<object, int> data;
            internal bool taint;

            internal dump_arg()
            {
                obj = null;
                str = null;
                dest = null;
                symbols = new System.Collections.Generic.Dictionary<int, int>();
                data = new System.Collections.Generic.Dictionary<object, int>();
                taint = false;
            }
        }

        internal class dump_call_arg
        {
            internal object obj;
            internal dump_arg arg;
            internal int limit;

            internal dump_call_arg()
            {
                obj = null;
                arg = null;
                limit = 0;
            }
        }

        //----------------------------------------------------------------------------------

        //from io.c
        internal static object rb_io_write(object io, object str, Frame caller)
        {
            return Ruby.Methods.io_write.singleton.Call1(null, io, caller, null, str);
        }

        internal static int save_mantissa(double d, char[] buf)
        {
            int e = 0;
            int i = 0;
            uint m;
            double n;


            double fabsd = System.Math.Abs(d);
            double frexpd = frexp(fabsd, out e);
            double ldexpd = ldexp(frexpd, DECIMAL_MANT);
            d = modf(ldexpd, out d);
            if (d > 0)
            {
                buf[i++] = (char)'\0';
                do
                {
                    d = modf(ldexp(d, MANT_BITS), out n);
                    m = (uint)n;
                    buf[i++] = (char)(byte)(m >> 24);
                    buf[i++] = (char)(byte)(m >> 16);
                    buf[i++] = (char)(byte)(m >> 8);
                    buf[i++] = (char)(byte)m;
                } while (d > 0);
                while (!(buf[i - 1] > 0)) --i;
            }
            return i;
        }

        internal static double ldexp(double mantissa, int exp)
        {
            return mantissa * System.Math.Pow(2, exp);
        }


        internal static double frexp(double d, out int exp)
        {
            exp = 0;
            if (d == 0) return d;

            if (d < 0) return -frexp(-d, out exp);

            while (d > 1)
            {
                d = d / 2;
                exp++;
            }

            while (d < 0.5)
            {
                d *= 2;
                exp--;
            }

            return d;
        }

        internal static double modf(double d, out double integerPortion)
        {
            //TEST: test that negatives are returned in integerPortion and exponent. 
            integerPortion = System.Math.Truncate(d);
            return d - integerPortion;
        }

        ////variable.c
        ////MOVE: this doesn't belong here, move to variable.cs
        internal static string rb_class_name(object klass, Frame caller)
        {
            return rb_class_path(((Class)klass).class_real(), caller);
        }

        ////variable.c
        ////MOVE: this doesn't belong here, move to variable.cs
        internal static string rb_class2name(object klass, Frame caller)
        {
            return rb_class_name(klass, caller);
        }

        ////variable.c
        ////MOVE: this doesn't belong here, move to variable.cs
        internal static string tmp_classpath = "__tmp_classpath__";
        internal static string rb_class_path(object klass, Frame caller)
        {
            string path = ((Class)klass).classname();

            if (path != null)
            {
                return path;
            }
            object p;
            if (((Class)klass).instance_vars.TryGetValue(tmp_classpath, out p))
            {
                return ((String)p).value;
            }
            else
            {
                string s = "Class";

                if (((Class)klass)._type == Class.Type.Module)
                {
                    if (((Class)Ruby.Methods.rb_obj_class.singleton.Call0(null, klass, caller, null)) == Ruby.Runtime.Init.rb_cModule)
                    {
                        s = "Module";
                    }
                    else
                    {
                        s = rb_class2name(((Basic)klass).my_class, caller);
                    }
                }
                //TEST: The .Net and the CRuby statements are different. CRuby puts the memory location
                //of the pointer into a string -- is this memory location read latter, if so we need to save the klass
                //object in a collection for latter retrieval. 
                path = string.Format("#<{0}:0x{1:x}>", s, klass.GetHashCode());
                //        sprintf(RSTRING(path)->ptr, "#<%s:0x%lx>", s, klass);
                ((Class)klass).instance_variable_set(tmp_classpath, new String(path));

                return path;
            }
        }

        internal static object class2path(object klass, Frame caller)
        {

            object path = new String(rb_class_path(klass, caller));

            string n = ((String)path).value;

            if (n[0] == '#')
            {
                string type;

                if (((Class)klass)._type == Class.Type.Class)
                {
                    type = "class";
                }
                else
                {
                    type = "module";
                }

                throw new TypeError(string.Format("can't dump anonymous {0} {1}", type, n)).raise(caller);
            }

            return path;
        }

        //MOVE: move to variable.c
        internal static string rb_obj_classname(object obj, Frame caller)
        {
            return rb_class2name(Class.CLASS_OF(obj), caller);          
        }       

        //----------------------------------------------------------------------------------

        internal static object dump(dump_call_arg arg, Frame caller)
        {
            w_object(arg.obj, arg.arg, arg.limit, caller);
            if (arg.arg.dest != null)
            {
                rb_io_write(arg.arg.dest, arg.arg.str, caller);
                ((String)arg.arg.str).value = "";
            }

            return null; 
        }

        internal static object dump_ensure(dump_arg arg)
        {
            arg.symbols = null;
            arg.data = null; 
            if (arg.taint)
            {
                ((Basic)arg.str).Tainted = true;
            }
            return null;
        }

        //----------------------------------------------------------------------------------

        internal static void w_byte(char c, dump_arg arg, Frame caller)
        {
            w_nbyte(c.ToString(), 1, arg, caller);
        }

        internal static void w_nbyte(string s, int n, dump_arg arg, Frame caller)
        {
            object buf = arg.str;
            ((String)buf).value = ((String)buf).value + s.Substring(0, n);
            if (arg.dest != null && ((String)buf).value.Length >= BUFSIZ)
            {
                if (arg.taint)
                {
                    ((Basic)buf).Tainted = true;
                }
                rb_io_write(arg.dest, buf, caller);
                ((String)buf).value = "";
            }
        }


        internal static void w_bytes(string s, int n, dump_arg arg, Frame caller)
        {
            w_long(n, arg, caller);
            w_nbyte(s, n, arg, caller);
        }

        internal static void w_short(int x, dump_arg arg, Frame caller)
        {
            w_byte((char)((x >> 0) & 0xff), arg, caller);
            w_byte((char)((x >> 8) & 0xff), arg, caller);
        }

        internal static void w_long(int x, dump_arg arg, Frame caller)
        {
            int sizeOfInt = 4;
            char[] buf = new char[sizeOfInt + 1];
            int i = 0;
            int len = 0;

            if (x == 0)
            {
                w_byte((char)0, arg, caller);
                return;
            }
            if (0 < x && x < 123)
            {
                w_byte((char)(x + 5), arg, caller);
                return;
            }
            if (-124 < x && x < 0)
            {
                w_byte((char)((x - 5) & 0xff), arg, caller);
                return;
            }
            for (i = 1; i < sizeOfInt + 1; i++)
            {
                buf[i] = (char)(x & 0xff);
                x = x >> 8;
                if (x == 0)
                {
                    buf[0] = (char)(byte)i;
                    break;
                }

                if (x == 0)
                {
                    buf[0] = (char)(byte)i;
                    break;
                }
                if (x == -1)
                {
                    buf[0] = (char)(byte)-i;
                    break;
                }
            }
            len = i;
            for (i = 0; i <= len; i++)
            {
                w_byte(buf[i], arg, caller);
            }
        }

        internal static void w_float(double d, dump_arg arg, Frame caller)
        {
            System.Text.StringBuilder buf = new System.Text.StringBuilder();
            if (double.IsInfinity(d))
            {
                if (double.IsNegativeInfinity(d))
                {
                    buf.Append("-inf");
                }
                else
                {
                    buf.Append("inf");
                }
            }
            else if (double.IsNaN(d))
            {
                buf.Append("nan");
            }
            else if (d == 0.00)
            {
                if (1.0 / d < 0)//TEST: will this behave correctly in C#???
                {
                    buf.Append("-0");
                }
                else
                {
                    buf.Append("0");
                }
            }
            else
            {
                int len;
                buf.Append(d.ToString("r"));
                len = buf.Length;
                char[] bufArray = new char[20];
                int manLength = save_mantissa(d, bufArray);
                len += manLength;
                buf.Append(new System.String(bufArray, 0, manLength));
                w_bytes(buf.ToString(), len, arg, caller);
                return;
            }
            string bufString = buf.ToString();
            w_bytes(bufString, bufString.Length, arg, caller);
        }

        internal static void w_symbol(int id, dump_arg arg, Frame caller)
        {
            string sym = Symbol.rb_id2name((uint) id);
            int num;

            if (arg.symbols.ContainsKey(id))
            {
                num = arg.symbols[id];
                w_byte(TYPE_SYMLINK, arg, caller);
                w_long(num, arg, caller);
            }
            else
            {
                w_byte(TYPE_SYMBOL, arg, caller);
                w_bytes(sym, sym.Length, arg, caller);
                arg.symbols.Add(id, arg.symbols.Count);
            }
        }

        internal static void w_unique(string s, dump_arg arg, Frame caller)
        {
            if (s.Length > 0 && s[0] == '#')
            {
                throw new TypeError(string.Format("can't dump anonymous class {0}", s)).raise(caller);
            }
            w_symbol((int)Symbol.rb_intern(s), arg, caller);
        }

        internal static void w_uclass(object obj, Class base_klass, dump_arg arg, Frame caller)
        {
            Class klass = Class.CLASS_OF(obj);

            w_extended(klass, arg, true, caller);
            klass = klass.class_real();
            if (klass != base_klass)
            {
                w_byte((char)TYPE_UCLASS, arg, caller);
                w_unique(klass.classname(), arg, caller);
            }
        }

        internal static void w_extended(Class klass, dump_arg arg, bool check, Frame caller)
        {
            string path; 

            if (klass._type == Class.Type.Singleton)
            {
                if (check && klass._methods.Count > 0 ||
                    (klass.instance_vars.Count > 0))
                {
                    throw new TypeError("singleton can't be dumped").raise(caller);
                }
                klass = klass.super;
            }
            while (klass._type == Class.Type.IClass) //TEST:
            {
                path = rb_class2name(((Basic)klass).my_class, caller);
                w_byte(TYPE_EXTENDED, arg, caller);
                w_unique(path, arg, caller);
                klass = ((Class)klass).super;
            }
        }

        internal static void w_class(char type, object obj, dump_arg arg, bool check, Frame caller)
        {
            string path;

            object klass = Class.CLASS_OF(obj);
            w_extended((Class)klass, arg, check, caller);
            w_byte(type, arg, caller);
            path = ((String)class2path(((Class)klass).class_real(), caller)).value;
            w_unique(path, arg, caller);
        }

        internal static int shortlen(long len, uint[] ds)
        {
            uint num;
            int offset = 0;

            num = ds[len - 1];
            while (num > 0)
            {
                num = num >> BITSPERSHORT;
                offset++;
            }
            return (int)((len - 1) * SIZEOF_BDIGITS / 2 + offset);
        }

        internal static void w_ivar(System.Collections.Generic.Dictionary<string, object> tbl, dump_call_arg arg, Frame caller)
        {
            if (tbl != null && tbl.Count > 0)
            {
                w_long(tbl.Count, arg.arg, caller);
                foreach (System.Collections.Generic.KeyValuePair<string, object> IDValuePair in tbl)
                {
                    w_symbol((int)Symbol.rb_intern(IDValuePair.Key), arg.arg, caller);
                    w_object(IDValuePair.Value, arg.arg, arg.limit, caller);
                }
            }
            else
            {
                w_long(0, arg.arg, caller);
            }                     
        }
        
        //from struct.c
        //MOVE: Move this code to struct.cs
        internal static object rb_struct_members(object s, Frame caller)
        {
            object sClass = (Class)Ruby.Methods.rb_obj_class.singleton.Call0(null, s, caller, null);
            object members = Ruby.Methods.rb_struct_s_members.singleton.Call0(null, sClass, caller, null);

            if (StructLen(s, caller) != ((Array)members).Count)
            {
                throw new TypeError(string.Format("struct size differs ({0} required {1} given)", StructLen(s, caller), ((Array)members).Count)).raise(caller);
            }

            return members;
        }

        internal static void w_object(object obj, dump_arg arg, int limit, Frame caller)
        {
            dump_call_arg c_arg = new dump_call_arg();
            System.Collections.Generic.Dictionary<string, object> ivtbl = null; 
            //    st_data_t num;

            if (limit == 0)
            {
                throw new ArgumentError("exceed depth limit").raise(caller);
            }

            limit--;
            c_arg.limit = limit;
            c_arg.arg = arg;

            if (obj != null && arg.data.ContainsKey(obj))
            {
                int num = arg.data[obj];
                w_byte(TYPE_LINK, arg, caller);
                w_long(num, arg, caller);
                return;
            }
                               
            ivtbl = Object.get_generic_ivars(obj);
            if (ivtbl != null)
            {
                w_byte(TYPE_IVAR, arg, caller);
            }

            if (obj == null)
            {
                w_byte(TYPE_NIL, arg, caller);
            }
            else if (obj is bool && (bool)obj == true)
            {
                w_byte(TYPE_TRUE, arg, caller);
            }
            else if (obj is bool && (bool)obj == false)
            {
                w_byte(TYPE_FALSE, arg, caller);
            }
            else if (obj is int)
            {
                w_byte((char)TYPE_FIXNUM, arg, caller);
                w_long((int)obj, arg, caller);
            }
            else if (obj is Symbol)
            {
                //FIXME: This code isn't being reached at the moment.
                //should be ok since the latest fixes - haven't tested this though
                w_symbol((int)((Symbol)obj).id_new, arg, caller);
            }
            else
            {
                if (((Basic)obj).Tainted)
                {
                    arg.taint = true;
                }

                arg.data[obj] = arg.data.Count;

                if (Eval.RespondTo(obj, s_mdump))
                {
                    object v;

                    v = Eval.CallPublic0(obj, caller, s_mdump, null);
                    w_class(TYPE_USRMARSHAL, obj, arg, false, caller);
                    w_object(v, arg, limit, caller);
                    if (ivtbl != null) w_ivar(null, c_arg, caller);
                    return;
                }
                if (Eval.RespondTo(obj, s_dump)) //TEST:
                {
                    object v;

                    v = Eval.CallPrivate(obj, caller, s_dump, null, limit);
                    if (!(v is String))
                    {
                        throw new TypeError("_dump() must return string").raise(caller);
                    }

                    if (ivtbl == null && (ivtbl = Object.get_generic_ivars(v)) != null)
                    {
                        w_byte(TYPE_IVAR, arg, caller);
                    }
                    w_class(TYPE_USERDEF, obj, arg, false, caller);
                    w_bytes(((String)v).value, ((String)v).value.Length, arg, caller);
                    if (ivtbl != null)
                    {
                        w_ivar(ivtbl, c_arg, caller);
                    }
                    return;              
                }


                if (obj is Class)
                {
                    if (((Class)obj)._type == Class.Type.Class)
                    {
                        if (((Class)obj)._type == Class.Type.Singleton)
                        {
                            throw new TypeError("singleton class can't be dumped").raise(caller);
                        }
                        w_byte(TYPE_CLASS, arg, caller);
                        {
                            object path = class2path(obj, caller);
                            w_bytes(((String)path).value, ((String)path).value.Length, arg, caller);
                        }
                    }
                    else if (((Class)obj)._type == Class.Type.Module)//TEST:
                    {
                        w_byte(TYPE_MODULE, arg, caller);
                        {
                            object path = class2path(obj, caller);
                            w_bytes(((String)path).value, ((String)path).value.Length, arg, caller);
                        }
                    }
                }
                else if (obj is Float)
                {
                    w_byte((char)TYPE_FLOAT, arg, caller);
                    w_float(((Float)obj).value, arg, caller);
                }
                else if (obj is Bignum)
                {
                    w_byte((char)TYPE_BIGNUM, arg, caller);
                    {
                        char sign = ((Bignum)(obj)).value.Sign < 0 ? '-' : '+';
                        long len = ((Bignum)(obj)).value.length;
                        uint[] d = ((Bignum)(obj)).value.Data;

                        w_byte((char)sign, arg, caller);
                        w_long(shortlen(len, d), arg, caller); /* w_short? */
                        int dIndex = 0;
                        while (len-- > 0)
                        {
                            uint num = d[dIndex];
                            int i;

                            for (i = 0; i < SIZEOF_BDIGITS; i += SIZEOF_SHORT)
                            {
                                w_short((int)num & SHORTMASK, arg, caller);
                                num = num >> BITSPERSHORT;
                                if (len == 0 && num == 0) break;
                            }
                            dIndex++;
                        }
                    }
                }
                else if (obj is Struct)
                {
                    w_class(TYPE_STRUCT, obj, arg, true, caller);
                    {
                        int len;
                        object mem;

                        len = StructLen(obj, caller);
                        mem = rb_struct_members(obj, caller);

                        w_long(len, arg, caller);
                        for (int i = 0; i < len; i++)
                        {
                            string symbolString = (string)((Array)mem).value[i]; //no interning atm
                            w_symbol((int)Symbol.rb_intern(symbolString), arg, caller);
                            w_object(((Struct)obj).instance_vars[symbolString], arg, limit, caller);
                        }
                    }
                }

                else if (obj is String)
                {
                    w_uclass(obj, Ruby.Runtime.Init.rb_cString, arg, caller);
                    w_byte((char)TYPE_STRING, arg, caller);
                    w_bytes(((String)obj).value, ((String)obj).value.Length, arg, caller);
                }
                else if (obj is Regexp)
                {
                    w_uclass(obj, Ruby.Runtime.Init.rb_cRegexp, arg, caller);
                    w_byte(TYPE_REGEXP, arg, caller);
                    string regExpStr = ((Regexp)obj).value.ToString();
                    w_bytes(regExpStr, regExpStr.Length, arg, caller);
                    w_byte((char)Regexp.rb_reg_options(caller, (Regexp)obj), arg, caller);
                }
                else if (obj is Array)
                {
                    w_uclass(obj, Ruby.Runtime.Init.rb_cArray, arg, caller);
                    w_byte((char)TYPE_ARRAY, arg, caller);
                    {
                        int len = ((Array)obj).Count;
                        System.Collections.ArrayList ptr = ((Array)obj).value;
                        w_long(len, arg, caller);
                        foreach (object arrObj in ptr)
                        {
                            w_object(arrObj, arg, limit, caller);
                        }
                    }
                }
                else if (obj is Hash)
                {
                    w_uclass(obj, Ruby.Runtime.Init.rb_cHash, arg, caller);
                    if (((Hash)obj).defaultProc != null)
                        throw new TypeError(string.Format("cannot dump hash with default proc")).raise(caller);
                    else if (((Hash)obj).defaultValue == null)
                        w_byte(TYPE_HASH, arg, caller);
                    else
                        w_byte(TYPE_HASH_DEF, arg, caller);
                    w_long(((Hash)(obj)).value.Count, arg, caller);
                    foreach (HashKeyValuePair pair in ((Hash)obj).value)
                    {
                        w_object(pair.Key.key, c_arg.arg, c_arg.limit, caller);
                        w_object(pair.Value, c_arg.arg, c_arg.limit, caller);
                    }
                    if (((Hash)obj).defaultValue != null)
                        w_object(((Hash)obj).defaultValue, arg, limit, caller);
                }
                else if (obj is Data) //TEST:
                {               
                    object v;
                        
                    w_class(TYPE_DATA, obj, arg, true, caller);
                    if (!Eval.RespondTo(obj, s_dump_data))
                        throw new TypeError(string.Format("no marshal_dump is defined for class {0}", rb_obj_classname(obj, caller))).raise(caller);

                    v = Eval.CallPrivate0(obj, caller, s_dump_data, null);
                    w_object(v, arg, limit, caller);
                }
                else if (obj is Object)//TEST:
                {
                    w_class(TYPE_OBJECT, obj, arg, true, caller);
                    w_ivar(((Object)obj).instance_vars, c_arg, caller);
                }
                else
                    throw new TypeError(string.Format("can't dump {0}", ((Basic)obj).my_class.classname())).raise(caller);
            }

            if (ivtbl != null)
            {
                w_ivar(ivtbl, c_arg, caller);
            }
        }

        private static int StructLen(object obj, Frame caller)
        {
            object len = Ruby.Methods.rb_struct_size.singleton.Call0(null, obj, caller, null);
            return (int)len;
        }

        //----------------------------------------------------------------------------------
        // Load
        //----------------------------------------------------------------------------------

        internal class load_arg
        {
            internal object src;
            internal int offset;
            internal System.Collections.Generic.Dictionary<int, int> symbols;
            internal object data;
            internal object proc;
            internal bool taint;

            internal load_arg()
            {
                src = null;
                offset = 0;
                symbols = new System.Collections.Generic.Dictionary<int, int>();
                data = null;
                proc = null;
                taint = false;
            }
        }

        //----------------------------------------------------------------------------------

        internal static void long_toobig(Frame caller, int size)
        {
            throw new TypeError(string.Format("long too big for this architecture (size {0}, given {1})", SIZEOF_LONG, size)).raise(caller);
        }

        internal static double load_mantissa(double d, string buf, int len)
        {
            int bufIndex = 0;

            if (--len > 0 && !(buf[bufIndex++] == (char)0))
            {    /* binary mantissa mark */
                int e;
                bool s = d < 0;
                int dig = 0;

                uint m;

                double fabsd = System.Math.Abs(d);
                double frexpd = frexp(fabsd, out e);
                double ldexpd = ldexp(frexpd, DECIMAL_MANT);
                modf(ldexpd, out d);
                do
                {
                    m = 0;
                    switch (len)
                    {
                        default:
                            {
                                m = (uint)buf[bufIndex++] & 0xff;
                                goto case 3;
                            }
                        case 3:
                            {
                                m = (uint)((m << 8) | (byte)(buf[bufIndex++] & 0xff));
                                goto case 3;
                            }
                        case 2:
                            {
                                m = (uint)((m << 8) | (byte)(buf[bufIndex++] & 0xff));
                                goto case 3;
                            }
                        case 1:
                            {
                                m = (uint)((m << 8) | (byte)(buf[bufIndex++] & 0xff));
                                break;
                            }
                    }
                    dig -= len < MANT_BITS / 8 ? 8 * len : MANT_BITS;
                    d += ldexp((double)m, dig);
                } while ((len -= MANT_BITS / 8) > 0);
                d = ldexp(d, e - DECIMAL_MANT);
                if (s) d = -d;
            }
            return d;

        }

        //variable.c
        //MOVE: this doesn't belong here, move to variable.cs
        internal static object rb_path2class(Frame caller, string path)
        {
            int pbeg = 0;
            int p = 0;
            //int id;
            object c = Ruby.Runtime.Init.rb_cObject;

            if (path[0] == '#')
            {
                throw new ArgumentError(string.Format("can't retrieve anonymous class {0}", path)).raise(caller);
            }
            while (p < path.Length)
            {

                object str;
                while (p < path.Length && path[p] != ':') p++;
                str = new String(path.Substring(pbeg, p - pbeg));
                if (p < path.Length && path[p] == ':')
                {
                    if (p + 1 >= path.Length || path[p + 1] != ':') 
                        goto undefined_class;
                    p += 2;
                    pbeg = p;
                }
                if (!((Class)c).const_defined(((String)str).value))
                {
                    goto undefined_class;
                }
                c = ((Class)c).const_get(((String)str).value, caller);
                Class.Type type = ((Class)c)._type;
                switch (type)
                {
                    case Class.Type.Class:
                    case Class.Type.Module:
                        break;
                    default:
                        throw new TypeError(string.Format("{0} does not refer class/module", path)).raise(caller);
                }
            }
            return c;

        undefined_class:
            throw new ArgumentError(string.Format("undefined class/module {0}", path.Substring(0, p))).raise(caller);
        }

        internal static object path2class(Frame caller, string path)
        {
            object v = rb_path2class(caller, path);

            if (!(v is Class))
            {
                throw new ArgumentError(string.Format("{0} does not refer class", path)).raise(caller);
            }
            return v;
        }

        //----------------------------------------------------------------------------------

        internal static object load(Frame caller, load_arg arg)
        {
            return r_object(caller, arg);
        }

        internal static object load_ensure(load_arg arg)
        {
            arg.symbols = null;
            return null;
        }

        //----------------------------------------------------------------------------------

        internal static int r_symlink(Frame caller, load_arg arg)
        {
            int id;
            int num = r_long(caller, arg);

            if (arg.symbols.ContainsKey(num))
            {
                id = arg.symbols[num];
                return id;
            }

            throw new ArgumentError("bad symbol").raise(caller);
        }

        internal static int r_symreal(Frame caller, load_arg arg)
        {
            int id;

            id = (int)Symbol.rb_intern(((String)r_bytes(caller, arg)).value);
            arg.symbols.Add(arg.symbols.Count, id);

            return id;
        }

        internal static int r_symbol(Frame caller, load_arg arg)
        {
            if (r_byte(caller, arg) == TYPE_SYMLINK)
            {
                return r_symlink(caller, arg);
            }
            return r_symreal(caller, arg);
        }

        internal static string r_unique(Frame caller, load_arg arg)
        {
            return Symbol.rb_id2name((uint)r_symbol(caller, arg));
        }

        internal static object r_entry(Frame caller, object v, load_arg arg)
        {
            Ruby.Methods.rb_hash_aset.singleton.Call2(null, arg.data, caller, null, ((Hash)(arg.data)).value.Count, v);
            if (arg.taint)
            {
                ((Basic)v).Tainted = true;
            }
            return v;
        }

        internal static object r_bytes(Frame caller, load_arg arg)
        {
            return r_bytes0(caller, r_long(caller, arg), arg);
        }

        internal static object r_bytes0(Frame caller, int len, load_arg arg)
        {
            object str;

            if (len == 0) return new String();

            if (arg.src is String)
            {
                if (((String)arg.src).value.Length > arg.offset)
                {
                    str = new String(((String)arg.src).value.Substring(arg.offset, len));
                    arg.offset += len;
                }
                else
                {
                    goto too_short;
                }
            }
            else
            {
                object src = arg.src;
                object n = len;
                //TEST: the address of n is passed in the ruby code, these calls probably arn't equivalent 
                //        str = rb_funcall2(src, s_read, 1, &n);
                str = Eval.CallPrivate(src, caller, s_read, null, n);
                if (str == null) goto too_short;
                if (!(str is String))
                {
                    str = String.StringValue(str, caller);
                }
                if (((String)str).value.Length != len) goto too_short;
                if (((Basic)str).Tainted)
                {
                    arg.taint = true;
                }
                if(((Basic)str).Tainted)
                {
                    arg.taint = true;
                }
            }

            return str;

        too_short:
            throw new ArgumentError("marshal data too short").raise(caller);
        }

        internal static int r_byte(Frame caller, load_arg arg)
        {
            int c;

            if (arg.src is String)
            {
                if (((String)arg.src).value.Length > arg.offset)
                {
                    c = (byte)((String)arg.src).value[arg.offset++];
                }
                else
                {
                    throw new ArgumentError("marshal data too short").raise(caller);
                }
            }
            else
            {
                object src = arg.src;
                object v = Eval.CallPrivate(src, caller, s_getc, null);
                if (v == null)
                {
                    throw EOFError.rb_eof_error().raise(caller);
                }
                c = (byte)(int)v;
            }

            return c;
        }

        internal static int r_long( Frame caller, load_arg arg)
        {
            int x;
            int c = ((((char)r_byte(caller, arg)) ^ 128) - 128);    

            if (c == 0) return 0;
            if (c > 0)
            {
                if (4 < c && c < 128)
                {
                    return c - 5;
                }
                if (c > SIZEOF_LONG) long_toobig(caller, c);
                x = 0;
                for (int i = 0; i < c; i++)
                {
                    x |= (int)r_byte(caller, arg) << (8 * i);
                }
            }
            else
            {
                if (-129 < c && c < -4)
                {
                    return c + 5;
                }
                c = -c;
                if (c > SIZEOF_LONG) long_toobig(caller, c);
                x = -1;
                for (int i = 0; i < c; i++)
                {
                    x &= ~((int)0xff << (8 * i));
                    x |= (int)r_byte(caller, arg) << (8 * i);
                }
            }
            return x;
        }

        internal static object r_string(Frame caller, load_arg arg)
        {
            return r_bytes(caller, arg);
        }

        internal static object r_object( Frame caller, load_arg arg)
        {
            int temp = 0;
            return r_object0(caller, arg, arg.proc, ref temp, null);
        }

        internal static bool rb_special_const_p(object v)
        {
            bool specialConst = false;

            if (v is int)
            {
                specialConst = true;
            }
            else if (v is bool)
            {
                specialConst = true;
            }
            if (v == null)
            { 
                specialConst = true;
            }

            return specialConst;
        }

        //MOVE: this is not marshal specific
        internal static bool RTEST(object o)
        {
            if (o is bool && ((bool)o) == false)
            {
                return false;
            }
            else if (o == null)
            {
                return false;
            }
            return true;
        }
       
        //variable.c
        //MOVE: this doesn't belong here, move to variable.cs
        internal static object rb_ivar_set(Frame caller, object obj, int id, object val)
        {
            if (!((Basic)obj).Tainted && Eval.rb_safe_level() >= 4)
            {
                throw new SecurityError("Insecure: can't modify instance variable").raise(caller);
            }
            if (((Basic)obj).Frozen)
            {
                TypeError.rb_error_frozen(caller, "object");
            }
            if ((obj is Class && (((Class)obj)._type == Class.Type.Class ||
                ((Class)obj)._type == Class.Type.Module)) ||
                obj is Object)
            {
                ((Object)obj).instance_variable_set(Symbol.rb_id2name((uint)id), val);
            }
            else
            {
                Object.generic_ivar_set(obj, Symbol.rb_id2name((uint)id), val);
            }

            return val;
        }

        internal static void r_ivar(Frame caller, object obj, load_arg arg)
        {
            int len;

            len = r_long(caller, arg);
            if (len > 0)
            {
                while (len-- > 0)
                {
                    int id = r_symbol(caller, arg);
                    object val = r_object(caller, arg);
                    rb_ivar_set(caller, obj, id, val);
                }
            }
        }

        internal static object path2module(string path, Frame caller)
        {
            object v = rb_path2class(caller, path);

            if (((Class)v)._type != Class.Type.Module)
            {
                throw new ArgumentError(string.Format("{0} does not refer module", path)).raise(caller);
            }
            return v;
        }

        internal static object r_object0(Frame caller, load_arg arg, object proc, ref int ivp, object extmod)
        {
            object v = null;
            int type = r_byte(caller, arg);
            int id;

            switch (type)
            {
                case TYPE_LINK: //TEST:

                    id = r_long(caller, arg);
                    v = Ruby.Methods.rb_hash_aref.singleton.Call1(null, arg.data, caller, null, id);
                    if (v == null)
                    {
                        throw new ArgumentError("dump format error (unlinked)").raise(caller);
                    }
                    return v;

                case TYPE_IVAR: //TEST:
                    {
                        int ivar = 1;
                        v = r_object0(caller, arg, null, ref ivar, extmod);
                        if (ivar > 0) r_ivar(caller, v, arg);                        
                    }
                break;

                case TYPE_EXTENDED: //TEST:
                    {
                        object m = path2module(r_unique(caller, arg), caller);
                        if (extmod == null)
                        {
                            extmod = new Array();
                            ((Array)extmod).value.Add(m);
                        }

                        int temp = 0;
                        v = r_object0(caller, arg, null, ref temp, extmod);
                        while (((Array)extmod).value.Count > 0)
                        {
                            m = ((Array)extmod).value[((Array)extmod).value.Count - 1];
                            ((Array)extmod).value.RemoveAt(((Array)extmod).value.Count - 1);
                            Class.rb_extend_object(caller, v, (Class)m);
                        }
                    }
                break;

                case TYPE_UCLASS:
                    {
                        object c = path2class(caller, r_unique(caller, arg));

                        if (((Class)c)._type == Class.Type.Singleton)
                        {
                            throw new TypeError(string.Format("singleton can't be loaded")).raise(caller);
                        }
                        int temp = 0;
                        v = r_object0(caller, arg, null, ref temp, extmod);
                        if (rb_special_const_p(v) || v is Object || (v is Class && ((Class)v)._type == Class.Type.Class))
                        {
                            throw new ArgumentError("dump format error (user class)").raise(caller);
                        }
                        if ((v is Class && ((Class)v)._type == Class.Type.Module) || !RTEST(Ruby.Methods.rb_class_inherited_p.singleton.Call1(null, c, caller, null, ((Basic)v).my_class)))
                        {
                            object tmp = new Object((Class)c);

                            //TODO: Write this comparison
                            //if (TYPE(v) != TYPE(tmp))
                            //{
                            //    throw new ArgumentError("dump format error (user class)").raise(caller);
                            //}
                        }
                        ((Basic)v).my_class = (Class)c;
                    }
                    break;

                case TYPE_NIL:
                    v = null;
                    break;

                case TYPE_TRUE:
                    v = true;
                    break;

                case TYPE_FALSE:
                    v = false;
                    break;

                case TYPE_FIXNUM:
                    {
                        v = r_long(caller, arg);
                    }
                    break;

                case TYPE_FLOAT:
                    {
                        double d;
                        object str = r_bytes(caller, arg);
                        string ptr = ((String)str).value;

                        if (ptr.Equals("nan"))
                        {
                            d = double.NaN;
                        }
                        else if (ptr.Equals("inf"))
                        {
                            d = double.PositiveInfinity;
                        }
                        else if (ptr.Equals("-inf"))
                        {
                            d = double.NegativeInfinity;
                        }
                        else if (ptr.Equals("-0"))
                        {
                            d = -0.0;
                        }
                        else
                        {
                            int e = 0;
                            d = String.strtod(ptr, 0, out e);
                            d = load_mantissa(d, ptr.Substring(e), ((String)str).value.Length - e);
                        }

                        v = new Float(d);
                        r_entry(caller, v, arg);
                    }
                    break;

                case TYPE_BIGNUM:
                    {
                        int len;
                        uint[] digits;
                        object data;

                        int sign = ((char)r_byte(caller, arg) == '+') ? 1 : -1;

                        len = r_long(caller, arg);
                        data = r_bytes0(caller, len * 2, arg);

                        int bigLen = (len + 1) * 2 / SIZEOF_BDIGITS;                      

                        //extract data to bytes
                        digits = new uint[bigLen];
                        byte[] digitData = new byte[bigLen * SIZEOF_BDIGITS];
                        char[] charDigitData = ((String)data).value.ToCharArray();
                        for (int i = 0; i < ((String)data).value.Length; i++)
                        {
                            digitData[i] = (byte)charDigitData[i];
                        }
                        //save data to digits
                        for (int uintCount = 0; uintCount < digits.Length; uintCount++)
                        {
                            digits[uintCount] = System.BitConverter.ToUInt32(digitData, (uintCount * 4));
                        }
                        v = new Bignum(new IronMath.integer(sign, digits));
                        r_entry(caller, v, arg);
                    }
                    break;
                case TYPE_STRING:
                    v = r_entry(caller, r_string(caller, arg), arg);
                    break;

                case TYPE_REGEXP:
                    {
                        object str = r_bytes(caller, arg);
                        int options = r_byte(caller, arg);
                        v = r_entry(caller, new Regexp(((String)str).value, options), arg);
                    }
                    break;

                case TYPE_ARRAY:
                    {
                        int len = r_long(caller, arg);

                        v = new Array();
                        r_entry(caller, v, arg);
                        while (len-- > 0)
                        {
                            ((Array)v).value.Add(r_object(caller, arg));
                        }
                    }
                    break;

                case TYPE_HASH:
                case TYPE_HASH_DEF:
                    {
                        int len = r_long(caller, arg);

                        v = new Hash();
                        r_entry(caller, v, arg);
                        while (len-- > 0)
                        {
                            object key = r_object(caller, arg);
                            object value = r_object(caller, arg);
                            ((Hash)v).Add(key, value);
                        }
                        if (type == TYPE_HASH_DEF)
                        {
                            ((Hash)v).defaultValue = r_object(caller, arg);
                        }
                    }
                    break;

                case TYPE_STRUCT:
                    {
                        Class klass;
                        object mem;
                        object[] values;
                        int len;
                        int slot;

                        string klassPath = r_unique(caller, arg);
                        klass = (Class)path2class(caller, klassPath);
                        mem = Ruby.Methods.rb_struct_s_members.singleton.Call0(klass, klass, caller, null);
                        if (mem == null)
                        {
                            throw new TypeError("uninitialized struct").raise(caller);
                        }
                        len = r_long(caller, arg);
                        values = new object[len];
                        for (int i = 0; i < len; i++)
                        {
                            slot = r_symbol(caller, arg);

                            if ((string)((Array)mem).value[i] != Symbol.rb_id2name((uint)slot))
                            {
                                string errorString = string.Format("struct {0} not compatible (:{1} for :{2})",
                                    rb_class2name(klass, caller),
                                    Symbol.rb_id2name((uint)slot),
                                    ((Array)mem).value[i]);

                                throw new TypeError(errorString).raise(caller);
                            }
                            values[i] = r_object(caller, arg);
                        }

                        v = Ruby.Methods.rb_class_new_instance.singleton.Call(klass, klass, caller, null, new Array(values));
                        r_entry(caller, v, arg);
                    }
                    break;
                case TYPE_USERDEF:
                    {
                        object klass = path2class(caller, r_unique(caller, arg));
                        object data;

                        if (!Eval.RespondTo(klass, s_load))
                        {
                            throw new TypeError(string.Format("class {0} needs to have method `_load'", rb_class2name(klass, caller))).raise(caller) ;
                        }

                        data = r_string(caller, arg);
                        if (ivp > 0)
                        {
                            r_ivar(caller, data, arg);
                            ivp = 0;
                        }
                        v = Eval.CallPrivate1(klass, caller, s_load, null, data);
                        r_entry(caller, v, arg);                        
                    }
                    break;
                case TYPE_USRMARSHAL: 
                    {
                        object klass = path2class(caller, r_unique(caller, arg));
                        object data;

                        v = Ruby.Methods.rb_obj_alloc.singleton.Call0(null, klass, caller, null);
                        if (extmod != null)
                        {
                            while (((Array)extmod).value.Count > 0)
                            {
                                object m = ((Array)extmod).value[((Array)extmod).value.Count - 1];
                                ((Array)extmod).value.RemoveAt(((Array)extmod).value.Count - 1);
                                Class.rb_extend_object(caller, v, (Class)m);
                            }
                        }
                        if (!Eval.RespondTo(v, s_mload))
                        {
                            throw new TypeError(string.Format("instance of {0} needs to have method `marshal_load'", rb_class2name(klass, caller))).raise(caller);
                        }
                        r_entry(caller, v, arg);
                        data = r_object(caller, arg);
                        Eval.CallPrivate1(v, caller, s_mload, null, data);
                    }
                    break;

                case TYPE_OBJECT:
                    {
                        object klass = path2class(caller, r_unique(caller, arg));
                        v = Ruby.Methods.rb_obj_alloc.singleton.Call0(null, klass, caller, null);
                        if (!(v is Object))
                        {
                            throw new ArgumentError(string.Format("dump format error")).raise(caller);
                        }
                        r_entry(caller, v, arg);
                        r_ivar(caller, v, arg);
                    }
                    break;

                case TYPE_DATA://TEST
                    {
                        object klass = path2class(caller, r_unique(caller, arg));
                        if (Eval.RespondTo(klass, s_alloc))
                        {
                            type_data_warn = true; //TEST: static int warn = Qtrue; - test that this is equivalent note the 'STATIC' keyword
                            if (type_data_warn)
                            {
                                Errors.rb_warn("define `allocate' instead of `_alloc'");
                                type_data_warn = false;
                            }
                            v = Eval.CallPrivate0(klass, caller, s_alloc, null);
                        }
                        else
                        {
                            v = Ruby.Methods.rb_obj_alloc.singleton.Call0(null, klass, caller, null);
                        }
                        if (!(v is Data))
                        {
                            throw new ArgumentError("dump format error").raise(caller);
                        }
                        r_entry(caller, v, arg);
                        if (!Eval.RespondTo(v, s_load_data))
                        {
                            throw new TypeError(string.Format("class {0} needs to have instance method `_load_data'", rb_class2name(klass, caller))).raise(caller);
                        }
                        int temp = 0;
                        Eval.CallPrivate1(v, caller, s_load_data, null, r_object0(caller, arg, null, ref temp, extmod));
                    }
                break;

                case TYPE_MODULE_OLD: //TEST:
                    {                     
                        object str = r_bytes(caller, arg);
                        v = rb_path2class(caller, ((String)str).value);
                        r_entry(caller, v, arg);
                    }
                break;

                case TYPE_CLASS:
                    {
                        object str = r_bytes(caller, arg);
                        v = path2class(caller, ((String)str).value);
                        r_entry(caller, v, arg);
                    }
                    break;
                case TYPE_MODULE: //TEST:
                    {                        
                        object str = r_bytes(caller, arg);

                        v = path2module(((String)str).value, caller);
                        r_entry(caller, v, arg);
                    }
                    break;

                case TYPE_SYMBOL: //TEST:
                    v = new Symbol((uint)r_symreal(caller, arg));
                    break;

                case TYPE_SYMLINK: //TEST:
                    return new Symbol((uint)r_symlink(caller, arg));

                default:
                    throw new ArgumentError(string.Format("dump format error(0x{0})", type)).raise(caller);
            }

            if (proc != null) //TEST:
            {
                Eval.CallPrivate1(proc, caller, "call", null, v);
            }
            return v;

        }
    }
}
