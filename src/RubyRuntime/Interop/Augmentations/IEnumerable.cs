using Ruby.Runtime;
using Ruby.Methods;

namespace Ruby.Interop
{
    partial class Augmentations
    {
        [Augmentation(typeof(System.Collections.IEnumerable))]
        internal class IEnumerable
        {
            [AugmentedMethod("to_a", 0)]
            internal class ToArray : Ruby.Runtime.MethodBody0
            {
                public override object Call0(Class last_class, object recv, Frame caller, Proc block)
                {
                    System.Collections.IEnumerable seq = (System.Collections.IEnumerable)recv;
                    System.Collections.ArrayList l = new System.Collections.ArrayList();

                    foreach (object o in seq)
                        l.Add(o);

                    return Array.CreateUsing(l);
                }
            }
        }
    }
}
