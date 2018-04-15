using System.IO;
using PPC.Common;
using PPC.Domain;
using PPC.Helpers;
using PPC.IDataAccess;

namespace PPC.DataAccess.FileBased
{
    public class ClosureDL : IClosureDL
    {
        public void SaveClosure(Closure closure)
        {
            if (!Directory.Exists(PPCConfigurationManager.CashRegisterClosurePath))
                Directory.CreateDirectory(PPCConfigurationManager.CashRegisterClosurePath);
            string filename = $"{PPCConfigurationManager.CashRegisterClosurePath}CashRegister_{closure.CreationTime:yyyy-MM-dd_HH-mm-ss}.xml";
            DataContractHelpers.Write(filename, closure);
        }
    }
}
