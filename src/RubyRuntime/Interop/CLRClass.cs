using System.Collections.Generic;
using System.Reflection;
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
            if (this._type == Type.IClass)
            {
                if (methodId == "new")
                    return new CLRMethod(new List<MethodBase>(clrtype.GetConstructors(BindingFlags.Public | BindingFlags.Instance)), this);
                else if (methodId == "allocator")
                    return new RubyMethod(Methods.rb_class_allocate_instance.singleton, 0, Access.Private, this);

                List<MethodBase> methods = new List<MethodBase>(clrtype.GetMethods(BindingFlags.Public | BindingFlags.Static)).FindAll(delegate(MethodBase item) { return item.Name == methodId; });

                if (methods.Count > 0)
                    return new CLRMethod(methods, this);
                else
                    return null;
            }
            else
            {
                if (methodId == "initialize")
                    return new RubyMethod(Methods.rb_obj_dummy.singleton, 0, Access.Private, this);

                List<MethodBase> methods = new List<MethodBase>(clrtype.GetMethods(BindingFlags.Public | BindingFlags.Instance)).FindAll(delegate(MethodBase item) { return item.Name == methodId; });

                if (methods.Count > 0)
                    return new CLRMethod(methods, this);
                else
                    return null;
            }
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

    internal class CLRMethod : RubyMethod
    {
        private List<MethodBase> methods;

        internal CLRMethod(List<MethodBase> methods, Class definingClass)
            : base(new CLRMethodBody(methods), -1, Access.Public, definingClass)
        {
            this.methods = methods;
        }

        internal static int Matches(System.Type parameter, object arg)
        {
            if (parameter.IsInstanceOfType(arg))
                return 1;
            if (arg is Basic && parameter.IsInstanceOfType(((Basic)arg).Inner()))
                return 0;
            if (arg == null && parameter.IsValueType)
                return 1;
            return -1;
        }
    }


    internal class CLRMethodBody : Ruby.Runtime.MethodBody
    {
        private List<MethodBase> methods;

        internal CLRMethodBody(List<MethodBase> methods)
        {
            this.methods = methods;
        }

        public override object Calln(Class last_class, object recv, Frame caller, ArgList args)
        {
            MethodBase matching_method = null;
            bool[] conversion = null;
            bool conversions = false;
            foreach (MethodBase method in methods)
            {
                ParameterInfo[] parameters = method.GetParameters();
                if (parameters.Length != args.Length)
                    continue;
                conversion = new bool[args.Length];
                bool match = true;
                conversions = false;
                for (int i = 0; i < args.Length; i++)
                {
                    int test = CLRMethod.Matches(parameters[i].ParameterType, args[i]);
                    if (test < 0)
                    {
                        match = false;
                        break;
                    }
                    if (test == 0)
                    {
                        conversion[i] = true;
                        conversions = true;
                    }
                }
                if (!match)
                    continue;

                matching_method = method;
                break;
            }

            if (matching_method != null)
            {
                if (conversions)
                    for (int i = 0; i< args.Length; i++)
                        if (conversion[i])
                            args[i] = ((Basic)args[i]).Inner();

                try
                {
                    if (matching_method.IsConstructor)
                        return ((ConstructorInfo)matching_method).Invoke(args.ToArray());
                    else
                        return matching_method.Invoke(recv, args.ToArray());
                }
                catch (System.Reflection.TargetInvocationException e)
                {
                    throw e.InnerException;
                }
            }
            else
                throw new Exception("matching method not found").raise(caller);
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
