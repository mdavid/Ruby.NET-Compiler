/**********************************************************************

  Ruby.NET Runtime Library
  Originally developed at Queensland University of Technology
 
  Some sections of this C# code mirror the structure of the C code in the
  Ruby 1.8.2 Interpreter Copyright (C) 1993-2003 Yukihiro Matsumoto, et.al.
  
**********************************************************************/


using Ruby;
using System.Collections.Generic;
using System.Text;


namespace Ruby.Compiler
{
    using AST;


    internal abstract class Terminator : Node
    {
        internal Terminator(YYLTYPE location)
            : base(location)
        {
        }
    }


    internal enum string_type
    {
        str_minusone = -1,
        str_squote = (0),
        str_dquote = (0x02),
        str_xquote = (0x02),
        str_regexp = (0x04 | 0x01 | 0x02),
        str_sword = (0x08),
        str_dword = (0x08 | 0x02),
        str_ssym = (0x10),
        str_dsym = (0x10 | 0x02),
    };


    internal class StringTerminator : Terminator
    {
        internal int func;
        internal int term;
        internal int paren;
        internal int name;
        internal object nest;

        internal StringTerminator(int func, int term, int paren, int name, YYLTYPE location): base(location)
        {
            this.func = func;
            this.term = term;
            this.paren = paren;
            this.name = name;
            this.nest = 0;
        }
    }


    internal class HEREDOC : Terminator
    {
        internal string label;
        internal int resume_position;
        internal String resume_line;

        internal HEREDOC(string label, int resume_position, String resume_line, YYLTYPE location): base(location)
        {
            this.label = label;
            this.resume_position = resume_position;
            this.resume_line = resume_line;
        }
    }
}
