using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game
{
    class Laser
    {
        List<Coord> path;
        List<Char> chars;

        ConsoleColor color;

        public Laser(ConsoleColor color)
        {
            this.color = color;
            path = new List<Coord>();
            chars = new List<char>();
        }

        public void Add(Coord c, Cardinal origin, Cardinal destination)
        {

            if (path.Contains(c))
                chars.Add('┼');
            else if ((origin == Cardinal.SUD && destination == Cardinal.NORD) || (origin == Cardinal.NORD && destination == Cardinal.SUD))
                chars.Add('│');
            else if ((origin == Cardinal.EST && destination == Cardinal.OUEST) || (origin == Cardinal.OUEST && destination == Cardinal.EST))
                chars.Add('─');
            else if ((origin == Cardinal.NORD && destination == Cardinal.EST) || (origin == Cardinal.EST && destination == Cardinal.NORD))
                chars.Add('┘');
            else if ((origin == Cardinal.NORD && destination == Cardinal.OUEST) || (origin == Cardinal.OUEST && destination == Cardinal.NORD))
                chars.Add('└');
            else if ((origin == Cardinal.SUD && destination == Cardinal.OUEST) || (origin == Cardinal.OUEST && destination == Cardinal.SUD))
                chars.Add('┌');
            else if ((origin == Cardinal.SUD && destination == Cardinal.EST) || (origin == Cardinal.EST && destination == Cardinal.SUD))
                chars.Add('┐');

            if (destination != Cardinal.NULL)
                path.Add(c);
        }

        public void Draw()
        {
            ExtendedConsole.SetActiveLayer(1);
            Console.ForegroundColor = color;
            for (int i = 0; i < path.Count; i++)
            {
                ExtendedConsole.VirtualWrite(chars[i], path[i].x, path[i].y + 2);
                ExtendedConsole.Update(path[i].x, path[i].y + 2, 1, 1);
                System.Threading.Thread.Sleep(Settings.LaserTickTime);
            }
        }

        public void Erase()
        {
            ExtendedConsole.SetActiveLayer(1);
            while (path.Count > 0)
            {
                ExtendedConsole.VirtualErase(path[0].x, path[0].y + 2, 1, 1);
                ExtendedConsole.Update(path[0].x, path[0].y + 2, 1, 1);
                Coord deletedCoord = path[0];
                path.RemoveAt(0);
                chars.RemoveAt(0);
                for (int i = 0; i < path.Count; i++)
                    if (path[i].x == deletedCoord.x && path[i].y == deletedCoord.y)
                    {
                        ExtendedConsole.VirtualWrite(chars[i], path[i].x, path[i].y + 2);

                    }
                System.Threading.Thread.Sleep(Settings.LaserTickTime);
            }
        }
    }
}
