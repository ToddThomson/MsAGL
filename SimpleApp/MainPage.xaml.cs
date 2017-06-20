using Microsoft.Msagl.Drawing;
using Microsoft.Msagl.GraphControl;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace SimpleApp
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        //RelativePanel panel = new RelativePanel();

        public MainPage()
        {
            this.InitializeComponent();

            //panel.HorizontalAlignment = HorizontalAlignment.Right;
            //panel.VerticalAlignment = VerticalAlignment.Top;
            RelativePanel.SetAlignRightWithPanel( this, true );
            RelativePanel.SetAlignTopWithPanel( this, true );
            RelativePanel.SetAlignLeftWithPanel( this, true );
            RelativePanel.SetAlignBottomWithPanel( this, true );

            //this.Content = panel;

            Loaded += MainPage_Loaded;
        }

        private void MainPage_Loaded( object sender, RoutedEventArgs e )
        {
            GraphViewer graphViewer = new GraphViewer();
            graphViewer.BindToPanel( outputGrid );
            Graph graph = new Graph();
 
            graph.AddEdge( "A", "B" );
            graph.Attr.LayerDirection = LayerDirection.LR;

            graphViewer.Graph = graph;
        }

        private void Page_SizeChanged( object sender, SizeChangedEventArgs e )
        {

        }

        private void ToggleButton_Click( object sender, RoutedEventArgs e )
        {
            Splitter.IsPaneOpen = !Splitter.IsPaneOpen;
        }
    }
}
