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
using System.Collections.Generic;
using Microsoft.Msagl.Core.DataStructures;
using Microsoft.Msagl.Core.Geometry;
using Microsoft.Msagl.Core.Layout;
using Microsoft.Msagl.Prototype.LayoutEditing;
using System.Linq;

#endregion

namespace Msagl.Uwp.UI.Layout
{
    /// <summary>
    /// the interface for undo objects
    /// </summary>
    public class UndoRedoAction
    {
        Set<IViewerObject> affectedObjects = new Set<IViewerObject>();

        /// <summary>
        /// the set of the objects that the viewer has to invalidate
        /// </summary>
        internal IEnumerable<IViewerObject> AffectedObjects
        {
            get { return affectedObjects; }
        }

        internal bool ContainsAffectedObject( IViewerObject o )
        {
            return affectedObjects.Contains( o );
        }

        internal void AddAffectedObject( IViewerObject o )
        {
            lock ( affectedObjects )
                affectedObjects.Insert( o );
        }

        internal void RemoveAffectedObject( IViewerObject o )
        {
            lock ( affectedObjects )
                affectedObjects.Remove( o );
        }

        internal void ClearAffectedObjects()
        {
            lock ( affectedObjects )
                affectedObjects.Clear();
        }

        internal UndoRedoAction( GeometryGraph graphPar )
        {
            this.Graph = graphPar;
            this.graphBoundingBoxBefore = this.Graph.BoundingBox;
        }

        GeometryGraph graph;

        /// <summary>
        /// the graph being edited
        /// </summary>
        public GeometryGraph Graph
        {
            get { return graph; }
            set { graph = value; }
        }

        UndoRedoAction next;
        UndoRedoAction prev;

        /// <summary>
        /// Undoes the action
        /// </summary>
        public virtual void Undo()
        {
            if ( GraphBoundingBoxHasChanged )
                this.Graph.BoundingBox = GraphBoundingBoxBefore;
        }

        /// <summary>
        /// Redoes the action
        /// </summary>
        public virtual void Redo()
        {
            if ( GraphBoundingBoxHasChanged )
                Graph.BoundingBox = GraphBoundingBoxAfter;
        }

        /// <summary>
        /// The pointer to the next undo object
        /// </summary>
        public UndoRedoAction Next
        {
            get { return next; }
            set { next = value; }
        }

        /// <summary>
        /// The pointer to the previous undo object
        /// </summary>
        public UndoRedoAction Previous
        {
            get { return prev; }
            set { prev = value; }
        }
        /// <summary>
        /// 
        /// </summary>
        protected Dictionary<GeometryObject, RestoreData> restoreDataDictionary =
            new Dictionary<GeometryObject, RestoreData>();

        internal void AddRestoreData( GeometryObject msaglObject, RestoreData restoreData )
        {
            lock ( restoreData )
                restoreDataDictionary[ msaglObject ] = restoreData;

        }

        internal static GeometryGraph GetParentGraph( GeometryObject geomObj )
        {
            do
            {
                GeometryGraph graph = geomObj.GeometryParent as GeometryGraph;
                if ( graph != null )
                    return graph;
                geomObj = geomObj.GeometryParent;
            } while ( true );
        }

        internal RestoreData GetRestoreData( GeometryObject msaglObject )
        {
            return restoreDataDictionary[ msaglObject ];
        }

        /// <summary>
        /// enumerates over all edited objects
        /// </summary>
        public IEnumerable<GeometryObject> EditedObjects
        {
            get { return restoreDataDictionary.Keys; }
        }


        Rectangle graphBoundingBoxBefore = new Rectangle();

        /// <summary>
        /// the graph bounding box before the change
        /// </summary>
        public Rectangle GraphBoundingBoxBefore
        {
            get { return graphBoundingBoxBefore; }
            set { graphBoundingBoxBefore = value; }
        }

        Rectangle graphBoundingBoxAfter;

        /// <summary>
        /// the graph bounding box after the change
        /// </summary>
        public Rectangle GraphBoundingBoxAfter
        {
            get { return graphBoundingBoxAfter; }
            set { graphBoundingBoxAfter = value; }
        }

        /// <summary>
        /// returns true if the was a change in the bounding box of the graph
        /// </summary>
        public bool GraphBoundingBoxHasChanged
        {
            get { return graphBoundingBoxAfter != graphBoundingBoxBefore; }
        }
    }
}