
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;
using Point = Microsoft.Msagl.Core.Geometry.Point;
using UwpPoint = Windows.Foundation.Point;

namespace Microsoft.Msagl.GraphControl
{
    internal class Common
    {
        internal static UwpPoint WpfPoint( Point p )
        {
            return new UwpPoint( p.X, p.Y );
        }

        internal static Point MsaglPoint( UwpPoint p )
        {
            return new Point( p.X, p.Y );
        }

        public static Brush BrushFromMsaglColor( Microsoft.Msagl.Drawing.Color color )
        {
            Color avalonColor = new Color { A = color.A, B = color.B, G = color.G, R = color.R };
            return new SolidColorBrush( avalonColor );
        }

        public static Brush BrushFromMsaglColor( byte colorA, byte colorR, byte colorG, byte colorB )
        {
            Color avalonColor = new Color { A = colorA, R = colorR, G = colorG, B = colorB };
            return new SolidColorBrush( avalonColor );
        }

        internal static void PositionFrameworkElement( FrameworkElement frameworkElement, Point center, double scale )
        {
            PositionFrameworkElement( frameworkElement, center.X, center.Y, scale );
        }

        static void PositionFrameworkElement( FrameworkElement frameworkElement, double x, double y, double scale )
        {
            if ( frameworkElement == null )
                return;

            MatrixTransform transform = new MatrixTransform();
            transform.Matrix = new Matrix( scale, 0, 0, -scale, x - scale * frameworkElement.Width / 2,
                    y + scale * frameworkElement.Height / 2 );

            frameworkElement.RenderTransform = transform;
        }
    }
}