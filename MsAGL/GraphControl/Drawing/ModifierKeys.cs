#region Copyright Notice

// Copyright (c) by Achilles Software, All rights reserved.
//
// Licensed under the MIT License. See License.txt in the project root for license information.
//
// Send questions regarding this copyright notice to: mailto:todd.thomson@achilles-software.com

/*
Microsoft Automatic Graph Layout,MSAGL 

Copyright (c) Microsoft Corporation

All rights reserved. 

MIT License 

Permission is hereby granted, free of charge, to any person obtaining
a copy of this software and associated documentation files (the
""Software""), to deal in the Software without restriction, including
without limitation the rights to use, copy, modify, merge, publish,
distribute, sublicense, and/or sell copies of the Software, and to
permit persons to whom the Software is furnished to do so, subject to
the following conditions:

The above copyright notice and this permission notice shall be
included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED *AS IS*, WITHOUT WARRANTY OF ANY KIND,
EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/

#endregion

using System;
using System.Collections.Generic;
using System.Text;

namespace Msagl.Uwp.UI.Drawing {
 
    /// <summary>
    /// Specifies the set of modifier keys.
    /// </summary>
    [Flags]
    public enum ModifierKeys {
      /// <summary>
      /// No modifiers are pressed.
      /// </summary>
        None = 0,
        /// <summary>
        /// THE alt key
        /// </summary>
        Alt = 1,
       /// <summary>
       /// the control key
       /// </summary>
        Control = 2,
        /// <summary>
        /// the shift key
        /// </summary>
        Shift = 4,
     /// <summary>
     /// the window logo key
     /// </summary>
        Windows = 8,
    }
}

