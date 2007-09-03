/**********************************************************************

  Ruby.NET Runtime Library
  Originally developed at Queensland University of Technology
 
  Some sections of this C# code mirror the structure of the C code in the
  Ruby 1.8.2 Interpreter Copyright (C) 1993-2003 Yukihiro Matsumoto, et.al.
  
**********************************************************************/
using Ruby.Compiler;
using Ruby.Runtime;
using System.Globalization;

namespace Ruby
{
    [UsedByRubyCompiler]
    public partial class Symbol : Basic
    {
        //TODO:
        //Check that no null chars can get into the symbol and 
        //that the symbol string is not empty

        //-----------------------------------------------------------------


        internal Symbol(): base(Ruby.Runtime.Init.rb_cSymbol)
        {
        }

        [UsedByRubyCompiler]
        public Symbol(string name)
            : this()
        {
            this.id_new = rb_intern(name);
        }

        internal Symbol(uint id)
            : this()
        {
            this.id_new = id;
        }

        public Symbol(Class klass)
            : base(klass) 
        { 
        }


        //----------------------------------------------------------------------------------

        internal static uint last_id = (uint)Tokens.tLAST_TOKEN;
        
        internal uint id_new;
        internal static System.Collections.Generic.Dictionary<string, uint> sym_tbl =
            new System.Collections.Generic.Dictionary<string, uint>();
        internal static System.Collections.Generic.Dictionary<uint, string> sym_rev_tbl =
            new System.Collections.Generic.Dictionary<uint, string>();

        //----------------------------------------------------------------------------------

        internal struct IDStringPair
        {

            internal IDStringPair(uint token, string name)
            {
                this.token = token;
                this.name = name;
            }

            internal uint token;
            internal string name;
        }

        internal static IDStringPair[] op_tbl = {
            new IDStringPair((uint)Tokens.tDOT2,    ".."),
            new IDStringPair((uint)Tokens.tDOT3,    "..."),
            new IDStringPair('+',    "+"),
            new IDStringPair('-',    "-"),
            new IDStringPair('+',    "+(binary)"),
            new IDStringPair('-',    "-(binary)"),
            new IDStringPair('*',    "*"),
            new IDStringPair('/',    "/"),
            new IDStringPair('%',    "%"),
            new IDStringPair((uint)Tokens.tPOW,    "**"),
            new IDStringPair((uint)Tokens.tUPLUS,    "+@"),
            new IDStringPair((uint)Tokens.tUMINUS,    "-@"),
            new IDStringPair((uint)Tokens.tUPLUS,    "+(unary)"),
            new IDStringPair((uint)Tokens.tUMINUS,    "-(unary)"),
            new IDStringPair('|',    "|"),
            new IDStringPair('^',    "^"),
            new IDStringPair('&',    "&"),
            new IDStringPair((uint)Tokens.tCMP,    "<=>"),
            new IDStringPair('>',    ">"),
            new IDStringPair((uint)Tokens.tGEQ,    ">="),
            new IDStringPair('<',    "<"),
            new IDStringPair((uint)Tokens.tLEQ,    "<="),
            new IDStringPair((uint)Tokens.tEQ,    "=="),
            new IDStringPair((uint)Tokens.tEQQ,    "==="),
            new IDStringPair((uint)Tokens.tNEQ,    "!="),
            new IDStringPair((uint)Tokens.tMATCH,    "=~"),
            new IDStringPair((uint)Tokens.tNMATCH,    "!~"),
            new IDStringPair('!',    "!"),
            new IDStringPair('~',    "~"),
            new IDStringPair('!',    "!(unary)"),
            new IDStringPair('~',    "~(unary)"),
            new IDStringPair('!',    "!@"),
            new IDStringPair('~',    "~@"),
            new IDStringPair((uint)Tokens.tAREF,    "[]"),
            new IDStringPair((uint)Tokens.tASET,    "[]="),
            new IDStringPair((uint)Tokens.tLSHFT,    "<<"),
            new IDStringPair((uint)Tokens.tRSHFT,    ">>"),
            new IDStringPair((uint)Tokens.tCOLON2,    "::"),
            new IDStringPair('`',    "`"),
            /*new IDStringPair(0,    "\0")*/};

        internal static byte ID_SCOPE_SHIFT = 3;
        internal static byte ID_SCOPE_MASK = 0x07;
        internal static byte ID_LOCAL = 0x01;
        internal static byte ID_INSTANCE = 0x02;
        internal static byte ID_GLOBAL = 0x03;
        internal static byte ID_ATTRSET = 0x04;
        internal static byte ID_CONST = 0x05;
        internal static byte ID_CLASS = 0x06;
        internal static byte ID_JUNK = 0x07;
        internal static byte ID_INTERNAL = ID_JUNK;

        internal static bool is_notop_id(uint id)
        {
            return ((id) > (uint)Tokens.tLAST_TOKEN);
        }

        internal static bool is_local_id(uint id)
        {
            return (is_notop_id(id) && ((id) & ID_SCOPE_MASK) == ID_LOCAL);
        }

        internal static bool is_global_id(uint id)
        {
            return (is_notop_id(id) && ((id) & ID_SCOPE_MASK) == ID_GLOBAL);
        }

        internal static bool is_instance_id(uint id)
        {
            return (is_notop_id(id) && ((id) & ID_SCOPE_MASK) == ID_INSTANCE);
        }

        internal static bool is_attrset_id(uint id)
        {
            return (is_notop_id(id) && ((id) & ID_SCOPE_MASK) == ID_ATTRSET);
        }

        internal static bool is_const_id(uint id)
        {
            return (is_notop_id(id) && ((id) & ID_SCOPE_MASK) == ID_CONST);
        }

        internal static bool is_class_id(uint id)
        {
            return (is_notop_id(id) && ((id) & ID_SCOPE_MASK) == ID_CLASS);
        }

        internal static bool is_junk_id(uint id)
        {
            return (is_notop_id(id) && ((id) & ID_SCOPE_MASK) == ID_JUNK);
        }

        internal static uint rb_id_attrset(uint id)
        {
            id &= (uint)~ID_SCOPE_MASK;
            id |= ID_ATTRSET;
            return id;
        }


        //----------------------------------------------------------------------------------

        internal static uint rb_intern(string name)
        {

            int m = 0; //m is an index into name
            uint id;
            int last;

            if (sym_tbl.TryGetValue(name, out id))
            {
                return id;
            }

            last = name.Length - 1;
            id = 0;

            switch (name[0])
            {
                case '$':
                    id |= ID_GLOBAL;
                    m++;
                    if (!Scanner.is_identchar(name[m])) m++;
                    break;
                case '@':
                    if (name[1] == '@')
                    {
                        m++;
                        id |= ID_CLASS;
                    }
                    else
                    {
                        id |= ID_INSTANCE;
                    }
                    m++;
                    break;
                default:
                    if (name[0] != '_' && !String.ISALPHA((byte)name[0]) && !MultiByteChar.ismbchar(name[0]))
                    {
                        /* operators */
                        for (int i = 0; i < op_tbl.Length; i++)
                        {
                            if (string.Compare(op_tbl[i].name, name) == 0)
                            {
                                id = op_tbl[i].token;
                                goto id_regist;
                            }
                        }
                    }

                    if (name[last] == '=') {
                    //  /* attribute assignment */
                        string buf = name.Substring(0, name.Length - 1);

                        id = rb_intern(buf);
                        if(id > (uint)Tokens.tLAST_TOKEN && !is_attrset_id(id)){                    
                          id = rb_id_attrset(id);
                          goto id_regist;
                        }
                        id = ID_ATTRSET;
                    }
                    else if (String.ISUPPER((byte)name[0])) {
                      id = ID_CONST;
                    }
                    else {
                      id = ID_LOCAL;
                    }
                    break;

            }
            while (m < name.Length && Scanner.is_identchar(name[m]))
            {
                m += MultiByteChar.mbclen(name[m]);
            }
            if (m < name.Length && name[m] != 0) id = ID_JUNK;          
            id |= ++last_id << ID_SCOPE_SHIFT;
        id_regist:
            sym_tbl.Add(name, id);
            sym_rev_tbl.Add(id, name);
            return id;
        }

        internal static string 
        rb_id2name(uint id)        
        {
            string name;
        
            if (id < (uint)Tokens.tLAST_TOKEN) {

                for (int i = 0; i < op_tbl.Length; i++)
                {
                    if (op_tbl[i].token == id)
                        return op_tbl[i].name;
                }        
            }

            if (sym_rev_tbl.TryGetValue(id, out name))
                return name;

            if (is_attrset_id(id))
            {
                uint id2 = (uint)(id & ~ID_SCOPE_MASK) | ID_LOCAL;
        
        again:
                name = rb_id2name(id2);
                if (name != null && name.Length > 0) {

                    string buf = name + "=";
                    rb_intern(buf);
                    return rb_id2name(id);
                }
                if (is_local_id(id2)) {
                    id2 = (uint)(id & ~ID_SCOPE_MASK) | ID_CONST;
                    goto again;
                }
            }
            return null;
        }

        //-----------------------------------------------------------------

        public override string ToString()
        {
            return rb_id2name(id_new);
        }

        internal static string rb_to_id(Frame caller, object obj)
        {
            if (obj is string)
                return str_to_id(caller, obj.ToString());
            else if (obj is String)
                return str_to_id(caller, obj.ToString());
            else if (obj is int)
            {
                Errors.rb_warn("do not use Fixnums as Symbols");
                string id = rb_id2name((uint)(int)obj);
                if (id == null)
                    throw new ArgumentError(string.Format(CultureInfo.InvariantCulture, "{0} is not a symbol", obj)).raise(caller);
                return id;
            }
            else if (obj is Symbol)
                return obj.ToString();
            else
            {
                String str = String.rb_check_string_type(obj, caller);
                if (str != null)
                    return str_to_id(caller, str.value);

                throw new TypeError(obj + " is not a Symbol").raise(caller);
            }
        }

        internal static string str_to_id(Frame caller, string str)
        {
            if (str == "")
                throw new ArgumentError("empty symbol string").raise(caller);
            return str; // rb_intern(str)
        }

        internal static string rb_to_string(Frame caller, object obj)
        {
            if (obj is string || obj is String || obj is int || obj is Symbol)
                return obj.ToString();
            else if (obj is Object)
            {
                String str = Object.CheckConvert<String>(obj, "to_s", null);
                return str.ToString();
            }
            else
            {
                throw new TypeError(obj + " is not a string").raise(caller);
            }
        }

        //-----------------------------------------------------------------

        //wartag: Deprecated 
        // - EVENTUALLY ALL CODE BELOW REPLACED/DELETED AS THE PROJECT INCORPORATES INTERNING
        
        internal string id_s
        {
            get
            {
                return rb_id2name(id_new);
            }           
        }

        //internal static System.Collections.Generic.Dictionary<int, string> sym_rev_tbl_depr =
        //    new System.Collections.Generic.Dictionary<int, string>();

        //deprecated
        //internal static int rb_intern(string name) // author: war, status: done
        //{
        //    int hashCode = name.GetHashCode();

        //    if (!sym_rev_tbl_depr.ContainsKey(hashCode))
        //    {
        //        sym_rev_tbl_depr.Add(hashCode, name);
        //    }
        //    return hashCode;
        //}

        //internal static string rb_id2name(int id) // author: war, status: done
        //{
        //    string name = null;

        //    if (sym_rev_tbl_depr.ContainsKey(id))
        //    {
        //        name = sym_rev_tbl_depr[id];
        //    }
        //    return name;
        //}



        internal static bool is_lowercase(char c)
        {
            return ((c >= 'a' && c <= 'z') || (c == '_'));
        }

        internal static bool is_uppercase(char c)
        {
            return (c >= 'A' && c <= 'Z');
        }

        internal static bool is_digit(char c)
        {
            return (c >= '0' && c <= '9');
        }

        internal static bool is_name_char(char c)
        {
            return (is_lowercase(c) || is_uppercase(c) || is_digit(c));
        }

        internal static bool is_local_id(string id)
        {
            if (id == null || id.Length == 0)
                return false;

            if (!is_lowercase(id[0]))
                return false;

            for (int i = 1; i < id.Length; i++)
                if (!is_name_char(id[i])) return false;

            return true;
        }

        internal static bool is_const_id(string id)
        {
            if (id == null || id.Length == 0)
                return false;

            if (!is_uppercase(id[0]))
                return false;

            for (int i = 1; i < id.Length; i++)
                if (!is_name_char(id[i])) return false;

            return true;
        }

        internal static bool is_class_id(string id)
        {
            return is_cvar_id(id);
        }

        internal static bool is_instance_id(string id)
        {
            if (id == null || id.Length <= 1)
                return false;

            if (id[0] != '@')
                return false;

            if (!is_lowercase(id[1]) && !is_uppercase(id[1]))
                return false;

            for (int i = 2; i < id.Length; i++)
                if (!is_name_char(id[i])) return false;

            return true;
        }

        internal static bool is_cvar_id(string id)
        {
            if (id == null || id.Length <= 2)
                return false;

            if (id[0] != '@' || id[1] != '@')
                return false;

            if (!is_lowercase(id[2]) && !is_uppercase(id[2]))
                return false;

            for (int i = 3; i < id.Length; i++)
                if (!is_name_char(id[i])) return false;

            return true;
        }
    }
}
