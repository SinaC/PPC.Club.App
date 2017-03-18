using System.Collections.Generic;
using PPC.Data.Contracts;

namespace PPC.Data.Players
{
    public interface IPlayersDb
    {
        List<Player> Load(string path);
        void Save(string path, IEnumerable<Player> players);
    }
}
