#if DEBUG && ! SILVERLIGHT
using System;
using System.Runtime.Serialization;

namespace Microsoft.Msagl.DebugHelpers {
    ///<summary>
    ///</summary>
    [DataContract]
    public class DebugShape {
        ///<summary>
        ///</summary>
        public int Pen { get; set; }
        ///<summary>
        ///</summary>
        public string Color { get; set; }
        /// <summary>
        /// Filling Color of the Shape.
        /// </summary>
        public string FillColor { get; set;}

    }
}
#endif