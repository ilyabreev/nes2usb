using System;
using System.IO;
using System.Linq;

namespace Gamebox
{
    public class GamesViewModel
    {
        private string _romDirectory = @"C:\Users\BIA\Desktop\250_games_for_Dendy";
        private String[] _games;
        
        public String[] Games
        {
            get
            {
                return _games;
            }
        }

        public GamesViewModel()
        {
            var directory = new DirectoryInfo(_romDirectory);
            var files = directory.GetFiles();
            _games = files.Select(f => f.Name).ToArray();
        }

        public string GetRomPath(int index)
        {
            return String.Format("{0}\\{1}", _romDirectory, _games[index]);
        }
    }
}
