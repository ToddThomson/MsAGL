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

using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;
using Point = Microsoft.Msagl.Core.Geometry.Point;
using UwpPoint = Windows.Foundation.Point;

#endregion

namespace Msagl.Uwp.UI
{
    internal class Common
    {
        internal static UwpPoint UwpPoint( Point p )
        {
            return new UwpPoint( p.X, p.Y );
        }

        internal static Point MsaglPoint( UwpPoint p )
        {
            return new Point( p.X, p.Y );
        }

        //public static Brush BrushFromMsaglColor( Msagl.Uwp.UI.Layout.Color color )
        //{
        //    Color brushColor = new Color { A = color.A, B = color.B, G = color.G, R = color.R };

        //    return new SolidColorBrush( brushColor );
        //}

        //public static Brush BrushFromMsaglColor( byte colorA, byte colorR, byte colorG, byte colorB )
        //{
        //    Color brushColor = new Color { A = colorA, R = colorR, G = colorG, B = colorB };

        //    return new SolidColorBrush( brushColor );
        //}

        internal static void PositionFrameworkElement( FrameworkElement frameworkElement, Point center, double scale )
        {
            PositionFrameworkElement( frameworkElement, center.X, center.Y, scale );
        }

        static void PositionFrameworkElement( FrameworkElement frameworkElement, double x, double y, double scale )
        {
            if ( frameworkElement == null )
                return;

            MatrixTransform transform = new MatrixTransform()
            {
                Matrix = new Matrix(
                scale, 0, 0, -scale, x - scale * frameworkElement.Width / 2,
                y + scale * frameworkElement.Height / 2 )
            };

            frameworkElement.RenderTransform = transform;
        }
    }
}