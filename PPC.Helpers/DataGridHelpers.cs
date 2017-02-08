using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace PPC.Helpers
{
    public static class DataGridHelpers
    {
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
            DataGrid dataGrid = obj as DataGrid;
            if (dataGrid != null)
            {
                if (args.OldValue == null && args.NewValue != null)
                    dataGrid.MouseDoubleClick += ExecuteDataGridDoubleClick;
                else if (args.OldValue != null && args.NewValue == null)
                    dataGrid.MouseDoubleClick -= ExecuteDataGridDoubleClick;
            }
        }

        private static void ExecuteDataGridDoubleClick(object sender, MouseButtonEventArgs args)
        {
            FrameworkElement obj = sender as FrameworkElement;
            ICommand cmd;
            if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
                 cmd = obj?.GetValue(DataGridDoubleClickWithModifierProperty) as ICommand;
            else
                cmd = obj?.GetValue(DataGridDoubleClickProperty) as ICommand;
            if (cmd == null)
                return;
            DataGridRow row = VisualTreeHelpers.FindAncestor<DataGridRow>(args.OriginalSource as DependencyObject);
            object parameter = row?.DataContext;
            if (cmd.CanExecute(parameter))
                cmd.Execute(parameter);
        }
    }
}
