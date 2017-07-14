#region Copyright Notices

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

using Microsoft.Msagl.Drawing;
using Windows.Foundation;
using Windows.System;
using Windows.UI.Input;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Input;

#endregion

namespace Msagl.Uwp.UI.GraphControl
{
    internal class GraphViewerPointerEventArgs : MsaglPointerEventArgs
    {
        PointerRoutedEventArgs pointerEventArgs;
        PointerPoint pointerInfo;

        internal GraphViewerPointerEventArgs( PointerRoutedEventArgs e, GraphViewer graphScrollerP )
        {
            pointerEventArgs = e;
            pointerInfo = e.GetCurrentPoint( (UIElement)graphScrollerP.GraphCanvas.Parent );
        }

        public override bool IsLeftButtonPressed
        {
            get
            {
                return pointerInfo.Properties.IsLeftButtonPressed;
            }
        }

        public override bool IsMiddleButtonPressed
        {
            get
            {
                return pointerInfo.Properties.IsMiddleButtonPressed;
            }
        }

        public override bool IsRightButtonPressed
        {
            get
            {
                return pointerInfo.Properties.IsRightButtonPressed;
            }
        }

        public override VirtualKeyModifiers KeyModifiers
        {
            get { return pointerEventArgs.KeyModifiers; }
        }

        public override bool Handled
        {
            get { return pointerEventArgs.Handled; }
            set { pointerEventArgs.Handled = value; }
        }

        public override int X
        {
            get { return (int)pointerInfo.Position.X; }
        }

        public override int Y
        {
            get { return (int)pointerInfo.Position.Y; }
        }

        /// <summary>
        /// number of clicks
        /// </summary>
        public override int Clicks
        {
            get
            {
                // FIXME: TJT - eliminate this
                return 0;// pointerEventArgs != null ? pointerEventArgs.Pointer.Clicks : 0;
            }
        }
    }
}