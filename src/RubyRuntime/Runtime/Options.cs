/**********************************************************************

  Ruby.NET Runtime Library
  Originally developed at Queensland University of Technology
 
  Some sections of this C# code mirror the structure of the C code in the
  Ruby 1.8.2 Interpreter Copyright (C) 1993-2003 Yukihiro Matsumoto, et.al.
  
**********************************************************************/


using Ruby;
using System.Collections.Generic;
using System.Text;

namespace Ruby.Runtime
{
    // Ruby.Options - runtime representation of command line options

    [UsedByRubyCompiler]
    public class Options
    {
        internal static global_variable ruby_debug = new global_variable();
        internal static verbose_global ruby_verbose = new verbose_global();
        internal static readonly_global do_print = new readonly_global();
        internal static readonly_global do_line = new readonly_global();
        internal static readonly_global do_split = new readonly_global();
        internal static arg0_global rb_progname = new arg0_global();
        internal static readonly_global rb_argv = new readonly_global();

        static Options()
        {
            do_print.value = false;
            do_split.value = false;
        }

        [UsedByRubyCompiler]
        public static void SetRuntimeOption(string name, object value)
        {
            switch (name)
            {
                case "do_line":
                    Options.do_line.value = (bool)value;
                    break;
                case "do_print":
                    Options.do_print.value = (bool)value;
                    break;
                case "do_split":
                    Options.do_split.value = (bool)value;
                    break;
                case "rb_fs":
                    String.rb_fs.value = new String((string)value);
                    break;
                case "rb_rs":
                    IO.rb_rs.value = new String((string)value);
                    break;
                case "kcode":
                    Regexp.rb_set_kcode((string)value);
                    break;
                case "rb_progname":
                    Options.rb_progname.value = new String((string)value);
                    ((Array)Eval.rb_features.value).Add(value);
                    break;
                case "require":
                    throw new System.NotImplementedException("require option");
                case "ruby_debug":
                    Options.ruby_debug.value = (bool)value;
                    break;
                case "ruby_inplace_mode":
                    IO.ruby_inplace_mode = (string)value;
                    break;
                case "ruby_verbose":
                    Options.ruby_verbose.value = (bool)value;
                    break;
                case "ruby_yydebug":
                    throw new System.NotImplementedException("ruby_yydebug option");
                case "safe_level":
                    Eval.safe.value = (int)value;
                    break;
                case "include":
                    // Fixme: should be prepended
                    ((Array)(Eval.rb_load_path.value)).Add(new String((string)value));
                    break;
                default:
                    Variables.gvar_set("$" + name, value != null ? new String((string)value) : (object)true, null);
                    break;
            }
        }

        [UsedByRubyCompiler]
        public static void SetArgs(string[] args)
        {
            foreach (string arg in args)
                ((Array)rb_argv.value).Add(new String(arg));
        }


        internal static string[] rb_w32_cmdvector(string cmd)
        {
            List<string> args = new List<string>();

            int i = 0;
            while (i < cmd.Length)
            {
                while (i < cmd.Length && char.IsWhiteSpace(cmd[i]))
                    i++;

                int start = i;

                int slashes = 0;
                bool globbing = false; 
                char quote = '\0';
                bool done = false;
                
                while (i < cmd.Length && !done) 
                {
                    switch (cmd[i]) 
                    {
                        case '\\':
                            slashes++;
                            break;

                        case ' ':
                        case '\t':
                        case '\n':
                            if (quote == '\0') 
                                done = true;
                            break;

                        case '*':
                        case '?':
                        case '[':
                        case '{':
                            if (quote != '\'')
                                globbing = true;
                            slashes = 0;
                            break;

                        case '\'':
                        case '\"':
                            if ((slashes & 1) == 0) 
                            {
                                if (quote == '\0')
                                    quote = cmd[i];
                                else if (quote == cmd[i])
                                    quote = '\0';
                            }
                            slashes = 0;
                            break;

                        default:
                            i++; // Fixme: NextChar
                            slashes = 0;
                            continue;
                    }
                    i++;
                }

                if (done) i--;

                string element = cmd.Substring(start, i-start);

                if (element.Length > 1 &&
                    ((element[0] == '\'' && element[element.Length - 1] == '\'') ||
                     (element[0] == '\"' && element[element.Length - 1] == '\"')))
                    element = element.Substring(1, element.Length - 2);

                if (globbing)
                {
                    List<string> files = Dir.glob(element);
                    if (files.Count == 0)
                        args.Add(element);
                    else
                        args.AddRange(files);

                }
                else if (element != "")
                {
                    args.Add(element);
                }
            }

            return args.ToArray();
        }
    }

    
    internal class verbose_global : global_variable
    {
        // verbose_setter
        internal override void setter(string id, object val, Frame caller)
        {
            value = Eval.Test(val) ? true : val;
        }
    }

    
    internal class arg0_global : global_variable
    {
        // set_arg0
        internal override void setter(string id, object val, Frame caller)
        {
            // Fixme
            value = val;
        }
    }
}
