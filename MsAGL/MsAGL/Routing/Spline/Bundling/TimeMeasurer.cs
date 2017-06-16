using System;
using Microsoft.Msagl.DebugHelpers;

namespace Microsoft.Msagl.Routing.Spline.Bundling {
    /// <summary>
    /// Outputs run time in debug mode 
    /// </summary>
    internal class TimeMeasurer {
#if DEBUG && TEST_MSAGL && REPORTING
        static Timer timer;
        static TimeMeasurer() {
            timer = new Timer();
            timer.Start();
        }
#endif

        internal delegate void Task();

        internal static void DebugOutput(string str) {
#if DEBUG && TEST_MSAGL && REPORTING
            timer.Stop();
            Debug.Write("{0}: ", String.Format("{0:0.000}", timer.Duration));
            Debug.WriteLine(str);
#endif
        }
    }
}
