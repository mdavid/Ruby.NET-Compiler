using System;
using System.Collections.Generic;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.ComponentModel;

namespace VSRuby.NET
{
    [DesignerCategory("")]
    public class RubyProvider : CodeDomProvider
    {
        private RubyFileNode file;

        public RubyProvider(RubyFileNode file)
        {
            this.file = file;
        }

        [Obsolete]
        public override ICodeCompiler CreateCompiler()
        {
            throw new NotSupportedException();
        }

        [Obsolete]
        public override ICodeGenerator CreateGenerator()
        {
            return new RubyCodeGenerator();
        }

        [Obsolete]
        public override ICodeParser CreateParser()
        {
            return new CodeDOMParser();
        }
    }
}
