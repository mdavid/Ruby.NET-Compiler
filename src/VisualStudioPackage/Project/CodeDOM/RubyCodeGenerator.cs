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
                            gen.GenerateMemberField(field);

                        gen.WriteLine();

                        // insert new InitializeComponent method
                        gen.GenerateMemberMethod(InitializeComponent);

                        // insert new methods
                        foreach (CodeMemberMethod method in new_methods)
                        {
                            gen.WriteLine();
                            gen.GenerateMemberMethod(method);
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


            public void GenerateMemberMethod(CodeMemberMethod member)
            {
                GenerateCommentStatementCollection(member.Comments);

                if (member is CodeConstructor)
                    member.Name = "initialize";

                Write("def {0}", member.Name);
                Write("(");
                for (int i = 0; i < member.Parameters.Count; i++)
                {
                    Write(member.Parameters[i].Name);
                    if (i != member.Parameters.Count - 1)
                        Write(",");
                }
                WriteLine(")");

                Indent++;
                foreach (CodeStatement statement in member.Statements)
                    GenerateStatement(statement);
                Indent--;

                if (member.Statements.Count == 0)
                    WriteLine();

                WriteLine("end");
            }


            public void GenerateMemberField(CodeMemberField member)
            {
                WriteLine("attr_accessor :{0}", member.Name);
            }


            public void GenerateStatement(CodeStatement statement)
            {
                if (statement is CodeAssignStatement)
                    GenerateAssignStatement((CodeAssignStatement)statement);
                else if (statement is CodeAttachEventStatement)
                    GenerateAttachEventStatement((CodeAttachEventStatement)statement);
                else if (statement is CodeCommentStatement)
                    GenerateCommentStatement((CodeCommentStatement)statement);
                else if (statement is CodeExpressionStatement)
                    GenerateExpressionStatement((CodeExpressionStatement)statement);
                else if (statement is CodeVariableDeclarationStatement)
                    GenerateVariableDeclarationStatement((CodeVariableDeclarationStatement)statement);
                else
                    throw new NotSupportedException();
            }


            public void GenerateAttachEventStatement(CodeAttachEventStatement statement)
            {
                GenerateExpression(statement.Event.TargetObject);
                Write(".add_{0}(", statement.Event.EventName);
                GenerateExpression(statement.Listener);
                WriteLine(")");
            }


            public void GenerateCommentStatementCollection(CodeCommentStatementCollection comments)
            {
                foreach (CodeCommentStatement comment in comments)
                    GenerateCommentStatement(comment);
            }


            public void GenerateCommentStatement(CodeCommentStatement statement)
            {
                WriteLine("# " + statement.Comment.Text);
            }


            public void GenerateAssignStatement(CodeAssignStatement statement)
            {
                if (statement.Left is CodePropertyReferenceExpression)
                {
                    CodePropertyReferenceExpression prop = (CodePropertyReferenceExpression)statement.Left;
                    GenerateExpression(prop.TargetObject);
                    Write(".set_{0}(", prop.PropertyName);
                    GenerateExpression(statement.Right);
                    WriteLine(")");
                }
                else
                {
                    GenerateExpression(statement.Left);
                    Write(" = ");
                    GenerateExpression(statement.Right);
                    WriteLine();
                }
            }


            public void GenerateExpressionStatement(CodeExpressionStatement statement)
            {
                GenerateExpression(statement.Expression);
                WriteLine();
            }


            public void GenerateVariableDeclarationStatement(CodeVariableDeclarationStatement statement)
            {
                Write("{0} = Interop.VariableInitialization(", statement.Name);

                if (statement.InitExpression == null)
                    Write("nil");
                else
                    GenerateExpression(statement.InitExpression);
                
                Write(", ");
                
                GenerateTypeReference(statement.Type);

                WriteLine(")");
            }


            public void GenerateExpression(CodeExpression expression)
            {
                if (expression is CodeArrayIndexerExpression)
                    GenerateArrayIndexerExpression((CodeArrayIndexerExpression)expression);
                else if (expression is CodeBinaryOperatorExpression)
                    GenerateBinaryOperatorExpression((CodeBinaryOperatorExpression)expression);
                else if (expression is CodeCastExpression)
                    GenerateCastExpression((CodeCastExpression)expression);
                else if (expression is CodeDelegateCreateExpression)
                    GenerateDelegateCreateExpression((CodeDelegateCreateExpression)expression);
                else if (expression is CodeFieldReferenceExpression)
                    GenerateFieldReferenceExpression((CodeFieldReferenceExpression)expression);
                else if (expression is CodeMethodInvokeExpression)
                    GenerateMethodInvokeExpression((CodeMethodInvokeExpression)expression);
                else if (expression is CodeMethodReferenceExpression)
                    GenerateMethodReferenceExpression((CodeMethodReferenceExpression)expression);
                else if (expression is CodeObjectCreateExpression)
                    GenerateObjectCreateExpression((CodeObjectCreateExpression)expression);
                else if (expression is CodePrimitiveExpression)
                    GeneratePrimitiveExpression((CodePrimitiveExpression)expression);
                else if (expression is CodePropertyReferenceExpression)
                    GeneratePropertyReferenceExpression((CodePropertyReferenceExpression)expression);
                else if (expression is CodeThisReferenceExpression)
                    GenerateThisReferenceExpression((CodeThisReferenceExpression)expression);
                else if (expression is CodeTypeOfExpression)
                    GenerateTypeOfExpression((CodeTypeOfExpression)expression);
                else if (expression is CodeArrayCreateExpression)
                    GenerateArrayCreateExpression((CodeArrayCreateExpression)expression);
                else if (expression is CodeTypeReferenceExpression)
                    GenerateTypeReferenceExpression((CodeTypeReferenceExpression)expression);
                else
                    throw new NotSupportedException(expression.GetType().ToString());
            }


            public void GenerateArrayCreateExpression(CodeArrayCreateExpression expression)
            {
                Write("Interop.ArrayCreate(");
                GenerateTypeReference(expression.CreateType);
                Write(", ");
                if (expression.SizeExpression != null)
                    GenerateExpression(expression.SizeExpression);
                else
                    Write(expression.Size);
                Write(", [");
                for (int i=0; i<expression.Initializers.Count; i++)
                {
                    GenerateExpression(expression.Initializers[i]);
                    if (i < expression.Initializers.Count - 1)
                        Write(", ");
                }
                Write("]");
            }


            public void GenerateTypeOfExpression(CodeTypeOfExpression expression)
            {
                Write("Interop.TypeOf(");
                GenerateTypeReference(expression.Type);
                Write(")");
            }


            public void GenerateCastExpression(CodeCastExpression expression)
            {
                Write("Interop.Cast(");
                GenerateTypeReference(expression.TargetType);
                Write(", ");
                GenerateExpression(expression.Expression);
                Write(")");
            }


            public void GenerateTypeReferenceExpression(CodeTypeReferenceExpression expression)
            {
                Write("Interop.TypeReference(");
                GenerateTypeReference(expression.Type);
                Write(")");
            }


            public void GenerateDelegateCreateExpression(CodeDelegateCreateExpression expression)
            {
                GenerateTypeReference(expression.DelegateType);
                Write(".new { |*args| ");
                GenerateExpression(expression.TargetObject);
                Write(".{0}(*args)", expression.MethodName);
                Write("}");
            }


            public void GenerateFieldReferenceExpression(CodeFieldReferenceExpression expression)
            {
                if (expression.TargetObject is CodeThisReferenceExpression)
                {
                    Write("@{0}", expression.FieldName);
                }
                else
                {
                    GenerateExpression(expression.TargetObject);
                    Write(".{0}", expression.FieldName);
                }
            }


            public void GenerateMethodInvokeExpression(CodeMethodInvokeExpression expression)
            {
                GenerateMethodReferenceExpression(expression.Method);
                Write("(");
                for (int i = 0; i < expression.Parameters.Count; i++)
                {
                    GenerateExpression(expression.Parameters[i]);
                    if (i != expression.Parameters.Count - 1)
                        Write(", ");
                }
                Write(")");
            }


            public void GenerateMethodReferenceExpression(CodeMethodReferenceExpression expression)
            {
                GenerateExpression(expression.TargetObject);
                Write(".");
                Write(expression.MethodName);
            }


            public void GenerateObjectCreateExpression(CodeObjectCreateExpression expression)
            {
                GenerateTypeReference(expression.CreateType);
                Write(".new(");

                for (int i = 0; i < expression.Parameters.Count; i++)
                {
                    GenerateExpression(expression.Parameters[i]);
                    if (i != expression.Parameters.Count - 1)
                        Write(", ");
                }
                Write(")");
            }


            public void GeneratePrimitiveExpression(CodePrimitiveExpression expression)
            {
                if (expression.Value is string)
                    Write("'{0}'", expression.Value);
                else if (expression.Value is bool)
                    Write((bool)expression.Value ? "true" : "false");
                else
                    Write(expression.Value);
            }


            public void GeneratePropertyReferenceExpression(CodePropertyReferenceExpression expression)
            {
                GenerateExpression(expression.TargetObject);
                Write(".get_{0}()", expression.PropertyName);
            }


            public void GenerateThisReferenceExpression(CodeThisReferenceExpression expression)
            {
                Write("self");
            }


            public void GenerateArrayIndexerExpression(CodeArrayIndexerExpression expression)
            {
                GenerateExpression(expression.TargetObject);
                Write("[");
                for (int i = 0; i < expression.Indices.Count; i++)
                {
                    GenerateExpression(expression.Indices[i]);
                    if (i != expression.Indices.Count - 1)
                        Write(", ");
                }
                Write("]");
            }


            public void GenerateBinaryOperatorExpression(CodeBinaryOperatorExpression expression)
            {
                GenerateExpression(expression.Left);
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
                GenerateExpression(expression.Right);
            }


            public void GenerateTypeReference(CodeTypeReference type)
            {
                if (type.ArrayRank > 0)
                    throw new NotImplementedException("Array Type References");
                else
                {
                    Write(type.BaseType.Replace(".", "::"));

                    if (type.TypeArguments.Count > 0)
                    {
                        Write("[");
                        for (int i = 0; i < type.TypeArguments.Count; i++)
                        {
                            GenerateTypeReference(type.TypeArguments[i]);
                            if (i != type.TypeArguments.Count - 1)
                                Write(", ");
                        }
                        Write("]");
                    }
                }
            }
        }
    }
}
