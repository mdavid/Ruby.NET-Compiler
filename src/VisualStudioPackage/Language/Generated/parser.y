%namespace Ruby.NET.Parser

%partial

%union 
{
    public int num;
    public Terminator term;
    public string id;
}

%token kCLASS kMODULE kDEF kUNDEF kBEGIN kRESCUE kENSURE kEND kIF kUNLESS kTHEN kELSIF kELSE
%token kCASE kWHEN kWHILE kUNTIL kFOR kBREAK kNEXT kREDO kRETRY kIN kDO kDO_COND kDO_BLOCK
%token kRETURN kYIELD kSUPER kSELF kNIL kTRUE kFALSE kAND kOR kNOT kIF_MOD kUNLESS_MOD
%token kWHILE_MOD kUNTIL_MOD kRESCUE_MOD kALIAS kDEFINED klBEGIN klEND k__LINE__ k__FILE__
%token tUPLUS tUMINUS tPOW tCMP tEQ tEQQ tNEQ tGEQ tLEQ tANDOP tOROP tMATCH tNMATCH
%token tDOT2 tDOT3 tAREF tASET tLSHFT tRSHFT tCOLON2 tCOLON3 tASSOC tLPAREN tSTRING_END
%token tLPAREN_ARG tLBRACK tLBRACE tLBRACE_ARG tSTAR tAMPER tSTRING_DVAR

%token <id>   tIDENTIFIER tFID tGVAR tIVAR tCONSTANT tCVAR tOP_ASGN 
%token tINTEGER tFLOAT tSTRING_CONTENT tNTH_REF tBACK_REF
%token <num>  tREGEXP_END 
%token <term> tSTRING_BEG tREGEXP_BEG tXSTRING_BEG tWORDS_BEG tQWORDS_BEG tSTRING_DBEG tSYMBEG

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
                enter_scope();
            }
        compstmt
            {
                leave_scope();
            }
        ;

bodystmt: compstmt opt_rescue opt_else opt_ensure
        ;

compstmt: stmts    opt_terms
            {
                DefineRegion(@$);
            }
        ;

stmts    : none
        | stmt
        | stmts terms stmt
        | error stmt
        ;

stmt    : kALIAS fitem 
            {
                scanner.lex_state =    Lex_State.EXPR_FNAME;
            } 
          fitem
        | kALIAS tGVAR tGVAR
        | kALIAS tGVAR tBACK_REF
        | kUNDEF undef_list
        | stmt kIF_MOD expr_value
        | stmt kUNLESS_MOD expr_value
        | stmt kWHILE_MOD expr_value
        | stmt kUNTIL_MOD expr_value
        | stmt kRESCUE_MOD stmt
        | klBEGIN
            {
                enter_scope();
            } 
          '{' compstmt '}'
            {
                Match(@3, @5);
                leave_scope();
            }
        | klEND 
            {
                enter_scope();
            }         
          '{' compstmt '}'
            {
                Match(@3, @5);            
                leave_scope();
            }          
        | lhs '=' command_call
        | mlhs '=' command_call
        | var_lhs tOP_ASGN command_call
        | primary_value '[' aref_args ']' tOP_ASGN command_call        {    Match(@2, @4); }
        | primary_value '.' tIDENTIFIER tOP_ASGN command_call
        | primary_value '.' tCONSTANT tOP_ASGN command_call
        | primary_value tCOLON2 tIDENTIFIER tOP_ASGN command_call
        | lhs '=' mrhs
        | mlhs '=' arg_value
        | mlhs '=' mrhs
        | expr
        ;

expr    : command_call
        | expr kAND expr
        | expr kOR expr
        | kNOT expr
        | '!' command_call
        | arg
        ;

expr_value    : expr
            ;

command_call: command
            | block_command
            | kRETURN call_args
            | kBREAK call_args
            | kNEXT call_args
            ;

block_command    : block_call
                | block_call '.' operation2 command_args
                | block_call tCOLON2 operation2 command_args
                ;

cmd_brace_block    : tLBRACE_ARG 
            {
                enter_scope();
            }
                  opt_block_var compstmt '}'
            {
                Match(@1, @5);
                leave_scope();
            }                  
                ;

command    : operation    command_args                                        %prec tLOWEST
        | operation command_args cmd_brace_block
        | primary_value '.' operation2 command_args                        %prec tLOWEST
        | primary_value '.' operation2 command_args cmd_brace_block
        | primary_value tCOLON2 operation2 command_args                    %prec tLOWEST
        | primary_value tCOLON2 operation2 command_args cmd_brace_block
        | kSUPER command_args
        | kYIELD command_args
        ;


mlhs    : mlhs_basic
        | tLPAREN mlhs_entry ')'        { Match(@1, @3); }
        ;
        
mlhs_entry    : mlhs_basic
            | tLPAREN mlhs_entry ')'    { Match(@1, @3); }
            ;
                    
    
mlhs_basic    : mlhs_head
            | mlhs_head mlhs_item
            | mlhs_head tSTAR mlhs_node
            | mlhs_head tSTAR
            | tSTAR mlhs_node
            | tSTAR
            ;

mlhs_item    : mlhs_node
            | tLPAREN mlhs_entry ')'    { Match(@1, @3); }
            ;

mlhs_head    : mlhs_item ','
            | mlhs_head mlhs_item ','
            ;
            
mlhs_node    : variable
                {
                    assignable($1);
                }
            | primary_value '[' aref_args ']'        { Match(@2, @4); }
            | primary_value '.' tIDENTIFIER
            | primary_value tCOLON2 tIDENTIFIER
            | primary_value '.' tCONSTANT
            | primary_value tCOLON2 tCONSTANT
            | tCOLON3 tCONSTANT
            ;

lhs    : variable
        {
            assignable($1);
        }
    | primary_value '[' aref_args ']'                { Match(@2, @4); }
    | primary_value '.' tIDENTIFIER
    | primary_value tCOLON2 tIDENTIFIER
    | primary_value '.' tCONSTANT
    | primary_value tCOLON2 tCONSTANT
    | tCOLON3 tCONSTANT
    ;

cname    : tCONSTANT
        ;

cpath    : tCOLON3 cname
        | cname
        | primary_value tCOLON2 cname
        ;

fname    : tIDENTIFIER
        | tCONSTANT
        | tFID
        | op
            {
                scanner.lex_state = Lex_State.EXPR_END;
            }
        | reswords
            {
                scanner.lex_state = Lex_State.EXPR_END;
            }
        ;

fitem    : fname
        | symbol
        ;

undef_list    : fitem
            | undef_list ',' 
                {
                    scanner.lex_state = Lex_State.EXPR_FNAME;
                } 
              fitem
            ;

op        : '|'
        | '^'
        | '&'
        | tCMP
        | tEQ
        | tEQQ    
        | tMATCH
        | '>'
        | tGEQ
        | '<'
        | tLEQ
        | tLSHFT
        | tRSHFT
        | '+'
        | '-'
        | '*'
        | tSTAR
        | '/'
        | '%'
        | tPOW
        | '~'
        | tUPLUS
        | tUMINUS
        | tAREF
        | tASET
        | '`'
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
        | lhs '=' arg kRESCUE_MOD arg
        | var_lhs tOP_ASGN arg
        | primary_value '[' aref_args ']' tOP_ASGN arg        { Match(@2, @4); }
        | primary_value '.' tIDENTIFIER tOP_ASGN arg
        | primary_value '.' tCONSTANT tOP_ASGN arg
        | primary_value tCOLON2 tIDENTIFIER tOP_ASGN arg    
        | primary_value tCOLON2 tCONSTANT tOP_ASGN arg
            {
                scanner.yyerror("constant re-assignment");
            }
        | tCOLON3 tCONSTANT tOP_ASGN arg
            {
                scanner.yyerror("constant re-assignment");
            }            
        | arg tDOT2 arg
        | arg tDOT3 arg
        | arg '+' arg
        | arg '-' arg
        | arg '*' arg
        | arg '/' arg
        | arg '%' arg
        | arg tPOW arg
        | tUMINUS_NUM tINTEGER tPOW arg
        | tUMINUS_NUM tFLOAT tPOW arg
        | tUPLUS arg
        | tUMINUS arg
        | arg '|' arg
        | arg '^' arg
        | arg '&' arg
        | arg tCMP arg
        | arg '>' arg
        | arg tGEQ arg
        | arg '<' arg
        | arg tLEQ arg
        | arg tEQ arg
        | arg tEQQ arg
        | arg tNEQ arg
        | arg tMATCH arg
        | arg tNMATCH arg
        | '!' arg
        | '~' arg
        | arg tLSHFT arg
        | arg tRSHFT arg
        | arg tANDOP arg
        | arg tOROP arg
        | kDEFINED opt_nl arg
        | arg '?' arg ':' arg
        | primary
        ;
        
arg_value    : arg
            ;

aref_args    : none
            | command opt_nl
            | args trailer
            | args ',' tSTAR arg opt_nl
            | assocs trailer
            | tSTAR arg opt_nl
            ;

paren_args    : '(' none ')'                            { Match(@1, @3); }
            | '(' call_args opt_nl ')'                { Match(@1, @4); }
            | '(' block_call opt_nl ')'                { Match(@1, @4); }
            | '(' args ',' block_call opt_nl ')'    { Match(@1, @6); }
            ;

opt_paren_args    : none
                | paren_args
                ;

call_args    : command
            | args opt_block_arg
            | args ',' tSTAR arg_value opt_block_arg
            | assocs opt_block_arg
            | assocs ',' tSTAR arg_value opt_block_arg
            | args ',' assocs opt_block_arg
            | args ',' assocs ',' tSTAR arg opt_block_arg
            | tSTAR arg_value opt_block_arg
            | block_arg
            ;

call_args2    : arg_value ',' args opt_block_arg
            | arg_value ',' block_arg
            | arg_value ',' tSTAR arg_value opt_block_arg
            | arg_value ',' args ',' tSTAR arg_value opt_block_arg
            | assocs opt_block_arg
            | assocs ',' tSTAR arg_value opt_block_arg
            | arg_value ',' assocs opt_block_arg
            | arg_value ',' args ',' assocs opt_block_arg
            | arg_value ',' assocs ',' tSTAR arg_value opt_block_arg
            | arg_value ',' args ',' assocs ',' tSTAR arg_value opt_block_arg
            | tSTAR arg_value opt_block_arg
            | block_arg
            ;

command_args:
                {
                    scanner.CMDARG_PUSH(1);
                }
              open_args
                {
                    scanner.CMDARG_POP();
                }
            ;

open_args    : call_args
            | tLPAREN_ARG
                {
                    scanner.lex_state = Lex_State.EXPR_ENDARG;
                }
              ')'
                { 
                    Match(@1, @3); 
                }
            | tLPAREN_ARG call_args2
                {
                    scanner.lex_state = Lex_State.EXPR_ENDARG;
                }
              ')'
                { 
                    Match(@1, @4); 
                }              
            ;

block_arg    : tAMPER arg_value
            ;

opt_block_arg    : ',' block_arg
                | none
                ;

args: arg_value
    | args ',' arg_value
    ;

mrhs: args ',' arg_value
    | args ',' tSTAR arg_value
    | tSTAR arg_value
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
        | kBEGIN bodystmt kEND
        | tLPAREN_ARG expr 
            {
                scanner.lex_state = Lex_State.EXPR_ENDARG;
            } 
          opt_nl ')'
                  { 
                    Match(@1, @5); 
                }
        | tLPAREN compstmt ')'
                { 
                    Match(@1, @3); 
                }        
        | primary_value tCOLON2 tCONSTANT
            {
            }
        | tCOLON3 tCONSTANT
            {
            }
        | primary_value '[' aref_args ']'        { Match(@2, @4); }
        | tLBRACK aref_args ']'                    { Match(@1, @3); }
        | tLBRACE assoc_list '}'                { Match(@1, @3); }
        | kRETURN
        | kYIELD '(' call_args ')'                { Match(@2, @4); }
        | kYIELD '(' ')'                        { Match(@2, @3); }
        | kYIELD
        | kDEFINED opt_nl '('  expr ')'            { Match(@3, @5); }
        | operation brace_block
        | method_call
        | method_call brace_block
        | kIF expr_value then compstmt if_tail kEND            { Match(@1, @6); }
        | kUNLESS expr_value then compstmt opt_else kEND    { Match(@1, @6); }
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
                Match(@1, @7); 
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
                Match(@1, @7); 
            }          
        | kCASE expr_value opt_terms case_body kEND        { Match(@1, @5); }
        | kCASE opt_terms case_body kEND                { Match(@1, @4); }
        | kCASE opt_terms kELSE compstmt kEND            { Match(@1, @5); }
        | kFOR
            {
                enter_scope();
            } 
          block_var kIN
            {
                leave_scope();
                scanner.COND_PUSH(1);
            }
          expr_value do
            {
                enter_scope();
                scanner.COND_POP();
            }
          compstmt kEND
            {
                // Match(@1, @10);
                leave_scope();
            }
        | kCLASS cpath superclass
            {
                enter_scope();
            } 
          bodystmt kEND
            {
                Match(@1, @6);
                leave_scope();
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
                enter_scope();
            }
          bodystmt kEND
            {
                Match(@1, @7);
                in_def = $<num>4;
                in_single = $<num>6;
                leave_scope();                
            }
        | kMODULE cpath
            {
                enter_scope();
            } 
          bodystmt kEND
            {
                Match(@1, @5);            
                leave_scope();
            }
        | kDEF fname
            {
                in_def++;
                enter_scope();
            }
          f_arglist bodystmt kEND
            {
                Match(@1, @6);            
                in_def--;
                leave_scope();
            }
        | kDEF singleton dot_or_colon
            {
                scanner.lex_state = Lex_State.EXPR_FNAME;
            }
          fname
            {
                in_single++;
                scanner.lex_state = Lex_State.EXPR_END;    
                enter_scope();
            }
          f_arglist bodystmt kEND
            {
                Match(@1, @9);            
                in_single--;
                leave_scope();
            }
        | kBREAK
        | kNEXT
        | kREDO
        | kRETRY
        ;

primary_value    : primary
                    {
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
        ;

opt_else: none
        | kELSE compstmt
        ;

block_var    : lhs
            | mlhs
            ;

opt_block_var    : none
                | '|' none '|'            { Match(@1, @3); }
                | tOROP
                | '|' block_var '|'        { Match(@1, @3); }
                ;

do_block: kDO_BLOCK 
            {
                enter_block();
            }
          opt_block_var compstmt kEND
            {
                Match(@1, @5);        
                leave_scope();
            }
        ;

block_call    : command do_block
            | block_call '.' operation2 opt_paren_args
            | block_call tCOLON2 operation2 opt_paren_args
            ;

method_call    : operation    paren_args
            | primary_value '.' operation2 opt_paren_args
            | primary_value tCOLON2 operation2 paren_args
            | primary_value tCOLON2 operation3
            | kSUPER paren_args
            | kSUPER
            ;

brace_block    : '{'
                {
                    enter_block();
                } 
              opt_block_var compstmt '}'
                {
                    Match(@1, @5); 
                    leave_scope();
                }
            | kDO
                {
                    enter_block();
                } 
              opt_block_var compstmt kEND
                {
                    Match(@1, @5);                        
                    leave_scope();
                }
            ;

case_body    : kWHEN when_args then compstmt cases
            ;
            
when_args    : args
            | args ',' tSTAR arg_value
            | tSTAR arg_value
            ;

cases    : opt_else
        | case_body
        ;

opt_rescue    : kRESCUE exc_list exc_var then compstmt opt_rescue
            | none
            ;

exc_list: arg_value
        | mrhs
        | none
        ;

exc_var    : tASSOC lhs
        | none
        ;

opt_ensure    : kENSURE compstmt
            | none
            ;

literal    : numeric
        | symbol
        | dsym
        ;
        
strings    : string
        ;
        
string    : string1
        | string string1
        ;

string1    : tSTRING_BEG string_contents tSTRING_END    { Match(@1, @3); }
        ;

xstring    : tXSTRING_BEG xstring_contents tSTRING_END    { Match(@1, @3); }
        ;

regexp    : tREGEXP_BEG xstring_contents tREGEXP_END    { Match(@1, @3); }
        ;

words    : tWORDS_BEG ' ' tSTRING_END                { Match(@1, @3); }
        | tWORDS_BEG word_list tSTRING_END            { Match(@1, @3); }
        ;

word_list    : /* none */
            | word_list word ' '
            ;

word    : string_content
        | word string_content
        ;

qwords    : tQWORDS_BEG ' ' tSTRING_END                { Match(@1, @3); }
        | tQWORDS_BEG qword_list tSTRING_END        { Match(@1, @3); }
        ;

qword_list    : /* empty */
            | qword_list tSTRING_CONTENT ' '
            ;

string_contents    : /* empty */
                | string_contents string_content
                ;

xstring_contents: /* empty */
                | xstring_contents string_content
                ;


string_content    : tSTRING_CONTENT
                | tSTRING_DVAR
                    {
                        $<term>$ = scanner.lex_strterm;
                        scanner.lex_strterm = null;
                        scanner.lex_state = Lex_State.EXPR_BEG;
                    }
                  string_dvar
                    {
                        scanner.lex_strterm = $<term>2;
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
                        Match(@1, @4);
                        scanner.lex_strterm = $<term>2;
                        scanner.COND_LEXPOP();
                        scanner.CMDARG_LEXPOP();
                    }
                ;

string_dvar    : tGVAR
            | tIVAR
            | tCVAR
            | backref
            ;

symbol    : tSYMBEG sym
            {
                scanner.lex_state = Lex_State.EXPR_END;
            }
        ;

sym    : fname
    | tIVAR
    | tGVAR
    | tCVAR
    ;

dsym: tSYMBEG xstring_contents tSTRING_END        { Match(@1, @3); }
    ;

numeric    : tINTEGER
        | tFLOAT
        | tUMINUS_NUM tINTEGER                        %prec tLOWEST
        | tUMINUS_NUM tFLOAT                        %prec tLOWEST
        ;

variable: tIDENTIFIER
        | tIVAR
        | tGVAR
        | tCONSTANT
        | tCVAR
        | kNIL
        | kSELF
        | kTRUE
        | kFALSE
        | k__FILE__
        | k__LINE__
        ;

var_ref    : variable
        ;
        
var_lhs    : variable            
            {
                assignable($1);
            }

        ;

backref    : tNTH_REF
        | tBACK_REF
        ;

superclass    : term
            | '<'
                {
                    scanner.lex_state = Lex_State.EXPR_BEG;
                }
              expr_value term
            | error term
                {
                    yyerrok();
                }
            ;

f_arglist    : '(' f_args opt_nl    ')'
                {
                    Match(@1, @4);
                    scanner.lex_state = Lex_State.EXPR_BEG;
                }
            | f_args term
            ;

f_args    : f_arg ',' f_optarg ',' f_rest_arg opt_f_block_arg
        | f_arg ',' f_optarg opt_f_block_arg
        | f_arg ',' f_rest_arg opt_f_block_arg
        | f_arg opt_f_block_arg
        | f_optarg ',' f_rest_arg opt_f_block_arg
        | f_optarg opt_f_block_arg
        | f_rest_arg opt_f_block_arg
        | f_block_arg
        | /* empty */ 
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
                        CurrentScope.add_local($1);
                }
            ;

f_arg    : f_norm_arg
        | f_arg ',' f_norm_arg
        ;

f_opt    : tIDENTIFIER '=' arg_value
            {        
                if (ID.Scope($1) != ID_Scope.LOCAL)
                    scanner.yyerror("formal argument must be local variable");
                else if (CurrentScope.has_local($1))
                    scanner.yyerror("duplicate optional argument name");
                else
                    CurrentScope.add_local($1);;
            }
        ;

f_optarg: f_opt
        | f_optarg ',' f_opt
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
                        CurrentScope.add_local($2);
                }
            | restarg_mark
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
                        CurrentScope.add_local($2);
                }
            ;

opt_f_block_arg    : ',' f_block_arg
                | none
                ;

singleton    : var_ref
            | '('
                {
                    scanner.lex_state = Lex_State.EXPR_BEG;
                }
              expr opt_nl ')'
                { 
                    Match(@1, @5); 
                }
            ;

assoc_list    : none
            | assocs trailer
            | args trailer
            ;

assocs    : assoc
        | assocs ',' assoc
        ;

assoc    : arg_value tASSOC arg_value
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

none: /* none */
    ;
    
%%