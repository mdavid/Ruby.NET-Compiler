using System;
using System.Collections.Generic;
using System.Text;
using System.CodeDom;
using System.CodeDom.Compiler;

namespace Ruby.CodeDom
{
    public class RubyCodeGenerator : ICodeGenerator
    {
        public string CreateEscapedIdentifier(string value)
        {
            throw new NotImplementedException();
        }

        public string CreateValidIdentifier(string value)
        {
            throw new NotImplementedException();
        }

        public void GenerateCodeFromCompileUnit(CodeCompileUnit e, System.IO.TextWriter w, CodeGeneratorOptions o)
        {
            using (InternalCodeGenerator gen = new InternalCodeGenerator(w))
                gen.Generate(e);
        }

        public void GenerateCodeFromExpression(CodeExpression e, System.IO.TextWriter w, CodeGeneratorOptions o)
        {
            using (InternalCodeGenerator gen = new InternalCodeGenerator(w))
                gen.Generate(e);
        }

        public void GenerateCodeFromNamespace(CodeNamespace e, System.IO.TextWriter w, CodeGeneratorOptions o)
        {
            using (InternalCodeGenerator gen = new InternalCodeGenerator(w))
                gen.Generate(e);
        }

        public void GenerateCodeFromStatement(CodeStatement e, System.IO.TextWriter w, CodeGeneratorOptions o)
        {
            using (InternalCodeGenerator gen = new InternalCodeGenerator(w))
                gen.Generate(e);
        }

        public void GenerateCodeFromType(CodeTypeDeclaration e, System.IO.TextWriter w, CodeGeneratorOptions o)
        {
            using (InternalCodeGenerator gen = new InternalCodeGenerator(w))
                gen.Generate(e);
        }

        public string GetTypeOutput(CodeTypeReference type)
        {
            throw new NotImplementedException();
        }

        public bool IsValidIdentifier(string value)
        {
            throw new NotImplementedException();
        }

        public bool Supports(GeneratorSupport supports)
        {
            throw new NotImplementedException();
        }

        public void ValidateIdentifier(string value)
        {
            throw new NotImplementedException();
        }

        private class InternalCodeGenerator : IndentedTextWriter
        {
            public InternalCodeGenerator(System.IO.TextWriter tw)
                : base(tw)
            {
            }

            Stack<List<string>> imports = new Stack<List<string>>();

            public void Generate(CodeCompileUnit unit)
            {
                foreach (string aref in unit.ReferencedAssemblies)
                    WriteLine("require '{0}'", aref);

                if (unit.ReferencedAssemblies.Count > 0)
                    WriteLine();

                foreach (CodeNamespace ns in unit.Namespaces)
                {
                    Generate(ns);
                    WriteLine();
                }
            }

            public void Generate(CodeNamespace ns)
            {
                imports.Push(new List<string>());
                foreach (CodeNamespaceImport import in ns.Imports)
                {
                    string ns_import = import.Namespace.Replace(".", "::");
                    imports.Peek().Add(ns_import + "::");

                    // this should perhaps be included inside the module
                    // however that has other consequences
                    WriteLine("include " + ns_import);
                }

                if (ns.Imports.Count > 0)
                    WriteLine();

                if (!string.IsNullOrEmpty(ns.Name))
                {
                    WriteLine("module " + ns.Name);
                    Indent++;
                }

                for (int i = 0; i < ns.Types.Count; i++)
                {
                    Generate(ns.Types[i]);
                    if (i != ns.Types.Count - 1)
                        WriteLine();
                }

                if (!string.IsNullOrEmpty(ns.Name))
                {
                    Indent--;
                    WriteLine("end");
                }

                imports.Pop();
            }

            public void Generate(CodeTypeDeclaration type)
            {
                Generate(type.Comments);

                string inherits = "";
                if (type.BaseTypes.Count > 1)
                    throw new NotSupportedException();
                else if (type.BaseTypes.Count == 1)
                    inherits = " < " + TypeToString(type.BaseTypes[0]);

                WriteLine("class {0}{1}", type.Name, inherits);
                {
                    Indent++;

                    for (int i = 0; i < type.Members.Count; i++)
                    {
                        Generate(type.Members[i]);

                        if (i != type.Members.Count - 1)
                            WriteLine();
                    }

                    Indent--;
                }
                WriteLine("end");
            }

            #region Members
            public void Generate(CodeTypeMember member)
            {
                if (member is CodeMemberEvent)
                    Generate((CodeMemberEvent)member);
                else if (member is CodeMemberField)
                    Generate((CodeMemberField)member);
                else if (member is CodeMemberMethod)
                    Generate((CodeMemberMethod)member);
                else if (member is CodeMemberProperty)
                    Generate((CodeMemberProperty)member);
                else if (member is CodeSnippetTypeMember)
                    Generate((CodeSnippetTypeMember)member);
                else if (member is CodeTypeDeclaration)
                    Generate((CodeTypeDeclaration)member);
                else
                    throw new NotSupportedException();
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

            public void Generate(CodeSnippetTypeMember member)
            {
                Generate(member.Comments);
                WriteLine(member.Text);
            }

            public void Generate(CodeTypeConstructor member)
            {
                Generate(member.Comments);
                foreach (CodeStatement statement in member.Statements)
                    Generate(statement);
            }

            public void Generate(CodeMemberEvent member)
            {
                throw new NotImplementedException();
            }

            public void Generate(CodeMemberField member)
            {
                if (member.InitExpression != null)
                    throw new NotImplementedException();

                string self = "";
                if (MemberAttributes.Static == (member.Attributes & MemberAttributes.Static))
                    self = "self.";

                WriteLine("def {0}{1}= value", self, member.Name);
                {
                    Indent++;
                    WriteLine("@{0} = value",member.Name);
                    Indent--;
                }
                WriteLine("end");
                WriteLine();
                WriteLine("def {0}{1}", self, member.Name);
                {
                    Indent++;
                    WriteLine("@{0}", member.Name);
                    Indent--;
                }
                WriteLine("end");
            }

            public void Generate(CodeMemberProperty member)
            {
                if (member.Parameters.Count > 0)
                    throw new NotSupportedException();

                string self = "";
                if (MemberAttributes.Static == (member.Attributes & MemberAttributes.Static))
                    self = "self.";

                if (member.HasGet)
                {
                    WriteLine("def {0}{1}", self, member.Name);
                    {
                        Indent++;
                        Generate(member.GetStatements);
                        Indent--;
                    }
                    WriteLine("end");
                }

                if (member.HasSet)
                {
                    if (member.HasGet)
                        WriteLine();

                    WriteLine("def {0}{1}= value", self, member.Name);
                    {
                        Indent++;
                        Generate(member.SetStatements);
                        Indent--;
                    }
                    WriteLine("end");
                }
            }
            #endregion

            #region Collections
            public void Generate(CodeStatementCollection statements)
            {
                foreach (CodeStatement statement in statements)
                    Generate(statement);
            }

            public void Generate(CodeCommentStatementCollection comments)
            {
                foreach (CodeCommentStatement comment in comments)
                    Generate(comment);
            }
            #endregion

            #region Statements
            public void Generate(CodeStatement statement)
            {
                if (statement is CodeAssignStatement)
                    Generate((CodeAssignStatement)statement);
                else if (statement is CodeAttachEventStatement)
                    Generate((CodeAttachEventStatement)statement);
                else if (statement is CodeCommentStatement)
                    Generate((CodeCommentStatement)statement);
                else if (statement is CodeConditionStatement)
                    Generate((CodeConditionStatement)statement);
                else if (statement is CodeExpressionStatement)
                    Generate((CodeExpressionStatement)statement);
                else if (statement is CodeGotoStatement)
                    Generate((CodeGotoStatement)statement);
                else if (statement is CodeIterationStatement)
                    Generate((CodeIterationStatement)statement);
                else if (statement is CodeLabeledStatement)
                    Generate((CodeLabeledStatement)statement);
                else if (statement is CodeMethodReturnStatement)
                    Generate((CodeMethodReturnStatement)statement);
                else if (statement is CodeRemoveEventStatement)
                    Generate((CodeRemoveEventStatement)statement);
                else if (statement is CodeSnippetStatement)
                    Generate((CodeSnippetStatement)statement);
                else if (statement is CodeThrowExceptionStatement)
                    Generate((CodeThrowExceptionStatement)statement);
                else if (statement is CodeTryCatchFinallyStatement)
                    Generate((CodeTryCatchFinallyStatement)statement);
                else if (statement is CodeVariableDeclarationStatement)
                    Generate((CodeVariableDeclarationStatement)statement);
                else
                    throw new NotSupportedException();
            }

            public void Generate(CodeAssignStatement statement)
            {
                Generate(statement.Left);
                Write(" = ");
                Generate(statement.Right);
                WriteLine();
            }

            public void Generate(CodeAttachEventStatement statement)
            {
                Generate(statement.Event.TargetObject);
                Write(".add_{0}(", statement.Event.EventName);
                Generate(statement.Listener);
                WriteLine(")");
            }

            public void Generate(CodeCommentStatement statement)
            {
                WriteLine("# " + statement.Comment.Text);
            }

            public void Generate(CodeConditionStatement statement)
            {
                Write("if "); Generate(statement.Condition); WriteLine(" then");
                {
                    Indent++;
                    Generate(statement.TrueStatements);
                    Indent--;
                }
                if (statement.FalseStatements.Count > 0)
                {
                    WriteLine("else");
                    Indent++;
                    Generate(statement.FalseStatements);
                    Indent--;
                }
                WriteLine("end");
            }

            public void Generate(CodeExpressionStatement statement)
            {
                Generate(statement.Expression);
                WriteLine();
            }

            public void Generate(CodeGotoStatement statement)
            {
                throw new NotImplementedException();
            }

            public void Generate(CodeIterationStatement statement)
            {
                WriteLine("begin");
                {
                    Indent++;
                    Generate(statement.InitStatement);
                    Write("while ("); Generate(statement.TestExpression); WriteLine(")");
                    {
                        Indent++;
                        Generate(statement.Statements);
                        Generate(statement.IncrementStatement);
                        Indent--;
                    }
                    Indent--;
                }
                WriteLine("end");
            }

            public void Generate(CodeLabeledStatement statement)
            {
                throw new NotImplementedException();
            }

            public void Generate(CodeMethodReturnStatement statement)
            {
                Write("return ");
                Generate(statement.Expression);
                WriteLine();
            }

            public void Generate(CodeRemoveEventStatement statement)
            {
                throw new NotImplementedException();
            }

            public void Generate(CodeSnippetStatement statement)
            {
                WriteLine(statement.Value);
            }

            public void Generate(CodeThrowExceptionStatement statement)
            {
                Write("raise ");
                Generate(statement.ToThrow);
                WriteLine();
            }

            public void Generate(CodeTryCatchFinallyStatement statement)
            {
                WriteLine("begin");
                {
                    Indent++;
                    Generate(statement.TryStatements);
                    Indent--;
                }

                foreach (CodeCatchClause cclause in statement.CatchClauses)
                {
                    WriteLine("rescue");
                    {
                        Indent++;
                        Generate(cclause.Statements);
                        Indent--;
                    }
                }

                if (statement.FinallyStatements.Count > 0)
                {
                    WriteLine("ensure");
                    Indent++;
                    Generate(statement.FinallyStatements);
                    Indent--;
                }

                WriteLine("end");
            }

            public void Generate(CodeVariableDeclarationStatement statement)
            {
                Write("{0} = ", statement.Name);

                if (statement.InitExpression == null)
                    Write("nil");
                else
                    Generate(statement.InitExpression);

                WriteLine();
                throw new NotImplementedException();
            }
            #endregion

            #region Expressions
            public void Generate(CodeExpression expression)
            {
                if (expression is CodeArgumentReferenceExpression)
                    Generate((CodeArgumentReferenceExpression)expression);
                else if (expression is CodeArrayCreateExpression)
                    Generate((CodeArrayCreateExpression)expression);
                else if (expression is CodeArrayIndexerExpression)
                    Generate((CodeArrayIndexerExpression)expression);
                else if (expression is CodeBaseReferenceExpression)
                    Generate((CodeBaseReferenceExpression)expression);
                else if (expression is CodeBinaryOperatorExpression)
                    Generate((CodeBinaryOperatorExpression)expression);
                else if (expression is CodeCastExpression)
                    Generate((CodeCastExpression)expression);
                else if (expression is CodeDefaultValueExpression)
                    Generate((CodeDefaultValueExpression)expression);
                else if (expression is CodeDelegateCreateExpression)
                    Generate((CodeDelegateCreateExpression)expression);
                else if (expression is CodeDelegateInvokeExpression)
                    Generate((CodeDelegateInvokeExpression)expression);
                else if (expression is CodeDirectionExpression)
                    Generate((CodeDirectionExpression)expression);
                else if (expression is CodeEventReferenceExpression)
                    Generate((CodeEventReferenceExpression)expression);
                else if (expression is CodeFieldReferenceExpression)
                    Generate((CodeFieldReferenceExpression)expression);
                else if (expression is CodeIndexerExpression)
                    Generate((CodeIndexerExpression)expression);
                else if (expression is CodeMethodInvokeExpression)
                    Generate((CodeMethodInvokeExpression)expression);
                else if (expression is CodeMethodReferenceExpression)
                    Generate((CodeMethodReferenceExpression)expression);
                else if (expression is CodeObjectCreateExpression)
                    Generate((CodeObjectCreateExpression)expression);
                else if (expression is CodeParameterDeclarationExpression)
                    Generate((CodeParameterDeclarationExpression)expression);
                else if (expression is CodePrimitiveExpression)
                    Generate((CodePrimitiveExpression)expression);
                else if (expression is CodePropertyReferenceExpression)
                    Generate((CodePropertyReferenceExpression)expression);
                else if (expression is CodePropertySetValueReferenceExpression)
                    Generate((CodePropertySetValueReferenceExpression)expression);
                else if (expression is CodeSnippetExpression)
                    Generate((CodeSnippetExpression)expression);
                else if (expression is CodeThisReferenceExpression)
                    Generate((CodeThisReferenceExpression)expression);
                else if (expression is CodeTypeOfExpression)
                    Generate((CodeTypeOfExpression)expression);
                else if (expression is CodeTypeReferenceExpression)
                    Generate((CodeTypeReferenceExpression)expression);
                else if (expression is CodeVariableReferenceExpression)
                    Generate((CodeVariableReferenceExpression)expression);
                else
                    throw new NotSupportedException();
            }

            public void Generate(CodeArgumentReferenceExpression expression)
            {
                Write(expression.ParameterName);
            }

            public void Generate(CodeArrayCreateExpression expression)
            {
                throw new NotImplementedException();
            }

            public void Generate(CodeArrayIndexerExpression expression)
            {
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

            public void Generate(CodeBaseReferenceExpression expression)
            {
                Write("super");
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

            public void Generate(CodeCastExpression expression)
            {
                Generate(expression.Expression);
            }

            public void Generate(CodeDefaultValueExpression expression)
            {
                throw new NotSupportedException();
            }

            public void Generate(CodeDelegateCreateExpression expression)
            {
                // TODO: evaluate "TargetObject" before we build the proc
                Write("{0}.new {{ |*args| ", TypeToString(expression.DelegateType));
                Generate(expression.TargetObject);
                Write(".{0}(*args)", expression.MethodName);
                Write("}");
            }

            public void Generate(CodeDelegateInvokeExpression expression)
            {
                Generate(expression.TargetObject);
                Write(".Invoke");
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

            public void Generate(CodeDirectionExpression expression)
            {
                throw new NotSupportedException();
            }

            public void Generate(CodeEventReferenceExpression expression)
            {
                Generate(expression.TargetObject);
                Write(".{0}", expression.EventName);
            }

            public void Generate(CodeFieldReferenceExpression expression)
            {
                Generate(expression.TargetObject);
                Write(".{0}", expression.FieldName);
            }

            public void Generate(CodeIndexerExpression expression)
            {
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
                Generate(expression.TargetObject);
                Write(".{0}", expression.MethodName);
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

            public void Generate(CodeParameterDeclarationExpression expression)
            {
                Write(expression.Name);
            }

            public void Generate(CodePrimitiveExpression expression)
            {
                if (expression.Value is string)
                    Write("\"{0}\"", expression.Value);
                else
                    Write(expression.Value);
            }

            public void Generate(CodePropertyReferenceExpression expression)
            {
                Generate(expression.TargetObject);
                Write(".{0}", expression.PropertyName);
            }

            public void Generate(CodePropertySetValueReferenceExpression expression)
            {
                Write("value");
            }

            public void Generate(CodeSnippetExpression expression)
            {
                Write(expression.Value);
            }

            public void Generate(CodeThisReferenceExpression expression)
            {
                Write("self");
            }

            public void Generate(CodeTypeOfExpression expression)
            {
                Write(TypeToString(expression.Type));
            }

            public void Generate(CodeTypeReferenceExpression expression)
            {
                Write(TypeToString(expression.Type));
            }

            public void Generate(CodeVariableReferenceExpression expression)
            {
                Write(expression.VariableName);
            }
            #endregion

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
                string prefix = "";

                foreach (List<string> scope in imports)
                    foreach (string import in scope)
                        if (import.Length > prefix.Length && name.StartsWith(import))
                            prefix = import;

                if (prefix == "")
                    return name;
                else
                    return name.Substring(prefix.Length);
            }
        }
    }
}
