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
using Microsoft.Msagl.Drawing;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Shapes;
using Windows.UI.Xaml.Media;
using Windows.UI;

#endregion

namespace Msagl.Uwp.UI.GraphControl
{
    internal class VLabel : IViewerObject, IInvalidatable
    {
        public event EventHandler MarkedForDraggingEvent;
        public event EventHandler UnmarkedForDraggingEvent;

        internal readonly FrameworkElement FrameworkElement;
        bool markedForDragging;

        public VLabel( Edge edge, FrameworkElement frameworkElement )
        {
            FrameworkElement = frameworkElement;
            DrawingObject = edge.Label;
        }

        public DrawingObject DrawingObject { get; private set; }

        public bool MarkedForDragging
        {
            get { return markedForDragging; }
            set
            {
                markedForDragging = value;

                if ( value )
                {
                    AttachmentLine = new Line
                    {
                        Stroke = new SolidColorBrush( Colors.Black ),
                        StrokeDashArray = { 1, 2 }
                    }; //the line will have 0,0, 0,0 start and end so it would not be rendered

                    ((Canvas)FrameworkElement.Parent).Children.Add( AttachmentLine );
                }
                else
                {
                    ((Canvas)FrameworkElement.Parent).Children.Remove( AttachmentLine );
                    AttachmentLine = null;
                }
            }
        }

        IEnumerable<double> OffsetElems()
        {
            yield return 1;
            yield return 2;
        }

        Line AttachmentLine { get; set; }

        public void Invalidate()
        {
            var label = (Microsoft.Msagl.Drawing.Label)DrawingObject;
            Common.PositionFrameworkElement( FrameworkElement, label.Center, 1 );
            var geomLabel = label.GeometryLabel;

            if ( AttachmentLine != null )
            {
                AttachmentLine.X1 = geomLabel.AttachmentSegmentStart.X;
                AttachmentLine.Y1 = geomLabel.AttachmentSegmentStart.Y;

                AttachmentLine.X2 = geomLabel.AttachmentSegmentEnd.X;
                AttachmentLine.Y2 = geomLabel.AttachmentSegmentEnd.Y;
            }
        }
    }
}