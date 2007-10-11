using System.Collections.Generic;
using System.Text;
using VSRuby.NET.ParserGenerator;
using VSRuby.NET.Parser;


namespace VSRuby.NET.Lexer
{
    // Abstract base class to allow overriding of Get/SetEolState
    public abstract class ScanBase : AScanner<ValueType, LexLocation>, IColorScan
    {
        //protected abstract int CurrentSc { set; get; }
        private int eolState;
        public int EolState
        {
            get { return eolState; }
            set { eolState = value; }
        }
        public abstract void SetSource(string s, int o);
        public abstract int GetNext(ref int c, out int s, out int e);
    }
}
