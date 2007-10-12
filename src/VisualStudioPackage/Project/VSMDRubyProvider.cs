using Microsoft.VisualStudio.Designer.Interfaces;
using System;


namespace VSRuby.NET
{
    public class VSMDRubyProvider : IVSMDCodeDomProvider
	{
		private RubyProvider provider;

		public VSMDRubyProvider(RubyFileNode file)
		{
            provider = new RubyProvider(file);
		}

		object IVSMDCodeDomProvider.CodeDomProvider
		{
			get { return provider; }
		}
	}
}