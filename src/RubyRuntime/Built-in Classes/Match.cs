/**********************************************************************

  Ruby.NET Runtime Library
  Originally developed at Queensland University of Technology
 
  Some sections of this C# code mirror the structure of the C code in the
  Ruby 1.8.2 Interpreter Copyright (C) 1993-2003 Yukihiro Matsumoto, et.al.
  
**********************************************************************/

using Ruby.Runtime;
using System;

namespace Ruby
{
    [UsedByRubyCompiler]
    public partial class Match : Basic
    {
        internal System.Text.RegularExpressions.Match value;
        internal string matched;

        //-------------------------------------------------------------------------------

        internal Match(Class klass) : base(klass) 
        {
            this.value = null;
            this.matched = null;
        }

        internal Match(System.Text.RegularExpressions.Match value, string matched)
            : base(Ruby.Runtime.Init.rb_cMatch)
        {
            this.value = value;
            this.matched = matched;
        }


        //-------------------------------------------------------------------------------


        internal override object Inner()
        {
            return value;
        }

        [UsedByRubyCompiler]
        public object get_nth(int nth)
        {
            return Regexp.rb_reg_nth_match(nth, this);
        }

        [UsedByRubyCompiler]
        public bool defined_nth(int nth)
        {
            return Regexp.rb_reg_nth_defined(nth, this);
        }

        [UsedByRubyCompiler]
        public object last_match(Frame caller)
        {
            return Regexp.last_match_glob.getter("$&", caller);
        }

        [UsedByRubyCompiler]
        public object match_pre(Frame caller)
        {
            return Regexp.prematch.getter("$`", caller);
        }

        [UsedByRubyCompiler]
        public object match_post(Frame caller)
        {
            return Regexp.postmatch.getter("$'", caller);
        }

        [UsedByRubyCompiler]
        public object match_last(Frame caller)
        {
            return Regexp.last_match_glob.getter("$+", caller);
        }

        // --------------------------------------------------------------------------------

        internal static Array match_array(Match match, int start)
        {
            bool taint = match.Tainted;

            System.Collections.ArrayList result = new System.Collections.ArrayList();

            foreach (System.Text.RegularExpressions.Group group in match.value.Groups)
            {
                String str = new String(group.Value);
                str.Tainted = taint;
                result.Add(str);
            }

            if (start == 0)
                return new Array(result);

            return new Array(result.GetRange(start, result.Count - 1));
        }
    }
}
