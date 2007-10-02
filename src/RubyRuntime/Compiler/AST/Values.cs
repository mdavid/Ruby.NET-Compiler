/**********************************************************************

  Ruby.NET Runtime Library
  Originally developed at Queensland University of Technology
 
**********************************************************************/


using System.Collections.Generic;
using System.Text;
using System.Diagnostics;


namespace Ruby.Compiler.AST
{

    internal class ARRAY : Node        // Array Value
    {
        internal ARGS args;
        // [ args ]

        internal ARRAY(YYLTYPE location)
            : base(location)
        {
            this.args = null;
        }

        internal ARRAY(Node args, YYLTYPE location)
            : base(location)
        {
            if (args is ARGS)
                this.args = (ARGS)args;
            else
                this.args = new ARGS(args, null, null, null, args.location);
        }

        internal override void GenCode0(CodeGenContext context)
        {
            if (args != null)
            {
                bool created;
                ISimple list = args.GenArgList(context, out created);
                list.GenSimple(context);
                context.callvirt(Runtime.ArgList.ToRubyArray);

                context.ReleaseLocal(list, created);
            }
            else
                context.newobj(Runtime.Array.ctor);
        }
    }




    internal class HASH : Node        // Hash value
    {
        // { key1 => value1, key2 => value2, ... }
        internal Node elements;

        internal HASH(Node elements, YYLTYPE location)
            : base(location)
        {
            this.elements = elements;
        }

        internal override void GenCode0(CodeGenContext context)
        {
            // hash = new Hash();
            context.newobj(Runtime.Hash.ctor);
            int hash = context.StoreInTemp("hash", Runtime.HashRef, location);

            Node entry = elements;
            while (entry != null)
            {
                bool key_created, value_created;

                ISimple key = context.PreCompute0(entry, "key", out key_created);
                entry = entry.nd_next;
                ISimple value = context.PreCompute0(entry, "value", out value_created);
                entry = entry.nd_next;

                // hash.Add(key, value);
                context.ldloc(hash);
                key.GenSimple(context);
                value.GenSimple(context);
                context.callvirt(Runtime.Hash.Add);

                context.ReleaseLocal(key, key_created);
                context.ReleaseLocal(value, value_created);
            }

            context.ldloc(hash);

            context.ReleaseLocal(hash, true);
        }
    }




    internal class REGEXP : Node        // Regular Expression
    {
        // /pattern/options
        private int options;
        private Node pattern;

        internal REGEXP(Node pattern, int options, YYLTYPE location)
            : base(location)
        {
            this.pattern = pattern;
            this.options = options;
        }

        internal override void GenCode0(CodeGenContext context)
        {
            // new Regexp(pattern, options);
            if (pattern is VALUE)
                context.ldstr(((VALUE)pattern).value.ToString());
            else
            {
                pattern.GenCode(context);
                context.callvirt(Runtime.String.ToString);
            }
            context.ldc_i4(options);
            context.newobj(Runtime.Regexp.ctor);
        }
    }
}