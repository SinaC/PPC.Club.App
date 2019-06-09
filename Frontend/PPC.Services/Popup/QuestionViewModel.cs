using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using EasyMVVM;

namespace PPC.Services.Popup
{
    internal class QuestionPopupAnswerItem : ObservableObject
    {
        private string _caption;
        public string Caption
        {
            get => _caption;
            set { Set(() => Caption, ref _caption, value); }
        }

        public Action ClickCallback { get; set; }
        public bool CloseOnClick { get; set; }
    }

    internal class QuestionPopupViewModel : ObservableObject
    {
        private readonly IPopupService _popupService;

        private string _question;
        public string Question
        {
            get => _question;
            protected set { Set(() => Question, ref _question, value); }
        }

        private List<QuestionPopupAnswerItem> _answerItems;
        public List<QuestionPopupAnswerItem> AnswerItems
        {
            get => _answerItems;
            protected set { Set(() => AnswerItems, ref _answerItems, value); }
        }

        private ICommand _clickCommand;
        public ICommand ClickCommand => _clickCommand = _clickCommand ?? new RelayCommand<QuestionPopupAnswerItem>(Click);

        public QuestionPopupViewModel(IPopupService modalPopupService)
        {
            _popupService = modalPopupService;
        }

        public void Initialize(string question, params QuestionActionButton[] actionButtons)
        {
            Question = question;
            AnswerItems = (actionButtons ?? Enumerable.Empty<QuestionActionButton>())
                .Select((x,i) => new {Button = x, Order = x.Order ?? i})
                .OrderBy(x => x.Order)
                .Select(x => new QuestionPopupAnswerItem
                {
                    Caption = x.Button.Caption,
                    ClickCallback = x.Button.ClickCallback,
                    CloseOnClick = x.Button.CloseOnClick
                }).ToList();
        }

        private void Click(QuestionPopupAnswerItem answer)
        {
            if (answer != null)
            {
                if (answer.CloseOnClick)
                    _popupService.Close(this);
                answer.ClickCallback?.Invoke();
            }
        }
    }

    internal class QuestionPopupViewModelDesignData : QuestionPopupViewModel
    {
        public QuestionPopupViewModelDesignData() : base(null)
        {
            Question = "How much wood could a woodchuck chuck if a woodchuck could chuck wood ?";
            AnswerItems = new List<QuestionPopupAnswerItem>
            {
                new QuestionPopupAnswerItem
                {
                    Caption = "A lot"
                },
                new QuestionPopupAnswerItem
                {
                    Caption = "42"
                },
                new QuestionPopupAnswerItem
                {
                    Caption = "I don't care"
                },
            };
        }
    }
}
