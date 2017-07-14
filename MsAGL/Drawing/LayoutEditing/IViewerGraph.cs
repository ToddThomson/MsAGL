using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Microsoft.Msagl.Drawing
{
    /// <summary>
    /// This interface represents a graph that is drawn by the viewer and can be edited by it
    /// </summary>
    public interface IViewerGraph
    {
        /// <summary>
        /// Gets the drawing graph.
        /// </summary>
        Graph DrawingGraph { get; }

        /// <summary>
        /// yields the graph nodes.
        /// </summary>
        /// <returns>Node collection</returns>
        IEnumerable<IViewerNode> Nodes();
        
        /// <summary>
        /// yields the edges
        /// </summary>
        /// <returns></returns>
        IEnumerable<IViewerEdge> Edges();
    }
}
