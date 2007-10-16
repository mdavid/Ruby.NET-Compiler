using System;
using System.Collections.Generic;
using System.Text;
using Ruby.Runtime;

namespace Ruby.Interop
{
    internal class VariableInitialization : MethodBody2
    {
        internal static VariableInitialization singleton = new VariableInitialization();

        public override object Call2(Class last_class, object recv, Frame caller, Proc block, object val, object type)
        {
            return val;
        }
    }

    internal class ArrayCreate : MethodBody3
    {
        internal static ArrayCreate singleton = new ArrayCreate();

        public override object Call3(Class last_class, object recv, Frame caller, Proc block, object typeref, object size, object array)
        {
            System.Array result = System.Array.CreateInstance(TypeOf.Convert(typeref, caller), (int)size);

            System.Collections.ArrayList inits = ((Ruby.Array)array).value;
            for (int i=0; i<inits.Count; i++)
                result.SetValue(inits[i], i);

            return result;
        }
    }

    internal class TypeOf : MethodBody1 
    {
        internal static TypeOf singleton = new TypeOf();


        public override object Call1(Class last_class, object recv, Frame caller, Proc block, object typeref)
        {
            return Convert((Class)typeref, caller);
        }

        public static Type Convert(object klass, Frame caller) 
        {
            if (klass is CLRClass)
                return ((CLRClass)klass).clrtype;
            else if (klass is Class)
                return ((Class)klass).allocate().GetType();
            else
                throw new Ruby.ArgumentError("Class argument expected").raise(caller);
        }
    }

    internal class Cast : MethodBody2
    {
        internal static Cast singleton = new Cast();

        public override object Call2(Class last_class, object recv, Frame caller, Proc block, object typeref, object val)
        {
            return val;
        }
    }

    internal class TypeReference : MethodBody1
    {
        internal static TypeReference singleton = new TypeReference();

        public override object Call1(Class last_class, object recv, Frame caller, Proc block, object typeref)
        {
            return typeref;
        }
    }
}
