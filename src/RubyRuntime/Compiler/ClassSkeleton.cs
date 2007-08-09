using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using PERWAPI;

namespace Ruby.Compiler
{
    internal class ClassSkeletonPostPass
    {
        internal ClassSkeleton subClass;
        internal PERWAPI.ClassDef subClassDef;
        internal AST.Node superClassNode;

        internal ClassSkeletonPostPass(ClassSkeleton subClass, PERWAPI.ClassDef subClassDef, AST.Node superClassNode)
        {
            this.subClass = subClass;
            this.subClassDef = subClassDef;
            this.superClassNode = superClassNode;
        }
    }

    internal class ClassSkeleton
    {
        internal enum ClassSkeletonType { Class, Module };
        internal string name;
        internal ClassSkeleton super = null;
        internal ClassSkeleton lexicalParent = null;
        internal Dictionary<string, ClassSkeleton> nestedClasses = new Dictionary<string, ClassSkeleton>();
        internal Dictionary<string, ClassSkeleton> includedModules = new Dictionary<string, ClassSkeleton>();
        internal PERWAPI.Class perwapiClass = null;
        internal PERWAPI.Class allocator = null;
        internal PERWAPI.MethodDef initMethod = null;

        internal ClassSkeleton(string name, PERWAPI.Class perwapiClass)
        {
            this.name = name;
            this.perwapiClass = perwapiClass;
        }

        internal ClassSkeleton FindClass(string className)
        {
            if (nestedClasses.ContainsKey(className))
                return nestedClasses[className];

            ClassSkeleton scope = lexicalParent;

            while (scope != null)
            {
                if (scope.name.Equals(className))
                    return scope;

                if (scope.nestedClasses.ContainsKey(className))
                    return scope.nestedClasses[className];

                scope = scope.lexicalParent;
            }

            return null;
        }

        internal ClassSkeleton FindClass(AST.Node node)
        {
            if (!(node is AST.CONST))
                return null;

            AST.CONST constName = (AST.CONST)node;
            ClassSkeleton head = FindClass(constName.vid);

            if (head == null)
                return null;

            if (constName.scope == null)
                return head;

            // FIXME: need to search inheritance hierarchy as well
            AST.Node scope = constName.scope;

            while (scope != null)
            {
                if (!(scope is AST.CONST))
                    return null;

                AST.CONST currentConst = (AST.CONST)scope;
                head = head.FindClass(currentConst.vid);

                if (head == null)
                    return null;

                if (currentConst.scope == null)
                    return head;

                scope = currentConst.scope;
            }

            return null;
        }

        internal static PERWAPI.Class FindPERWAPIClass(ClassSkeleton skel, AST.Node node, List<PERWAPI.ReferenceScope> peFiles)
        {
            if (skel != null)
            {
                ClassSkeleton foundClass = skel.FindClass(node);
                if (foundClass != null && foundClass.perwapiClass != null)
                    return foundClass.perwapiClass;
            }

            PERWAPI.ClassRef perwapiClass = GetClassFromPEFile(node, peFiles, "");

            if (perwapiClass != null)
                return perwapiClass;

            List<PERWAPI.ReferenceScope> rubyRuntimeList = new List<PERWAPI.ReferenceScope>();

            if (Ruby.Compiler.Compiler.peRubyRuntime == null)
                Ruby.Compiler.Compiler.peRubyRuntime = PERWAPI.PEFile.ReadExportedInterface(Ruby.Compiler.Compiler.FindFile(Ruby.Compiler.Compiler.RUBY_RUNTIME, Ruby.Compiler.Compiler.GetPath()).FullName);

            if (Ruby.Compiler.Compiler.peRubyRuntime != null)
            {
                rubyRuntimeList.Add(Ruby.Compiler.Compiler.peRubyRuntime);
                return GetClassFromPEFile(node, rubyRuntimeList, "Ruby");
            }

            return null;
        }

        internal static List<string> ConstNodeToClassList(AST.Node node)
        {
            List<string> classList = new List<string>();

            while (node != null)
            {
                if (!(node is AST.CONST))
                    return null;

                AST.CONST constNode = (AST.CONST)node;

                classList.Add(constNode.vid);
                node = constNode.scope;
            }

            classList.Reverse();
            return classList;
        }

        internal static PERWAPI.ClassRef GetClassFromPEFile(AST.Node node, List<PERWAPI.ReferenceScope> peFiles, string nsPrefix)
        {
            if (!(node is AST.CONST))
                return null;

            AST.CONST constName = (AST.CONST)node;
            string head = constName.vid;
            PERWAPI.ClassRef peClass = null;
            AST.Node scope = node;
            string peNamespace = "", peNamespaceOrig = "";
            List<string> classList = ConstNodeToClassList(node);
            string className = classList[classList.Count - 1];

            for (int i = 0; i < classList.Count - 1; i++)
            {
                if (peNamespace.Length > 0)
                    peNamespace += ".";

                peNamespace += classList[i];
            }

            peNamespaceOrig = peNamespace;

            foreach (PERWAPI.ReferenceScope peFile in peFiles)
            {
                peNamespace = peNamespaceOrig;
                if (peNamespace.Length > 0 && nsPrefix.Length > 0)
                    peNamespace = "." + peNamespace;
                peNamespace = nsPrefix + peNamespace;

                if (peNamespace.Length > 0)
                    peClass = peFile.GetClass(peNamespace, className);
                else
                    peClass = peFile.GetClass(className);

                if (peClass != null)
                {
                    return peClass;
                }

                // try nested classes instead
                // FIXME: need to try combination of namespaces and nested classes
                peNamespace = "";
                PERWAPI.ClassRef nestedClassRef = null;
                bool flag = true;
                for (int i = 0; i < classList.Count && flag; i++)
                {
                    if (i == 0)
                    {
                        nestedClassRef = peFile.GetClass(classList[i]);
                        if (nestedClassRef == null)
                        {
                            // must be a namespace
                            peNamespace = classList[i];
                        }
                    }
                    else
                    {
                        if (nestedClassRef == null)
                        {
                            // flag = false;
                            nestedClassRef = peFile.GetClass(peNamespace, classList[i]);
                            if (nestedClassRef == null)
                            {
                                peNamespace += "." + classList[i];
                            }
                        }
                        else
                        {
                            nestedClassRef = nestedClassRef.GetNestedClass(classList[i]);
                            if (i == (classList.Count - 1) && nestedClassRef != null)
                                return nestedClassRef;
                        }
                    }
                }
            }

            return null;             
        }
            
    }
}
