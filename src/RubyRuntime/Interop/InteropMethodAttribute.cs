using System;
using System.Collections.Generic;
using System.Text;

namespace Ruby.Interop
{
    public class InteropMethodAttribute: System.Attribute
    {
        internal string name;

        internal InteropMethodAttribute(string name)
        {
            this.name = name;
        }
    }
}
