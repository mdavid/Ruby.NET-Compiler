using Ruby.Runtime;
using Ruby.Interop;

namespace Ruby
{
    public partial class Module
    {
         [InteropMethod("nesting")]
        public static object nesting()
        {
            return Eval.Call0(Ruby.Runtime.Init.rb_cModule, "nesting");
        }

         [InteropMethod("<")]
        public static object operator<(Module p1, object p2)
        {
            return Eval.Call1(p1, "<", p2);
        }

         [InteropMethod("<=")]
        public static object operator<=(Module p1, object p2)
        {
            return Eval.Call1(p1, "<=", p2);
        }

         [InteropMethod("<=>")]
        public object cmp(object p1)
        {
            return Eval.Call1(this, "<=>", p1);
        }

         [InteropMethod(">")]
        public static object operator>(Module p1, object p2)
        {
            return Eval.Call1(p1, ">", p2);
        }

         [InteropMethod(">=")]
        public static object operator>=(Module p1, object p2)
        {
            return Eval.Call1(p1, ">=", p2);
        }

         [InteropMethod("ancestors")]
        public object ancestors()
        {
            return Eval.Call0(this, "ancestors");
        }

         [InteropMethod("autoload")]
        public object autoload(object p1, object p2)
        {
            return Eval.Call2(this, "autoload", p1, p2);
        }

         [InteropMethod("autoload?")]
        public object is_autoload(object p1)
        {
            return Eval.Call1(this, "autoload?", p1);
        }

         [InteropMethod("class_eval")]
        public object class_eval(params object[] args)
        {
            return Eval.Call(this, "class_eval", args);
        }

         [InteropMethod("class_variables")]
        public object class_variables()
        {
            return Eval.Call0(this, "class_variables");
        }

         [InteropMethod("const_defined?")]
        public object is_const_defined(object p1)
        {
            return Eval.Call1(this, "const_defined?", p1);
        }

         [InteropMethod("const_get")]
        public object const_get(object p1)
        {
            return Eval.Call1(this, "const_get", p1);
        }

         [InteropMethod("const_missing")]
        public object const_missing(object p1)
        {
            return Eval.Call1(this, "const_missing", p1);
        }

         [InteropMethod("const_set")]
        public object const_set(object p1, object p2)
        {
            return Eval.Call2(this, "const_set", p1, p2);
        }

         [InteropMethod("constants")]
        public object constants()
        {
            return Eval.Call0(this, "constants");
        }

         [InteropMethod("include?")]
        public object is_include(object p1)
        {
            return Eval.Call1(this, "include?", p1);
        }

         [InteropMethod("included_modules")]
        public object included_modules()
        {
            return Eval.Call0(this, "included_modules");
        }

         [InteropMethod("instance_method")]
        public object instance_method(object p1)
        {
            return Eval.Call1(this, "instance_method", p1);
        }

         [InteropMethod("instance_methods")]
        public object instance_methods(params object[] args)
        {
            return Eval.Call(this, "instance_methods", args);
        }

         [InteropMethod("method_defined?")]
        public object is_method_defined(object p1)
        {
            return Eval.Call1(this, "method_defined?", p1);
        }

         [InteropMethod("module_eval")]
        public object module_eval(params object[] args)
        {
            return Eval.Call(this, "module_eval", args);
        }

         [InteropMethod("name")]
        public object name()
        {
            return Eval.Call0(this, "name");
        }

         [InteropMethod("private_class_method")]
        public object private_class_method(params object[] args)
        {
            return Eval.Call(this, "private_class_method", args);
        }

         [InteropMethod("private_instance_methods")]
        public object private_instance_methods(params object[] args)
        {
            return Eval.Call(this, "private_instance_methods", args);
        }

         [InteropMethod("private_method_defined?")]
        public object is_private_method_defined(object p1)
        {
            return Eval.Call1(this, "private_method_defined?", p1);
        }

         [InteropMethod("protected_instance_methods")]
        public object protected_instance_methods(params object[] args)
        {
            return Eval.Call(this, "protected_instance_methods", args);
        }

         [InteropMethod("protected_method_defined?")]
        public object is_protected_method_defined(object p1)
        {
            return Eval.Call1(this, "protected_method_defined?", p1);
        }

         [InteropMethod("public_class_method")]
        public object public_class_method(params object[] args)
        {
            return Eval.Call(this, "public_class_method", args);
        }

         [InteropMethod("public_instance_methods")]
        public object public_instance_methods(params object[] args)
        {
            return Eval.Call(this, "public_instance_methods", args);
        }

         [InteropMethod("public_method_defined?")]
        public object is_public_method_defined(object p1)
        {
            return Eval.Call1(this, "public_method_defined?", p1);
        }

    }
}

