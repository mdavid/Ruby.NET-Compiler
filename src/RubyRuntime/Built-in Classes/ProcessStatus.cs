using Ruby;
using System.Collections.Generic;
using System.Text;
using Ruby.Runtime;

namespace Ruby
{

    public partial class ProcessStatus : Object
    {
        internal ProcessStatus(System.Diagnostics.Process process)
            : base(Ruby.Runtime.Init.rb_cProcStatus)
        {
            //Ruby process status is 16-bit integer:
            //High-order bits: exit-code
            //Low-order bits : 0x00 if process has exited
            //                 0x7f if process is stopped
            //                 other if process is running
            if (process.HasExited)
                this.st = 0;
            else
            {
                this.st = 0x7f;
                foreach (System.Threading.Thread t in process.Threads)
                {
                    if (t.ThreadState == System.Threading.ThreadState.Running)
                    {
                        this.st = 1;
                        break;
                    }
                }
            }

            this.st |= process.ExitCode << 8;
            this.pid = process.Id;
        }

        static void last_status_set(System.Diagnostics.Process process)
        {
            Process.rb_last_status.value = new ProcessStatus(process);
        }
    
        internal int pid;
        internal int st;

        internal override object Inner()
        {
            return st;
        }

        internal bool WIFEXITED()
        {
            return (st & 0xff) == 0;
        }

        internal bool WIFSIGNALED()
        {
            return false;
        }

        internal bool WIFSTOPPED()
        {
            return (st & 0xff) == 0x7f;
        }

        internal int WEXITSTATUS()
        {
            return (st >> 8) & 0xff;
        }

        internal int WSTOPSIG()
        {
            return WEXITSTATUS();
        }

        internal int WTERMSIG()
        {
            return st & 0x7f;
        }
    }
}
