using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.Xaml;

namespace Msagl.GraphControl
{
    public static class PointerHelpers
    {
        public static Point GetPointerPosition()
        {
            Window currentWindow = Window.Current;

            Point point;

            try
            {
                point = currentWindow.CoreWindow.PointerPosition;
            }
            catch ( UnauthorizedAccessException )
            {
                return new Point( double.NegativeInfinity, double.NegativeInfinity );
            }

            Rect bounds = currentWindow.Bounds;

            return new Point( point.X - bounds.X, point.Y - bounds.Y );
        }
    }
}
