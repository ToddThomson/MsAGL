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

#endregion

namespace Microsoft.Msagl.Drawing
{
    /// <summary>
    /// Microsoft.Msagl.Drawing attribute.
    /// </summary>
    [DataContract]
    public class GraphAttr : AttributeBase
    {
        #region Constructor(s)

        /// <summary>
        /// constructor
        /// </summary>
        public GraphAttr()
        {
        }

        #endregion

        #region Properties

        double minimalWidth = 32;
        /// <summary>
        /// The resulting layout should be not more narrow than this value. 
        /// </summary>
        public double MinimalWidth
        {
            get { return minimalWidth; }
            set { minimalWidth = Math.Max( value, 32 ); }
        }

        double minimalHeight = 32;
        /// <summary>
        /// The resulting layout should at least as high as this this value
        /// </summary>
        public double MinimalHeight
        {
            get { return minimalHeight; }
            set { minimalHeight = Math.Max( value, 32 ); }
        }

        double minNodeHeight = 9;
        /// <summary>
        /// the lower bound for the node height
        /// </summary>
        public double MinNodeHeight
        {
            get { return minNodeHeight; }
            set { minNodeHeight = Math.Max( 9.0 / 10, value ); }
        }

        double minNodeWidth = 13.5;
        /// <summary>
        /// the lower bound for the node width
        /// </summary>
        public double MinNodeWidth
        {
            get { return minNodeWidth; }
            set { minNodeWidth = Math.Max( 13.5 / 10, value ); }
        }

        bool simpleStretch = true;
        /// <summary>
        /// Works together with AspectRatio. If is set to false then the apsect ratio algtorithm kicks in.
        /// </summary>
        public bool SimpleStretch
        {
            get { return simpleStretch; }
            set { simpleStretch = value; }
        }

        /// <summary>
        /// the required aspect ratio of the graph bounding box
        /// </summary>
        public double AspectRatio { get; set; }

        int border;
        /// <summary>
        /// thickness of the graph border line
        /// </summary>
        public int Border
        {
            get { return border; }
            set { border = value; }
        }

        ///<summary>
        ///Background color for drawing ,plus initial fill color - white by default.
        ///</summary>
        internal Color bgcolor = new Color( 255, 255, 255 );//white

        /// <summary>
        /// Background color for drawing and initial fill color.
        /// </summary>
        //[Description("Background color for drawing ,plus initial fill color.")]
        public Color BackgroundColor
        {
            get { return bgcolor; }
            set { bgcolor = value; }
        }

        private double margin = 10;
        /// <summary>
        /// margins width
        /// </summary>
        public double Margin
        {
            get { return margin; }
            set { margin = value; }
        }

        bool optimizeLabelPositions = true;
        /// <summary>
        /// if set to true then the label positions are optimized
        /// </summary>
        public bool OptimizeLabelPositions
        {
            get { return optimizeLabelPositions; }
            set { optimizeLabelPositions = value; }
        }

        double minNodeSeparation = 72 * 0.50 / 8;
        /// <summary>
        /// the minimal node separation
        /// </summary>
        public double MinNodeSeparation { get { return minNodeSeparation; } }

        ///<summary>
        ///Separation between nodes
        ///</summary>
        private double nodesep = 72 * 0.50 / 4;
        /// <summary>
        /// Gets or Sets the separation between nodes.
        /// </summary>
        public double NodeSeparation
        {
            get { return nodesep; }
            set { nodesep = Math.Max( value, MinNodeSeparation ); }
        }

        private LayerDirection layerdir = LayerDirection.TB;
        /// <summary>
        /// Directs node layers
        /// </summary>
        public LayerDirection LayerDirection
        {
            get { return layerdir; }
            set { layerdir = value; }
        }
        ///<summary>
        ///Separation between layers in
        ///</summary>
        private double layersep = 72 * 0.5; //is equal to minLayerSep
                                            
        /// <summary>
        /// the distance between two neighbor layers
        /// </summary>
        public double LayerSeparation
        {
            get { return layersep; }
            set { layersep = Math.Max( value, minLayerSep ); }
        }

        double minLayerSep = 72 * 0.5 * 0.01;
        /// <summary>
        /// the minimal layer separation
        /// </summary>
        public double MinLayerSeparation
        {
            get { return minLayerSep; }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// dumps the attribute into a string
        /// </summary>
        /// <param name="label"></param>
        /// <returns></returns>
        public string ToString( string label )
        {
            string ret = "graph [";
            if ( !String.IsNullOrEmpty( label ) )
            {
                label = label.Replace( "\r\n", "\\n" );
                ret += "label=" + Utils.Quote( label );
            }

            if ( this.LayerDirection != LayerDirection.None )
            {
                ret = Utils.ConcatWithLineEnd( ret, "layerdir=" + this.LayerDirection.ToString() );
            }

            ret = Utils.ConcatWithLineEnd( ret, "layersep=" + this.LayerSeparation );
            ret = Utils.ConcatWithLineEnd( ret, "nodesep=" + this.NodeSeparation );
            ret = Utils.ConcatWithLineEnd(
                ret,
                Utils.ColorToString( "color=", Color.ToString() ),
                Utils.ColorToString( "bgcolor=", this.bgcolor.ToString() ),
                StylesToString( "\r\n" ), "]" );

            return ret;
        }

        #endregion
    }
}
