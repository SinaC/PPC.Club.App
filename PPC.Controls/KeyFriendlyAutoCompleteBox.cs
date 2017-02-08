using System.Windows.Controls;
using System.Windows.Input;

namespace PPC.Controls
{
    public class KeyFriendlyAutoCompleteBox : AutoCompleteBox
    {
        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            e.Handled = false;
        }

    }
}
