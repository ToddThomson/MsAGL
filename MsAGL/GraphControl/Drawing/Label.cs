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

using Microsoft.Msagl.Core.DataStructures;
using Microsoft.Msagl.Core.Geometry;
using Microsoft.Msagl.Core.Layout;

using System;
using System.Runtime.Serialization;

using P2 = Microsoft.Msagl.Core.Geometry.Point;

#endregion

namespace Msagl.Uwp.UI.Drawing
{
    /// <summary>
    /// Keep the information related to an object label
    /// </summary>
    [DataContract]
    public class Label : DrawingObject
    {
        Microsoft.Msagl.Core.Layout.Label geometryLabel = new Microsoft.Msagl.Core.Layout.Label();

        #region Constructor(s)

        /// <summary>
        /// an empty constructor
        /// </summary>
        public Label() : this( "" )
        {
        }

        /// <summary>
        /// a constructor with text
        /// </summary>
        /// <param name="text"></param>
        public Label( string text )
        {
            this.DefaultStyleKey = typeof( Node );

            this.text = text;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the drawing object control owner.
        /// </summary>
        public DrawingObject Owner { get; set; }

        public Point Center
        {
            get
            {
                if ( Owner == null )
                    return new Point();

                var edge = Owner as Edge;

                if ( edge != null )
                    return edge.GeometryEdge.Label.Center;

                return ((Node)Owner).GeometryNode.Center;
            }
        }

        double width;
        double height;
        
        /// <summary>
        /// the width of the label
        /// </summary>
        public double Width
        {
            get { return GeometryLabel == null ? width : GeometryLabel.Width; }
            set
            {
                if ( GeometryLabel == null )
                    width = value;
                else
                    GeometryLabel.Width = value;
            }
        }

        /// <summary>
        /// the height of the label
        /// </summary>
        public double Height
        {
            get
            {
                return GeometryLabel == null ? height : GeometryLabel.Height;
            }
            set
            {
                if ( GeometryLabel == null )
                    height = value;
                else
                    GeometryLabel.Height = value;
            }
        }

        /// <summary>
        /// left coordinate 
        /// </summary>
        public double Left { get { return Center.X - Width / 2; } }
        /// <summary>
        /// top coordinate
        /// </summary>
        public double Top { get { return Center.Y + Height / 2; } }

        /// <summary>
        /// left coordinate 
        /// </summary>
        public double Right { get { return Center.X + Width / 2; } }
        /// <summary>
        /// top coordinate
        /// </summary>
        public double Bottom { get { return Center.Y - Height / 2; } }

        /// <summary>
        /// gets the left top corner
        /// </summary>
        public P2 LeftTop { get { return new P2( Left, Top ); } }

        /// <summary>
        /// gets the right bottom corner
        /// </summary>
        public P2 RightBottom { get { return new P2( Right, Bottom ); } }

        /// <summary>
        /// Returns the bounding box of the label
        /// </summary>
        override public Rectangle BoundingBox
        {
            get { return new Rectangle( LeftTop, RightBottom ); }
        }

        /// <summary>
        /// Gets or sets the label size.
        /// </summary>
        virtual public Size Size
        {
            get
            {
                return new Size( Width, Height );
            }

            set
            {
                Width = value.Width;
                Height = value.Height;
            }
        }

        /// <summary>
        /// Gets or sets the Label font color.
        /// </summary>
        public Color FontColor { get; set; } = Color.Black;

        /// <summary>
        /// Gets or sets the Label font style.
        /// </summary>
        public FontStyle FontStyle { get; set; } = FontStyle.Regular;

        ///<summary>
        ///Type face font.
        ///</summary>
        string fontName = "";

        ///<summary>
        ///Type face font
        ///</summary>
        //[Description("type face font"),
        //DefaultValue("")]
        public string FontName
        {
            get
            {
                if ( String.IsNullOrEmpty( fontName ) )
                    return DefaultFontName;
                else
                    return fontName;
            }
            set
            {
                fontName = value;
            }
        }

        string text;
        /// <summary>
        /// A label of the entity. The label is rendered opposite to the ID. 
        /// </summary>
        public string Text
        {
            get { return text; }
            set
            {
                if ( value != null )
                    text = value.Replace( "\\n", "\n" );
                else
                    text = "";
            }
        }

        internal double fontsize = DefaultFontSize;

        ///<summary>
        ///The point size of the id.
        ///</summary>
        public double FontSize
        {
            get { return fontsize; }
            set { fontsize = value; }
        }

        /// <summary>
        /// Gets or sets the name of the default font.
        /// </summary>
        public static string DefaultFontName { get; set; } = "Times-Roman";

        /// <summary>
        /// the default font size
        /// </summary>
        static public int DefaultFontSize { get; set; } = 12;

        /// <summary>
        /// gets or set geometry label
        /// </summary>
        public Microsoft.Msagl.Core.Layout.Label GeometryLabel
        {
            get { return geometryLabel; }
            set { geometryLabel = value; }
        }

        /// <summary>
        /// gets the geometry of the label
        /// </summary>
        public override GeometryObject GeometryObject
        {
            get { return GeometryLabel; }
            set { GeometryLabel = (Microsoft.Msagl.Core.Layout.Label)value; }
        }

        #endregion
    }
}
