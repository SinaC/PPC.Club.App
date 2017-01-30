using System.Windows;
using System.Windows.Input;

namespace PPC.Popup
{
    public interface ISaveNavigationAndFocusPopup
    {
        KeyboardNavigationMode SavedTabNavigationMode { get; set; }
        KeyboardNavigationMode SavedDirectionalNavigationMode { get; set; }
        IInputElement SavedFocusedElement { get; set; }
    }
}
