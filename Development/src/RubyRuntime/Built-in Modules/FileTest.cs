/**********************************************************************

  Ruby.NET Runtime Library
  Originally developed at Queensland University of Technology
 
  Some sections of this C# code mirror the structure of the C code in the
  Ruby 1.8.2 Interpreter Copyright (C) 1993-2003 Yukihiro Matsumoto, et.al.
  
**********************************************************************/

using Ruby.Runtime;

namespace Ruby
{

    public partial class FileTest
    {
        private static void define_filetest_function(string name, MethodBody body, int arity)
        {
            // BBTAG: someone may need to pass in a proper caller parameter here
            Ruby.Runtime.Init.rb_mFileTest.define_module_function(name, body, arity, null);
        }

        internal static System.Security.AccessControl.FileSecurity getacl(string path)
        {
            return System.IO.File.GetAccessControl(path);
        }
        
        internal static bool access(string path, System.Security.AccessControl.FileSystemRights right)
        {
            try
            {
                System.Security.AccessControl.FileSecurity acl = getacl(path);
                System.Security.AccessControl.AuthorizationRuleCollection arc = acl.GetAccessRules(true, true, typeof(System.Security.Principal.SecurityIdentifier));
                foreach (System.Security.AccessControl.AuthorizationRule ar in arc)
                {
                    if (ar.Equals(right))
                        return true;
                }
                return false;
            }
            catch // (System.Security.SecurityException e)
            {
                return false;
            }
        }

        internal static bool owned(string path)
        {
            try
            {
                System.Security.AccessControl.FileSecurity acl = getacl(path);
                System.Security.Principal.IdentityReference owner = acl.GetOwner(typeof(System.Security.Principal.SecurityIdentifier));
                if (System.Security.Principal.WindowsIdentity.GetCurrent().User.Equals(owner))
                    return true;
                else
                    return false;
            }
            catch // (System.Security.SecurityException e)
            {
                return false;
            }
        }
    }
}
