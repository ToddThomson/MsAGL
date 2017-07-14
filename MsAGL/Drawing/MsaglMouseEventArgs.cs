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
using Windows.System;
using Windows.UI.Xaml;

namespace Microsoft.Msagl.Drawing
{ 
    /// <summary>
    /// An abstract class supporting pointer events.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage( "Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Msagl" )]
    public abstract class MsaglPointerEventArgs : EventArgs
    {
        /// <summary>
        ///  Gets the current state of the left mouse button.
        /// </summary>
        public abstract bool IsLeftButtonPressed { get; }

        /// <summary>
        ///  Gets the current state of the middle mouse button.
        /// </summary>
        public abstract bool IsMiddleButtonPressed { get; }

        /// <summary>
        ///    Gets the current state of the right mouse button.
        /// </summary>
        public abstract bool IsRightButtonPressed { get; }

        /// <summary>
        /// Gets the key modifiers for the pointer event.
        /// </summary>
        public abstract VirtualKeyModifiers KeyModifiers { get; }

        /// <summary>
        /// gets or sets the handled flag
        /// </summary>
        public abstract bool Handled { get; set; }

        /// <summary>
        /// X position of the mouse cursor
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage( "Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "X" )]
        public abstract int X { get; }

        /// <summary>
        /// Y position of the mouse cursor
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage( "Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Y" )]
        public abstract int Y { get; }

        /// <summary>
        /// gets the number of clicks of the button
        /// </summary>
        public abstract int Clicks { get; }
    }
}
