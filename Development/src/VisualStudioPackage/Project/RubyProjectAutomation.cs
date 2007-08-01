using Microsoft.VisualStudio.Package;
using Microsoft.VisualStudio.Package.Automation;

namespace Ruby.NET
{
    public class RubyProjectAutomation : OAProject
    {
        public RubyProjectAutomation(ProjectNode project): base(project)
        {
        }
    }
}