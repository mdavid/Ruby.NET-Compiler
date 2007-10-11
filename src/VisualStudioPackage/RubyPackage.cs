using System;
using System.Text;
using Microsoft.VisualStudio;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Package;


namespace VSRuby.NET
{
    // Generic Package attributes ...
    [DefaultRegistryRoot("Software\\Microsoft\\VisualStudio\\8.0Exp")]
    [Guid("60CC6DB2-7F18-409c-9E28-B69964DCF160")]
    [InstalledProductRegistration(false, "#100", "#101", "0.8.2", IconResourceID = 400)]
    [PackageRegistration(UseManagedResourcesOnly = true)]
    // Project Attributes ...
    [ProvideMenuResource(1000, 1)]
    [ProvideEditorExtension(typeof(EditorFactory), ".rb", 32)]
    [ProvideEditorLogicalView(typeof(EditorFactory), "{7651a702-06e5-11d1-8ebd-00a0c90f26ea}")]  //LOGVIEWID_Designer
    [ProvideEditorLogicalView(typeof(EditorFactory), "{7651a701-06e5-11d1-8ebd-00a0c90f26ea}")]  //LOGVIEWID_Code
    [ProvideProjectFactory(typeof(RubyProjectFactory), "Ruby", "Ruby Files (*.rbproj);*.rbproj", "rbproj", "rbproj", ".\\NullPath", LanguageVsTemplate = "Ruby")]
    [ProvideObject(typeof(ApplicationPropertyPage))]
    [ProvideObject(typeof(BuildPropertyPage))]
    // Language Service Attributes
    [ProvideService(typeof(VSRuby.NET.LanguageService))]
    [ProvideLanguageExtension(typeof(VSRuby.NET.LanguageService), ".rb")]
    [ProvideLanguageService(typeof(VSRuby.NET.LanguageService), "Ruby.NET", 0, CodeSense = true, EnableCommenting = true, MatchBraces = true, ShowCompletion = true, ShowMatchingBrace = true, AutoOutlining = true, EnableAsyncCompletion = true, CodeSenseDelay = 0)]
    public class RubyPackage : VSRuby.NET.BabelPackage
    {
        protected override void Initialize()
        {
            base.Initialize();
            this.RegisterProjectFactory(new RubyProjectFactory(this));
            this.RegisterEditorFactory(new EditorFactory(this));
        }
    }
}





