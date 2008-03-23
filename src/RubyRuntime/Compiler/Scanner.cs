/**********************************************************************

  Ruby.NET Runtime Library
  Originally developed at Queensland University of Technology
 
  Some sections of this C# code mirror the structure of the C code in the
  Ruby 1.8.2 Interpreter Copyright (C) 1993-2003 Yukihiro Matsumoto, et.al.
  
**********************************************************************/


using Ruby;
using System.Collections.Generic;
using System.Text;
using Microsoft.Build.Utilities;
using gppg;


namespace Ruby.Compiler
{
    using AST;
    using System.Globalization;

    internal enum Lex_State
    {
        EXPR_BEG,            /* ignore newline, +/- is a sign. */
        EXPR_END,            /* newline significant, +/- is a operator. */
        EXPR_ARG,            /* newline significant, +/- is a operator. */
        EXPR_CMDARG,        /* newline significant, +/- is a operator. */
        EXPR_ENDARG,        /* newline significant, +/- is a operator. */
        EXPR_MID,            /* newline significant, +/- is a operator. */
        EXPR_FNAME,            /* ignore newline, no reserved words. */
        EXPR_DOT,            /* right after `.' or `::', no reserved words. */
        EXPR_CLASS,            /* immediate after `class', no here document. */
    };


    internal class Scanner : IScanner<ValueType, YYLTYPE>
    {
        internal delegate String GetDelegate(object v);

        internal StringBuilder yytext;

        internal int sourceline;
        internal string sourcefile;

        internal Lex_State lex_state;
        internal int heredoc_end = 0;
        internal bool commastart = true;
        internal Terminator lex_strterm = null;

        private GetDelegate lex_gets;
        private object lex_input;
        private String lex_lastline;
        private string lex_buffer;
        private int lex_p = 0;
        private int lex_pend = 0;
        private StringBuilder tokenbuf = null;
        private Parser parser;
        internal int start_line = 1;
        private int start_column = 0;

        internal Scanner(Parser parser, IO file)
        {
            this.parser = parser;
            this.lex_gets = IO.rb_io_gets;
            this.lex_input = file;
        }

        internal Scanner(Parser parser, System.IO.TextReader reader)
        {
            this.parser = parser;
            this.lex_gets = stream_reader;
            this.lex_input = reader;
        }

        internal Scanner(Parser parser, String s)
        {
            this.parser = parser;
            this.lex_gets = lex_get_str;
            this.lex_input = s;
        }

        public override YYLTYPE yylloc
        {
            get
            {
                return new YYLTYPE(sourcefile, start_line, start_column, sourceline, lex_p+1);
            }
        }


        public override int yylex()
        {
            int c;
            bool space_seen = false;
            bool cmd_state;

            set_start();

            if (lex_strterm != null)
                if (lex_strterm is HEREDOC)
                {
                    int token = here_document((HEREDOC)lex_strterm);
                    if (token == (int)Tokens.tSTRING_END)
                    {
                        lex_strterm = null;
                        lex_state = Lex_State.EXPR_END;
                    }
                    return token;
                }
                else
                {
                    int token = parse_string((StringTerminator)lex_strterm);
                    if (token == (int)Tokens.tSTRING_END || token == (int)Tokens.tREGEXP_END)
                    {
                        lex_strterm = null;
                        lex_state = Lex_State.EXPR_END;
                    }
                    return token;
                }

            cmd_state = commastart;
            commastart = false;

        retry:
            set_start();
            switch (c = nextc())
            {
                case '\0':        /* NUL */
                //case '\004':        /* ^D */ fixme
                //case '\032':        /* ^Z */ fixme
                case -1:            /* end of script. */
                    return (int)Tokens.EOF;

                /* white spaces */
                case ' ':
                case '\t':
                case '\f':
                case '\r':
                    //case '\13': /* '\v' */ fixme
                    space_seen = true;
                    goto retry;

                case '#':        /* it's a comment */
                    while ((c = nextc()) != '\n')
                    {
                        if (c == -1)
                            return (int)Tokens.EOF;
                    }
                    goto case '\n';
                /* fall through */
                case '\n':
                    switch (lex_state)
                    {
                        case Lex_State.EXPR_BEG:
                        case Lex_State.EXPR_FNAME:
                        case Lex_State.EXPR_DOT:
                        case Lex_State.EXPR_CLASS:
                            goto retry;
                        default:
                            break;
                    }
                    commastart = true;
                    lex_state = Lex_State.EXPR_BEG;
                    return '\n';

                case '*':
                    if ((c = nextc()) == '*')
                    {
                        if ((c = nextc()) == '=')
                        {
                            yylval.id = ID.intern(Tokens.tPOW);
                            lex_state = Lex_State.EXPR_BEG;
                            return (int)Tokens.tOP_ASGN;
                        }
                        pushback(c);
                        c = (int)Tokens.tPOW;
                    }
                    else
                    {
                        if (c == '=')
                        {
                            yylval.id = ID.intern('*');
                            lex_state = Lex_State.EXPR_BEG;
                            return (int)Tokens.tOP_ASGN;
                        }
                        pushback(c);
                        if (IS_ARG() && space_seen && !char.IsWhiteSpace((char)c))
                        {
                            yywarn("`*' interpreted as argument prefix");
                            c = (int)Tokens.tSTAR;
                        }
                        else if (lex_state == Lex_State.EXPR_BEG || lex_state == Lex_State.EXPR_MID)
                        {
                            c = (int)Tokens.tSTAR;
                        }
                        else
                        {
                            c = '*';
                        }
                    }
                    switch (lex_state)
                    {
                        case Lex_State.EXPR_FNAME:
                        case Lex_State.EXPR_DOT:
                            lex_state = Lex_State.EXPR_ARG; break;
                        default:
                            lex_state = Lex_State.EXPR_BEG; break;
                    }
                    return c;

                case '!':
                    lex_state = Lex_State.EXPR_BEG;
                    if ((c = nextc()) == '=')
                    {
                        return (int)Tokens.tNEQ;
                    }
                    if (c == '~')
                    {
                        return (int)Tokens.tNMATCH;
                    }
                    pushback(c);
                    return '!';

                case '=':
                    if (was_bol())
                    {
                        /* skip embedded rd document */
                        if (lex_buffer.Substring(lex_p, 5) == "begin" && char.IsWhiteSpace(lex_buffer[lex_p + 5]))
                        {
                            for (; ; )
                            {
                                lex_p = lex_pend;
                                c = nextc();
                                if (c == -1)
                                {
                                    yyerror("embedded document meets end of file");
                                    return (int)Tokens.EOF;
                                }
                                if (c != '=') continue;
                                if (lex_buffer.Substring(lex_p, 3) == "end" && (lex_p + 3 == lex_pend || char.IsWhiteSpace(lex_buffer[lex_p + 3])))
                                {
                                    break;
                                }
                            }
                            lex_p = lex_pend;
                            goto retry;
                        }
                    }

                    switch (lex_state)
                    {
                        case Lex_State.EXPR_FNAME:
                        case Lex_State.EXPR_DOT:
                            lex_state = Lex_State.EXPR_ARG; break;
                        default:
                            lex_state = Lex_State.EXPR_BEG; break;
                    }
                    if ((c = nextc()) == '=')
                    {
                        if ((c = nextc()) == '=')
                        {
                            return (int)Tokens.tEQQ;
                        }
                        pushback(c);
                        return (int)Tokens.tEQ;
                    }
                    if (c == '~')
                    {
                        return (int)Tokens.tMATCH;
                    }
                    else if (c == '>')
                    {
                        return (int)Tokens.tASSOC;
                    }
                    pushback(c);
                    return '=';

                case '<':
                    c = nextc();
                    if (c == '<' &&
                        lex_state != Lex_State.EXPR_END &&
                        lex_state != Lex_State.EXPR_DOT &&
                        lex_state != Lex_State.EXPR_ENDARG &&
                        lex_state != Lex_State.EXPR_CLASS &&
                        (!IS_ARG() || space_seen))
                    {
                        int token = heredoc_identifier();
                        if (token != 0) return token;
                    }
                    switch (lex_state)
                    {
                        case Lex_State.EXPR_FNAME:
                        case Lex_State.EXPR_DOT:
                            lex_state = Lex_State.EXPR_ARG; break;
                        default:
                            lex_state = Lex_State.EXPR_BEG; break;
                    }
                    if (c == '=')
                    {
                        if ((c = nextc()) == '>')
                        {
                            return (int)Tokens.tCMP;
                        }
                        pushback(c);
                        return (int)Tokens.tLEQ;
                    }
                    if (c == '<')
                    {
                        if ((c = nextc()) == '=')
                        {
                            yylval.id = ID.intern(Tokens.tLSHFT);
                            lex_state = Lex_State.EXPR_BEG;
                            return (int)Tokens.tOP_ASGN;
                        }
                        pushback(c);
                        return (int)Tokens.tLSHFT;
                    }
                    pushback(c);
                    return '<';

                case '>':
                    switch (lex_state)
                    {
                        case Lex_State.EXPR_FNAME:
                        case Lex_State.EXPR_DOT:
                            lex_state = Lex_State.EXPR_ARG; break;
                        default:
                            lex_state = Lex_State.EXPR_BEG; break;
                    }
                    if ((c = nextc()) == '=')
                    {
                        return (int)Tokens.tGEQ;
                    }
                    if (c == '>')
                    {
                        if ((c = nextc()) == '=')
                        {
                            yylval.id = ID.intern(Tokens.tRSHFT);
                            lex_state = Lex_State.EXPR_BEG;
                            return (int)Tokens.tOP_ASGN;
                        }
                        pushback(c);
                        return (int)Tokens.tRSHFT;
                    }
                    pushback(c);
                    return '>';

                case '"':
                    lex_strterm = new StringTerminator((int)string_type.str_dquote, '"', 0, 0, yylloc);
                    yylval.term = lex_strterm;
                    return (int)Tokens.tSTRING_BEG;

                case '`':
                    if (lex_state == Lex_State.EXPR_FNAME)
                    {
                        lex_state = Lex_State.EXPR_END;
                        return c;
                    }
                    if (lex_state == Lex_State.EXPR_DOT)
                    {
                        if (cmd_state)
                            lex_state = Lex_State.EXPR_CMDARG;
                        else
                            lex_state = Lex_State.EXPR_ARG;
                        return c;
                    }
                    lex_strterm = new StringTerminator((int)string_type.str_xquote, '`', 0, 0, yylloc);
                    yylval.term = lex_strterm;
                    return (int)Tokens.tXSTRING_BEG;

                case '\'':
                    lex_strterm = new StringTerminator((int)string_type.str_squote, '\'', 0, 0, yylloc);
                    yylval.term = lex_strterm;
                    return (int)Tokens.tSTRING_BEG;

                case '?':
                    if (lex_state == Lex_State.EXPR_END || lex_state == Lex_State.EXPR_ENDARG)
                    {
                        lex_state = Lex_State.EXPR_BEG;
                        return '?';
                    }
                    c = nextc();
                    if (c == -1)
                    {
                        yyerror("incomplete character syntax");
                        return (int)Tokens.EOF;
                    }
                    if (char.IsWhiteSpace((char)c))
                    {
                        if (!IS_ARG())
                        {
                            int c2 = 0;
                            switch (c)
                            {
                                case ' ':
                                    c2 = 's';
                                    break;
                                case '\n':
                                    c2 = 'n';
                                    break;
                                case '\t':
                                    c2 = 't';
                                    break;
                                case '\v':
                                    c2 = 'v';
                                    break;
                                case '\r':
                                    c2 = 'r';
                                    break;
                                case '\f':
                                    c2 = 'f';
                                    break;
                            }
                            if (c2 != 0)
                            {
                                yywarn("invalid character syntax; use ?\\{0}", (char)c2);
                            }
                        }
                        pushback(c);
                        lex_state = Lex_State.EXPR_BEG;
                        return '?';
                    }
                    else if (MultiByteChar.ismbchar((char)c))
                    {
                        yywarn("multibyte character literal not supported yet;");
                        pushback(c);
                        lex_state = Lex_State.EXPR_BEG;
                        return '?';
                    }
                    else if ((char.IsLetterOrDigit((char)c) || c == '_') && lex_p < lex_pend && is_identchar(lex_buffer[lex_p]))
                    {
                        pushback(c);
                        lex_state = Lex_State.EXPR_BEG;
                        return '?';
                    }
                    else if (c == '\\')
                    {
                        c = read_escape();
                    }
                    c &= 0xff;
                    lex_state = Lex_State.EXPR_END;
                    yylval.node = new VALUE(c, yylloc);
                    return (int)Tokens.tINTEGER;

                case '&':
                    if ((c = nextc()) == '&')
                    {
                        lex_state = Lex_State.EXPR_BEG;
                        if ((c = nextc()) == '=')
                        {
                            yylval.id = ID.intern(Tokens.tANDOP);
                            lex_state = Lex_State.EXPR_BEG;
                            return (int)Tokens.tOP_ASGN;
                        }
                        pushback(c);
                        return (int)Tokens.tANDOP;
                    }
                    else if (c == '=')
                    {
                        yylval.id = ID.intern('&');
                        lex_state = Lex_State.EXPR_BEG;
                        return (int)Tokens.tOP_ASGN;
                    }
                    pushback(c);
                    if (IS_ARG() && space_seen && !char.IsWhiteSpace((char)c))
                    {
                        yywarn("`&' interpreted as argument prefix");
                        c = (int)Tokens.tAMPER;
                    }
                    else if (lex_state == Lex_State.EXPR_BEG || lex_state == Lex_State.EXPR_MID)
                    {
                        c = (int)Tokens.tAMPER;
                    }
                    else
                    {
                        c = '&';
                    }
                    switch (lex_state)
                    {
                        case Lex_State.EXPR_FNAME:
                        case Lex_State.EXPR_DOT:
                            lex_state = Lex_State.EXPR_ARG;
                            break;
                        default:
                            lex_state = Lex_State.EXPR_BEG;
                            break;
                    }
                    return c;

                case '|':
                    if ((c = nextc()) == '|')
                    {
                        lex_state = Lex_State.EXPR_BEG;
                        if ((c = nextc()) == '=')
                        {
                            yylval.id = ID.intern(Tokens.tOROP);
                            lex_state = Lex_State.EXPR_BEG;
                            return (int)Tokens.tOP_ASGN;
                        }
                        pushback(c);
                        return (int)Tokens.tOROP;
                    }
                    if (c == '=')
                    {
                        yylval.id = ID.intern('|');
                        lex_state = Lex_State.EXPR_BEG;
                        return (int)Tokens.tOP_ASGN;
                    }
                    if (lex_state == Lex_State.EXPR_FNAME || lex_state == Lex_State.EXPR_DOT)
                    {
                        lex_state = Lex_State.EXPR_ARG;
                    }
                    else
                    {
                        lex_state = Lex_State.EXPR_BEG;
                    }
                    pushback(c);
                    return '|';

                case '+':
                    c = nextc();
                    if (lex_state == Lex_State.EXPR_FNAME || lex_state == Lex_State.EXPR_DOT)
                    {
                        lex_state = Lex_State.EXPR_ARG;
                        if (c == '@')
                        {
                            return (int)Tokens.tUPLUS;
                        }
                        pushback(c);
                        return '+';
                    }
                    if (c == '=')
                    {
                        yylval.id = ID.intern('+');
                        lex_state = Lex_State.EXPR_BEG;
                        return (int)Tokens.tOP_ASGN;
                    }
                    if (lex_state == Lex_State.EXPR_BEG || lex_state == Lex_State.EXPR_MID ||
                        (IS_ARG() && space_seen && !char.IsWhiteSpace((char)c)))
                    {
                        if (IS_ARG()) arg_ambiguous();
                        lex_state = Lex_State.EXPR_BEG;
                        pushback(c);
                        if (char.IsDigit((char)c))
                        {
                            c = '+';
                            goto start_num;
                        }
                        return (int)Tokens.tUPLUS;
                    }
                    lex_state = Lex_State.EXPR_BEG;
                    pushback(c);
                    return '+';

                case '-':
                    c = nextc();
                    if (lex_state == Lex_State.EXPR_FNAME || lex_state == Lex_State.EXPR_DOT)
                    {
                        lex_state = Lex_State.EXPR_ARG;
                        if (c == '@')
                        {
                            return (int)Tokens.tUMINUS;
                        }
                        pushback(c);
                        return '-';
                    }
                    if (c == '=')
                    {
                        yylval.id = ID.intern('-');
                        lex_state = Lex_State.EXPR_BEG;
                        return (int)Tokens.tOP_ASGN;
                    }
                    if (lex_state == Lex_State.EXPR_BEG || lex_state == Lex_State.EXPR_MID ||
                        (IS_ARG() && space_seen && !char.IsWhiteSpace((char)c)))
                    {
                        if (IS_ARG()) arg_ambiguous();
                        lex_state = Lex_State.EXPR_BEG;
                        pushback(c);
                        if (char.IsDigit((char)c))
                        {
                            return (int)Tokens.tUMINUS_NUM;
                        }
                        return (int)Tokens.tUMINUS;
                    }
                    lex_state = Lex_State.EXPR_BEG;
                    pushback(c);
                    return '-';

                case '.':
                    lex_state = Lex_State.EXPR_BEG;
                    if ((c = nextc()) == '.')
                    {
                        if ((c = nextc()) == '.')
                        {
                            return (int)Tokens.tDOT3;
                        }
                        pushback(c);
                        return (int)Tokens.tDOT2;
                    }
                    pushback(c);
                    if (char.IsDigit((char)c))
                    {
                        yyerror("no .<digit> floating literal anymore; put 0 before dot");
                    }
                    lex_state = Lex_State.EXPR_DOT;
                    return '.';


                case '0':
                case '1':
                case '2':
                case '3':
                case '4':
                case '5':
                case '6':
                case '7':
                case '8':
                case '9':
                start_num:
                    {
                        bool is_float = false, seen_point = false, seen_e = false;
                        int nondigit = 0;

                        lex_state = Lex_State.EXPR_END;
                        newtok();
                        if (c == '-' || c == '+')
                        {
                            tokadd((char)c);
                            c = nextc();
                        }
                        if (c == '0')
                        {
                            int start = toklen();
                            c = nextc();
                            if (c == 'x' || c == 'X')
                            {
                                /* hexadecimal */
                                c = nextc();
                                if (char.IsDigit((char)c) || ('A' <= char.ToUpperInvariant((char)c) && char.ToUpperInvariant((char)c) <= 'F'))
                                {
                                    do
                                    {
                                        if (c == '_')
                                        {
                                            if (nondigit != 0) break;
                                            nondigit = c;
                                            continue;
                                        }
                                        if (!(char.IsDigit((char)c) || ('A' <= char.ToUpperInvariant((char)c) && char.ToUpperInvariant((char)c) <= 'F')))
                                            break;
                                        nondigit = 0;
                                        tokadd((char)c);
                                    } while ((c = nextc()) != -1);
                                }
                                pushback(c);
                                tokfix();
                                if (toklen() == start)
                                {
                                    yyerror("numeric literal without digits");
                                }
                                else if (nondigit != 0) goto trailing_uc;
                                yylval.node = VALUE.StringToNumber(tok(), 16, yylloc);
                                return (int)Tokens.tINTEGER;
                            }
                            if (c == 'b' || c == 'B')
                            {
                                /* binary */
                                c = nextc();
                                if (c == '0' || c == '1')
                                {
                                    do
                                    {
                                        if (c == '_')
                                        {
                                            if (nondigit != 0) break;
                                            nondigit = c;
                                            continue;
                                        }
                                        if (c != '0' && c != '1') break;
                                        nondigit = 0;
                                        tokadd((char)c);
                                    } while ((c = nextc()) != -1);
                                }
                                pushback(c);
                                tokfix();
                                if (toklen() == start)
                                {
                                    yyerror("numeric literal without digits");
                                }
                                else if (nondigit != 0) goto trailing_uc;
                                yylval.node = VALUE.StringToNumber(tok(), 2, yylloc);
                                return (int)Tokens.tINTEGER;
                            }
                            if (c == 'd' || c == 'D')
                            {
                                /* decimal */
                                c = nextc();
                                if (char.IsDigit((char)c))
                                {
                                    do
                                    {
                                        if (c == '_')
                                        {
                                            if (nondigit != 0) break;
                                            nondigit = c;
                                            continue;
                                        }
                                        if (!char.IsDigit((char)c)) break;
                                        nondigit = 0;
                                        tokadd((char)c);
                                    } while ((c = nextc()) != -1);
                                }
                                pushback(c);
                                tokfix();
                                if (toklen() == start)
                                {
                                    yyerror("numeric literal without digits");
                                }
                                else if (nondigit != 0) goto trailing_uc;
                                yylval.node = VALUE.StringToNumber(tok(), 10, yylloc);
                                return (int)Tokens.tINTEGER;
                            }
                            if (c == 'o' || c == 'O')
                            {
                                /* prefixed octal */
                                c = nextc();
                                if (c == '_')
                                {
                                    yyerror("numeric literal without digits");
                                }
                            }
                            if (c == '_' || (c >= '0' && c <= '7'))
                            {
                            /* octal */
                                do
                                {
                                    if (c == '_')
                                    {
                                        if (nondigit != 0) break;
                                        nondigit = c;
                                        continue;
                                    }
                                    if (c < '0' || c > '7') break;
                                    nondigit = 0;
                                    tokadd((char)c);
                                } while ((c = nextc()) != -1);
                                if (toklen() > start)
                                {
                                    pushback(c);
                                    tokfix();
                                    if (nondigit != 0) goto trailing_uc;
                                    yylval.node = VALUE.StringToNumber(tok(), 8, yylloc);
                                    return (int)Tokens.tINTEGER;
                                }
                                if (nondigit != 0)
                                {
                                    pushback(c);
                                    goto trailing_uc;
                                }
                            }
                            if (c > '7' && c <= '9')
                            {
                                yyerror("Illegal octal digit");
                            }
                            else if (c == '.' || c == 'e' || c == 'E')
                            {
                                tokadd('0');
                            }
                            else
                            {
                                pushback(c);
                                yylval.node = new VALUE(0, yylloc);
                                return (int)Tokens.tINTEGER;
                            }
                        }

                        for (; ; )
                        {
                            switch (c)
                            {
                                case '0':
                                case '1':
                                case '2':
                                case '3':
                                case '4':
                                case '5':
                                case '6':
                                case '7':
                                case '8':
                                case '9':
                                    nondigit = 0;
                                    tokadd((char)c);
                                    break;

                                case '.':
                                    if (nondigit != 0) goto trailing_uc;
                                    if (seen_point || seen_e)
                                    {
                                        goto decode_num;
                                    }
                                    else
                                    {
                                        int c0 = nextc();
                                        if (!char.IsDigit((char)c0))
                                        {
                                            pushback(c0);
                                            goto decode_num;
                                        }
                                        c = c0;
                                    }
                                    tokadd('.');
                                    tokadd((char)c);
                                    is_float = true;
                                    seen_point = true;
                                    nondigit = 0;
                                    break;

                                case 'e':
                                case 'E':
                                    if (nondigit != 0)
                                    {
                                        pushback(c);
                                        c = nondigit;
                                        goto decode_num;
                                    }
                                    if (seen_e)
                                    {
                                        goto decode_num;
                                    }
                                    tokadd((char)c);
                                    seen_e = true;
                                    is_float = true;
                                    nondigit = c;
                                    c = nextc();
                                    if (c != '-' && c != '+') continue;
                                    tokadd((char)c);
                                    nondigit = c;
                                    break;

                                case '_':    /* `_' in number just ignored */
                                    if (nondigit != 0) goto decode_num;
                                    nondigit = c;
                                    break;

                                default:
                                    goto decode_num;
                            }
                            c = nextc();
                        }

                    decode_num:
                        pushback(c);
                        tokfix();

                    trailing_uc:
                        if (nondigit != 0)
                            yyerror("trailing '{0}' in number", nondigit);

                        if (is_float)
                        {
                            double d = double.Parse(tok(), CultureInfo.InvariantCulture);
                            yylval.node = new VALUE(d, yylloc);
                            return (int)Tokens.tFLOAT;
                        }
                        yylval.node = VALUE.StringToNumber(tok(), 10, yylloc);
                        return (int)Tokens.tINTEGER;
                    }

                case ']':
                case '}':
                case ')':
                    COND_LEXPOP();
                    CMDARG_LEXPOP();
                    lex_state = Lex_State.EXPR_END;
                    return c;

                case ':':
                    c = nextc();
                    if (c == ':')
                    {
                        if (lex_state == Lex_State.EXPR_BEG || lex_state == Lex_State.EXPR_MID ||
                        lex_state == Lex_State.EXPR_CLASS || (IS_ARG() && space_seen))
                        {
                            lex_state = Lex_State.EXPR_BEG;
                            return (int)Tokens.tCOLON3;
                        }
                        lex_state = Lex_State.EXPR_DOT;
                        return (int)Tokens.tCOLON2;
                    }
                    if (lex_state == Lex_State.EXPR_END || lex_state == Lex_State.EXPR_ENDARG || char.IsWhiteSpace((char)c))
                    {
                        pushback(c);
                        lex_state = Lex_State.EXPR_BEG;
                        return ':';
                    }
                    switch (c)
                    {
                        case '\'':
                            lex_strterm = new StringTerminator((int)string_type.str_ssym, c, 0, 0, yylloc);
                            break;
                        case '"':
                            lex_strterm = new StringTerminator((int)string_type.str_dsym, c, 0, 0, yylloc);
                            break;
                        default:
                            pushback(c);
                            break;
                    }
                    lex_state = Lex_State.EXPR_FNAME;
                    yylval.term = lex_strterm;
                    return (int)Tokens.tSYMBEG;

                case '/':
                    if (lex_state == Lex_State.EXPR_BEG || lex_state == Lex_State.EXPR_MID)
                    {
                        lex_strterm = new StringTerminator((int)string_type.str_regexp, '/', 0, 0, yylloc);
                        yylval.term = lex_strterm;
                        return (int)Tokens.tREGEXP_BEG;
                    }
                    if ((c = nextc()) == '=')
                    {
                        yylval.id = ID.intern('/');
                        lex_state = Lex_State.EXPR_BEG;
                        return (int)Tokens.tOP_ASGN;
                    }
                    pushback(c);
                    if (IS_ARG() && space_seen)
                    {
                        if (!char.IsWhiteSpace((char)c))
                        {
                            arg_ambiguous();
                            lex_strterm = new StringTerminator((int)string_type.str_regexp, '/', 0, 0, yylloc);
                            yylval.term = lex_strterm;
                            return (int)Tokens.tREGEXP_BEG;
                        }
                    }
                    switch (lex_state)
                    {
                        case Lex_State.EXPR_FNAME:
                        case Lex_State.EXPR_DOT:
                            lex_state = Lex_State.EXPR_ARG; break;
                        default:
                            lex_state = Lex_State.EXPR_BEG; break;
                    }
                    return '/';

                case '^':
                    if ((c = nextc()) == '=')
                    {
                        yylval.id = ID.intern('^');
                        lex_state = Lex_State.EXPR_BEG;
                        return (int)Tokens.tOP_ASGN;
                    }
                    switch (lex_state)
                    {
                        case Lex_State.EXPR_FNAME:
                        case Lex_State.EXPR_DOT:
                            lex_state = Lex_State.EXPR_ARG; break;
                        default:
                            lex_state = Lex_State.EXPR_BEG; break;
                    }
                    pushback(c);
                    return '^';

                case ';':
                    commastart = true;
                    goto case ',';
                case ',':
                    lex_state = Lex_State.EXPR_BEG;
                    return c;

                case '~':
                    if (lex_state == Lex_State.EXPR_FNAME || lex_state == Lex_State.EXPR_DOT)
                    {
                        if ((c = nextc()) != '@')
                        {
                            pushback(c);
                        }
                    }
                    switch (lex_state)
                    {
                        case Lex_State.EXPR_FNAME:
                        case Lex_State.EXPR_DOT:
                            lex_state = Lex_State.EXPR_ARG; break;
                        default:
                            lex_state = Lex_State.EXPR_BEG; break;
                    }
                    return '~';

                case '(':
                    commastart = true;
                    if (lex_state == Lex_State.EXPR_BEG || lex_state == Lex_State.EXPR_MID)
                    {
                        c = (int)Tokens.tLPAREN;
                    }
                    else if (space_seen)
                    {
                        if (lex_state == Lex_State.EXPR_CMDARG)
                        {
                            c = (int)Tokens.tLPAREN_ARG;
                        }
                        else if (lex_state == Lex_State.EXPR_ARG)
                        {
                            yywarn("don't put space before argument parentheses");
                            c = '(';
                        }
                    }
                    COND_PUSH(0);
                    CMDARG_PUSH(0);
                    lex_state = Lex_State.EXPR_BEG;
                    return c;

                case '[':
                    if (lex_state == Lex_State.EXPR_FNAME || lex_state == Lex_State.EXPR_DOT)
                    {
                        lex_state = Lex_State.EXPR_ARG;
                        if ((c = nextc()) == ']')
                        {
                            if ((c = nextc()) == '=')
                            {
                                return (int)Tokens.tASET;
                            }
                            pushback(c);
                            return (int)Tokens.tAREF;
                        }
                        pushback(c);
                        return '[';
                    }
                    else if (lex_state == Lex_State.EXPR_BEG || lex_state == Lex_State.EXPR_MID)
                    {
                        c = (int)Tokens.tLBRACK;
                    }
                    else if (IS_ARG() && space_seen)
                    {
                        c = (int)Tokens.tLBRACK;
                    }
                    lex_state = Lex_State.EXPR_BEG;
                    COND_PUSH(0);
                    CMDARG_PUSH(0);
                    return c;

                case '{':
                    if (IS_ARG() || lex_state == Lex_State.EXPR_END)
                        c = '{';          /* block (primary) */
                    else if (lex_state == Lex_State.EXPR_ENDARG)
                        c = (int)Tokens.tLBRACE_ARG;  /* block (expr) */
                    else
                        c = (int)Tokens.tLBRACE;      /* hash */
                    COND_PUSH(0);
                    CMDARG_PUSH(0);
                    lex_state = Lex_State.EXPR_BEG;
                    return c;

                case '\\':
                    c = nextc();
                    if (c == '\n')
                    {
                        space_seen = true;
                        goto retry; /* skip \\n */
                    }
                    pushback(c);
                    return '\\';

                case '%':
                    if (lex_state == Lex_State.EXPR_BEG || lex_state == Lex_State.EXPR_MID)
                    {
                        c = nextc();
                        return quotation(c);
                    }
                    if ((c = nextc()) == '=')
                    {
                        yylval.id = ID.intern('%');
                        lex_state = Lex_State.EXPR_BEG;
                        return (int)Tokens.tOP_ASGN;
                    }
                    if (IS_ARG() && space_seen && !char.IsWhiteSpace((char)c))
                        return quotation(c);

                    switch (lex_state)
                    {
                        case Lex_State.EXPR_FNAME:
                        case Lex_State.EXPR_DOT:
                            lex_state = Lex_State.EXPR_ARG; break;
                        default:
                            lex_state = Lex_State.EXPR_BEG; break;
                    }
                    pushback(c);
                    return '%';

                case '$':
                    lex_state = Lex_State.EXPR_END;
                    newtok();
                    c = nextc();
                    switch (c)
                    {
                        case '_':        /* $_: last read line string */
                            c = nextc();
                            if (is_identchar((char)c))
                            {
                                tokadd('$');
                                tokadd('_');
                                break;
                            }
                            pushback(c);
                            c = '_';
                            goto case '*';
                        case '~':        /* $~: match-data */
                            goto case '*';
                        /* fall through */
                        case '*':        /* $*: argv */
                        case '$':        /* $$: pid */
                        case '?':        /* $?: last status */
                        case '!':        /* $!: error string */
                        case '@':        /* $@: error position */
                        case '/':        /* $/: input record separator */
                        case '\\':        /* $\: output record separator */
                        case ';':        /* $;: field separator */
                        case ',':        /* $,: output field separator */
                        case '.':        /* $.: last read line number */
                        case '=':        /* $=: ignorecase */
                        case ':':        /* $:: load path */
                        case '<':        /* $<: reading filename */
                        case '>':        /* $>: default output handle */
                        case '\"':        /* $": already loaded files */
                            tokadd('$');
                            tokadd((char)c);
                            tokfix();
                            yylval.id = ID.intern(tok());
                            return (int)Tokens.tGVAR;

                        case '-':
                            tokadd('$');
                            tokadd((char)c);
                            c = nextc();
                            tokadd((char)c);
                            tokfix();
                            yylval.id = ID.intern(tok());
                            /* xxx shouldn't check if valid option variable */
                            return (int)Tokens.tGVAR;

                        case '&':        /* $&: last match */
                        case '`':        /* $`: string before last match */
                        case '\'':        /* $': string after last match */
                        case '+':        /* $+: string matches last paren. */
                            yylval.node = new BACK_REF(parser.CurrentScope, (char)c, yylloc);
                            return (int)Tokens.tBACK_REF;

                        case '1':
                        case '2':
                        case '3':
                        case '4':
                        case '5':
                        case '6':
                        case '7':
                        case '8':
                        case '9':
                            tokadd('$');
                            do
                            {
                                tokadd((char)c);
                                c = nextc();
                            } while (char.IsDigit((char)c));
                            pushback(c);
                            tokfix();
                            yylval.node = new NTH_REF(parser.CurrentScope, int.Parse(tok().Substring(1), CultureInfo.InvariantCulture), yylloc);
                            return (int)Tokens.tNTH_REF;

                        default:
                            if (!is_identchar((char)c))
                            {
                                pushback(c);
                                return '$';
                            }
                            goto case '0';
                        case '0':
                            tokadd('$');
                            break;
                    }
                    break;

                case '@':
                    c = nextc();
                    newtok();
                    tokadd('@');
                    if (c == '@')
                    {
                        tokadd('@');
                        c = nextc();
                    }
                    if (char.IsDigit((char)c))
                    {
                        if (tokenbuf.Length == 1)
                        {
                            yyerror("`@{0}' is not allowed as an instance variable name", (char)c);
                        }
                        else
                        {
                            yyerror("`@@{0}' is not allowed as a class variable name", (char)c);
                        }
                    }
                    if (!is_identchar((char)c))
                    {
                        pushback(c);
                        return '@';
                    }
                    break;

                case '_':
                    if (was_bol() && whole_match_p("__END__", 0))
                    {
                        lex_lastline = null;
                        return (int)Tokens.EOF;
                    }
                    newtok();
                    break;

                default:
                    if (!is_identchar((char)c))
                    {
                        yyerror("Invalid char '{0}' in expression", (char)c);
                        goto retry;
                    }

                    newtok();
                    break;
            }

            do
            {
                tokadd((char)c);
                if (MultiByteChar.ismbchar((char)c))
                {
                    int i, len = MultiByteChar.mbclen((char)c) - 1;

                    for (i = 0; i < len; i++)
                    {
                        c = nextc();
                        tokadd((char)c);
                    }
                }
                c = nextc();
            } while (is_identchar((char)c));

            if ((c == '!' || c == '?') && is_identchar(tok()[0]) && !peek('='))
                tokadd((char)c);
            else
                pushback(c);

            tokfix();

            {
                int result = 0;

                switch (tok()[0])
                {
                    case '$':
                        lex_state = Lex_State.EXPR_END;
                        result = (int)Tokens.tGVAR;
                        break;
                    case '@':
                        lex_state = Lex_State.EXPR_END;
                        if (tok()[1] == '@')
                            result = (int)Tokens.tCVAR;
                        else
                            result = (int)Tokens.tIVAR;
                        break;

                    default:
                        if (toklast() == '!' || toklast() == '?')
                        {
                            result = (int)Tokens.tFID;
                        }
                        else
                        {
                            if (lex_state == Lex_State.EXPR_FNAME)
                            {
                                if ((c = nextc()) == '=' && !peek('~') && !peek('>') &&
                                (!peek('=') || (lex_p + 1 < lex_pend && lex_buffer[lex_p + 1] == '>')))
                                {
                                    result = (int)Tokens.tIDENTIFIER;
                                    tokadd((char)c);
                                    tokfix();
                                }
                                else
                                {
                                    pushback(c);
                                }
                            }
                            if (result == 0 && char.IsUpper(tok()[0]))
                                result = (int)Tokens.tCONSTANT;
                            else
                                result = (int)Tokens.tIDENTIFIER;
                        }

                        if (lex_state != Lex_State.EXPR_DOT)
                        {
                            keyword kw;

                            /* See if it is a reserved word.  */
                            kw = reserved_word(tok());
                            if (kw != null)
                            {
                                Lex_State state = lex_state;
                                lex_state = kw.state;
                                if (state == Lex_State.EXPR_FNAME)
                                {
                                    yylval.id = ID.intern(kw.name);
                                }
                                if (kw.id[0] == (int)Tokens.kDO)
                                {
                                    if (COND_P()) return (int)Tokens.kDO_COND;
                                    if (CMDARG_P() && state != Lex_State.EXPR_CMDARG)
                                        return (int)Tokens.kDO_BLOCK;
                                    if (state == Lex_State.EXPR_ENDARG)
                                        return (int)Tokens.kDO_BLOCK;
                                    return (int)Tokens.kDO;
                                }
                                if (state == Lex_State.EXPR_BEG)
                                    return kw.id[0];
                                else
                                {
                                    if (kw.id[0] != kw.id[1])
                                        lex_state = Lex_State.EXPR_BEG;
                                    return kw.id[1];
                                }
                            }
                        }

                        if (lex_state == Lex_State.EXPR_BEG ||
                            lex_state == Lex_State.EXPR_MID ||
                            lex_state == Lex_State.EXPR_DOT ||
                            lex_state == Lex_State.EXPR_ARG ||
                            lex_state == Lex_State.EXPR_CMDARG)
                        {
                            if (cmd_state)
                                lex_state = Lex_State.EXPR_CMDARG;
                            else
                                lex_state = Lex_State.EXPR_ARG;
                        }
                        else
                            lex_state = Lex_State.EXPR_END;
                        break;
                }

                yylval.id = ID.intern(tok());

                if (ID.Scope(yylval.id) == ID_Scope.LOCAL && parser.CurrentScope.has_local(yylval.id))
                {
                    lex_state = Lex_State.EXPR_END;
                }

                return result;
            }
        }

        private String stream_reader(object s)
        {
            System.IO.TextReader reader = (System.IO.TextReader)s;

            string line = reader.ReadLine();

            if (line == null)
                return null;
            else
                return new String(line + "\n");
        }


        private int lex_gets_ptr;

        private String lex_get_str(object s)
        {
            string str = ((String)s).value;

            int beg, end, pend;

            beg = 0;
            if (lex_gets_ptr != 0)
            {
                if (str.Length == lex_gets_ptr)
                    return null;
                beg += lex_gets_ptr;
            }
            pend = str.Length;
            end = beg;
            while (end < pend)
            {
                if (str[end++] == '\n') break;
            }
            lex_gets_ptr = end;
            return new String(str.Substring(beg, end - beg));
        }

        private String lex_getline()
        {
            String line = lex_gets(lex_input);
            return line;
        }


        private bool at_eol()
        {
            return (lex_p == lex_pend);
        }


        internal void set_start()
        {
            if (at_eol())
            {
                start_line = sourceline + 1;
                start_column = 0;
            }
            else
            {
                start_line = sourceline;
                start_column = lex_p;
            }

            yytext = new StringBuilder();
        }

        internal int nextc()
        {
            int c;

            if (lex_p == lex_pend)
            {
                if (lex_input != null)
                {
                    String v = lex_getline();

                    if (v == null) return -1;
                    if (heredoc_end > 0)
                    {
                        sourceline = heredoc_end;
                        heredoc_end = 0;
                    }
                    sourceline++;
                    lex_buffer = v.value;
                    lex_p = 0;
                    lex_pend = lex_buffer.Length;
                    lex_lastline = v;
                }
                else
                {
                    lex_lastline = null;
                    return -1;
                }
            }
            if (lex_p < lex_buffer.Length)
            {
                System.Diagnostics.Debug.Assert(0 <= lex_p && lex_p < lex_buffer.Length);
                c = (char)lex_buffer[lex_p++];
            }
            else
                c = '\0';

            if (c == '\r' && lex_p < lex_pend && lex_buffer[lex_p] == '\n')
            {
                lex_p++;
                c = '\n';
            }

            yytext.Append((char)c);

            return c;
        }

        private void pushback(int c)
        {
            yytext.Remove(yytext.Length - 1, 1);
            if (c == -1) return;
            if (lex_p > 0) lex_p--;
        }

        private bool was_bol() { return (lex_p == 1); }
        private bool peek(char c) { return (lex_p != lex_pend && (c) == lex_buffer[lex_p]); }

        private void tokfix() { }
        private string tok() { return tokenbuf.ToString(); }
        private int toklen() { return tokenbuf.Length; }
        private int toklast() { return (tokenbuf.Length > 0 ? tokenbuf[tokenbuf.Length - 1] : 0); }

        private void newtok()
        {
            if (tokenbuf == null)
                tokenbuf = new StringBuilder();
            else
                tokenbuf.Length = 0;
        }

        private void tokadd(char c)
        {
            tokenbuf.Append(c);
        }

        private int read_escape()
        {
            int c;

            switch (c = nextc())
            {
                case '\\':    /* Backslash */
                    return c;

                case 'n':    /* newline */
                    return '\n';

                case 't':    /* horizontal tab */
                    return '\t';

                case 'r':    /* carriage-return */
                    return '\r';

                case 'f':    /* form-feed */
                    return '\f';

                case 'v':    /* vertical tab */
                    return '\v';

                case 'a':    /* alarm(bell) */
                    return '\a';

                //case 'e':    /* escape */ fixme
                //return '\e';          fixme

                case '0':
                case '1':
                case '2':
                case '3': /* octal constant */
                case '4':
                case '5':
                case '6':
                case '7':
                    {
                        int numlen;

                        pushback(c);
                        c = (int)scan_oct(lex_buffer.Substring(lex_p), 3, out numlen);
                        lex_p += numlen;
                    }
                    return c;

                case 'x':    /* hex constant */
                    {
                        int numlen;

                        c = (int)scan_hex(lex_buffer.Substring(lex_p), 2, out numlen);
                        if (numlen == 0)
                        {
                            yyerror("Invalid escape character syntax");
                            return 0;
                        }
                        lex_p += numlen;
                    }
                    return c;

                case 'b':    /* backspace */
                    return '\b'; //'\010';

                case 's':    /* space */
                    return ' ';

                case 'M':
                    if ((c = nextc()) != '-')
                    {
                        yyerror("Invalid escape character syntax");
                        pushback(c);
                        return '\0';
                    }
                    if ((c = nextc()) == '\\')
                    {
                        return read_escape() | 0x80;
                    }
                    else if (c == -1) goto eof;
                    else
                    {
                        return ((c & 0xff) | 0x80);
                    }

                case 'C':
                    if ((c = nextc()) != '-')
                    {
                        yyerror("Invalid escape character syntax");
                        pushback(c);
                        return '\0';
                    }
                    goto case 'c';

                case 'c':
                    if ((c = nextc()) == '\\')
                    {
                        c = read_escape();
                    }
                    else if (c == '?')
                        return 0177;
                    else if (c == -1) goto eof;
                    return c & 0x9f;


                case -1:
                eof:
                    yyerror("Invalid escape character syntax");
                    return '\0';

                default:
                    return c;
            }
        }

        private int tokadd_escape(int term)
        {
            int c;

            switch (c = nextc())
            {
                case '\n':
                    return 0;        /* just ignore */

                case '0':
                case '1':
                case '2':
                case '3': /* octal constant */
                case '4':
                case '5':
                case '6':
                case '7':
                    {
                        int i;

                        tokadd('\\');
                        tokadd((char)c);
                        for (i = 0; i < 2; i++)
                        {
                            c = nextc();
                            if (c == -1) goto eof;
                            if (c < '0' || '7' < c)
                            {
                                pushback(c);
                                break;
                            }
                            tokadd((char)c);
                        }
                    }
                    return 0;

                case 'x':    /* hex constant */
                    {
                        int numlen;

                        tokadd('\\');
                        tokadd((char)c);
                        scan_hex(lex_buffer.Substring(lex_p), 2, out numlen);
                        if (numlen == 0)
                        {
                            yyerror("Invalid escape character syntax");
                            return -1;
                        }
                        while (numlen-- != 0)
                            tokadd((char)nextc());
                    }
                    return 0;

                case 'M':
                    if ((c = nextc()) != '-')
                    {
                        yyerror("Invalid escape character syntax");
                        pushback(c);
                        return 0;
                    }
                    tokadd('\\'); tokadd('M'); tokadd('-');
                    goto escaped;

                case 'C':
                    if ((c = nextc()) != '-')
                    {
                        yyerror("Invalid escape character syntax");
                        pushback(c);
                        return 0;
                    }
                    tokadd('\\'); tokadd('C'); tokadd('-');
                    goto escaped;

                case 'c':
                    tokadd('\\'); tokadd('c');
                escaped:
                    if ((c = nextc()) == '\\')
                    {
                        return tokadd_escape(term);
                    }
                    else if (c == -1) goto eof;
                    tokadd((char)c);
                    return 0;


                case -1:
                eof:
                    yyerror("Invalid escape character syntax");
                    return -1;

                default:
                    if (c != '\\' || c != term)
                        tokadd('\\');
                    tokadd((char)c);
                    break;
            }
            return 0;
        }


        private int regx_options()
        {
            int kcode = 0;
            int options = 0;
            int c;

            newtok();
            while (char.IsLetter((char)(c = nextc())))
            {
                switch (c)
                {
                    case 'i':
                        options |= (int)Regexp.RE_OPTION.IGNORECASE;
                        break;
                    case 'x':
                        options |= (int)Regexp.RE_OPTION.EXTENDED;
                        break;
                    case 'm':
                        options |= (int)Regexp.RE_OPTION.MULTILINE;
                        break;
                    case 'o':
                        // Fixme: Once option not implemented.
                        break;
                    case 'n':
                        kcode |= (int)Regexp.KCODE.FIXED;
                        break;
                    case 'e':
                        kcode = (int)Regexp.KCODE.EUC;
                        break;
                    case 's':
                        kcode = (int)Regexp.KCODE.SJIS;
                        break;
                    case 'u':
                        kcode = (int)Regexp.KCODE.UTF8;
                        break;
                    default:
                        tokadd((char)c);
                        break;
                }
            }
            pushback(c);
            if (toklen() > 0)
            {
                tokfix();
                yyerror("unknown regexp option{0} - {1}",
                        toklen() > 1 ? "s" : "", tok());
            }
            return options | kcode;
        }

        private const int STR_FUNC_ESCAPE = 0x01;
        private const int STR_FUNC_EXPAND = 0x02;
        private const int STR_FUNC_REGEXP = 0x04;
        private const int STR_FUNC_QWORDS = 0x08;
        private const int STR_FUNC_SYMBOL = 0x10;
        private const int STR_FUNC_INDENT = 0x20;

        private int tokadd_string(int func, int term, int paren, ref object nest)
        {
            int c;

            while ((c = nextc()) != -1)
            {
                if (paren != 0 && c == paren)
                {
                    nest = (int)nest + 1;
                }
                else if (c == term)
                {
                    if (nest == null || (int)nest == 0)
                    {
                        pushback(c);
                        break;
                    }
                    nest = (int)nest - 1;
                }
                else if (((func & STR_FUNC_EXPAND) != 0) && c == '#' && lex_p < lex_pend)
                {
                    int c2 = lex_buffer[lex_p];
                    if (c2 == '$' || c2 == '@' || c2 == '{')
                    {
                        pushback(c);
                        break;
                    }
                }
                else if (c == '\\')
                {
                    c = nextc();
                    switch (c)
                    {
                        case '\n':
                            if ((func & STR_FUNC_QWORDS) != 0) break;
                            if ((func & STR_FUNC_EXPAND) != 0) continue;
                            tokadd('\\');
                            break;

                        case '\\':
                            if ((func & STR_FUNC_ESCAPE) != 0) tokadd((char)c);
                            break;

                        default:
                            if ((func & STR_FUNC_REGEXP) != 0)
                            {
                                pushback(c);
                                if (tokadd_escape(term) < 0)
                                    return -1;
                                continue;
                            }
                            else if ((func & STR_FUNC_EXPAND) != 0)
                            {
                                pushback(c);
                                if ((func & STR_FUNC_ESCAPE) != 0) tokadd('\\');
                                c = read_escape();
                            }
                            else if ((func & STR_FUNC_QWORDS) != 0 && char.IsWhiteSpace((char)c))
                            {
                                /* ignore backslashed spaces in %w */
                            }
                            else if (c != term && !(paren != 0 && c == paren))
                            {
                                tokadd('\\');
                            }
                            break;
                    }
                }
                else if (MultiByteChar.ismbchar((char)c))
                {
                    int i, len = MultiByteChar.mbclen((char)c) - 1;

                    for (i = 0; i < len; i++)
                    {
                        tokadd((char)c);
                        c = nextc();
                    }
                }
                else if ((func & STR_FUNC_QWORDS) != 0 && char.IsWhiteSpace((char)c))
                {
                    pushback(c);
                    break;
                }
                if (c == 0 && (func & STR_FUNC_SYMBOL) != 0)
                {
                    func &= ~STR_FUNC_SYMBOL;
                    yyerror("symbol cannot contain '\\0'");
                    continue;
                }
                tokadd((char)c);
            }
            return c;
        }

        private int parse_string(StringTerminator quote)
        {
            int func = (int)quote.func;
            int term = quote.term;
            int paren = quote.paren;
            int c;
            bool space = false;

            if (func == -1) return (int)Tokens.tSTRING_END;
            c = nextc();
            if ((func & STR_FUNC_QWORDS) != 0 && char.IsWhiteSpace((char)c))
            {
                do { c = nextc(); } while (char.IsWhiteSpace((char)c));
                space = true;
            }
            if (c == term && (int)(quote.nest) == 0)
            {
                if ((func & STR_FUNC_QWORDS) != 0)
                {
                    quote.func = -1;
                    return ' ';
                }
                if ((func & STR_FUNC_REGEXP) == 0) return (int)Tokens.tSTRING_END;
                yylval.num = regx_options();
                return (int)Tokens.tREGEXP_END;
            }
            if (space)
            {
                pushback(c);
                return ' ';
            }
            newtok();
            if ((func & STR_FUNC_EXPAND) != 0 && c == '#')
            {
                switch (c = nextc())
                {
                    case '$':
                    case '@':
                        pushback(c);
                        return (int)Tokens.tSTRING_DVAR;
                    case '{':
                        return (int)Tokens.tSTRING_DBEG;
                }
                tokadd('#');
            }
            pushback(c);
            if (tokadd_string(func, term, paren, ref quote.nest) == -1)
            {
                sourceline = (int)quote.location.first_line;
                yyerror("unterminated string meets end of file");
                return (int)Tokens.tSTRING_END;
            }

            tokfix();
            yylval.node = new VALUE(tok(), yylloc);
            return (int)Tokens.tSTRING_CONTENT;
        }

        private int heredoc_identifier()
        {
            int c = nextc(), term, func = 0;

            if (c == '-')
            {
                c = nextc();
                func = STR_FUNC_INDENT;
            }
            switch (c)
            {
                case '\'':
                    func |= (int)string_type.str_squote; goto quoted;
                case '"':
                    func |= (int)string_type.str_dquote; goto quoted;
                case '`':
                    func |= (int)string_type.str_xquote;
                quoted:
                    newtok();
                    tokadd((char)func);
                    term = c;
                    while ((c = nextc()) != -1 && c != term)
                    {
                        tokadd((char)c);
                    }
                    if (c == -1)
                    {
                        yyerror("unterminated here document identifier");
                        return 0;
                    }
                    break;

                default:
                    if (!is_identchar((char)c))
                    {
                        pushback(c);
                        if ((func & STR_FUNC_INDENT) != 0)
                        {
                            pushback('-');
                        }
                        return 0;
                    }
                    term = '"';
                    func |= (int)string_type.str_dquote;
                    newtok();
                    tokadd((char)func);
                    do
                    {
                        tokadd((char)c);
                    } while ((c = nextc()) != -1 && is_identchar((char)c));
                    pushback(c);
                    break;
            }

            tokfix();
            int resume = lex_p;
            lex_p = lex_pend;
            lex_strterm = new HEREDOC(tok(), resume, lex_lastline, yylloc);
            yylval.term = lex_strterm;
            return term == '`' ? (int)Tokens.tXSTRING_BEG : (int)Tokens.tSTRING_BEG;
        }

        private void heredoc_restore(HEREDOC here)
        {
            String line = here.resume_line;
            lex_lastline = line;
            lex_buffer = line.value;
            lex_pend = lex_buffer.Length;
            lex_p = (int)here.resume_position;
            heredoc_end = sourceline;
            sourceline = (int)here.location.first_line;
        }

        private bool whole_match_p(string eos, int indent)
        {
            int p = 0;
            int n;

            if (indent != 0)
            {
                while (p < lex_buffer.Length && char.IsWhiteSpace(lex_buffer[p])) p++;
            }
            n = lex_pend - (p + eos.Length);
            if (n < 0 || (n > 0 && lex_buffer[p + eos.Length] != '\n' && lex_buffer[p + eos.Length] != '\r')) return false;
            if (eos == lex_buffer.Substring(p, eos.Length)) return true;
            return false;
        }

        private int here_document(HEREDOC here)
        {
            int c, func, indent = 0;
            string eos;
            int p, pend;
            VALUE str = null;

            eos = here.label;
            indent = (func = eos[0]) & STR_FUNC_INDENT;
            eos = eos.Substring(1);

            if ((c = nextc()) == -1)
            {
                yyerror("can't find string \"{0}\" anywhere before EOF", eos);
                heredoc_restore(here);
                lex_strterm = null;
                return 0;
            }

            if (was_bol() && whole_match_p(eos, indent))
            {
                heredoc_restore(here);
                return (int)Tokens.tSTRING_END;
            }

            if ((func & STR_FUNC_EXPAND) == 0)
            {
                do
                {
                    string buffer = lex_lastline.value;
                    p = 0;
                    pend = lex_pend;
                    if (pend > p)
                    {
                        switch (buffer[pend - 1])
                        {
                            case '\n':
                                if (--pend == p || buffer[pend - 1] != '\r')
                                {
                                    pend++;
                                    break;
                                }
                                goto case '\r';
                            case '\r':
                                --pend;
                                break;
                        }
                    }
                    if (str != null)
                        str = VALUE.str_cat(str, buffer.Substring(p), pend - p, null);
                    else
                        str = VALUE.str_new(buffer.Substring(p), pend - p, null);
                    if (pend < lex_pend) VALUE.str_cat(str, "\n", 1, null);
                    lex_p = lex_pend;
                    if (nextc() == -1)
                    {
                        yyerror("can't find string \"{0}\" anywhere before EOF", eos);
                        heredoc_restore(here);
                        lex_strterm = null;
                        return 0;
                    }
                } while (!whole_match_p(eos, indent));
            }
            else
            {
                newtok();
                if (c == '#')
                {
                    switch (c = nextc())
                    {
                        case '$':
                        case '@':
                            pushback(c);
                            return (int)Tokens.tSTRING_DVAR;
                        case '{':
                            return (int)Tokens.tSTRING_DBEG;
                    }
                    tokadd('#');
                }
                do
                {
                    pushback(c);
                    object tmp = null;
                    if ((c = tokadd_string(func, '\n', 0, ref tmp)) == -1)
                    {
                        yyerror("can't find string \"{0}\" anywhere before EOF", eos);
                        heredoc_restore(here);
                        lex_strterm = null;
                        return 0;
                    }
                    if (c != '\n')
                    {
                        yylval.node = new VALUE(tok(), yylloc);
                        return (int)Tokens.tSTRING_CONTENT;
                    }
                    tokadd((char)nextc());
                    if ((c = nextc()) == -1)
                    {
                        yyerror("can't find string \"{0}\" anywhere before EOF", eos);
                        heredoc_restore(here);
                        lex_strterm = null;
                        return 0;
                    }
                } while (!whole_match_p(eos, indent));
                str = new VALUE(tok(), yylloc);
            }
            heredoc_restore(here);
            lex_strterm = new StringTerminator((int)string_type.str_minusone, 0, 0, 0, yylloc);
            yylval.node = str;
            return (int)Tokens.tSTRING_CONTENT;
        }

        
        private class keyword
        {
            internal string name;
            internal int[] id;
            internal Lex_State state;

            internal keyword(string name, Tokens id, Lex_State state)
            {
                this.name = name;
                this.id = new int[] { (int)id, (int)id };
                this.state = state;
            }

            internal keyword(string name, Tokens id0, Tokens id1, Lex_State state)
            {
                this.name = name;
                this.id = new int[] { (int)id0, (int)id1 };
                this.state = state;
            }
        };

        private keyword reserved_word(string word)
        {
            switch (word)
            {
                case "end": return new keyword("end", Tokens.kEND, Lex_State.EXPR_END);
                case "else": return new keyword("else", Tokens.kELSE, Lex_State.EXPR_BEG);
                case "case": return new keyword("case", Tokens.kCASE, Lex_State.EXPR_BEG);
                case "ensure": return new keyword("ensure", Tokens.kENSURE, Lex_State.EXPR_BEG);
                case "module": return new keyword("module", Tokens.kMODULE, Lex_State.EXPR_BEG);
                case "elsif": return new keyword("elsif", Tokens.kELSIF, Lex_State.EXPR_BEG);
                case "def": return new keyword("def", Tokens.kDEF, Lex_State.EXPR_FNAME);
                case "rescue": return new keyword("rescue", Tokens.kRESCUE, Tokens.kRESCUE_MOD, Lex_State.EXPR_MID);
                case "not": return new keyword("not", Tokens.kNOT, Lex_State.EXPR_BEG);
                case "then": return new keyword("then", Tokens.kTHEN, Lex_State.EXPR_BEG);
                case "yield": return new keyword("yield", Tokens.kYIELD, Lex_State.EXPR_ARG);
                case "for": return new keyword("for", Tokens.kFOR, Lex_State.EXPR_BEG);
                case "self": return new keyword("self", Tokens.kSELF, Lex_State.EXPR_END);
                case "false": return new keyword("false", Tokens.kFALSE, Lex_State.EXPR_END);
                case "retry": return new keyword("retry", Tokens.kRETRY, Lex_State.EXPR_END);
                case "return": return new keyword("return", Tokens.kRETURN, Lex_State.EXPR_MID);
                case "true": return new keyword("true", Tokens.kTRUE, Lex_State.EXPR_END);
                case "if": return new keyword("if", Tokens.kIF, Tokens.kIF_MOD, Lex_State.EXPR_BEG);
                case "defined?": return new keyword("defined?", Tokens.kDEFINED, Lex_State.EXPR_ARG);
                case "super": return new keyword("super", Tokens.kSUPER, Lex_State.EXPR_ARG);
                case "undef": return new keyword("undef", Tokens.kUNDEF, Lex_State.EXPR_FNAME);
                case "break": return new keyword("break", Tokens.kBREAK, Lex_State.EXPR_MID);
                case "in": return new keyword("in", Tokens.kIN, Lex_State.EXPR_BEG);
                case "do": return new keyword("do", Tokens.kDO, Lex_State.EXPR_BEG);
                case "nil": return new keyword("nil", Tokens.kNIL, Lex_State.EXPR_END);
                case "until": return new keyword("until", Tokens.kUNTIL, Tokens.kUNTIL_MOD, Lex_State.EXPR_BEG);
                case "unless": return new keyword("unless", Tokens.kUNLESS, Tokens.kUNLESS_MOD, Lex_State.EXPR_BEG);
                case "or": return new keyword("or", Tokens.kOR, Lex_State.EXPR_BEG);
                case "next": return new keyword("next", Tokens.kNEXT, Lex_State.EXPR_MID);
                case "when": return new keyword("when", Tokens.kWHEN, Lex_State.EXPR_BEG);
                case "redo": return new keyword("redo", Tokens.kREDO, Lex_State.EXPR_END);
                case "and": return new keyword("and", Tokens.kAND, Lex_State.EXPR_BEG);
                case "begin": return new keyword("begin", Tokens.kBEGIN, Lex_State.EXPR_BEG);
                case "__LINE__": return new keyword("__LINE__", Tokens.k__LINE__, Lex_State.EXPR_END);
                case "class": return new keyword("class", Tokens.kCLASS, Lex_State.EXPR_CLASS);
                case "__FILE__": return new keyword("__FILE__", Tokens.k__FILE__, Lex_State.EXPR_END);
                case "END": return new keyword("END", Tokens.klEND, Lex_State.EXPR_END);
                case "BEGIN": return new keyword("BEGIN", Tokens.klBEGIN, Lex_State.EXPR_END);
                case "while": return new keyword("while", Tokens.kWHILE, Tokens.kWHILE_MOD, Lex_State.EXPR_BEG);
                case "alias": return new keyword("alias", Tokens.kALIAS, Lex_State.EXPR_FNAME);
                default: return null;
            }
        }

        private void arg_ambiguous()
        {
            yywarn("ambiguous first argument; put parentheses or even spaces");
        }

        private bool IS_ARG() { return (lex_state == Lex_State.EXPR_ARG || lex_state == Lex_State.EXPR_CMDARG); }

        private int quotation(int c)
        {
            int term;
            int paren;
            if (!char.IsLetterOrDigit((char)c))
            {
                term = c;
                c = 'Q';
            }
            else
            {
                term = nextc();
                if (char.IsLetterOrDigit((char)term) || MultiByteChar.ismbchar((char)term))
                {
                    yyerror("unknown type of %string");
                    return 0;
                }
            }
            if (c == -1 || term == -1)
            {
                yyerror("unterminated quoted string meets end of file");
                return 0;
            }
            paren = term;
            if (term == '(') term = ')';
            else if (term == '[') term = ']';
            else if (term == '{') term = '}';
            else if (term == '<') term = '>';
            else paren = 0;

            switch (c)
            {
                case 'Q':
                    lex_strterm = new StringTerminator((int)string_type.str_dquote, term, paren, c, yylloc);
                    yylval.term = lex_strterm;
                    return (int)Tokens.tSTRING_BEG;

                case 'q':
                    lex_strterm = new StringTerminator((int)string_type.str_squote, term, paren, c, yylloc);
                    yylval.term = lex_strterm;
                    return (int)Tokens.tSTRING_BEG;

                case 'W':
                    lex_strterm = new StringTerminator((int)string_type.str_dquote | STR_FUNC_QWORDS, term, paren, c, yylloc);
                    yylval.term = lex_strterm;
                    do { c = nextc(); } while (char.IsWhiteSpace((char)c));
                    pushback(c);
                    return (int)Tokens.tWORDS_BEG;

                case 'w':
                    lex_strterm = new StringTerminator((int)string_type.str_squote | STR_FUNC_QWORDS, term, paren, c, yylloc);
                    yylval.term = lex_strterm;
                    do { c = nextc(); } while (char.IsWhiteSpace((char)c));
                    pushback(c);
                    return (int)Tokens.tQWORDS_BEG;

                case 'x':
                    lex_strterm = new StringTerminator((int)string_type.str_xquote, term, paren, c, yylloc);
                    yylval.term = lex_strterm;
                    return (int)Tokens.tXSTRING_BEG;

                case 'r':
                    lex_strterm = new StringTerminator((int)string_type.str_regexp, term, paren, c, yylloc);
                    yylval.term = lex_strterm;
                    return (int)Tokens.tREGEXP_BEG;

                case 's':
                    lex_strterm = new StringTerminator((int)string_type.str_ssym, term, paren, c, yylloc);
                    yylval.term = lex_strterm;
                    lex_state = Lex_State.EXPR_FNAME;
                    return (int)Tokens.tSYMBEG;

                default:
                    yyerror("unknown type of %string");
                    return 0;
            }
        }

        internal static uint scan_oct(string num, int len, out int s)
        {
            s = 0;
            uint retval = 0;

            while (len-- > 0 && num[s] >= '0' && num[s] <= '7')
            {
                retval <<= 3;
                retval |= (uint)(num[s++] - '0');
            }
            return retval;
        }

        internal static uint scan_hex(string num, int len, out int s)
        {
            string hexdigit = "0123456789abcdef0123456789ABCDEF";
            s = 0;
            uint retval = 0;
            int tmp;

            while (len-- > 0 && s < num.Length && (tmp = hexdigit.IndexOf(num[s])) >= 0)
            {
                retval <<= 4;
                retval |= (uint)(tmp & 15);
                s++;
            }
            return retval;
        }


        private void BITSTACK_PUSH(ref int stack, int n)
        {
            stack = (stack << 1) | ((n) & 1);
        }
        private int BITSTACK_POP(ref int stack)
        {
            return (stack >>= 1);
        }
        private void BITSTACK_LEXPOP(ref int stack)
        {
            stack = (stack >> 1) | (stack & 1);
        }
        private bool BITSTACK_SET_P(int stack)
        {
            return (stack & 1) != 0;
        }

        private int cond_stack = 0;
        internal void COND_PUSH(int n) { BITSTACK_PUSH(ref cond_stack, n); }
        internal int COND_POP() { return BITSTACK_POP(ref cond_stack); }
        internal void COND_LEXPOP() { BITSTACK_LEXPOP(ref cond_stack); }
        internal bool COND_P() { return BITSTACK_SET_P(cond_stack); }

        private int cmdarg_stack = 0;
        internal void CMDARG_PUSH(int n) { BITSTACK_PUSH(ref cmdarg_stack, n); }
        internal int CMDARG_POP() { return BITSTACK_POP(ref cmdarg_stack); }
        internal void CMDARG_LEXPOP() { BITSTACK_LEXPOP(ref cmdarg_stack); }
        internal bool CMDARG_P() { return BITSTACK_SET_P(cmdarg_stack); }

        internal static bool is_identchar(char c)
        {
            return char.IsLetterOrDigit(c) || (c) == '_' || MultiByteChar.ismbchar((char)c);
        }




        internal int errors = 0;
        internal int warnings = 0;


        internal void yyerror(string msg)
        {
            errors++;
            if (Compiler.log != null)
                Compiler.LogError(msg, sourcefile, start_line, start_column, sourceline, lex_p);
            else
                throw new SyntaxError(msg + " at column " + start_column + ", line " + start_line + " " + sourcefile).raise(null);
        }

        public override void yyerror(string fmt, params object[] args)
        {
            if (args.Length > 0)
                yyerror(System.String.Format(CultureInfo.InvariantCulture, fmt, args));
            else
                yyerror(fmt);
        }


        internal void yywarn(string msg)
        {
            warnings++;
            if (Ruby.Runtime.Eval.Test(Ruby.Runtime.Options.ruby_verbose.value))
                Compiler.LogWarning(msg, sourcefile, start_line, start_column, sourceline, lex_p);
        }

        internal void yywarn(string fmt, params object[] args)
        {
            yywarn(System.String.Format(CultureInfo.InvariantCulture, fmt, args));
        }
    }
}
