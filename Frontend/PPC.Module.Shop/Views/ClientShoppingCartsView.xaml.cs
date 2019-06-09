using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using PPC.Module.Shop.ViewModels;

namespace PPC.Module.Shop.Views
{
    /// <summary>
    /// Interaction logic for ClientShoppingCartsView.xaml
    /// </summary>
    public partial class ClientShoppingCartsView : UserControl
    {
        private const string SourceKeyName = "Source";

        public ClientShoppingCartsView()
        {
            InitializeComponent();
        }

        private new void Drop(object sender, DragEventArgs e)
        {
            ClientShoppingCartViewModel sourceVm = e.Data.GetData(SourceKeyName) as ClientShoppingCartViewModel;
            ClientShoppingCartViewModel targetVm = (e.Source as FrameworkElement)?.DataContext as ClientShoppingCartViewModel;
            ClientShoppingCartsViewModel vm = DataContext as ClientShoppingCartsViewModel;
            if (vm == null || sourceVm == null || targetVm == null)
                return;
            vm.MergeClient(sourceVm, targetVm);
        }

        private new void PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                FrameworkElement fe = sender as FrameworkElement;
                if (fe == null)
                    return;

                Task.Factory.StartNew(() =>
                {
                    Thread.Sleep(500);
                    Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        if (e.LeftButton == MouseButtonState.Pressed)
                        {
                            var data = new DataObject();
                            data.SetData(SourceKeyName, fe.DataContext);
                            DragDrop.DoDragDrop(fe, data, DragDropEffects.Move);
                            e.Handled = true;
                        }
                    }), null);
                }, CancellationToken.None);
                //var data = new DataObject(SourceKeyName, fe.DataContext);
                //DragDrop.DoDragDrop(fe, data, DragDropEffects.Move);
                //e.Handled = true;
            }
        }
    }
}
