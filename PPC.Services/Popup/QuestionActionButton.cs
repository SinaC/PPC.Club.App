using System;

namespace PPC.Services.Popup
{
    public class QuestionActionButton
    {
        public string Caption { get; set; }
        public Action ClickCallback { get; set; }
        public int? Order { get; set; }
        public bool CloseOnClick { get; set; }

        public QuestionActionButton()
        {
            ClickCallback = null;
            Order = null;
            CloseOnClick = true;
        }

        public static QuestionActionButton Ok(Action okAction = null)
        {
            return new QuestionActionButton
            {
                Caption = "Ok",
                ClickCallback = okAction
            };
        }

        public static QuestionActionButton Yes(Action yesAction = null)
        {
            return new QuestionActionButton
            {
                Caption = "Yes",
                ClickCallback = yesAction
            };
        }

        public static QuestionActionButton No(Action noAction = null)
        {
            return new QuestionActionButton
            {
                Caption = "No",
                ClickCallback = noAction
            };
        }

        public static QuestionActionButton Cancel(Action cancelAction = null)
        {
            return new QuestionActionButton
            {
                Caption = "Cancel",
                ClickCallback = cancelAction
            };
        }
    }
}
