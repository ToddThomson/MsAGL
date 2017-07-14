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
using Microsoft.Msagl.Core.DataStructures;
using Microsoft.Msagl.Core.Geometry;
using Microsoft.Msagl.Core.Geometry.Curves;
using Microsoft.Msagl.Core.Layout;
using Microsoft.Msagl.Drawing;
using Rectangle = Microsoft.Msagl.Core.Geometry.Rectangle;
using System.Collections.Generic;
using Edge = Microsoft.Msagl.Core.Layout.Edge;
using ILabeledObject = Microsoft.Msagl.Core.Layout.ILabeledObject;
using Label = Microsoft.Msagl.Core.Layout.Label;
using System.Runtime.Serialization;

#endregion

namespace Microsoft.Msagl.Drawing
{
    /// <summary>
    /// If this delegate is not null and returns true then no node rendering is done by the viewer, the delegate is supposed to do the job.
    /// </summary>
    /// <param name="node"></param>
    /// <param name="graphics"></param>
    public delegate bool DelegateToOverrideNodeRendering( Node node, object graphics );
    /// <summary>
    /// By default a node boundary is calculated from Attr.Shape and the label size. 
    /// If the delegate is not null and returns not a null ICurve then this curve is taken as the node boundary
    /// </summary>
    /// <returns></returns>
    public delegate ICurve DelegateToSetNodeBoundary( Node node );

    /// <summary>
    /// The Node class for representing graph nodes.
    /// The Node class is derived from DrawingObject which is the base class for graph objects.
    /// </summary>
    [DataContract]
    public class Node : DrawingObject, ILabeledObject
    {
        #region Constructor(s)

        /// <summary>
        /// Creates a Node instance
        /// </summary>
        /// <param name="id">node name</param>
        public Node( string id )
        {
            Label = new Label();
            Label.GeometryLabel = null;

            Label.Owner = this;
            Attr = new NodeAttr();
            attr.Id = id;
            Label.Text = id; //one can change the label later
        }

        #endregion

        #region Properties

        /// <summary>
        /// the node ID
        /// </summary>
        public string Id
        {
            get { return this.attr.Id; }
            set { attr.Id = value; }
        }

        Label label;
        /// <summary>
        /// the label of the object
        /// </summary>
        public Label Label
        {
            get { return label; }
            set { label = value; }
        }

        Set<Edge> outEdges = new Set<Edge>();
        /// <summary>
        /// Enumerates over outgoing edges of the node
        /// </summary>
        public IEnumerable<Edge> OutEdges { get { return outEdges; } }

        Set<Edge> inEdges = new Set<Edge>();
        /// <summary>
        /// enumerates over the node incoming edges
        /// </summary>
        public IEnumerable<Edge> InEdges { get { return inEdges; } }

        Set<Edge> selfEdges = new Set<Edge>();
        Core.Layout.Node geometryNode;

        /// <summary>
        /// enumerates over the node self edges
        /// </summary>
        public IEnumerable<Edge> SelfEdges { get { return selfEdges; } }

        DelegateToOverrideNodeRendering drawNodeDelegate;
        /// <summary>
        /// A delegate to draw a node. If this delegate is not null and returns true then no node rendering is done
        /// </summary>
        public DelegateToOverrideNodeRendering DrawNodeDelegate
        {
            get { return drawNodeDelegate; }
            set { drawNodeDelegate = value; }
        }

        DelegateToSetNodeBoundary nodeBoundaryDelegate;
        /// <summary>
        /// By default a node boundary is calculated from Attr.Shape and the label size. 
        /// If the delegate is not null and returns not a null ICurve then this curve is taken as the node boundary
        /// </summary>
        /// <returns></returns>
        public DelegateToSetNodeBoundary NodeBoundaryDelegate
        {
            get { return nodeBoundaryDelegate; }
            set { nodeBoundaryDelegate = value; }
        }

        /// <summary>
        /// gets the node bounding box
        /// </summary>
        override public Rectangle BoundingBox
        {
            get { return GeometryNode.BoundaryCurve.BoundingBox; }
        }

        NodeAttr attr;
        /// <summary>
        /// gets or sets the node attribute.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage( "Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Attr" )]
        public NodeAttr Attr
        {
            get { return attr; }
            set { attr = value; }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// 
        /// </summary>
        /// <param name="o"></param>
        /// <returns></returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage( "Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "o" )]
        public int CompareTo( object o )
        {
            Node n = o as Node;

            if ( n == null )
                throw new InvalidOperationException();

            return String.Compare( this.Attr.Id, n.Attr.Id, StringComparison.Ordinal );
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            string label_text = Label == null ? Id : Label.Text;
            return Utils.Quote( label_text ) + "[" + Attr.ToString() + "," + GeomDataString() + "]";
        }

        #endregion

        string HeightString() { return "height=" + GeometryNode.Height; }
        string WidthString() { return "width=" + GeometryNode.Width; }
        string CenterString() { return "pos=" + string.Format( "\"{0},{1}\"", GeometryNode.Center.X, GeometryNode.Center.Y ); }
        string GeomDataString()
        {
            return Utils.ConcatWithComma( HeightString(), CenterString(), WidthString() );
        }
        
        /// <summary>
        /// add an incoming edge to the node
        /// </summary>
        /// <param name="e"></param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage( "Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "e" )]
        public void AddInEdge( Edge e )
        {
            inEdges.Insert( e );
        }

        /// <summary>
        /// adds and outcoming edge to the node
        /// </summary>
        /// <param name="e"></param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage( "Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "e" )]
        public void AddOutEdge( Edge e )
        {
            outEdges.Insert( e );
        }

        /// <summary>
        /// adds a self edge to the node
        /// </summary>
        /// <param name="e"></param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage( "Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "e" )]
        public void AddSelfEdge( Edge e )
        {
            selfEdges.Insert( e );
        }

        /// <summary>
        /// Removes an in-edge from the node's edge list (this won't remove the edge from the graph).
        /// </summary>
        /// <param name="edge">The edge to be removed</param>
        public void RemoveInEdge( Edge edge )
        {
            inEdges.Remove( edge );
        }

        /// <summary>
        /// Removes an out-edge from the node's edge list (this won't remove the edge from the graph).
        /// </summary>
        /// <param name="edge">The edge to be removed</param>
        public void RemoveOutEdge( Edge edge )
        {
            outEdges.Remove( edge );
        }

        /// <summary>
        /// Removes a self-edge from the node's edge list (this won't remove the edge from the graph).
        /// </summary>
        /// <param name="edge">The edge to be removed</param>
        public void RemoveSelfEdge( Edge edge )
        {
            selfEdges.Remove( edge );
        }

        /// <summary>
        /// gets the geometry node
        /// </summary>
        public override GeometryObject GeometryObject
        {
            get { return GeometryNode; }
            set { GeometryNode = (Core.Layout.Node)value; }
        }

        /// <summary>
        /// the underlying geometry node
        /// </summary>
        public Core.Layout.Node GeometryNode
        {
            get { return geometryNode; }
            set { geometryNode = value; }
        }

        /// <summary>
        /// a shortcut to the node label text
        /// </summary>
        public string LabelText
        {
            get { return Label != null ? Label.Text : ""; }
            set
            {
                if ( Label == null )
                    Label = new Label();
                Label.Text = value;
            }
        }

        /// <summary>
        /// enumerates over all edges
        /// </summary>
        public IEnumerable<Edge> Edges
        {
            get
            {
                foreach ( Edge e in InEdges )
                    yield return e;
                foreach ( Edge e in OutEdges )
                    yield return e;
                foreach ( Edge e in SelfEdges )
                    yield return e;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public double Height
        {
            get { return GeometryNode.Height; }
        }

        /// <summary>
        /// 
        /// </summary>
        public double Width
        {
            get { return GeometryNode.Width; }
        }

        /// <summary>
        /// 
        /// </summary>
        public Point Pos { get { return GeometryNode.Center; } }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals( object obj )
        {
            var otherNode = obj as Node;
            if ( otherNode == null )
                return false;
            return otherNode.Id == Id;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        /// <summary>
        /// 
        /// </summary>
        public override bool IsVisible
        {
            get
            {
                return base.IsVisible;
            }
            set
            {
                base.IsVisible = value;
                if ( !value )
                    foreach ( var edge in Edges )
                        edge.IsVisible = false;
            }
        }
    }
}
