using System;
using System.Globalization;
using System.IO;
using System.Windows.Forms;
using System.Drawing;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Package;
using Microsoft.VisualStudio.Shell;


namespace Ruby.NET //Microsoft.Samples.VisualStudio.IDE.Project
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
            this.SupportsProjectDesigner = true;

            ImageOffset = this.ImageHandler.ImageList.Images.Count;
            foreach (Image img in RubyImageList.Images)
                this.ImageHandler.AddImage(img);
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

        public override int Close()
        {
            if (listener != null)
            {
                listener.Dispose();
                listener = null;
            }

            return base.Close();
        }

        public override FileNode CreateFileNode(ProjectElement item)
        {
            return new RubyFileNode(this, item);
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
