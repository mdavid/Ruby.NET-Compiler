/**********************************************************************

  Ruby.NET Runtime Library
  Originally developed at Queensland University of Technology
 
  Some sections of this C# code mirror the structure of the C code in the
  Ruby 1.8.2 Interpreter Copyright (C) 1993-2003 Yukihiro Matsumoto, et.al.
  
**********************************************************************/

using Ruby.Runtime;


namespace Ruby
{
    namespace Runtime
    {
        internal enum Access { Private, Protected, Public, ModuleFunction, notPrivate };
    }

    public partial class Method : Basic
    {
        internal Class oklass; // BBTAG: this corresponds to the klass field in the CRuby METHOD struct
        internal Class rklass;
        internal object recv;
        internal string id, oid;
        internal RubyMethod body;


        internal Method(object recv, string id)
            : base(Ruby.Runtime.Init.rb_cMethod)
        {
            this.id = id;
            this.recv = recv;
            this.body = Eval.FindPrivateMethod(recv, null, id, out rklass);
        }

        internal Method(object recv, string id, string oid, Class rklass, RubyMethod body)
            : this(recv, id)
        {
            this.oid = oid;
            this.rklass = rklass;
            this.body = body;
        }

        internal Method(object recv, string id, string oid, Class rklass, Class oklass, RubyMethod body, bool unbound)
            : this(recv, id)
        {
            if (unbound)
                this.my_class = Ruby.Runtime.Init.rb_cUnboundMethod;
            this.oid = oid;
            this.rklass = rklass;
            this.oklass = oklass;
            this.body = body;
        }
    }
}
