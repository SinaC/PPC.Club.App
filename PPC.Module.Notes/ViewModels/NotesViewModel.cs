using System;
using EasyIoc;
using EasyMVVM;
using PPC.IDataAccess;
using PPC.Log;
using PPC.Services.Popup;
using PPC.Domain;

namespace PPC.Module.Notes.ViewModels
{
    public class NotesViewModel : ObservableObject
    {
        private IPopupService PopupService => IocContainer.Default.Resolve<IPopupService>();
        private ILog Logger => IocContainer.Default.Resolve<ILog>();
        private ISessionDL SessionDL => IocContainer.Default.Resolve<ISessionDL>();

        private string _note;
        public string Note
        {
            get { return _note; }
            set
            {
                if (Set(() => Note, ref _note, value))
                    SessionDL.SaveNotes(Note); // TODO: async save
            }
        }

        private bool _isNoteFocused;
        public bool IsNoteFocused
        {
            get { return _isNoteFocused; }
            set
            {
                // Force RaisePropertyChanged
                _isNoteFocused = value;
                RaisePropertyChanged(() => IsNoteFocused);
            }
        }

        public void Reload(Session session)
        {
            Note = session.Notes;
        }

        //public void Reload()
        //{
        //    try
        //    {
        //        Note = SessionDL.GetNotes();
        //    }
        //    catch (Exception ex)
        //    {
        //        Logger.Exception("Error while loading notes", ex);
        //        PopupService.DisplayError("Error while loading notes", ex);
        //    }
        //}

        public void GotFocus()
        {
            IsNoteFocused = true; // grrrrrrrrr f**king focus
        }
    }

    public class NotesViewModelDesignData : NotesViewModel
    {
        public NotesViewModelDesignData()
        {
            Note =
                "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Donec eleifend ipsum sit amet tempor fringilla.Quisque consequat est nec pretium tincidunt. Curabitur viverra velit id sapien dapibus, in auctor magna commodo.Vestibulum vulputate velit eu vehicula dictum. Integer consequat vestibulum dui, nec vulputate sapien porta eu. Ut nec euismod urna. Curabitur porttitor lectus ac viverra mollis. Suspendisse malesuada elementum dui non dignissim. Ut cursus sagittis metus, vel bibendum felis semper sed. Sed in nisl quis urna facilisis eleifend.Lorem ipsum dolor sit amet, consectetur adipiscing elit."
                + "Aliquam ac dignissim nisl. Mauris eu leo a ligula ullamcorper faucibus.Ut ut dignissim diam, nec molestie magna. Integer malesuada leo erat, sit amet accumsan dui cursus eu.Morbi quis ante nunc. Nunc eros lorem, luctus id fermentum eget, dictum nec urna. Vivamus ultricies sed erat eget scelerisque. Mauris id orci vitae metus aliquam pellentesque.Nunc ultricies imperdiet arcu, eu molestie dui maximus rhoncus. Aliquam a diam non nisl aliquet ullamcorper ut eget nunc. Nunc metus mauris, feugiat sit amet lorem ac, ultrices convallis neque.Integer velit justo, fringilla a ultrices et, pretium placerat quam. Vivamus ut lectus fringilla, posuere elit ac, tempus nisi.In et tortor nec felis ultricies tincidunt porttitor vitae tortor. Pellentesque convallis convallis posuere. Aliquam orci lectus, tincidunt quis blandit vel, tempus vel ligula."
                + "Suspendisse finibus luctus fringilla. Aliquam scelerisque magna sit amet eros aliquet cursus. Etiam luctus rutrum ligula, quis lobortis sapien imperdiet at. Nulla facilisi. Fusce tristique varius enim ac interdum. Aliquam maximus velit eu tempus ornare. Proin tincidunt iaculis porta. In semper mauris in augue condimentum, nec egestas sem rhoncus."
                + "Ut vehicula quis turpis at porttitor. Nulla ornare efficitur tortor. Aenean consequat dolor ex, nec ultrices est molestie at. Duis euismod faucibus blandit. Sed nec lectus nibh. Aenean magna tortor, mollis at bibendum pulvinar, vulputate vel eros. Nunc eget sem eget turpis commodo convallis ut varius augue. Quisque condimentum magna non felis elementum, vitae porta leo mattis.In porta neque ut pulvinar pulvinar. Donec viverra mollis velit a fermentum. Integer et massa eleifend, efficitur quam id, placerat sem.Cras luctus, urna et auctor rhoncus, ligula lacus efficitur risus, et convallis ipsum ante ac turpis.Nam placerat interdum dignissim."
                + "Vestibulum non purus lectus. Ut pretium magna in interdum faucibus. Pellentesque habitant morbi tristique senectus et netus et malesuada fames ac turpis egestas.In porta enim vehicula, aliquet libero sit amet, lacinia leo. Maecenas in consequat justo. Proin gravida urna orci, sed tristique orci semper vitae. Mauris at vestibulum nunc, at placerat dolor. Ut sit amet pellentesque metus.Curabitur iaculis hendrerit nunc eget faucibus. Curabitur sit amet ante laoreet, tristique orci porttitor, sollicitudin urna. Interdum et malesuada fames ac ante ipsum primis in faucibus.Praesent commodo volutpat mi eget elementum. Nam at sapien quam. Nulla at eros sit amet ipsum faucibus auctor non vitae turpis.";
        }
    }
}
