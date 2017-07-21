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
using System.Text;
using Microsoft.Msagl.Core.Geometry.Curves;
using Msagl.Uwp.UI.Layout;
using P2 = Microsoft.Msagl.Core.Geometry.Point;

#endregion

namespace Msagl.Uwp.UI.Layout
{
    /// <summary>
    /// a helper class for creation of node boundary curves
    /// </summary>
    public sealed class NodeBoundaryCurves
    {
        NodeBoundaryCurves() { }

        /// <summary>
        /// a helper function to creat a node boundary curve 
        /// </summary>
        /// <param name="node">the node</param>
        /// <param name="width">the node width</param>
        /// <param name="height">the node height</param>
        /// <returns></returns>
        public static ICurve GetNodeBoundaryCurve( Node node, double width, double height )
        {
            if ( node == null )
                throw new InvalidOperationException();

            NodeAttr nodeAttr = node.Attr;

            switch ( nodeAttr.Shape )
            {
                case Shape.Ellipse:
                case Shape.DoubleCircle:
                    return CurveFactory.CreateEllipse( width, height, new P2( 0, 0 ) );

                case Shape.Circle:
                {
                    double r = Math.Max( width / 2, height / 2 );
                    return CurveFactory.CreateEllipse( r, r, new P2( 0, 0 ) );
                }

                case Shape.Box:
                    if ( nodeAttr.XRadius != 0 || nodeAttr.YRadius != 0 )
                        return CurveFactory.CreateRectangleWithRoundedCorners(
                            width, height, nodeAttr.XRadius,
                            nodeAttr.YRadius, new P2( 0, 0 ) );

                    return CurveFactory.CreateRectangle( width, height, new P2( 0, 0 ) );

                case Shape.Diamond:
                    return CurveFactory.CreateDiamond( width, height, new P2( 0, 0 ) );

                case Shape.House:
                    return CurveFactory.CreateHouse( width, height, new P2() );

                case Shape.InvHouse:
                    return CurveFactory.CreateInvertedHouse( width, height, new P2() );

                case Shape.Octagon:
                    return CurveFactory.CreateOctagon( width, height, new P2() );
#if DEBUG
                case Shape.TestShape:
                    return CurveFactory.CreateTestShape( width, height );
#endif
                default:
                    {
                        //  Debug.WriteLine("creating ellipse for shape {0}",nodeAttr.Shape);
                        return new Ellipse(
                          new P2( width / 2, 0 ), new P2( 0, height / 2 ), new P2() );
                    }
            }
        }
    }
}
