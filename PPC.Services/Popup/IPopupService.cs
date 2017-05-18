using System;
using System.Windows;
using EasyMVVM;

namespace PPC.Services.Popup
{
    public class QuestionActionButton
    {
        public string Caption { get; set; }
        public Action ClickCallback { get; set; }
        public int Order { get; set; }
        public bool CloseOnClick { get; set; }

        public QuestionActionButton()
        {
            ClickCallback = null;
            CloseOnClick = true;
        }
    }

    public interface IPopupService
    {
        void RegisterView<TViewModel, TView>()
            where TViewModel : ObservableObject
            where TView : FrameworkElement;

        void DisplayModal(ObservableObject viewModel, string title);
        void DisplayModal(ObservableObject viewModel, string title, double width, double height, double maxWidth = 800, double maxHeight = 600);

        void DisplayQuestion(string title, string question, params QuestionActionButton[] buttons);

        void DisplayError(string title, string error);
        void DisplayError(string title, Exception ex);

        void Close(ObservableObject viewModel);
    }
}
