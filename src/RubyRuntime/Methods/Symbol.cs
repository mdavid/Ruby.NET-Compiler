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
    
    internal class rb_sym_all_symbols : MethodBody0 // author: war, status: done
    {
        internal static rb_sym_all_symbols singleton = new rb_sym_all_symbols();

        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {
            Array ary = new Array();
            foreach (uint id in Symbol.sym_tbl.Values)
            {
                ary.Add(new Symbol(id));
            }

            return ary;
        }
    }


    internal class sym_to_i : MethodBody0 // author: war, status: done
    {
        internal static sym_to_i singleton = new sym_to_i();

        public override object Call0(Class last_class, object sym, Frame caller, Proc block)
        {
            return (int)((Symbol)sym).id_new;
        }
    }


    
    internal class sym_to_int : MethodBody0 // author: cjs, status: done
    {
        internal static sym_to_int singleton = new sym_to_int();

        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {
            Errors.rb_warning("treating Symbol as an integer");
            return sym_to_i.singleton.Call0(last_class, recv, caller, block);
        }
    }


    
    internal class sym_inspect : MethodBody0 //author: war, status: done
    {
        internal static sym_inspect singleton = new sym_inspect();

        public override object Call0(Class last_class, object sym, Frame caller, Proc block)
        {
            String str;
            string name;
            uint id = ((Symbol)sym).id_new;           

            name = Symbol.rb_id2name(id);
            str = new String();
            str.value = ':' + name;            
            if (Symbol.is_junk_id(id))
            {
                str = (String)Methods.rb_str_dump.singleton.Call0(last_class, str, caller, block);
                ((String)str).value = ":\"" + ((String)str).value.Substring(2);
            }
            return str;

            //return new String(":" + ((Symbol)recv).id);
        }
    }

    
    internal class sym_to_s : MethodBody0 //author: war, status: done
    {
        internal static sym_to_s singleton = new sym_to_s();

        public override object Call0(Class last_class, object sym, Frame caller, Proc block)
        {
            return new String(Symbol.rb_id2name(((Symbol)sym).id_new));
        }
    }

    
    internal class sym_to_sym : MethodBody0 //author: Brian, status: done
    {
        internal static sym_to_sym singleton = new sym_to_sym();

        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {
            return recv;
        }
    }

    internal class rb_sym_eql : MethodBody1 //status: done
    {
        internal static rb_sym_eql singleton = new rb_sym_eql();

        public override object Call1(Class last_class, object recv, Frame caller, Proc block, object param0)
        {
            if (param0 is Symbol)
            {
                return ((Symbol)recv).id_new == ((Symbol)param0).id_new;
            }

            return false;
        }
    }  


}
