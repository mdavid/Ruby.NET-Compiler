using Ruby.Runtime;
using Ruby.Methods;

namespace Ruby.Interop
{
    partial class Augmentations
    {
        [Augmentation(typeof(System.ICloneable))]
        internal class ICloneable
        {
            [AugmentedMethod("clone", 0)]
            internal class Clone : Ruby.Runtime.MethodBody0
            {
                public override object Call0(Class last_class, object recv, Frame caller, Proc block)
                {
                    return ((System.ICloneable)recv).Clone();
                }
            }
        }
    }
}
