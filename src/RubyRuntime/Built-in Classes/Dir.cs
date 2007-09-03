/**********************************************************************

  Ruby.NET Runtime Library
  Originally developed at Queensland University of Technology
 
  Some sections of this C# code mirror the structure of the C code in the
  Ruby 1.8.2 Interpreter Copyright (C) 1993-2003 Yukihiro Matsumoto, et.al.
  
**********************************************************************/

using Ruby.Runtime;
using System.Globalization;

namespace Ruby
{

    public partial class Dir : Object
    {
        internal System.IO.DirectoryInfo _dir;
        internal string _path;
        internal Array _entries;
        internal int _pos;


        //-----------------------------------------------------------------


        public Dir(Class klass)
            : base(klass)
        {
        }



        //-----------------------------------------------------------------


        internal const int FNM_NOESCAPE = 0x01;
        internal const int FNM_PATHNAME = 0x02;
        internal const int FNM_DOTMATCH = 0x04;
        internal const int FNM_CASEFOLD = 0x08;


        internal const int FNM_NOMATCH = 1;
        internal const int FNM_ERROR = 2;



        //-----------------------------------------------------------------



        internal static Dir dir_initialize(Frame caller, object _dir, string dirname)
        {
            Dir dir = (Dir)_dir;

            try
            {
                dir._dir = new System.IO.DirectoryInfo(dirname);
                dir._path = dirname;
                dir._entries = Dir.DirectoryEntries(caller, dir._dir);
                dir._pos = 0;
            }
            catch (System.Exception ex)
            {
                throw SystemCallError.rb_sys_fail(dirname, ex, caller).raise(caller);
            }

            return dir;
        }


        internal static Dir GetDIR(Frame caller, object obj)
        {
            if (!(obj is Dir))
                throw new TypeError(string.Format(CultureInfo.InvariantCulture, "wrong argument type {0} (expected Dir)", ((Object)obj).my_class._name)).raise(caller);

            Dir dir = (Dir)obj;
            if (dir._dir == null)
                dir_closed(caller);

            return dir;
        }


        internal static void dir_closed(Frame caller)
        {
            throw new IOError("closed directory").raise(caller);
        }

        internal static Dir dir_open_dir(Frame caller, object path)
        {
            object dir = Eval.CallPrivate(Ruby.Runtime.Init.rb_cDir, caller, "open", null, new object[] { path });


            if (!(dir is Dir))
            {
                throw new TypeError(string.Format(CultureInfo.InvariantCulture, "wrong argument type {0} (expected Dir)", ((Object)dir).my_class._name)).raise(caller);
            }
            return (Dir)dir;
        }


        private static bool hasGlobChars(string searchString)
        {
            if (searchString.IndexOfAny(new char[] { '*', '?' }) >= 0)
                return true;


            int bracketPos;
            if ((bracketPos = searchString.IndexOf('[')) >= 0 && searchString.IndexOf(']', bracketPos) >= 0)
                return true;
            if ((bracketPos = searchString.IndexOf('{')) >= 0 && searchString.IndexOf('}', bracketPos) >= 0)
                return true;


            return false;
        }


        private static string getRoot(string path, int index)
        {
            int rootEnd = path.Substring(0, index).LastIndexOf('/');


            if (rootEnd > 0)
                return path.Substring(0, rootEnd);
            else
                return string.Empty;
        }


        private static string getElement(string path, int index)
        {
            int rootEnd = path.Substring(0, index).LastIndexOf('/');
            int elementEnd = path.IndexOf('/', rootEnd + 1);


            if (elementEnd > 0)
                return path.Substring(rootEnd + 1, elementEnd - rootEnd - 1);
            else
                return path.Substring(rootEnd + 1);
        }


        private static string getRemainder(string path, int index)
        {
            int elementEnd = path.IndexOf('/', index);


            if (elementEnd > 0)
                return path.Substring(elementEnd);
            else
                return string.Empty;
        }


        private static System.Collections.Generic.List<string> glob(string searchString)
        {
            System.Collections.Generic.List<string> files = new System.Collections.Generic.List<string>();

            // BBTAG: why??
            //if (searchString.Contains("\\"))
            //    return files;


            if (!hasGlobChars(searchString))
            {
                if (System.IO.File.Exists(searchString) || System.IO.Directory.Exists(searchString))
                    files.Add(searchString);
                //else
                    //Errors.rb_warn(searchString);
                return files;
            }


            // Record nested braces
            int braceStart, braceEnd;
            if ((braceStart = searchString.IndexOf('{')) >= 0 && (braceEnd = searchString.IndexOf('}', braceStart)) >= 0)
            {
                string[] brace = searchString.Substring(braceStart + 1, braceEnd - braceStart - 1).Split(new char[] { ',' });
                searchString = searchString.Remove(braceStart, braceEnd - braceStart + 1);
                foreach (string insert in brace)
                    files.AddRange(glob(searchString.Insert(braceStart, insert)));
                return files;
            }


            // Ignore nested brackets
            int bracketStart, bracketEnd;
            if ((bracketStart = searchString.IndexOf('[')) >= 0 && (bracketEnd = searchString.IndexOf(']', bracketStart)) >= 0)
            {
                string bracket = searchString.Substring(bracketStart + 1, bracketEnd - bracketStart - 1);
                searchString = searchString.Remove(bracketStart, bracketEnd - bracketStart + 1);


                if (bracket.StartsWith("^"))
                {
                    for (char c = (char)0; c < char.MaxValue; c++)
                    {
                        if (!bracket.Contains(c.ToString()) && (char.IsLetterOrDigit(c) || c == ' ' || (!char.IsWhiteSpace(c) && !char.IsControl(c) && c != '\"' && c != '\\' && c != '<' && c != '>' && c != '|' && c != ':')))
                            files.AddRange(glob(searchString.Insert(bracketStart, c.ToString())));
                    }
                }


                for (int i = 0; i < bracket.Length; i++)
                {
                    if (i + 1 < bracket.Length && bracket[i + 1] == '-' && i + 2 < bracket.Length)
                    {
                        for (char c = bracket[i]; c < bracket[i + 2]; c++)
                        {
                            files.AddRange(glob(searchString.Insert(bracketStart, c.ToString())));
                        }
                        i += 2;
                    }
                    else
                    {
                        files.AddRange(glob(searchString.Insert(bracketStart, bracket[i].ToString())));
                    }
                }
                return files;
            }


            int anyDirIndex = searchString.IndexOf("**");
            if (anyDirIndex > 0)
            {
                string dirRoot = getRoot(searchString, anyDirIndex);
                string dirElement = getElement(searchString, anyDirIndex);
                string remainder = getRemainder(searchString, anyDirIndex);
                if (remainder.Equals(string.Empty))
                    searchString = searchString.Remove(anyDirIndex, 1);
                else
                {
                    files.AddRange(glob(dirRoot + remainder));
                    try
                    {
                        foreach (string dir in System.IO.Directory.GetDirectories(dirRoot, dirElement, System.IO.SearchOption.AllDirectories))
                            files.AddRange(glob(dir.Replace('\\', '/') + remainder));
                    }
                    catch (System.IO.DirectoryNotFoundException)
                    {
                        //Errors.rb_warn(searchString));
                    }
                    return files;
                }
            }


            int anyIndex = searchString.IndexOfAny(new char[] { '*', '?' });
            if (anyIndex > 0 && searchString.IndexOf('/', anyIndex) > 0)
            {
                string remainder = getRemainder(searchString, anyIndex);
                try
                {
                    foreach (string dir in System.IO.Directory.GetDirectories(getRoot(searchString, anyIndex), getElement(searchString, anyIndex), System.IO.SearchOption.TopDirectoryOnly))
                        files.AddRange(glob(dir.Replace('\\', '/') + remainder));
                }
                catch (System.IO.DirectoryNotFoundException)
                {
                    //Errors.rb_warn(searchString));
                }
                return files;
            }


            string root = getRoot(searchString, searchString.Length);
            string realroot = root.Equals(string.Empty) ? "." : root;
            string element = searchString.Substring(root.Length == 0 ? 0 : root.Length + 1);
            try
            {
                foreach (string file in System.IO.Directory.GetFileSystemEntries(realroot, element))
                    if (root.Equals(string.Empty))
                        files.Add(file.Substring(2).Replace('\\', '/'));
                    else
                        files.Add(file.Replace('\\', '/'));
            }
            catch (System.IO.DirectoryNotFoundException)
            {
                //Errors.rb_warn(searchString));
            }


            return files;
        }


        internal static Array rb_push_glob(Frame caller, Proc block, object str, int flags)
        {
            Array ary = new Array();
            string path = String.StringValue(str, caller);


            foreach (string file in glob(path))
            {
                String s = new String(file);
                s.Tainted = true;
                ary.Add(s);
            }

            if (block != null)
            {
                Methods.rb_ary_each.singleton.Call0(null, ary, caller, block);
                return null;
            }
            return ary;
        }


        //-----------------------------------------------------------------


        internal static Array DirectoryEntries(Frame caller, System.IO.DirectoryInfo cd)
        {
            System.IO.FileSystemInfo[] infos = cd.GetFileSystemInfos();

            System.Collections.ArrayList names = new System.Collections.ArrayList();

            names.Add(new String("."));
            names.Add(new String(".."));

            for (int i = 0; i < infos.Length; i++)
                names.Add(new String(infos[i].Name));

            names.Sort(new String.CaseInsensitiveComparer(caller));

            return new Array(names);
        }
    }
}
