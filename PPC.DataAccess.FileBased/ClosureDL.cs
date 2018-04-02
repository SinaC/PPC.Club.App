using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using PPC.Common;
using PPC.Domain;
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
            using (XmlTextWriter writer = new XmlTextWriter(filename, Encoding.UTF8))
            {
                writer.Formatting = Formatting.Indented;
                DataContractSerializer serializer = new DataContractSerializer(typeof(Domain.Closure));
                serializer.WriteObject(writer, closure);
            }
        }
    }
}
