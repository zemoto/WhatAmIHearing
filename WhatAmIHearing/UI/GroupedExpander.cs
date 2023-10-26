using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Linq;

namespace WhatAmIHearing.UI;

internal sealed class GroupedExpander : Expander
{
   private static readonly Dictionary<string, List<GroupedExpander>> ExpanderGroups = new();

   public static readonly DependencyProperty GroupNameProperty = DependencyProperty.Register(
      nameof( GroupName ), typeof( string ), typeof( GroupedExpander ), new PropertyMetadata( string.Empty, OnGroupNameChanged ) );
   private static void OnGroupNameChanged( DependencyObject sender, DependencyPropertyChangedEventArgs e )
   {
      if ( sender is not GroupedExpander expander )
      {
         return;
      }

      if ( e.OldValue is string oldGroupName && !string.IsNullOrEmpty( oldGroupName ) )
      {
         var group = ExpanderGroups[oldGroupName];
         group.Remove( expander );

         if ( group.Count == 0 )
         {
            ExpanderGroups.Remove( oldGroupName );
         }
      }

      if ( e.NewValue is string newGroupName && !string.IsNullOrEmpty( newGroupName ) )
      {
         if ( !ExpanderGroups.ContainsKey( newGroupName ) )
         {
            ExpanderGroups[newGroupName] = new List<GroupedExpander>();
         }

         ExpanderGroups[newGroupName].Add( expander );
      }
   }

   public string GroupName
   {
      get => (string)GetValue( GroupNameProperty );
      set => SetValue( GroupNameProperty, value );
   }

   public GroupedExpander() => Expanded += OnExpanded;

   private void OnExpanded( object sender, RoutedEventArgs e )
   {
      if ( string.IsNullOrEmpty( GroupName ) )
      {
         return;
      }

      foreach ( var expander in ExpanderGroups[GroupName].Where( expander => expander != this ) )
      {
         expander.IsExpanded = false;
      }
   }
}
