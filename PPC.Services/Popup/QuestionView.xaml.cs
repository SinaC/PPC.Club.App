using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace PPC.Services.Popup
{
    /// <summary>
    /// Interaction logic for QuestionView.xaml
    /// </summary>
    internal partial class QuestionView : UserControl
    {
        public QuestionView()
        {
            InitializeComponent();

            //http://stackoverflow.com/questions/17894506/visualtreehelper-getchildrencount-return-0
            //http://stackoverflow.com/questions/29273566/how-to-focus-a-datatemplated-textbox-in-the-first-element-of-an-itemscontrol-in
            AnswerButtons.ItemContainerGenerator.StatusChanged += ItemContainerGeneratorOnStatusChanged;
        }

        private void ItemContainerGeneratorOnStatusChanged(object sender, EventArgs eventArgs)
        {
            if (AnswerButtons.ItemContainerGenerator.Status == GeneratorStatus.ContainersGenerated)
            {
                if (AnswerButtons.Items.Count > 0)
                {
                    var item0Container = AnswerButtons.ItemContainerGenerator.ContainerFromItem(AnswerButtons.Items[0]);
                    var element = item0Container as FrameworkElement;
                    if (element != null)
                        element.Loaded += OnLoaded;
                }
            }
        }

        private void OnLoaded(object sender, RoutedEventArgs routedEventArgs)
        {
            FrameworkElement element = sender as FrameworkElement;
            if (element != null)
                element.Loaded -= OnLoaded;
            SetFocusOnFirstButton();
        }

        private void SetFocusOnFirstButton()
        {
            var firstButton = FindVisualChild<Button>(AnswerButtons.ItemContainerGenerator.ContainerFromIndex(0));
            if (firstButton != null)
            {
                FocusManager.SetFocusedElement(AnswerButtons, firstButton);
                firstButton.Dispatcher.BeginInvoke((Action)delegate
                {
                    firstButton.Focus();
                    Keyboard.Focus(firstButton);
                }, DispatcherPriority.Input);
            }
        }

        private static T FindVisualChild<T>(DependencyObject depObj)
            where T : DependencyObject
        {
            if (depObj != null)
                for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
                {
                    DependencyObject child = VisualTreeHelper.GetChild(depObj, i);
                    var visualChild = child as T;
                    if (visualChild != null)
                        return visualChild;
                    T childItem = FindVisualChild<T>(child);
                    if (childItem != null)
                        return childItem;
                }
            return null;
        }
    }
}
