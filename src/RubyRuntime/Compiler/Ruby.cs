/**********************************************************************

  Ruby.NET Compiler
  Originally developed at Queensland University of Technology
 
  Some sections of this C# code mirror the structure of the C code in the
  Ruby 1.8.2 Interpreter Copyright (C) 1993-2003 Yukihiro Matsumoto, et.al.
  
**********************************************************************/

using Ruby.Runtime;
using Ruby;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using PERWAPI;
using System.Globalization;


namespace Ruby.Compiler
{
    public class RubyEntry
    {
        private static bool sflag = false;
        private static bool xflag = false;

        private static bool do_check = false;
        private static bool do_loop = false;
        private static bool do_line = false;
        private static bool do_split = false;
        private static bool do_print = false;

        private static AST.SOURCEFILE tree = null;

        private static string script;
        private static List<string> rb_argv;


        public static void Process(string[] args)
        {
            if (process_options())
            {
                PEFile pefile = tree.GenerateCode(script, ".exe", runtime_options);
                AST.SOURCEFILE.ExecuteMain(pefile, rb_argv.ToArray());
            }
        }


        private static bool process_options()
        {
            string[] args = Options.rb_w32_cmdvector(System.Environment.CommandLine);

            bool OK = proc_options(1, args);

            if (do_check && OK)
            {
                System.Console.WriteLine("Syntax OK");
                return false;
            }

            if (do_print)
                rb_parser_append_print();

            if (do_loop)
                rb_parser_while_loop(do_line, do_split);

            return OK;
        }

        private static void rb_parser_append_print()
        {
            tree.body = Parser.append(tree.body,
                new AST.METHOD_CALL("print", new AST.ARRAY(new AST.ARGS(new AST.GVAR("$_", null), null, null, null, null), null), null));
        }

        private static void rb_parser_while_loop(bool split, bool chop)
        {
            if (split)
                tree.body =
                    Parser.append(new AST.ASSIGNMENT(new AST.GVAR("$F", null),
                        new AST.METHOD_CALL(new AST.GVAR("$_", null), "split", null, null), null),
                        tree.body);

            if (chop)
                tree.body =
                    Parser.append(new AST.METHOD_CALL(new AST.GVAR("$_", null), "chop!", null, null),
                        tree.body);

            tree.body = new AST.GetWhile(tree.body, tree, null);
        }


        private static bool proc_options(int first, string[] args)
        {
            bool do_search = false;
            bool version = false;
            bool copyright = false;
            bool verbose = false;
            String e_script = null;

            int i;
            for (i = first; i < args.Length; i++)
            {
                if (args[i][0] != '-' || args[i].Length == 1)
                    break;

                string s = args[i].Substring(1);

            reswitch:
                if (s == "")
                {
                    i++;
                    break;
                }
                switch (s[0])
                {
                    case 'a':
                        do_split = true;
                        set_runtime_option("do_split", do_split);
                        s = s.Substring(1);
                        goto reswitch;

                    case 'p':
                        do_print = true;
                        set_runtime_option("do_print", true);
                        goto case 'n';

                    case 'n':
                        do_loop = true;
                        s = s.Substring(1);
                        goto reswitch;

                    case 'd':
                        set_runtime_option("ruby_debug", true);
                        set_runtime_option("ruby_verbose", true);
                        s = s.Substring(1);
                        goto reswitch;

                    case 'y':
                        set_runtime_option("ruby_yydebug", 1);
                        s = s.Substring(1);
                        goto reswitch;

                    case 'v':
                        Version.ruby_show_version();
                        verbose = true;
                        s = s.Substring(1);
                        goto reswitch;

                    case 'w':
                        set_runtime_option("ruby_verbose", true);
                        s = s.Substring(1);
                        goto reswitch;

                    case 'W':
                        {
                            int numlen;
                            int v = 2;      // -W as -W2
                            Scanner.scan_oct(s, 1, out numlen);
                            if (numlen == 0) v = 1;
                            s = s.Substring(numlen);
                            switch (v)
                            {
                                case 0:
                                    set_runtime_option("ruby_verbose", null); break;
                                case 1:
                                    set_runtime_option("ruby_verbose", false); break;
                                default:
                                    set_runtime_option("ruby_verbose", true); break;
                            }
                            goto reswitch;
                        }
                    case 'c':
                        do_check = true;
                        s = s.Substring(1);
                        goto reswitch;

                    case 's':
                        sflag = true;
                        s = s.Substring(1);
                        goto reswitch;

                    case 'h':
                        usage(compiler_name);
                        return false;

                    case 'l':
                        do_line = true;
                        set_runtime_option("do_line", do_line);
                        s = s.Substring(1);
                        goto reswitch;

                    case 'S':
                        do_search = true;
                        s = s.Substring(1);
                        goto reswitch;

                    case 'e':
                        s = s.Substring(1);
                        if (s == "")
                        {
                            if (i + 1 < args.Length)
                                s = args[++i];
                            else
                            {
                                System.Console.Error.WriteLine("{0}: no code specified for -e", compiler_name);
                                return false;
                            }
                        }
                        if (script == null) script = "-e";
                        e_script = new String(s + "\n");
                        break;

                    case 'r':
                        if (s != "")
                            set_runtime_option("require", s);
                        else if (i + 1 < args.Length)
                            set_runtime_option("require", args[++i]);
                        break;

                    case 'i':
                        s = s.Substring(1);
                        set_runtime_option("ruby_inplace_mode", s);
                        break;

                    case 'x':
                        xflag = true;
                        s = s.Substring(1);
                        if (s != "")
                            try
                            {
                                System.IO.Directory.SetCurrentDirectory(s);
                            }
                            catch (System.IO.IOException)
                            {
                                rb_fatal("Can't chdir to {0}", s);
                            }
                        break;

                    case 'C':
                    case 'X':
                        s = s.Substring(1);
                        if (s == "")
                            if (i + 1 < args.Length)
                                s = args[++i];
                            else
                                rb_fatal("Can't chdir");
                        try
                        {
                            System.IO.Directory.SetCurrentDirectory(s);
                        }
                        catch (System.IO.IOException)
                        {
                            rb_fatal("Can't chdir to {0}", s);
                        }
                        break;

                    case 'F':
                        if (s != "")
                            set_runtime_option("rb_fs", s);
                        break;

                    case 'K':
                        if (s != "")
                            set_runtime_option("kcode", s);
                        s = s.Substring(1);
                        goto reswitch;

                    case 'T':
                        {
                            int numlen;
                            uint v = 1;
                            if (s != "")
                            {
                                v = Scanner.scan_oct(s, 2, out numlen);
                                if (numlen == 0) v = 1;
                                s = s.Substring(numlen);
                            }
                            set_runtime_option("safe_level", v);
                            goto reswitch;
                        }

                    case 'I':
                        s = s.Substring(1);
                        if (s != "")
                            set_runtime_option("include", s);
                        else if (i + 1 < args.Length)
                            set_runtime_option("include", args[++i]);
                        break;

                    case '0':
                        {
                            int numlen;
                            uint v;

                            v = Scanner.scan_oct(s, 4, out numlen);
                            s = s.Substring(numlen);
                            if (v > 0377)
                                set_runtime_option("rb_rs", null);
                            else if (v == 0 && numlen >= 2)
                                set_runtime_option("rb_rs", "\n\n");
                            else
                            {
                                char c = (char)(v & 0xff);
                                set_runtime_option("rb_rs", new string(c, 1));
                            }
                            goto reswitch;
                        }

                    case '-':
                        s = s.Substring(1);
                        if (s == "")
                        {
                            i++;
                            goto switch_end;
                        }
                        if (s == "copyright")
                            copyright = true;
                        else if (s == "debug")
                        {
                            set_runtime_option("ruby_debug", true);
                            set_runtime_option("ruby_verbose", true);
                        }
                        else if (s == "version")
                            version = true;
                        else if (s == "verbose")
                        {
                            verbose = true;
                            set_runtime_option("ruby_verbose", true);
                        }
                        else if (s == "yydebug")
                            set_runtime_option("ruby_yydebug", 1);
                        else if (s == "help")
                        {
                            usage(compiler_name);
                            return false;
                        }
                        else
                        {
                            System.Console.Error.WriteLine("{0}: invalid option --{1} (-h will show valid options)\n", compiler_name, s);
                            return false;
                        }
                        break;

                    default:
                        System.Console.Error.WriteLine("{0}: invalid option -{1} (-h will show valid options)\n", compiler_name, s);
                        return false;
                }
            }

        switch_end:
            if (first == 0) return false;

            if (version)
            {
                Version.ruby_show_version();
                return false;
            }

            if (copyright)
            {
                Version.ruby_show_copyright();
                return false;
            }

            if (e_script == null)
                if (i >= args.Length)
                {
                    if (verbose) return false;
                    script = "-";
                }
                else
                {
                    script = args[i];
                    if (script == "")
                        script = "-";
                    else if (do_search)
                    {
                        string path = System.Environment.GetEnvironmentVariable("RUBYPATH");

                        script = null;
                        if (path != null)
                            script = File.dln_find(args[i], path);
                        if (script == null)
                            script = File.dln_find(args[i], System.Environment.GetEnvironmentVariable("PATH_ENV"));
                        if (script == null)
                            script = args[i];
                    }

                    script = script.Replace('/', Path.DirectorySeparatorChar);
                    i++;
                }

            if (script == "-")
                e_script = new String(System.Console.In.ReadToEnd());

            ruby_script(script);
            ruby_set_argv(args, i);
            process_sflag();

            if (e_script != null)
                tree = (AST.SOURCEFILE)Parser.ParseString(null, null, script, e_script, 1);
            else
            {
                List<string> more_options;
                tree = File.load_file(null, script, xflag, out more_options, null);
                if (more_options != null)
                    proc_options(0, more_options.ToArray());
            }
            process_sflag();
            xflag = false;

            return true;
        }

        static void ruby_set_argv(string[] args, int i)
        {
            rb_argv = new List<string>(args);
            rb_argv.RemoveRange(0, i);
        }


        static void ruby_script(string script)
        {
            set_runtime_option("rb_progname", script);
        }


        static void process_sflag()
        {
            if (!sflag) return;

            int count = 0;
            foreach (string s in rb_argv)
            {
                if (s[0] != '-' || s == "--") break;
                string ss = s.Substring(1);
                count++;
                int p = ss.IndexOf('=');
                if (p >= 0)
                    set_runtime_option(ss.Substring(0, p), ss.Substring(p + 1));
                else
                    set_runtime_option(ss, null);
            }

            rb_argv.RemoveRange(0, count);

            sflag = false;
        }

        static void usage(string name)
        {
            string[] usage_msg = new string[] {
                "-0[octal]       specify record separator (\\0, if no argument)",
                "-a              autosplit mode with -n or -p (splits $_ into $F)",
                "-c              check syntax only",
                "-Cdirectory     cd to directory, before executing your script",
                "-d              set debugging flags (set $DEBUG to true)",
                "-e 'command'    one line of script. Several -e's allowed. Omit [programfile]",
                "-Fpattern       split() pattern for autosplit (-a)",
                "-i[extension]   edit ARGV files in place (make backup if extension supplied)",
                "-Idirectory     specify $LOAD_PATH directory (may be used more than once)",
                "-Kkcode         specifies KANJI (Japanese) code-set",
                "-l              enable line ending processing",
                "-n              assume 'while gets(); ... end' loop around your script",
                "-p              assume loop like -n but print line also like sed",
                "-rlibrary       require the library, before executing your script",
                "-s              enable some switch parsing for switches after script name",
                "-S              look for the script using PATH environment variable",
                "-T[level]       turn on tainting checks",
                "-v              print version number, then turn on verbose mode",
                "-w              turn warnings on for your script",
                "-W[level]       set warning level; 0=silence, 1=medium, 2=verbose (default)",
                "-x[directory]   strip off text before #!ruby line and perhaps cd to directory",
                "--copyright     print the copyright",
                "--version       print the version",
                "--exe           compile to a .NET exe file",
                "--dll           compile to a .NET dll file"
                };

            System.Console.WriteLine("Usage: {0} [switches] [--] [programfile] [arguments]\n", name);

            foreach (string p in usage_msg)
                System.Console.WriteLine("  {0}", p);
        }


        private static void rb_fatal(string msg, params object[] args)
        {
            throw new fatal(string.Format(CultureInfo.InvariantCulture, msg, args)).raise(null);
        }

        private static string compiler_name
        {
            get { return System.Environment.GetCommandLineArgs()[0]; }
        }

        private static List<KeyValuePair<string, object>> runtime_options = new List<KeyValuePair<string, object>>();

        private static void set_runtime_option(string name, object value)
        {
            runtime_options.Add(new KeyValuePair<string, object>(name, value));
        }
    }
}

