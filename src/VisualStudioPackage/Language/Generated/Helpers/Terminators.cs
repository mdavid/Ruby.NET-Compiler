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
    public abstract class Terminator
    {
        public Terminator()
        {
        }
    }


    public enum string_type
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

    public class StringTerminator : Terminator
    {
        public int func;
        public int term;
        public int paren;
        public int name;
        public object nest;

        public StringTerminator(int func, int term, int paren, int name)
        {
            this.func = func;
            this.term = term;
            this.paren = paren;
            this.name = name;
            this.nest = 0;
        }
    }

    public class HEREDOC : Terminator
    {
        public string label;
        public int resume_position;
        public String resume_line;

        public HEREDOC(string label, int resume_position, String resume_line)
        {
            this.label = label;
            this.resume_position = resume_position;
            this.resume_line = resume_line;
        }
    }
}
