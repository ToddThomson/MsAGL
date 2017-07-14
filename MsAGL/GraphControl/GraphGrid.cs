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
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Documents;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Shapes;

#endregion

namespace Msagl.Uwp.UI.GraphControl
{
    public sealed class GraphGrid : Control
    {
        private List<Line> HorizontalLines;
        private List<Line> VerticalLines;

        public GraphGrid()
        {
            this.DefaultStyleKey = typeof( GraphGrid );
        }
    }
}
