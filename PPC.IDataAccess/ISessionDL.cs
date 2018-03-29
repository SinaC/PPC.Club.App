using System.Collections.Generic;
using PPC.Domain;

namespace PPC.IDataAccess
{
    public interface ISessionDL
    {
        List<ShopTransaction> GetTransactions();
        void InsertTransaction(ShopTransaction transaction);
        void UpdateTransaction(ShopTransaction transaction);
        void DeleteTransaction(ShopTransaction transaction);
        void SaveTransactions(IEnumerable<ShopTransaction> transactions);

        List<ClientCart> GetClientCarts();
        void SaveClientCart(ClientCart clientCart);

        bool HasActiveSession();
        void CreateActiveSession();
        Session GetActiveSession();
        void CloseActiveSession();
    }
}
