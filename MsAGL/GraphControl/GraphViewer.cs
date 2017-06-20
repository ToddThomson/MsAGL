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
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

using Microsoft.Msagl.Core;
using Microsoft.Msagl.Core.Geometry;
using Microsoft.Msagl.Core.Geometry.Curves;
using Microsoft.Msagl.Core.Layout;
using Microsoft.Msagl.Drawing;
using Microsoft.Msagl.Layout.LargeGraphLayout;
using Microsoft.Msagl.Miscellaneous;
using Microsoft.Msagl.Miscellaneous.LayoutEditing;
using DrawingEdge = Microsoft.Msagl.Drawing.Edge;
using ILabeledObject = Microsoft.Msagl.Drawing.ILabeledObject;
using Label = Microsoft.Msagl.Drawing.Label;
using ModifierKeys = Microsoft.Msagl.Drawing.ModifierKeys;
using Node = Microsoft.Msagl.Core.Layout.Node;
using Point = Microsoft.Msagl.Core.Geometry.Point;
using Rectangle = Microsoft.Msagl.Core.Geometry.Rectangle;
using Size = Windows.Foundation.Size;
using UwpPoint = Windows.Foundation.Point;
using Edge = Microsoft.Msagl.Core.Layout.Edge;
using Ellipse = Windows.UI.Xaml.Shapes.Ellipse;
using LineSegment = Microsoft.Msagl.Core.Geometry.Curves.LineSegment;
using Windows.UI.Xaml.Shapes;
using XamlShapes = Windows.UI.Xaml.Shapes;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml;
using Windows.Foundation;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Input;
using System.Diagnostics;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Text;
using Windows.UI.Popups;
using Windows.UI.Xaml.Media.Imaging;
using System.Threading.Tasks;
using Msagl.GraphControl;
using Windows.Graphics.Display;

namespace Microsoft.Msagl.GraphControl
{
    public class GraphViewer : IViewer
    {
        #region Events

        public event EventHandler LayoutStarted;
        public event EventHandler LayoutComplete;

        #endregion

        #region Fields

        Path _targetArrowheadPathForRubberEdge;
        Path _rubberEdgePath;
        Path _rubberLinePath;
        Point _sourcePortLocationForEdgeRouting;

        BackgroundWorker _backgroundWorker;
        CancelToken _cancelToken = new CancelToken();

        Point _pointerPressedPositionInGraph;
        bool _pointerPressedPositionInGraph_initialized;

        UwpPoint _objectUnderMouseDetectionLocation;

        Ellipse _sourcePortCircle;

        readonly Canvas _graphCanvas = new Canvas();

        Graph _drawingGraph;
        Pointer _graphCanvasCapturedPointer;

        readonly Dictionary<DrawingObject, FrameworkElement> drawingObjectsToFrameworkElements =
            new Dictionary<DrawingObject, FrameworkElement>();

        readonly LayoutEditor layoutEditor;

        GeometryGraph geometryGraphUnderLayout;

        /* Thread layoutThread; */

        bool needToCalculateLayout = true;

        object _objectUnderPointer;

        static double _dpiX;
        static double _dpiY;

        readonly Dictionary<DrawingObject, IViewerObject> drawingObjectsToIViewerObjects =
            new Dictionary<DrawingObject, IViewerObject>();

        FrameworkElement _rectToFillGraphBackground;
        Windows.UI.Xaml.Shapes.Rectangle _rectToFillCanvas;

        GeometryGraph GeomGraph
        {
            get { return _drawingGraph.GeometryGraph; }
        }

        #endregion

        #region Properties

        /// <summary>
        /// The canvas to draw the graph.
        /// </summary>
        public Canvas GraphCanvas { get { return _graphCanvas; } }

        public LayoutEditor LayoutEditor { get { return layoutEditor; } }

        protected Ellipse TargetPortCircle { get; set; }

        #endregion

        /*
        readonly DispatcherTimer layoutThreadCheckingTimer = new DispatcherTimer();
        */

        /// <summary>
        /// Set to true to perform layout in an async task.
        /// </summary>
        public bool RunLayoutAsync = false;

        #region Constructor(s)

        public GraphViewer()
        {
            //LargeGraphNodeCountThreshold = 0;
            layoutEditor = new LayoutEditor( this );

            _graphCanvas.SizeChanged += GraphCanvasSizeChanged;
            _graphCanvas.PointerPressed += GraphCanvasPointerPressed;
            //_graphCanvas.MouseRightButtonDown += GraphCanvasRightMouseDown;
            _graphCanvas.PointerMoved += GraphCanvasPointerMoved;

            _graphCanvas.PointerReleased += GraphCanvasPointerReleased;
            _graphCanvas.PointerWheelChanged += GraphCanvasPointerWheelChanged;
            //_graphCanvas.MouseRightButtonUp += GraphCanvasRightMouseUp;
            ViewChangeEvent += AdjustBtrectRenderTransform;

            LayoutEditingEnabled = true;

            // TJT: Is this really needed?
            clickCounter = new ClickCounter( () => PointerHelpers.GetPointerPosition() );// (UIElement)_graphCanvas.Parent ) );
            clickCounter.Elapsed += ClickCounterElapsed;

            //TEST:
            _graphCanvas.Width = 500;
            _graphCanvas.Height = 500;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// adds the main panel of the viewer to the children of the parent
        /// </summary>
        /// <param name="panel"></param>
        public void BindToPanel( Panel panel )
        {
            panel.Children.Add( GraphCanvas );
            GraphCanvas.UpdateLayout();
        }

        /// <summary>
        /// keeps centerOfZoom pinned to the screen and changes the scale by zoomFactor
        /// </summary>
        /// <param name="zoomFactor"></param>
        /// <param name="centerOfZoom"></param>
        public void ZoomAbout( double zoomFactor, UwpPoint centerOfZoom )
        {
            var scale = zoomFactor * FitFactor;
            var centerOfZoomOnScreen = _graphCanvas.TransformToVisual( (FrameworkElement)_graphCanvas.Parent ).TransformPoint( centerOfZoom );

            SetTransform( scale, centerOfZoomOnScreen.X - centerOfZoom.X * scale,
                         centerOfZoomOnScreen.Y + centerOfZoom.Y * scale );
        }

        #endregion

        #region Event Handlers

        void ClickCounterElapsed( object sender, EventArgs e )
        {
            var vedge = clickCounter.ClickedObject as VEdge;

            if ( vedge != null )
            {
                if ( clickCounter.UpCount == clickCounter.DownCount && clickCounter.UpCount == 1 )
                    HandleClickForEdge( vedge );
            }

            clickCounter.ClickedObject = null;
        }

        void AdjustBtrectRenderTransform( object sender, EventArgs e )
        {
            if ( _rectToFillCanvas == null )
                return;

            _rectToFillCanvas.RenderTransform = (Transform)_graphCanvas.RenderTransform.Inverse;
            var parent = (Panel)GraphCanvas.Parent;
            _rectToFillCanvas.Width = parent.ActualWidth;
            _rectToFillCanvas.Height = parent.ActualHeight;

        }

        void GraphCanvasRightMouseUp( object sender, PointerRoutedEventArgs e )
        {
            OnPointerReleased( e );
        }

        void HandleClickForEdge( VEdge vEdge )
        {
            //todo : add a hook
            var lgSettings = Graph.LayoutAlgorithmSettings as LgLayoutSettings;
            if ( lgSettings != null )
            {
                var lgEi = lgSettings.GeometryEdgesToLgEdgeInfos[ vEdge.Edge.GeometryEdge ];
                lgEi.SlidingZoomLevel = lgEi.SlidingZoomLevel != 0 ? 0 : double.PositiveInfinity;

                ViewChangeEvent( null, null );
            }
        }
        
        /*
                Tuple<string, VoidDelegate>[] CreateToggleZoomLevelMenuCoupleForNode(VNode vNode) {
                    var list = new List<Tuple<string, VoidDelegate>>();
                    var lgSettings = Graph.LayoutAlgorithmSettings as LgLayoutSettings;
                    if (lgSettings != null) {
                        var lgNodeInfo = lgSettings.GeometryNodesToLgNodeInfos[(Node) vNode.DrawingObject.GeometryObject];
                        list.Add(ToggleZoomLevelMenuCouple(lgNodeInfo));
                        list.Add(MakeAllNodeEdgesAlwaysVisible(lgNodeInfo));
                        list.Add(RestoreAllNodeEdgesZoomLevels(lgNodeInfo));

                        return list.ToArray();
                    }
                    return null;
                }
        */

        /*
                Tuple<string, VoidDelegate> RestoreAllNodeEdgesZoomLevels(LgNodeInfo lgNodeInfo) {
                    const string title = "Restore zoom levels of adjacent edges";

                    var lgSettings = (LgLayoutSettings) Graph.LayoutAlgorithmSettings;

                    return new Tuple<string, VoidDelegate>(title, () => {
                        foreach (var edge in lgNodeInfo.GeometryNode.Edges) {
                            var lgEi = lgSettings.GeometryEdgesToLgEdgeInfos[edge];
                            lgEi.SlidingZoomLevel = double.PositiveInfinity;
                        }
                        ViewChangeEvent(null, null);
                    });
                }
        */

        /*
                Tuple<string, VoidDelegate> MakeAllNodeEdgesAlwaysVisible(LgNodeInfo lgNodeInfo) {
                    const string title = "Set zoom levels for adjacent edges to 1";

                    var lgSettings = (LgLayoutSettings) Graph.LayoutAlgorithmSettings;

                    return new Tuple<string, VoidDelegate>(title, () => {
                        foreach (var edge in lgNodeInfo.GeometryNode.Edges) {
                            var lgEi = lgSettings.GeometryEdgesToLgEdgeInfos[edge];
                            lgEi.SlidingZoomLevel = 1;
                        }
                        ViewChangeEvent(null, null);
                    });
                }
        */

        /*
                Tuple<string, VoidDelegate> CreateToggleZoomLevelMenuCoupleForEdge(VEdge vedge) {
                    var lgSettings = (LgLayoutSettings) Graph.LayoutAlgorithmSettings;
                    var lgEdgeInfo = lgSettings.GeometryEdgesToLgEdgeInfos[(Edge) vedge.DrawingObject.GeometryObject];

                    return ToggleZoomLevelMenuCouple(lgEdgeInfo);
                }
        */

        /*
                Tuple<string, VoidDelegate> ToggleZoomLevelMenuCouple(LgInfoBase lgEdgeInfo) {
                    string title;
                    double newZoomLevel;
                    if (lgEdgeInfo.ZoomLevel > 0) {
                        title = "Make always visible";
                        newZoomLevel = 0;
                    } else {
                        title = "Restore zoom level";
                        newZoomLevel = double.PositiveInfinity;
                    }

                    return new Tuple<string, VoidDelegate>(title, () => {
                        lgEdgeInfo.SlidingZoomLevel = newZoomLevel;
                        ViewChangeEvent(null, null);
                    });
                }
        */

        void GraphCanvasPointerPressed( object sender, PointerRoutedEventArgs e )
        {
            var pointerInfo = e.GetCurrentPoint( _graphCanvas );

            clickCounter.AddPointerPressed( _objectUnderPointer );

            PointerPressed?.Invoke( this, CreatePointerEventArgs( e ) );

            if ( e.Handled )
                return;

            _pointerPressedPositionInGraph = Common.MsaglPoint( pointerInfo.Position );
            _pointerPressedPositionInGraph_initialized = true;
        }

        void GraphCanvasPointerMoved( object sender, PointerRoutedEventArgs e )
        {
            PointerMoved?.Invoke( this, CreatePointerEventArgs( e ) );

            if ( e.Handled )
                return;

            var pointerInfo = e.GetCurrentPoint( _graphCanvas );

            if ( pointerInfo.Properties.IsLeftButtonPressed && (!LayoutEditingEnabled || _objectUnderPointer == null) )
            {
                if ( !_pointerPressedPositionInGraph_initialized )
                {
                    _pointerPressedPositionInGraph = Common.MsaglPoint( pointerInfo.Position );
                    _pointerPressedPositionInGraph_initialized = true;
                }

                Pan( e );
            }
            else
            {
                // Retrieve the coordinate of the pointer position.
                UwpPoint mouseLocation = pointerInfo.Position;
                // Clear the contents of the list used for hit test results.
                ObjectUnderPointer = null;

                UpdateHitObjectUnderPointerPressedLocation( mouseLocation );
            }
        }

        void GraphCanvasRightMouseDown( object sender, PointerRoutedEventArgs e )
        {
            PointerPressed?.Invoke( this, CreatePointerEventArgs( e ) );
        }

        private void GraphCanvasPointerWheelChanged( object sender, PointerRoutedEventArgs e )
        {
            var pointerInfo = e.GetCurrentPoint( _graphCanvas );

            if ( pointerInfo.Properties.MouseWheelDelta != 0 )
            {
                const double zoomFractionLocal = 0.9;
                var zoomInc = pointerInfo.Properties.MouseWheelDelta < 0 ? zoomFractionLocal : 1.0 / zoomFractionLocal;

                ZoomAbout( ZoomFactor * zoomInc, pointerInfo.Position );

                e.Handled = true;
            }
        }

        void GraphCanvasPointerReleased( object sender, PointerRoutedEventArgs e )
        {
            OnPointerReleased( e );
            clickCounter.AddPointerReleased();

            if ( _graphCanvasCapturedPointer != null )
            {
                e.Handled = true;
                _graphCanvas.ReleasePointerCapture( _graphCanvasCapturedPointer );
                _graphCanvasCapturedPointer = null;
            }
        }

        void OnPointerReleased( PointerRoutedEventArgs e )
        {
            PointerReleased?.Invoke( this, CreatePointerEventArgs( e ) );
        }

        #endregion

        void UpdateHitObjectUnderPointerPressedLocation( UwpPoint pt )
        {
            _objectUnderMouseDetectionLocation = pt;
            // Expand the hit test area by creating a geometry centered on the hit test point.

            var rect = new Rect(
                new UwpPoint( pt.X - MouseHitTolerance, pt.Y - MouseHitTolerance ),
                new UwpPoint( pt.X + MouseHitTolerance, pt.Y + MouseHitTolerance ) );
            var expandedHitTestArea = new RectangleGeometry();

            expandedHitTestArea.Rect = rect;

            // Set up a callback to receive the hit test result enumeration.
            var hitElements = VisualTreeHelper.FindElementsInHostCoordinates( rect, _graphCanvas );
        
            foreach ( var frameworkElement in hitElements )
            {
                //if ( frameworkElement.Tag == null )
                //    return HitTestResultBehavior.Continue;

                var tag = (frameworkElement as FrameworkElement).Tag;
                var iviewerObj = tag as IViewerObject;

                if ( iviewerObj != null )
                {
                    if ( ObjectUnderPointer is IViewerEdge || ObjectUnderPointer == null
                        || Canvas.GetZIndex( frameworkElement ) > Canvas.GetZIndex( GetFrameworkElementFromIViewerObject( ObjectUnderPointer ) ) )
                    {
                        //always overwrite an edge or take the one with greater zIndex
                        ObjectUnderPointer = iviewerObj;
                    }
                }
            }
        }

        FrameworkElement GetFrameworkElementFromIViewerObject( IViewerObject viewerObject )
        {
            FrameworkElement ret;

            if ( viewerObject is VNode vNode )
            {
                ret = vNode.FrameworkElementOfNodeForLabel ?? vNode.BoundaryPath;
            }
            else if ( viewerObject is VLabel vLabel )
            {
                ret = vLabel.FrameworkElement;
            }
            else if ( viewerObject is VEdge vEdge )
            {
                ret = vEdge.CurvePath;
            }
            else
            {
                throw new InvalidOperationException( "Unexpected object type in GraphViewer" );
            }
           
            if ( ret == null )
                throw new InvalidOperationException( "Did not find a framework element!" );

            return ret;
        }

        // Return the result of the hit test to the callback.
        void MyHitTestResultCallbackWithNoCallbacksToTheUser( IEnumerable<UIElement> hitElements )
        {
            foreach ( var frameworkElement in hitElements )
            {
                object tag = (frameworkElement as FrameworkElement).Tag;
                if ( tag != null )
                {
                    //it is a tagged element
                    var ivo = tag as IViewerObject;
                    if ( ivo != null )
                    {
                        _objectUnderPointer = ivo;
                        if ( tag is VNode || tag is Label )
                            return;// HitTestResultBehavior.Stop;
                    }
                    else
                    {
                        System.Diagnostics.Debug.Assert( tag is Rail );
                        _objectUnderPointer = tag;
                        return;// HitTestResultBehavior.Stop;
                    }
                }
            }

        }

        protected double MouseHitTolerance
        {
            get { return (0.05) * DpiX / CurrentScale; }
        }

        /// <summary>
        /// This function pins the sourcePoint to screenPoint.
        /// </summary>
        /// <param name="screenPoint"></param>
        /// <param name="sourcePoint"></param>
        void SetTransformFromTwoPoints( UwpPoint screenPoint, Point sourcePoint )
        {
            var scale = CurrentScale;
            SetTransform( scale, screenPoint.X - scale * sourcePoint.X, screenPoint.Y + scale * sourcePoint.Y );
        }

        void Pan( PointerRoutedEventArgs e )
        {
            if ( UnderLayout )
                return;

            PointerPoint pointerInfo = e.GetCurrentPoint( (UIElement)_graphCanvas.Parent );

            // FIXME: if ( !_graphCanvas.IsPointerCaptured )
                _graphCanvas.CapturePointer( e.Pointer );

            SetTransformFromTwoPoints(
                pointerInfo.Position,
                _pointerPressedPositionInGraph );

            ViewChangeEvent?.Invoke( null, null );
        }

        public double CurrentScale
        {
            get { return ((MatrixTransform)_graphCanvas.RenderTransform).Matrix.M11; }
        }

        internal MsaglPointerEventArgs CreatePointerEventArgs( PointerRoutedEventArgs e )
        {
            return new GraphViewerPointerEventArgs( e, this );
        }

        void GraphCanvasSizeChanged( object sender, SizeChangedEventArgs e )
        {
            if ( _drawingGraph == null )
                return;
            
            // keep the same zoom level
            double oldfit = GetFitFactor( e.PreviousSize );
            double fitNow = FitFactor;
            double scaleFraction = fitNow / oldfit;

            SetTransform( CurrentScale * scaleFraction, CurrentXOffset * scaleFraction, CurrentYOffset * scaleFraction );
        }

        protected double CurrentXOffset
        {
            get { return ((MatrixTransform)_graphCanvas.RenderTransform).Matrix.OffsetX; }
        }

        protected double CurrentYOffset
        {
            get { return ((MatrixTransform)_graphCanvas.RenderTransform).Matrix.OffsetY; }
        }

        /// <summary>
        /// 
        /// </summary>
        public double ZoomFactor
        {
            get { return CurrentScale / FitFactor; }
        }

        #region IViewer

        public event EventHandler<EventArgs> ViewChangeEvent;

        public event EventHandler<MsaglPointerEventArgs> PointerPressed;
        public event EventHandler<MsaglPointerEventArgs> PointerMoved;
        public event EventHandler<MsaglPointerEventArgs> PointerReleased;

        public event EventHandler<ObjectUnderMouseCursorChangedEventArgs> ObjectUnderMouseCursorChanged;

        public IViewerObject ObjectUnderPointer
        {
            get
            {
                // This function can bring a stale object. 
                var location = PointerHelpers.GetPointerPosition(); // _graphCanvas );

                if ( !(_objectUnderMouseDetectionLocation == location) )
                    UpdateHitObjectUnderPointerPressedLocation( location );

                return GetIViewerObjectFromObjectUnderCursor( _objectUnderPointer );
            }

            private set
            {
                var previousObject = _objectUnderPointer;
                bool callSelectionChanged = _objectUnderPointer != value && ObjectUnderMouseCursorChanged != null;

                _objectUnderPointer = value;

                if ( callSelectionChanged )
                    ObjectUnderMouseCursorChanged( this, new ObjectUnderMouseCursorChangedEventArgs(
                        GetIViewerObjectFromObjectUnderCursor( previousObject ),
                        GetIViewerObjectFromObjectUnderCursor( _objectUnderPointer ) ) );
            }
        }

        IViewerObject GetIViewerObjectFromObjectUnderCursor( object obj )
        {
            if ( obj == null )
                return null;
            return obj as IViewerObject;
        }

        public void Invalidate( IViewerObject objectToInvalidate )
        {
            ((IInvalidatable)objectToInvalidate).Invalidate();
        }

        public void Invalidate()
        {
            //todo: is it right to do nothing
        }

        public event EventHandler GraphChanged;

        public ModifierKeys ModifierKeys
        {
            get
            {
                //FIXME: 

                ModifierKeys Keyboard = ModifierKeys.None;
                switch ( Keyboard )
                {
                    case ModifierKeys.Alt:
                        return ModifierKeys.Alt;
                    case ModifierKeys.Control:
                        return ModifierKeys.Control;
                    case ModifierKeys.None:
                        return ModifierKeys.None;
                    case ModifierKeys.Shift:
                        return ModifierKeys.Shift;
                    case ModifierKeys.Windows:
                        return ModifierKeys.Windows;

                    default:
                        return ModifierKeys.None;
                }
            }

        }

        public Point ScreenToSource( MsaglPointerEventArgs e )
        {
            var p = new Point( e.X, e.Y );
            var m = Transform.Inverse;

            return m * p;
        }

        public IEnumerable<IViewerObject> Entities
        {
            get
            {
                foreach ( var viewerObject in drawingObjectsToIViewerObjects.Values )
                {
                    yield return viewerObject;
                    var edge = viewerObject as VEdge;
                    if ( edge != null )
                        if ( edge.VLabel != null )
                            yield return edge.VLabel;
                }
            }
        }

        internal static double DpiXStatic
        {
            get
            {
                if ( _dpiX == 0 )
                    GetDpi();
                return _dpiX;
            }
        }

        static void GetDpi()
        {
            DisplayInformation displayInformation = DisplayInformation.GetForCurrentView();

            _dpiX = displayInformation.RawDpiX;
            _dpiY = displayInformation.RawDpiY;
        }

        public double DpiX
        {
            get { return DpiXStatic; }
        }

        public double DpiY
        {
            get { return DpiYStatic; }
        }

        static double DpiYStatic
        {
            get
            {
                if ( _dpiX == 0 )
                    GetDpi();
                return _dpiY;
            }
        }

        public void OnDragEnd( IEnumerable<IViewerObject> changedObjects )
        {
            throw new NotImplementedException();
        }

        public double LineThicknessForEditing { get; set; }

        /// <summary>
        /// The layout editing with the mouse is enabled if and only if this field is set to false
        /// </summary>
        public bool LayoutEditingEnabled { get; set; }

        public bool InsertingEdge { get; set; }

        public void PopupMenus( params Tuple<string, VoidDelegate>[] menuItems )
        {
            var contextMenu = new MenuFlyout();

            foreach ( var pair in menuItems )
                contextMenu.Items.Add( CreateMenuItem( pair.Item1, pair.Item2 ) );

            contextMenu.Closed += ContextMenuClosed;

            contextMenu.ShowAt( _graphCanvas );
        }

        void ContextMenuClosed( object sender, object e )
        {
            // FIXME: ContextMenuService.SetContextMenu( _graphCanvas, null );
        }

        public static MenuFlyoutItem CreateMenuItem( string text, VoidDelegate voidVoidDelegate )
        {
            var menuItem = new MenuFlyoutItem { Text = text };
            menuItem.Click += (RoutedEventHandler)(delegate { voidVoidDelegate(); });

            return menuItem;
        }

        public double UnderlyingPolylineCircleRadius
        {
            get { return 0.1 * DpiX / CurrentScale; }
        }

        public Graph Graph
        {
            get
            {
                return _drawingGraph;
            }

            set
            {
                _drawingGraph = value;

                if ( _drawingGraph != null )
                    Debug.WriteLine( "starting processing a graph with {0} nodes and {1} edges", _drawingGraph.NodeCount, _drawingGraph.EdgeCount );

                ProcessGraph();
            }
        }
        
        //
        //        void Dumpxy() {
        //            using (System.IO.StreamWriter file = new System.IO.StreamWriter(@"C:\tmp\dumpxy")) {
        //                file.WriteLine("~nodes");
        //                foreach (var node in Graph.Nodes) {
        //                    var c = node.GeometryNode.Center;
        //                    file.WriteLine("{0} {1} {2}", node.Id, c.X, c.Y);
        //                }
        //                file.WriteLine("~edges");
        //                foreach (var edge in Graph.Edges)
        //                {
        //                    file.WriteLine("{0} {1}", edge.Source, edge.Target);
        //                }
        //            }
        //        }

        const double DesiredPathThicknessInInches = 0.016;

        readonly Dictionary<DrawingObject, Func<DrawingObject, FrameworkElement>> registeredCreators =
            new Dictionary<DrawingObject, Func<DrawingObject, FrameworkElement>>();

        readonly ClickCounter clickCounter;

        public string MsaglFileToSave;

        double GetBorderPathThickness()
        {
            return DesiredPathThicknessInInches * DpiX / CurrentScale;
        }

        readonly Object _processGraphLock = new object();

        void ProcessGraph()
        {
            lock ( _processGraphLock )
            {
                ProcessGraphUnderLock();
            }
        }

        private void ProcessGraphUnderLock()
        {
            try
            {
                LayoutStarted?.Invoke( null, null );

                CancelToken = new CancelToken();

                if ( _drawingGraph == null )
                    return;

                HideCanvas();
                ClearGraphViewer();
                CreateFrameworkElementsForLabelsOnly();

                if ( NeedToCalculateLayout )
                {
                    _drawingGraph.CreateGeometryGraph();

                    // forcing the layout recalculation
                    if ( _graphCanvas.Dispatcher.HasThreadAccess )
                    {
                        PopulateGeometryOfGeometryGraph();
                    }
                    else
                    {
                        // FIXME:
                        //await _graphCanvas.Dispatcher.RunAsync( CoreDispatcherPriority.Normal, PopulateGeometryOfGeometryGraph );
                    }
                }

                geometryGraphUnderLayout = _drawingGraph.GeometryGraph;

                if ( RunLayoutAsync )
                    SetUpBackgrounWorkerAndRunAsync();
                else
                    RunLayoutInUIThread();
            }
            catch ( Exception e )
            {
                // FIXME: MessageBox.Show( e.ToString() );
            }
        }

        void RunLayoutInUIThread()
        {
            LayoutGraph();
            PostLayoutStep();

            LayoutComplete?.Invoke( null, null );
        }

        void SetUpBackgrounWorkerAndRunAsync()
        {
            _backgroundWorker = new BackgroundWorker();
            _backgroundWorker.DoWork += ( a, b ) => LayoutGraph();
            _backgroundWorker.RunWorkerCompleted += ( sender, args ) =>
            {
                if ( args.Error != null )
                {
                    //var dialog = new MessageDialog( args.Error.ToString() );
                    //await dialog.ShowAsync();
                    // Fixme: MessageBox.Show( args.Error.ToString() );
                    ClearGraphViewer();
                }
                else if ( CancelToken.Canceled )
                {
                    ClearGraphViewer();
                }
                else
                {
                    if ( _graphCanvas.Dispatcher.HasThreadAccess )
                    {
                        PostLayoutStep();
                    }
                    else
                    {
                        //_graphCanvas.Dispatcher.RunAsync( CoreDispatcherPriority.Normal, PostLayoutStep );
                    }
                }

                _backgroundWorker = null; //this will signal that we are not under layout anymore          

                LayoutComplete?.Invoke( null, null );
            };

            _backgroundWorker.RunWorkerAsync();
        }

        async void HideCanvas()
        {
            if ( _graphCanvas.Dispatcher.HasThreadAccess )
                _graphCanvas.Visibility = Visibility.Collapsed; // hide canvas while we lay it out asynchronously.
            else
                await _graphCanvas.Dispatcher.RunAsync( CoreDispatcherPriority.Normal, () => _graphCanvas.Visibility = Visibility.Collapsed );
        }

        void LayoutGraph()
        {
            if ( NeedToCalculateLayout )
            {
                try
                {
                    LayoutHelpers.CalculateLayout( geometryGraphUnderLayout, _drawingGraph.LayoutAlgorithmSettings,
                                                  CancelToken );
                    if ( MsaglFileToSave != null )
                    {
                        _drawingGraph.Write( MsaglFileToSave );
                        Debug.WriteLine( "saved into {0}", MsaglFileToSave );

                        // FIXME: Environment.Exit( 0 ); Why
                        Application.Current.Exit();
                    }
                }
                catch ( OperationCanceledException )
                {
                    //swallow this exception
                }
            }
        }

        void PostLayoutStep()
        {
            _graphCanvas.Visibility = Visibility.Visible;
            PushDataFromLayoutGraphToFrameworkElements();
            _backgroundWorker = null; //this will signal that we are not under layout anymore

            GraphChanged?.Invoke( this, null );

            SetInitialTransform();
        }

        /*
                void SubscribeToChangeVisualsEvents() {
                    //            foreach(var cluster in drawingGraph.RootSubgraph.AllSubgraphsDepthFirstExcludingSelf())
                    //                cluster.Attr.VisualsChanged += AttrVisualsChanged;
                    foreach (var edge in drawingGraph.Edges) {
                        DrawingEdge edge1 = edge;
                        edge.Attr.VisualsChanged += (a, b) => AttrVisualsChangedForEdge(edge1);
                    }

                    foreach (var node in drawingGraph.Nodes) {
                        Drawing.Node node1 = node;
                        node.Attr.VisualsChanged += (a, b) => AttrVisualsChangedForNode(node1);
                    }

                }
        */

        /*
                void AttrVisualsChangedForNode(Drawing.Node node) {
                    IViewerObject viewerObject;
                    if (drawingObjectsToIViewerObjects.TryGetValue(node, out viewerObject)) {
                        var vNode = (VNode) viewerObject;
                        if (vNode != null)
                            vNode.Invalidate();
                    }
                }
        */

        //        void SetupTimerOnViewChangeEvent(object sender, EventArgs e) {
        //            SetupRoutingTimer();
        //        }

        /*
                void TestCorrectness(GeometryGraph oGraph, Set<Drawing.Node> oDrawingNodes, Set<DrawingEdge> oDrawgingEdges) {
                    if (Entities.Count() != oGraph.Nodes.Count + oGraph.Edges.Count) {
                        foreach (var newDrawingNode in oDrawingNodes) {
                            if (!drawingObjectsToIViewerObjects.ContainsKey(newDrawingNode))
                                Debug.WriteLine();
                        }
                        foreach (var drawingEdge in oDrawgingEdges) {
                            if (!drawingObjectsToIViewerObjects.ContainsKey(drawingEdge))
                                Debug.WriteLine();
                        }
                        foreach (var viewerObject in Entities) {
                            if (viewerObject is VEdge) {
                                Debug.Assert(oDrawgingEdges.Contains(viewerObject.DrawingObject));
                            } else {
                                if (viewerObject is VNode) {
                                    Debug.Assert(oDrawingNodes.Contains(viewerObject.DrawingObject));
                                } else {
                                    Debug.Fail("expecting a node or an edge");
                                }
                            }

                        }

                    }
                }
        */

        /// <summary>
        /// creates a viewer node
        /// </summary>
        /// <param name="drawingNode"></param>
        /// <returns></returns>
        public IViewerNode CreateIViewerNode( Drawing.Node drawingNode )
        {
            var frameworkElement = CreateTextBlockForDrawingObj( drawingNode );
            var width = frameworkElement.Width + 2 * drawingNode.Attr.LabelMargin;
            var height = frameworkElement.Height + 2 * drawingNode.Attr.LabelMargin;
            var bc = NodeBoundaryCurves.GetNodeBoundaryCurve( drawingNode, width, height );
            drawingNode.GeometryNode = new Node( bc, drawingNode );
            var vNode = CreateVNode( drawingNode );
            layoutEditor.AttachLayoutChangeEvent( vNode );

            return vNode;
        }

        void ClearGraphViewer()
        {
            ClearGraphCanvasChildren();

            drawingObjectsToIViewerObjects.Clear();
            drawingObjectsToFrameworkElements.Clear();
        }

        async void ClearGraphCanvasChildren()
        {
            if ( _graphCanvas.Dispatcher.HasThreadAccess )
                _graphCanvas.Children.Clear();
            else 
                await _graphCanvas.Dispatcher.RunAsync( CoreDispatcherPriority.Normal, () => _graphCanvas.Children.Clear() );
        }

        /*
                void StartLayoutCalculationInThread() {
                    PushGeometryIntoLayoutGraph();
                    graphCanvas.RaiseEvent(new RoutedEventArgs(LayoutStartEvent));

                    layoutThread =
                        new Thread(
                            () =>
                            LayoutHelpers.CalculateLayout(geometryGraphUnderLayout, graph.LayoutAlgorithmSettings));

                    layoutThread.Start();

                    //the timer monitors the thread and then pushes the data from layout graph to the framework
                    layoutThreadCheckingTimer.IsEnabled = true;
                    layoutThreadCheckingTimer.Tick += LayoutThreadCheckingTimerTick;
                    layoutThreadCheckingTimer.Interval = new TimeSpan((long) 10e6);
                    layoutThreadCheckingTimer.Start();
                }
        */

        /*
                void LayoutThreadCheckingTimerTick(object sender, EventArgs e) {
                    if (layoutThread.IsAlive)
                        return;

                    if (Monitor.TryEnter(layoutThreadCheckingTimer)) {
                        if (layoutThreadCheckingTimer.IsEnabled == false)
                            return; //somehow it is called on more time after stopping and disabling
                        layoutThreadCheckingTimer.Stop();
                        layoutThreadCheckingTimer.IsEnabled = false;

                        TransferLayoutDataToWpf();

                        graphCanvas.RaiseEvent(new RoutedEventArgs(LayoutEndEvent));
                        if (LayoutComplete != null) 
                            LayoutComplete(this, new EventArgs());               
                    }
                }
        */

        //        void TransferLayoutDataToWpf() {
        //            PushDataFromLayoutGraphToFrameworkElements();
        //            graphCanvas.Visibility = Visibility.Visible;
        //            SetInitialTransform();
        //        }
        /// <summary>
        /// zooms to the default view
        /// </summary>
        public void SetInitialTransform()
        {
            if ( _drawingGraph == null || GeomGraph == null ) return;

            var scale = FitFactor;
            var graphCenter = GeomGraph.BoundingBox.Center;
            var vp = new Rectangle( new Point( 0, 0 ),
                                   new Point( _graphCanvas.RenderSize.Width, _graphCanvas.RenderSize.Height ) );

            SetTransformOnViewportWithoutRaisingViewChangeEvent( scale, graphCenter, vp );
        }

        public Image DrawImage( string fileName )
        {
            //var ltrans = _graphCanvas.LayoutTransform;
            var rtrans = _graphCanvas.RenderTransform;
            //_graphCanvas.LayoutTransform = null;
            _graphCanvas.RenderTransform = null;
            var renderSize = _graphCanvas.RenderSize;

            double scale = FitFactor;
            int w = (int)(GeomGraph.Width * scale);
            int h = (int)(GeomGraph.Height * scale);

            SetTransformOnViewportWithoutRaisingViewChangeEvent( scale, GeomGraph.BoundingBox.Center, new Rectangle( 0, 0, w, h ) );

            Size size = new Size( w, h );
            // Measure and arrange the surface
            // VERY IMPORTANT
            _graphCanvas.Measure( size );
            _graphCanvas.Arrange( new Rect( new UwpPoint( 0, 0 ), size ) );

            foreach ( var node in _drawingGraph.Nodes.Concat( _drawingGraph.RootSubgraph.AllSubgraphsDepthFirstExcludingSelf() ) )
            {
                IViewerObject o;
                if ( drawingObjectsToIViewerObjects.TryGetValue( node, out o ) )
                {
                    ((VNode)o).Invalidate();
                }
            }

            RenderTargetBitmap renderBitmap = new RenderTargetBitmap();
            //.PixelHeight = h;
            // w, h, DpiX, DpiY, PixelFormats.Pbgra32 );

            //await renderBitmap.RenderAsync( _graphCanvas );

            if ( fileName != null )
                // Create a file stream for saving image
                using ( System.IO.FileStream outStream = new System.IO.FileStream( fileName, System.IO.FileMode.Create ) )
                {
                    //FIXME:
                    //// Use png encoder for our data
                    //PngBitmapEncoder encoder = new PngBitmapEncoder();
                    //// push the rendered bitmap to it
                    //encoder.Frames.Add( BitmapFrame.Create( renderBitmap ) );
                    //// save the data to the stream
                    //encoder.Save( outStream );
                }

            //_graphCanvas.LayoutTransform = ltrans;
            _graphCanvas.RenderTransform = rtrans;
            _graphCanvas.Measure( renderSize );
            _graphCanvas.Arrange( new Rect( new UwpPoint( 0, 0 ), renderSize ) );
            
            return new Image { Source = renderBitmap };
        }

        void SetTransformOnViewportWithoutRaisingViewChangeEvent( double scale, Point graphCenter, Rectangle vp )
        {
            var dx = vp.Width / 2 - scale * graphCenter.X;
            var dy = vp.Height / 2 + scale * graphCenter.Y;

            SetTransformWithoutRaisingViewChangeEvent( scale, dx, dy );

        }


        /*
                void FixArrowheads(LgLayoutSettings lgSettings) {
                    const double arrowheadRatioToBoxDiagonal = 0.3;
                    var maximalArrowheadLength = lgSettings.MaximalArrowheadLength();
                    if (lgSettings.OGraph == null) return;
                    foreach (Edge geomEdge in lgSettings.OGraph.Edges) {

                        var edge = (DrawingEdge) geomEdge.UserData;
                        var vEdge = (VEdge) drawingObjectsToIViewerObjects[edge];

                        if (geomEdge.EdgeGeometry.SourceArrowhead != null) {
                            var origLength = vEdge.EdgeAttrClone.ArrowheadLength;
                            geomEdge.EdgeGeometry.SourceArrowhead.Length =
                                Math.Min(Math.Min(origLength, maximalArrowheadLength),
                                         geomEdge.Source.BoundingBox.Diagonal*arrowheadRatioToBoxDiagonal);
                        }
                        if (geomEdge.EdgeGeometry.TargetArrowhead != null) {
                            var origLength = vEdge.EdgeAttrClone.ArrowheadLength;
                            geomEdge.EdgeGeometry.TargetArrowhead.Length =
                                Math.Min(Math.Min(origLength, maximalArrowheadLength),
                                         geomEdge.Target.BoundingBox.Diagonal*arrowheadRatioToBoxDiagonal);
                        }
                    }
                }
        */



        public Rectangle ClientViewportMappedToGraph
        {
            get
            {
                var t = Transform.Inverse;
                var p0 = new Point( 0, 0 );
                var p1 = new Point( _graphCanvas.RenderSize.Width, _graphCanvas.RenderSize.Height );
                return new Rectangle( t * p0, t * p1 );
            }
        }

        void SetTransform( double scale, double dx, double dy )
        {
            if ( ScaleIsOutOfRange( scale ) )
                return;
            _graphCanvas.RenderTransform = new MatrixTransform() { Matrix = new Matrix( scale, 0, 0, -scale, dx, dy ) };

            ViewChangeEvent?.Invoke( null, null );
        }

        void SetTransformWithoutRaisingViewChangeEvent( double scale, double dx, double dy )
        {
            if ( ScaleIsOutOfRange( scale ) )
                return;

            _graphCanvas.RenderTransform = new MatrixTransform() { Matrix = new Matrix( scale, 0, 0, -scale, dx, dy ) };
        }

        bool ScaleIsOutOfRange( double scale )
        {
            return scale < 0.000001 || scale > 100000.0; //todo: remove hardcoded values
        }

        double FitFactor
        {
            get
            {
                var geomGraph = GeomGraph;

                if ( _drawingGraph == null || geomGraph == null ||

                    geomGraph.Width == 0 || geomGraph.Height == 0 )
                    return 1;

                var size = _graphCanvas.RenderSize;

                return GetFitFactor( size );
            }
        }

        double GetFitFactor( Size rect )
        {
            var geomGraph = GeomGraph;
            return geomGraph == null ? 1 : Math.Min( rect.Width / geomGraph.Width, rect.Height / geomGraph.Height );
        }

        void PushDataFromLayoutGraphToFrameworkElements()
        {
            CreateRectToFillCanvas();
            CreateAndPositionGraphBackgroundRectangle();
            CreateVNodes();
            CreateEdges();
        }

        void CreateRectToFillCanvas()
        {
            var parent = (Panel)GraphCanvas.Parent;
            _rectToFillCanvas = new XamlShapes.Rectangle();

            Canvas.SetLeft( _rectToFillCanvas, 0 );
            Canvas.SetTop( _rectToFillCanvas, 0 );

            _rectToFillCanvas.Width = parent.ActualWidth;
            _rectToFillCanvas.Height = parent.ActualHeight;

            _rectToFillCanvas.Fill = new SolidColorBrush( Colors.Transparent );
            Canvas.SetZIndex( _rectToFillCanvas, -2 );

            _graphCanvas.Children.Add( _rectToFillCanvas );
        }

        void CreateEdges()
        {
            foreach ( var edge in _drawingGraph.Edges )
                CreateEdge( edge, null );
        }

        VEdge CreateEdge( DrawingEdge edge, LgLayoutSettings lgSettings )
        {
            lock ( this )
            {
                if ( drawingObjectsToIViewerObjects.ContainsKey( edge ) )
                    return (VEdge)drawingObjectsToIViewerObjects[ edge ];
                if ( lgSettings != null )
                    return CreateEdgeForLgCase( lgSettings, edge );

                FrameworkElement labelTextBox;
                drawingObjectsToFrameworkElements.TryGetValue( edge, out labelTextBox );
                var vEdge = new VEdge( edge, labelTextBox );

                var zIndex = ZIndexOfEdge( edge );
                drawingObjectsToIViewerObjects[ edge ] = vEdge;

                if ( edge.Label != null )
                    SetVEdgeLabel( edge, vEdge, zIndex );

                Canvas.SetZIndex( vEdge.CurvePath, zIndex );
                _graphCanvas.Children.Add( vEdge.CurvePath );
                SetVEdgeArrowheads( vEdge, zIndex );

                return vEdge;
            }
        }

        int ZIndexOfEdge( DrawingEdge edge )
        {
            var source = (VNode)drawingObjectsToIViewerObjects[ edge.SourceNode ];
            var target = (VNode)drawingObjectsToIViewerObjects[ edge.TargetNode ];

            var zIndex = Math.Max( source.ZIndex, target.ZIndex ) + 1;
            return zIndex;
        }

        VEdge CreateEdgeForLgCase( LgLayoutSettings lgSettings, DrawingEdge edge )
        {
            return (VEdge)(drawingObjectsToIViewerObjects[ edge ] = new VEdge( edge, lgSettings )
            {
                PathStrokeThicknessFunc = () => GetBorderPathThickness() * edge.Attr.LineWidth
            });
        }

        void SetVEdgeLabel( DrawingEdge edge, VEdge vEdge, int zIndex )
        {
            FrameworkElement frameworkElementForEdgeLabel;

            if ( !drawingObjectsToFrameworkElements.TryGetValue( edge, out frameworkElementForEdgeLabel ) )
            {
                drawingObjectsToFrameworkElements[ edge ] =
                    frameworkElementForEdgeLabel = CreateTextBlockForDrawingObj( edge );
                frameworkElementForEdgeLabel.Tag = new VLabel( edge, frameworkElementForEdgeLabel );
            }

            vEdge.VLabel = (VLabel)frameworkElementForEdgeLabel.Tag;

            if ( frameworkElementForEdgeLabel.Parent == null )
            {
                _graphCanvas.Children.Add( frameworkElementForEdgeLabel );
                Canvas.SetZIndex( frameworkElementForEdgeLabel, zIndex );
            }
        }

        void SetVEdgeArrowheads( VEdge vEdge, int zIndex )
        {
            if ( vEdge.SourceArrowHeadPath != null )
            {
                Canvas.SetZIndex( vEdge.SourceArrowHeadPath, zIndex );
                _graphCanvas.Children.Add( vEdge.SourceArrowHeadPath );
            }
            if ( vEdge.TargetArrowHeadPath != null )
            {
                Canvas.SetZIndex( vEdge.TargetArrowHeadPath, zIndex );
                _graphCanvas.Children.Add( vEdge.TargetArrowHeadPath );
            }
        }

        void CreateVNodes()
        {
            foreach ( var node in _drawingGraph.Nodes.Concat( _drawingGraph.RootSubgraph.AllSubgraphsDepthFirstExcludingSelf() ) )
            {
                CreateVNode( node );
                Invalidate( drawingObjectsToIViewerObjects[ node ] );
            }
        }

        IViewerNode CreateVNode( Drawing.Node node )
        {
            lock ( this )
            {
                if ( drawingObjectsToIViewerObjects.ContainsKey( node ) )
                    return (IViewerNode)drawingObjectsToIViewerObjects[ node ];

                FrameworkElement feOfLabel;
                if ( !drawingObjectsToFrameworkElements.TryGetValue( node, out feOfLabel ) )
                    feOfLabel = CreateAndRegisterFrameworkElementOfDrawingNode( node );

                var vn = new VNode( node, feOfLabel,
                    e => (VEdge)drawingObjectsToIViewerObjects[ e ], () => GetBorderPathThickness() * node.Attr.LineWidth );

                foreach ( var fe in vn.FrameworkElements )
                    _graphCanvas.Children.Add( fe );

                drawingObjectsToIViewerObjects[ node ] = vn;

                #region commented out animation

                /* //playing with the animation
                p.Fill = Brushes.Green;

                SolidColorBrush brush = new SolidColorBrush();
                p.Fill = brush;
                ColorAnimation ca = new ColorAnimation(Colors.Green, Colors.White, new Duration(TimeSpan.FromMilliseconds(3000)));
                //Storyboard sb = new Storyboard();
                //Storyboard.SetTargetProperty(ca, new PropertyPath("Color"));
                //Storyboard.SetTarget(ca, brush);            
                //sb.Children.Add(ca);
                //sb.Begin(p);
                brush.BeginAnimation(SolidColorBrush.ColorProperty, ca);
                */

                #endregion

                return vn;
            }
        }

        public FrameworkElement CreateAndRegisterFrameworkElementOfDrawingNode( Drawing.Node node )
        {
            lock ( this )
                return drawingObjectsToFrameworkElements[ node ] = CreateTextBlockForDrawingObj( node );
        }

        void CreateAndPositionGraphBackgroundRectangle()
        {
            CreateGraphBackgroundRect();
            SetBackgroundRectanglePositionAndSize();

            var rect = _rectToFillGraphBackground as XamlShapes.Rectangle;

            if ( rect != null )
            {
                rect.Fill = Common.BrushFromMsaglColor( _drawingGraph.Attr.BackgroundColor );
                //rect.Fill = Brushes.Green;
            }

            Canvas.SetZIndex( _rectToFillGraphBackground, -1 );

            _graphCanvas.Children.Add( _rectToFillGraphBackground );
        }

        void CreateGraphBackgroundRect()
        {
            var lgGraphBrowsingSettings = _drawingGraph.LayoutAlgorithmSettings as LgLayoutSettings;

            if ( lgGraphBrowsingSettings == null )
            {
                _rectToFillGraphBackground = new XamlShapes.Rectangle();
            }
        }


        void SetBackgroundRectanglePositionAndSize()
        {
            if ( GeomGraph == null )
                return;
            //            Canvas.SetLeft(_rectToFillGraphBackground, geomGraph.Left);
            //            Canvas.SetTop(_rectToFillGraphBackground, geomGraph.Bottom);
            _rectToFillGraphBackground.Width = GeomGraph.Width;
            _rectToFillGraphBackground.Height = GeomGraph.Height;

            var center = GeomGraph.BoundingBox.Center;
            Common.PositionFrameworkElement( _rectToFillGraphBackground, center, 1 );
        }


        async void PopulateGeometryOfGeometryGraph()
        {
            geometryGraphUnderLayout = _drawingGraph.GeometryGraph;

            foreach ( Node msaglNode in geometryGraphUnderLayout.Nodes )
            {
                var node = (Drawing.Node)msaglNode.UserData;

                if ( _graphCanvas.Dispatcher.HasThreadAccess )
                    msaglNode.BoundaryCurve = GetNodeBoundaryCurve( node );
                else
                {
                    var msagNodeInThread = msaglNode;
                    await _graphCanvas.Dispatcher.RunAsync( CoreDispatcherPriority.Normal, () => msagNodeInThread.BoundaryCurve = GetNodeBoundaryCurve( node ) );
                }
                
                //AssignLabelWidthHeight(msaglNode, msaglNode.UserData as DrawingObject);
            }

            foreach ( Cluster cluster in geometryGraphUnderLayout.RootCluster.AllClustersWideFirstExcludingSelf() )
            {
                var subgraph = (Subgraph)cluster.UserData;

                if ( _graphCanvas.Dispatcher.HasThreadAccess )
                    cluster.CollapsedBoundary = GetClusterCollapsedBoundary( subgraph );
                else
                {
                    var clusterInThread = cluster;

                    await _graphCanvas.Dispatcher.RunAsync( CoreDispatcherPriority.Normal,
                        () => clusterInThread.BoundaryCurve = GetClusterCollapsedBoundary( subgraph ) );
                }

                if ( cluster.RectangularBoundary == null )
                    cluster.RectangularBoundary = new RectangularClusterBoundary();
                cluster.RectangularBoundary.TopMargin = subgraph.DiameterOfOpenCollapseButton + 0.5 +
                                                        subgraph.Attr.LineWidth / 2;
                //AssignLabelWidthHeight(msaglNode, msaglNode.UserData as DrawingObject);
            }

            foreach ( var msaglEdge in geometryGraphUnderLayout.Edges )
            {
                var drawingEdge = (DrawingEdge)msaglEdge.UserData;
                AssignLabelWidthHeight( msaglEdge, drawingEdge );
            }
        }

        ICurve GetClusterCollapsedBoundary( Subgraph subgraph )
        {
            double width, height;

            FrameworkElement fe;
            if ( drawingObjectsToFrameworkElements.TryGetValue( subgraph, out fe ) )
            {

                width = fe.Width + 2 * subgraph.Attr.LabelMargin + subgraph.DiameterOfOpenCollapseButton;
                height = Math.Max( fe.Height + 2 * subgraph.Attr.LabelMargin, subgraph.DiameterOfOpenCollapseButton );
            }
            else
                return GetApproximateCollapsedBoundary( subgraph );

            if ( width < _drawingGraph.Attr.MinNodeWidth )
                width = _drawingGraph.Attr.MinNodeWidth;
            if ( height < _drawingGraph.Attr.MinNodeHeight )
                height = _drawingGraph.Attr.MinNodeHeight;

            return NodeBoundaryCurves.GetNodeBoundaryCurve( subgraph, width, height );
        }

        ICurve GetApproximateCollapsedBoundary( Subgraph subgraph )
        {
            if ( textBoxForApproxNodeBoundaries == null )
                SetUpTextBoxForApproxNodeBoundaries();


            double width, height;
            if ( String.IsNullOrEmpty( subgraph.LabelText ) )
                height = width = subgraph.DiameterOfOpenCollapseButton;
            else
            {
                double a = ((double)subgraph.LabelText.Length) / textBoxForApproxNodeBoundaries.Text.Length *
                           subgraph.Label.FontSize / Label.DefaultFontSize;
                width = textBoxForApproxNodeBoundaries.Width * a + subgraph.DiameterOfOpenCollapseButton;
                height =
                    Math.Max(
                        textBoxForApproxNodeBoundaries.Height * subgraph.Label.FontSize / Label.DefaultFontSize,
                        subgraph.DiameterOfOpenCollapseButton );
            }

            if ( width < _drawingGraph.Attr.MinNodeWidth )
                width = _drawingGraph.Attr.MinNodeWidth;
            if ( height < _drawingGraph.Attr.MinNodeHeight )
                height = _drawingGraph.Attr.MinNodeHeight;

            return NodeBoundaryCurves.GetNodeBoundaryCurve( subgraph, width, height );
        }


        void AssignLabelWidthHeight( Core.Layout.ILabeledObject labeledGeomObj,
                                    DrawingObject drawingObj )
        {
            if ( drawingObjectsToFrameworkElements.ContainsKey( drawingObj ) )
            {
                FrameworkElement fe = drawingObjectsToFrameworkElements[ drawingObj ];
                labeledGeomObj.Label.Width = fe.Width;
                labeledGeomObj.Label.Height = fe.Height;
            }
        }


        ICurve GetNodeBoundaryCurve( Drawing.Node node )
        {
            double width, height;
            FrameworkElement fe;

            if ( drawingObjectsToFrameworkElements.TryGetValue( node, out fe ) )
            {
                width = fe.Width + 2 * node.Attr.LabelMargin;
                height = fe.Height + 2 * node.Attr.LabelMargin;
            }
            else
                return GetNodeBoundaryCurveByMeasuringText( node );

            if ( width < _drawingGraph.Attr.MinNodeWidth )
                width = _drawingGraph.Attr.MinNodeWidth;

            if ( height < _drawingGraph.Attr.MinNodeHeight )
                height = _drawingGraph.Attr.MinNodeHeight;

            return NodeBoundaryCurves.GetNodeBoundaryCurve( node, width, height );
        }

        TextBlock textBoxForApproxNodeBoundaries;

        public static Size MeasureText( string text, FontFamily fontFamily, double fontSize )
        {
            var tb = new TextBlock { Text = text, FontSize = fontSize };
            tb.Measure( new Size( Double.PositiveInfinity, Double.PositiveInfinity ) );

            return tb.DesiredSize;

            //FormattedText formattedText = new FormattedText(
            //    text,
            //    System.Globalization.CultureInfo.CurrentCulture,
            //    FlowDirection.LeftToRight,
            //    new Typeface( family, new System.Windows.FontStyle(), FontWeights.Regular, FontStretches.Normal ),
            //    size,
            //    Brushes.Black,
            //    null );

            //return new Size( formattedText.Width, formattedText.Height );
        }

        ICurve GetNodeBoundaryCurveByMeasuringText( Drawing.Node node )
        {
            double width, height;
            if ( String.IsNullOrEmpty( node.LabelText ) )
            {
                width = 10;
                height = 10;
            }
            else
            {
                var size = MeasureText( node.LabelText, new FontFamily( node.Label.FontName ), node.Label.FontSize );
                width = size.Width;
                height = size.Height;
            }

            width += 2 * node.Attr.LabelMargin;
            height += 2 * node.Attr.LabelMargin;

            if ( width < _drawingGraph.Attr.MinNodeWidth )
                width = _drawingGraph.Attr.MinNodeWidth;
            if ( height < _drawingGraph.Attr.MinNodeHeight )
                height = _drawingGraph.Attr.MinNodeHeight;

            return NodeBoundaryCurves.GetNodeBoundaryCurve( node, width, height );
        }

        void SetUpTextBoxForApproxNodeBoundaries()
        {
            textBoxForApproxNodeBoundaries = new TextBlock
            {
                Text = "Fox jumping over River",
                FontFamily = new FontFamily( Label.DefaultFontName ),
                FontSize = Label.DefaultFontSize,
            };

            textBoxForApproxNodeBoundaries.Measure( new Size( double.PositiveInfinity, double.PositiveInfinity ) );
            textBoxForApproxNodeBoundaries.Width = textBoxForApproxNodeBoundaries.DesiredSize.Width;
            textBoxForApproxNodeBoundaries.Height = textBoxForApproxNodeBoundaries.DesiredSize.Height;
        }

        async void CreateFrameworkElementsForLabelsOnly()
        {
            foreach ( var edge in _drawingGraph.Edges )
            {
                var fe = CreateDefaultFrameworkElementForDrawingObject( edge );
                if ( fe != null )
                {
                    if ( _graphCanvas.Dispatcher.HasThreadAccess )
                        fe.Tag = new VLabel( edge, fe );
                    else
                    {
                        var localEdge = edge;
                        await _graphCanvas.Dispatcher.RunAsync( CoreDispatcherPriority.Normal, () => fe.Tag = new VLabel( localEdge, fe ) );
                    }
                }
            }

            foreach ( var node in _drawingGraph.Nodes )
                CreateDefaultFrameworkElementForDrawingObject( node );

            if ( _drawingGraph.RootSubgraph != null )
                foreach ( var subgraph in _drawingGraph.RootSubgraph.AllSubgraphsWidthFirstExcludingSelf() )
                    CreateDefaultFrameworkElementForDrawingObject( subgraph );
        }

        //        void CreateFrameworkElementForEdgeLabel(DrawingEdge edge) {
        //            var textBlock = CreateTextBlockForDrawingObj(edge);
        //            if (textBlock == null) return;
        //            drawingGraphObjectsToTextBoxes[edge] = textBlock;            
        //            textBlock.Tag = new VLabel(edge, textBlock);
        //        }

        public void RegisterLabelCreator( DrawingObject drawingObject, Func<DrawingObject, FrameworkElement> func )
        {
            registeredCreators[ drawingObject ] = func;
        }

        public void UnregisterLabelCreator( DrawingObject drawingObject )
        {
            registeredCreators.Remove( drawingObject );
        }

        public Func<DrawingObject, FrameworkElement> GetLabelCreator( DrawingObject drawingObject )
        {
            return registeredCreators[ drawingObject ];
        }

        FrameworkElement CreateTextBlockForDrawingObj( DrawingObject drawingObj )
        {
            Func<DrawingObject, FrameworkElement> registeredCreator;

            if ( registeredCreators.TryGetValue( drawingObj, out registeredCreator ) )
                return registeredCreator( drawingObj );

            if ( drawingObj is Subgraph )
                return null; //todo: add Label support later

            var labeledObj = drawingObj as ILabeledObject;

            if ( labeledObj == null )
                return null;

            var drawingLabel = labeledObj.Label;

            if ( drawingLabel == null )
                return null;

            TextBlock textBlock = null;

            if ( _graphCanvas.Dispatcher.HasThreadAccess )
            {
                textBlock = CreateTextBlock( drawingLabel );
            }
            else
            {
                //await _graphCanvas.Dispatcher.RunAsync( CoreDispatcherPriority.Normal, () => textBlock = CreateTextBlock( drawingLabel ) );
            }

            return textBlock;
        }

        static TextBlock CreateTextBlock( Label drawingLabel )
        {
            var textBlock = new TextBlock
            {
                Tag = drawingLabel,
                Text = drawingLabel.Text,
                FontFamily = new FontFamily( drawingLabel.FontName ),
                FontSize = drawingLabel.FontSize,
                Foreground = Common.BrushFromMsaglColor( drawingLabel.FontColor )
            };

            textBlock.Measure( new Size( double.PositiveInfinity, double.PositiveInfinity ) );
            textBlock.Width = textBlock.DesiredSize.Width;
            textBlock.Height = textBlock.DesiredSize.Height;

            return textBlock;
        }

        FrameworkElement CreateDefaultFrameworkElementForDrawingObject( DrawingObject drawingObject )
        {
            lock ( this )
            {
                var textBlock = CreateTextBlockForDrawingObj( drawingObject );

                if ( textBlock != null )
                    drawingObjectsToFrameworkElements[ drawingObject ] = textBlock;

                return textBlock;
            }
        }

        public void DrawRubberLine( MsaglPointerEventArgs args )
        {
            DrawRubberLine( ScreenToSource( args ) );
        }

        public void StopDrawingRubberLine()
        {
            _graphCanvas.Children.Remove( _rubberLinePath );
            _rubberLinePath = null;

            _graphCanvas.Children.Remove( _targetArrowheadPathForRubberEdge );
            _targetArrowheadPathForRubberEdge = null;
        }

        public void AddEdge( IViewerEdge edge, bool registerForUndo )
        {
            //if (registerForUndo) drawingLayoutEditor.RegisterEdgeAdditionForUndo(edge);

            var drawingEdge = edge.Edge;
            Edge geomEdge = drawingEdge.GeometryEdge;

            _drawingGraph.AddPrecalculatedEdge( drawingEdge );
            _drawingGraph.GeometryGraph.Edges.Add( geomEdge );

        }

        public IViewerEdge CreateEdgeWithGivenGeometry( DrawingEdge drawingEdge )
        {
            return CreateEdge( drawingEdge, _drawingGraph.LayoutAlgorithmSettings as LgLayoutSettings );
        }

        public void AddNode( IViewerNode node, bool registerForUndo )
        {
            if ( _drawingGraph == null )
                throw new InvalidOperationException(); // adding a node when the graph does not exist
            var vNode = (VNode)node;
            _drawingGraph.AddNode( vNode.Node );
            _drawingGraph.GeometryGraph.Nodes.Add( vNode.Node.GeometryNode );
            layoutEditor.AttachLayoutChangeEvent( vNode );
            _graphCanvas.Children.Add( vNode.FrameworkElementOfNodeForLabel );
            layoutEditor.CleanObstacles();
        }

        public IViewerObject AddNode( Drawing.Node drawingNode )
        {
            Graph.AddNode( drawingNode );
            var vNode = CreateVNode( drawingNode );
            LayoutEditor.AttachLayoutChangeEvent( vNode );
            LayoutEditor.CleanObstacles();

            return vNode;
        }

        public void RemoveEdge( IViewerEdge edge, bool registerForUndo )
        {
            lock ( this )
            {
                var vedge = (VEdge)edge;
                var dedge = vedge.Edge;
                _drawingGraph.RemoveEdge( dedge );
                _drawingGraph.GeometryGraph.Edges.Remove( dedge.GeometryEdge );
                drawingObjectsToFrameworkElements.Remove( dedge );
                drawingObjectsToIViewerObjects.Remove( dedge );

                vedge.RemoveItselfFromCanvas( _graphCanvas );
            }
        }

        public void RemoveNode( IViewerNode node, bool registerForUndo )
        {
            lock ( this )
            {
                RemoveEdges( node.Node.OutEdges );
                RemoveEdges( node.Node.InEdges );
                RemoveEdges( node.Node.SelfEdges );
                drawingObjectsToFrameworkElements.Remove( node.Node );
                drawingObjectsToIViewerObjects.Remove( node.Node );
                var vnode = (VNode)node;
                vnode.DetouchFromCanvas( _graphCanvas );

                _drawingGraph.RemoveNode( node.Node );
                _drawingGraph.GeometryGraph.Nodes.Remove( node.Node.GeometryNode );
                layoutEditor.DetachNode( node );
                layoutEditor.CleanObstacles();
            }
        }

        void RemoveEdges( IEnumerable<DrawingEdge> drawingEdges )
        {
            foreach ( var de in drawingEdges.ToArray() )
            {
                var vedge = (VEdge)drawingObjectsToIViewerObjects[ de ];
                RemoveEdge( vedge, false );
            }
        }

        public IViewerEdge RouteEdge( DrawingEdge drawingEdge )
        {
            var geomEdge = GeometryGraphCreator.CreateGeometryEdgeFromDrawingEdge( drawingEdge );
            var geomGraph = _drawingGraph.GeometryGraph;

            LayoutHelpers.RouteAndLabelEdges( geomGraph, _drawingGraph.LayoutAlgorithmSettings, new[] { geomEdge } );

            return CreateEdge( drawingEdge, _drawingGraph.LayoutAlgorithmSettings as LgLayoutSettings );
        }

        public IViewerGraph ViewerGraph { get; set; }

        public double ArrowheadLength
        {
            get { return 0.2 * DpiX / CurrentScale; }
        }

        public void SetSourcePortForEdgeRouting( Point portLocation )
        {
            _sourcePortLocationForEdgeRouting = portLocation;

            if ( _sourcePortCircle == null )
            {
                _sourcePortCircle = CreatePortPath();
                _graphCanvas.Children.Add( _sourcePortCircle );
            }

            _sourcePortCircle.Width = _sourcePortCircle.Height = UnderlyingPolylineCircleRadius;
            _sourcePortCircle.StrokeThickness = _sourcePortCircle.Width / 10;

            Common.PositionFrameworkElement( _sourcePortCircle, portLocation, 1 );
        }

        Ellipse CreatePortPath()
        {
            return new Ellipse
            {
                Stroke = new SolidColorBrush( Colors.Brown ),
                Fill = new SolidColorBrush( Colors.Brown )
            };
        }

        public void SetTargetPortForEdgeRouting( Point portLocation )
        {
            if ( TargetPortCircle == null )
            {
                TargetPortCircle = CreatePortPath();
                _graphCanvas.Children.Add( TargetPortCircle );
            }

            TargetPortCircle.Width = TargetPortCircle.Height = UnderlyingPolylineCircleRadius;
            TargetPortCircle.StrokeThickness = TargetPortCircle.Width / 10;

            Common.PositionFrameworkElement( TargetPortCircle, portLocation, 1 );
        }

        public void RemoveSourcePortEdgeRouting()
        {
            _graphCanvas.Children.Remove( _sourcePortCircle );
            _sourcePortCircle = null;
        }

        public void RemoveTargetPortEdgeRouting()
        {
            _graphCanvas.Children.Remove( TargetPortCircle );
            TargetPortCircle = null;
        }

        public void DrawRubberEdge( EdgeGeometry edgeGeometry )
        {
            if ( _rubberEdgePath == null )
            {
                _rubberEdgePath = new Path
                {
                    Stroke = new SolidColorBrush( Colors.Black ),
                    StrokeThickness = GetBorderPathThickness() * 3
                };
                _graphCanvas.Children.Add( _rubberEdgePath );
                _targetArrowheadPathForRubberEdge = new Path
                {
                    Stroke = new SolidColorBrush( Colors.Black ),
                    StrokeThickness = GetBorderPathThickness() * 3
                };
                _graphCanvas.Children.Add( _targetArrowheadPathForRubberEdge );
            }

            _rubberEdgePath.Data = VEdge.GetICurveWpfGeometry( edgeGeometry.Curve );
            _targetArrowheadPathForRubberEdge.Data = VEdge.DefiningTargetArrowHead( edgeGeometry, edgeGeometry.LineWidth );
        }

        bool UnderLayout
        {
            get { return _backgroundWorker != null; }
        }

        public void StopDrawingRubberEdge()
        {
            _graphCanvas.Children.Remove( _rubberEdgePath );
            _graphCanvas.Children.Remove( _targetArrowheadPathForRubberEdge );
            _rubberEdgePath = null;
            _targetArrowheadPathForRubberEdge = null;
        }

        public PlaneTransformation Transform
        {
            get
            {
                var mt = _graphCanvas.RenderTransform as MatrixTransform;
                if ( mt == null )
                    return PlaneTransformation.UnitTransformation;
                var m = mt.Matrix;
                return new PlaneTransformation( m.M11, m.M12, m.OffsetX, m.M21, m.M22, m.OffsetY );
            }
            set
            {
                SetRenderTransformWithoutRaisingEvents( value );

                if ( ViewChangeEvent != null )
                    ViewChangeEvent( null, null );
            }
        }

        void SetRenderTransformWithoutRaisingEvents( PlaneTransformation value )
        {
            _graphCanvas.RenderTransform = new MatrixTransform()
            {
                Matrix = new Matrix( value[ 0, 0 ], value[ 0, 1 ], value[ 1, 0 ], value[ 1, 1 ],
                                     value[ 0, 2 ], value[ 1, 2 ] )
            };
        }

        public bool NeedToCalculateLayout
        {
            get { return needToCalculateLayout; }
            set { needToCalculateLayout = value; }
        }

        /// <summary>
        /// the cancel token used to cancel a long running layout
        /// </summary>
        public CancelToken CancelToken
        {
            get { return _cancelToken; }
            set { _cancelToken = value; }
        }

        /// <summary>
        /// no layout is done, but the overlap is removed for graphs with geometry
        /// </summary>
        public bool NeedToRemoveOverlapOnly { get; set; }

        public void DrawRubberLine( Point rubberEnd )
        {
            if ( _rubberLinePath == null )
            {
                _rubberLinePath = new Path
                {
                    Stroke = new SolidColorBrush( Colors.Black ),
                    StrokeThickness = GetBorderPathThickness() * 3
                };
                _graphCanvas.Children.Add( _rubberLinePath );
                //                targetArrowheadPathForRubberLine = new Path {
                //                    Stroke = Brushes.Black,
                //                    StrokeThickness = GetBorderPathThickness()*3
                //                };
                //                graphCanvas.Children.Add(targetArrowheadPathForRubberLine);
            }
            _rubberLinePath.Data =
                VEdge.GetICurveWpfGeometry( new LineSegment( _sourcePortLocationForEdgeRouting, rubberEnd ) );
        }

        public void StartDrawingRubberLine( Point startingPoint )
        {
        }

        #endregion

        public IViewerNode CreateIViewerNode( Drawing.Node drawingNode, Point center, object visualElement )
        {
            if ( _drawingGraph == null )
                return null;
            var frameworkElement = visualElement as FrameworkElement ?? CreateTextBlockForDrawingObj( drawingNode );
            var width = frameworkElement.Width + 2 * drawingNode.Attr.LabelMargin;
            var height = frameworkElement.Height + 2 * drawingNode.Attr.LabelMargin;
            var bc = NodeBoundaryCurves.GetNodeBoundaryCurve( drawingNode, width, height );
            drawingNode.GeometryNode = new Node( bc, drawingNode ) { Center = center };
            var vNode = CreateVNode( drawingNode );
            _drawingGraph.AddNode( drawingNode );
            _drawingGraph.GeometryGraph.Nodes.Add( drawingNode.GeometryNode );
            layoutEditor.AttachLayoutChangeEvent( vNode );
            MakeRoomForNewNode( drawingNode );

            return vNode;
        }

        void MakeRoomForNewNode( Drawing.Node drawingNode )
        {
            IncrementalDragger incrementalDragger = new IncrementalDragger( 
                new[] { drawingNode.GeometryNode },
                Graph.GeometryGraph,
                Graph.LayoutAlgorithmSettings );

            incrementalDragger.Drag( new Point() );

            foreach ( var n in incrementalDragger.ChangedGraph.Nodes )
            {
                var dn = (Drawing.Node)n.UserData;
                var vn = drawingObjectsToIViewerObjects[ dn ] as VNode;

                if ( vn != null )
                    vn.Invalidate();
            }

            foreach ( var n in incrementalDragger.ChangedGraph.Edges )
            {
                var dn = (Drawing.Edge)n.UserData;
                var ve = drawingObjectsToIViewerObjects[ dn ] as VEdge;

                if ( ve != null )
                    ve.Invalidate();
            }
        }
    }
}