using PPC.MVVM;

namespace PPC.Tab
{
    public abstract class TabBase : ObservableObject
    {
        public abstract string Header { get; }

        public virtual bool IsClosable => false;
    }
}
