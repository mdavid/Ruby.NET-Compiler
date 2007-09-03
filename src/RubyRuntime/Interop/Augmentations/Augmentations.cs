using System.Collections.Generic;
using System;
using System.Reflection;
using Ruby.Runtime;

namespace Ruby.Interop
{
    internal partial class Augmentations
    {
        [AttributeUsage(AttributeTargets.Class, AllowMultiple=true)]
        internal class Augmentation : System.Attribute
        {
            public readonly System.Type Type;

            public Augmentation(System.Type type)
            {
                this.Type = type;
            }
        }

        [AttributeUsage(AttributeTargets.Class, AllowMultiple=true)]
        internal class AugmentedMethodAttribute : Attribute
        {
            public readonly string Name;
            public readonly int Arity;

            public AugmentedMethodAttribute(string name, int arity)
            {
                this.Name = name;
                this.Arity = arity;
            }
        }

        private static Dictionary<System.Type, System.Type> augmentations;
        static Augmentations()
        {
            augmentations = new Dictionary<System.Type, System.Type>();

            foreach (System.Type type in typeof(Augmentations).GetNestedTypes(
                BindingFlags.NonPublic | BindingFlags.Public))
            {
                foreach (Augmentation a in type.GetCustomAttributes(
                    typeof(Augmentation), false))
                {
                    augmentations.Add(a.Type, type);
                }
            }
        }

        internal static void Augment(System.Type type, Ruby.Class klass, Frame caller)
        {
            System.Type augmentation;
            if (augmentations.TryGetValue(type, out augmentation))
            {
                foreach (System.Type augment in augmentation.GetNestedTypes(
                    BindingFlags.NonPublic | BindingFlags.Public))
                {
                    if (typeof(Ruby.Runtime.MethodBody).IsAssignableFrom(augment))
                    {
                        Ruby.Runtime.MethodBody body = (Ruby.Runtime.MethodBody)
                            System.Activator.CreateInstance(augment);

                        foreach (AugmentedMethodAttribute method in augment.GetCustomAttributes(
                            typeof(AugmentedMethodAttribute), false))
                        {
                            Class.rb_define_method(klass, method.Name, body, method.Arity, caller);
                        }
                    }
                }
            }
        }
    }
}
