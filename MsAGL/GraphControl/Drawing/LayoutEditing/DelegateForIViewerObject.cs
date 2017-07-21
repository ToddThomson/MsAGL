using System.Diagnostics.CodeAnalysis;

namespace Msagl.Uwp.UI.Drawing{
    /// <summary>
    /// a delegate type with IViewerObject as a parameter
    /// </summary>
    /// <param name="obj"></param>
    [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Draggable")]
    public delegate void DelegateForIViewerObject(IViewerObject obj);
}