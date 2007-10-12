using Microsoft.VisualStudio.Package.Automation;
using Microsoft.VisualStudio.Package;
using System.Runtime.InteropServices;


namespace VSRuby.NET
{
    [ComVisible(true)]
    [Guid("68343E6A-7789-4a58-8DCC-AE9887D3BE60")]
    public class RubyFileNodeAutomation : OAFileItem
	{
        public RubyFileNodeAutomation(RubyProjectAutomation project, FileNode node)
            : base(project, node)
		{
		}

        public override EnvDTE.Window Open(string viewKind)
        {
            if (viewKind == EnvDTE.Constants.vsViewKindPrimary && ((RubyFileNode)Node).SubType == "Form")
                return base.Open(EnvDTE.Constants.vsViewKindDesigner);
            else
                return base.Open(viewKind);
        }
	}
}
