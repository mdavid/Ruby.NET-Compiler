using System;
using Microsoft.VisualStudio.Package;
using System.ComponentModel;
using System.Runtime.InteropServices;


namespace VSRuby.NET
{
    [ComVisible(true), CLSCompliant(false), System.Runtime.InteropServices.ClassInterface(ClassInterfaceType.AutoDual)]
    [Guid("AA249C33-4246-48f2-A263-822061B2A043")]
    public class RubyProjectNodeProperties : ProjectNodeProperties
	{
        public RubyProjectNodeProperties(ProjectNode node)
            : base(node)
        {
        }


        [Browsable(false)]
        public string OutputFileName
        {
            get
            {
                return ((RubyProjectNode)(this.Node.ProjectMgr)).OutputFileName;
            }
        }

        [Browsable(false)]
        public string MainFile
        {
            get
            {
                return this.Node.ProjectMgr.GetProjectProperty("MainFile", true);
            }
        }

        [Browsable(false)]
        public string AssemblyName
        {
            get
            {
                return this.Node.ProjectMgr.GetProjectProperty("AssemblyName");
            }
            set
            {
                this.Node.ProjectMgr.SetProjectProperty("AssemblyName", value);
            }
        }

        [Browsable(false)]
        public string DefaultNamespace
        {
            get
            {
                return this.Node.ProjectMgr.GetProjectProperty("RootNamespace");
            }
            set
            {
                this.Node.ProjectMgr.SetProjectProperty("RootNamespace", value);
            }
        }

        [Browsable(false)]
        public string RootNamespace
        {
            get
            {
                return this.Node.ProjectMgr.GetProjectProperty("RootNamespace");
            }
            set
            {
                this.Node.ProjectMgr.SetProjectProperty("RootNamespace", value);
            }
        }

        [Browsable(false)]
        public string OutputType
        {
            get
            {
                return this.Node.ProjectMgr.GetProjectProperty("OutputType");
            }
            set
            {
                this.Node.ProjectMgr.SetProjectProperty("OutputType", value);
            }
        }
	}
}
