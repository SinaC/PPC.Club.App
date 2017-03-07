using System;
using System.Windows.Input;
using EasyMVVM;

namespace PPC.Popups
{
    public class ErrorPopupViewModel : ObservableObject
    {
        public IPopupService PopupService { get; }

        private string _summary;
        public string Summary
        {
            get { return _summary; }
            set { Set(() => Summary, ref _summary, value); }
        }

        private string _details;
        public string Details
        {
            get { return _details; }
            set
            {
                if (Set(() => Details, ref _details, value))
                    RaisePropertyChanged(() => HasDetails);
            }
        }
        
        public bool HasDetails => !string.IsNullOrWhiteSpace(Details);

        private ICommand _clickCommand;
        public ICommand ClickCommand
        {
            get
            {
                _clickCommand = _clickCommand ?? new RelayCommand<QuestionPopupAnswerItem>(Click);
                return _clickCommand;
            }
        }

        private void Click(QuestionPopupAnswerItem answer)
        {
            PopupService.Close(this);
        }

        public ErrorPopupViewModel(IPopupService popupService, Exception ex)
        {
            PopupService = popupService;

            Summary = ex.Message;
            Details = ex.ToString();
        }

        public ErrorPopupViewModel(IPopupService popupService, string error)
        {
            PopupService = popupService;

            Summary = error;
            Details = null;
        }
    }

    public class ErrorPopupViewModelDesignData : ErrorPopupViewModel
    {
        public ErrorPopupViewModelDesignData() : base(null, new Exception())
        {
            Summary = "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Maecenas mattis, magna convallis euismod vehicula, ligula tellus commodo ipsum, a mollis felis ligula elementum eros. Sed non molestie eros. Etiam quis venenatis justo. Phasellus commodo leo ut nibh aliquam, id ornare eros elementum. Nullam non turpis sagittis, scelerisque magna faucibus, posuere mauris. Quisque eu quam sit amet augue aliquam pretium nec in magna. Ut dui nulla, facilisis dignissim ultrices eu, fringilla vel odio. Vestibulum ante ipsum primis in faucibus orci luctus et ultrices posuere cubilia Curae; Donec vitae mi ac ante convallis sodales. Nulla finibus augue ac eleifend commodo. Cras eu turpis nunc. Morbi in ex vel lectus dignissim tincidunt. Suspendisse lobortis magna massa.";
            Details =
                "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Maecenas mattis, magna convallis euismod vehicula, ligula tellus commodo ipsum, a mollis felis ligula elementum eros. Sed non molestie eros. Etiam quis venenatis justo. Phasellus commodo leo ut nibh aliquam, id ornare eros elementum. Nullam non turpis sagittis, scelerisque magna faucibus, posuere mauris. Quisque eu quam sit amet augue aliquam pretium nec in magna. Ut dui nulla, facilisis dignissim ultrices eu, fringilla vel odio. Vestibulum ante ipsum primis in faucibus orci luctus et ultrices posuere cubilia Curae; Donec vitae mi ac ante convallis sodales. Nulla finibus augue ac eleifend commodo. Cras eu turpis nunc. Morbi in ex vel lectus dignissim tincidunt. Suspendisse lobortis magna massa."
                + "Ut interdum nibh nec scelerisque eleifend. Duis non lorem vel erat consequat placerat mattis ut ante. Vivamus at leo id metus aliquam tristique eget non neque. Duis ac velit nec sem tempus lacinia.Pellentesque ut lectus cursus tellus fringilla sodales vitae eu felis. Morbi convallis porttitor justo vel ullamcorper. Proin purus turpis, venenatis sit amet tortor quis, fermentum semper nunc.Duis sodales maximus sagittis. Proin ac condimentum dui. Sed in semper elit, eget congue sapien. Donec malesuada tincidunt mi sed dignissim. Cras nec iaculis justo, nec fermentum augue. Nullam aliquam risus purus, id gravida nibh facilisis eget. Nullam justo est, facilisis in sagittis eu, blandit a libero. Phasellus eget est quis est gravida interdum quis eget augue. Sed congue, urna ut ultricies lacinia, sem odio fermentum purus, vitae congue eros elit sit amet erat."
                + "Integer nec diam faucibus, pretium nunc ut, egestas lacus.Donec odio sem, semper sed est at, vestibulum euismod purus. Donec vel hendrerit neque. Sed non nisl et neque viverra ullamcorper.Maecenas diam metus, suscipit sed venenatis bibendum, facilisis sit amet velit.Quisque suscipit libero a egestas imperdiet. Etiam vel viverra urna. Praesent posuere purus quam, vel eleifend sapien ultricies et. Praesent ante odio, suscipit non felis in, ornare mattis risus.Nullam augue risus, sollicitudin et pretium in, auctor a nunc."
                + "Vestibulum quis auctor lorem. Suspendisse interdum lectus quis ipsum maximus, at suscipit quam accumsan.Nullam condimentum sed tortor eget tincidunt. Nullam ullamcorper vehicula tincidunt. Nulla sed nisi ac nisi ultrices egestas.Nam leo ipsum, dignissim et nulla ac, euismod venenatis felis. Aenean et urna quam. Morbi eget dapibus est. Mauris tristique quam vel magna laoreet ultrices.Proin augue augue, facilisis a erat eu, fermentum fringilla ex. Suspendisse fringilla volutpat mauris."
                + "Vestibulum ante ipsum primis in faucibus orci luctus et ultrices posuere cubilia Curae; Curabitur vestibulum fermentum sollicitudin. Sed nec ornare sem. Phasellus arcu quam, dictum at ipsum ac, malesuada rhoncus metus. Aenean rhoncus dui at enim volutpat viverra.Quisque nec nisi tempor risus tempus luctus.Aliquam arcu metus, eleifend ac sollicitudin at, molestie eget sapien. Cras pellentesque, mi vitae fringilla ultrices, turpis metus bibendum turpis, ut tincidunt neque leo vitae ipsum.Pellentesque habitant morbi tristique senectus et netus et malesuada fames ac turpis egestas.";
        }
    }
}
