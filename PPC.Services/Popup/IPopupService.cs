using System;
using System.Windows;
using EasyMVVM;

namespace PPC.Services.Popup
{
    public interface IPopupService
    {
        void RegisterView<TViewModel, TView>()
            where TViewModel : ObservableObject
            where TView : FrameworkElement;

        void DisplayModal(ObservableObject viewModel, string title);
        void DisplayModal(ObservableObject viewModel, string title, double width, double height, double maxWidth = 800, double maxHeight = 600);

        void DisplayQuestion(string title, string question, params QuestionActionButton[] buttons);

        void DisplayError(string title, string error, Action onCloseAction = null);
        void DisplayError(string title, Exception ex, Action onCloseAction = null);

        void Close(ObservableObject viewModel);
    }
}
