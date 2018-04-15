using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using EasyMVVM;
using PPC.Helpers;

namespace PPC.Services.Popup
{
    public sealed class PopupService : IPopupService
    {
        private readonly Dictionary<Type, Type> _associatedViewsByViewModels;
        private readonly object _windowsLockObject;
        private readonly List<PopupWindow> _windows;
        private readonly Window _mainWindow;

        #region IPopupService

        public void RegisterView<TViewModel, TView>()
            where TViewModel : ObservableObject
            where TView : FrameworkElement
        {
            Type viewModelType = typeof(TViewModel);
            if (_associatedViewsByViewModels.ContainsKey(viewModelType))
                _associatedViewsByViewModels[viewModelType] = typeof(TView); // replace old associated type
            else
                _associatedViewsByViewModels.Add(typeof(TViewModel), typeof(TView));
        }

        public void DisplayModal(ObservableObject viewModel, string title)
        {
            FrameworkElement view = CreatePopupView(viewModel);
            DisplayViewInModalPopup(view, title);
        }

        public void DisplayModal(ObservableObject viewModel, string title, double width, double height, double maxWidth = 800, double maxHeight = 600)
        {
            FrameworkElement view = CreatePopupView(viewModel);
            DisplayViewInModalPopup(view, title, width, height, maxWidth, maxHeight);
        }

        public void DisplayQuestion(string title, string question, params QuestionActionButton[] buttons)
        {
            QuestionPopupViewModel viewModel = new QuestionPopupViewModel(this);
            viewModel.Initialize(question, buttons);
            QuestionView view = new QuestionView
            {
                DataContext = viewModel
            };
            DisplayViewInModalPopup(view, title);
        }

        public void DisplayError(string title, string error, Action onCloseAction = null)
        {
            ErrorViewModel viewModel = new ErrorViewModel(this, error);
            ErrorView view = new ErrorView
            {
                DataContext = viewModel
            };
            DisplayViewInModalPopup(view, title, onCloseAction);
        }

        public void DisplayError(string title, Exception ex, Action onCloseAction = null)
        {
            ErrorViewModel viewModel = new ErrorViewModel(this, ex);
            ErrorView view = new ErrorView
            {
                DataContext = viewModel
            };
            DisplayViewInModalPopup(view, title, onCloseAction);
        }

        public void Close(ObservableObject viewModel)
        {
            PopupWindow window;
            lock (_windowsLockObject)
                window = _windows.FirstOrDefault(x => (x.PopupContentPresenter.Content as FrameworkElement)?.DataContext == viewModel);
            window?.Close();
        }

        #endregion

        public PopupService(Window mainWindow)
        {
            _associatedViewsByViewModels = new Dictionary<Type, Type>();
            _windowsLockObject = new object();
            _windows = new List<PopupWindow>();
            _mainWindow = mainWindow;
        }

        private FrameworkElement CreatePopupView(ObservableObject viewModel)
        {
            Type viewModelType = viewModel.GetType();

            // Search in registered view-viewmodel
            Type viewType;
            if (!_associatedViewsByViewModels.TryGetValue(viewModelType, out viewType))
            // if not found in registered view-viewmodel, search attribute
            {
                PopupAssociatedViewAttribute attribute = viewModelType.GetCustomAttributes<PopupAssociatedViewAttribute>().FirstOrDefault();
                if (attribute == null)
                    throw new InvalidOperationException($"PopupAssociatedView attribute not found on {viewModelType}.");
                //// Search ClosePopupCommandAttribute and hook command
                //Type iCommandType = typeof(ICommand);
                //IEnumerable<FieldInfo> closeCommandFields = viewModelType.GetFields(BindingFlags.NonPublic | BindingFlags.Instance).Where(x => x.GetCustomAttributes<ClosePopupCommandAttribute>().Any() && x.FieldType == iCommandType); // TODO: inheriting from ICommand ?
                //foreach (FieldInfo fieldInfo in closeCommandFields)
                //{
                //    ClosePopupCommandAttribute closePopupCommandAttribute = fieldInfo.GetCustomAttributes<ClosePopupCommandAttribute>().FirstOrDefault();
                //    PropertyInfo relatedProperty = viewModelType.GetProperty(closePopupCommandAttribute.RelatedProperty);
                //    ICommand originalCommand = relatedProperty.GetValue(viewModel) as ICommand;
                //    // TODO: check if RelayCommand<T>
                //    ICommand closedCommand = new RelayCommand<object>(o =>
                //    {
                //        Close(viewModel);
                //        originalCommand.Execute(o);
                //    });
                //    fieldInfo.SetValue(viewModel, closedCommand);
                //}
                //
                viewType = attribute.ViewType;
            }
            FrameworkElement view = Activator.CreateInstance(viewType) as FrameworkElement;
            if (view == null)
                throw new InvalidOperationException($"Popup {viewType} instance is not a valid FrameworkElement.");
            view.DataContext = viewModel;
            return view;
        }

        private void DisplayViewInModalPopup(FrameworkElement view, string title, Action onCloseAction = null)
        {
            //http://stackoverflow.com/questions/2593498/wpf-modal-window-using-showdialog-blocks-all-other-windows
            //http://stackoverflow.com/questions/5971686/how-to-create-a-task-tpl-running-a-sta-thread
            TaskScheduler scheduler = TaskScheduler.FromCurrentSynchronizationContext();
            CancellationToken token = new CancellationToken();
            Task.Factory.StartNew(() =>
                {
                    PopupWindow modalPopupWindow = new PopupWindow
                    {
                        Owner = _mainWindow,
                        Title = title,
                        PopupContentPresenter =
                        {
                            Content = view
                        },
                        SizeToContent = SizeToContent.WidthAndHeight

                    };
                    modalPopupWindow.Closing += ModalPopupWindowOnClosing;
                    lock (_windowsLockObject)
                        _windows.Add(modalPopupWindow);
                    modalPopupWindow.SourceInitialized += ModalPopupWindow_SourceInitialized; ;
                    modalPopupWindow.ShowDialog();
                    onCloseAction?.Invoke();
                },
                token,
                TaskCreationOptions.None,
                scheduler);
        }

        private void ModalPopupWindow_SourceInitialized(object sender, EventArgs e)
        {
            Window window = (Window) sender;
            if (window == null)
                return;
            window.SourceInitialized -= ModalPopupWindow_SourceInitialized;
            WindowHelpers.HideMinimizeAndMaximizeButtons(window);
        }

        private void DisplayViewInModalPopup(FrameworkElement view, string title, double width, double height, double maxWidth, double maxHeight)
        {
            //http://stackoverflow.com/questions/2593498/wpf-modal-window-using-showdialog-blocks-all-other-windows
            //http://stackoverflow.com/questions/5971686/how-to-create-a-task-tpl-running-a-sta-thread
            TaskScheduler scheduler = TaskScheduler.FromCurrentSynchronizationContext();
            CancellationToken token = new CancellationToken();
            Task.Factory.StartNew(() =>
                {
                    PopupWindow modalPopupWindow = new PopupWindow
                    {
                        Owner = _mainWindow,
                        Title = title,
                        PopupContentPresenter =
                        {
                            Content = view
                        },
                        Width = width,
                        Height = height,
                        MaxWidth = maxWidth,
                        MaxHeight = maxHeight
                    };
                    modalPopupWindow.Closing += ModalPopupWindowOnClosing;
                    lock (_windowsLockObject)
                        _windows.Add(modalPopupWindow);
                    modalPopupWindow.SourceInitialized += ModalPopupWindow_SourceInitialized;
                    modalPopupWindow.ShowDialog();
                },
                token,
                TaskCreationOptions.None,
                scheduler);
        }

        private void ModalPopupWindowOnClosing(object sender, CancelEventArgs cancelEventArgs)
        {
            PopupWindow window = sender as PopupWindow;
            if (window == null)
                return;
            window.Closing -= ModalPopupWindowOnClosing;
            lock (_windowsLockObject)
                _windows.Remove(window);
        }

        #region ideas

        private void DisplayNonModal(ObservableObject viewModel, string title)
        {
            Type viewModelType = viewModel.GetType();
            PopupAssociatedViewAttribute attribute = viewModelType.GetCustomAttributes<PopupAssociatedViewAttribute>().FirstOrDefault();
            if (attribute == null)
                throw new InvalidOperationException($"PopupAssociatedViewAttribute not found on {viewModelType}.");
            FrameworkElement view = Activator.CreateInstance(attribute.ViewType) as FrameworkElement;
            if (view == null)
                throw new InvalidOperationException($"Cannot create {attribute.ViewType} instance.");
            view.DataContext = viewModel;
            PopupWindow modalPopupWindow = new PopupWindow
            {
                Owner = _mainWindow,
                Title = title,
                PopupContentPresenter =
                {
                    Content = view
                }
            };
            modalPopupWindow.Closed += (sender, args) => _mainWindow.IsEnabled = true;
            _mainWindow.IsEnabled = false;
            modalPopupWindow.Show(); // TODO: ShowDialog ?
        }

        #endregion
    }
}
