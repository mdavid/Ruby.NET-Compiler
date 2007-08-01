/***************************************************************************

Copyright (c) Microsoft Corporation. All rights reserved.
This code is licensed under the Visual Studio SDK license terms.
THIS CODE IS PROVIDED *AS IS* WITHOUT WARRANTY OF
ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING ANY
IMPLIED WARRANTIES OF FITNESS FOR A PARTICULAR
PURPOSE, MERCHANTABILITY, OR NON-INFRINGEMENT.

***************************************************************************/
using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio;

namespace Ruby.NET
{
    [Guid("1be19140-e6ab-4387-937c-d9746098c461")]
    class LanguageService : BabelLanguageService
    {
        public LanguageService()
        {
        }

        public override int ValidateBreakpointLocation(Microsoft.VisualStudio.TextManager.Interop.IVsTextBuffer buffer, int line, int col, Microsoft.VisualStudio.TextManager.Interop.TextSpan[] pCodeSpan)
        {
            pCodeSpan[0].iStartIndex = col;
            pCodeSpan[0].iStartLine = line;
            pCodeSpan[0].iEndIndex = col;
            pCodeSpan[0].iEndLine = line;
            return VSConstants.S_OK;
        }
    }
}
