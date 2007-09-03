using Ruby.Runtime;
using Ruby.Methods;

namespace Ruby.Interop
{
    partial class Augmentations
    {
        [Augmentation(typeof(System.Object))]
        internal class Object
        {
            [AugmentedMethod("to_s", 0)]
            internal new class ToString : Ruby.Runtime.MethodBody0
            {
                public override object Call0(Class last_class, object recv, Frame caller, Proc block)
                {
                    return new Ruby.String(recv.ToString());
                }
            }

            [AugmentedMethod("hash", 0)]
            internal new class GetHashCode : Ruby.Runtime.MethodBody0
            {
                public override object Call0(Class last_class, object recv, Frame caller, Proc block)
                {
                    return recv.GetHashCode();
                }
            }

            [AugmentedMethod("kind_of?", 1)]
            [AugmentedMethod("is_a?", 1)]
            internal class Is : Ruby.Runtime.MethodBody0
            {
                public override object Call1(Class last_class, object recv, Frame caller, Proc block, object p1)
                {
                    System.Type type = null;
                    CLRClass clrclass = p1 as CLRClass;

                    if (clrclass != null)
                    {
                        type = clrclass.clrtype;
                    }
                    else
                    {
                        type = p1 as System.Type;
                    }

                    if (type != null)
                    {
                        return type.IsInstanceOfType(recv);
                    }

                    return rb_obj_is_kind_of.singleton.Call1(last_class, recv, caller, block, p1);
                }
            }
        }
    }
}
