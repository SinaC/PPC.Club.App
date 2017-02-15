using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace PPC.Helpers
{
    public static class DataGridHelpers
    {

        #region DoubleClick

        public static readonly DependencyProperty DataGridDoubleClickProperty = DependencyProperty.RegisterAttached(
            "DataGridDoubleClickCommand",
            typeof(ICommand),
            typeof(DataGridHelpers),
            new PropertyMetadata(AttachOrRemoveDataGridDoubleClickEvent));

        public static ICommand GetDataGridDoubleClickCommand(DependencyObject obj)
        {
            return (ICommand) obj.GetValue(DataGridDoubleClickProperty);
        }

        public static void SetDataGridDoubleClickCommand(DependencyObject obj, ICommand value)
        {
            obj.SetValue(DataGridDoubleClickProperty, value);
        }

        public static readonly DependencyProperty DataGridDoubleClickWithModifierProperty = DependencyProperty.RegisterAttached(
            "DataGridDoubleClickWithModifierCommand",
            typeof(ICommand),
            typeof(DataGridHelpers),
            new PropertyMetadata(AttachOrRemoveDataGridDoubleClickEvent));

        public static ICommand GetDataGridDoubleClickWithModifierCommand(DependencyObject obj)
        {
            return (ICommand)obj.GetValue(DataGridDoubleClickWithModifierProperty);
        }

        public static void SetDataGridDoubleClickWithModifierCommand(DependencyObject obj, ICommand value)
        {
            obj.SetValue(DataGridDoubleClickWithModifierProperty, value);
        }

        public static void AttachOrRemoveDataGridDoubleClickEvent(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            DataGrid @this = obj as DataGrid;
            if (@this != null)
            {
                if (args.OldValue == null && args.NewValue != null)
                    @this.MouseDoubleClick += ExecuteDataGridDoubleClick;
                else if (args.OldValue != null && args.NewValue == null)
                    @this.MouseDoubleClick -= ExecuteDataGridDoubleClick;
            }
        }

        private static void ExecuteDataGridDoubleClick(object sender, MouseButtonEventArgs args)
        {
            DataGrid @this = sender as DataGrid;
            ICommand cmd;
            if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
                 cmd = @this?.GetValue(DataGridDoubleClickWithModifierProperty) as ICommand;
            else
                cmd = @this?.GetValue(DataGridDoubleClickProperty) as ICommand;
            if (cmd == null)
                return;
            DataGridRow row = VisualTreeHelpers.FindAncestor<DataGridRow>(args.OriginalSource as DependencyObject);
            object parameter = row?.DataContext ?? @this.SelectedItem;
            if (cmd.CanExecute(parameter))
                cmd.Execute(parameter);
        }

        #endregion

        #region AutoScroll

        public static readonly DependencyProperty AutoscrollProperty = DependencyProperty.RegisterAttached(
            "Autoscroll",
            typeof(bool),
            typeof(DataGridHelpers),
            new PropertyMetadata(default(bool), AutoscrollChangedCallback));

        private static readonly Dictionary<DataGrid, NotifyCollectionChangedEventHandler> HandlersDict = new Dictionary<DataGrid, NotifyCollectionChangedEventHandler>();

        private static void AutoscrollChangedCallback(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs args)
        {
            var dataGrid = dependencyObject as DataGrid;
            if (dataGrid == null)
                throw new InvalidOperationException("Dependency object is not DataGrid.");

            if ((bool)args.OldValue)
            {
                Unsubscribe(dataGrid);
                dataGrid.Unloaded -= DataGridOnUnloaded;
                dataGrid.Loaded -= DataGridOnLoaded;
            }
            if ((bool)args.NewValue)
            {
                Subscribe(dataGrid);
                dataGrid.Unloaded += DataGridOnUnloaded;
                dataGrid.Loaded += DataGridOnLoaded;
            }
        }

        private static void Subscribe(DataGrid dataGrid)
        {
            NotifyCollectionChangedEventHandler handler;
            HandlersDict.TryGetValue(dataGrid, out handler);
            if (handler != null)
                return;
            handler = (sender, eventArgs) => ScrollToEnd(dataGrid);
            HandlersDict.Add(dataGrid, handler);
            ((INotifyCollectionChanged)dataGrid.Items).CollectionChanged += handler;
            ScrollToEnd(dataGrid);
        }

        private static void Unsubscribe(DataGrid dataGrid)
        {
            NotifyCollectionChangedEventHandler handler;
            HandlersDict.TryGetValue(dataGrid, out handler);
            if (handler == null)
                return;
            ((INotifyCollectionChanged)dataGrid.Items).CollectionChanged -= handler;
            HandlersDict.Remove(dataGrid);
        }

        private static void DataGridOnLoaded(object sender, RoutedEventArgs routedEventArgs)
        {
            var dataGrid = (DataGrid)sender;
            if (GetAutoscroll(dataGrid))
                Subscribe(dataGrid);
        }

        private static void DataGridOnUnloaded(object sender, RoutedEventArgs routedEventArgs)
        {
            var dataGrid = (DataGrid)sender;
            if (GetAutoscroll(dataGrid))
                Unsubscribe(dataGrid);
        }

        private static void ScrollToEnd(DataGrid datagrid)
        {
            if (datagrid.Items.Count == 0)
                return;
            datagrid.ScrollIntoView(datagrid.Items[datagrid.Items.Count - 1]);
        }

        public static void SetAutoscroll(DependencyObject element, bool value)
        {
            element.SetValue(AutoscrollProperty, value);
        }

        public static bool GetAutoscroll(DependencyObject element)
        {
            return (bool)element.GetValue(AutoscrollProperty);
        }

        #endregion
    }
}
