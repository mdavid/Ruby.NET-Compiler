using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using Ruby.Methods;
using Ruby.Runtime;
using Ruby;


namespace Ruby.Interop
{
    internal class CLRClass: Class
    {
        private System.Type clrtype;

        internal CLRClass(System.Type clrtype, Class superclass, Type type): base(clrtype.Name, superclass, type)
        {
            this.clrtype = clrtype;
        }

        internal override bool get_method(string methodId, out RubyMethod method, out Class klass)
        {
            method = FindCLRMethod(methodId, clrtype);
            if (method != null)
            {
                klass = this;
                return true;
            }
            else
            {
                if (base.get_method(methodId, out method, out klass))
                    return true;

                if (Init.rb_cClass.get_method(methodId, out method, out klass))
                    return true;

                if (Init.rb_cModule.get_method(methodId, out method, out klass))
                    return true;

                if (Init.rb_cObject.get_method(methodId, out method, out klass))
                    return true;

                return false;
            }
        }

        internal override object const_get(string name, bool recurse, Frame caller)
        {
            if (instance_vars.ContainsKey(name))
                return instance_vars[name];

            Class c = super;

            while (c != null)
            {
                if (c.instance_vars.ContainsKey(name))
                    return c.instance_vars[name];

                c = c.super;
            }

            if (Init.rb_cClass.instance_vars.ContainsKey(name))
                return Init.rb_cClass.instance_vars[name];

            if (Init.rb_cModule.instance_vars.ContainsKey(name))
                return Init.rb_cModule.instance_vars[name];

            if (Init.rb_cObject.instance_vars.ContainsKey(name))
                return Init.rb_cObject.instance_vars[name];

            return Eval.CallPrivate(this, caller, "const_missing", null, new Symbol(name));
        }

        private RubyMethod FindCLRMethod(string methodId, System.Type clrtype)
        {
            BindingFlags flags = BindingFlags.Instance | BindingFlags.Public;
            if (this._type == Type.IClass)
            {
                if (methodId == "new")
                {
                    if (clrtype.IsSubclassOf(typeof(System.Delegate)))
                    {
                        return new RubyMethod(new DelegateConstructor(clrtype), 0, Access.Public, this);
                    }
                    else
                    {
                        ConstructorInfo[] ci = clrtype.GetConstructors();

                        if (ci == null || ci.Length == 0)
                            return null;

                        MethodBase[] mi = new MethodBase[ci.Length];
                        System.Array.Copy(ci, mi, ci.Length);
                        return new RubyMethod(new MultiMethod(mi), -1, Access.Public, this);
                    }
                }
                else if (methodId == "allocator")
                {
                    return new RubyMethod(Methods.rb_class_allocate_instance.singleton, 0, Access.Private, this);
                }

                flags = BindingFlags.Static | BindingFlags.Public;
            }

            if (methodId == "initialize")
                return new RubyMethod(Methods.rb_obj_dummy.singleton, 0, Access.Private, this);

            bool is_setter = false;
            // methods ending with "=" are expected to be either
            // field or property setters
            if (methodId.EndsWith("="))
            {
                is_setter = true;
                methodId = methodId.Substring(0, methodId.Length - 1);
            }

            if (methodId == "[]")
            {
                object[] attributes = clrtype.GetCustomAttributes(
                    typeof(System.Reflection.DefaultMemberAttribute), true);

                if (attributes.Length > 0)
                {
                    methodId = ((DefaultMemberAttribute)attributes[0]).MemberName;
                }
            }

            MemberInfo[] members = clrtype.GetMember(methodId, flags);

            if (members == null || members.Length == 0)
                return null;

            if (members[0] is MethodBase)
            {
                if (is_setter)
                    return null;

                MethodBase[] methods = new MethodBase[members.Length];
                System.Array.Copy(members, methods, members.Length);
                return new RubyMethod(new MultiMethod(methods), -1, Access.Public, this);
            }

            if (members[0] is PropertyInfo)
            {
                // not all the property overloads may have the getter/setter
                // we're looking for, so we maintain a count and resize
                // the methods array later if necessary
                int count = 0;
                MethodBase[] methods = new MethodBase[members.Length];
                
                foreach (PropertyInfo pi in members)
                {
                    MethodInfo method = is_setter ? pi.GetSetMethod() : pi.GetGetMethod();
                    if (method != null)
                        methods[count++] = method;
                }
                
                if (count == 0)
                    return null;

                if (count < members.Length)
                    System.Array.Resize(ref methods, count);

                return new RubyMethod(new MultiMethod(methods), -1, Access.Public, this);
            }

            FieldInfo field = members[0] as FieldInfo;
            if (field != null)
            {
                if (is_setter)
                    return new RubyMethod(new FieldSetter(field), 1, Access.Public, this);
                else
                    return new RubyMethod(new FieldGetter(field), 0, Access.Public, this);
            }

            //EventInfo eventinfo = members[0] as EventInfo;
            //if (eventinfo != null)
            //{
            //    return ...;
            //}

            //Type type = members[0] as Type;
            //TODO: nested types?

            return null;
        }

        internal static void LoadCLRAssembly(System.Reflection.Assembly Assembly, Frame caller)
        {
            foreach (System.Type type in Assembly.GetExportedTypes())
                Load(type, caller);
        }

        internal static Dictionary<System.Type, Class> CLRTypes = new Dictionary<System.Type, Class>();

        internal static Class Load(System.Type type, Frame caller)
        {
            if (type == null)
                return null;

            Class context = Ruby.Runtime.Init.rb_cObject;
            if (type.Namespace != null)
                foreach (string Namespace in type.Namespace.Split('.'))
                {
                    object innerContext;
                    if (context.const_defined(Namespace, false) && (innerContext = context.const_get(Namespace, caller)) is Class)
                        context = (Class)innerContext;
                    else
                        context.define_const(Namespace, context = new CLRNamespace(Namespace));
                }
            
            if (CLRTypes.ContainsKey(type))
                return CLRTypes[type];
            else
            {
            Class klass = new CLRClass(type, Load(type.BaseType, caller), Type.Class);
            Class meta = new CLRClass(type, klass.super, Type.IClass);
            klass.my_class = meta;
            CLRTypes[type] = klass;
            context.define_const(type.Name, klass);
            return klass; 
            }
        }
    }

    internal class FieldGetter : Ruby.Runtime.MethodBody0
    {
        FieldInfo field;
        internal FieldGetter(FieldInfo field)
        {
            this.field = field;
        }

        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {
            return field.GetValue(recv);
        }
    }

    internal class FieldSetter : Ruby.Runtime.MethodBody1
    {
        FieldInfo field;
        internal FieldSetter(FieldInfo field)
        {
            this.field = field;
        }

        public override object Call1(Class last_class, object recv, Frame caller, Proc block, object p1)
        {
            field.SetValue(recv, p1);
            return p1;
        }
    }

    internal class MultiMethod : Ruby.Runtime.MethodBody
    {
        MethodBase[] methods;

        internal MultiMethod(MethodBase[] methods)
        {
            this.methods = methods;
        }

        internal enum MatchResult
        {
            NoMatch,
            Exact,
            BasicConversion
        }

        private static MatchResult MatchArgument(System.Type parameter, object arg)
        {
            if (parameter.IsInstanceOfType(arg))
                return MatchResult.Exact;
            if (arg is Basic && parameter.IsInstanceOfType(((Basic)arg).Inner()))
                return MatchResult.BasicConversion;
            if (arg == null && !parameter.IsValueType)
                return MatchResult.Exact;
            return MatchResult.NoMatch;
        }

        private MethodBase MatchArguments(object[] args, out MatchResult[] conversions)
        {
            conversions = new MatchResult[args.Length];

            foreach (MethodBase method in methods)
            {
                ParameterInfo[] parameters = method.GetParameters();
                if (parameters.Length != args.Length)
                    continue;

                for (int i = 0; i < args.Length; i++)
                {
                    MatchResult result = MatchArgument(parameters[i].ParameterType, args[i]);
                    if (result == MatchResult.NoMatch)
                        goto next_method;
                    else
                        conversions[i] = result;
                }

                return method;

                next_method: ;
            }

            return null;
        }

        public override object Calln(Class last_class, object recv, Frame caller, ArgList args)
        {
            object[] out_args = args.ToArray(); 
            MatchResult[] conversions;
            MethodBase method = MatchArguments(out_args, out conversions);

            if (method != null)
            {
                for (int i = 0; i < args.Length; i++)
                {
                    switch (conversions[i])
                    {
                        case MatchResult.Exact:
                            break;
                        case MatchResult.BasicConversion:
                            out_args[i] = ((Basic)out_args[i]).Inner();
                            break;
                        default:
                            throw new System.NotSupportedException();
                    }
                }

                try
                {
                    if (!method.IsConstructor)
                        return method.Invoke(recv, out_args);
                    else
                        return ((ConstructorInfo)method).Invoke(out_args);
                }
                catch (System.Reflection.TargetInvocationException e)
                {
                    throw e.InnerException;
                }
            }
            else
            {
                throw new Exception(
                    string.Format("couldn't match arguments to ({0}:{1})",
                    this.methods[0].DeclaringType.FullName,
                    this.methods[0].Name)).raise(caller);
            }
        }
    }

    internal class DelegateConstructor : Ruby.Runtime.MethodBody0
    {
        public DelegateConstructor(System.Type delegateType)
        {
            this.delegateType = delegateType;
        }

        System.Type delegateType;
        DynamicMethod wrapper;

        internal DynamicMethod BuildWrapper()
        {
            MethodInfo mi = delegateType.GetMethod("Invoke");
            ParameterInfo[] param = mi.GetParameters();
            System.Type[] args = new System.Type[param.Length + 1];
            args[0] = typeof(Ruby.Proc);

            for (int i = 0; i < param.Length; i++)
                args[i + 1] = param[i].ParameterType;

            DynamicMethod dm = new DynamicMethod(
                "proc_invoker", mi.ReturnType, args, typeof(Ruby.Proc), true);
            ILGenerator il = dm.GetILGenerator();
            
            il.Emit(OpCodes.Ldarg_0); // proc
            il.Emit(OpCodes.Ldnull); // caller

            il.Emit(OpCodes.Ldc_I4, param.Length); // params object[] args
            il.Emit(OpCodes.Newarr, typeof(System.Object));

            for (int i = 0; i < param.Length; i++)
            {
                il.Emit(OpCodes.Dup);
                il.Emit(OpCodes.Ldc_I4, i);
                il.Emit(OpCodes.Ldarg, i + 1);
                if (param[i].ParameterType.IsValueType)
                    il.Emit(OpCodes.Box, param[i].ParameterType);
                il.Emit(OpCodes.Stelem, typeof(System.Object));
            }

            il.Emit(OpCodes.Call, typeof(Ruby.Proc).GetMethod(
                "yield", BindingFlags.Instance | BindingFlags.NonPublic));

            if (mi.ReturnType == typeof(void))
                il.Emit(OpCodes.Pop);
            else if (mi.ReturnType.IsValueType)
                il.Emit(OpCodes.Unbox, mi.ReturnType);
            else
                il.Emit(OpCodes.Castclass, mi.ReturnType);

            il.Emit(OpCodes.Ret);

            return dm;
        }

        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {
            if (wrapper == null)
                wrapper = BuildWrapper();

            return wrapper.CreateDelegate(delegateType, block);
        }
    }

    internal class CLRNamespace : Class
    {
        internal CLRNamespace(string Namespace)
            : base(Namespace, null, Type.Module)
        {
        }
    }
}
