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

namespace Ruby.NET
{
    public class Resolver : Ruby.NET.IASTResolver
    {
        #region IASTResolver Members

        public IList<Ruby.NET.Declaration> FindCompletions(object result, int line, int col)
        {
            return new List<Ruby.NET.Declaration>();
        }

        public IList<Ruby.NET.Declaration> FindMembers(object result, int line, int col)
        {
            // ManagedMyC.Parser.AAST aast = result as ManagedMyC.Parser.AAST;
            List<Ruby.NET.Declaration> members = new List<Ruby.NET.Declaration>();

            //foreach (string state in aast.startStates.Keys)
            //    members.Add(new Declaration(state, state, 0, state));

            return members;
        }

        public string FindQuickInfo(object result, int line, int col)
        {
            return "unknown";
        }

        public IList<Ruby.NET.Method> FindMethods(object result, int line, int col, string name)
        {
            return new List<Ruby.NET.Method>();
        }

        #endregion
    }
}
