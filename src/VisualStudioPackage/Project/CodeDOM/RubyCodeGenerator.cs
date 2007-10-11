using System;
using System.Collections.Generic;
using System.Text;
using System.CodeDom;
using System.CodeDom.Compiler;
using Ruby.Compiler;



namespace VSRuby.NET
{
    public class RubyCodeGenerator : ICodeGenerator
    {
        public void GenerateCodeFromCompileUnit(CodeCompileUnit e, System.IO.TextWriter w, CodeGeneratorOptions o)
        {
            string text = (string) e.UserData["text"];

            CodeTypeDeclaration FormClass = e.Namespaces[0].Types[0];

            Dictionary<string, YYLTYPE> fields = (Dictionary<string, YYLTYPE>)FormClass.UserData["fields"];
            Dictionary<string, YYLTYPE> methods = (Dictionary<string, YYLTYPE>)FormClass.UserData["methods"];

            List<CodeMemberField> renamed_fields = new List<CodeMemberField>();
            List<CodeMemberField> new_fields = new List<CodeMemberField>();

            List<CodeMemberMethod> renamed_methods = new List<CodeMemberMethod>();
            List<CodeMemberMethod> new_methods = new List<CodeMemberMethod>();

            Dictionary<string, YYLTYPE> removed_fields = (Dictionary<string, YYLTYPE>)FormClass.UserData["fields"];
            Dictionary<string, YYLTYPE> removed_methods = (Dictionary<string, YYLTYPE>)FormClass.UserData["methods"];

            CodeMemberMethod InitializeComponent = null;
            foreach (CodeTypeMember member in FormClass.Members)
            {
                if (member is CodeMemberField)
                {
                    CodeMemberField field = (CodeMemberField)member;
                    if (field.UserData.Contains("original_name"))
                    {
                        string original_name = (string)field.UserData["original_name"];
                        removed_fields.Remove(original_name);
                        
                        if (original_name != field.Name)
                            renamed_fields.Add(field);
                    }
                    else
                        new_fields.Add(field);
                }
                else if (member is CodeMemberMethod)
                {
                    CodeMemberMethod method = (CodeMemberMethod)member;
                    if (method.Name == "InitializeComponent")
                        InitializeComponent = method;

                    if (method.UserData.Contains("original_name"))
                    {
                        string original_name = (string)method.UserData["original_name"];
                        removed_methods.Remove(original_name);

                        if (original_name != method.Name)
                            renamed_methods.Add(method);
                    }
                    else
                        new_methods.Add(method);
                }
                else
                    throw new System.NotSupportedException(member.GetType().ToString());
            }

            string[] lines = text.Split(new string[] { "\r\n" }, StringSplitOptions.None);

            bool[] removed = new bool[lines.Length+1];
            for (int i=0; i<removed.Length; i++)
                removed[i] = false;

            foreach (YYLTYPE location in removed_fields.Values)
                for (int i=location.first_line; i <= location.last_line; i++)
                    removed[i] = true;

            foreach (YYLTYPE location in removed_methods.Values)
                for (int i=location.first_line; i <= location.last_line; i++)
                    removed[i] = true;

            CodeTypeMember[] renamed = new CodeTypeMember[lines.Length+1];
            for (int i=0; i<renamed.Length; i++)
                renamed[i] = null;

            foreach (CodeMemberField field in renamed_fields)
            {
                YYLTYPE location = (YYLTYPE)field.UserData["name_location"];
                renamed[location.first_line] = field;
            }
            foreach (CodeMemberMethod method in renamed_methods)
            {
                YYLTYPE location = (YYLTYPE)method.UserData["name_location"];
                renamed[location.first_line] = method;
            }

            Ruby.Compiler.YYLTYPE InitializeComponentLocation = (YYLTYPE)InitializeComponent.UserData["location"];

            // if InitializeComponent didn't previously exist
            if (InitializeComponentLocation == null)
            {
                // create empty range at start of class
                InitializeComponentLocation = (YYLTYPE)FormClass.UserData["body_location"];
                InitializeComponentLocation.last_line = InitializeComponentLocation.first_line - 1;
            }

            for (int i = 1; i <= lines.Length; i++)
            {
                string line = lines[i-1];

                if (i == InitializeComponentLocation.first_line)
                    using (InternalCodeGenerator gen = new InternalCodeGenerator(w))
                    {
                        gen.Indent++;
                        gen.WriteLine();

                        // insert new fields
                        foreach (CodeMemberField field in new_fields)
                            gen.Generate(field);

                        gen.WriteLine();

                        // insert new InitializeComponent method
                        gen.Generate(InitializeComponent);

                        // insert new methods
                        foreach (CodeMemberMethod method in new_methods)
                        {
                            gen.WriteLine();
                            gen.Generate(method);
                        }

                        gen.Indent--;
                    }
                else if (i < InitializeComponentLocation.first_line || i > InitializeComponentLocation.last_line)
                {
                    if (renamed[i] != null)
                    {
                        YYLTYPE location = (YYLTYPE)renamed[i].UserData["name_location"];
                        string original = (string)renamed[i].UserData["original_name"];
                        line = line.Substring(0, location.first_column) + renamed[i].Name + line.Substring(location.first_column+original.Length);
                    }
                    if (!removed[i])
                        w.WriteLine(line);
                }
            }
        }

        public void ValidateIdentifier(string value)
        {
        }

        public bool IsValidIdentifier(string value)
        {
            return true;
        }

        public string CreateValidIdentifier(string value)
        {
            return value;
        }

        #region NotSupported
        public void GenerateCodeFromExpression(CodeExpression e, System.IO.TextWriter w, CodeGeneratorOptions o)
        {
            throw new NotSupportedException();
        }

        public void GenerateCodeFromNamespace(CodeNamespace e, System.IO.TextWriter w, CodeGeneratorOptions o)
        {
            throw new NotSupportedException();
        }

        public void GenerateCodeFromStatement(CodeStatement e, System.IO.TextWriter w, CodeGeneratorOptions o)
        {
            throw new NotSupportedException();
        }

        public void GenerateCodeFromType(CodeTypeDeclaration e, System.IO.TextWriter w, CodeGeneratorOptions o)
        {
            throw new NotSupportedException();
        }

        public string CreateEscapedIdentifier(string value)
        {
            throw new NotSupportedException();
        }

        public string GetTypeOutput(CodeTypeReference type)
        {
            throw new NotSupportedException();
        }

        public bool Supports(GeneratorSupport supports)
        {
            throw new NotSupportedException();
        }
        #endregion


        private class InternalCodeGenerator : IndentedTextWriter
        {
            public InternalCodeGenerator(System.IO.TextWriter tw): base(tw, "  ")
            {
            }


            public void Generate(CodeMemberMethod member)
            {
                if (member is CodeTypeConstructor)
                    Generate((CodeTypeConstructor)member);
                else
                {
                    if (member is CodeEntryPointMethod)
                    {
                        member.Name = "main";
                        member.Attributes = member.Attributes | MemberAttributes.Static;
                    }
                    else if (member is CodeConstructor)
                    {
                        member.Name = "initialize";
                    }

                    Generate(member.Comments);
                    string self = "";
                    if (MemberAttributes.Static == (member.Attributes & MemberAttributes.Static))
                        self = "self.";

                    Write("def {0}{1}", self, member.Name);
                    if (member.Parameters.Count > 0)
                    {
                        Write("(");
                        for (int i = 0; i < member.Parameters.Count; i++)
                        {
                            Write(member.Parameters[i].Name);
                            if (i != member.Parameters.Count - 1)
                                Write(",");
                        }
                        Write(")");
                    }
                    WriteLine();

                    {
                        Indent++;
                        foreach (CodeStatement statement in member.Statements)
                            Generate(statement);
                        Indent--;
                    }
                    WriteLine("end");



                    if (member is CodeEntryPointMethod)
                    {
                        WriteLine();
                        WriteLine("self.{0}", member.Name);
                    }
                }
            }


            public void Generate(CodeMemberField member)
            {
                WriteLine("attr_accessor :{0}", member.Name);
            }


            public void Generate(CodeStatement statement)
            {
                if (statement is CodeAssignStatement)
                    Generate((CodeAssignStatement)statement);
                else if (statement is CodeAttachEventStatement)
                    Generate((CodeAttachEventStatement)statement);
                else if (statement is CodeCommentStatement)
                    Generate((CodeCommentStatement)statement);
                else if (statement is CodeExpressionStatement)
                    Generate((CodeExpressionStatement)statement);
                else if (statement is CodeVariableDeclarationStatement)
                    Generate((CodeVariableDeclarationStatement)statement);
                else
                    throw new NotSupportedException();
            }


            public void Generate(CodeAttachEventStatement statement)
            {
                Generate(statement.Event.TargetObject);
                Write(".add_{0}(", statement.Event.EventName);
                Generate(statement.Listener);
                WriteLine(")");
            }


            public void Generate(CodeCommentStatementCollection comments)
            {
                foreach (CodeCommentStatement comment in comments)
                    Generate(comment);
            }


            public void Generate(CodeCommentStatement statement)
            {
                WriteLine("# " + statement.Comment.Text);
            }


            public void Generate(CodeAssignStatement statement)
            {
                if (statement.Left is CodePropertyReferenceExpression)
                {
                    CodePropertyReferenceExpression prop = (CodePropertyReferenceExpression)statement.Left;
                    Generate(prop.TargetObject);
                    Write(".set_{0}(", prop.PropertyName);
                    Generate(statement.Right);
                    WriteLine(")");
                }
                else
                {
                    Generate(statement.Left);
                    Write(" = ");
                    Generate(statement.Right);
                    WriteLine();
                }
            }


            public void Generate(CodeExpressionStatement statement)
            {
                Generate(statement.Expression);
                WriteLine();
            }


            public void Generate(CodeVariableDeclarationStatement statement)
            {
                //Fixme
                Write("{0} = ", statement.Name);

                if (statement.InitExpression == null)
                    Write("nil");
                else
                    Generate(statement.InitExpression);

                WriteLine();
            }


            public void Generate(CodeExpression expression)
            {
                if (expression is CodeArrayIndexerExpression)
                    Generate((CodeBaseReferenceExpression)expression);
                else if (expression is CodeBinaryOperatorExpression)
                    Generate((CodeBinaryOperatorExpression)expression);
                else if (expression is CodeCastExpression)
                    Generate((CodeCastExpression)expression);
                else if (expression is CodeDelegateCreateExpression)
                    Generate((CodeDelegateCreateExpression)expression);
                else if (expression is CodeFieldReferenceExpression)
                    Generate((CodeFieldReferenceExpression)expression);
                else if (expression is CodeMethodInvokeExpression)
                    Generate((CodeMethodInvokeExpression)expression);
                else if (expression is CodeMethodReferenceExpression)
                    Generate((CodeMethodReferenceExpression)expression);
                else if (expression is CodeObjectCreateExpression)
                    Generate((CodeObjectCreateExpression)expression);
                else if (expression is CodePrimitiveExpression)
                    Generate((CodePrimitiveExpression)expression);
                else if (expression is CodePropertyReferenceExpression)
                    Generate((CodePropertyReferenceExpression)expression);
                else if (expression is CodeThisReferenceExpression)
                    Generate((CodeThisReferenceExpression)expression);
                else if (expression is CodeTypeOfExpression)
                    Generate((CodeTypeOfExpression)expression);
                else if (expression is CodeArrayCreateExpression)
                    Generate((CodeArrayCreateExpression)expression);
                else if (expression is CodeTypeReferenceExpression)
                    Generate((CodeTypeReferenceExpression)expression);
                else
                    throw new NotSupportedException(expression.GetType().ToString());
            }


            public void Generate(CodeCastExpression expression)
            {
                // Fixme
                Generate(expression.Expression);
            }


            public void Generate(CodeArrayCreateExpression expression)
            {
                // Fixme
                throw new NotImplementedException(expression.GetType().ToString());
            }


            public void Generate(CodeTypeOfExpression expression)
            {
                Write("{0}.GetType()", TypeToString(expression.Type));
            }


            public void Generate(CodeTypeReferenceExpression expression)
            {
                Write(TypeToString(expression.Type));
            }


            public void Generate(CodeDelegateCreateExpression expression)
            {
                Write("{0}.new {{ |*args| ", TypeToString(expression.DelegateType));
                Generate(expression.TargetObject);
                Write(".{0}(*args)", expression.MethodName);
                Write("}");
            }


            public void Generate(CodeFieldReferenceExpression expression)
            {
                if (expression.TargetObject is CodeThisReferenceExpression)
                {
                    Write("@{0}", expression.FieldName);
                }
                else
                {
                    Generate(expression.TargetObject);
                    Write(".{0}", expression.FieldName);
                }
            }


            public void Generate(CodeMethodInvokeExpression expression)
            {
                Generate(expression.Method);
                if (expression.Parameters.Count > 0)
                {
                    Write("(");
                    for (int i = 0; i < expression.Parameters.Count; i++)
                    {
                        Generate(expression.Parameters[i]);
                        if (i != expression.Parameters.Count - 1)
                            Write(", ");
                    }
                    Write(")");
                }
            }


            public void Generate(CodeMethodReferenceExpression expression)
            {
                if (!(expression.TargetObject is CodeThisReferenceExpression) || Char.IsUpper(expression.MethodName[0]))
                {
                    Generate(expression.TargetObject);
                    Write(".");
                }
                Write(expression.MethodName);
            }


            public void Generate(CodeObjectCreateExpression expression)
            {
                Write("{0}.new", TypeToString(expression.CreateType));
                if (expression.Parameters.Count > 0)
                {
                    Write("(");
                    for (int i = 0; i < expression.Parameters.Count; i++)
                    {
                        Generate(expression.Parameters[i]);
                        if (i != expression.Parameters.Count - 1)
                            Write(", ");
                    }
                    Write(")");
                }
            }


            public void Generate(CodePrimitiveExpression expression)
            {
                if (expression.Value is string)
                    Write("'{0}'", expression.Value);
                else if (expression.Value is bool)
                    Write((bool)expression.Value ? "true" : "false");
                else
                    Write(expression.Value);
            }


            public void Generate(CodePropertyReferenceExpression expression)
            {
                Generate(expression.TargetObject);
                Write(".get_{0}", expression.PropertyName);
            }

            public void Generate(CodeThisReferenceExpression expression)
            {
                Write("self");
            }


            public void Generate(CodeArrayIndexerExpression expression)
            {
                throw new NotSupportedException(expression.GetType().ToString());
                Generate(expression.TargetObject);
                Write("[");
                for (int i = 0; i < expression.Indices.Count; i++)
                {
                    Generate(expression.Indices[i]);
                    if (i != expression.Indices.Count - 1)
                        Write(", ");
                }
                Write("]");
            }


            public void Generate(CodeBinaryOperatorExpression expression)
            {
                Generate(expression.Left);
                switch (expression.Operator)
                {
                    case CodeBinaryOperatorType.Add:
                        Write(" + ");
                        break;
                    case CodeBinaryOperatorType.Assign:
                        Write(" = ");
                        break;
                    case CodeBinaryOperatorType.BitwiseAnd:
                        Write(" & ");
                        break;
                    case CodeBinaryOperatorType.BitwiseOr:
                        Write(" | ");
                        break;
                    case CodeBinaryOperatorType.BooleanAnd:
                        Write(" && ");
                        break;
                    case CodeBinaryOperatorType.GreaterThan:
                        Write(" > ");
                        break;
                    case CodeBinaryOperatorType.GreaterThanOrEqual:
                        Write(" >= ");
                        break;
                    case CodeBinaryOperatorType.IdentityEquality:
                        Write(" == ");
                        break;
                    case CodeBinaryOperatorType.IdentityInequality:
                        Write(" != ");
                        break;
                    case CodeBinaryOperatorType.LessThan:
                        Write(" < ");
                        break;
                    case CodeBinaryOperatorType.LessThanOrEqual:
                        Write(" <= ");
                        break;
                    case CodeBinaryOperatorType.Modulus:
                        Write(" % ");
                        break;
                    case CodeBinaryOperatorType.Multiply:
                        Write(" * ");
                        break;
                    case CodeBinaryOperatorType.Subtract:
                        Write(" - ");
                        break;
                    case CodeBinaryOperatorType.ValueEquality:
                        Write(".eql? ");
                        break;
                    default:
                        throw new NotSupportedException();
                }
                Generate(expression.Right);
            }





            private string TypeToString(CodeTypeReference type)
            {
                string name = type.BaseType.Replace(".", "::");
                if (type.TypeArguments.Count > 0)
                {
                    StringBuilder sb = new StringBuilder(name);
                    sb.Append("[");
                    for (int i = 0; i < type.TypeArguments.Count; i++)
                    {
                        sb.Append(TypeToString(type.TypeArguments[i]));

                        if (i != type.TypeArguments.Count - 1)
                            sb.Append(",");
                    }
                    sb.Append("]");
                    name = sb.ToString();
                }

                return name;
            }
        }
    }
}
