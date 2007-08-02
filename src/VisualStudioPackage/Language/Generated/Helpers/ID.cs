/**********************************************************************

  Ruby.NET Runtime Library
  Originally developed at Queensland University of Technology
 
  Some sections of this C# code mirror the structure of the C code in the
  Ruby 1.8.2 Interpreter Copyright (C) 1993-2003 Yukihiro Matsumoto, et.al.
  
**********************************************************************/


using System;
using System.Collections.Generic;
using System.Text;

namespace Ruby.NET
{
    public enum ID_Scope { NONE, LOCAL, INSTANCE, GLOBAL, ATTRSET, CONST, CLASS, INTERNAL, JUNK };

    // IDs are currently represented as CLR strings - this may be changed in future optimized versions.
    public class ID
    {
        public static ID_Scope Scope(string name)
        {
            switch (name[0])
            {
                case '$':
                    return ID_Scope.GLOBAL;
                case '@':
                    if (name[1] == '@')
                        return ID_Scope.CLASS;
                    else
                        return ID_Scope.INSTANCE;
                default:
                    if (name[name.Length - 1] == '=')
                        return ID_Scope.ATTRSET;
                    if (Char.IsUpper(name[0]))
                        return ID_Scope.CONST;
                    else
                        return ID_Scope.LOCAL;
            }
        }
    }
}