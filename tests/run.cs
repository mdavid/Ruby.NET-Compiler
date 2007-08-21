using System;
using System.IO;
using System.Diagnostics;
using System.Collections.Generic;

public class Test
{
    struct Result
    {
        public int Pass;
        public int Fail;

        public static Result operator +(Result a, Result b)
        {
            Result r = new Result();
            r.Pass = a.Pass + b.Pass;
            r.Fail = a.Fail + b.Fail;
            return r;
        }

        public override string ToString()
        {
            return string.Format("{0}/{1}", Pass, Pass + Fail);
        }
    }

    interface ITestRunner : IDisposable
    {
        Result RunTests(IList<string> paths);
    }

    class CommandRunner : ITestRunner
    {
        public CommandRunner(string cmd, string log)
        {
            this.cmd = cmd;
            FileStream logfile = File.Create(log);
            this.log = new StreamWriter(logfile);
        }

        string cmd;
        StreamWriter log;

        public Result RunTests(IList<string> paths)
        {
            Result results = new Result();

            for (int i = 0; i < paths.Count; i += Environment.ProcessorCount)
            {
                List<Process> tasks = new List<Process>();

                for (int j = 0; j < Environment.ProcessorCount && i + j < paths.Count; j++)
                {
                    string file = Path.GetFileName(paths[i + j]);
                    string dir = Path.GetDirectoryName(paths[i + j]);
                    ProcessStartInfo psi = new ProcessStartInfo(cmd, file);
                    psi.UseShellExecute = false;
                    psi.RedirectStandardOutput = true;
                    psi.RedirectStandardError = true;
                    psi.WorkingDirectory = dir;
                    tasks.Add(Process.Start(psi));
                }

                foreach (Process process in tasks)
                {
                    process.WaitForExit();

                    if (process.ExitCode == 0)
                    {
                        Console.Write(".");
                        results.Pass++;
                    }
                    else
                    {
                        Console.Write("x");
                        results.Fail++;

                        log.WriteLine(process.StartInfo.Arguments);
                        log.WriteLine("  Out:");

                        string line;
                        while (null != (line = process.StandardOutput.ReadLine()))
                        {
                            log.Write("    ");
                            log.WriteLine(line);
                        }

                        log.WriteLine("  Error:");
                        while (null != (line = process.StandardError.ReadLine()))
                        {
                            log.Write("    ");
                            log.WriteLine(line);
                        }

                        log.Flush();
                    }
                }
            }

            return results;
        }

        public void Dispose()
        {
            log.Dispose();
        }
    }

    static int Main()
    {
        string ruby = Path.GetFullPath("../bin/Ruby.exe");
        string log = Path.GetFullPath("test-log.txt");

        using (ITestRunner runner = new CommandRunner(ruby, log))
        {
            Result r = RunTests(Path.GetFullPath("."), "", runner);
            Console.WriteLine("Total: {0}", r);
            return r.Fail;
        }
    }

    static Result RunTests(string path, string indent, ITestRunner runner)
    {
        Result r = new Result();

        Console.Write(indent);
        Console.WriteLine("{0}", Path.GetFileName(path));

        foreach (string child in Directory.GetDirectories(path))
            r += RunTests(child, indent + " ", runner);

        Console.Write(indent);
        Console.Write(" ");
        r += runner.RunTests(Directory.GetFiles(path, "*.rb"));
        Console.WriteLine(r);
        return r;
    }
}
