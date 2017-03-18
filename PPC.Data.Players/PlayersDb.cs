using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;
using PPC.Data.Contracts;

namespace PPC.Data.Players
{
    public class PlayersDb : IPlayersDb
    {
        public List<Player> Load(string path)
        {
            global::LocalPlayers localPlayers;
            using (StreamReader sr = new StreamReader(path))
            {
                XmlSerializer serializer = new XmlSerializer(typeof(global::LocalPlayers));
                localPlayers = (global::LocalPlayers) serializer.Deserialize(sr);
            }

            return localPlayers.Items.Select(x => new Player
            {
                DCINumber = x.DciNumber,
                FirstName = x.FirstName,
                MiddleName = x.MiddleInitial,
                LastName = x.LastName,
                CountryCode = x.CountryCode,
                IsJudge = Convert.ToBoolean(x.IsJudge)
            }).ToList();
        }

        public void Save(string path, IEnumerable<Player> players)
        {
            global::LocalPlayers localPlayers = new global::LocalPlayers
            {
                Items = players.Select(x => new global::LocalPlayersPlayer
                {
                    DciNumber = x.DCINumber,
                    FirstName = x.FirstName,
                    MiddleInitial = x.MiddleName,
                    LastName = x.LastName,
                    CountryCode = x.CountryCode,
                    IsJudge = Convert.ToString(x.IsJudge)
                }).ToArray()
            };

            if (File.Exists(path))
            {
                string backupPath = path.Replace(".xml", $"_{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.xml");
                File.Copy(path, backupPath);
            }

            using (StreamWriter sw = new StreamWriter(path))
            {
                var settings = new XmlWriterSettings
                {
                    Indent = true,
                    OmitXmlDeclaration = true
                };

                using (XmlWriter xw = XmlWriter.Create(sw, settings))
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(global::LocalPlayers));
                    var emptyNamespaces = new XmlSerializerNamespaces(new[] {XmlQualifiedName.Empty});
                    serializer.Serialize(xw, localPlayers, emptyNamespaces);
                }
            }
        }
    }
}
