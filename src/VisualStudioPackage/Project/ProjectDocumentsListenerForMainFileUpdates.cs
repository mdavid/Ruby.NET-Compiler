using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Package;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System.Diagnostics;


namespace VSRuby.NET
{
    class ProjectDocumentsListenerForMainFileUpdates : ProjectDocumentsListener
    {
        private RubyProjectNode project;

        public ProjectDocumentsListenerForMainFileUpdates(ServiceProvider serviceProvider, RubyProjectNode project)
            : base(serviceProvider)
        {
            this.project = project;
        }

        public override int OnAfterRenameFiles(int cProjects, int cFiles, IVsProject[] projects, int[] firstIndices, string[] oldFileNames, string[] newFileNames, VSRENAMEFILEFLAGS[] flags)
        {
            //Get the current value of the MainFile Property
            string currentMainFile = this.project.GetProjectProperty("MainFile", true);
            string fullPathToMainFile = Path.Combine(Path.GetDirectoryName(this.project.BaseURI.Uri.LocalPath), currentMainFile);

            //Investigate all of the oldFileNames if they belong to the current project and if they are equal to the current MainFile
            int index = 0;
            foreach (string oldfile in oldFileNames)
            {
                //Compare this project with the project that the old file belongs to
                IVsProject belongsToProject = projects[firstIndices[index]];
                if (Utilities.IsSameComObject(belongsToProject, this.project))
                {
                    //Compare the files and update the MainFile Property if the currentMainFile is an old file
                    if (IsSamePath(oldfile, fullPathToMainFile))
                    {
                        //Get the newfilename and update the MainFile property
                        string newfilename = newFileNames[index];
                        RubyFileNode node = FindChild(project, newfilename) as RubyFileNode;
                        if (node == null)
                            throw new InvalidOperationException("Could not find the RubyFileNode object");
                        this.project.SetProjectProperty("MainFile", node.GetRelativePath());
                        break;
                    }
                }

                index++;
            }

            return VSConstants.S_OK;
        }

        public override int OnAfterRemoveFiles(int cProjects, int cFiles, IVsProject[] projects, int[] firstIndices, string[] oldFileNames, VSREMOVEFILEFLAGS[] flags)
        {
            //Get the current value of the MainFile Property
            string currentMainFile = this.project.GetProjectProperty("MainFile", true);
            string fullPathToMainFile = Path.Combine(Path.GetDirectoryName(this.project.BaseURI.Uri.LocalPath), currentMainFile);

            //Investigate all of the oldFileNames if they belong to the current project and if they are equal to the current MainFile
            int index = 0;
            foreach (string oldfile in oldFileNames)
            {
                //Compare this project with the project that the old file belongs to
                IVsProject belongsToProject = projects[firstIndices[index]];
                if (Utilities.IsSameComObject(belongsToProject, this.project))
                {
                    //Compare the files and update the MainFile Property if the currentMainFile is an old file
                    if (IsSamePath(oldfile, fullPathToMainFile))
                    {
                        //Get the first available rb file in the project and update the MainFile property
                        List<RubyFileNode> RubyFileNodes = new List<RubyFileNode>();
                        FindNodesOfType<RubyFileNode>(project, RubyFileNodes);
                        string newMainFile = string.Empty;
                        if (RubyFileNodes.Count > 0)
                        {
                            newMainFile = RubyFileNodes[0].GetRelativePath();
                        }
                        this.project.SetProjectProperty("MainFile", newMainFile);
                        break;
                    }
                }

                index++;
            }

            return VSConstants.S_OK;
        }

        public override int OnAfterAddFilesEx(int cProjects, int cFiles, IVsProject[] projects, int[] firstIndices, string[] newFileNames, VSADDFILEFLAGS[] flags)
        {
            //Get the current value of the MainFile Property
            string currentMainFile = this.project.GetProjectProperty("MainFile", true);
            if (!string.IsNullOrEmpty(currentMainFile))
                //No need for further operation since MainFile is already set
                return VSConstants.S_OK;

            string fullPathToMainFile = Path.Combine(Path.GetDirectoryName(this.project.BaseURI.Uri.LocalPath), currentMainFile);

            //Investigate all of the newFileNames if they belong to the current project and set the first RubyFileNode found equal to MainFile
            int index = 0;
            foreach (string newfile in newFileNames)
            {
                //Compare this project with the project that the new file belongs to
                IVsProject belongsToProject = projects[firstIndices[index]];
                if (Utilities.IsSameComObject(belongsToProject, this.project))
                {
                    //If the newfile is a Ruby filenode we willl map this file to the MainFile property
                    RubyFileNode filenode = FindChild(project, newfile) as RubyFileNode;
                    if (filenode != null)
                    {
                        this.project.SetProjectProperty("MainFile", filenode.GetRelativePath());
                        break;
                    }
                }

                index++;
            }

            return VSConstants.S_OK;
        }

        public static bool IsSamePath(string file1, string file2)
        {
            if (file1 == null || file1.Length == 0)
            {
                return (file2 == null || file2.Length == 0);
            }

            Uri uri1 = null;
            Uri uri2 = null;

            try
            {
                if (!Uri.TryCreate(file1, UriKind.Absolute, out uri1) || !Uri.TryCreate(file2, UriKind.Absolute, out uri2))
                {
                    return false;
                }

                if (uri1 != null && uri1.IsFile && uri2 != null && uri2.IsFile)
                {
                    return 0 == String.Compare(uri1.LocalPath, uri2.LocalPath, StringComparison.OrdinalIgnoreCase);
                }

                return file1 == file2;
            }
            catch (UriFormatException e)
            {
                Trace.WriteLine("Exception " + e.Message);
            }

            return false;
        }

        internal void FindNodesOfType<T>(HierarchyNode parent, List<T> nodes)
            where T : HierarchyNode
        {
            for (HierarchyNode n = parent.FirstChild; n != null; n = n.NextSibling)
            {
                if (n is T)
                {
                    T nodeAsT = (T)n;
                    nodes.Add(nodeAsT);
                }

                FindNodesOfType<T>(n, nodes);
            }
        }

        static HierarchyNode FindChild(HierarchyNode parent, string name)
        {
            if (String.IsNullOrEmpty(name))
            {
                return null;
            }

            HierarchyNode result;
            for (HierarchyNode child = parent.FirstChild; child != null; child = child.NextSibling)
            {
                if (!String.IsNullOrEmpty(child.VirtualNodeName) && String.Compare(child.VirtualNodeName, name, StringComparison.OrdinalIgnoreCase) == 0)
                {
                    return child;
                }
                // If it is a foldernode then it has a virtual name but we want to find folder nodes by the document moniker or url
                else if ((String.IsNullOrEmpty(child.VirtualNodeName) || (child is FolderNode)) &&
                        (IsSamePath(child.GetMkDocument(), name) || IsSamePath(child.Url, name)))
                {
                    return child;
                }

                result = FindChild(child, name);
                if (result != null)
                {
                    return result;
                }
            }
            return null;
        }
    }
}
