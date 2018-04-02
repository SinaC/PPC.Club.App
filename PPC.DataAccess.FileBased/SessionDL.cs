using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Xml;
using PPC.Common;
using PPC.Domain;
using PPC.IDataAccess;

namespace PPC.DataAccess.FileBased
{
    public class SessionDL : ISessionDL
    {
        private const string ShopFilename = "_shop.xml";
        private const string NotesFilename = "_note.xml";

        #region Transactions

        private string BuildShopFilename => $"{PPCConfigurationManager.BackupPath}{ShopFilename}";

        public List<ShopTransaction> GetTransactions()
        {
            List<ShopTransaction> transactions = new List<ShopTransaction>();

            string filename = BuildShopFilename;
            if (File.Exists(filename))
            {
                Shop shop;
                using (XmlTextReader reader = new XmlTextReader(filename))
                {
                    DataContractSerializer serializer = new DataContractSerializer(typeof(Shop));
                    shop = (Shop) serializer.ReadObject(reader);
                }
                transactions = shop.Transactions;
            }

            return transactions;
        }

        public void InsertTransaction(ShopTransaction transaction)
        {
            List<ShopTransaction> transactions = GetTransactions();
            transactions.Add(transaction);
            SaveTransactions(transactions);
        }

        public void UpdateTransaction(ShopTransaction transaction)
        {
            List<ShopTransaction> transactions = GetTransactions();
            transactions.RemoveAll(x => x.Guid == transaction.Guid);
            transactions.Add(transaction);
            SaveTransactions(transactions);
        }

        public void DeleteTransaction(ShopTransaction transaction)
        {
            List<ShopTransaction> transactions = GetTransactions();
            transactions.RemoveAll(x => x.Guid == transaction.Guid);
            SaveTransactions(transactions);
        }

        public void SaveTransactions(IEnumerable<ShopTransaction> transactions)
        {
            if (!Directory.Exists(PPCConfigurationManager.BackupPath))
                Directory.CreateDirectory(PPCConfigurationManager.BackupPath);
            Shop shop = new Shop
            {
                Transactions = transactions.ToList(),
            };
            string filename = $"{PPCConfigurationManager.BackupPath}{ShopFilename}";
            using (XmlTextWriter writer = new XmlTextWriter(filename, Encoding.UTF8))
            {
                writer.Formatting = Formatting.Indented;
                DataContractSerializer serializer = new DataContractSerializer(typeof(Shop));
                serializer.WriteObject(writer, shop);
            }
        }

        #endregion

        #region Client carts

        private Func<ClientCart, string> BuildClientFilename => cart => $"{PPCConfigurationManager.BackupPath}{(cart.HasFullPlayerInfos ? cart.DciNumber : cart.ClientName.ToLowerInvariant())}.xml";

        public List<ClientCart> GetClientCarts()
        {
            List<Exception> exceptions = null;
            List<ClientCart> carts = new List<ClientCart>();
            string path = PPCConfigurationManager.BackupPath;
            if (Directory.Exists(path))
            {
                foreach (string filename in Directory.EnumerateFiles(path, "*.xml", SearchOption.TopDirectoryOnly).Where(x => !x.Contains(ShopFilename) && !x.Contains(NotesFilename)))
                {
                    try
                    {
                        ClientCart cart = LoadClient(filename);
                        carts.Add(cart);
                    }
                    catch (Exception ex)
                    {
                        // TODO: logging
                        exceptions = exceptions ?? new List<Exception>();
                        exceptions.Add(new Exception($"Error while loading {filename ?? "??"} cart", ex));
                    }
                }
            }
            if (exceptions != null)
                throw new GetClientCartsException(carts, exceptions);
            return carts;
        }

        public void SaveClientCart(ClientCart clientCart)
        {
            if (!Directory.Exists(PPCConfigurationManager.BackupPath))
                Directory.CreateDirectory(PPCConfigurationManager.BackupPath);
            string filename = BuildClientFilename(clientCart);
            using (XmlTextWriter writer = new XmlTextWriter(filename, Encoding.UTF8))
            {
                writer.Formatting = Formatting.Indented;
                DataContractSerializer serializer = new DataContractSerializer(typeof(ClientCart));
                serializer.WriteObject(writer, clientCart);
            }
        }

        private ClientCart LoadClient(string filename)
        {
            ClientCart cart;
            using (XmlTextReader reader = new XmlTextReader(filename))
            {
                DataContractSerializer serializer = new DataContractSerializer(typeof(ClientCart));
                cart = (ClientCart)serializer.ReadObject(reader);
            }
            return cart;
        }

        #endregion

        #region Notes

        private string BuildNotesFilename => $"{PPCConfigurationManager.BackupPath}{NotesFilename}";

        public string GetNotes()
        {
            string filename = BuildNotesFilename;
            if (File.Exists(filename))
                return File.ReadAllText(filename, Encoding.UTF8);
            return null;
        }

        public void SaveNotes(string notes)
        {
            if (!Directory.Exists(PPCConfigurationManager.BackupPath))
                Directory.CreateDirectory(PPCConfigurationManager.BackupPath);
            string filename = BuildNotesFilename;
            File.WriteAllText(filename, notes, Encoding.UTF8);
        }

        #endregion

        #region Session

        public bool HasActiveSession()
        {
            return Directory.EnumerateFiles(PPCConfigurationManager.BackupPath).Any();
        }

        public void CreateActiveSession()
        {
            // NOP
        }

        public Session GetActiveSession()
        {
            Session session = new Session
            {
                Guid = Guid.NewGuid(),
                CreationTime = DateTime.Now,
                LastReloadTime = DateTime.Now,
            };

            session.ClientCarts = GetClientCarts();
            session.Transactions = GetTransactions();

            return session;
        }

        public void CloseActiveSession()
        {
            string savePath = PPCConfigurationManager.BackupPath + $"{DateTime.Now:yyyy-MM-dd HH-mm-ss}\\";
            if (!Directory.Exists(savePath))
                Directory.CreateDirectory(savePath);

            // Move backup files into save folder
            string backupPath = PPCConfigurationManager.BackupPath;
            foreach (string file in Directory.EnumerateFiles(backupPath))
            {
                string saveFilename = savePath + Path.GetFileName(file);
                File.Move(file, saveFilename);
            }
        }

        #endregion
    }
}
