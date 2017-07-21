#region Copyright Notice

// Copyright (c) by Achilles Software, All rights reserved.
//
// Licensed under the MIT License. See License.txt in the project root for license information.
//
// Send questions regarding this copyright notice to: mailto:todd.thomson@achilles-software.com

#endregion

#region Namespaces

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation.Collections;

#endregion

namespace Msagl.Uwp.UI.Layout
{
    public sealed class VectorChangedEventArgs : IVectorChangedEventArgs
    {
        public CollectionChange CollectionChange { get; set; }
        public uint Index { get; set; }
    }

    public sealed class NodeCollection : IObservableVector<Node>, IList<Node>, INotifyCollectionChanged, IList
    {
        #region Events

        public event VectorChangedEventHandler<Node> VectorChanged;
        public event NotifyCollectionChangedEventHandler CollectionChanged;

        #endregion

        #region Fields

        private ObservableCollection<Node> nodes;

        #endregion

        #region Constructor(s)

        public NodeCollection()
        {
            nodes = new ObservableCollection<Node>();
            nodes.CollectionChanged += Nodes_CollectionChanged;
        }

        #endregion

        #region Public Methods

        public int IndexOf( Node item )
        {
            return nodes.IndexOf( item );
        }

        public void Insert( int index, Node item )
        {
            nodes.Insert( index, item );
        }

        public void RemoveAt( int index )
        {
            nodes.RemoveAt( index );
        }

        public Node this[ int index ]
        {
            get { return nodes[ index ]; }
            set { nodes[ index ] = value; }
        }

        public void Add( Node item )
        {
            nodes.Add( item );
        }

        public void Clear()
        {
            nodes.Clear();
        }

        public bool Contains( Node item )
        {
            return nodes.Contains( item );
        }

        public void CopyTo( Node[] array, int arrayIndex )
        {
            nodes.CopyTo( array, arrayIndex );
        }

        public bool Remove( Node item )
        {
            return nodes.Remove( item );
        }

        public int Count
        {
            get
            {
                return nodes.Count;
            }
        }

        public bool IsReadOnly
        {
            get
            {
                return false;
            }
        }

        public bool IsFixedSize => ((IList)nodes).IsFixedSize;

        public bool IsSynchronized => ((IList)nodes).IsSynchronized;

        public object SyncRoot => ((IList)nodes).SyncRoot;

        object IList.this[ int index ] { get => ((IList)nodes)[ index ]; set => ((IList)nodes)[ index ] = value; }

        public IEnumerator<Node> GetEnumerator()
        {
            return nodes.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return nodes.GetEnumerator();
        }

        #endregion

        #region IList

        public int Add( object value )
        {
            return ((IList)nodes).Add( value );
        }

        public bool Contains( object value )
        {
            return ((IList)nodes).Contains( value );
        }

        public int IndexOf( object value )
        {
            return ((IList)nodes).IndexOf( value );
        }

        public void Insert( int index, object value )
        {
            ((IList)nodes).Insert( index, value );
        }

        public void Remove( object value )
        {
            ((IList)nodes).Remove( value );
        }

        public void CopyTo( Array array, int index )
        {
            ((IList)nodes).CopyTo( array, index );
        }

        #endregion

        #region Event Handlers

        private void Nodes_CollectionChanged( object sender, NotifyCollectionChangedEventArgs e )
        {
            if ( VectorChanged != null )
            {
                VectorChangedEventArgs args = new VectorChangedEventArgs();
                args.CollectionChange = CollectionChange.Reset;
                VectorChanged( this, args );
            }

            if ( CollectionChanged != null )
            {
                CollectionChanged( this, new NotifyCollectionChangedEventArgs( NotifyCollectionChangedAction.Reset ) );
                CollectionChanged( this, e );
            }
        }


        #endregion
    }
}
