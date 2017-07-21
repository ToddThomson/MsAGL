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
using System.Runtime.Serialization;
using Windows.UI;

#endregion

namespace Msagl.Uwp.UI.Layout
{
    /// <summary>
    /// Attributes of a Node.
    /// </summary>
    [DataContract]
    public class NodeAttr : AttributeBase
    {
        #region Properties

        double padding = 2;
        /// <summary>
        /// Splines should avoid being closer to the node than Padding
        /// </summary>
        public double Padding
        {
            get { return padding; }
            set
            {
                padding = Math.Max( 0, value );
                RaiseVisualsChangedEvent( this, null );
            }
        }

        double xRad = 3;
        /// <summary>
        ///x radius of the rectangle box 
        /// </summary>
        public double XRadius
        {
            get { return xRad; }
            set
            {
                xRad = value;
                RaiseVisualsChangedEvent( this, null );
            }
        }

        double yRad = 3;
        /// <summary>
        /// y radius of the rectangle box 
        /// </summary>
        public double YRadius
        {
            get { return yRad; }
            set { yRad = value; }
        }

        static Color defaultFillColor = Colors.LightGray;
        /// <summary>
        /// the default fill color
        /// </summary>
        static public Color DefaultFillColor
        {
            get { return defaultFillColor; }
            set { defaultFillColor = value; }
        }

        internal Color fillcolor = Colors.Transparent;
        ///<summary>
        /// Gets or Sets the Node fill color. The default value is Transparent.
        ///</summary>
        public Color FillColor
        {
            get
            {
                return fillcolor;
            }
            set
            {
                fillcolor = value;
                RaiseVisualsChangedEvent( this, null );
            }
        }

        internal Shape shape = Shape.Box;
        /// <summary>
        /// Gets or Sets the Node shape. Defaults to Box. 
        /// </summary>
        public Shape Shape
        {
            get { return shape; }
            set
            {
                shape = value;
                RaiseVisualsChangedEvent( this, null );
            }
        }

        int labelMargin = 1;
        /// <summary>
        /// Gets or sets the node label margin. The default is 1.
        /// </summary>
        public int LabelMargin
        {
            get { return labelMargin; }
            set
            {
                labelMargin = value;
                RaiseVisualsChangedEvent( this, null );
            }
        }

        private double labelWidthToHeightRatio = 1.0;
        /// <summary>
        /// the label width to height ratio.
        /// </summary>
        public double LabelWidthToHeightRatio
        {
            get { return labelWidthToHeightRatio; }
            set { labelWidthToHeightRatio = value; }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Clones the node attribute
        /// </summary>
        /// <returns></returns>
        public NodeAttr Clone()
        {
            return this.MemberwiseClone() as NodeAttr;
        }
        
        /// <summary>
        /// Converts the Node attributes to a string.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return Utils.ConcatWithComma(
                StylesToString( "," ),
                Utils.ColorToString( "color=", base.Color.ToString() ),
                Utils.ShapeToString( "shape=", this.shape ),
                Utils.ColorToString( "fillcolor=", fillcolor.ToString() ),
                IdToString() );
        }

        //void AddFilledStyle(){
        //    if(Array.IndexOf(styles,Style.filled)==-1){
        //        Style []st=new Style[styles.Length+1];
        //        st[0]=Style.filled;
        //        styles.CopyTo(st,1);
        //        styles=st;
        //    }
        //}

        //void RemoveFilledStyle()
        //{

        //  int index;
        //  if ((index = Array.IndexOf(styles, Style.filled)) != -1)
        //  {
        //    Style[] st = new Style[styles.Length - 1];

        //    int count = 0;
        //    for (int j = 0; j < styles.Length; j++)
        //    {
        //      if (j != index)
        //        st[count++] = styles[j];
        //    }
        //    styles = st;
        //  }
        //}

        #endregion
    }
}
