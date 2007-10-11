using System;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.Package;
using IOleServiceProvider = Microsoft.VisualStudio.OLE.Interop.IServiceProvider;

namespace VSRuby.NET
{
    [GuidAttribute("1BD65B90-FE85-4f2a-9DCB-8318FF019233")]
    public class RubyProjectFactory : ProjectFactory
    {
        public RubyProjectFactory(RubyPackage package): base(package)
        {
        }

        protected override Microsoft.VisualStudio.Package.ProjectNode CreateProject()
        {
            RubyProjectNode project = new RubyProjectNode();
            project.SetSite((IOleServiceProvider)((IServiceProvider)this.Package).GetService(typeof(IOleServiceProvider)));
            return project;
        }
    }
}
