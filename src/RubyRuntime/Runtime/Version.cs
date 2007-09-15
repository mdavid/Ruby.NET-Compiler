/**********************************************************************

  Ruby.NET Runtime Library
  Originally developed at Queensland University of Technology
 
  Some sections of this C# code mirror the structure of the C code in the
  Ruby 1.8.2 Interpreter Copyright (C) 1993-2003 Yukihiro Matsumoto, et.al.
  
**********************************************************************/


using System;

namespace Ruby.Runtime
{
    // Ruby.Version - display Ruby version information
    
    internal class Version
    {
        internal const string ruby_version = "1.8.2";
        internal const string ruby_release_date = "2006-06-30";
        internal const string ruby_platform = "Ruby.Net-mswin";

        internal static void ruby_show_version()
        {
            System.Console.WriteLine("ruby {0} ({1}) [{2}]", ruby_version, ruby_release_date, ruby_platform);
        }

        internal static void ruby_show_copyright()
        {
            System.Console.WriteLine("Ruby.Net Copyright (C) 2007");
        }
    }
}
