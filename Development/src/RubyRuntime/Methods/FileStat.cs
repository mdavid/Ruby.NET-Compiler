/**********************************************************************

  Ruby.NET Runtime Library
  Originally developed at Queensland University of Technology
 
  Some sections of this C# code mirror the structure of the C code in the
  Ruby 1.8.2 Interpreter Copyright (C) 1993-2003 Yukihiro Matsumoto, et.al.
  
**********************************************************************/

using Ruby;
using Ruby.Runtime;

namespace Ruby.Methods
{
    
    internal class rb_stat_s_alloc : MethodBody0  // author: cjs, status: done
    {
        internal static rb_stat_s_alloc singleton = new rb_stat_s_alloc();

        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {
            return new FileStat((Class)recv);
        }
    }

    
    internal class rb_stat_init : MethodBody1 // author: cjs, status: done
    {
        internal static rb_stat_init singleton = new rb_stat_init();

        public override object Call1(Class last_class, object recv, Frame caller, Proc block, object p1)
        {
            FileStat self = (FileStat)recv;

            string fname = String.StringValue(p1, caller);

            try
            {
            }
            catch (System.Exception e)
            {
                throw SystemCallError.rb_sys_fail(fname, e, caller).raise(caller);
            }

            return null;
        }
    }

    
    internal class rb_stat_init_copy : MethodBody1  // author: cjs, status: done
    {
        internal static rb_stat_init_copy singleton = new rb_stat_init_copy();

        public override object Call1(Class last_class, object recv, Frame caller, Proc block, object p1)
        {
            if (p1 == recv)
                return recv;

            TypeError.rb_check_frozen(caller, p1);

            if (!(recv is FileStat && p1 is FileStat))
                throw new TypeError("wrong argument class").raise(caller);

            FileStat orig = (FileStat)recv;
            FileStat copy = (FileStat)p1;

            copy.stat = (FileStat.Stat)orig.stat.Clone();

            return copy;
        }
    }

    
    internal class rb_stat_cmp : MethodBody1  // author: cjs, status: done
    {
        internal static rb_stat_cmp singleton = new rb_stat_cmp();

        public override object Call1(Class last_class, object recv, Frame caller, Proc block, object p1)
        {
            if (recv is FileStat && p1 is FileStat)
            {
                FileStat self = (FileStat)recv;
                FileStat other = (FileStat)p1;

                return self.stat.st_mtime.CompareTo(other.stat.st_mtime);
            }

            return null;
        }        
    }


    
    internal class rb_stat_dev : MethodBody0 // author: cjs, status: done
    {
        internal static rb_stat_dev singleton = new rb_stat_dev();

        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {
            FileStat self = (FileStat)recv;

            return self.stat.st_dev;
        }
    }


    
    internal class rb_stat_dev_major : MethodBody0  // author: cjs, status: done
    {
        internal static rb_stat_dev_major singleton = new rb_stat_dev_major();

        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {
            return null;
        }
    }

    
    internal class rb_stat_dev_minor : MethodBody0 // author: cjs, status: done
    {
        internal static rb_stat_dev_minor singleton = new rb_stat_dev_minor();

        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {
            return null;
        }
    }

    
    internal class rb_stat_ino : MethodBody0 // author: cjs, status: done
    {
        internal static rb_stat_ino singleton = new rb_stat_ino();

        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {
            FileStat self = (FileStat)recv;

            return self.stat.st_ino;
        }
    }

    
    internal class rb_stat_mode : MethodBody0 // author: cjs, status: done
    {
        internal static rb_stat_mode singleton = new rb_stat_mode();

        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {
            FileStat self = (FileStat)recv;
            
            return self.stat.st_mode;
        }
    }

    
    internal class rb_stat_nlink : MethodBody0 // author: cjs, status: done
    {
        internal static rb_stat_nlink singleton = new rb_stat_nlink();

        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {
            FileStat self = (FileStat)recv;

            return self.stat.st_nlink;
        }
    }

    
    internal class rb_stat_uid : MethodBody0 // author: cjs, status: done
    {
        internal static rb_stat_uid singleton = new rb_stat_uid();

        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {
            FileStat self = (FileStat)recv;

            return self.stat.st_uid;
        }
    }

    
    internal class rb_stat_gid : MethodBody0 // author: cjs, status: done
    {
        internal static rb_stat_gid singleton = new rb_stat_gid();

        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {
            FileStat self = (FileStat)recv;

            return self.stat.st_gid;
        }
    }

    
    internal class rb_stat_rdev : MethodBody0 // author: cjs, status: done
    {
        internal static rb_stat_rdev singleton = new rb_stat_rdev();

        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {
            return null;
        }
    }

    
    internal class rb_stat_rdev_major : MethodBody0 // author: cjs, status: done
    {
        internal static rb_stat_rdev_major singleton = new rb_stat_rdev_major();

        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {
            return null;
        }
    }


    
    internal class rb_stat_rdev_minor : MethodBody0 // author: cjs, status: done
    {
        internal static rb_stat_rdev_minor singleton = new rb_stat_rdev_minor();

        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {
            return null;
        }
    }


    
    internal class rb_stat_size : MethodBody0 // author: cjs, status: done
    {
        internal static rb_stat_size singleton = new rb_stat_size();

        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {
            FileStat self = (FileStat)recv;

            return self.stat.st_size;
        }
    }


    
    internal class rb_stat_blksize : MethodBody0// author: cjs, status: done
    {
        internal static rb_stat_blksize singleton = new rb_stat_blksize();

        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {
            return null;
        }
    }

    
    internal class rb_stat_blocks : MethodBody0 // author: cjs, status: done
    {
        internal static rb_stat_blocks singleton = new rb_stat_blocks();

        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {
            return null;
        }
    }

    
    internal class rb_stat_atime : MethodBody0 // author: cjs, status: done
    {
        internal static rb_stat_atime singleton = new rb_stat_atime();

        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {
            FileStat self = (FileStat)recv;

            return new Time(self.stat.st_atime);
        }
    }

    
    internal class rb_stat_mtime : MethodBody0 // author: cjs, status: done
    {
        internal static rb_stat_mtime singleton = new rb_stat_mtime();

        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {
            FileStat self = (FileStat)recv;

            return new Time(self.stat.st_mtime);
        }
    }

    
    internal class rb_stat_ctime : MethodBody0 // author: cjs, status: done
    {
        internal static rb_stat_ctime singleton = new rb_stat_ctime();

        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {
            FileStat self = (FileStat)recv;

            return new Time(self.stat.st_ctime);
        }
    }

    
    internal class rb_stat_inspect : MethodBody0 // author: cjs, status: done
    {
        internal static rb_stat_inspect singleton = new rb_stat_inspect();

        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {
            FileStat self = (FileStat)recv;

            System.Text.StringBuilder str = new System.Text.StringBuilder();
            str.Append("#<");

            str.Append(((Class)recv)._name + " ");
            str.Append("dev=0x" + self.stat.st_dev.ToString("1x") + " ");
            str.Append("ino=" + self.stat.st_ino.ToString() + " ");
            str.Append("mode=0" + self.stat.st_mode.ToString("1o") + " ");
            str.Append("nlink=" + self.stat.st_nlink.ToString() + " ");
            str.Append("uid=" + self.stat.st_uid.ToString() + " ");
            str.Append("gid=" + self.stat.st_gid.ToString() + " ");
            str.Append("rdev=nil ");
            str.Append("size=" + self.stat.st_size.ToString() + " ");
            str.Append("blocks=nil ");
            str.Append("blksize=nil ");
            str.Append("atime=" + self.stat.st_atime.ToString() + " ");
            str.Append("mtime=" + self.stat.st_mtime.ToString() + " ");
            str.Append("ctime=" + self.stat.st_ctime.ToString() + " ");
            
            str.Append(">");

            String result = new String(str.ToString());
            result.Tainted |= self.Tainted;

            return result;
        }
    }


    
    internal class rb_stat_ftype : MethodBody0 // author: cjs, status: done
    {
        internal static rb_stat_ftype singleton = new rb_stat_ftype();

        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {
            FileStat self = (FileStat)recv;

            return File.rb_file_ftype(self.stat);
        }
    }


    
    internal class rb_stat_d : MethodBody0 // author: cjs, status: done
    {
        internal static rb_stat_d singleton = new rb_stat_d();

        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {
            FileStat self = (FileStat)recv;

            return self.stat.fsi is System.IO.DirectoryInfo;
        }
    }


    
    internal class rb_stat_r : MethodBody0 // author: cjs, status: done
    {
        internal static rb_stat_r singleton = new rb_stat_r();

        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {
            FileStat self = (FileStat)recv;

            return FileTest.access(self.stat.fsi.FullName, System.Security.AccessControl.FileSystemRights.Read);
        }
    }


    
    internal class rb_stat_R : MethodBody0 // author: cjs, status: done
    {
        internal static rb_stat_R singleton = new rb_stat_R();

        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {
            FileStat self = (FileStat)recv;

            return FileTest.access(self.stat.fsi.FullName, System.Security.AccessControl.FileSystemRights.Read);
        }
    }

    
    internal class rb_stat_w : MethodBody0 // author: cjs, status: done
    {
        internal static rb_stat_w singleton = new rb_stat_w();

        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {
            FileStat self = (FileStat)recv;

            return FileTest.access(self.stat.fsi.FullName, System.Security.AccessControl.FileSystemRights.Write);
        }
    }


    
    internal class rb_stat_W : MethodBody0 // author: cjs, status: done
    {
        internal static rb_stat_W singleton = new rb_stat_W();

        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {
            FileStat self = (FileStat)recv;

            return FileTest.access(self.stat.fsi.FullName, System.Security.AccessControl.FileSystemRights.Write);
        }
    }


    
    internal class rb_stat_x : MethodBody0 // author: cjs, status: done
    {
        internal static rb_stat_x singleton = new rb_stat_x();

        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {
            FileStat self = (FileStat)recv;

            return FileTest.access(self.stat.fsi.FullName, System.Security.AccessControl.FileSystemRights.ExecuteFile);
        }
    }


    
    internal class rb_stat_X : MethodBody0 // author: cjs, status: done
    {
        internal static rb_stat_X singleton = new rb_stat_X();

        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {
            FileStat self = (FileStat)recv;

            return FileTest.access(self.stat.fsi.FullName, System.Security.AccessControl.FileSystemRights.ExecuteFile);
        }
    }


    
    internal class rb_stat_f : MethodBody0 // author: cjs, status: done
    {
        internal static rb_stat_f singleton = new rb_stat_f();

        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {
            FileStat self = (FileStat)recv;

            return self.stat.fsi is System.IO.FileInfo;
        }
    }


    
    internal class rb_stat_z : MethodBody0 // author: cjs, status: done
    {
        internal static rb_stat_z singleton = new rb_stat_z();

        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {
            FileStat self = (FileStat)recv;

            return ((long)self.stat.st_size) == 0;
        }
    }


    
    internal class rb_stat_s : MethodBody0 // author: cjs, status: done
    {
        internal static rb_stat_s singleton = new rb_stat_s();

        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {
            FileStat self = (FileStat)recv;

            return self.stat.st_size;
        }
    }


    
    internal class rb_stat_owned : MethodBody0 // author: cjs, status: done
    {
        internal static rb_stat_owned singleton = new rb_stat_owned();

        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {
            FileStat self = (FileStat)recv;

            System.Security.AccessControl.FileSecurity fs = new System.Security.AccessControl.FileSecurity(self.stat.fsi.FullName, System.Security.AccessControl.AccessControlSections.Owner);

            return System.Security.Principal.WindowsIdentity.GetCurrent().Name.Equals(fs.GetOwner(typeof(System.Security.Principal.NTAccount)).Value);
        }
    }



    
    internal class rb_stat_grpowned : MethodBody0 // author: cjs, status: done
    {
        internal static rb_stat_grpowned singleton = new rb_stat_grpowned();

        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {
            return false;
        }
    }


    
    internal class rb_stat_p : MethodBody0 // author: cjs, status: done
    {
        internal static rb_stat_p singleton = new rb_stat_p();

        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {
            return false;
        }
    }


    
    internal class rb_stat_l : MethodBody0 // author: cjs, status: done
    {
        internal static rb_stat_l singleton = new rb_stat_l();

        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {
            return false;
        }
    }


    
    internal class rb_stat_S : MethodBody0 // author: cjs, status: done
    {
        internal static rb_stat_S singleton = new rb_stat_S();

        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {
            return false;
        }
    }


    
    internal class rb_stat_b : MethodBody0 // author: cjs, status: done
    {
        internal static rb_stat_b singleton = new rb_stat_b();

        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {
            FileStat self = (FileStat)recv;

            return false;
        }
    }



    
    internal class rb_stat_c : MethodBody0 // author: cjs, status: done
    {
        internal static rb_stat_c singleton = new rb_stat_c();

        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {
            FileStat self = (FileStat)recv;

            return false;
        }
    }


    
    internal class rb_stat_suid : MethodBody0 // author: cjs, status: done
    {
        internal static rb_stat_suid singleton = new rb_stat_suid();

        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {
            return false;
        }
    }


    
    internal class rb_stat_sgid : MethodBody0 // author: cjs, status: done
    {
        internal static rb_stat_sgid singleton = new rb_stat_sgid();

        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {
            return false;
        }
    }

    
    internal class rb_stat_sticky : MethodBody0 // author: cjs, status: done
    {
        internal static rb_stat_sticky singleton = new rb_stat_sticky();

        public override object Call0(Class last_class, object recv, Frame caller, Proc block)
        {
            return false;
        }
    }
}
