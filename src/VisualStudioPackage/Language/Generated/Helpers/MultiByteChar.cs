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
    // MultiByte character handling - needs fixing ...
    public class MultiByteChar
    {
        public static bool ismbchar(char c)
        {
            return char.IsSurrogate(c);
        }

        public static int mbclen(char c)
        {
            return 1;
        }
    }
}