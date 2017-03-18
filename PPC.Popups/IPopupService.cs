using System;
using EasyMVVM;

namespace PPC.Popups
{
    public class ActionButton
    {
        public string Caption { get; set; }
        public Action ClickCallback { get; set; }
        public int Order { get; set; }
        public bool CloseOnClick { get; set; }

        public ActionButton()
        {
            ClickCallback = null;
            CloseOnClick = true;
        }
    }

    public interface IPopupService
    {
        // Modal popup
        IPopup DisplayModal<T>(T viewModel, string title, Func<bool> closeConfirmation = null)
            where T : ObservableObject;

        // Messages popup (shouldn't be moved)
        IPopup DisplayMessages(params string[] messages);

        // Question popup (displayed in a Modal)
        IPopup DisplayQuestion(string title, string question, params ActionButton[] actionButtons);

        // Error popup
        IPopup DisplayError(string title, string error);
        IPopup DisplayError(string title, Exception ex);

        // Move
        void Move(IPopup popup, double horizontalOffset, double verticalOffset);

        // Close
        void Close(IPopup popup);

        // Close
        void Close<T>(T viewModel)
            where T : ObservableObject;
    }
}
