using System.Diagnostics.CodeAnalysis;

namespace Msagl.Uwp.UI.Drawing{
    /// <summary>
    /// type of a polyline corner for insertion or deletion
    /// </summary>
    [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Polyline")]
    public enum PolylineCornerType {
        /// <summary>
        /// a corner to insert
        /// </summary>
        PreviousCornerForInsertion,
        /// <summary>
        /// a corner to delete
        /// </summary>
        CornerToDelete
    }
}