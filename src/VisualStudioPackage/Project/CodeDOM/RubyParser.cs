using System.CodeDom.Compiler;
using System.CodeDom;
using Microsoft.CSharp;
using System.IO;
using Ruby.Compiler.AST;


namespace VSRuby.NET
{
    public class CodeDOMParser : ICodeParser 
    {
        public CodeCompileUnit Parse(System.IO.TextReader codeStream)
        {
            string text = codeStream.ReadToEnd();

            Ruby.Compiler.AST.Scope AST = Ruby.Compiler.Parser.ParseString(null, null, "", new Ruby.String(text), 1);

            CodeCompileUnit CodeDom = ((SOURCEFILE)AST).ToCodeCompileUnit();

            CodeDom.UserData["text"] = text;

            return CodeDom;
        }
    }
}
