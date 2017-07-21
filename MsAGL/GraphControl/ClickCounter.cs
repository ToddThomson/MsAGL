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

using System;
using Msagl.Uwp.UI.Drawing;
using Windows.UI.Xaml;
using Windows.Foundation;
using System.Numerics;

#endregion

namespace Msagl.Uwp.UI
{
    internal class ClickCounter
    {
        readonly Func<Point> mousePosition;
        internal object ClickedObject;
        internal bool IsRunning { get; private set; }

        internal ClickCounter( Func<Point> mousePosition )
        {
            this.mousePosition = mousePosition;

            clickTimer.Tick += TimeTick;
            clickTimer.Interval = TimeSpan.FromMilliseconds( 500 );

            IsRunning = false;
        }

        internal int DownCount { get; private set; }
        internal int UpCount { get; private set; }
        readonly DispatcherTimer clickTimer = new DispatcherTimer();
        internal Point LastDownClickPosition;

        internal void AddPointerPressed( object objectUnderPointer )
        {
            if ( !IsRunning )
            {
                DownCount = 0;
                UpCount = 0;
                clickTimer.Start();
                IsRunning = true;
            }

            LastDownClickPosition = this.mousePosition();
            ClickedObject = objectUnderPointer;

            DownCount++;
        }

        internal void AddPointerReleased()
        {
            const double minDistanceForClickDownAndUp = 0.1;

            if ( IsRunning )
            {
                //var distancePointerMoved = Point.
                // FIXME:
                //if ( (mousePosition() - LastDownClickPosition).Length > minDistanceForClickDownAndUp )
                //{
                //    //it is not a click
                //    UpCount = 0;
                //    DownCount = 0;
                //    clickTimer.Stop();
                //    IsRunning = false;
                //}
                //else
                //    UpCount++;
            }
        }

        void TimeTick( object sender, Object obj )
        {
            clickTimer.Stop();
            IsRunning = false;
            OnElapsed();
        }

        public event EventHandler<EventArgs> Elapsed;

        protected virtual void OnElapsed()
        {
            Elapsed?.Invoke( this, EventArgs.Empty );
        }
    }
}