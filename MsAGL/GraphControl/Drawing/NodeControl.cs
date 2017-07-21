#region Copyright Notice

// Copyright (c) by Achilles Software, All rights reserved.
//
// Licensed under the MIT License. See License.txt in the project root for license information.
//
// Send questions regarding this copyright notice to: mailto:todd.thomson@achilles-software.com

#endregion

#region Namespaces

using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

#endregion

namespace Msagl.Uwp.UI.Drawing
{
    public partial class Node
    {
        /// <summary>
        /// Identifies the ContentTemplate attached property.
        /// </summary>
        public static readonly DependencyProperty ContentTemplateProperty =
            DependencyProperty.RegisterAttached( "ContentTemplate", typeof( DataTemplate ), typeof( Node ), new PropertyMetadata( null ) );

        /// <summary>
        /// Identifies the Shape property.
        /// </summary>
        public static readonly DependencyProperty ShapeProperty =
            DependencyProperty.Register( nameof( Shape ), typeof( Shape ), typeof( Node ), new PropertyMetadata( Shape.Box, OnShapePropertyChanged ) );

        /// <summary>
        /// Gets or sets the shape of the node.
        /// </summary>
        public Shape Shape
        {
            get
            {
                return (Shape)this.GetValue( ShapeProperty );
            }

            set
            {
                this.SetValue( ShapeProperty, value );
            }
        }

        private static void OnShapePropertyChanged( DependencyObject sender, DependencyPropertyChangedEventArgs args )
        {
            Node node = sender as Node;

            Shape newShape = (Shape)args.NewValue;

            // Call UpdateStates because the Value might have caused the control to change ValueStates.
            node.UpdateVisualStates( true );

            // Call OnValueChanged to raise the ValueChanged event.
            node.attr.RaiseVisualsChangedEvent( node, null );
        }

        /// <summary>
        /// Gets the <see cref="DataTemplate"/> instance associated with the specified dependency object.
        /// </summary>
        public static DataTemplate GetContentTemplate( DependencyObject instance )
        {
            if ( instance == null )
            {
                throw new ArgumentNullException();
            }

            return instance.GetValue( ContentTemplateProperty ) as DataTemplate;
        }

        /// <summary>
        /// Sets the provided <see cref="DataTemplate"/> instance to the specified dependency object.
        /// </summary>
        public static void SetContentTemplate( DependencyObject instance, DataTemplate template )
        {
            if ( instance == null )
            {
                throw new ArgumentNullException();
            }

            instance.SetValue( ContentTemplateProperty, template );
        }

        private void UpdateVisualStates( bool useTransitions )
        {
            // The Node currently has no visual states.
        }

        protected override void OnApplyTemplate()
        {
            // TODO: apply node template parts

            UpdateVisualStates( false );
        }
    }
}
