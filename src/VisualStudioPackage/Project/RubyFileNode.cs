using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.Package;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio;
using System.IO;
using VsCommands = Microsoft.VisualStudio.VSConstants.VSStd97CmdID;


namespace VSRuby.NET
{
    public class RubyFileNode: FileNode
    {
        public RubyFileNode(ProjectNode root, ProjectElement e)
            : base(root, e)
        {
        }

        public string SubType
        {
            get
            {
                return ItemNode.GetMetadata(ProjectFileConstants.SubType);
            }
            set
            {
                ItemNode.SetMetadata(ProjectFileConstants.SubType, value);
            }
        }


        private RubyFileNodeAutomation automationObject;

        public override object GetAutomationObject()
        {
            if (null == automationObject)
            {
                automationObject = new RubyFileNodeAutomation(this.ProjectMgr.GetAutomationObject() as RubyProjectAutomation, this);
            }
            return automationObject;
        }

        public override int ImageIndex
        {
            get
            {
                if (SubType == "Form")
                    return (int)ProjectNode.ImageName.WindowsForm;
                if (this.FileName.ToLower().EndsWith(".rb"))
                    return RubyProjectNode.ImageOffset + 0;
                else
                    return base.ImageIndex;
            }
        }

        protected override void DoDefaultAction()
        {
            FileDocumentManager manager = this.GetDocumentManager() as FileDocumentManager;
            Guid viewGuid = (SubType == "Form" ? VSConstants.LOGVIEWID_Designer : VSConstants.LOGVIEWID_Code);
            IVsWindowFrame frame;
            manager.Open(false, false, viewGuid, out frame, WindowFrameShowAction.Show);
        }

        protected override int QueryStatusOnNode(Guid guidCmdGroup, uint cmd, IntPtr pCmdText, ref QueryStatusResult result)
        {
            if (guidCmdGroup == Microsoft.VisualStudio.Shell.VsMenus.guidStandardCommandSet97)
            {
                switch ((VsCommands)cmd)
                {
                    case VsCommands.AddNewItem:
                    case VsCommands.AddExistingItem:
                    case VsCommands.ViewCode:
                        result |= QueryStatusResult.SUPPORTED | QueryStatusResult.ENABLED;
                        return VSConstants.S_OK;
                    case VsCommands.ViewForm:
                        if (SubType == "Form")
                            result |= QueryStatusResult.SUPPORTED | QueryStatusResult.ENABLED;
                        return VSConstants.S_OK;
                }
            }
            else if (guidCmdGroup == RubyMenus.guidRubyProjectCmdSet && cmd == RubyMenus.SetAsMain.ID)
            {
                result |= QueryStatusResult.SUPPORTED | QueryStatusResult.ENABLED;
                return VSConstants.S_OK;
            }
 
            return base.QueryStatusOnNode(guidCmdGroup, cmd, pCmdText, ref result);
        }

        public string GetRelativePath()
        {
            string path = this.Caption;
            for (HierarchyNode node = this.Parent; node != null && !(node is ProjectNode); node = node.Parent)
                path = Path.Combine(node.Caption, path);

            return path;
        }

        protected override int ExecCommandOnNode(Guid guidCmdGroup, uint cmd, uint nCmdexecopt, IntPtr pvaIn, IntPtr pvaOut)
        {
            if (guidCmdGroup == RubyMenus.guidRubyProjectCmdSet && cmd == RubyMenus.SetAsMain.ID)
            {
                ((RubyProjectNode)ProjectMgr).SetProjectProperty("MainFile", this.GetRelativePath());
                return VSConstants.S_OK;
            }

            return base.ExecCommandOnNode(guidCmdGroup, cmd, nCmdexecopt, pvaIn, pvaOut);
        }

        protected override NodeProperties CreatePropertiesObject()
        {
            RubyFileNodeProperties properties = new RubyFileNodeProperties(this);   
            return properties;
        }

        internal OleServiceProvider.ServiceCreatorCallback ServiceCreator
        {
            get { return new OleServiceProvider.ServiceCreatorCallback(this.CreateServices); }
        }

        private object CreateServices(Type serviceType)
        {
            object service = null;
            if (typeof(EnvDTE.ProjectItem) == serviceType)
            {
                service = GetAutomationObject();
            }
            if (typeof(SVSMDCodeDomProvider) == serviceType)
            {
                return new VSMDRubyProvider(null);
            }
            return service;
        }
    }
}
