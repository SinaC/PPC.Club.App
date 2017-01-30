using System.Collections.Generic;
using PPC.MVVM;

namespace PPC.Popup
{
    public class MessagePopupViewModel : ObservableObject
    {
        private List<string> _messages;
        public List<string> Messages
        {
            get { return _messages; }
            set { Set(() => Messages, ref _messages, value); }
        }
    }

    public class MessagePopupViewModelDesignData : MessagePopupViewModel
    {
        public MessagePopupViewModelDesignData()
        {
            Messages = new List<string>
            {
                "LIGNE 1",
                "UNE LONGUE LIGNE 2",
                "UN TRES TRES TRES LONGUE LIGNE 3",
                "LIGNE 4"
            };
        }
    }
}
