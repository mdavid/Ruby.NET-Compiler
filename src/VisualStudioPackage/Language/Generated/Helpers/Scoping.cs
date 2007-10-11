/**********************************************************************

  VSRuby.NET Runtime Library
  Originally developed at Queensland University of Technology
 
**********************************************************************/


using System.Collections.Generic;

namespace VSRuby.NET
{
    public class Scope
    {
        public List<string> locals_list = new List<string>();
        public Scope block_parent;

        public Scope()
        {
            this.block_parent = null;
        }

        public Scope(Scope block_parent)
        {
            this.block_parent = block_parent;
        }


        public bool has_local(string id)
        {
            if (locals_list.Contains(id))
                return true;
            else if (block_parent != null)
                return block_parent.has_local(id);
            else
                return false;
        }


        public virtual void add_local(string id)
        {
            locals_list.Add(id);
        }
    }
}