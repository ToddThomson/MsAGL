using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.Msagl.Core.Geometry.Curves;
using Microsoft.Msagl.Core.Layout;
using Microsoft.Msagl.Drawing;
using Edge = Microsoft.Msagl.Drawing.Edge;
using Ellipse = Microsoft.Msagl.Core.Geometry.Curves.Ellipse;
using LineSegment = Microsoft.Msagl.Core.Geometry.Curves.LineSegment;
using Node = Microsoft.Msagl.Drawing.Node;
using Point = Microsoft.Msagl.Core.Geometry.Point;
using Polyline = Microsoft.Msagl.Core.Geometry.Curves.Polyline;
using Shape = Microsoft.Msagl.Drawing.Shape;
using Size = Windows.Foundation.Size;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Shapes;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI;
using Windows.Foundation;

namespace Microsoft.Msagl.GraphControl {

    public class VNode : IViewerNode, IInvalidatable
    {

        #region Fields

        internal Path BoundaryPath;
        internal FrameworkElement FrameworkElementOfNodeForLabel;
        readonly Func<Edge, VEdge> _funcFromDrawingEdgeToVEdge;
        Subgraph _subgraph;
        Node _node;
        Border _collapseButtonBorder;
        Rectangle _topMarginRect;
        Path _collapseSymbolPath;
        readonly Brush _collapseSymbolPathInactive = new SolidColorBrush( Windows.UI.Colors.Silver );

        #endregion

        #region Properties

        internal int ZIndex
        {
            get
            {
                var geomNode = Node.GeometryNode;
                if ( geomNode == null )
                    return 0;
                int ret = 0;
                do
                {
                    if ( geomNode.ClusterParents == null )
                        return ret;
                    geomNode = geomNode.ClusterParents.FirstOrDefault();
                    if ( geomNode != null )
                        ret++;
                    else
                        return ret;
                } while ( true );
            }
        }

        public Node Node
        {
            get
            {
                return _node;
            }

            private set
            {
                _node = value;
                _subgraph = _node as Subgraph;
            }
        }

        #endregion

        #region Constructor(s)

        internal VNode( Node node, FrameworkElement frameworkElementOfNodeForLabelOfLabel,
            Func<Edge, VEdge> funcFromDrawingEdgeToVEdge, Func<double> pathStrokeThicknessFunc )
        {
            PathStrokeThicknessFunc = pathStrokeThicknessFunc;
            Node = node;
            FrameworkElementOfNodeForLabel = frameworkElementOfNodeForLabelOfLabel;

            _funcFromDrawingEdgeToVEdge = funcFromDrawingEdgeToVEdge;

            CreateNodeBoundaryPath();
            if ( FrameworkElementOfNodeForLabel != null )
            {
                FrameworkElementOfNodeForLabel.Tag = this; //get a backpointer to the VNode 
                Common.PositionFrameworkElement( FrameworkElementOfNodeForLabel, node.GeometryNode.Center, 1 );
                // FIXME: FrameworkElementOfNodeForLabel.ZIndex = ( BoundaryPath ) + 1;
            }
            SetupSubgraphDrawing();
            Node.Attr.VisualsChanged += ( a, b ) => Invalidate();
            Node.IsVisibleChanged += obj =>
            {
                foreach ( var frameworkElement in FrameworkElements )
                {
                    frameworkElement.Visibility = Node.IsVisible ? Visibility.Visible : Visibility.Collapsed;
                }
            };
        }

        #endregion

        internal IEnumerable<FrameworkElement> FrameworkElements
        {
            get
            {
                if ( FrameworkElementOfNodeForLabel != null )
                    yield return FrameworkElementOfNodeForLabel;

                if ( BoundaryPath != null )
                    yield return BoundaryPath;

                if ( _collapseButtonBorder != null )
                {
                    yield return _collapseButtonBorder;
                    yield return _topMarginRect;
                    yield return _collapseSymbolPath;
                }
            }
        }

        #region Private Methods

        void SetupSubgraphDrawing()
        {
            if ( _subgraph == null ) return;

            SetupTopMarginBorder();
            SetupCollapseSymbol();
        }

        void SetupTopMarginBorder()
        {
            var cluster = (Cluster)_subgraph.GeometryObject;
            _topMarginRect = new Rectangle
            {
                Fill = new SolidColorBrush( Colors.Transparent ),
                Width = Node.Width,
                Height = cluster.RectangularBoundary.TopMargin
            };
            PositionTopMarginBorder( cluster );
            SetZIndexAndMouseInteractionsForTopMarginRect();
        }

        void PositionTopMarginBorder( Cluster cluster )
        {
            var box = cluster.BoundaryCurve.BoundingBox;

            Common.PositionFrameworkElement( _topMarginRect,
                box.LeftTop + new Point( _topMarginRect.Width / 2, -_topMarginRect.Height / 2 ), 1 );
        }

        void SetZIndexAndMouseInteractionsForTopMarginRect()
        {
            _topMarginRect.PointerEntered +=
                (
                    ( a, b ) =>
                    {
                        _collapseButtonBorder.Background =
                            Common.BrushFromMsaglColor( _subgraph.CollapseButtonColorActive );
                        _collapseSymbolPath.Stroke = new SolidColorBrush( Colors.Black );
                    }
                    );

            _topMarginRect.PointerExited +=
                ( a, b ) =>
                {
                    _collapseButtonBorder.Background = Common.BrushFromMsaglColor( _subgraph.CollapseButtonColorInactive );
                    _collapseSymbolPath.Stroke = new SolidColorBrush( Colors.Silver );
                };
            Canvas.SetZIndex( _topMarginRect, int.MaxValue );
        }

        void SetupCollapseSymbol()
        {
            var collapseBorderSize = GetCollapseBorderSymbolSize();
            Debug.Assert( collapseBorderSize > 0 );
            _collapseButtonBorder = new Border
            {
                Background = Common.BrushFromMsaglColor( _subgraph.CollapseButtonColorInactive ),
                Width = collapseBorderSize,
                Height = collapseBorderSize,
                CornerRadius = new CornerRadius( collapseBorderSize / 2 )
            };

            Canvas.SetZIndex( _collapseButtonBorder, Canvas.GetZIndex( BoundaryPath ) + 1 );


            var collapseButtonCenter = GetCollapseButtonCenter( collapseBorderSize );
            Common.PositionFrameworkElement( _collapseButtonBorder, collapseButtonCenter, 1 );

            double w = collapseBorderSize * 0.4;
            _collapseSymbolPath = new Path
            {
                Data = CreateCollapseSymbolPath( collapseButtonCenter + new Point( 0, -w / 2 ), w ),
                Stroke = _collapseSymbolPathInactive,
                StrokeThickness = 1
            };

            Canvas.SetZIndex( _collapseSymbolPath, Canvas.GetZIndex( _collapseButtonBorder ) + 1 );
            _topMarginRect.PointerPressed += TopMarginRectMouseLeftButtonDown;
        }

        #endregion

        /// <summary>
        /// </summary>
        public event Action<IViewerNode> IsCollapsedChanged;

        void InvokeIsCollapsedChanged()
        {
            IsCollapsedChanged?.Invoke( this );
        }

        void TopMarginRectMouseLeftButtonDown( object sender, PointerRoutedEventArgs e )
        {
            var pointerInfo = e.GetCurrentPoint( _collapseButtonBorder );

            var pos = pointerInfo.Position;

            if ( pos.X <= _collapseButtonBorder.Width && pos.Y <= _collapseButtonBorder.Height && pos.X >= 0 && pos.Y >= 0 )
            {
                e.Handled = true;
                var cluster = (Cluster)_subgraph.GeometryNode;
                cluster.IsCollapsed = !cluster.IsCollapsed;
                InvokeIsCollapsedChanged();
            }
        }

        double GetCollapseBorderSymbolSize()
        {
            return ((Cluster)_subgraph.GeometryNode).RectangularBoundary.TopMargin -
                   PathStrokeThickness / 2 - 0.5;
        }

        Point GetCollapseButtonCenter( double collapseBorderSize )
        {
            var box = _subgraph.GeometryNode.BoundaryCurve.BoundingBox;
            //cannot trust subgraph.GeometryNode.BoundingBox for a cluster
            double offsetFromBoundaryPath = PathStrokeThickness / 2 + 0.5;
            var collapseButtonCenter = box.LeftTop + new Point( collapseBorderSize / 2 + offsetFromBoundaryPath,
                -collapseBorderSize / 2 - offsetFromBoundaryPath );
            return collapseButtonCenter;
        }

        /*
                void FlipCollapsePath() {
                    var size = GetCollapseBorderSymbolSize();
                    var center = GetCollapseButtonCenter(size);

                    if (collapsePathFlipped) {
                        collapsePathFlipped = false;
                        collapseSymbolPath.RenderTransform = null;
                    }
                    else {
                        collapsePathFlipped = true;
                        collapseSymbolPath.RenderTransform = new RotateTransform(180, center.X, center.Y);
                    }
                }
        */

        Geometry CreateCollapseSymbolPath( Point center, double width )
        {
            var pathGeometry = new PathGeometry();
            var pathFigure = new PathFigure { StartPoint = Common.WpfPoint( center + new Point( -width, width ) ) };

            pathFigure.Segments.Add( new Windows.UI.Xaml.Media.LineSegment { Point = Common.WpfPoint( center ) } );
            pathFigure.Segments.Add( new Windows.UI.Xaml.Media.LineSegment { Point = Common.WpfPoint( center + new Point( width, width ) ) } );

            pathGeometry.Figures.Add( pathFigure );

            return pathGeometry;
        }

        internal void CreateNodeBoundaryPath()
        {
            if ( FrameworkElementOfNodeForLabel != null )
            {
                // FrameworkElementOfNode.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
                var center = Node.GeometryNode.Center;
                var margin = 2 * Node.Attr.LabelMargin;
                var bc = NodeBoundaryCurves.GetNodeBoundaryCurve( Node,
                    FrameworkElementOfNodeForLabel
                        .Width + margin,
                    FrameworkElementOfNodeForLabel
                        .Height + margin );
                bc.Translate( center );
                //                if (LgNodeInfo != null) {
                //                    //LgNodeInfo.OriginalCurveOfGeomNode = bc;
                //                    Node.GeometryNode.BoundaryCurve =
                //                        bc.Transform(PlaneTransformation.ScaleAroundCenterTransformation(LgNodeInfo.Scale,
                //                            Node.GeometryNode.Center))
                //                            .Clone();
                //                }
            }
            BoundaryPath = new Path { Data = CreatePathFromNodeBoundary(), Tag = this };
            Canvas.SetZIndex( BoundaryPath, ZIndex );
            SetFillAndStroke();
            if ( Node.Label != null )
            {
                ToolTip toolTip = new ToolTip();
                toolTip.Content = Node.LabelText;
                ToolTipService.SetToolTip( BoundaryPath, toolTip );

                if ( FrameworkElementOfNodeForLabel != null )
                    ToolTipService.SetToolTip( FrameworkElementOfNodeForLabel, toolTip );
            }
        }

        internal Func<double> PathStrokeThicknessFunc;

        double PathStrokeThickness
        {
            get { return PathStrokeThicknessFunc != null ? PathStrokeThicknessFunc() : Node.Attr.LineWidth; }
        }

        byte GetTransparency( byte t )
        {
            return t;
        }

        void SetFillAndStroke()
        {
            byte trasparency = GetTransparency( Node.Attr.Color.A );
            BoundaryPath.Stroke =
                Common.BrushFromMsaglColor( new Drawing.Color( trasparency, Node.Attr.Color.R, Node.Attr.Color.G,
                    Node.Attr.Color.B ) );
            SetBoundaryFill();
            BoundaryPath.StrokeThickness = PathStrokeThickness;

            var textBlock = FrameworkElementOfNodeForLabel as TextBlock;
            if ( textBlock != null )
            {
                var col = Node.Label.FontColor;
                textBlock.Foreground =
                    Common.BrushFromMsaglColor( new Drawing.Color( GetTransparency( col.A ), col.R, col.G, col.B ) );
            }
        }

        void SetBoundaryFill()
        {
            BoundaryPath.Fill = Common.BrushFromMsaglColor( Node.Attr.FillColor );
        }

        Geometry DoubleCircle()
        {
            var box = Node.BoundingBox;
            double w = box.Width;
            double h = box.Height;
            var pathGeometry = new PathGeometry();
            var r = new Rect( box.Left, box.Bottom, w, h );

            // FIXME: TJT

            //pathGeometry.Figures.Add( new EllipseGeometry()
            //{
            //    Center = new Windows.Foundation.Point( w, h ),
            //    RadiusX = r.Left,
            //    RadiusY = r.Bottom
            //} );
                
            //var inflation = Math.Min(5.0, Math.Min(w/3, h/3));
            //r.Inflate(-inflation, -inflation);
            //pathGeometry.AddGeometry(new EllipseGeometry(r));

            return pathGeometry;
        }

        Geometry CreatePathFromNodeBoundary()
        {
            Geometry geometry;

            switch ( Node.Attr.Shape )
            {
                case Shape.Box:
                case Shape.House:
                case Shape.InvHouse:
                case Shape.Diamond:
                case Shape.Octagon:
                case Shape.Hexagon:

                    geometry = CreateGeometryFromMsaglCurve( Node.GeometryNode.BoundaryCurve );
                    break;

                case Shape.DoubleCircle:
                    geometry = DoubleCircle();
                    break;

                default:
                    geometry = GetEllipseGeometry();
                    break;
            }

            return geometry;
        }

        Geometry CreateGeometryFromMsaglCurve( ICurve iCurve )
        {
            var pathGeometry = new PathGeometry();
            var pathFigure = new PathFigure
            {
                IsClosed = true,
                IsFilled = true,
                StartPoint = Common.WpfPoint( iCurve.Start )
            };

            var curve = iCurve as Curve;
            if ( curve != null )
            {
                AddCurve( pathFigure, curve );
            }
            else
            {
                var rect = iCurve as RoundedRect;
                if ( rect != null )
                    AddCurve( pathFigure, rect.Curve );
                else
                {
                    var ellipse = iCurve as Ellipse;
                    if ( ellipse != null )
                    {
                        return new EllipseGeometry
                        {
                            Center = Common.WpfPoint( ellipse.Center ),
                            RadiusX = ellipse.AxisA.Length,
                            RadiusY = ellipse.AxisB.Length
                        };
                    }
                    var poly = iCurve as Polyline;
                    if ( poly != null )
                    {
                        var p = poly.StartPoint.Next;
                        do
                        {
                            pathFigure.Segments.Add( new Windows.UI.Xaml.Media.LineSegment { Point = Common.WpfPoint( p.Point ) } );

                            p = p.NextOnPolyline;
                        } while ( p != poly.StartPoint );
                    }
                }
            }

            pathGeometry.Figures.Add( pathFigure );

            return pathGeometry;
        }

        static void AddCurve( PathFigure pathFigure, Curve curve )
        {
            foreach ( ICurve seg in curve.Segments )
            {
                var ls = seg as LineSegment;

                if ( ls != null )
                    pathFigure.Segments.Add( new Windows.UI.Xaml.Media.LineSegment { Point = Common.WpfPoint( ls.End ) } );
                else
                {
                    var ellipse = seg as Ellipse;

                    if ( ellipse != null )
                    {
                        pathFigure.Segments.Add( new ArcSegment
                        {
                            Point = Common.WpfPoint( ellipse.End ),
                            Size = new Size( ellipse.AxisA.Length, ellipse.AxisB.Length ),
                            RotationAngle = Point.Angle( new Point( 1, 0 ), ellipse.AxisA ),
                            //ellipse.ParEnd - ellipse.ParEnd >= Math.PI,
                            SweepDirection = !ellipse.OrientedCounterclockwise() ? SweepDirection.Counterclockwise : SweepDirection.Clockwise
                        } );
                    }
                }
            }
        }

        Geometry GetEllipseGeometry()
        {
            return new EllipseGeometry
            {
                Center = Common.WpfPoint( Node.BoundingBox.Center ),
                RadiusX = Node.BoundingBox.Width / 2,
                RadiusY = Node.BoundingBox.Height / 2
            };
        }

        #region Implementation of IViewerObject

        public DrawingObject DrawingObject
        {
            get { return Node; }
        }

        public bool MarkedForDragging { get; set; }

        public event EventHandler MarkedForDraggingEvent;
        public event EventHandler UnmarkedForDraggingEvent;

        #endregion

        public IEnumerable<IViewerEdge> InEdges
        {
            get { return Node.InEdges.Select( e => _funcFromDrawingEdgeToVEdge( e ) ); }
        }

        public IEnumerable<IViewerEdge> OutEdges
        {
            get { return Node.OutEdges.Select( e => _funcFromDrawingEdgeToVEdge( e ) ); }
        }

        public IEnumerable<IViewerEdge> SelfEdges
        {
            get { return Node.SelfEdges.Select( e => _funcFromDrawingEdgeToVEdge( e ) ); }
        }

        public void SetStrokeFill()
        {
            throw new NotImplementedException();
        }

        public void Invalidate()
        {
            if ( !Node.IsVisible )
            {
                foreach ( var fe in FrameworkElements )
                    fe.Visibility = Visibility.Collapsed;

                return;
            }

            BoundaryPath.Data = CreatePathFromNodeBoundary();

            Common.PositionFrameworkElement( FrameworkElementOfNodeForLabel, Node.BoundingBox.Center, 1 );

            SetFillAndStroke();

            if ( _subgraph == null )
                return;

            PositionTopMarginBorder( (Cluster)_subgraph.GeometryNode );
            double collapseBorderSize = GetCollapseBorderSymbolSize();
            var collapseButtonCenter = GetCollapseButtonCenter( collapseBorderSize );
            Common.PositionFrameworkElement( _collapseButtonBorder, collapseButtonCenter, 1 );
            double w = collapseBorderSize * 0.4;
            _collapseSymbolPath.Data = CreateCollapseSymbolPath( collapseButtonCenter + new Point( 0, -w / 2 ), w );
            _collapseSymbolPath.RenderTransform = ((Cluster)_subgraph.GeometryNode).IsCollapsed
                ? new RotateTransform
                {
                    Angle = 180,
                    CenterX = collapseButtonCenter.X,
                    CenterY = collapseButtonCenter.Y
                }
                : null;

            _topMarginRect.Visibility =
                _collapseSymbolPath.Visibility =
                    _collapseButtonBorder.Visibility = Visibility.Visible;

        }

        public override string ToString()
        {
            return Node.Id;
        }

        internal void DetouchFromCanvas( Canvas graphCanvas )
        {
            if ( BoundaryPath != null )
                graphCanvas.Children.Remove( BoundaryPath );

            if ( FrameworkElementOfNodeForLabel != null )
                graphCanvas.Children.Remove( FrameworkElementOfNodeForLabel );
        }
    }
}