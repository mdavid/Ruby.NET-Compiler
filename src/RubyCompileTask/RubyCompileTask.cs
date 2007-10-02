using System.Collections.Generic;
using Microsoft.Build.Utilities;
using Microsoft.Build.Framework;


namespace Ruby.NET
{
    public class RubyCompileTask : Task
    {
        public override bool Execute()
        {
            Ruby.Compiler.Compiler.log = this.Log;
            Ruby.Compiler.Compiler.InteropWarnings = true;

            try
            {
                List<string> args = new List<string>();

                args.Add("/target:" + TargetType);

                args.Add("/out:" + OutputAssembly);

                args.Add("/debug:" + DebugType);

                args.Add("/main:" + MainFile);

                foreach (ITaskItem source in sources)
                    args.Add(source.ItemSpec);

                foreach (ITaskItem assemblyReference in this.ReferencedAssemblies)
                    args.Add(assemblyReference.ItemSpec);

                Ruby.Compiler.Compiler.Process(args.ToArray());

                return !Log.HasLoggedErrors;
            }
            catch (System.Exception e)
            {
                Ruby.Compiler.Compiler.LogError("Internal compiler error: " + e.Message + "\n" + e.StackTrace); 
                return false;
            }
        }

        private ITaskItem[] sources;
        private ITaskItem[] referencedAssemblies = new ITaskItem[0];
        private string targetType;
        private string outputAssembly;
        private string debugType;
        private string mainFile;

        [Required]
        public ITaskItem[] Sources
        {
            get { return sources; }
            set { sources = value; }
        }

        public ITaskItem[] ReferencedAssemblies
        {
            get { return referencedAssemblies; }
            set
            {
                if (value != null)
                {
                    referencedAssemblies = value;
                }
                else
                {
                    referencedAssemblies = new ITaskItem[0];
                }
            }
        }

        [Required]
        public string MainFile
        {
            get { return mainFile; }
            set { mainFile = value; }
        }

        [Required]
        public string TargetType
        {
            get { return targetType; }
            set { targetType = value; }
        }

        [Required]
        public string OutputAssembly
        {
            get { return outputAssembly; }
            set { outputAssembly = value; }
        }

        [Required]
        public string DebugType
        {
            get { return debugType; }
            set { debugType = value; }
        }
    }
}