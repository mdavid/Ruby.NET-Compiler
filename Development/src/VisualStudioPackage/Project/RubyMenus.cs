using System;
using System.ComponentModel.Design;


namespace Ruby.NET
{
    public sealed class RubyMenus
    {
        internal static readonly Guid guidRubyProjectCmdSet = new Guid("{13ed0892-9c6c-4cd1-984b-c129feb468db}");
        internal static readonly CommandID SetAsMain = new CommandID(guidRubyProjectCmdSet, 0x3001);
    }
}
