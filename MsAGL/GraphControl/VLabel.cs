using System;
using System.Collections.Generic;
using Microsoft.Msagl.Drawing;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Shapes;
using Windows.UI.Xaml.Media;
using Windows.UI;

namespace Microsoft.Msagl.GraphControl
{
    internal class VLabel : IViewerObject, IInvalidatable
    {
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
                        // FIXME: StrokeDashArray = new DoubleCollection() { OffsetElems(). })
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

        public event EventHandler MarkedForDraggingEvent;
        public event EventHandler UnmarkedForDraggingEvent;

        public void Invalidate()
        {
            var label = (Drawing.Label)DrawingObject;
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