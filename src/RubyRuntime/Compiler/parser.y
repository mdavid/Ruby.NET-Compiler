%namespace Ruby.Compiler

%using Ruby.Compiler.AST

%union
{
    internal Node node;
    internal LVALUE lval;
    internal string id;
    internal int num;
    internal Terminator term;
}

%YYLTYPE YYLTYPE

%visibility public
%partial

%token kCLASS kMODULE kDEF kUNDEF kBEGIN kRESCUE kENSURE kEND kIF kUNLESS kTHEN kELSIF kELSE
%token kCASE kWHEN kWHILE kUNTIL kFOR kBREAK kNEXT kREDO kRETRY kIN kDO kDO_COND kDO_BLOCK
%token kRETURN kYIELD kSUPER kSELF kNIL kTRUE kFALSE kAND kOR kNOT kIF_MOD kUNLESS_MOD
%token kWHILE_MOD kUNTIL_MOD kRESCUE_MOD kALIAS kDEFINED klBEGIN klEND k__LINE__ k__FILE__
%token tUPLUS tUMINUS tPOW tCMP tEQ tEQQ tNEQ tGEQ tLEQ tANDOP tOROP tMATCH tNMATCH
%token tDOT2 tDOT3 tAREF tASET tLSHFT tRSHFT tCOLON2 tCOLON3 tASSOC tLPAREN tSTRING_END
%token tLPAREN_ARG tLBRACK tLBRACE tLBRACE_ARG tSTAR tAMPER tSTRING_DVAR

%token <id>   tIDENTIFIER tFID tGVAR tIVAR tCONSTANT tCVAR tOP_ASGN 
%token <node> tINTEGER tFLOAT tSTRING_CONTENT tNTH_REF tBACK_REF
%token <num>  tREGEXP_END 
%token <term> tSTRING_BEG tREGEXP_BEG tXSTRING_BEG tWORDS_BEG tQWORDS_BEG tSTRING_DBEG tSYMBEG

%type <node> strings string1 xstring regexp string_contents opt_ensure command_args mrhs
%type <node> word literal numeric singleton string_content var_ref primary PROGRAM
%type <node> dsym cpath words qwords word_list qword_list aref_args primary_value 
%type <node> bodystmt compstmt stmts stmt expr command_call args arg block_arg arg_value
%type <node> if_tail opt_else case_body cases opt_rescue exc_list expr_value do_block
%type <node> when_args call_args call_args2 open_args paren_args opt_paren_args method_call
%type <node> superclass block_call block_command assoc_list assocs assoc cmd_brace_block
%type <node> f_arglist f_args f_optarg f_opt f_block_arg opt_f_block_arg  brace_block
%type <node> undef_list backref string_dvar none string
%type <node> f_norm_arg f_rest_arg f_arg opt_block_arg command   
%type <node> xstring_contents
%type <id>   variable sym operation operation2 operation3 cname op fname symbol fitem
%type <lval> var_lhs mlhs_head mlhs_basic mlhs mlhs_node lhs mlhs_item mlhs_entry opt_block_var block_var exc_var

%nonassoc tLOWEST
%nonassoc tLBRACE_ARG
%nonassoc  kIF_MOD kUNLESS_MOD kWHILE_MOD kUNTIL_MOD
%left  kOR kAND
%right kNOT
%nonassoc kDEFINED
%right '=' tOP_ASGN
%left kRESCUE_MOD
%right '?' ':'
%nonassoc tDOT2 tDOT3
%left  tOROP
%left  tANDOP
%nonassoc  tCMP tEQ tEQQ tNEQ tMATCH tNMATCH
%left  '>' tGEQ '<' tLEQ
%left  '|' '^'
%left  '&'
%left  tLSHFT tRSHFT
%left  '+' '-'
%left  '*' '/' '%'
%right tUMINUS_NUM tUMINUS
%right tPOW
%right '!' '~' tUPLUS

%token tLAST_TOKEN


%%


PROGRAM    :    {
                scanner.lex_state = Lex_State.EXPR_BEG;
                if (CurrentScope == null)
                    eval_tree = enter_scope(new SOURCEFILE(null));    
                else
                    eval_tree = CurrentScope;                
            }
        compstmt
            {
                leave_scope(@2, $2);
            }
        ;

bodystmt: compstmt
          opt_rescue
          opt_else
          opt_ensure
            {
                if ($2 != null || $3 != null || $4 != null)
                    $$ = new TRY_BLOCK(CurrentScope, $1, $2, $3, $4, @$);
                else
                    $$ = $1;
            }
        ;

compstmt: stmts    opt_terms
            {
                $$ = $1;
            }
        ;

stmts    : none
        | stmt
        | stmts terms stmt
            {
                $$ = append($1, $3);
            }
        | error stmt
            {
                $$ = $2;
            }
        ;

stmt    : kALIAS fitem 
            {
                scanner.lex_state =    Lex_State.EXPR_FNAME;
            } 
          fitem
            {
                $$ = new ALIAS(CurrentScope, $2, $4, @1);
            }
        | kALIAS tGVAR tGVAR
            {
                $$ = new VALIAS($2, $3, @1);
            }
        | kALIAS tGVAR tBACK_REF
            {
                $$ = new VALIAS($2, ID.intern("$" + ((BACK_REF)$3).ch), @1);
            }
        | kALIAS tGVAR tNTH_REF
            {
                scanner.yyerror("cant make alias for the number variable");
                $$ = null;
            }
        | kUNDEF undef_list
            {
                $$ = $2;
            }
        | stmt kIF_MOD expr_value
            {
                $$ = new IF($3, $1, null, @$);
            }
        | stmt kUNLESS_MOD expr_value
            {
                $$ = new IF($3, null, $1, @$);
            }
        | stmt kWHILE_MOD expr_value
            {
                if ($1 is begin)
                    $$ = new PostTestLoop(CurrentScope, $3, true, $1, @$);
                else 
                    $$ = new PreTestLoop(CurrentScope, $3, true, $1, @$);
            }
        | stmt kUNTIL_MOD expr_value
            {
                if ($1 is begin)
                    $$ = new PostTestLoop(CurrentScope, $3, false, $1, @$);
                else
                    $$ = new PreTestLoop(CurrentScope, $3, false, $1, @$);
            }
        | stmt kRESCUE_MOD stmt
            {
                $$ = new RESCUE_EXPR($1, $3, @$);
            }
        | klBEGIN
            {
                if (in_def != 0 || in_single != 0)
                    scanner.yyerror("BEGIN in method");
                            
                eval_tree_begin = (Scope)append(eval_tree_begin, enter_scope(new BEGIN(CurrentScope, @1)));            
            }
          '{' compstmt '}'
            {
                leave_scope(@1, $4);
                $$ = null;
            }
        | klEND
            {
                enter_scope(new END(CurrentScope, @1));
            } 
          '{' compstmt '}'
            {                    
                $$ = leave_scope(@1, $4);
            }
        | lhs '=' command_call
            {
                $$ =  new ASSIGNMENT($1, $3, @$);
            }
        | mlhs '=' command_call
            {
                $$ = new ASSIGNMENT($1, $3, @$);
            }
        | var_lhs tOP_ASGN command_call
            {
                $$ = new OP_ASGN($1, $2, $3, @$);
            }
        | primary_value '[' aref_args ']' tOP_ASGN command_call
            {                
                $$ = new OP_ASGN(new ARRAY_ACCESS($1, $3, @2), $5, $6, @$);
            }
        | primary_value '.' tIDENTIFIER tOP_ASGN command_call
            {
                $$ = new OP_ASGN2($1, $3, $4, $5, @$);
            }
        | primary_value '.' tCONSTANT tOP_ASGN command_call
            {
                $$ = new OP_ASGN2($1, $3, $4, $5, @$);
            }
        | primary_value tCOLON2 tIDENTIFIER tOP_ASGN command_call
            {
                $$ = new OP_ASGN2($1, $3, $4, $5, @$);
            }
        | backref tOP_ASGN command_call
            {
                backref_error($1);
                $$ = null;
            } 
        | lhs '=' mrhs
            {
                $$ = new ASSIGNMENT($1, $3, @$);
            }
        | mlhs '=' arg_value
            {
                $$ = new ASSIGNMENT($1, $3, @$);
            }
        | mlhs '=' mrhs
            {
                $$ = new ASSIGNMENT($1, $3, @$);
            }
        | expr
        ;

expr    : command_call
        | expr kAND expr
            {
                $$ = new AND($1, $3, @2);
            }
        | expr kOR expr
            {
                $$ = new OR($1, $3, @2);
            }
        | kNOT expr
            {
                $$ = new NOT($2, @1);
            }
        | '!' command_call
            {
                $$ = new NOT($2, @1);
            }
        | arg
        ;

expr_value    : expr
            ;

command_call: command
                {
                    $$ = $1;
                }
            | block_command
                {
                    $$ = $1;
                }
            | kRETURN call_args
                {
                    $$ = new RETURN(CurrentScope, $2, @1);
                }
            | kBREAK call_args
                {
                    $$ = new BREAK(CurrentScope, $2, @1);
                }
            | kNEXT call_args
                {
                    $$ = new NEXT(CurrentScope, $2, @1);
                }
            ;

block_command    : block_call
                    {
                        $$ = $1;
                    }
                | block_call '.' operation2 command_args
                    {
                        $$ = new METHOD_CALL($1, $3, $4, @3);
                    }
                | block_call tCOLON2 operation2 command_args
                    {
                        $$ = new METHOD_CALL($1, $3, $4, @3);
                    }
                ;

cmd_brace_block    : tLBRACE_ARG
                    {
                        enter_scope(new BLOCK(CurrentScope, @1));                    
                    }
                  opt_block_var 
                    {
                    }
                  compstmt
                  '}'
                    {
                        $$ = leave_scope(@$, $3, $5);
                    }
        ;

command    : operation    command_args                            %prec tLOWEST
            {
                $$ = new METHOD_CALL($1, $2, @1);
            }
        | operation command_args cmd_brace_block
            {
                $$ = new METHOD_CALL($1, $2, $3, @1);
            }
        | primary_value '.' operation2 command_args                %prec tLOWEST
            {
                $$ = new METHOD_CALL($1, $3, $4, @3);
            }
        | primary_value '.' operation2 command_args cmd_brace_block
            {
                $$ = new METHOD_CALL($1, $3, $4, $5, @3);
            }
        | primary_value tCOLON2 operation2 command_args            %prec tLOWEST
            {
                $$ = new METHOD_CALL($1, $3, $4, @3);
            }
        | primary_value tCOLON2 operation2 command_args cmd_brace_block
            {
                $$ = new METHOD_CALL($1, $3, $4, $5, @3);
            }
        | kSUPER command_args
            {
                $$ = new SUPER(CurrentScope, $2, @1);
            }
        | kYIELD command_args
            {
                $$ = new YIELD($2, @1);
            }
        ;


mlhs    : mlhs_basic
            {
                $$ = $1;
            }
        | tLPAREN mlhs_entry ')'
            {
                $$ = $2;
            }
        ;
        
mlhs_entry    : mlhs_basic
            | tLPAREN mlhs_entry ')'
                {
                    $$ = $2;
                }    
            ;
                    
    
mlhs_basic    : mlhs_head
                {
                    $$ = $1;
                }
            | mlhs_head mlhs_item
                {
                    $$ = ((MLHS)$1).append($2);
                }
            | mlhs_head tSTAR mlhs_node
                {
                    $$ = ((MLHS)$1).append(new LHS_STAR($3, @3));
                }
            | mlhs_head tSTAR
                {
                    $$ = ((MLHS)$1).append(new LHS_STAR(null, @2));
                }
            | tSTAR mlhs_node
                {
                    $$ = new LHS_STAR($2, @1);
                }
            | tSTAR
                {
                    $$ = new LHS_STAR(null, @1);
                }
            ;

mlhs_item    : mlhs_node
                {
                    $$ = $1;
                }
            | tLPAREN mlhs_entry ')'
                {
                    $$ = $2;
                }
            ;

mlhs_head    : mlhs_item ','
                {
                    $$ = new MLHS($1, @1);
                }
            | mlhs_head mlhs_item ','
                {
                    $$ = ((MLHS)$1).append($2);
                }
            ;
            
mlhs_node    : variable
                {
                    $$ = assignable($1, @1);
                }
            | primary_value '[' aref_args ']'
                {
                    $$ = new ARRAY_ACCESS($1, $3, @2);
                }
            | primary_value '.' tIDENTIFIER
                {
                    $$ = new ATTRIBUTE($1, $3, @3);
                }
            | primary_value tCOLON2 tIDENTIFIER
                {
                    $$ = new ATTRIBUTE($1, $3, @3);
                }
            | primary_value '.' tCONSTANT
                {
                    $$ = new ATTRIBUTE($1, $3, @3);
                }
            | primary_value tCOLON2 tCONSTANT
                {
                    $$ = new CONST(CurrentScope, $1, $3, @3);
                }
            | tCOLON3 tCONSTANT
                {
                    $$ = new CONST(CurrentScope, null, $2, @2);
                }
            | backref
                {
                    backref_error($1);
                    $$ = null;
                }
            ;

lhs    : variable
        {
            $$ = assignable($1, @1);
        }
    | primary_value '[' aref_args ']'
        {
            $$ = new ARRAY_ACCESS($1, $3, @2);
        }
    | primary_value '.' tIDENTIFIER
        {
            $$ = new ATTRIBUTE($1, $3, @3);
        }
    | primary_value tCOLON2 tIDENTIFIER
        {
            $$ = new ATTRIBUTE($1, $3, @3);
        }
    | primary_value '.' tCONSTANT
        {
            $$ = new ATTRIBUTE($1, $3, @3);
        }
    | primary_value tCOLON2 tCONSTANT
        {
            $$ = new CONST(CurrentScope, $1, $3, @3);
        }
    | tCOLON3 tCONSTANT
        {
            $$ = new CONST(CurrentScope, null, $2, @2);
        }
    | backref
        {
            backref_error($1);
            $$ = null;
        }
    ;

cname    : tIDENTIFIER
            {
                scanner.yyerror("class/module name must be CONSTANT");
            }
        | tCONSTANT
        ;

cpath    : tCOLON3 cname
            {
                $$ = new CONST(CurrentScope, null, $2, @2);
            }
        | cname
            {
                $$ = new CONST(CurrentScope, $1, @1);
            }
        | primary_value tCOLON2 cname
            {
                $$ = new CONST(CurrentScope, $1, $3, @3);
            }
        ;

fname    : tIDENTIFIER
            {
                $$ = $1;    
            }
        | tCONSTANT
            {
                $$ = $1;    
            }        
        | tFID
            {
                $$ = $1;    
            }        
        | op
            {
                scanner.lex_state = Lex_State.EXPR_END;
                $$ = $1;
            }
        | reswords
            {
                scanner.lex_state = Lex_State.EXPR_END;
                $$ = $<id>1;
            }
        ;

fitem    : fname
            {
                $$ = $1;
            }
        | symbol
            {
                $$ = $1;
            }
        ;

undef_list    : fitem
                {
                    $$ = new UNDEF(CurrentScope, $1, @1);
                }
            | undef_list ',' 
                {
                    scanner.lex_state = Lex_State.EXPR_FNAME;
                } 
              fitem
                {
                    $$ = append($1, new UNDEF(CurrentScope, $4, @4));
                }
            ;

op        : '|'        { $$ = ID.intern('|');    }
        | '^'        { $$ = ID.intern('^'); }
        | '&'        { $$ = ID.intern('&'); }
        | tCMP        { $$ = ID.intern(Tokens.tCMP); }
        | tEQ        { $$ = ID.intern(Tokens.tEQ); }
        | tEQQ        { $$ = ID.intern(Tokens.tEQQ); }
        | tMATCH    { $$ = ID.intern(Tokens.tMATCH); }
        | '>'        { $$ = ID.intern('>'); }
        | tGEQ        { $$ = ID.intern(Tokens.tGEQ); }
        | '<'        { $$ = ID.intern('<'); }
        | tLEQ        { $$ = ID.intern(Tokens.tLEQ); }
        | tLSHFT    { $$ = ID.intern(Tokens.tLSHFT); }
        | tRSHFT    { $$ = ID.intern(Tokens.tRSHFT); }
        | '+'        { $$ = ID.intern('+'); }
        | '-'        { $$ = ID.intern('-'); }
        | '*'        { $$ = ID.intern('*'); }
        | tSTAR        { $$ = ID.intern('*'); }
        | '/'        { $$ = ID.intern('/'); }
        | '%'        { $$ = ID.intern('%'); }
        | tPOW        { $$ = ID.intern(Tokens.tPOW); }
        | '~'        { $$ = ID.intern('~'); }
        | tUPLUS    { $$ = ID.intern(Tokens.tUPLUS); }
        | tUMINUS    { $$ = ID.intern(Tokens.tUMINUS); }
        | tAREF        { $$ = ID.intern(Tokens.tAREF); }
        | tASET        { $$ = ID.intern(Tokens.tASET); }
        | '`'        { $$ = ID.intern('`'); }
        ;

reswords: k__LINE__    | k__FILE__     | klBEGIN | klEND
        | kALIAS | kAND | kBEGIN | kBREAK | kCASE | kCLASS | kDEF
        | kDEFINED | kDO | kELSE | kELSIF | kEND | kENSURE | kFALSE
        | kFOR | kIN | kMODULE | kNEXT | kNIL | kNOT
        | kOR | kREDO | kRESCUE | kRETRY | kRETURN | kSELF | kSUPER
        | kTHEN | kTRUE | kUNDEF | kWHEN | kYIELD
        | kIF_MOD | kUNLESS_MOD | kWHILE_MOD | kUNTIL_MOD | kRESCUE_MOD
        ;

arg        : lhs '=' arg
            {
                $$ = new ASSIGNMENT($1, $3, @$);
            }
        | lhs '=' arg kRESCUE_MOD arg
            {
                $$ = new ASSIGNMENT($1, new RESCUE_EXPR($3, $5, @4), @$);
            }
        | var_lhs tOP_ASGN arg
            {
                $$ = new OP_ASGN($1, $2, $3, @$);
            }
        | primary_value '[' aref_args ']' tOP_ASGN arg
            {
                $$ = new OP_ASGN(new ARRAY_ACCESS($1, $3, @2), $5, $6, @$);
            }
        | primary_value '.' tIDENTIFIER tOP_ASGN arg
            {
                $$ = new OP_ASGN2($1, $3, $4, $5, @$);
            }
        | primary_value '.' tCONSTANT tOP_ASGN arg
            {
                $$ = new OP_ASGN2($1, $3, $4, $5, @$);
            }
        | primary_value tCOLON2 tIDENTIFIER tOP_ASGN arg
            {
                $$ = new OP_ASGN2($1, $3, $4, $5, @$);
            }
        | primary_value tCOLON2 tCONSTANT tOP_ASGN arg
            {
                scanner.yyerror("constant re-assignment");
                $$ = null;
            }
        | tCOLON3 tCONSTANT tOP_ASGN arg
            {
                scanner.yyerror("constant re-assignment");
                $$ = null;
            }
        | backref tOP_ASGN arg
            {
                backref_error($1);
                $$ = null;
            }
        | arg tDOT2 arg
            {
                $$ = new DOT2($1, $3, @2);
            }
        | arg tDOT3 arg
            {
                $$ = new DOT3($1, $3, @2);
            }
        | arg '+' arg
            {
                $$ = new METHOD_CALL($1, ID.intern('+'), $3, @2);
            }
        | arg '-' arg
            {
                $$ = new METHOD_CALL($1, ID.intern('-'), $3, @2);
            }
        | arg '*' arg
            {
                $$ = new METHOD_CALL($1, ID.intern('*'), $3, @2);
            }
        | arg '/' arg
            {
                $$ = new METHOD_CALL($1, ID.intern('/'), $3, @2);
            }
        | arg '%' arg
            {
                $$ = new METHOD_CALL($1, ID.intern('%'), $3, @2);
            }
        | arg tPOW arg
            {
                $$ = new METHOD_CALL($1, ID.intern(Tokens.tPOW), $3, @2);
            }
        | tUMINUS_NUM tINTEGER tPOW arg
            {
                $$ = new METHOD_CALL(new METHOD_CALL($2, ID.intern(Tokens.tPOW), $4, @3), ID.intern(Tokens.tUMINUS), new ARGS(@1), @1);
            }
        | tUMINUS_NUM tFLOAT tPOW arg
            {
                $$ = new METHOD_CALL(new METHOD_CALL($2, ID.intern(Tokens.tPOW), $4, @3), ID.intern(Tokens.tUMINUS), new ARGS(@1), @1);
            }
        | tUPLUS arg
            {
                $$ = new METHOD_CALL($2, ID.intern(Tokens.tUPLUS), new ARGS(@2), @1);
            }
        | tUMINUS arg
            {
                $$ = new METHOD_CALL($2, ID.intern(Tokens.tUMINUS), new ARGS(@2), @1);
            }
        | arg '|' arg
            {
                $$ = new METHOD_CALL($1, ID.intern('|'), $3, @2);
            }
        | arg '^' arg
            {
                $$ = new METHOD_CALL($1, ID.intern('^'), $3, @2);
            }
        | arg '&' arg
            {
                $$ = new METHOD_CALL($1, ID.intern('&'), $3, @2);
            }
        | arg tCMP arg
            {
                $$ = new METHOD_CALL($1, ID.intern(Tokens.tCMP), $3, @2);
            }
        | arg '>' arg
            {
                $$ = new METHOD_CALL($1, ID.intern('>'), $3, @2);
            }
        | arg tGEQ arg
            {
                $$ = new METHOD_CALL($1, ID.intern(Tokens.tGEQ), $3, @2);
            }
        | arg '<' arg
            {
                $$ = new METHOD_CALL($1, ID.intern('<'), $3, @2);
            }
        | arg tLEQ arg
            {
                $$ = new METHOD_CALL($1, ID.intern(Tokens.tLEQ), $3, @2);
            }
        | arg tEQ arg
            {
                $$ = new METHOD_CALL($1, ID.intern(Tokens.tEQ), $3, @2);
            }
        | arg tEQQ arg
            {
                $$ = new METHOD_CALL($1, ID.intern(Tokens.tEQQ), $3, @2);
            }
        | arg tNEQ arg
            {
                $$ = new NOT(new METHOD_CALL($1, ID.intern(Tokens.tEQ), $3, @2), @2);
            }
        | arg tMATCH arg
            {
                $$ = new MATCH($1, $3, @2);
            }
        | arg tNMATCH arg
            {
                $$ = new NOT(new MATCH($1, $3, @2), @2);
            }
        | '!' arg
            {
                $$ = new NOT($2, @1);
            }
        | '~' arg
            {
                $$ = new METHOD_CALL($2, ID.intern('~'), new ARGS(@2), @1);
            }
        | arg tLSHFT arg
            {
                $$ = new METHOD_CALL($1, ID.intern(Tokens.tLSHFT), $3, @2);
            }
        | arg tRSHFT arg
            {
                $$ = new METHOD_CALL($1, ID.intern(Tokens.tRSHFT), $3, @2);
            }
        | arg tANDOP arg
            {
                $$ = new AND($1, $3, @2);
            }
        | arg tOROP arg
            {
                $$ = new OR($1, $3, @2);
            }
        | kDEFINED opt_nl 
            {
                // Empty placeholder
            }
          arg
            {
                $$ = new DEFINED($4, @1);
            }
        | arg '?' arg ':' arg
            {
                $$ = new IF($1, $3, $5, @2);
            }
        | primary
            {
                $$ = $1;
            }
        ;
        
arg_value    : arg
                {
                    $$ = $1;
                }    
            ;

aref_args    : none
                {
                    $$ = new ARGS(null, null, null, null, @$);
                }
            | command opt_nl
                {
                    scanner.yywarn("parenthesize argument(s) for future version");
                    $$ = new ARGS($1, null, null, null, @$);
                }
            | args trailer
                {
                    $$ = new ARGS($1, null, null, null, @$);
                }
            | args ',' tSTAR arg opt_nl
                {
                    $$ = new ARGS($1, null, $4, null, @$);
                }
            | assocs trailer
                {
                    $$ = new ARGS(null, $1, null, null, @$);
                }
            | tSTAR arg opt_nl
                {
                    $$ = new ARGS(null, null, $2, null, @$);
                }
            ;

paren_args    : '(' none ')'
                {
                    $$ = new ARGS(null, null, null, null, @$);
                }
            | '(' call_args opt_nl ')'
                {
                    $$ = $2;
                }
            | '(' block_call opt_nl ')'
                {
                    scanner.yywarn("parenthesize argument for future version");
                    $$ = $2;
                }
            | '(' args ',' block_call opt_nl ')'
                {
                    scanner.yywarn("parenthesize argument for future version");    
                    $$ = new ARGS($2, null, null, $4, @$);
                }
            ;

opt_paren_args    : none
                {
                    $$ = new ARGS(null, null, null, null, @$);
                }

                | paren_args
                ;

call_args    : command
                {
                    scanner.yywarn("parenthesize argument(s) for future version");                
                    $$ = new ARGS($1, null, null, null, @$);
                }
            | args opt_block_arg
                {
                    $$ = new ARGS($1, null, null, $2, @$);
                }
            | args ',' tSTAR arg_value opt_block_arg
                {
                    $$ = new ARGS($1, null, $4, $5, @$);
                }
            | assocs opt_block_arg
                {
                    $$ = new ARGS(null, $1, null, $2, @$);
                }
            | assocs ',' tSTAR arg_value opt_block_arg
                {
                    $$ = new ARGS(null, $1, $4, $5, @$);
                }
            | args ',' assocs opt_block_arg
                {
                    $$ = new ARGS($1, $3, null, $4, @$);
                }
            | args ',' assocs ',' tSTAR arg opt_block_arg
                {
                    $$ = new ARGS($1, $3, $6, $7, @$);
                }
            | tSTAR arg_value opt_block_arg
                {
                    $$ = new ARGS(null, null, $2, $3, @$);
                }
            | block_arg
                {
                    $$ = new ARGS(null, null, null, $1, @$);
                }
            ;

call_args2    : arg_value ',' args opt_block_arg
                {
                    $$ = new ARGS(append($1, $3), null, null, $4, @$);
                }
            | arg_value ',' block_arg
                {
                    $$ = new ARGS($1, null, null, $3, @$);
                }
            | arg_value ',' tSTAR arg_value opt_block_arg
                {
                    $$ = new ARGS($1, null, $4, $5, @$);
                }
            | arg_value ',' args ',' tSTAR arg_value opt_block_arg
                {
                    $$ = new ARGS(append($1, $3), null, $6, $7, @$);
                }
            | assocs opt_block_arg
                {
                    $$ = new ARGS(null, $1, null, $2, @$);
                }
            | assocs ',' tSTAR arg_value opt_block_arg
                {
                    $$ = new ARGS(null, $1, $4, $5, @$);
                }
            | arg_value ',' assocs opt_block_arg
                {
                    $$ = new ARGS($1, $3, null, $4, @$);
                }
            | arg_value ',' args ',' assocs opt_block_arg
                {
                    $$ = new ARGS(append($1, $3), $5, null, $6, @$);
                }
            | arg_value ',' assocs ',' tSTAR arg_value opt_block_arg
                {
                    $$ = new ARGS($1, $3, $6, $7, @$);
                }
            | arg_value ',' args ',' assocs ',' tSTAR arg_value opt_block_arg
                {
                    $$ = new ARGS(append($1, $3), $5, $8, $9, @$);
                }
            | tSTAR arg_value opt_block_arg
                {
                    $$ = new ARGS(null, null, $2, $3, @$);
                }
            | block_arg
                {
                    $$ = new ARGS(null, null, null, $1, @$);
                }
            ;

command_args:
                {
                    scanner.CMDARG_PUSH(1);
                }
              open_args
                {
                    scanner.CMDARG_POP();
                    $$ = $2;
                }
            ;

open_args    : call_args
            | tLPAREN_ARG
                {
                    scanner.lex_state = Lex_State.EXPR_ENDARG;
                }
              ')'
                {
                    scanner.yywarn("dont put space before argument parentheses");    
                    $$ = null;
                }
            | tLPAREN_ARG call_args2
                {
                    scanner.lex_state = Lex_State.EXPR_ENDARG;
                }
              ')'
                {
                    scanner.yywarn("dont put space before argument parentheses");    
                    $$ = $2;
                }
            ;

block_arg    : tAMPER arg_value
                {
                    $$ = new AMPER($2, @1);
                }
            ;

opt_block_arg    : ',' block_arg
                    {
                        $$ = $2;
                    }
                | none
                    {
                        $$ = null;
                    }
                ;

args: arg_value
        {
            $$ = $1;
        }
    | args ',' arg_value
        {
            $$ = append($1, $3);
        }
    ;

mrhs: args ',' arg_value
        {
            $$ = new ARGS(append($1, $3), null, null, null, @$);
        }
    | args ',' tSTAR arg_value
        {
            $$ = new ARGS($1, null, $4, null, @$);
        }
    | tSTAR arg_value
        {
            $$ = new ARGS(null, null, $2, null, @$);
        }
    ;

primary    : literal
        | strings
        | xstring
        | regexp
        | words
        | qwords
        | var_ref
        | backref
        | tFID
            {
                $$ = new METHOD_CALL($1, new ARGS(@1), @1);
            }
        | kBEGIN 
            {
                // Empty placeholder
            }
          bodystmt kEND
            {
                $$ = new begin($3, @$);
            }
        | tLPAREN_ARG expr 
            {
                scanner.lex_state = Lex_State.EXPR_ENDARG;
            } 
          opt_nl ')'
            {
                scanner.yywarn("(...) interpreted as grouped expression");            
                $$ = $2;
            }
        | tLPAREN compstmt ')'
            {
                $$ = $2;
            }
        | primary_value tCOLON2 tCONSTANT
            {
                $$ = new CONST(CurrentScope, $1, $3, @3);
            }
        | tCOLON3 tCONSTANT
            {
                $$ = new CONST(CurrentScope, null, $2, @2);
            }
        | primary_value '[' aref_args ']'
            {
                $$ = new ARRAY_ACCESS($1, $3, @2);
            }
        | tLBRACK aref_args ']'
            {
                $$ = new ARRAY($2, @1);
            }
        | tLBRACE assoc_list '}'
            {
                $$ = new HASH($2, @1);
            }
        | kRETURN
            {
                $$ = new RETURN(CurrentScope, null, @1);
            }
        | kYIELD '(' call_args ')'
            {
                $$ = new YIELD($3, @1);
            }
        | kYIELD '(' ')'
            {
                $$ = new YIELD(new ARGS(@1), @1);
            }
        | kYIELD
            {
                $$ = new YIELD(new ARGS(@1), @1);
            }
        | kDEFINED opt_nl '(' 
            {
                // Empty placeholder
            }
          expr ')'
            {
                $$ = new DEFINED($5, @1);
            }
        | operation brace_block
            {
                $$ = new METHOD_CALL($1, new ARGS(@1), $2, @1);
            }
        | method_call
        | method_call brace_block
            {    
                $$ = $1;    
                ((CALL)$1).block = $2;
            }
        | kIF expr_value then compstmt if_tail kEND
            {
                $$ = new IF($2, $4, $5, @$);
            }
        | kUNLESS expr_value then compstmt opt_else kEND
            {
                $$ = new IF($2, $5, $4, @$);
            }
        | kWHILE
            {
                scanner.COND_PUSH(1);
            }
          expr_value do
            {
                scanner.COND_POP();
            }
          compstmt kEND
            {
                $$ = new PreTestLoop(CurrentScope, $3, true, $6, @$);
            }
        | kUNTIL
            {
                scanner.COND_PUSH(1);
            }
          expr_value do
            {
                scanner.COND_POP();
            }
          compstmt kEND
            {
                $$ = new PreTestLoop(CurrentScope, $3, false, $6, @$);
            }
        | kCASE expr_value opt_terms case_body kEND
            {
                $$ = new CASE($2, $4, @$);
            }
        | kCASE opt_terms case_body kEND
            {
                $$ = new CASE(null, $3, @$);
            }
        | kCASE opt_terms kELSE compstmt kEND
            {
                $$ = $4; // fixme???
            }
        | kFOR 
            {
                $$ = new FORBODY(CurrentScope, @1);
                enter_scope((Scope)($$));
            }
          block_var kIN
            {
                leave_scope(@1);            
                scanner.COND_PUSH(1);
            }
          expr_value do
            {
                scanner.COND_POP();
                enter_scope((Scope)($2.node));
            }
          compstmt kEND
            {
                $$ = new FOR($6, leave_scope(@7, $3, $9), @$);
            }
        | kCLASS cpath superclass
            {                
                enter_scope(new CLASS(CurrentScope, @1));
            }
          bodystmt kEND
            {
                $$ = leave_scope(@$, $2, $3, $5);
            }
        | kCLASS tLSHFT expr
            {
                $<num>$ = in_def;
                in_def = 0;
            }
          term
            {
                $<num>$ = in_single;
                in_single = 0;
                enter_scope(new SCLASS(CurrentScope, @1));
            }
          bodystmt kEND
            {
                in_def = $<num>4;
                in_single = $<num>6;
                $$ = leave_scope(@$, $3, $7);
            }
        | kMODULE cpath
            {
                enter_scope(new MODULE(CurrentScope, @1));
            }
          bodystmt kEND
            {
                $$ = leave_scope(@$, $2, $4);
            }
        | kDEF fname
            {
                in_def++;
                enter_scope(new DEFN(CurrentScope, $2, @1, @2));
            }
          f_arglist bodystmt kEND
            {

                in_def--;
                $$ = leave_scope(@$, $4, $5);
            }
        | kDEF singleton dot_or_colon
            {
                scanner.lex_state = Lex_State.EXPR_FNAME;
            }
          fname
            {
                in_single++;
                scanner.lex_state = Lex_State.EXPR_END;
                enter_scope(new DEFS(CurrentScope, $5, @1, @5));                
            }
          f_arglist bodystmt kEND
            {

                in_single--;
                $$ = leave_scope(@$, $2, $7, $8);
            }
        | kBREAK
            {
                $$ = new BREAK(CurrentScope, null, @1);
            }
        | kNEXT
            {
                $$ = new NEXT(CurrentScope, null, @1);
            }
        | kREDO
            {
                $$ = new REDO(CurrentScope, @1);
            }
        | kRETRY
            {
                $$ = new RETRY(CurrentScope, @1);
            }
        ;

primary_value    : primary
                    {
                        $$ = $1;
                    }
                ;


then    : term
        | ':'
        | kTHEN
        | term kTHEN
        ;

do        : term
        | ':'
        | kDO_COND
        ;

if_tail    : opt_else
        | kELSIF expr_value then compstmt if_tail
            {
                $$ = new IF($2, $4, $5, @$);
            }
        ;

opt_else: none
        | kELSE compstmt
            {
                $$ = $2;
            }
        ;

block_var    : lhs
            | mlhs
            ;

opt_block_var    : none
                | '|' /* none */ '|'
                    {
                        $$ = new MLHS(null, @1);
                    }
                | tOROP
                    {
                        $$ = new MLHS(null, @1);
                    }
                | '|' block_var '|'
                    {
                        $$ = $2;
                    }
                ;

do_block: kDO_BLOCK
            {
                enter_scope(new BLOCK(CurrentScope, @1));
            }
          opt_block_var 
            {
                // Empty placeholder
            }
          compstmt kEND
            {

                $$ = leave_scope(@$, $3, $5);
            }
        ;

block_call    : command do_block
                {                            
                    ((CALL)$1).block = $2;
                    $$ = $1;
                }
            | block_call '.' operation2 opt_paren_args
                {
                    $$ = new METHOD_CALL($1, $3, $4, @3);
                }
            | block_call tCOLON2 operation2 opt_paren_args
                {
                    $$ = new METHOD_CALL($1, $3, $4, @3);
                }
            ;

method_call    : operation    paren_args
                {
                    $$ = new METHOD_CALL($1, $2, @1);
                }
            | primary_value '.' operation2 opt_paren_args
                {
                    $$ = new METHOD_CALL($1, $3, $4, @3);
                }
            | primary_value tCOLON2 operation2 paren_args
                {
                    $$ = new METHOD_CALL($1, $3, $4, @3);
                }
            | primary_value tCOLON2 operation3
                {
                    $$ = new METHOD_CALL($1, $3, new ARGS(@3), @3);
                }
            | kSUPER paren_args
                {
                    $$ = new SUPER(CurrentScope, $2, @1);
                }
            | kSUPER
                {
                    $$ = new SUPER(CurrentScope, null, @1);
                }
            ;

brace_block    : '{'
                {
                    enter_scope(new BLOCK(CurrentScope, @1));
                }
              opt_block_var 
                {
                    // Empty placeholder                
                }
              compstmt '}'
                {

                    $$ = leave_scope(@$, $3, $5);
                }
            | kDO
                {
                    enter_scope(new BLOCK(CurrentScope, @1));        
                }
              opt_block_var 
                {
                    // Empty placeholder
                }
              compstmt kEND
                {
            
                    $$ = leave_scope(@$, $3, $5);
                }
            ;

case_body    : kWHEN when_args then compstmt cases
                {
                    $$ = new WHEN($2, $4, $5, @1);
                }
            ;
            
when_args    : args
                {
                    $$ = new ARGS($1, null, null, null, @$);
                }
            | args ',' tSTAR arg_value
                {
                    $$ = new ARGS($1, null, $4, null, @$);
                }
            | tSTAR arg_value
                {
                    $$ = new ARGS(null, null, $2, null, @$);
                }
            ;

cases    : opt_else
        | case_body
        ;

opt_rescue    : kRESCUE exc_list exc_var then compstmt opt_rescue
                {
                    $$ = new RESCUE_CLAUSE(CurrentScope, $2, $3, $5, $6, @1);        
                }
            | none
            ;

exc_list: arg_value
            {
                $$ = $1;
            }
        | mrhs
            {
                $$ = ((ARGS)$1).parameters;
            }
        | none
        ;

exc_var    : tASSOC lhs
            {
                $$ = $2;
            }
        | none
        ;

opt_ensure    : kENSURE compstmt
                {
                    $$ = $2;
                }
            | none
            ;

literal    : numeric
        | symbol
            {
                $$ = new SYMBOL($1, @1);
            }
        | dsym
        ;
        
strings    : string
        ;
        
string    : string1
        | string string1
            {
                $$ = Concat($1, $2);
            }
        ;

string1    : tSTRING_BEG string_contents tSTRING_END
            {
                $$ = $2;    
            }
        ;

xstring    : tXSTRING_BEG xstring_contents tSTRING_END
            {
                $$ = new XSTRING($2, @1);
            }
        ;

regexp    : tREGEXP_BEG xstring_contents tREGEXP_END
            {
                $$ = new REGEXP($2, $3, @1);
            }
        ;

words    : tWORDS_BEG ' ' tSTRING_END
            {
                $$ = new ARRAY(@1);
            }
        | tWORDS_BEG word_list tSTRING_END
            {
                $$ = new ARRAY($2, @1);
            }
        ;

word_list    : /* none */
                {
                    $$ = null;
                }
            | word_list word ' '
                {
                    $$ = append($1, $2);
                }
            ;

word    : string_content
        | word string_content
            {
                $$ = Concat($1, $2);
            }
        ;

qwords    : tQWORDS_BEG ' ' tSTRING_END
            {
                $$ = new ARRAY(@1);
            }
        | tQWORDS_BEG qword_list tSTRING_END
            {
                $$ = new ARRAY($2, @1);
            }
        ;

qword_list    : /* empty */
                {
                    $$ = null;
                }
            | qword_list tSTRING_CONTENT ' '
                {
                    $$ = append($1, $2);
                }
            ;

string_contents    : /* empty */
                    {
                        $$ = new VALUE("", @$);
                    }
                | string_contents string_content
                    {
                        $$ = Concat($1, $2);
                    }
                ;

xstring_contents    : /* empty */
                    {
                        $$ = new VALUE("", @$);
                    }
                | xstring_contents string_content
                    {
                        $$ = Concat($1, $2);
                    }
                ;


string_content    : tSTRING_CONTENT
                    {
                        $$ = $1;
                    }
                | tSTRING_DVAR
                    {
                        $<term>$ = scanner.lex_strterm;
                        scanner.lex_strterm = null;
                        scanner.lex_state = Lex_State.EXPR_BEG;
                    }
                  string_dvar
                    {
                        scanner.lex_strterm = $<term>2;
                        $$ = new EVAL_CODE($3, @1);
                    }
                | tSTRING_DBEG
                    {
                        $<term>$ = scanner.lex_strterm;
                        scanner.lex_strterm = null;
                        scanner.lex_state = Lex_State.EXPR_BEG;
                        scanner.COND_PUSH(0);
                        scanner.CMDARG_PUSH(0);
                    }
                  compstmt '}'
                    {
                        scanner.lex_strterm = $<term>2;
                        scanner.COND_LEXPOP();
                        scanner.CMDARG_LEXPOP();
                        $$ = new EVAL_CODE($3, @1);
                    }
                ;

string_dvar    : tGVAR        {$$ = new GVAR($1, @1);}
            | tIVAR        {$$ = new IVAR($1, @1);}
            | tCVAR        {$$ = new CVAR(CurrentScope, $1, @1);}
            | backref
            ;

symbol    : tSYMBEG sym
            {
                scanner.lex_state = Lex_State.EXPR_END;
                $$ = $2;
            }
        ;

sym    : fname
    | tIVAR
    | tGVAR
    | tCVAR
    ;

dsym: tSYMBEG xstring_contents tSTRING_END
        {
            $$ = new SYMBOL($2, @1);
        }
    ;

numeric    : tINTEGER
        | tFLOAT
        | tUMINUS_NUM tINTEGER                        %prec tLOWEST
            {
                $$ = negate_lit((VALUE)$2);
            }
        | tUMINUS_NUM tFLOAT                        %prec tLOWEST
            {
                $$ = negate_lit((VALUE)$2);
            }
        ;

variable: tIDENTIFIER { $$ = $1; }
        | tIVAR
        | tGVAR
        | tCONSTANT
        | tCVAR
        | kNIL        {$$ = ID.intern(Tokens.kNIL);}
        | kSELF        {$$ = ID.intern(Tokens.kSELF);}
        | kTRUE        {$$ = ID.intern(Tokens.kTRUE);}
        | kFALSE    {$$ = ID.intern(Tokens.kFALSE);}
        | k__FILE__    {$$ = ID.intern(Tokens.k__FILE__);}
        | k__LINE__    {$$ = ID.intern(Tokens.k__LINE__);}
        ;

var_ref    : variable
            {
                $$ = gettable($1, @1);
            }
        ;
        
var_lhs    : variable
            {
                $$ = assignable($1, @1);
            }
        ;

backref    : tNTH_REF
        | tBACK_REF
        ;

superclass    : term
                {
                    $$ = null;
                }
            | '<'
                {
                    scanner.lex_state = Lex_State.EXPR_BEG;
                }
              expr_value term
                {
                    $$ = $3;
                }
            | error term
                {
                    yyerrok();
                     $$ = null;
                }
            ;

f_arglist    : '(' f_args opt_nl    ')'
                {
                    $$ = $2;
                    scanner.lex_state = Lex_State.EXPR_BEG;
                }
            | f_args term
                {
                    $$ = $1;
                }
            ;

f_args    : f_arg ',' f_optarg ',' f_rest_arg opt_f_block_arg
            {
                $$ = new FORMALS($1, $3, $5, $6, @$);
            }
        | f_arg ',' f_optarg opt_f_block_arg
            {
                $$ = new FORMALS($1, $3, null, $4, @$);
            }
        | f_arg ',' f_rest_arg opt_f_block_arg
            {
                $$ = new FORMALS($1, null, $3, $4, @$);
            }
        | f_arg opt_f_block_arg
            {
                $$ = new FORMALS($1, null, null, $2, @$);
            }
        | f_optarg ',' f_rest_arg opt_f_block_arg
            {
                $$ = new FORMALS(null, $1, $3, $4, @$);
            }
        | f_optarg opt_f_block_arg
            {
                $$ = new FORMALS(null, $1, null, $2, @$);
            }
        | f_rest_arg opt_f_block_arg
            {
                $$ = new FORMALS(null, null, $1, $2, @$);
            }
        | f_block_arg
            {
                $$ = new FORMALS(null, null, null, $1, @$);
            }
        | /* empty */ 
            {
                $$ = new FORMALS(null, null, null, null, @$);
            }
        ;

f_norm_arg    : tCONSTANT
                {    
                    scanner.yyerror("formal argument cannot be a constant");
                }
            | tIVAR
                {
                    scanner.yyerror("formal argument cannot be an instance variable");                
                }
            | tGVAR
                {
                    scanner.yyerror("formal argument cannot be a global variable");
                }
            | tCVAR
                {
                    scanner.yyerror("formal argument cannot be a class variable");
                }                
            | tIDENTIFIER
                {           
                    if (ID.Scope($1) != ID_Scope.LOCAL)
                        scanner.yyerror("formal argument must be a local variable");
                    else if (CurrentScope.has_local($1))
                        scanner.yyerror("duplicate argument name");
                    else                
                        $$ = CurrentScope.add_local($1, @1);
                }
            ;

f_arg    : f_norm_arg
            {
                $$ = $1;
            }
        | f_arg ',' f_norm_arg
            {
                $$ = append($1, $3);
            }
        ;

f_opt        : tIDENTIFIER '=' arg_value
            {        
                if (ID.Scope($1) != ID_Scope.LOCAL)
                    scanner.yyerror("formal argument must be local variable");
                else if (CurrentScope.has_local($1))
                    scanner.yyerror("duplicate optional argument name");
                else
                    $$ = new ASSIGNMENT(CurrentScope.add_local($1, @1), $3, @$);
            }
        ;

f_optarg    : f_opt
            {
                $$ = $1;
            }
        | f_optarg ',' f_opt
            {
                $$ = append($1, $3);
            }
        ;

restarg_mark: '*'
            | tSTAR
            ;

f_rest_arg    : restarg_mark tIDENTIFIER
                {    
                    if (ID.Scope($2) != ID_Scope.LOCAL)
                        scanner.yyerror("rest argument must be local variable");
                    else if (CurrentScope.has_local($2))
                        scanner.yyerror("duplicate rest argument name");
                    else    
                        $$ = CurrentScope.add_local($2, @2);
                }
            | restarg_mark
                {
                    $$ = CurrentScope.add_local("?rest?", @1);
                }
            ;

blkarg_mark    : '&'
            | tAMPER
            ;

f_block_arg    : blkarg_mark tIDENTIFIER
                {        
                    if (ID.Scope($2) != ID_Scope.LOCAL)
                        scanner.yyerror("block argument must be a local variable");
                    else if (CurrentScope.has_local($2))
                        scanner.yyerror("duplicate block argument name");
                    else    
                        $$ = CurrentScope.add_local($2, @2);
                }
            ;

opt_f_block_arg    : ',' f_block_arg
                    {
                        $$ = $2;
                    }
                | none
                ;

singleton    : var_ref
            | '('
                {
                    scanner.lex_state = Lex_State.EXPR_BEG;
                }
              expr opt_nl ')'
                {                        
                    $$ = $3;
                }
            ;

assoc_list    : none
            | assocs trailer
                {
                    $$ = $1;
                }
            | args trailer
                {                                    
                    $$ = $1;
                }
            ;

assocs    : assoc
        | assocs ',' assoc
            {
                $$ = append($1, $3);
            }
        ;

assoc    : arg_value tASSOC arg_value
            {
                // Fixme: should be a Hash???
                $$ = append($1, $3);
            }
        ;

operation    : tIDENTIFIER
            | tCONSTANT
            | tFID
            ;

operation2    : tIDENTIFIER
            | tCONSTANT
            | tFID
            | op
            ;

operation3    : tIDENTIFIER
            | tFID
            | op
            ;

dot_or_colon: '.'
            | tCOLON2
            ;

opt_terms    : /* none */
            | terms
            ;

opt_nl    : /* none */
        | '\n'
        ;

trailer    : /* none */
        | '\n'
        | ','
        ;

term    : ';'        { yyerrok(); }
        | '\n'
        ;

terms    : term
        | terms ';'    { yyerrok(); }
        ;

none: /* none */    { $$ = null; }
    ;
    
%%