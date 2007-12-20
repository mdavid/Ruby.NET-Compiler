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
        internal readonly System.Type clrtype;

        internal CLRClass(System.Type clrtype, Class superclass, Type type): base(clrtype.Name, superclass, type)
        {
            this.clrtype = clrtype;
        }

        internal override bool get_method(string methodId, out RubyMethod method, out Class klass)
        {
            if (_methods.TryGetValue(methodId, out method))
            {
                klass = this;
                return true;
            }

            if (null != (method = FindCLRMethod(methodId, clrtype)))
            {
                klass = this;
                this._methods[methodId] = method;
                return true;
            }

            if (super != null && super.get_method(methodId, out method, out klass))
                return true;

            klass = null;
            return false;
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
                // static methods
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
                else if (methodId == "[]")
                {
                    // instantiate a generic type
                    // ruby: type = ns::List[System::Int32]
                    if (clrtype.IsGenericType)
                    {
                        // clrtype is Type`n+Inner but we are looking for Type`n+Inner`n
                        // we need to strip generic arguments from the name, but supply
                        // them to the GenericTypeGetter
                        return new RubyMethod(
                            new GenericTypeGetter(
                                clrtype.Assembly,
                                clrtype.GetGenericTypeDefinition().FullName,
                                clrtype.GetGenericArguments()),
                            -1, Access.Public, this);
                    }
                    else
                    {
                        return new RubyMethod(
                            new GenericTypeGetter(clrtype.Assembly, clrtype.FullName, null),
                            -1, Access.Public, this);
                    }
                }

                flags = BindingFlags.Static | BindingFlags.Public;
            }

            bool is_setter = false;
            // methods ending with "=" are expected to be either
            // field or property setters
            if (methodId.EndsWith("="))
            {
                is_setter = true;
                methodId = methodId.Substring(0, methodId.Length - 1);
            }

            // default member access, an Indexer in C#
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

            if (members.Length == 0)
            {
                // we didn't find a member with the exact name
                // but we still need to check for nested types with
                // additional type parameters
                string genericNestedId = methodId + "`";
                foreach (System.Type nested in clrtype.GetNestedTypes(flags))
                {
                    if (nested.Name.StartsWith(genericNestedId))
                    {
                        return new RubyMethod(
                            new ValueMethod(
                                new GenericContainer(
                                    clrtype.Assembly, clrtype.Name + "+" + methodId,
                                    clrtype.GetGenericArguments())),
                            0, Access.Public, this);
                    }
                }

                return null;
            }

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

            // nested types
            System.Type type = members[0] as System.Type;
            if (type != null)
            {
                // see section 10.7.1 of ECMA
                if (type.IsGenericTypeDefinition)
                    type = type.MakeGenericType(clrtype.GetGenericArguments());

                return new RubyMethod(
                    new NestedTypeGetter(Load(type, null, false)),
                    0, Access.Public, this);
            }

            return null;
        }

        internal static void LoadCLRAssembly(System.Reflection.Assembly Assembly, Frame caller)
        {
            foreach (System.Type type in Assembly.GetExportedTypes())
                Load(type, caller, true);
        }

        private static Dictionary<System.Type, CLRClass> cache =
            new Dictionary<System.Type, CLRClass>();

        internal static bool TryLoad(System.Type type, out CLRClass klass)
        {
            // TODO: (reader) lock ?
            return cache.TryGetValue(type, out klass);
        }

        internal static CLRClass Define(System.Type type, CLRClass klass)
        {
            // TODO: (writer) lock ?
            CLRClass existing;
            if (cache.TryGetValue(type, out existing))
                return existing;

            cache[type] = klass;
            return klass;
        }

        internal static CLRClass Load(System.Type type, Frame caller, bool makeConstant)
        {
            if (type == null)
                return null;

            CLRClass klass;
            if (!TryLoad(type, out klass))
            {
                System.Type baseType = type.BaseType;
                CLRClass baseClass = baseType == null 
                    ? null
                    : Load(baseType, caller, makeConstant && baseType.Assembly == type.Assembly);

                if (baseClass != null)
                {
                    klass = new CLRClass(type, baseClass, Type.Class);
                    klass.my_class = new CLRClass(type, baseClass.my_class, Type.IClass);

                    System.Type[] inherited = baseType.GetInterfaces();
                    foreach (System.Type iface in type.GetInterfaces())
                    {
                        if (0 > System.Array.IndexOf(inherited, iface))
                            Class.rb_include_module(caller, klass, Load(iface, caller, false));
                    }
                }
                else
                {
                    if (type.IsInterface)
                    {
                        klass = new CLRClass(type, null, Type.Module);
                        klass.my_class = new CLRClass(type, Init.rb_cModule, Type.IClass);
                    }
                    else
                    {
                        klass = new CLRClass(type, Init.rb_cObject, Type.Class);
                        klass.my_class = new CLRClass(type, Init.rb_cClass, Type.IClass);
                    }

                    foreach (System.Type iface in type.GetInterfaces())
                        Class.rb_include_module(caller, klass, Load(iface, caller, false));
                }

                Augmentations.Augment(type, klass, caller);
                klass = Define(type, klass);
            }

            if (makeConstant)
            {
                Class context = context = Ruby.Runtime.Init.rb_cObject;
                if (type.Namespace != null)
                {
                    foreach (string Namespace in type.Namespace.Split('.'))
                    {
                        object innerContext;
                        if (context.const_defined(Namespace, false) && (innerContext = context.const_get(Namespace, caller)) is Class)
                            context = (Class)innerContext;
                        else
                            context.define_const(Namespace, context = new CLRNamespace(Namespace));
                    }
                }

                if (type.IsNested)
                {
                    // nested types can be loaded by a getter in their owner
                    // ruby: Namespace::Type.InnerType
                }
                else if (type.IsGenericTypeDefinition)
                {
                    string name = type.Name.Substring(0, type.Name.LastIndexOf('`'));
                    string containerName =
                        type.Namespace == null
                        ? name
                        : type.Namespace + "." + name;

                    // if a non-generic type exists with the same name
                    // we can use that in the construction of a generic type
                    System.Type container = type.Assembly.GetType(containerName);

                    // otherwise we much build a GenericContainer
                    if (container == null && !context.const_defined(name))
                    {
                        context.define_const(name,
                            new GenericContainer(type.Assembly, containerName, null));
                    }
                }
                else
                {
                    context.define_const(type.Name, klass);
                }
            }

            return klass;
        }
    }

    internal class NestedTypeGetter : Ruby.Runtime.MethodBody0
    {
        CLRClass klass;
        internal NestedTypeGetter(CLRClass klass)
        {
            this.klass = klass;
        }

        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {
            return klass;
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

    // given a name and assembly, this instantiates a generic type
    // based on the arguments / arity of the method call
    internal class GenericTypeGetter : Ruby.Runtime.MethodBody
    {
        private Assembly assembly;
        private string fullname;
        System.Type[] closedArguments;

        internal GenericTypeGetter(Assembly assembly, string fullname, System.Type[] closedArguments)
        {
            this.assembly = assembly;
            this.fullname = fullname;
            this.closedArguments = closedArguments;
        }

        public override object Calln(Class last_class, object recv, Frame caller, ArgList args)
        {
            if (args.Length == 0)
                throw new System.ArgumentException();

            string name = fullname + "`" + args.Length;
            System.Type type = assembly.GetType(name);

            if (type == null)
                throw new System.TypeLoadException();

            int extra = closedArguments == null ? 0 : closedArguments.Length;
            System.Type[] typeArguments = new System.Type[extra + args.Length];

            if (extra > 0)
                System.Array.Copy(closedArguments, typeArguments, extra);

            for (int i = 0; i < args.Length; i++)
            {
                System.Type typeArg = args[i] as System.Type;
                if (typeArg != null)
                {
                    typeArguments[extra + i] = typeArg;
                    continue;
                }

                CLRClass klass = args[i] as CLRClass;
                if (klass != null)
                {
                    typeArguments[extra + i] = klass.clrtype;
                    continue;
                }

                // TODO: support ruby types?
                throw new System.ArgumentException();
            }

            return CLRClass.Load(type.MakeGenericType(typeArguments), caller, false);
        }
    }

    internal class ValueMethod : Ruby.Runtime.MethodBody0
    {
        public ValueMethod(object value)
        {
            this.value = value;
        }

        object value;

        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {
            return value;
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
            BasicConversion,
            ProcConversion,
            ArrayConversion
        }

        private static MatchResult MatchArgument(System.Type parameter, object arg)
        {
            if (parameter.IsInstanceOfType(arg))
                return MatchResult.Exact;
            if (arg is Basic && parameter.IsInstanceOfType(((Basic)arg).Inner()))
                return MatchResult.BasicConversion;
            if (arg == null && !parameter.IsValueType)
                return MatchResult.Exact;
            if (arg is Proc && parameter.IsSubclassOf(typeof(System.Delegate)))
                return MatchResult.ProcConversion;
            if (arg is Array && (parameter.IsSubclassOf(typeof(System.Array))))
                return MatchResult.ArrayConversion;
            return MatchResult.NoMatch;
        }

        private bool MatchArguments(object[] args, MatchResult[] conversions, ParameterInfo[] parameters)
        {
            for (int i = 0; i < args.Length; i++)
            {
                if (MatchResult.NoMatch == (conversions[i] = MatchArgument(parameters[i].ParameterType, args[i])))
                    return false;
            }
            return true;
        }

        private MethodBase MatchArguments(object[] args, out MatchResult[] conversions,
            out ParameterInfo[] parameters)
        {
            conversions = new MatchResult[args.Length];
            parameters = null;

            foreach (MethodBase method in methods)
            {
                parameters = method.GetParameters();
                if (parameters.Length != args.Length)
                    continue;

                if (MatchArguments(args, conversions, parameters))
                    return method;
            }

            return null;
        }

        private object ArrayConversion(Array src, System.Type arrayType)
        {
            System.Type elementType = arrayType.GetElementType();
            System.Array dst = System.Array.CreateInstance(elementType, src.Count);

            for (int i = 0; i < src.Count; i++)
            {
                object o = src[i];
                MatchResult result = MatchArgument(elementType, o);

                if (result == MatchResult.NoMatch)
                    throw new System.NotSupportedException();

                dst.SetValue(ConvertArgument(o, result, elementType), i);
            }

            return dst;
        }

        private object ConvertArgument(object arg, MatchResult conversion, System.Type parameter)
        {
            switch (conversion)
            {
                case MatchResult.Exact:
                    return arg;
                case MatchResult.BasicConversion:
                    return ((Basic)arg).Inner();
                case MatchResult.ProcConversion:
                    return DelegateConstructor.Convert((Proc)arg, parameter);
                case MatchResult.ArrayConversion:
                    return ArrayConversion((Array)arg, parameter);
                default:
                    throw new System.NotSupportedException();
            }
        }

        private void ConvertArguments(object[] out_args, MatchResult[] conversions, ParameterInfo[] parameters)
        {
            for (int i = 0; i < out_args.Length; i++)
                out_args[i] = ConvertArgument(out_args[i], conversions[i], parameters[i].ParameterType);
        }

        public override object Calln(Class last_class, object recv, Frame caller, ArgList args)
        {
            object[] out_args;

            if (args.block != null)
            {
                out_args = new object[args.Length + 1];
                for (int i = 0; i < args.Length; i++)
                    out_args[i] = args[i];

                out_args[args.Length] = args.block;
            }
            else
            {
                out_args = args.ToArray();
            }

            MatchResult[] conversions;
            ParameterInfo[] parameters;
            MethodBase method = MatchArguments(out_args, out conversions, out parameters);

            if (method != null)
            {
                ConvertArguments(out_args, conversions, parameters);

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

        internal static object Convert(Proc block, System.Type delegateType)
        {
            // TODO: we should actually be caching
            // these DelegateConstructors somewhere
            DelegateConstructor d = new DelegateConstructor(delegateType);
            return d.BuildWrapper().CreateDelegate(delegateType, block);
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
            this.my_class = Init.rb_cModule;
        }
    }

    // this is used in the case where there exists a type T`n but no T
    internal class GenericContainer : Class
    {
        internal GenericContainer(Assembly assembly, string name, System.Type[] arguments)
            : base(name, null, Type.Module)
        {
            this.define_module_function("[]", new GenericTypeGetter(assembly, name, arguments), -1, null);
        }
    }
}
