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
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Runtime.Serialization;

#endregion

namespace Microsoft.Msagl.Drawing
{
    /// <summary>
    /// Base class for attribute hierarchy.
    /// Some of the attributes are present just for DOT compatibility and not fully supported.  
    /// </summary>
    [DataContract]
    public abstract class AttributeBase
    {
        #region Events

        /// <summary>
        /// An event to signal that the the entity visual state has changed. 
        /// </summary>
        public event EventHandler VisualsChanged;

        #endregion

        #region Constructor(s)

        /// <summary>
        /// a default constructor
        /// </summary>
        protected AttributeBase()
        {
        }

        #endregion

        #region Properties

        static CultureInfo uSCultureInfo = new CultureInfo( "en-US" );
        /// <summary>
        /// The current culture. Not tested with another culture.
        /// </summary>
        public static CultureInfo USCultureInfo
        {
            get { return uSCultureInfo; }
            set { uSCultureInfo = value; }
        }

        Color color = new Color( 0, 0, 0 );
        /// <summary>
        /// The color of the object. The default is Black.
        /// </summary>
        public Color Color
        {
            get { return color; }
            set
            {
                color = value;
                RaiseVisualsChangedEvent();
            }
        }

        internal List<Style> styles = new List<Style>();
        /// <summary>
        /// Gets the list of attribute styles.
        /// </summary>
        [SuppressMessage( "Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays" )]
        public IEnumerable<Style> Styles
        {
            get { return styles; }
        }

        string id;
        /// <summary>
        /// the ID of the entity
        /// </summary>
        public string Id
        {
            get { return id; }
            set { id = value; }
        }

        internal double lineWidth = 1;
        ///<summary>
        /// Influences border width of clusters, border width of nodes and edge thickness.
        ///</summary>
        public virtual double LineWidth
        {
            get { return lineWidth; }
            set
            {
                lineWidth = value;
                RaiseVisualsChangedEvent();
            }
        }

        ///<summary>
        /// The URI of the entity, it seems not to be present in DOT
        ///</summary>
        public string Uri { get; set; }

        #endregion

        #region Public Methods

        /// <summary>
        /// 
        /// </summary>
        /// <param name="style"></param>
        public void AddStyle( Style style )
        {
            styles.Add( style );

            RaiseVisualsChangedEvent();
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="style"></param>
        public void RemoveStyle( Style style )
        {
            styles.Remove( style );

            RaiseVisualsChangedEvent();
        }

        /// <summary>
        /// 
        /// </summary>
        public void ClearStyles()
        {
            styles.Clear();

            RaiseVisualsChangedEvent();
        }

        #endregion

        #region Event Handlers

        internal void RaiseVisualsChangedEvent( object sender, EventArgs args )
        {
            VisualsChanged?.Invoke( sender, args );
        }

        void RaiseVisualsChangedEvent()
        {
            VisualsChanged?.Invoke( this, null );
        }

        #endregion

        internal string IdToString()
        {
            if ( String.IsNullOrEmpty( Id ) )
                return "";

            return "id=" + Utils.Quote( Id );
        }

        [SuppressMessage( "Microsoft.Globalization", "CA1305:SpecifyIFormatProvider", MessageId = "System.Int32.ToString" )]
        internal string StylesToString( string delimeter )
        {
            var al = new List<string>();

            if ( lineWidth != -1 )
                al.Add( "style=\"setlinewidth(" + lineWidth + ")\"" );

            if ( styles != null )
            {
                foreach ( Style style in styles )
                    al.Add( "style=" + Utils.Quote( style.ToString() ) );
            }

            var s = al.ToArray();

            string ret = Utils.ConcatWithDelimeter( delimeter, s );

            return ret;
        }
    }
}