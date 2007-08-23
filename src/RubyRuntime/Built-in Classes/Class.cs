/**********************************************************************

  Ruby.NET Runtime Library
  Originally developed at Queensland University of Technology
 
  Some sections of this C# code mirror the structure of the C code in the
  Ruby 1.8.2 Interpreter Copyright (C) 1993-2003 Yukihiro Matsumoto, et.al.
  
**********************************************************************/

using Ruby.Runtime;
using System.Diagnostics;
using System.Collections.Generic;
using System.Collections;
using System;
using System.Globalization;

namespace Ruby
{
    [UsedByRubyCompiler]
    public partial class Class : Object
    {
        internal enum Type { Singleton, Class, IClass, Module };

        internal string _name;
        internal object attached; // only used for singletons
        internal Type _type;
        internal Class super;
        internal Dictionary<string, RubyMethod> _methods = new Dictionary<string, RubyMethod>();
        internal Access scope_vmode = Access.Public;
        internal string classpath; // the cached classpath for this class. If this is not set, then it is constructed dynamically
        internal string tmp_classpath;
        internal Dictionary<string, string> autoload_tbl = new Dictionary<string, string>();


        // -----------------------------------------------------------------------------

        // BBTAG: equivalent to NODE_MEMO in CRuby for autoload constants: these are just 'stubs'
        // that allow us to insert autoload constants into the instance_vars table
        internal class NodeAutoloadMemo
        {
            internal string filename;

            internal NodeAutoloadMemo(string filename)
            {
                this.filename = filename;
            }
        }

        // -----------------------------------------------------------------------------

        internal Class(string name, Class superclass, Type type)
            : base(Ruby.Runtime.Init.rb_cClass)
        {
            this._name = name;
            this._type = type;
            this.super = superclass;
        }

        internal Class(string name, Class superclass, Type type, Class klass)
            : base(klass)
        {
            this._name = name;
            this._type = type;
            this.super = superclass;
        }

        protected Class(Class klass) : base(klass) 
        { 
        }


        // -----------------------------------------------------------------------------


        // BBTAG: helper method to get the nearest non-virtual super class
        internal Class super_real()
        {
            if (super != null && super._type != Type.IClass && super._type != Type.Singleton)
                return super;
            else
                if (super != null)
                    return super.super_real();
                else
                    return null;
        }


        internal object class_inherited(Class super, Frame caller)
        {
            if (super == null)
                super = Ruby.Runtime.Init.rb_cObject;

            return Eval.CallPrivate(super, caller, "inherited", null, this);
        }


        internal Class class_real() //status: done
        {
            if (_type == Type.IClass || _type == Type.Singleton)
                return super.class_real();
            else
                return this;
        }


        internal bool get_method(string methodId, out RubyMethod method)
        {
            Class klass;
            return get_method(methodId, out method, out klass);
        }


        internal virtual bool get_method(string methodId, out RubyMethod method, out Class klass)
        {
            if (_methods.TryGetValue(methodId, out method))
            {
                klass = this;
                return true;
            }

            Class superKlass = super;
            if (superKlass != null)
            {
                // BBTAG: calling new for a subclass of a CLRClass is a special case
                if (this._type == Type.Singleton && superKlass is Interop.CLRClass && !(this is Interop.CLRClass) && methodId == "new")
                {
                    klass = this;
                    method = new RubyMethod(Ruby.Methods.rb_class_new_instance.singleton, -1, Access.Public, this);
                    return true;
                }

                if (superKlass.get_method(methodId, out method, out klass))
                    return true;
            }

            klass = null;
            return false;
        }

        // In CRuby this is not just a handy abstraction, but does the method caching. (kjg)
        internal static object rb_method_boundp(Class klass, string name, bool acceptPrivate)
        {
            RubyMethod meth = null;
            if (klass.get_method(name, out meth))
                return (meth.access != Access.Private || acceptPrivate);
            else
                return false;
        }


        internal void get_specified_methods(ArrayList list, Access attr)  // author: kjg, this is non-recursive
        {
            // Return the specified methods for just this Class!
            if (attr == Access.notPrivate)
            {
                foreach (KeyValuePair<string, RubyMethod> method in _methods)
                    if (method.Value != null && method.Value.access != Access.Private && !String.ListContains(list, method.Key))
                        list.Add(new String(method.Key));
            }
            else    // single access value case
            {
                foreach (KeyValuePair<string, RubyMethod> method in _methods)
                    if (method.Value != null && method.Value.access == attr && !String.ListContains(list, method.Key))
                        list.Add(new String(method.Key));
            }
        }

        internal void get_all_specified_methods(ArrayList list, Access attr, object arg0) // author: kjg
        {
            bool recur = Eval.Test(arg0);
            Class rcvr = this;
			while (rcvr._type == Type.Singleton || rcvr._type == Type.IClass) // find a real class
			{
				rcvr.get_specified_methods(list, attr);
				rcvr = rcvr.super;
			}
            do
            {
                rcvr.get_specified_methods(list, attr);
                rcvr = rcvr.super;
            } while (rcvr != null && recur);
        }

        // Alternative version of public_methods, using helper method above.
        internal void public_methods_alt(ArrayList list) //author: kjg, status: done
        {
            get_all_specified_methods(list, Access.Public, true);
        }


        internal void public_methods(ArrayList list) //status: done
        {
            foreach (KeyValuePair<string, RubyMethod> method in _methods)
                if (method.Value.access == Access.Public && !String.ListContains(list, method.Key))
                    list.Add(new String(method.Key));

            if (super != null)
                super.public_methods(list);
        }

        internal void include_module(Frame caller, Class module) // status: done
        {
            frozen_class_p(caller);

            if (!Tainted)
                Eval.rb_secure(4, null);

            if (module == null)
                return;

            if (my_class == module)
                return;

            if (module._type != Type.Module)
                check_type(caller, module, Type.Module);

            Object.obj_infect(this, module);

            Class c = this;
            while (module != null)
            {
                bool superclass_seen = false;
                if (this._methods == module._methods)
                    throw new ArgumentError("Cyclic include detected").raise(caller);

                for (Class p = this.super; p != null; p = p.super)
                    switch (p._type)
                    {
                        case Type.IClass:
                            if (p._methods == module._methods)
                            {
                                if (!superclass_seen)
                                    c = p;
                                goto skip;
                            }
                            break;
                        case Type.Class:
                            superclass_seen = true;
                            break;
                    }
                c = c.super = include_class_new(module, c.super);
            skip:
                module = module.super;
            }
        }


        internal bool is_kind_of(Class baseClass) //status: done
        {
            if (this == baseClass || this._methods == baseClass._methods)
                return true;
            else if (super != null)
                return super.is_kind_of(baseClass);
            else
                return false;
        }


        internal void check_inheritable(Frame caller) // author: Brian, status: done
        {
            if (this._type == Type.Singleton)
                throw new TypeError("can't make subclass of virtual class").raise(caller);

            if (this._type != Type.Class)
                throw new TypeError(System.String.Format(CultureInfo.InvariantCulture, "Superclass must be a Class ({0} given)", super._name)).raise(caller);
        }

        public void define_alloc_func(MethodBody body)
        {
            // Make sure the new allocator method generates objects that are subtypes of those
            // generated by the old allocator method - prevents compiler-generated CLR interop classes
            // overwriting Ruby built-in classes with incompatible allocator methods

            RubyMethod oldAlloc = null;
            Class c = this;

            //while (c != null)
            //{
                if (singleton_class(null, c)._methods.ContainsKey("allocator"))
                    oldAlloc = singleton_class(null, c)._methods["allocator"];

                if (oldAlloc != null && oldAlloc.body != null && body != null)
                {
                    object oldObj, newObj;

                    oldObj = oldAlloc.body.Call0(null, oldAlloc.definingClass, null, null);
                    newObj = body.Call0(null, oldAlloc.definingClass, null, null);

                    if (oldObj != null && newObj != null && !(newObj.GetType().IsInstanceOfType(oldObj)))
                    {
                    //    if (oldObj.GetType().FullName == "Ruby.Object" && newObj.GetType().FullName != "Ruby.Basic")
                            return;
                        //if ((oldObj.GetType().Equals(new Ruby.Object().GetType()) && !(newObj.GetType().Equals(new Ruby.Basic(null).GetType()))))
                        //    return;
                    }
                }

            //    c = c.super_real();
            //}
             
            rb_define_singleton_method(this, "allocator", body, 0, null);
        }

		internal void undef_alloc_func() // author: KJG, Brian needs to check
		{
			rb_define_singleton_method(this, "allocator", null, 0, null);
		}

        internal void define_private_method(string name, MethodBody body, int arity, Frame caller)//status: done
        {
            // methods[name] = new RubyMethod(body, arity, Access.Private);
            add_method(name, body, arity, Access.Private, caller);
        }


        internal void define_public_method(string name, MethodBody body, int arity, Frame caller)//status: done
        {
            // methods[name] = new RubyMethod(body, arity, Access.Public);
            add_method(name, body, arity, Access.Public, caller);
        }

        internal Access get_visibility_mode(Frame caller)
        {
            Access access = Access.Public;
            Class nesting = Init.rb_cObject;

            if (caller != null)
            {
                if (caller.nesting().Length > 0)
                    nesting = caller.nesting()[0];

                if (nesting == this)
                    access = caller.scope_vmode;
            }
            return access;
        }

        [UsedByRubyCompiler]
        public void define_method(string name, MethodBody body, int arity, Frame caller)
        {
            Debug.Assert(_methods != null);

            add_method(name, body, arity, caller);
        }

        internal void add_method(string name, MethodBody body, int arity, Frame caller)
        {
            if (caller != null && caller.scope_vmode == Access.ModuleFunction &&
                caller.nesting().Length > 0 && caller.nesting()[0] == this)
            {
                add_method(name, body, arity, Access.Private, caller);
                Class.rb_define_singleton_method(this, name, body, arity, caller);
            }
            else
            {
                add_method(name, body, arity, get_visibility_mode(caller), caller);
            }
        }

        // BBTAG: corresponds to rb_add_method: complete implementation of method definition, 
        // including invocation tests
        internal void add_method(string name, MethodBody body, int arity, Access access, Frame caller)//author: Brian, status: done
        {
            Debug.Assert(_methods != null);

            if (Eval.rb_safe_level() >= 4 && (this == Ruby.Runtime.Init.rb_cObject || !Tainted))
                throw new SecurityError("Insecure: can't define method").raise(caller);

            if ((_type != Type.Singleton) && (name.Equals("initialize") || name.Equals("initialize_copy")))
                access = Access.Private;
            else if ((_type == Type.Singleton) && name.Equals("allocate"))
            {
                Errors.rb_warn((System.String.Format(CultureInfo.InvariantCulture, "defining {0}.allocate is deprecated; use define_alloc_func()", ((Class)attached)._name)));
                name = "allocator";
            }

            _methods[name] = new RubyMethod(body, arity, access, this);

            if (!name.Equals("allocator") && Eval.RubyRunning)
            {
                if (_type == Type.Singleton)
                    Eval.CallPrivate(attached, caller, "singleton_method_added", null, new Symbol(name));
                else
                    Eval.CallPrivate(this, caller, "method_added", null, new Symbol(name));
            }
        }

        internal void remove_method(string name, Frame caller)
        {
            if (this == Ruby.Runtime.Init.rb_cObject)
                Eval.rb_secure(4, caller);

            if (Eval.rb_safe_level() >= 4 && !Tainted)
                throw new SecurityError("Insecure: can't remove method").raise(caller);

            if (Frozen)
                throw TypeError.rb_error_frozen(caller, "class/module").raise(caller);

            if (name.Equals("__id__") || name.Equals("__send__") || name.Equals("init"))
                Errors.rb_warn("removing " + name + " may cause serious problem");

            if (!_methods.ContainsKey(name) || (_methods[name] == null))
                throw new NameError(System.String.Format(CultureInfo.InvariantCulture, "method `{0}' not defined in {1}", name, this._name)).raise(caller);

            _methods.Remove(name);

            if (_type == Type.Singleton)
                Eval.CallPrivate(attached, caller, "singleton_method_removed", null, new Symbol(name));
            else
                Eval.CallPrivate(this, caller, "method_removed", null, new Symbol(name));
        }

        internal void frozen_class_p(Frame caller) // rb_frozen_class_p
        {
            string desc = "something";

            if (Frozen)
            {
                if (_type == Type.Singleton)
                    desc = "object";
                else
                {
                    if (_type == Type.Module || _type == Type.IClass)
                        desc = "module";
                    else if (_type == Type.Class)
                        desc = "class";
                }

                throw TypeError.rb_error_frozen(caller, desc).raise(caller);
            }
        }

        internal static void print_undef(Class klass, string id, Frame caller)
        {
             throw new NameError("undefined method `" + id + "' for " + ((klass._type == Type.Module) ? "module" : "class") + " `" + klass._name + "'").raise(caller);
        }

        internal void alias(string newId, string oldId, Frame caller)//author: Brian, status: done
        {
            frozen_class_p(caller);

            if (newId == oldId)
                return;

            if (this == Ruby.Runtime.Init.rb_cObject)
                Eval.rb_secure(4, caller);

            RubyMethod method = null;

            Class origin;
            if (!get_method(oldId, out method, out origin))
            {
                if (!((_type == Type.Module) && (Ruby.Runtime.Init.rb_cObject.get_method(oldId, out method, out origin))))
                {
                    // cannot alias nonexistant method
                    print_undef(this, oldId, caller);
                    //throw new NameError("undefined method `" + oldId + "' for " + name).raise(caller);
                }
            }

            if (Options.ruby_verbose.value != null && _methods.ContainsKey(newId))
                Errors.rb_warning("discarding old " + newId);

            _methods[newId] = new MethodAlias(method);

            if (_type == Type.Singleton)
                Eval.CallPrivate(attached, null, "singleton_method_added", null, new Symbol(newId));
            else
                Eval.CallPrivate(this, null, "method_added", null, new Symbol(newId));

        }

        internal void define_module_function(string name, MethodBody body, int arity, Frame caller)//status: done
        {
            define_private_method(name, body, arity, caller);
            rb_define_singleton_method(this, name, body, arity, caller);
        }


        [UsedByRubyCompiler]
        public void undef_method(string name)//status: done
        {
            _methods[name] = null;
        }

        internal object remove_const(string name)
        {
            object obj = instance_vars[name];
            instance_vars.Remove(name);
            return obj;
        }

        internal object remove_cvar(string name)
        {
            return remove_const(name);
        }

        // BBTAG: corresponds to rb_undef: undefines a method *and* also invokes the callbacks
        internal void undef(string name, Frame caller)//author: Brian, status: done
        {
            if (Eval.ruby_cbase(caller) == Ruby.Runtime.Init.rb_cObject && this == Ruby.Runtime.Init.rb_cObject)
                Eval.rb_secure(4, caller);

            if (Eval.rb_safe_level() >= 4 && !Tainted)
                throw new SecurityError("Insecure: can't undef `" + name + "'").raise(caller);

            frozen_class_p(caller);

            if (name.Equals("__id__") || name.Equals("__send__") || name.Equals("init"))
                Errors.rb_warn("undefining " + name + " may cause serious problem");

            RubyMethod method;
            if (!get_method(name, out method))
            {
                Class klass = this;
                string str = "class";

                if (klass._type == Type.Singleton)
                    klass = (Class)klass.attached;

                if (klass._type == Type.Module)
                    str = "module";

                throw new NameError("undefined method '" + name + "' for " + str + " " + klass._name).raise(caller);
            }

            _methods[name] = null;

            if (_type == Type.Singleton)
                Eval.CallPrivate(attached, caller, "singleton_method_undefined", null, new Symbol(name));
            else
                Eval.CallPrivate(this, caller, "method_undefined", null, new Symbol(name));
        }

        internal void redefine_visibility(Frame caller, string name, Access noex)//author: Brian, status: done
        {
            RubyMethod method;
            Class origin;

            if (!get_method(name, out method, out origin))
            {
                if (!((_type == Type.Module) && (Ruby.Runtime.Init.rb_cObject.get_method(name, out method, out origin))))
                {
                    // cannot redefine visibility for nonexistant method
                    throw new NameError("undefined method '" + name + "' for " + name).raise(caller);
                }
            }

            if (origin == this)
                method.access = noex;
            else
                _methods[name] = new RubyMethod(new CallSuperMethodBody(this, name), method.arity, noex, this);
        }

        internal void secure_visibility(Frame caller)
        {
            if (Eval.rb_safe_level() >= 4 && !Tainted)
            {
                throw new SecurityError("Insecure: can't change method visibility").raise(caller);
            }
        }

        internal void visibility_helper(Frame caller, Access access, ArgList args)//author: Brian, status: done
        {
            secure_visibility(caller);
            if (args.Length == 0)
            {
                //scope_vmode = access;
                if (caller != null)
                    caller.scope_vmode = access;
            }
            else
                set_method_visibility(caller, access, args.ToArray());
        }

        internal void set_method_visibility(Frame caller, Access access, object[] args)
        {
            foreach (object s in args)
                redefine_visibility(caller, Symbol.rb_to_id(caller, s), access);
        }

        internal void attr(string id, bool read, bool write, Frame caller)
        {
            attr(id, read, write, Access.Public, caller);
        }

        private void attr(string id, bool read, bool write, Access access, Frame caller) // author: Brian, status: done
        {
            Access attrAccess = access;

            if (access == Access.Private || access == Access.ModuleFunction)
            {
                attrAccess = Access.Private;
                Errors.rb_warn((get_visibility_mode(caller) == Access.ModuleFunction) ? "attribute accessor as module function" : "private attribute?");
            }

            if (read)
                add_method(id, new AttrReaderMethodBody("@"+id), 0, access, caller);

            if (write)
                add_method(id + "=", new AttrWriterMethodBody("@"+id), 1, access, caller);
        }

        internal bool const_defined(string name, bool recurse)//status: done
        {
            if (instance_vars.ContainsKey(name))
                return true;
            else
            {
                if (!recurse)
                    return false;

                Class c = super;

                while (c != null)
                {
                    if (c.instance_vars.ContainsKey(name))
                        return true;

                    c = c.super;
                }

                if (_type == Type.Module)
                    return Ruby.Runtime.Init.rb_cObject.const_defined(name);

                return false;
            }
        }

        internal bool const_defined(string name)
        {
            return const_defined(name, true);
        }


        internal virtual object const_get(string name, bool recurse, Frame caller)//status: done
        {
            if (instance_vars.ContainsKey(name))
            {
                if (instance_vars[name] is NodeAutoloadMemo)
                {
                    autoload_load(name, caller);
                    return const_get(name, recurse, caller);
                }
                else
                {
                    return instance_vars[name];
                }
            }
            else
            {
                if (!recurse)
                    return Eval.CallPrivate(this, caller, "const_missing", null, new Symbol(name));

                Class c = super;

                while (c != null)
                {
                    // special case for CLR classes
                    if (c is Interop.CLRClass)
                        return c.const_get(name, recurse, caller);

                    if (c.instance_vars.ContainsKey(name))
                    {
                        if (c.instance_vars[name] is NodeAutoloadMemo)
                        {
                            autoload_load(name, caller);
                            continue;
                        }
                        else
                            return c.instance_vars[name];
                    }

                    c = c.super;
                }

                if (_type == Type.Module)
                    return Ruby.Runtime.Init.rb_cObject.const_get(name, caller);

                return Eval.CallPrivate(this, caller, "const_missing", null, new Symbol(name));
            }
        }

        internal object const_get(string name, Frame caller)
        {
            return const_get(name, true, caller);
        }

        internal object const_set(string name, object value)//status: done
        {
            instance_vars[name] = value;
            return value;
        }

        // non-recursive
        internal Array get_constants_at () //author: Brian
        {
            Array result = new Array();

            foreach (KeyValuePair<string, object> var in instance_vars)
            {
                if (Symbol.is_const_id(var.Key))
                    result.Add(new String(var.Key));
            }

            return result;
        }

        // recursive
        internal Array get_constants() //author: Brian
        {
            ArrayList result = new ArrayList();

            Class klass = this;
            while (klass != null)
            {
                if (klass == Ruby.Runtime.Init.rb_cObject && this != Ruby.Runtime.Init.rb_cObject)
                    break;

                //klass = klass.class_real();
                result.AddRange(klass.get_constants_at());
                klass = klass.super;
            }

            return new Array(result);
        }

        internal void define_const(string name, object value)//status: done
        {
            if (!Symbol.is_const_id(name))
                Errors.rb_warn("rb_define_const: invalid name `" + name + "' for constant");

            if (this == Ruby.Runtime.Init.rb_cObject)
                Eval.rb_secure(4, null);

            const_set(name, value);
        }


        internal void define_alias(string name1, string name2, Frame caller) //status: done
        {
            alias(name1, name2, caller);
        }

        public override string ToString() //status: done
        {
            string str = classname();

            if (str == null || str.Length == 0)
                return class_path();

            return str;
        }

        internal string classname()//author: Brian, status: done
        {
            if (classpath == null || classpath.Equals(""))
            {
                if (_name == null || _name.Equals(""))
                {
                    classpath = find_class_path();
                    return classpath;
                }

                classpath = _name;
                return classpath;
            }

            return classpath;
        }

        private string find_class_path()//author: Brian, status: done
        {
            ArrayList pathList = new ArrayList();
            classpath = find_class_path_helper(Ruby.Runtime.Init.rb_cObject, pathList);
            return classpath;
        }

        private string find_class_path_helper(Object obj, ArrayList pathList)//author: Brian, status: done
        {
            Class klass = this;
            foreach (KeyValuePair<string, object> pair in obj.instance_vars)
            {
                if (!Symbol.is_const_id(pair.Key))
                    continue;

                if (pair.Value == Ruby.Runtime.Init.rb_cObject)
                    continue;

                if (pair.Value == klass)
                {
                    pathList.Add(pair.Key);
                    string path = "";
                    bool flag = false;

                    foreach (string key in pathList)
                    {
                        if (flag)
                            path += "::";
                        path += key;
                        flag = true;
                    }

                    return path;
                }

                if (pair.Value is Class)
                {
                    pathList.Add(pair.Key);
                    string path = find_class_path_helper((Object)pair.Value, pathList);
                    if (path != null)
                        return path;
                    pathList.Clear();
                }
            }

            // not found
            return null;
        }

        private void set_class_path(Class under, string name)//author: Brian, status: done
        {
            if (under == Ruby.Runtime.Init.rb_cObject)
                classpath = name;
            else
                classpath = under.class_path() + "::" + name;
        }

        private string class_path()
        {
            string path = classname();

            if (path != null && !path.Equals(""))
                return path;

            if (tmp_classpath != null && !tmp_classpath.Equals(""))
            {
                return tmp_classpath;
            }
            else
            {
                string s = "Class";

                if (_type == Type.Module) // FIXME
                    s = "Module";

                tmp_classpath = "#<" + s + ":0x" + System.Convert.ToString(GetHashCode(), 16) + ">";
                return tmp_classpath;
            }
        }

        internal Method mnew(Frame caller, object obj, string id, bool unbound)//author: Brian, status: done
        {
            RubyMethod method;
            Class klass = this;
            Class rKlass = klass;

        again:
            if (!klass.get_method(id, out method))
                throw new NameError("undefined method " + id).raise(caller);

            if (method.body is CallSuperMethodBody)
            {
                klass = klass.super;
                goto again;
            }

            while (rKlass != klass && (rKlass._type == Type.Singleton || rKlass._type == Type.IClass))
            {
                rKlass = rKlass.super;
            }

            if (klass._type == Type.IClass)
                klass = klass.my_class;

            Method m = new Method(obj, id, id, rKlass, this, method, unbound);
            Object.obj_infect(m, klass);
            return m;
        }

        internal void uninitialized_const(Frame caller, string id)
        {
            if (this != Ruby.Runtime.Init.rb_cObject)
                throw new NameError("uninitialized constant " + classname() + "::" + id).raise(caller);
            else
                throw new NameError("uninitialized constant " + id).raise(caller);
        }

        // rb_autoload
        internal void autoload(Frame caller, string id, string file)
        {
            if (!Symbol.is_const_id(id))
                throw new NameError(id, "autoload must be constant name").raise(caller);

            if (file == null || file.Length == 0)
                throw new ArgumentError("empty file name").raise(caller);

            if (autoload_tbl.ContainsKey(id))
                return;

            instance_vars[id] = null;
            autoload_tbl[id] = file;
            instance_vars[id] = new NodeAutoloadMemo(file);
        }

        internal bool autoload_load(string id, Frame caller)
        {
            if (autoload_tbl.ContainsKey(id))
            {
                string file = autoload_tbl[id];
                autoload_tbl.Remove(id);
                instance_variable_remove(id);
                // FIXME: need require_safe; is caller.block_arg OK if we don't have a block?
                return (bool)Methods.rb_f_require.singleton.Call1(my_class, this, caller, caller.block_arg, new String(file));
            }
            else
            {
                return false;
            }
        }

        // ----------------------------------------------------------------------------

        internal static Class rb_singleton_class_clone(object obj)
        {
            Class klass = ((Basic)obj).my_class;

            if (klass._type != Type.Singleton)
	            return klass;
            else 
            {
	            /* copy singleton(unnamed) class */
	            Class clone = new Class(klass);
                clone.Frozen = klass.Frozen;
                clone.Tainted = klass.Tainted;

	            if (obj is Class)
	                clone.my_class = clone;
	            else
	                clone.my_class = rb_singleton_class_clone(klass);

	            clone.super = klass.super;
                clone.instance_vars = null;
                clone._methods = null;
	            if (klass.instance_vars != null)
	                clone.instance_vars = new Dictionary<string,object>(klass.instance_vars);

	            clone._methods = new Dictionary<string,RubyMethod>();

                foreach (KeyValuePair<string, RubyMethod> m in klass._methods)
                    clone._methods.Add(m.Key, new RubyMethod(m.Value.body, m.Value.arity, m.Value.access, clone));
	            
                clone.my_class.attached = clone;
                clone._type = Type.Singleton;

	            return clone;
            }
        }

        internal static void rb_singleton_class_attached(Class klass, object obj)
        {
            if (klass._type == Type.Singleton)
                klass.attached = obj;
        }

        internal static void rb_define_global_function(string name, MethodBody body, int arity, Frame caller)//status: done
        {
            Ruby.Runtime.Init.rb_mKernel.define_module_function(name, body, arity, caller);
        }

        internal static void rb_define_singleton_method(object obj, string name, MethodBody body, int arity, Frame caller)//status: done
        {
            singleton_class(caller, obj).define_method(name, body, arity, caller);
        }


        internal static Class include_class_new(Class module, Class super) //status: done
        {
            Class klass = new Class("proxy->" + module._name, super, Type.IClass);

            if (module._type == Type.IClass)
                module = module.my_class;

            klass.instance_vars = module.instance_vars;
            klass._methods = module._methods;
            Debug.Assert(klass._methods != null);

            klass.super = super;

            if (module._type == Type.IClass)
                klass.my_class = module.my_class;
            else
                klass.my_class = module;

            return klass;
        }


        [UsedByRubyCompiler]
        public static Class CLASS_OF(object obj) //status: done
        {
            if (obj is Object)
                return ((Object)obj).my_class;

            if (obj is Basic)
                return ((Basic)obj).my_class;

            if (obj is int)
                return Ruby.Runtime.Init.rb_cFixnum;

            if (obj == null)
                return Ruby.Runtime.Init.rb_cNilClass;

            if (obj is bool)
                if ((bool)obj)
                    return Ruby.Runtime.Init.rb_cTrueClass;
                else
                    return Ruby.Runtime.Init.rb_cFalseClass;

            if (obj is Enum)
                return Ruby.Runtime.Init.rb_cFixnum;

            // BBTAG: need special case for Symbols as well
            if (obj is Symbol)
                return Ruby.Runtime.Init.rb_cSymbol;


            Interop.CLRClass result = null;
            System.Type type = obj.GetType();

            if (Interop.CLRClass.TryLoad(type, out result))
                return result;

            Class result2;
            // BBTAG: if it is a CLR class that we don't recognize, try looking in the Ruby constants table
            if (TryGetClassForType(type, out result2))
                return result2;

            // BBTAG: create a new CLRClass if one doesn't exist
            return Interop.CLRClass.Load(type, null, false);
        }

        internal static bool TryGetClassForType(System.Type type, out Class result)
        {
            string fullName = type.FullName;
            Class klass = Init.rb_cObject;
            result = null;

            foreach (string token in fullName.Split('.'))
            {
                if (!klass.const_defined(token))
                    return false;

                object c = klass.const_get(token, null);

                if (!(c is Class))
                    return false;

                klass = (Class)c;
            }

            result = klass;
            return true;
        }


        // BBTAG: helper method, refactoring of rb_define_class and rb_define_module (both have the same logic)
        private static Class define_class_module_helper(object scope, string name, object super, Type type, Frame caller) // author: Brian, status: provisional
        {
            if ((type != Type.Class) && (type != Type.Module))
                throw new TypeError("Cannot manually define virtual class").raise(caller);

            Class context = null;

            if (scope == null)
                context = Ruby.Runtime.Init.rb_cObject;
            else if (scope is Class)
                context = (Class)scope;
            else
                throw new TypeError("scope is not a Class").raise(caller);

            Class klass = null;
            if (context.const_defined(name, false))
            {
                object val = context.const_get(name, caller);
                if (val is Class)
                    klass = (Class)val;
                else
                    throw new TypeError(System.String.Format(CultureInfo.InvariantCulture, "{0} is not a class", val)).raise(caller);

                if ((type == Type.Module) && (klass._type != Type.Module))
                    throw new TypeError(System.String.Format(CultureInfo.InvariantCulture, "{0} is not a module", val)).raise(caller);

                // BBTAG: try skipping proxy classes when doing the base class check
                if ((type == Type.Class) && (super != null) && (klass.super_real() != super))
                    throw new NameError("can't change base class").raise(caller);

                return klass;
            }

            if (super == null)
                super = Ruby.Runtime.Init.rb_cObject;

            if (type == Type.Class)
                klass = define_class_id(name, super); // BBTAG: this connects the metaclass supers by default
            else
                klass = new Class(name, null, Type.Module, Ruby.Runtime.Init.rb_cModule);

            context.const_set(name, klass);

            klass.set_class_path(context, name);

            if (type == Type.Class)
                klass.class_inherited((Class)super, caller);
            return klass;
        }


        [UsedByRubyCompiler]
        public static Class singleton_class(Frame caller, object obj) //status: done
        {
            if (obj is int || obj is Symbol)
                throw new TypeError("can't define singleton").raise(caller);

            if (obj is Basic)
                return singleton_class((Basic)obj);
            else
            {
                string classname;
                if (Object.rb_special_const_p(obj, out classname))
                {
                    if (obj is bool && (bool)obj == true)
                        return Ruby.Runtime.Init.rb_cTrueClass;
                    else if (obj is bool && (bool)obj == false)
                        return Ruby.Runtime.Init.rb_cFalseClass;
                    else if (obj == null)
                        return Ruby.Runtime.Init.rb_cNilClass;
                    Exception.rb_bug("unknown immediate " + obj, null);
                }
                return null;
            }
        }

        private static Class singleton_class(Basic obj) //status: done
        {
            Class singleton;

            if (obj.my_class._type == Type.Singleton && obj.my_class.attached == obj)
                singleton = obj.my_class;
            else
                singleton = rb_make_metaclass(obj, obj.my_class);

            if (obj.Tainted)
                singleton.Tainted = true;
            else
                singleton.Tainted = false;

            if (obj.Frozen)
                singleton.Frozen = true;

            return singleton;
        }

        internal static void check_type(Frame caller, object arg, Type val)
        {
            Object.CheckType<Class>(caller, arg);
            
            if (((Class)arg)._type != val)
                throw new TypeError(string.Format(CultureInfo.InvariantCulture, "wrong argument type {0} ({1} expected)", ((Class)arg)._type, val.ToString())).raise(caller);
        }

        internal static void is_class_or_module(Frame caller, object arg, string msg)
        {
            if (arg is Class)
                return;

            if (msg == null)
                msg = "wrong argument type (class or module expected)";

            throw new TypeError(msg).raise(caller);
        }

        internal static Class rb_make_metaclass(Basic obj, Class super) //status: done
        {
            Class klass = new Class("meta->" + (obj is Class ? ((Class)obj)._name : "??"), super, Type.Singleton);
            obj.my_class = klass;
            klass.attached = obj;

            if (obj is Class && ((Class)obj)._type == Type.Singleton)
            {
                klass.my_class = klass;
                klass.super = ((Class)obj).super.class_real().my_class;
            }
            else
            {
                Class metasuper = super.class_real().my_class;

                if (metasuper != null)
                    klass.my_class = metasuper;
            }

            return klass;
        }


        internal static int rb_scan_args(Frame caller, Array args, int numMandatoryParams, int numOptionalParams, bool excessExpected)
        {
            int argCount = args.Count;

            if (numMandatoryParams > argCount) //Not enough params supplied 
                throw new ArgumentError(string.Format(CultureInfo.InvariantCulture, "wrong number of arguments ({0} for {1})", argCount, numMandatoryParams)).raise(caller);

            if (!excessExpected)
            {
                int numMaxParams = numMandatoryParams + numOptionalParams;
                if (argCount > numMaxParams) //Too many params
                    throw new ArgumentError(string.Format(CultureInfo.InvariantCulture, "wrong number of arguments ({0} for {1})", argCount, numMaxParams)).raise(caller);
            }

            return argCount;
        }

        internal static Class boot_defclass(string name, Class super)
        {
            Class klass = new Class(name, super, Type.Class);
            Class objectClass = Ruby.Runtime.Init.rb_cObject != null ? Ruby.Runtime.Init.rb_cObject : klass;
            objectClass.const_set(name, klass);
            return klass;
        }

        internal static Class define_class_id(string name, object super) // author: Brian, status: done
        {
            Class klass = new Class(name, (Class)super, Type.Class);
            rb_make_metaclass(klass, ((Class)super).my_class);
            return klass;
        }

        internal static Class rb_define_class(string name, object super, Frame caller)
        {
            return rb_define_class(null, name, super, caller);
        }

        [UsedByRubyCompiler]
        public static Class rb_define_class(object scope, string name, object super, Frame caller) // status: done
        {
            // BBTAG: use new helper method instead
            return define_class_module_helper(scope, name, super, Type.Class, caller);
        }


        internal static Class rb_define_module(string name, Frame caller)
        {
            return rb_define_module(null, name, caller);
        }

        [UsedByRubyCompiler]
        public static Class rb_define_module(object scope, string name, Frame caller) // status: done
        {
            // BBTAG: use new helper method instead
            return define_class_module_helper(scope, name, null, Type.Module, caller);
        }

        // -----------------------------------------------------------------------------

        internal static string rb_class_name(Class klass)
        {
            return klass.class_real().class_path();
        }

        internal static string rb_obj_classname(object obj)
        {
            return rb_class_name(Class.CLASS_OF(obj));
        }

        internal static void rb_define_method(Class klass, string name, MethodBody method, int arity, Frame caller)
        {
            klass.define_public_method(name, method, arity, caller);
        }

        internal static void rb_undef_method(Class klass, string name)
        {
            klass.undef_method(name);
        }

        internal static void rb_define_alloc_func(Class klass, MethodBody body)
        {
            klass.define_alloc_func(body);
        }

        internal static void rb_undef_alloc_func(Class klass)
        {
            klass.undef_alloc_func();
        }

        internal static void rb_define_private_method(Class klass, string name, MethodBody body, int arity, Frame caller)
        {
            klass.define_private_method(name, body, arity, caller);
        }

        internal static void rb_define_module_function(Class module, string name, MethodBody method, int arity, Frame caller)
        {
            module.define_module_function(name, method, arity, caller);
        }

        internal static void rb_include_module(Frame caller, Class klass, Class module)
        {
            klass.include_module(caller, module);
        }

        internal static void rb_extend_object(Frame caller, object obj, Class module)
        {
            rb_include_module(caller, singleton_class(caller, obj), module);
        }

        internal static void rb_define_alias(Class klass, string name1, string name2, Frame caller)
        {
            klass.define_alias(name1, name2, caller);
        }

        internal static Class rb_define_module_under(Class outer, string name, Frame caller)
        {
            // Fixme !!!
            return rb_define_module(outer, name, caller);
        }

        internal static Class rb_define_class_under(Class outer, string name, Class super, Frame caller)
        {
            // Fixme !!!
            return rb_define_class(outer, name, super, caller);
        }
    }
}
