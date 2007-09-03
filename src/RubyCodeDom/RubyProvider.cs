using System;
using System.Collections.Generic;
using System.Text;
using System.CodeDom;
using System.CodeDom.Compiler;

namespace Ruby.CodeDom
{
    public class RubyProvider : CodeDomProvider
    {
        public override ICodeCompiler CreateCompiler()
        {
            return new RubyCompiler();
        }

        public override ICodeGenerator CreateGenerator()
        {
            return new RubyCodeGenerator();
        }
    }
}
