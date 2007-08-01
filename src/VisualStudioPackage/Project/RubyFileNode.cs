using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.Package;
using Microsoft.VisualStudio;
using System.IO;


namespace Ruby.NET
{
    public class RubyFileNode: FileNode
    {
        public RubyFileNode(ProjectNode root, ProjectElement e)
            : base(root, e)
        {
        }

        public override void ReDraw(UIHierarchyElement element)
        {
            base.ReDraw(element);
        }

        public override int ImageIndex
        {
            get
            {
                if (this.FileName.ToLower().EndsWith(".rb"))
                    return RubyProjectNode.ImageOffset + 0;
                else
                    return base.ImageIndex;
            }
        }

        protected override int QueryStatusOnNode(Guid guidCmdGroup, uint cmd, IntPtr pCmdText, ref QueryStatusResult result)
        {
            if (guidCmdGroup == RubyMenus.guidRubyProjectCmdSet && cmd == RubyMenus.SetAsMain.ID)
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
    }
}
