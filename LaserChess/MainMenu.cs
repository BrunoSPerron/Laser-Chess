using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game
{
    class MainMenu
    {
        public MainMenu()
        {
            Start();
        }

        private void Start()
        {
            ExtendedConsole.AnimatedMenuBoxOpening(0, 0, 49, 29);
            GameController.nextScreen = ExtendedConsole.ShowMenuAndGetChoice(new string[] { "New game", "Online", "Settings", "Quit" }, 30, 18);
        }
    }
}