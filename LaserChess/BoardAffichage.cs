using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game
{
    partial class Board
    {

        void Draw()
        {
            ExtendedConsole.SetActiveLayer(0);
            for (int i = 0; i < cases.GetLength(0); i++)
                for (int j = 0; j < cases.GetLength(1); j++)
                {
                    Console.BackgroundColor = cases[i, j].backgroundColor;
                    Console.ForegroundColor = cases[i, j].foregroundColor;
                    ExtendedConsole.VirtualWrite(cases[i, j].character, i, j + 2);
                }

            Console.BackgroundColor = cases[redKing.pos.x, redKing.pos.y].backgroundColor;
            Console.ForegroundColor = Settings.PlayerOneColor;
            ExtendedConsole.VirtualWrite('K', redKing.pos.x, redKing.pos.y + 2);

            Console.BackgroundColor = cases[blueKing.pos.x, blueKing.pos.y].backgroundColor;
            Console.ForegroundColor = Settings.PlayerTwoColor;
            ExtendedConsole.VirtualWrite('K', blueKing.pos.x, blueKing.pos.y + 2);

            DrawMessageBox();

            ExtendedConsole.Update();

        }

        void DrawMessageBox()
        {
            ExtendedConsole.SetActiveLayer(0);
            Console.BackgroundColor = ConsoleColor.Black;
            Console.ForegroundColor = ConsoleColor.DarkGray;
            for (int i = 0; i < 49; i++)
            {
                ExtendedConsole.VirtualWrite('#', i, 28);
                ExtendedConsole.VirtualWrite('#', i, 0);
            }
        }

        public void UpdatePos(Coord c)
        {
            ExtendedConsole.SetActiveLayer(0);
            Console.BackgroundColor = cases[c.x, c.y].backgroundColor;
            Console.ForegroundColor = cases[c.x, c.y].foregroundColor;
            if (blueKing.pos == c)
            {
                Console.ForegroundColor = Settings.PlayerTwoColor;
                ExtendedConsole.VirtualWrite('K', c.x, c.y + 2);
            }
            else if (redKing.pos == c)
            {
                Console.ForegroundColor = Settings.PlayerOneColor;
                ExtendedConsole.VirtualWrite('K', c.x, c.y + 2);
            }
            else
                ExtendedConsole.VirtualWrite(cases[c.x, c.y].character, c.x, c.y + 2);

            ExtendedConsole.Update(c.x, c.y + 2);
        }

        public void UpdateMirrorPointer(Coord c, bool reversedMirror)
        {
            ExtendedConsole.SetActiveLayer(1);
            if (redKing.pos == c || blueKing.pos == c || !cases[c.x, c.y].CheckIfPassable())
            {
                Console.ForegroundColor = ConsoleColor.Red;
                ExtendedConsole.VirtualWrite('X', c.x, c.y + 2);
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.White;
                if (reversedMirror)
                    ExtendedConsole.VirtualWrite("\\", c.x, c.y + 2);
                else
                    ExtendedConsole.VirtualWrite("/", c.x, c.y + 2);
            }

            DrawRuler(c);

            ExtendedConsole.Update(c.x, c.y + 2);
        }

        public void ErasePointer(Coord c)
        {
            ExtendedConsole.SetActiveLayer(1);
            ExtendedConsole.VirtualLayerReset(1);
            ExtendedConsole.Update(0, c.y + 2, width, 1);
            ExtendedConsole.Update(c.x, 2, 1, height);
        }

        public void ShowPossibleDirections(King k, List<int> possibilities)
        {
            ExtendedConsole.SetActiveLayer(1);
            foreach (int i in possibilities)
            {
                char c = ' ';
                int x = 0;
                int y = 0;
                switch (i)
                {
                    case 1: c = '1'; ; x = k.pos.x - 1; y = k.pos.y + 1; break;
                    case 2: c = '2'; ; x = k.pos.x; y = k.pos.y + 1; break;
                    case 3: c = '3'; x = k.pos.x + 1; y = k.pos.y + 1; break;
                    case 4: c = '4'; x = k.pos.x - 1; y = k.pos.y; break;
                    case 5: c = 'K'; x = k.pos.x; y = k.pos.y; break;
                    case 6: c = '6'; x = k.pos.x + 1; y = k.pos.y; break;
                    case 7: c = '7'; x = k.pos.x - 1; y = k.pos.y - 1; break;
                    case 8: c = '8'; x = k.pos.x; y = k.pos.y - 1; break;
                    case 9: c = '9'; x = k.pos.x + 1; y = k.pos.y - 1; break;
                }

                if (x < 0)
                    x = width - 1;
                else if (x > width - 1)
                    x = 0;

                if (y < 0)
                    y = height - 1;
                else if (y > height - 1)
                    y = 0;

                ExtendedConsole.VirtualWrite(c, x, y + 2);
                ExtendedConsole.Update(x, y + 2);
            }
        }

        void DrawRuler(Coord c)
        {
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            for (int i = 0; i < 49; i++)
            {
                if ((i != c.x) && !(cases[i, c.y] is Wall) && !(cases[i, c.y] is Mirror) && (blueKing.pos != new Coord(i, c.y)) && (redKing.pos != new Coord(i, c.y)))
                {
                    Console.BackgroundColor = cases[i, c.y].backgroundColor;
                    ExtendedConsole.VirtualWrite('─', i, c.y + 2);
                }
            }
            ExtendedConsole.Update(0, c.y + 2, 49, 1);

            for (int i = 0; i < 24; i++)
            {
                if ((i != c.y) && !(cases[c.x, i] is Wall) && !(cases[c.x, i] is Mirror) && (blueKing.pos != new Coord(c.x, i)) && (redKing.pos != new Coord(c.x, i)))
                {
                    Console.BackgroundColor = cases[c.x, i].backgroundColor;
                    ExtendedConsole.VirtualWrite('│', c.x, i + 2);
                }
            }
            ExtendedConsole.Update(c.x, 2, 1, 24);
        }

        public void HidePossibleDirections(King k)
        {
            for (int i = k.pos.x - 1; i <= k.pos.x + 1; i++)
                for (int j = k.pos.y - 1; j <= k.pos.y + 1; j++)
                {
                    int x = i;
                    int y = j;

                    if (x < 0)
                        x = width - 1;
                    else if (x > width - 1)
                        x = 0;

                    if (y < 0)
                        y = height - 1;
                    else if (y > height - 1)
                        y = 0;

                    ExtendedConsole.VirtualErase(x, y + 2);
                    ExtendedConsole.Update(x, y + 2);
                }
            ExtendedConsole.SetActiveLayer(1);
        }
    }
}
