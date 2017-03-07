using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using PPC.Helpers;

namespace PPC.Popups
{
    /// <summary>
    /// Interaction logic for QuestionPopup.xaml
    /// </summary>
    public partial class QuestionPopup : UserControl
    {
        public QuestionPopup()
        {
            InitializeComponent();

            Loaded += OnLoaded;
        }

        // set focus on first button
        private void OnLoaded(object sender, RoutedEventArgs routedEventArgs)
        {
            var firstButton = VisualTreeHelpers.FindVisualChild<Button>(AnswerButtons.ItemContainerGenerator.ContainerFromIndex(0));
            if (firstButton != null)
                FocusManager.SetFocusedElement(AnswerButtons, firstButton);
        }

    }
}
