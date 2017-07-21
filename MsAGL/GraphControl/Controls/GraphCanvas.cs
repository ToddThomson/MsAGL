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
using Windows.UI.Xaml.Controls;

#endregion

namespace Msagl.Uwp.UI.Controls
{
    public class GraphCanvas: Canvas
    {
        protected override Size MeasureOverride( Size availableSize )
        {
            base.MeasureOverride( availableSize );

            double canvasWidth = MinWidth;
            double canvasHeight = MinHeight;

            foreach ( var child in this.Children )
            {
                if ( child is FrameworkElement fe )
                {
                    child.Measure( availableSize );

                    canvasWidth = Math.Max( canvasWidth, fe.ActualWidth + (double)child.GetValue( Canvas.LeftProperty ) );
                    canvasHeight = Math.Max( canvasHeight, fe.ActualHeight + (double)child.GetValue( Canvas.TopProperty ) );
                }
            }

            return new Size( canvasWidth, canvasHeight );
        }
    }
}
