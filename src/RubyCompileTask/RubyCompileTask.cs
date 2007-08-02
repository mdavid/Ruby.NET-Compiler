using System.Collections.Generic;
using Microsoft.Build.Utilities;
using Microsoft.Build.Framework;

namespace Ruby.NET
{
    public class RubyCompileTask : Task
    {
        public override bool Execute()
        {
            List<string> args = new List<string>();

            args.Add("/target:" + TargetType);

            args.Add("/out:" + OutputAssembly);

            args.Add("/debug:" + DebugType);

            args.Add("/main:" + MainFile);

            foreach (ITaskItem source in sources)
                args.Add(source.ItemSpec);

            try
            {
                Ruby.Compiler.Compiler.Process(args.ToArray(), Log);
                return !Log.HasLoggedErrors;
            }
            catch (System.Exception e)
            {
                Log.LogErrorFromException(e);
                return false;
            }
        }

        private ITaskItem[] sources;
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