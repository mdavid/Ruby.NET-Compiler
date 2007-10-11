using Microsoft.VisualStudio.Package;
using Microsoft.VisualStudio.Package.Automation;
using System.Runtime.InteropServices;

namespace VSRuby.NET
{
    [ComVisible(true)]
    public class RubyProjectAutomation : OAProject
    {
        public RubyProjectAutomation(RubyProjectNode project): base(project)
        {
        }
    }
}