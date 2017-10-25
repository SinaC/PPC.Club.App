using System.Collections.Generic;
using PPC.Domain;

namespace PPC.IDataAccess
{
    public interface IPlayerDL
    {
        List<Player> Load(string path);
        void Save(string path, IEnumerable<Player> players);
    }
}
