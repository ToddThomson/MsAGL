#region Copyright Notice

// Copyright (c) by Achilles Software, All rights reserved.
//
// Licensed under the MIT License. See License.txt in the project root for license information.
//
// Send questions regarding this copyright notice to: mailto:todd.thomson@achilles-software.com

#endregion

#region Namespaces

using Windows.Foundation;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Shapes;

#endregion

namespace Msagl.GraphControl
{
    internal class DraggingCanvas : Canvas
    {
        Color DefaultBorderColor = Colors.Blue;
        Color DefaultBackColor = Color.FromArgb( 10, 0, 0, 255 );

        #region Fields

        Rectangle draggingFrame;

        #endregion

        #region Constructor

        public DraggingCanvas()
        {
            draggingFrame = new Rectangle();
            this.Children.Add( draggingFrame );

            draggingFrame.Width = 0;
            draggingFrame.Height = 0;

            draggingFrame.Visibility = Visibility.Collapsed;
            
            // Initial placement of the drag selection box.         
            Canvas.SetLeft( draggingFrame, 0 );
            Canvas.SetTop( draggingFrame, 0 );
        }

        #endregion

        public void HideDraggingFrame()
        {
            draggingFrame.Visibility = Visibility.Collapsed;
        }

        /// <summary>
        /// Draws a reversible frame on the screen within the specified bounds, with the
        /// specified background color, and in the specified state.
        /// </summary>
        /// <param name="rectangle">The Rectangle that represents the dimensions of the rectangle to draw.</param>
        /// <param name="backColor">The Color of the background behind the frame.</param>
        /// <param name="style"></param>
        public void DrawDraggingFrame( Rect rectangle, Color backColor, DoubleCollection stroke )
        {
            draggingFrame.Stroke = new SolidColorBrush( Colors.Blue );
            draggingFrame.Fill = new SolidColorBrush( backColor );

            draggingFrame.StrokeDashArray = stroke;

            Canvas.SetLeft( draggingFrame, rectangle.X );
            draggingFrame.Width = rectangle.Width;

            Canvas.SetTop( draggingFrame, rectangle.Y );
            draggingFrame.Height = rectangle.Height;

            draggingFrame.Visibility = Visibility.Visible;
        }
    }
}
