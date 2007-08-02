/**********************************************************************

  Ruby.NET Runtime Library
  Originally developed at Queensland University of Technology
 
  Some sections of this C# code mirror the structure of the C code in the
  Ruby 1.8.2 Interpreter Copyright (C) 1993-2003 Yukihiro Matsumoto, et.al.
  
**********************************************************************/

using Ruby.Runtime;
using Ruby;
using System.Diagnostics;
using System.Collections;

namespace Ruby
{

    public class Basic
    {
        private bool frozen;
        private bool tainted;
        internal Class my_class;

        internal ArrayList finalizers = null;
        internal bool finalize_flag = false;           // for deprecated ObjectSpace finalizers

        // -----------------------------------------------------------------------------

        protected Basic(Class klass)
        {
            this.my_class = klass;
            this.frozen = false;
            this.tainted = false;
            ObjectSpace.objects.Add(new System.WeakReference(this));
        }

        // -----------------------------------------------------------------------------

        internal bool Tainted
        {
            get { return tainted; }
            set { tainted = value; }
        }
        internal bool Frozen
        {
            get { return frozen; }
            set { frozen = value;}
        }

        // ---------------------------------------------------------------------------

        ~Basic()
        {
            if (finalize_flag && ObjectSpace._finalizers != null)
            {
                foreach (object finalizer in ObjectSpace._finalizers)
                {
                    Eval.CallPublic(finalizer, null, "call", null, this.GetHashCode());
                }
            }

            if (finalizers != null)
            {
                foreach (object finalizer in finalizers)
                {
                    Eval.CallPublic(finalizer, null, "call", null, this.GetHashCode());
                }
            }
        }

        internal virtual object Inner()
        {
            return this;
        }

    }
}
