using System;
using Microsoft.VisualStudio.Package;
using System.ComponentModel;
using System.Runtime.InteropServices;


namespace VSRuby.NET
{
    [ComVisible(true), CLSCompliant(false)]
    [Guid("0A3F2FC2-EF81-482e-987D-CA4711577713")]
	public class RubyFileNodeProperties: SingleFileGeneratorNodeProperties
	{
        public RubyFileNodeProperties(HierarchyNode node)
            : base(node)
        {
        }

        [Browsable(false)]
        public string Url
        {
            get
            {
                return "file:///" + this.Node.Url;
            }
        }

        [Browsable(false)]
        public string SubType
        {
            get
            {
                return ((RubyFileNode)this.Node).SubType;
            }
            set
            {
                ((RubyFileNode)this.Node).SubType = value;
            }
        }
        
	}
}
