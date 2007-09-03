/**********************************************************************

  Ruby.NET Runtime Library
  Originally developed at Queensland University of Technology
 
  Some sections of this C# code mirror the structure of the C code in the
  Ruby 1.8.2 Interpreter Copyright (C) 1993-2003 Yukihiro Matsumoto, et.al.
  
**********************************************************************/

using Ruby.Runtime;

namespace Ruby
{

    public partial class FileStat : Basic
    {
        internal FileStat() : this(Ruby.Runtime.Init.rb_cStat) { }

        internal FileStat(string path)
            : this(Ruby.Runtime.Init.rb_cStat)
        {
            this.stat = fstat(path);
        }

        public FileStat(Class klass) : base(klass) { }

        internal Stat stat;

        internal override object Inner()
        {
            return stat;
        }

        internal class Stat : System.ICloneable
        {
            internal System.IO.FileSystemInfo fsi;
            internal int st_dev;
            internal int st_ino;
            internal int st_mode;
            internal int st_nlink;
            internal int st_uid;
            internal int st_gid;
            //internal int st_rdev;
            internal long st_size;
            //internal int st_blksize;
            //internal int st_blocks;
            internal System.DateTime st_atime;
            internal System.DateTime st_mtime;
            internal System.DateTime st_ctime;

            public object Clone()
            {
                return this.MemberwiseClone();
            }
        }

        internal Stat fstat(string path)
        {
            System.IO.FileSystemInfo fsi;
            if (System.IO.Directory.Exists(path))
                fsi = new System.IO.DirectoryInfo(path);
            else if (System.IO.File.Exists(path))
                fsi = new System.IO.FileInfo(path);
            else
                throw SystemCallError.rb_sys_fail(path, new System.IO.FileNotFoundException("", path), null).raise(null);

            Stat st = new Stat();
            st.fsi = fsi;
            st.st_dev = (int)fsi.FullName.ToCharArray(0,1)[0];  // drive letter as int, what to do for networked files?
            st.st_ino = 0; // no inode on win32
            st.st_mode = 0;
            st.st_nlink = 1; // no symbolic link support on win32
            st.st_uid = 0;  // no uid in win32
            st.st_gid = 0; // no gid in win32
            //st.st_rdev = 0; // no rdev in win32
            st.st_size = (fsi is System.IO.FileInfo) ? ((System.IO.FileInfo)fsi).Length : 0;
            //st.st_blksize = 0; // no blksize in win32
            //st.st_blocks = 0; // no blocks in win32
            st.st_atime = fsi.LastAccessTime;
            st.st_mtime = fsi.LastWriteTime;
            st.st_ctime = fsi.CreationTime;

            return st;
        }
    }
}
