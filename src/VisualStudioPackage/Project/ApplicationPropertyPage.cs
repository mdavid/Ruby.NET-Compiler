using System;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Package;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.Shell;
using System.ComponentModel;


namespace VSRuby.NET
{
    [ComVisible(true), Guid("9899AEF5-D58F-48ff-9185-A2D4B78E517B")]
    public class ApplicationPropertyPage : SettingsPage
    {
        public ApplicationPropertyPage()
        {

            Name = "Application";
        }

        private string assemblyName;
        private OutputType outputType;

        [LocDisplayName("Assembly name:")]
        public string AssemblyName
        {
            get { return this.assemblyName; }
            set { this.assemblyName = value; this.IsDirty = true; }
        }

        [LocDisplayName("Output type:")]
        public OutputType OutputType
        {
            get { return this.outputType; }
            set { this.outputType = value; this.IsDirty = true; }
        }


        public override string GetClassName()
        {
            return GetType().FullName;
        }

        protected override void BindProperties()
        {
            assemblyName = ProjectMgr.GetProjectProperty("AssemblyName", true);

            string outputType = ProjectMgr.GetProjectProperty("OutputType", false);

            if (outputType != null && outputType.Length > 0)
            {
                try
                {
                    this.outputType = (OutputType)Enum.Parse(typeof(OutputType), outputType);
                }
                catch (ArgumentException)
                {
                }
            }
        }

        protected override int ApplyChanges()
        {
            ProjectMgr.SetProjectProperty("AssemblyName", assemblyName);
            ProjectMgr.SetProjectProperty("OutputType", outputType.ToString());

            IsDirty = false;
            return VSConstants.S_OK;
        }
    }
}