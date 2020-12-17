using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Game;

namespace LaserChess
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.Title = "Laser Chess";
            ExtendedConsole.SetConsoleSize(49, 29);

            Settings.Load();

            new GameController();
        }
    }
}