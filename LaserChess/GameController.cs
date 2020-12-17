using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game
{
    class GameController
    {
        static public int nextScreen { get; set; } = -1;
        bool gameIsOver = false;

        public GameController()
        {
            while (!gameIsOver)
                Navigate();
        }

        private void Navigate()
        {
            switch (nextScreen)
            {
                case 0:
                    nextScreen = -1;
                    new Game();
                    break;
                case 1:
                    nextScreen = -1;
                    new ConnectionScreen();
                    break;
                case 2:
                    nextScreen = -1;
                    new Settings();
                    break;
                case 3:
                    gameIsOver = true;
                    break;
                default:
                    new MainMenu();
                    break;
            }
        }
    }
}
