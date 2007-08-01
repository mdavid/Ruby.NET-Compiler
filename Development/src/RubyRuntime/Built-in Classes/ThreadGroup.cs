/**********************************************************************

  Ruby.NET Runtime Library
  Originally developed at Queensland University of Technology
 
  Some sections of this C# code mirror the structure of the C code in the
  Ruby 1.8.2 Interpreter Copyright (C) 1993-2003 Yukihiro Matsumoto, et.al.
  
**********************************************************************/

using Ruby.Runtime;

namespace Ruby
{

    public partial class ThreadGroup : Object
    {
        internal int enclosed;
        internal ThreadGroup group;

        //-----------------------------------------------------------------

        internal ThreadGroup()
            :base(Ruby.Runtime.Init.cThGroup)
        {

        }

        internal ThreadGroup(Class klass)
            : base(klass)
        {
        }

        //-----------------------------------------------------------------
    }
}
