/**********************************************************************

  Ruby.NET Runtime Library
  Originally developed at Queensland University of Technology
 
  Some sections of this C# code mirror the structure of the C code in the
  Ruby 1.8.2 Interpreter Copyright (C) 1993-2003 Yukihiro Matsumoto, et.al.
  
**********************************************************************/


using System.Collections.Generic;
using System.IO;
using Microsoft.Build.Utilities;
using Ruby.Runtime;
using System.Globalization;


namespace Ruby
{

    public partial class File : IO
    {
        internal static object separator;

        internal const string PATH_SEP = ";";

        internal const int LOCK_SH = 1;
        internal const int LOCK_EX = 2;
        internal const int LOCK_NB = 4;
        internal const int LOCK_UN = 8;

        internal const int MAXPATHLEN = 1024;

        // -------------------------------------------------------

        public File(Class klass)
            : base(klass)
        {
        }

        internal File()
            : base(Ruby.Runtime.Init.rb_cFile)
        {
        }

        // -------------------------------------------------------

        internal static void define_filetest_function(string name, MethodBody func, int argc)
        {
            //FIXME: Caller
            Class.rb_define_module_function(Ruby.Runtime.Init.rb_mFileTest, name, func, argc, null);
            Class.rb_define_singleton_method(Ruby.Runtime.Init.rb_cFile, name, func, argc, null);
        }

        internal static void rb_file_const(string name, object value)
        {
            Variables.rb_define_const(Ruby.Runtime.Init.rb_mFConst, name, value);
        }

        // -------------------------------------------------------------

        internal static void chmod_internal(string path, int mode, Frame caller)
        {
            if (chmod(path, mode) < 0)
            {
                throw SystemCallError.rb_sys_fail(path, new System.IO.IOException(), caller).raise(caller);
            }
        }

        internal static int chmod(string path, int mode)
        {
            int _S_IWRITE = 128; //octal 0200

            try
            {
                if (System.IO.File.Exists(path) || System.IO.Directory.Exists(path))
                {
                    //this code appears to do nothing for directories on my WinXP machine but 
                    //using the 'attrib' command line tool for windows it is equivalent to ruby. 
                    if ((mode & _S_IWRITE) != 0)
                    {
                        System.IO.File.SetAttributes(path, System.IO.File.GetAttributes(path) & ~System.IO.FileAttributes.ReadOnly);

                    }
                    else
                    {
                        System.IO.File.SetAttributes(path, System.IO.File.GetAttributes(path) | System.IO.FileAttributes.ReadOnly);
                    }
                }
                else
                {
                    return -1;
                }
            }
            catch
            {
                return -1;
            }
            return 1;
        }


        internal static string getcwdofdrv(char driveletter)
        {
            return "\\";
        }

        internal static bool has_drive_letter(string path)
        {
            return (path.Length > 1 && char.IsLetter(path[0]) && path[1] == ':');
        }

        internal static bool isdirsep(char c)
        {
            return (c == '\\' || c == '/');
        }

        internal static bool isdirsep(string path)
        {
            return (path.StartsWith("\\") || path.StartsWith("/"));
        }

        internal static bool is_absolute_path(string path)
        {
            if (path.Length > 2 && has_drive_letter(path) && isdirsep(path[2]))
                return true;
            if (isdirsep(path[0]) && path.Length > 1 && isdirsep(path[1]))
                return true;
            return false;
        }

        internal static string skiproot(string path)
        {
            if (has_drive_letter(path))
                path = path.Substring(2);
            while (isdirsep(path))
                path = path.Substring(1);
            return path;
        }

        internal static string skipprefix(string path)
        {
            return rb_path_skip_prefix(path);
        }

        internal static string rb_path_skip_prefix(string path)
        {
            if (isdirsep(path) && isdirsep(path.Substring(1)))
            {
                if ((path = nextdirsep(path.Substring(2))) != null)
                {
                    if (path == null || path.Length == 0)
                        return path;
                    path = nextdirsep(path.Substring(1));
                }
                return path;
            }

            if (has_drive_letter(path))
                return path.Substring(2);

            return path;
        }

        internal static string strrdirsep(string path)
        {
            return rb_path_last_separator(path);
        }

        internal static bool strchr(string str, char val)
        {
            return (new string((char)val, 1).IndexOfAny(str.ToCharArray()) >= 0);
        }

        internal static void test_check(int n, Array args, Frame caller)
        {
            if (++n != args.Count)
                throw new ArgumentError(string.Format(CultureInfo.InvariantCulture, "wrong number of arguments ({0} for {1})", args.Count, n)).raise(caller);
        }

        internal static string rb_path_last_separator(string path)
        {
            string last = null;
            int i = 0;
            while (path != null && i < path.Length)
            {
                if (isdirsep(path[i]))
                {
                    int tmp = i++;
                    while (i < path.Length && isdirsep(path[i]))
                        i++;
                    if (i >= path.Length)
                        break;
                    last = path.Substring(tmp);
                }
                else
                    i++;
            }
            return last;
        }

        internal static string nextdirsep(string path)
        {
            return rb_path_next(path);
        }

        internal static string rb_path_next(string path)
        {
            for (int i = 0; i < path.Length; i++)
                if (!isdirsep(path))
                    path = path.Substring(1);
                else
                    break;
            return path;
        }

        internal static int chompdirsep(string path)
        {
            return rb_path_end(path);
        }

        internal static int rb_path_end(string path)
        {
            int i = 0;
            while (path != null && i < path.Length)
            {
                if (isdirsep(path.Substring(i)))
                {
                    int last = i;
                    while (isdirsep(path.Substring(i)))
                        i++;
                    if (i >= path.Length)
                        return last;
                }
                else
                    i++;
            }
            return i;
        }

        internal static int rmext(string p, string e)
        {
            if (e == null || e.Length == 0)
                return 0;

            if (e.Length == 2 && e.EndsWith("*"))
                return p.LastIndexOf(e.ToCharArray(0, 1)[0]);

            p = p.Substring(0, chompdirsep(p));

            if (p.EndsWith(e))
                return p.Length - e.Length;

            return 0;
        }

        internal static object rb_file_ftype(FileStat.Stat st)
        {
            if (st.fsi is System.IO.DirectoryInfo)
                return new String("directory");
            else if (st.fsi is System.IO.FileInfo)
                return new String("file");
            //else if (st is ?)
            //    return "characterSpecial");
            //else if (st is ?)
            //    return "blockSpecial");
            else
                return new String("unknown");
        }


        internal static String file_expand_path(Frame caller, object fname, object dname, System.Text.StringBuilder result)
        {
            string s, b, root;
            int buf, p, pend;
            int buflen, dirlen, bdiff;
            bool tainted;

            String Fname = String.RStringValue(fname, caller);

            s = Fname.value;
            p = buf = 0;
            buflen = result.Capacity;
            pend = p + buflen;
            tainted = Fname.Tainted;

            if (s[0] == '~')
            {
                if (isdirsep(s[1]) || s[1] == '\0')
                {
                    string dir = System.Environment.GetEnvironmentVariable("HOME");

                    if (dir == null)
                    {
                        throw new ArgumentError(string.Format(CultureInfo.InvariantCulture, "couldn't find HOME environment -- expanding `{0}'", s)).raise(caller);
                    }
                    dirlen = dir.Length;
                    //BUFCHECK(dirlen > buflen);
                    bdiff = p - buf;
                    while (dirlen > buflen)
                    {
                        buflen *= 2;
                    }
                    result.Capacity = buflen;
                    buf = 0;
                    p = buf + bdiff;
                    pend = buf + buflen;

                    result.Append(dir);
                    for (p = buf; p < result.Length; p++)
                    {
                        if (result[p] == '\\')
                        {
                            result[p] = '/';
                        }
                    }
                    s = s.Substring(1);
                    tainted = true;
                }
                else
                {
                    s = nextdirsep(b = s);
                    //BUFCHECK(bdiff + (s - b) >= buflen);
                    bdiff = p - buf;
                    while (bdiff + (s.Length - b.Length) >= buflen)
                    {
                        buflen *= 2;
                    }
                    result.Capacity = buflen;
                    buf = 0;
                    p = buf + bdiff;
                    pend = buf + buflen;

                    result.Append(b, 0, s.Length - b.Length);
                    p += s.Length - b.Length;
                }
            }
            /* skip drive letter */
            else if (has_drive_letter(s))
            {
                if (isdirsep(s[2]))
                {
                    /* specified drive letter, and full path */
                    /* skip drive letter */
                    //BUFCHECK(bdiff + 2 >= buflen);
                    bdiff = p - buf;
                    while (bdiff + 2 >= buflen)
                    {
                        buflen *= 2;
                    }
                    result.Capacity = buflen;
                    buf = 0;
                    p = buf + bdiff;
                    pend = buf + buflen;

                    result.Append(s, 0, 2);
                    p += 2;
                    s = s.Substring(2);
                }
                else
                {
                    /* specified drive, but not full path */
                    bool same = false;
                    if (dname != null)
                    {
                        file_expand_path(caller, dname, null, result);
                        p = buf = 0;
                        buflen = result.Capacity;
                        pend = p + buflen;
                        if (has_drive_letter(result.ToString()) && (result[0] == s[0]))
                        {
                            /* ok, same drive */
                            same = true;
                        }
                    }
                    if (!same)
                    {
                        string dir = new System.IO.DriveInfo(s).RootDirectory.FullName;

                        tainted = true;
                        dirlen = dir.Length;
                        //BUFCHECK(dirlen > buflen);
                        bdiff = p - buf;
                        while (dirlen > buflen)
                        {
                            buflen *= 2;
                        }
                        result.Capacity = buflen;
                        buf = 0;
                        p = buf + bdiff;
                        pend = buf + buflen;

                        result.Append(dir);
                    }
                    p = chompdirsep(skiproot(result.ToString()));
                    s = s.Substring(2);
                }
            }
            else if (!is_absolute_path(s))
            {
                if (dname != null)
                {
                    file_expand_path(caller, dname, null, result);
                    p = buf = 0;
                    buflen = result.Capacity;
                    pend = p + buflen;
                }
                else
                {
                    string dir = System.Environment.CurrentDirectory;

                    tainted = true;
                    dirlen = dir.Length;
                    //BUFCHECK(dirlen > buflen);
                    bdiff = p - buf;
                    while (dirlen > buflen)
                    {
                        buflen *= 2;
                    }
                    result.Capacity = buflen;
                    buf = 0;
                    p = buf + bdiff;
                    pend = buf + buflen;

                    result.Append(dir);
                }
                if (isdirsep(s))
                {
                    /* specified full path, but not drive letter nor UNC */
                    /* we need to get the drive letter or UNC share name */
                    p = result.ToString().IndexOf(skipprefix(result.ToString()));
                }
                else
                {
                    string tmp = skiproot(result.ToString());
                    p = chompdirsep(tmp) + result.Length - tmp.Length;
                }
            }
            else
            {
                int i = 0;
                do
                    i++;
                while (i < s.Length && isdirsep(s[i]));
                p = buf + i;
                //BUFCHECK(bdiff >= buflen);
                bdiff = p - buf;
                while (bdiff >= buflen)
                {
                    buflen *= 2;
                }
                result.Capacity = buflen;
                buf = 0;
                p = buf + bdiff;
                pend = buf + buflen;

                if (result.Length < p - buf)
                    result.Append('/');
                else
                    result[p - buf] = '/';
            }

            if (p < result.Length)
                result.Remove(p, result.Length - p);

            if (p > buf)
                if (p - 1 > result.Length - 1)
                {
                    result.Append('/');
                    p++;
                }
                else if (result[p - 1] == '/')
                { }
                else
                {
                    result.Append('/');
                    p++;
                }
            else
            {
                result.Append('/');
                p++;
            }

            root = skipprefix(result.ToString());

            b = s;
            while (s != null && s.Length > 0)
            {
                switch (s[0])
                {
                    case '.':
                        if (b.Equals(s))
                        {    /* beginning of path element */
                            s = s.Substring(1);
                            if (s.Length > 0)
                                switch (s[0])
                                {
                                    case '\0':
                                        b = s;
                                        break;
                                    case '.':
                                        if (s[1] == '\0' || isdirsep(s[1]))
                                        {
                                            /* We must go back to the parent */
                                            if ((b = strrdirsep(root)) == null)
                                            {
                                                result[p] = '/';
                                            }
                                            else
                                            {
                                                p = s.IndexOf(b);
                                            }
                                            s = s.Substring(1);
                                            b = s;
                                        }
                                        break;
                                    case '/':
                                    case '\\':
                                        s = s.Substring(1);
                                        b = s;
                                        break;
                                    default:
                                        /* ordinary path element, beginning don't move */
                                        break;
                                }
                            else
                                b = s;
                        }
                        else
                            s = s.Substring(1);
                        break;
                    case '/':
                    case '\\':
                        if (b.Length > s.Length)
                        {
                            int rootdiff = root.Length - buf;
                            //BUFCHECK(bdiff + (s - b + 1) >= buflen);
                            bdiff = p - buf;
                            while (bdiff + (b.Length - s.Length + 1) >= buflen)
                            {
                                buflen *= 2;
                            }
                            result.Capacity = buflen;
                            buf = 0;
                            p = buf + bdiff;
                            pend = buf + buflen;

                            root = result.ToString().Substring(buf + rootdiff);
                            result.Append(b, 0, b.Length - s.Length);
                            p += b.Length - s.Length + 1;
                            result.Append('/');
                        }
                        s = s.Substring(1);
                        b = s;
                        break;
                    default:
                        s = s.Substring(1);
                        break;
                }
            }

            if (b.Length > s.Length)
            {
                //BUFCHECK(bdiff + (s - b) >= buflen);
                bdiff = p - buf;
                while (bdiff + (b.Length - s.Length) >= buflen)
                {
                    buflen *= 2;
                }
                result.Capacity = buflen;
                buf = 0;
                p = buf + bdiff;
                pend = buf + buflen;

                result.Append(b, 0, b.Length - s.Length);
                p += b.Length - s.Length;
            }
            if (result.Length > 3 && result[result.Length - 1] == '/')
                result.Remove(result.Length - 1, 1);

            if (result.Length > p - buf)
                result.Remove(p - buf, result.Length - (p - buf));

            String res = new String(result.ToString());
            res.Tainted = tainted;
            return res;
        }

        internal static String rb_file_expand_path(Frame caller, object fname, object dname)
        {
            return file_expand_path(caller, fname, dname, new System.Text.StringBuilder(MAXPATHLEN + 2));
        }

        private static bool ISDIRSEP(bool pathname, char c)
        {
            return pathname && isdirsep(c);
        }

        private static bool PERIOD(bool period, string str, int pos)
        {
            return (period && str[pos] == '.' && (pos == 0 || isdirsep(str[pos - 1])));
        }

        private static char downcase(bool nocase, char c)
        {
            return nocase && char.IsUpper(c) ? char.ToLowerInvariant(c) : c;
        }

        private static string range(string pat, char test, int flags)
        {
            bool not, ok = false;
            bool escape = !((flags & Dir.FNM_NOESCAPE) > 0);
            bool nocase = (flags & Dir.FNM_CASEFOLD) > 0;
            int i = 0;

            not = (pat[i] == '!' || pat[i] == '^');
            if (not)
                i++;

            test = downcase(nocase, test);

            while (pat[i] != ']')
            {
                char cstart, cend;
                if (escape && pat[i] == '\\')
                    i++;

                if (i + 1 < pat.Length)
                    cstart = cend = pat[i++];
                else
                    return null;

                if (pat[i++] == '-' && pat[1] != ']')
                {
                    i++;
                    if (escape && pat[i] == '\\')
                        i++;

                    if (i + 1 < pat.Length)
                        cend = pat[i++];
                    else
                        return null;
                }
                if (downcase(nocase, cstart) <= test && test <= downcase(nocase, cend))
                    ok = true;
            }
            return ok == not ? null : pat.Substring(i + 1);
        }

        internal static int fnmatch(string pat, string str, int flags)
        {
            bool escape = !((flags & Dir.FNM_NOESCAPE) > 0);
            bool pathname = (flags & Dir.FNM_PATHNAME) > 0;
            bool period = !((flags & Dir.FNM_DOTMATCH) > 0);
            bool nocase = (flags & Dir.FNM_CASEFOLD) > 0;

            if (pat == null || pat.Length == 0)
                pat = "";
            if (str == null || str.Length == 0)
                str = "";

            int j = 0;
            for (int i = 0; i < pat.Length; i++)
            {
                char c = pat[i];
                char test;

                switch (c)
                {
                    case '?':
                        if (j > str.Length || ISDIRSEP(pathname, str[j]) || PERIOD(period, str, j))
                            return Dir.FNM_NOMATCH;
                        j++;
                        break;
                    case '*':
                        while (i < pat.Length && (c = pat[i++]) == '*')
                            ;

                        if (PERIOD(period, str, j))
                            return Dir.FNM_NOMATCH;

                        if (i >= pat.Length)
                        {
                            if (pathname && rb_path_next(str.Substring(j)).Equals(""))
                                return Dir.FNM_NOMATCH;
                            else
                                return 0;
                        }
                        else if (ISDIRSEP(pathname, c))
                        {
                            str = rb_path_next(str.Substring(j));
                            j = 0;
                            if (str.Length > 0)
                            {
                                break;
                            }
                            return Dir.FNM_NOMATCH;
                        }

                        test = (escape && (c == '\\')) ? pat[i] : c;
                        test = downcase(nocase, test);
                        i--;
                        while (j < str.Length)
                        {
                            if ((c == '?' || c == '[' || downcase(nocase, str[j]) == test) &&
                                fnmatch(pat.Substring(i), str.Substring(j), flags | Dir.FNM_DOTMATCH) > 0)
                                return 0;
                            else if (ISDIRSEP(pathname, str[j]))
                                break;
                            j++;
                        }
                        return Dir.FNM_NOMATCH;

                    case '[':
                        if (j > str.Length || ISDIRSEP(pathname, str[j]) || PERIOD(period, str, j))
                            return Dir.FNM_NOMATCH;
                        pat = range(pat.Substring(i), str[j], flags);
                        i = 0;
                        if (pat == null || pat.Length == 0)
                            return Dir.FNM_NOMATCH;
                        j++;
                        break;

                    case '\\':
                        if (escape && i < pat.Length && strchr("*?[]\\", pat[i]))
                        {
                            c = pat[i];
                            if (i > pat.Length)
                                c = '\\';
                            else
                                i++;
                        }
                        /* FALLTHROUGH */
                        goto default;
                    default:
                        if (ISDIRSEP(pathname, c) && isdirsep(str[j]))
                        { }
                        else
                            if (downcase(nocase, c) != downcase(nocase, str[j]))
                                return Dir.FNM_NOMATCH;
                        j++;
                        break;
                }
            }
            return j > str.Length ? 0 : Dir.FNM_NOMATCH;
        }

        internal static string Extension(string filename)
        {
            return new System.IO.FileInfo(filename).Extension;
        }

        internal static string dln_find(string fname, string path)
        {
            return dln_find(fname, new Array(path.Split(';')), true);
            // TODO: See if "true" is correct.
        }

        internal static string dln_find(string fname, Array path, bool try_add_ext)
        {
            if (is_absolute_path(fname))
                return find_fullname(fname, try_add_ext);

            fname = fname.Replace('/', Path.DirectorySeparatorChar);

            foreach (String dir in path)
            {
                string fullname = find_fullname(Path.Combine(dir.value, fname), try_add_ext);
                if (fullname != null)
                    return fullname;
            }

            return null;
        }

        private static string find_fullname(string fname, bool try_add_ext)
        {
            if (System.IO.File.Exists(fname))
                return fname;

            if (try_add_ext)
            {
                System.IO.FileInfo file = new System.IO.FileInfo(fname);
                string temp;

                if (file.Extension == "")
                {
                    temp = fname + ".rb";
                    if (System.IO.File.Exists(temp))
                        return temp;

                    temp = fname + ".dll";
                    if (System.IO.File.Exists(temp))
                        return temp;
                }

                if (file.Extension == ".rb")
                {
                    temp = fname.Substring(0, fname.Length - 3) + ".dll";
                    if (System.IO.File.Exists(temp))
                        return temp;
                }
            }

            return null;
        }

        internal static bool file_load_ok(string path)
        {
            if (!System.IO.File.Exists(path))
                return false;

            try
            {
                System.IO.Stream f = System.IO.File.OpenRead(path);
                f.Close();
                return true;
            }
            catch (System.UnauthorizedAccessException)
            {
                return false;
            }
            catch (System.IO.IOException)
            {
                return false;
            }
        }

        internal static string basename(string name)
        {
            System.IO.FileInfo file = new System.IO.FileInfo(name);
            return Path.Combine(file.DirectoryName, file.Name.Substring(0, file.Name.Length - file.Extension.Length));
        }

        internal static string stripExtension(string name)
        {
            // BBTAG: try using absolute paths instead
            //return fileNameToClassName(name);
            System.IO.FileInfo file = new System.IO.FileInfo(name);
            return file.Name.Substring(0, file.Name.Length - file.Extension.Length);
        }

        internal static string fileNameToClassName(string name)
        {
            System.IO.FileInfo file = new System.IO.FileInfo(name);
            // FIXME: change to RegExp
            string className = file.FullName.Replace('.', '_').Replace('/', '_').Replace(':', '_').Replace('\\', '_').Replace(' ', '_');
            return className.Substring(0, className.Length - file.Extension.Length);
        }

        internal static String rb_find_file(String path, bool try_add_ext)
        {
            string f = path.value;

            if (file_load_ok(f))
                return path;

            f = dln_find(f, (Array)Eval.rb_load_path.value, try_add_ext);

            if (f != null && file_load_ok(f))
                if (f.EndsWith(".rb") && needs_compiling(f, ".dll"))
                    return new String(f);
                else
                    return new String(basename(f) + ".dll");
            else
                return null;
        }

        internal static bool needs_compiling(string f, string option)
        {
            //string bname = basename(f);

            //if (file_load_ok(bname + option))
            //{
            //    System.IO.FileInfo rb = new System.IO.FileInfo(bname + ".rb");
            //    System.IO.FileInfo dll = new System.IO.FileInfo(bname + option);
            //    System.IO.FileInfo compiler = new System.IO.FileInfo(System.Reflection.Assembly.GetExecutingAssembly().Location);
            //    bool yes = dll.LastWriteTime < rb.LastWriteTime || dll.LastWriteTime < compiler.LastWriteTime;
            //    return yes;
            //}

            return true;
        }


        internal static Compiler.AST.SOURCEFILE load_file(Frame caller, string fname, bool xflag, out List<string> more_options, TaskLoggingHelper log)
        {
            int line_start;

            IO f = null;
            try
            {
                f = IO.rb_file_open(caller, fname, "r");
            }
            catch (System.IO.IOException e)
            {
                throw new LoadError(e.Message).raise(caller);
            }
            string header_line = null;

            if (xflag)
                line_start = skip_file_header(caller, f, out header_line);
            else
                line_start = 1;

            if (header_line != null || f.Peek(caller) == '#')
            {
                if (header_line == null)
                    header_line = f.ReadLine(caller);
                more_options = read_interpreter_path_and_options(header_line);
                line_start++;
            }
            else
                more_options = null;

            Compiler.AST.SOURCEFILE AST = Compiler.Parser.ParseFile(fname, f, line_start, log);

            // Fixme: Read DATA after program here

            IO.rb_io_close(f);

            return AST;
        }

        private static int skip_file_header(Frame caller, IO f, out string line)
        {
            int line_start = 1;

            while ((line = f.ReadLine(caller)) != null)
            {
                line_start++;
                if (line.StartsWith("#!") && line.Contains("ruby"))
                    return line_start;
            }
            throw new LoadError("No Ruby script found in input").raise(caller);
        }

        private static List<string> read_interpreter_path_and_options(string line)
        {
            if (line.StartsWith("#!"))
            {
                int p;
                if (line.Contains("ruby"))
                    p = line.IndexOf("ruby") + 4;
                else
                {
                    p = 2;
                    while (p < line.Length && char.IsWhiteSpace(line[p]))
                        p++;
                    int start = p;
                    while (p < line.Length && !char.IsWhiteSpace(line[p]))
                        p++;
                    string path = line.Substring(start, p - start);
                }

                return ReadOptions(line, p);
            }
            else
                return null;
        }


        private static List<string> ReadOptions(string line, int p)
        {
            List<string> options = new List<string>();

            while (p < line.Length)
            {
                while (p < line.Length && char.IsWhiteSpace(line[p]))
                    p++;

                if (line[p] == '-')
                {
                    int start = p;
                    p++;
                    while (p < line.Length && !char.IsWhiteSpace(line[p]))
                        p++;
                    options.Add(line.Substring(start, p - start));
                }
                else
                    break;
            }

            return options;
        }
    }
}
