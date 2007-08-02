/**********************************************************************

  Ruby.NET Runtime Library
  Originally developed at Queensland University of Technology
 
  Some sections of this C# code mirror the structure of the C code in the
  Ruby 1.8.2 Interpreter Copyright (C) 1993-2003 Yukihiro Matsumoto, et.al.
  
**********************************************************************/

using System.Collections;
using System.Collections.Generic;
using System;

namespace Ruby
{

    public partial class ObjectSpace
    {
        internal static ArrayList _finalizers = new ArrayList();       // deprecated finalizers table

        internal static List<WeakReference> objects = new List<WeakReference>();

        public static Dictionary<int, object> id_lookup = new Dictionary<int, object>();

        public static int obj2id(object o)
        {
            int id = o.GetHashCode();
            
            if (!id_lookup.ContainsKey(id))
                id_lookup.Add(id, o);

            return id;
        }
    }
}
