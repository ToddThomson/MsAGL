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

#region Namespaces

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Msagl.Core.Geometry;
using System.Runtime.Serialization;

#endregion

namespace Microsoft.Msagl.Core.Layout
{
    /// <summary>
    /// keeps the arrowhead info
    /// </summary>
#if TEST_MSAGL
    [DataContract]
#endif
    public class Arrowhead
    {
        ///<summary>
        /// Gets or sets the arrow head length.
        ///</summary>
        public const double DefaultArrowheadLength = 10;

        ///<summary>
        /// The overall length of the arrow head
        ///</summary>
        public double Length { get; set; } = DefaultArrowheadLength;

        ///<summary>
        /// The width of the arrow head at the base
        ///</summary>
        public double Width { get; set; }

        ///<summary>
        /// Where the tip of the arrow head is
        ///</summary>
        public Point TipPosition { get; set; }

        ///<summary>
        /// A relative offset that moves the tip position 
        ///</summary>
        public double Offset { get; set; }

        /// <summary>
        /// Clone the arrowhead information
        /// </summary>
        /// <returns></returns>
        public Arrowhead Clone()
        {
            return new Arrowhead()
            {
                Length = this.Length,
                Width = this.Width,
                TipPosition = this.TipPosition,
                Offset = this.Offset
            };
        }
    }
}
