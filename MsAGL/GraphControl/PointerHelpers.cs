#region Copyright Notice

// Copyright (c) by Achilles Software, All rights reserved.
//
// Licensed under the MIT License. See License.txt in the project root for license information.
//
// Send questions regarding this copyright notice to: mailto:todd.thomson@achilles-software.com

#endregion

#region Namespaces

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.Xaml;

#endregion

namespace Msagl.Uwp.UI
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
