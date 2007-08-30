using Ruby;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using Microsoft.Build.Utilities;
using Ruby.Runtime;


namespace Ruby.Compiler
{
    public class Compiler
    {
        public static string RUBY_RUNTIME = "Ruby.NET.Runtime.dll";
        public static PERWAPI.ReferenceScope peRubyRuntime = null;
        public static PERWAPI.ReferenceScope mscorlib = null;

        public static void Process(string[] args)
        {
            Process(args, null);
        }

        public static bool RubyCompilerRunning()
        {
            return Assembly.GetEntryAssembly().FullName.Contains("RubyCompiler");
        }

        public static List<string> GetPath()
        {
            List<string> pathSet = new List<string>();

            string CLRpath = Path.GetDirectoryName(Assembly.GetAssembly(typeof(string)).Location);
            pathSet.Add(CLRpath);
            string assemblyLocation = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            pathSet.Add(assemblyLocation);

            return pathSet;
        }

        public static System.IO.FileInfo FindFile(string filename, List<string> path)
        {
            if (System.IO.File.Exists(filename))
                return new System.IO.FileInfo(filename);

            foreach (string dir in path)
            {
                string absolutePath = dir + "/" + filename;
                if (System.IO.File.Exists(absolutePath))
                    return new System.IO.FileInfo(absolutePath);
            }
            throw new LoadError("File not found: " + filename).raise(null);
            return null;
        }

        public static void Process(string[] args, TaskLoggingHelper log)
        {
            List<string> inputFiles = new List<string>();

            string outFile = null;
            string debug = "full";
            string main = null;
            string target = null;

            foreach (string arg in args)
            {
                if (arg.StartsWith("/"))
                {
                    if (arg.StartsWith("/out:", StringComparison.InvariantCultureIgnoreCase))
                        outFile = arg.Substring(5);
                    else if (arg.StartsWith("/target:", StringComparison.InvariantCultureIgnoreCase))
                        target = arg.Substring(8).ToLowerInvariant();
                    else if (arg.StartsWith("/t:", StringComparison.InvariantCultureIgnoreCase))
                        target = arg.Substring(3).ToLowerInvariant();
                    else if (arg.StartsWith("/main:", StringComparison.InvariantCultureIgnoreCase))
                        main = arg.Substring(6);
                    else if (arg.StartsWith("/debug:", StringComparison.InvariantCultureIgnoreCase))
                        debug = arg.Substring(7).ToLowerInvariant();
                    else if (arg.StartsWith("/debug", StringComparison.InvariantCultureIgnoreCase))
                        debug = arg.Substring(6).ToLowerInvariant();
                    else if (arg.StartsWith("/help", StringComparison.InvariantCultureIgnoreCase) || arg.StartsWith("/?", StringComparison.InvariantCultureIgnoreCase))
                    {
                        Console.WriteLine("Usage RubyCompiler.exe {<options>|<sourcefiles>}");
                        Console.WriteLine();
                        Console.WriteLine("Options:");
                        Console.WriteLine(" /out:<file>                   Specify output file name (default: base name of file with main class or first file)");
                        Console.WriteLine(" /target:exe                   Build a console executable (default) (Short form: /t:exe)");
                        Console.WriteLine(" /target:library               Build a library (Short form: /t:library)");
                        Console.WriteLine(" /main:<file>                  Specify the main source file (default: the first source file)");
                        Console.WriteLine(" /debug[+|-]                   Emit debugging information");
                        Console.WriteLine(" /debug:{full|none}            Specify debugging type ('full' is default, and enables attaching a debugger to a running program)");
                        Console.WriteLine(" /help                         Display this usage message (Short form: /?)");
                        return;
                    }
                    else
                        throw new System.Exception("Unrecognized option " + arg);
                }
                else
                    inputFiles.Add(arg);
            }

            if (inputFiles.Count == 0)
                throw new System.Exception("No inputs specified");

            if (outFile == null)
                outFile = inputFiles[0]; // defaults to first source file name

            System.IO.FileInfo file = new System.IO.FileInfo(outFile);

            if (target == null)
                if (file.Extension == ".exe" || file.Extension == ".dll")
                    target = file.Extension.Substring(1);
                else
                    target = "exe"; // default

            // Remove any file extension
            outFile = file.Directory + @"\" + file.Name.Substring(0, file.Name.Length - file.Extension.Length);

            if (target == "exe")
                outFile += ".exe";
            else if (target == "library")
                outFile += ".dll";
            else
                throw new System.Exception("Invalid target type for /target: must specify 'exe' or 'library'");

            List<Ruby.Compiler.AST.SOURCEFILE> files = new List<Ruby.Compiler.AST.SOURCEFILE>();
            List<PERWAPI.ReferenceScope> peFiles = new List<PERWAPI.ReferenceScope>();
            peRubyRuntime = PERWAPI.PEFile.ReadExportedInterface(FindFile(RUBY_RUNTIME, GetPath()).FullName);
            mscorlib = PERWAPI.PEFile.ReadExportedInterface(FindFile("mscorlib.dll", GetPath()).FullName);

            if (main != null)
            {
                List<string> options;
                files.Add(File.load_file(null, main, false, out options, log));
            }

            foreach (string input in inputFiles)
            {
                System.IO.FileInfo inputFile = new System.IO.FileInfo(input);
                if (inputFile.Extension == ".dll")
                {
                    System.IO.FileInfo location = FindFile(input, GetPath());
                    if (location == null)
                        throw new LoadError("File not found: " + input).raise(null);
                    PERWAPI.ReferenceScope reference = PERWAPI.PEFile.ReadExportedInterface(location.FullName);
                    peFiles.Add(reference);
                }
                else
                {
                    List<string> options;
                    if (input != main)
                    {
                        files.Add(File.load_file(null, input, false, out options, log));
                    }
                }
            }

            if (main == null)
                main = inputFiles[0];

            List<KeyValuePair<string,object>> name = new List<KeyValuePair<string,object>>();
            name.Add(new KeyValuePair<string, object>("rb_progname", main));

            PERWAPI.PEFile assembly = Ruby.Compiler.AST.SOURCEFILE.GenerateCode(files, peFiles, outFile, name);
            assembly.MakeDebuggable(true, true);
            assembly.WritePEFile(debug!="none");
        }
    }
}
