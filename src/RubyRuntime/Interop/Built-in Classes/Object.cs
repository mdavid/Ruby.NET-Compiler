using Ruby.Runtime;
using Ruby.Interop;

namespace Ruby
{
    public partial class Object
    {
         [InteropMethod("==")]
        public object equals(object p1)
        {
            return Eval.Call1(this, "==", p1);
        }

         [InteropMethod("===")]
        public object eqq(object p1)
        {
            return Eval.Call1(this, "===", p1);
        }

         [InteropMethod("=~")]
        public object match(object p1)
        {
            return Eval.Call1(this, "=~", p1);
        }

         [InteropMethod("__id__")]
        public object __id__()
        {
            return Eval.Call0(this, "__id__");
        }

         [InteropMethod("__send__")]
        public object __send__(params object[] args)
        {
            return Eval.Call(this, "__send__", args);
        }

         [InteropMethod("class")]
        public object klass()
        {
            return Eval.Call0(this, "class");
        }

         [InteropMethod("clone")]
        public object clone()
        {
            return Eval.Call0(this, "clone");
        }

         [InteropMethod("display")]
        public object display(params object[] args)
        {
            return Eval.Call(this, "display", args);
        }

         [InteropMethod("dup")]
        public object dup()
        {
            return Eval.Call0(this, "dup");
        }

         [InteropMethod("eql?")]
        public object is_eql(object p1)
        {
            return Eval.Call1(this, "eql?", p1);
        }

         [InteropMethod("equal?")]
        public object is_equal(object p1)
        {
            return Eval.Call1(this, "equal?", p1);
        }

         [InteropMethod("extend")]
        public object extend(params object[] args)
        {
            return Eval.Call(this, "extend", args);
        }

         [InteropMethod("freeze")]
        public object freeze()
        {
            return Eval.Call0(this, "freeze");
        }

         [InteropMethod("frozen?")]
        public object is_frozen()
        {
            return Eval.Call0(this, "frozen?");
        }

         [InteropMethod("hash")]
        public object hash()
        {
            return Eval.Call0(this, "hash");
        }

         [InteropMethod("id")]
        public object id()
        {
            return Eval.Call0(this, "id");
        }

         [InteropMethod("inspect")]
        public object inspect()
        {
            return Eval.Call0(this, "inspect");
        }

         [InteropMethod("instance_eval")]
        public object instance_eval(params object[] args)
        {
            return Eval.Call(this, "instance_eval", args);
        }

         [InteropMethod("instance_of?")]
        public object is_instance_of(object p1)
        {
            return Eval.Call1(this, "instance_of?", p1);
        }

         [InteropMethod("instance_variable_get")]
        public object instance_variable_get(object p1)
        {
            return Eval.Call1(this, "instance_variable_get", p1);
        }

         [InteropMethod("instance_variable_set")]
        public object instance_variable_set(object p1, object p2)
        {
            return Eval.Call2(this, "instance_variable_set", p1, p2);
        }

         [InteropMethod("instance_variables")]
        public object instance_variables()
        {
            return Eval.Call0(this, "instance_variables");
        }

         [InteropMethod("is_a?")]
        public object is_is_a(object p1)
        {
            return Eval.Call1(this, "is_a?", p1);
        }

         [InteropMethod("kind_of?")]
        public object is_kind_of(object p1)
        {
            return Eval.Call1(this, "kind_of?", p1);
        }

         [InteropMethod("method")]
        public object method(object p1)
        {
            return Eval.Call1(this, "method", p1);
        }

         [InteropMethod("methods")]
        public object methods(params object[] args)
        {
            return Eval.Call(this, "methods", args);
        }

         [InteropMethod("nil?")]
        public object is_nil()
        {
            return Eval.Call0(this, "nil?");
        }

         [InteropMethod("object_id")]
        public object object_id()
        {
            return Eval.Call0(this, "object_id");
        }

         [InteropMethod("private_methods")]
        public object private_methods(params object[] args)
        {
            return Eval.Call(this, "private_methods", args);
        }

         [InteropMethod("protected_methods")]
        public object protected_methods(params object[] args)
        {
            return Eval.Call(this, "protected_methods", args);
        }

         [InteropMethod("public_methods")]
        public object public_methods(params object[] args)
        {
            return Eval.Call(this, "public_methods", args);
        }

         [InteropMethod("respond_to?")]
        public object is_respond_to(params object[] args)
        {
            return Eval.Call(this, "respond_to?", args);
        }

         [InteropMethod("send")]
        public object send(params object[] args)
        {
            return Eval.Call(this, "send", args);
        }

         [InteropMethod("singleton_methods")]
        public object singleton_methods(params object[] args)
        {
            return Eval.Call(this, "singleton_methods", args);
        }

         [InteropMethod("taint")]
        public object taint()
        {
            return Eval.Call0(this, "taint");
        }

         [InteropMethod("tainted?")]
        public object is_tainted()
        {
            return Eval.Call0(this, "tainted?");
        }

         [InteropMethod("to_a")]
        public object to_a()
        {
            return Eval.Call0(this, "to_a");
        }

         [InteropMethod("to_s")]
        public object to_s()
        {
            return Eval.Call0(this, "to_s");
        }

         [InteropMethod("type")]
        public object type()
        {
            return Eval.Call0(this, "type");
        }

         [InteropMethod("untaint")]
        public object untaint()
        {
            return Eval.Call0(this, "untaint");
        }

    }
}

