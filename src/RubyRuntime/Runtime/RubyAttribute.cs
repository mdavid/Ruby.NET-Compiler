using System;
using System.Collections.Generic;
using System.Text;

namespace Ruby.Runtime
{
    [UsedByRubyCompiler]
    public class RubyAttribute: Attribute
    {
        [UsedByRubyCompiler]
        public RubyAttribute()
        {
        }
    }

    public class FrameAttribute : Attribute
    {
        public string sourcefile, classname;

        public FrameAttribute(string sourcefile, string classname)
        {
            this.sourcefile = sourcefile;
            this.classname = classname;
        }
    }
}
