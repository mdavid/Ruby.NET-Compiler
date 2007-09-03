using Ruby.Runtime;
using Ruby.Methods;

namespace Ruby.Interop
{
    partial class Augmentations
    {
        [Augmentation(typeof(System.Collections.ICollection))]
        internal class ICollection
        {
            [AugmentedMethod("to_a", 0)]
            internal class ToArray : Ruby.Runtime.MethodBody0
            {
                public override object Call0(Class last_class, object recv, Frame caller, Proc block)
                {
                    return new Array((System.Collections.ICollection)recv);
                }
            }
        }
    }
}
