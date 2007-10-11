using System;
using System.Globalization;
using System.IO;
using System.Windows.Forms;
using System.Drawing;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Package;
using Microsoft.VisualStudio.Shell;


namespace VSRuby.NET //Microsoft.Samples.VisualStudio.IDE.Project
{
    [GuidAttribute("07E91719-6969-4d9a-A2CF-79CE562F1D99")]
    public class RubyProjectNode : ProjectNode
    {
        private static ImageList RubyImageList;
        internal static int ImageOffset;


        static RubyProjectNode()
        {
            //string[] names = typeof(RubyProjectNode).Assembly.GetManifestResourceNames();
            RubyImageList = Utilities.GetImageList(typeof(RubyProjectNode).Assembly.GetManifestResourceStream("Project.Resources.RubyImageList.bmp"));
        }

        public RubyProjectNode()
        {
            this.OleServiceProvider.AddService(typeof(VSLangProj.VSProject), new OleServiceProvider.ServiceCreatorCallback(this.CreateServices), false);
            this.SupportsProjectDesigner = true;

            ImageOffset = this.ImageHandler.ImageList.Images.Count;
            foreach (Image img in RubyImageList.Images)
                this.ImageHandler.AddImage(img);

            InitializeCATIDs();
        }

        private void InitializeCATIDs()
        {
            this.AddCATIDMapping(typeof(RubyProjectNodeProperties), typeof(RubyProjectNodeProperties).GUID);
            this.AddCATIDMapping(typeof(RubyFileNodeProperties), typeof(RubyFileNodeProperties).GUID);
            this.AddCATIDMapping(typeof(RubyFileNodeAutomation), typeof(RubyFileNodeAutomation).GUID);
            this.AddCATIDMapping(typeof(FolderNodeProperties), new Guid("DD3E8FBF-46E5-4e01-9488-FF4254633549"));
            this.AddCATIDMapping(typeof(FileNodeProperties), typeof(RubyFileNodeProperties).GUID);
        }




        public string OutputFileName
        {
            get
            {
                string assemblyName = this.ProjectMgr.GetProjectProperty("AssemblyName", true);

                string outputTypeAsString = this.ProjectMgr.GetProjectProperty("OutputType", false);
                OutputType outputType = (OutputType)Enum.Parse(typeof(OutputType), outputTypeAsString);

                return assemblyName + GetOuputExtension(outputType);
            }
        }

        public static string GetOuputExtension(OutputType outputType)
        {
            if (outputType == OutputType.Library)
                return ".dll";
            else
                return ".exe";
        }

        public override string ProjectType
        {
            get { return this.GetType().Name; }
        }

        public override Guid ProjectGuid
        {
            get { return typeof(RubyProjectFactory).GUID; }
        }

        public override int ImageIndex
        {
            get
            {
                return ImageOffset + 1;
            }
        }

        private ProjectDocumentsListenerForMainFileUpdates listener;

        public override int SetSite(Microsoft.VisualStudio.OLE.Interop.IServiceProvider site)
        {
            base.SetSite(site);

            listener = new ProjectDocumentsListenerForMainFileUpdates((ServiceProvider)this.Site, this);
            listener.Init();

            return VSConstants.S_OK;
        }

        protected override NodeProperties CreatePropertiesObject()
        {
            return new RubyProjectNodeProperties(this);
        }

        public override int Close()
        {
            if (listener != null)
            {
                listener.Dispose();
                listener = null;
            }

            return base.Close();
        }


        private VSLangProj.VSProject vsProject = null;

        protected internal VSLangProj.VSProject VSProject
        {
            get
            {
                if (vsProject == null)
                    vsProject = new Microsoft.VisualStudio.Package.Automation.OAVSProject(this);
                return vsProject;
            }
        }


        private object CreateServices(Type serviceType)
        {
            if (typeof(VSLangProj.VSProject) == serviceType)
            {
                return VSProject;
            }
            else if (typeof(EnvDTE.Project) == serviceType)
            {
                return GetAutomationObject();
            }
            else
                return null;
        }


        public override FileNode CreateFileNode(ProjectElement item)
        {
            RubyFileNode newNode = new RubyFileNode(this, item);

            string include = item.GetMetadata(ProjectFileConstants.Include);

            newNode.OleServiceProvider.AddService(typeof(EnvDTE.Project), new OleServiceProvider.ServiceCreatorCallback(CreateServices), false);
            newNode.OleServiceProvider.AddService(typeof(EnvDTE.ProjectItem), newNode.ServiceCreator, false);
            newNode.OleServiceProvider.AddService(typeof(VSLangProj.VSProject), new OleServiceProvider.ServiceCreatorCallback(CreateServices), false);

            if (IsCodeFile(include))
                newNode.OleServiceProvider.AddService(typeof(SVSMDCodeDomProvider), newNode.ServiceCreator, false);
                // new OleServiceProvider.ServiceCreatorCallback(CreateServices), false);

            return newNode;
        }

        public override bool IsCodeFile(string strFileName)
        {
            if (String.IsNullOrEmpty(strFileName))
                return false;
            else
                return (String.Compare(Path.GetExtension(strFileName), ".rb", StringComparison.OrdinalIgnoreCase) == 0);
        }

        public override object GetAutomationObject()
        {  
            return new RubyProjectAutomation(this);
        }

        protected  override Guid[] GetConfigurationIndependentPropertyPages()
        {
            Guid[] result = new Guid[1];
            result[0] = typeof(ApplicationPropertyPage).GUID;
            return result;
        }

        protected override Guid[] GetPriorityProjectDesignerPages()
        {
            Guid[] result = new Guid[2];
            result[0] = typeof(ApplicationPropertyPage).GUID;
            result[1] = typeof(BuildPropertyPage).GUID;
            return result;
        }

        protected override Guid[] GetConfigurationDependentPropertyPages()
        {
            Guid[] result = new Guid[1];
            result[0] = typeof(BuildPropertyPage).GUID;
            return result;
        }

        public override void AddFileFromTemplate(string source, string target)
        {
            if (!File.Exists(source))
                throw new FileNotFoundException(string.Format("Template file not found: {0}", source));

            string fileName = Path.GetFileNameWithoutExtension(target);
            string nameSpace = this.FileTemplateProcessor.GetFileNamespace(target, this);

            this.FileTemplateProcessor.AddReplace("%className%", fileName);
            this.FileTemplateProcessor.AddReplace("%namespace%", nameSpace);

            try
            {
                this.FileTemplateProcessor.UntokenFile(source, target);

            }
            catch (Exception exceptionObj)
            {
                throw new FileLoadException("Failed to add template file to project", target, exceptionObj);
            }
        }

        public override int GetFormatList(out string ppszFormatList)
        {   // ???
            ppszFormatList = "Ruby Project File (*.rbproj)*.rbproj";
            return VSConstants.S_OK;
        }
    }
}
