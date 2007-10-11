/**********************************************************************

  Ruby.NET Runtime Library
  Originally developed at Queensland University of Technology
 
**********************************************************************/

using System.CodeDom;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using PERWAPI;


namespace Ruby.Compiler.AST
{

    internal abstract class LVALUE : Node
    {
        internal LVALUE(YYLTYPE location): base(location)
        {
        }

        internal abstract void Assign(CodeGenContext context, Node rhs);
    }


    internal abstract class VAR : LVALUE
    {
        // vid

        internal string vid;

        internal VAR(string vid, YYLTYPE location): base(location)
        {
            this.vid = vid;
        }

        protected string Name
        {
            get { return ID.ToDotNetName(vid); }
        }

        public override CodeExpression ToCodeExpression()
        {
            return new CodeVariableReferenceExpression(vid);
        }
    }



    internal class DummyParam : LVALUE
    {
        internal DummyParam(YYLTYPE location)
            : base(location)
        {
        }

        internal override void Assign(CodeGenContext context, Node rhs)
        {
            rhs.GenCode(context);
        }
    }



    internal class PARAM : ListGen, ISimple     // Formal Parameter (used internally)
    {
        private string parameter_name;

        internal PARAM(string parameter_name, YYLTYPE location): base(location) 
        {
            this.parameter_name = parameter_name; 
        }

        public void GenSimple(CodeGenContext context)
        {
            context.ldarg(parameter_name);
        }

        internal override ISimple GenArgList(CodeGenContext context, out bool created)
        {
            created = false;
            return this;
        }
    }



    internal class StaticLocalVar : VAR, ISimple
    {
        private Scope parent_scope;
        
        private Field field
        {
            get
            {
                return CodeGenContext.FindField(parent_scope.FrameClass, Name);
            }
        }

        internal StaticLocalVar(Scope parent_scope, string vid, YYLTYPE location): base(vid, location) 
        {
            this.parent_scope = parent_scope;
        }

        internal StaticLocalVar(string vid, Scope scope, YYLTYPE location)
            : base(vid, location)
        {
            this.parent_scope = scope;
            Debug.Assert(scope != null);
        }

        public virtual void GenSimple(CodeGenContext context)
        {
            context.ldloc(0);
            context.ldfld(field);
        }

          internal override void Defined(CodeGenContext context)
        {
            context.PushTrue();
            context.box(PrimitiveType.Boolean);
        }

        internal override string DefinedName()
        {
            return "local-variable";
        }

        internal override void Assign(CodeGenContext context, Node rhs)
        {
            // object value = rhs;
            bool created;
            ISimple value = context.PreCompute(rhs, "rhs", out created);

            // locals.field = value
            context.ldloc(0);
            value.GenSimple(context);
            context.stfld(field);

            GenCode0(context);

            context.ReleaseLocal(value, created);
        }
    }



    internal class StaticOuterVar : VAR, ISimple
    {
        internal BLOCK block;
        internal int depth;

        internal StaticOuterVar(string vid, BLOCK block, int depth, YYLTYPE location): base(vid, location)
        {
            this.block = block;
            this.depth = depth;
        }

        private Field field
        {
            get
            {
                PERWAPI.Class outerFrameType = (PERWAPI.Class)block.frameFields[depth - 1].GetFieldType();
                return CodeGenContext.FindField(outerFrameType, Name);
            }
        }


        public virtual void GenSimple(CodeGenContext context)
        {        
            // thisblock.localsN.vid
            context.ldarg(0);
            context.ldfld(block.frameFields[depth - 1]);
            context.ldfld(field);
        }

        internal override void Assign(CodeGenContext context, Node rhs)
        {
            // object value = rhs;
            bool created;
            ISimple value = context.PreCompute(rhs, "rhs", out created);

            // thisblock.localsN.vid = value;
            context.ldarg(0);
            context.ldfld(block.frameFields[depth - 1]);
            value.GenSimple(context);
            context.stfld(field);

            value.GenSimple(context);

            context.ReleaseLocal(value, created);
        }

          internal override void Defined(CodeGenContext context)
        {
            context.PushTrue();
            context.box(PrimitiveType.Boolean);
        }

        internal override string DefinedName()
        {
            return "local-variable(in-block)";
        }
    }



    internal class DynamicLocalVar : StaticLocalVar, ISimple
    {
        internal DynamicLocalVar(string vid, BLOCK block, YYLTYPE location): base(vid, block, location)
        {
        }

        public override void GenSimple(CodeGenContext context)
        {
            // frame.GetDynamic("vid");
            context.ldloc(0);
            context.ldstr(Name);
            context.call(Runtime.Frame.GetDynamic);
        }

        internal override void Assign(CodeGenContext context, Node rhs)
        {
            // object value = rhs;
            bool created;
            ISimple value = context.PreCompute(rhs, "rhs", out created);

            // frame.SetDynamic("vid", value);
            context.ldloc(0);
            context.ldstr(Name);
            value.GenSimple(context);
            context.call(Runtime.Frame.SetDynamic);

            value.GenSimple(context);

            context.ReleaseLocal(value, created);
        }
    }



    internal class DynamicOuterVar : StaticOuterVar, ISimple
    {
        internal DynamicOuterVar(string vid, BLOCK block, int depth, YYLTYPE location): base(vid, block, depth, location)
        {
        }

        public override void GenSimple(CodeGenContext context)
        {
            // thisblock.localsN.GetDynamic("vid");
            context.ldarg(0);
            context.ldfld(block.frameFields[depth - 1]);
            context.ldstr(Name);
            context.call(Runtime.Frame.GetDynamic);
        }

        internal override void Assign(CodeGenContext context, Node rhs)
        {
            // object value = rhs;
            bool created;
            ISimple value = context.PreCompute(rhs, "rhs", out created);

            // thisblock.localsN.SetDynamic("vid", value);
            context.ldarg(0);
            context.ldfld(block.frameFields[depth - 1]);
            context.ldstr(Name);
            value.GenSimple(context);
            context.call(Runtime.Frame.SetDynamic);

            value.GenSimple(context);

            context.ReleaseLocal(value, created);
        }
    }



    internal class GVAR : VAR, ISimple      // Global Variable
    {
        internal GVAR(string vid, YYLTYPE location) : base(vid, location) 
        { 
        }

        internal override string DefinedName()
        {
            return "global-variable";
        }

        internal override void Defined(CodeGenContext context)
        {
            // Variables.gvar_defined(vid);
            context.ldstr(vid.ToString());
            context.call(Runtime.Variables.gvar_defined);
        }

        public void GenSimple(CodeGenContext context)
        {
            // Variables.gvar_get(vid, caller);
            context.ldstr(vid.ToString());
            context.ldloc(0);
            context.call(Runtime.Variables.gvar_get);
        }

        internal override void Assign(CodeGenContext context, Node rhs)
        {
            bool created;
            ISimple value = context.PreCompute(rhs, "rhs", out created);

            //Ruby.Variables.gvar_set(vid, rhs, caller);
            context.ldstr(vid.ToString());
            value.GenSimple(context);
            context.ldloc(0);
            context.call(Runtime.Variables.gvar_set);

            context.ReleaseLocal(value, created);
        }
    }



    internal class IVAR : VAR, ISimple      // Instance Variable
    {
        internal IVAR(string vid, YYLTYPE location) : base(vid.Substring(1), location) { }


        internal override string DefinedName()
        {
            return "instance-variable";
        }

          internal override void Defined(CodeGenContext context)
        {
            // Eval.ivar_defined(recv, vid)
            context.ldarg("recv");
            context.ldstr(vid.ToString());
            context.call(Runtime.Eval.ivar_defined);
        }

        public void GenSimple(CodeGenContext context)
        {
            // Eval.ivar_get(recv, vid)
            context.ldarg("recv");
            context.ldstr(vid.ToString());
            context.call(Runtime.Eval.ivar_get);
        }

        internal override void Assign(CodeGenContext context, Node rhs)
        {
            // object value = rhs;
            bool created;
            ISimple value = context.PreCompute(rhs, "rhs", out created);

            // Eval.ivar_set(caller, recv, "vid", rhs);
            context.ldloc(0);
            context.ldarg("recv");
            context.ldstr(vid.ToString());
            value.GenSimple(context);
            context.call(Runtime.Eval.ivar_set);

            context.ReleaseLocal(value, created);
        }

        public override CodeExpression ToCodeExpression()
        {
            return new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), vid);
        }
    }



    internal class CONST : VAR     // Constant
    {
        internal Node scope = null;
        private bool qualified;
        private Scope parent_scope;

        internal CONST(Scope parent_scope, string vid, YYLTYPE location) : base(vid, location) 
        {
            qualified = false;
            this.parent_scope = parent_scope;
        }

        internal CONST(Scope parent_scope, Node scope, string vid, YYLTYPE location) : base(vid, location) 
        { 
            this.scope = scope;
            this.parent_scope = parent_scope;
            qualified = true;
        }

        public override string ToString()
        {
            if (qualified)
                if (scope != null)
                    return scope.ToString() + "::" + vid;
                else
                    return ":::" + vid;
            else
                return vid;
        }


        internal override string DefinedName()
        {
            return "constant";
        }

         internal override void Defined(CodeGenContext context)
        {
            if (qualified)
                if (scope != null)
                {
                    // object result;
                    int result = context.CreateLocal("result", PrimitiveType.Object);
                    PERWAPI.CILLabel endLabel = context.NewLabel();
                    // try {
                    context.StartBlock(Clause.Try);
                    {
                        // result = Eval.const_defined(scope, vid, caller);
                        scope.GenCode(context);
                        context.ldstr(vid.ToString());
                        context.ldloc(0);
                        context.call(Runtime.Eval.const_defined);
                        context.stloc(result);
                        context.leave(endLabel);
                    }
                    TryBlock block = context.EndTryBlock();
                    // catch (System.Exception) {
                    context.StartBlock(Clause.Catch);
                    {
                        // result = null;
                        context.ldnull();
                        context.stloc(result);
                        context.leave(endLabel);
                    }
                    context.EndCatchBlock(Runtime.SystemExceptionRef, block);

                    context.CodeLabel(endLabel);
                    context.ldloc(result);
                    context.ReleaseLocal(result, true);
                }
                else
                {
                    context.ldsfld(Ruby.Compiler.Runtime.Init.rb_cObject);
                    context.ldstr(vid.ToString());
                    context.ldloc(0);
                    context.call(Runtime.Eval.const_defined);
                }
            else
            {
                context.ruby_cbase(parent_scope);
                context.ldstr(vid.ToString());
                context.ldloc(0);
                context.call(Runtime.Eval.const_defined);
            }
        }


        internal override void GenCode0(CodeGenContext context)
        {
            if (qualified)
                if (scope != null)
                    scope.GenCode(context);
                else
                    context.ldsfld(Ruby.Compiler.Runtime.Init.rb_cObject);
            else
                context.ruby_cbase(parent_scope);

            context.ldstr(vid.ToString());
            context.ldloc(0);
            context.call(Runtime.Eval.get_const);
        }


        internal override void Assign(CodeGenContext context, Node rhs)
        {
            bool value_created;
            ISimple value;

            if (qualified) // Fixme: scope == null???
            {
                // object where = scope;
                bool where_created;
                ISimple where = context.PreCompute(scope, "scope", out where_created);
            
                // object value = rhs;
                value = context.PreCompute(rhs, "rhs", out value_created);

                context.ldloc(0);
                where.GenSimple(context);

                context.ReleaseLocal(where, where_created);
            }
            else
            {
                // object value = rhs;
                value = context.PreCompute(rhs, "rhs", out value_created);

                context.ldloc(0);
                context.ruby_cbase(parent_scope);
            }

            context.ldstr(vid.ToString());
            value.GenSimple(context);
            context.call(Runtime.Eval.set_const);

            context.ReleaseLocal(value, value_created);
        }
    }



    internal class CVAR : VAR, ISimple      // Class Variable
    {
        private Scope parent_scope;

        internal CVAR(Scope parent_scope, string vid, YYLTYPE location)
            : base(vid, location) 
        {
            this.parent_scope = parent_scope;
        }

        public void GenSimple(CodeGenContext context)
        {
            // Fixme: make sure CurrentRubyClass is not a singleton (see cvar_cbase)
            // Ruby.Variables.cvar_get(caller, ruby_cref, "vid");
            context.ldloc(0);
            context.ruby_cbase(parent_scope);
            context.ldstr(vid.ToString());
            context.call(Runtime.Variables.cvar_get);
        }

        internal override void Assign(CodeGenContext context, Node rhs)
        {
            // Fixme: make sure CurrentRubyClass is not a singleton (see cvar_cbase)

            // object value = rhs;
            bool created;
            ISimple value = context.PreCompute(rhs, "rhs", out created);

            // Ruby.Variables.cvar_set(caller, ruby_cref, "vid", value);
            context.ldloc(0);
            context.ruby_cbase(parent_scope);
            context.ldstr(vid.ToString());
            value.GenSimple(context);
            context.call(Runtime.Variables.cvar_set);

            context.ReleaseLocal(value, created);
        }

          internal override void Defined(CodeGenContext context)
        {
            // Fixme: make sure CurrentRubyClass is not a singleton (see cvar_cbase)
            // Ruby.Variables.cvar_defined(ruby_cref, "vid");
            context.ruby_cbase(parent_scope);
            context.ldstr(vid.ToString());
            context.call(Runtime.Variables.cvar_defined);
        }

        internal override string DefinedName()
        {
            return "class variable";
        }
    }



    internal class BACK_REF : Node
    {
        // $&, $`, $\, $+

        internal char ch;

        internal BACK_REF(Scope current, char ch, YYLTYPE location)
            : base(location)
        {
            this.ch = ch;
        }

        internal override void GenCode0(CodeGenContext context)
        {
            context.ldloc(0);
            context.call(Runtime.Frame.get_Tilde);
            PERWAPI.CILLabel label1 = context.NewLabel();
            context.brfalse(label1);
            context.ldloc(0);
            context.call(Runtime.Frame.get_Tilde);
            context.ldloc(0);

            switch (ch)
            {
                case '&':
                    context.callvirt(Runtime.Match.last_match);
                    break;
                case '`':
                    context.callvirt(Runtime.Match.match_pre);
                    break;               
                case '\'':
                    context.callvirt(Runtime.Match.match_post);
                    break;                
                case '+':
                    context.callvirt(Runtime.Match.match_last);
                    break;
                default:
                    throw new NotImplementedException("BACK_REF $" + ch);
            }
            PERWAPI.CILLabel label2 = context.NewLabel();
            context.br(label2);
            context.CodeLabel(label1);
            context.ldnull();
            context.CodeLabel(label2);
        }

        public override CodeExpression ToCodeExpression()
        {
            return new CodeVariableReferenceExpression("$" + ch);
        }
    }



    internal class NTH_REF : Node    // Numbered variable
    {
        // $nth

        internal int nth;

        internal NTH_REF(Scope current, int nth, YYLTYPE location)
            : base(location)
        {
            this.nth = nth;
        }

        internal override void GenCode0(CodeGenContext context)
        {
            CILLabel endLabel = context.NewLabel();

            context.ldloc(0);
            context.call(Runtime.Frame.get_Tilde);
            context.dup();
            context.brfalse(endLabel);
            context.ldc_i4(nth);
            context.callvirt(Runtime.Match.get_nth);
            context.CodeLabel(endLabel);
        }

        internal override void Defined(CodeGenContext context)
        {
            CILLabel endLabel = context.NewLabel();
        
            context.ldloc(0);
            context.call(Runtime.Frame.get_Tilde);
            context.dup();
            context.brfalse(endLabel);
            context.ldc_i4(nth);
            context.callvirt(Runtime.Match.defined_nth);
            context.CodeLabel(endLabel);
        }

        internal override string DefinedName()
        {
            return "$" + nth;
        }

        public override CodeExpression ToCodeExpression()
        {
            return new CodeVariableReferenceExpression("$" + nth.ToString());
        }
    }



    internal class ATTRIBUTE : VAR
    {
        // scope::vid

        private Node attr_scope = null;

        internal ATTRIBUTE(Node attr_scope, string vid, YYLTYPE location)
            : base(vid, location)
        {
            this.attr_scope = attr_scope;
        }

        internal override void Assign(CodeGenContext context, Node rhs)
        {
            new METHOD_CALL(attr_scope, vid+"=", rhs, location).GenCode(context);
        }

        public override CodeExpression ToCodeExpression()
        {
            return new CodeFieldReferenceExpression(attr_scope.ToCodeExpression(), vid);
        }

        internal override string  DefinedName()
        {
            return "assignment";
        }
    }



    internal class ARRAY_ACCESS : LVALUE
    {
        // array [ args ]

        protected Node array;
        protected ListGen args;

        internal ARRAY_ACCESS(Node array, Node args, YYLTYPE location): base(location)
        {
            Debug.Assert(array != null);
            this.array = array;
            this.args = (ListGen)args;
        }

        internal override void GenCode0(CodeGenContext context)
        {
            // array.AREF(args)
            new METHOD_CALL(array, ID.intern(Tokens.tAREF), (Node)args, location).GenCode(context);
        }


        private Node rhs;


        internal override void Assign(CodeGenContext context, Node rhs)
        {
            this.rhs = rhs;
            // a.ASET(index++rhs); 
            new METHOD_CALL(array, ID.intern(Tokens.tASET), new ProxyList(ArgsPlusRHS, location), location).GenCode(context);
        }



        private ISimple index;

        internal void AssignOp(CodeGenContext context, string op, Node rhs)
        {
            // object a = array;
            bool a_created;
            ISimple a = context.PreCompute(array, "array", out a_created);

            // ArgList index = args;
            bool index_created;
            index = args.GenArgList(context, out index_created);

            // access <=> a[index];
            ARRAY_ACCESS access = new ARRAY_ACCESS((Node)a, new ProxyList(Index, location), location);

            // access1 = (access2 op rhs);
            access.Assign(context, METHOD_CALL.Create(access, op, rhs, location));

            context.ReleaseLocal(a, a_created);
            context.ReleaseLocal(index, index_created);
        }


        private ISimple Index(CodeGenContext context, out bool created)
        {
            created = false;
            return index;
        }


        // append rhs to args array
        private ISimple ArgsPlusRHS(CodeGenContext context, out bool created)
        {
            // ArgList temp = args;
            ISimple temp = args.GenArgList(context, out created);

            // object value = rhs;
            bool value_created;
            ISimple value = context.PreCompute(rhs, "rhs", out value_created);

            // temp.Add(value);
            temp.GenSimple(context);
            value.GenSimple(context);
            context.callvirt(Runtime.ArgList.Add);

            context.ReleaseLocal(value, value_created);

            return temp;
        }

        public override CodeExpression ToCodeExpression()
        {
            CodeArrayIndexerExpression access = new CodeArrayIndexerExpression();
            access.TargetObject = array.ToCodeExpression();

            for (Node n = args; n != null; n = n.nd_next)
                access.Indices.Add(n.ToCodeExpression());

            return access;
        }
    }
}