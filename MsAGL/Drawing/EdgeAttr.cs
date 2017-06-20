using System;
using System.ComponentModel;
using System.Runtime.Serialization;

namespace Microsoft.Msagl.Drawing {
    /// <summary>
    /// Edge layout attributes.
    /// </summary>
#if !SILVERLIGHT && ! WINDOWS_UWP
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Attr"), Description("Edge layout attributes."),
    TypeConverterAttribute(typeof(ExpandableObjectConverter))]
#endif
    [DataContract]
    public sealed class EdgeAttr : AttributeBase
    {
        #region Properties

        /// <summary>
        /// The separation of the edge in layers. The default is 1.
        /// </summary>
        public int Separation { get; set; } = 1;
  
        /// <summary>
        /// Greater weight keeps the edge short
        /// </summary>
        public int Weight { get; set; } = 1;

        ArrowStyle arrowheadAtSource = ArrowStyle.NonSpecified;

        /// <summary>
        /// Arrow style; Normal and None are supported.
        /// </summary>
        public ArrowStyle ArrowheadAtSource
        {
            get { return arrowheadAtSource; }
            set
            {
                arrowheadAtSource = value;

                RaiseVisualsChangedEvent( this, null );
            }
        }

        /// <summary>
        /// Arrow style; for the moment only a few are supported.
        /// </summary>
        ArrowStyle arrowheadAtTarget = ArrowStyle.NonSpecified;

        /// <summary>
        /// Arrow style; for the moment only the Normal and None are supported.
        /// </summary>
        public ArrowStyle ArrowheadAtTarget
        {
            get { return arrowheadAtTarget; }
            set
            {
                arrowheadAtTarget = value;
                RaiseVisualsChangedEvent( this, null );
            }
        }

        double arrowheadLength = 10;

        /// <summary>
        /// the length of an arrowhead of the edge
        /// </summary>
        public double ArrowheadLength
        {
            get { return arrowheadLength; }
            set
            {
                arrowheadLength = value;
                RaiseVisualsChangedEvent( this, null );
            }
        }

        /// <summary>
        /// Signals if an arrow should be drawn at the end.
        /// </summary>
        public bool ArrowAtTarget
        {
            get { return ArrowheadAtTarget != ArrowStyle.None; }
        }

        /// <summary>
        /// is true if need to draw an arrow at the source
        /// </summary>
        public bool ArrowAtSource
        {
            get { return !(ArrowheadAtSource == ArrowStyle.NonSpecified || ArrowheadAtSource == ArrowStyle.None); }
        }

        /// <summary>
        /// the position of the arrow head at the source. Applicable for MDS layouts
        /// </summary>
        public double Length { get; set; } = 1;

        #endregion

        #region Constructor(s)

        /// <summary>
        /// Constructor
        /// </summary>
        public EdgeAttr() {
            Color = new Color(0, 0, 0);
        }

        #endregion

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public EdgeAttr Clone() {
            return MemberwiseClone() as EdgeAttr;
        }

        /// <summary>
        /// ToString with a parameter.
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:ValidateArgumentsOfPublicMethods"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1304:SpecifyCultureInfo", MessageId = "System.String.ToLower")]
        public string ToStringWithText(string text) {
            string ret = "";
            if (!String.IsNullOrEmpty(text)) {
                text = text.Replace("\r\n", "\\n");
                ret += "label=" + Utils.Quote(text);
            }


            if (arrowheadAtSource != ArrowStyle.NonSpecified)
                ret = Utils.ConcatWithComma(ret, "arrowhead=" + arrowheadAtSource.ToString().ToLower());

            ret = Utils.ConcatWithComma(ret, Utils.ColorToString("color=", Color.ToString()),
                                StylesToString(","),                              
                                IdToString()
                                );

            return ret;
        }

    }
}
