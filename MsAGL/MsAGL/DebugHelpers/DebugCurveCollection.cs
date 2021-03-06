#if TEST_MSAGL && ! SILVERLIGHT
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;

namespace Microsoft.Msagl.DebugHelpers{
    /// <summary>
    /// 
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1711:IdentifiersShouldNotHaveIncorrectSuffix"), DataContract]
    public class DebugCurveCollection{
        /// <summary>
        /// 
        /// </summary>
        /// <param name="debugCurves"></param>
        DebugCurveCollection(IEnumerable<DebugCurve> debugCurves){
            DebugCurvesArray = debugCurves.ToArray();
        }
        /// <summary>
        /// 
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields")]
        public DebugCurve[] DebugCurvesArray;

        ///<summary>
        ///</summary>
        ///<param name="debugCurves"></param>
        ///<param name="fileName"></param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        public static void WriteToFile(IEnumerable<DebugCurve> debugCurves, string fileName) {
            Stream stream = File.Open(fileName, FileMode.Create);
            var bformatter = new DataContractSerializer( typeof( DebugCurve ) );
            try {
                bformatter.WriteObject( stream, new DebugCurveCollection(debugCurves));
            }
            catch (SerializationException e) {
                Debug.WriteLine(e.ToString());
            }
            stream.Dispose();
        }

        ///<summary>
        ///</summary>
        ///<param name="fileName"></param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        public static IEnumerable<DebugCurve> ReadFromFile(string fileName) {
            Stream stream = File.Open(fileName, FileMode.Open);
            var bformatter = new DataContractSerializer( typeof( DebugCurve ));
            DebugCurveCollection dc = null;
            try {
                dc = bformatter.ReadObject(stream) as DebugCurveCollection;
            }
            catch (SerializationException e) {
                Debug.WriteLine(e.ToString());
            }
            stream.Dispose();
            return new List<DebugCurve>(dc.DebugCurvesArray);
        }
    }
}
#endif