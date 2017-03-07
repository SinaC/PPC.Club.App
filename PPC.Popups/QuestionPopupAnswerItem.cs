using System;
using EasyMVVM;

namespace PPC.Popups
{
    public class QuestionPopupAnswerItem : ObservableObject
    {
        private string _caption;
        public string Caption
        {
            get { return _caption; }
            set { Set(() => Caption, ref _caption, value); }
        }

        public Action ClickCallback { get; set; }
        public bool CloseOnClick { get; set; }
    }
}
