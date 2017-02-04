using System.Windows.Controls;
using System.Windows.Input;

namespace PPC.Sale
{
    public class MyAutoCompleteBox : AutoCompleteBox
    {
        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            e.Handled = false;
        }

    }
}
