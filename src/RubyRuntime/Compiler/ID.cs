/**********************************************************************

  Ruby.NET Runtime Library
  Originally developed at Queensland University of Technology
 
  Some sections of this C# code mirror the structure of the C code in the
  Ruby 1.8.2 Interpreter Copyright (C) 1993-2003 Yukihiro Matsumoto, et.al.
  
**********************************************************************/


using System;
using System.Collections.Generic;
using System.Text;

namespace Ruby.Compiler
{
    internal enum ID_Scope { NONE, LOCAL, INSTANCE, GLOBAL, ATTRSET, CONST, CLASS, INTERNAL, JUNK };

    // IDs are currently represented as CLR strings - this may be changed in future optimized versions.

    internal class ID
    {
        internal static string intern(string name)
        {
            return name;
        }

        internal static string intern(char name)
        {
            return new System.String(name, 1);
        }

        internal static string intern(Tokens token)
        {
            switch (token)
            {
                case Tokens.tDOT2: return "..";
                case Tokens.tDOT3: return "...";
                case Tokens.tPOW: return "**";
                case Tokens.tUPLUS: return "+@";
                case Tokens.tUMINUS: return "-@";
                case Tokens.tCMP: return "<=>";
                case Tokens.tGEQ: return ">=";
                case Tokens.tLEQ: return "<=";
                case Tokens.tEQ: return "==";
                case Tokens.tEQQ: return "===";
                case Tokens.tNEQ: return "!=";
                case Tokens.tMATCH: return "=~";
                case Tokens.tNMATCH: return "!~";
                case Tokens.tAREF: return "[]";
                case Tokens.tASET: return "[]=";
                case Tokens.tLSHFT: return "<<";
                case Tokens.tRSHFT: return ">>";
                case Tokens.tCOLON2: return "::";
                case Tokens.tOROP: return "||";
                case Tokens.tANDOP: return "&&";
                default: return token.ToString();
            }
        }


        internal static string ToDotNetName(string name)
        {
            string DotNetName;

            if (name == "~")
                DotNetName = "_tilde";
            else
                DotNetName = name;

            if (ID.Scope(name) == ID_Scope.ATTRSET)
                DotNetName += "_attrset";

            return DotNetName;
        }


        internal static ID_Scope Scope(string name)
        {
            switch (name[0])
            {
                case '$':
                    return ID_Scope.GLOBAL;
                case '@':
                    if (name[1] == '@')
                        return ID_Scope.CLASS;
                    else
                        return ID_Scope.INSTANCE;
                default:
                    if (name[name.Length - 1] == '=')
                        return ID_Scope.ATTRSET;
                    if (char.IsUpper(name[0]))
                        return ID_Scope.CONST;
                    else
                        return ID_Scope.LOCAL;
            }
        }
    }
}
