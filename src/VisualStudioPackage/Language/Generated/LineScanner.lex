%using Ruby.NET.Parser;

%namespace Ruby.NET.Lexer


%x DEF
%x CLASS

%%

<*> [ \t\r\n\f\v]*					{	}

def									{ BEGIN(DEF); return (int)Tokens.Keyword; }
<DEF> [a-zA-Z_][a-zA-Z_0-9\.]*	    { BEGIN(INITIAL); return (int)Tokens.MethodName; }
<DEF> .								{ BEGIN(INITIAL); } 

class								{ BEGIN(CLASS); return (int)Tokens.Keyword; }
module								{ BEGIN(CLASS); return (int)Tokens.Keyword; }
<CLASS> [a-zA-Z_][a-zA-Z_0-9\.]*	{ BEGIN(INITIAL); return (int)Tokens.ClassName; }
<CLASS> .							{ BEGIN(INITIAL); }

\{									|
\[									|
\(									|
)									|	
|									| 				
}									|
]									return (int)Tokens.Bracket;

\"									|
\'									|
`									return (int)Tokens.Quote;

+									|
-									|
*									|
=									|
!									|
&									|
^									|
~									|
,									|
;									|
>									|
\<									|
\.									|
+@									|
-@									|
::									|
\.\.								|
\|\|								| 
\^									|
\\									|
\.									|
?									|
/									|
\%                                                                      |
:									|
@									return (int)Tokens.Operator;

#.*									return (int)Tokens.Comment;

$[&'`+]								|
$[0-9]+								|
$[*$?!@/\\;,.=:<>"]					|
$-.									|
$[&`\\+]							|
$[0-9]+								return (int)Tokens.Ident;

\"([^\"\n]|(\\\"))*\"				|
\'([^\'\n]|(\\\'))*\'				|
`([^`\n]|(\\`))*`					return (int)Tokens.String;

0[xX][0-9A-Fa-f_]*					|
0[bB][01_]*							|
0[dD][0-9_]							|
0[oO][0-7_]							|
[0-9_]*([eE][+\-][0-9_]*)?			|
[0-9_]*(\.[0-9_]*)?([eE][+\-][0-9_]*)?	return (int)Tokens.Number;

[$@]?[a-zA-Z_][a-zA-Z_0-9]*			return check_keyword(yytext);


.									System.Console.Error.WriteLine("Error unknown'{0}'", yytext); return (int)Tokens.Unknown;

%%

	public static int check_keyword(string ident)
	{
		switch (ident)
		{
			case "if":		
            case "elsif":
            case "class":
            case "def":	
            case "do":	
            case "module":	
            case "yield":
            case "else":	
            case "unless":	
            case "ensure":	
            case "super":	
            case "then":	
            case "return":
            case "next":	
            case "while":	
            case "case":
            case "when":	
            case "break":	
            case "for":	
			case "in":	
            case "begin":
            case "rescue":	
            case "until":
            case "alias":
            case "undef":
            case "retry":
            case "redo":
            case "BEGIN":	
			case "end":	
            case "END":				
				return (int)Tokens.Keyword;

            case "and":	
            case "or":
            case "not":				
				return (int)Tokens.Operator;
		
			case "__FILE__":
			case "__LINE__":		
            case "self":					
			case "false":	
			case "nil":	
			case "true":
				return (int)Tokens.Literal;				
				
			default:		
				return (int)Tokens.Ident;
	    }
	}
