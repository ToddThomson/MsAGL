#region Copyright Notice

// Copyright (c) by Achilles Software, All rights reserved.
//
// Licensed under the MIT License. See License.txt in the project root for license information.
//
// Send questions regarding this copyright notice to: mailto:todd.thomson@achilles-software.com

#endregion

#region Namespaces

using Msagl.Uwp.UI;
using Msagl.Uwp.UI.Layout;
using Msagl.Uwp.UI.Controls;

using Windows.Graphics.Display;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

#endregion

namespace SimpleApp.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class GraphViewerPage : Page
    {
        private GraphViewer GraphViewer { get; set; }

        public GraphViewerPage()
        {
            this.InitializeComponent();
            this.Loaded += GraphViewerPage_Loaded;

            this.Loading += GraphViewerPage_Loading;
        }

        private void GraphViewerPage_Loading( FrameworkElement sender, object args )
        {
        }

        private void GraphViewerPage_Loaded(object sender, RoutedEventArgs e)
        {
            double? diagonal = DisplayInformation.GetForCurrentView().DiagonalSizeInInches;

            //move commandbar to page bottom on small screens
            if (diagonal < 7)
            {
                topbar.Visibility = Visibility.Collapsed;
                pageTitleContainer.Visibility = Visibility.Visible;
                bottombar.Visibility = Visibility.Visible;
            }
            else
            {
                topbar.Visibility = Visibility.Visible;
                pageTitleContainer.Visibility = Visibility.Collapsed;
                bottombar.Visibility = Visibility.Collapsed;
            }

            this.GraphViewer = graphViewer;

            // Create the sample graph...
            Graph graph = new Graph();

            graph.AddNode( new Node( "C" ) );
            graph.AddEdge( "A", "B" );

            graph.Attr.LayerDirection = LayerDirection.LR;

            GraphViewer.Graph = graph;
        }

        private void OutputGrid_SizeChanged( object sender, SizeChangedEventArgs e )
        {
            GraphViewer.Invalidate();
        }

        private void Pan_Click( object sender, RoutedEventArgs e )
        {
            if ( GraphViewer != null )
                GraphViewer.EditingMode = EditingMode.Pan;
        }

        private void Zoom_Click( object sender, RoutedEventArgs e )
        {
            if ( GraphViewer != null )
                GraphViewer.EditingMode = EditingMode.Zoom;
        }

        private void Select_Click( object sender, RoutedEventArgs e )
        {
            if ( GraphViewer != null )
                GraphViewer.EditingMode = EditingMode.Select;
        }
    }
}
