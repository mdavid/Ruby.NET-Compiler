using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.Package;
using Ruby.NET.Lexer;
using Microsoft.VisualStudio.TextManager.Interop;

namespace Ruby.NET
{
    public static partial class Configuration
    {
        static CommentInfo myCInfo;
        public static CommentInfo MyCommentInfo { get { return myCInfo; } }

        static Configuration()
        {
            myCInfo.LineStart = "#";
            myCInfo.UseLineComments = true;

            // default colors - currently, these need to be declared
            CreateColor("Keyword", COLORINDEX.CI_BLUE,  COLORINDEX.CI_USERTEXT_BK);
            CreateColor("Comment",  COLORINDEX.CI_DARKGREEN, COLORINDEX.CI_USERTEXT_BK);
            CreateColor("Identifier", COLORINDEX.CI_USERTEXT_FG, COLORINDEX.CI_USERTEXT_BK);
            CreateColor("String",   COLORINDEX.CI_MAROON, COLORINDEX.CI_USERTEXT_BK);
            CreateColor("Number",  COLORINDEX.CI_USERTEXT_FG, COLORINDEX.CI_USERTEXT_BK);
            TokenColor ClassName = CreateColor("Ruby Class Name ", COLORINDEX.CI_AQUAMARINE, COLORINDEX.CI_USERTEXT_BK);
            TokenColor MethodName = CreateColor("Ruby Method Name", COLORINDEX.CI_AQUAMARINE, COLORINDEX.CI_USERTEXT_BK);
            TokenColor Operator = CreateColor("Operator", COLORINDEX.CI_USERTEXT_FG, COLORINDEX.CI_USERTEXT_BK);
            
            //
            // map tokens to color classes
            //
            ColorToken((int)Tokens.String, TokenType.String, TokenColor.String, TokenTriggers.None);
            ColorToken((int)Tokens.Literal, TokenType.Literal, TokenColor.Number, TokenTriggers.None);
            ColorToken((int)Tokens.Keyword, TokenType.Delimiter, TokenColor.Keyword, TokenTriggers.None);
            ColorToken((int)Tokens.Bracket, TokenType.Delimiter, TokenColor.Text, TokenTriggers.MatchBraces);
            ColorToken((int)Tokens.Quote, TokenType.Delimiter, TokenColor.String, TokenTriggers.MatchBraces);
            ColorToken((int)Tokens.Comment, TokenType.Comment, TokenColor.Comment, TokenTriggers.None);
            ColorToken((int)Tokens.Ident, TokenType.Identifier, TokenColor.Identifier, TokenTriggers.None);
            ColorToken((int)Tokens.Number, TokenType.Literal, TokenColor.Number, TokenTriggers.None);
            ColorToken((int)Tokens.ClassName, TokenType.Identifier, ClassName, TokenTriggers.None);
            ColorToken((int)Tokens.MethodName, TokenType.Identifier, MethodName, TokenTriggers.None);
            ColorToken((int)Tokens.Operator, TokenType.Operator, Operator, TokenTriggers.None);
        }
    }
}