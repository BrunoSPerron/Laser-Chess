using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game
{
    struct King
    {
        public Coord pos;
        public bool hasBeenPlaced;
        public int hitPoint;
        public King(int hitPoint = 1, bool hasBeenPlaced = false)
        {
            pos = new Coord();
            this.hasBeenPlaced = false;
            this.hitPoint = hitPoint;
        }

        public void MoveTo(int x, int y)
        {
            pos.x = x;
            pos.y = y;
        }
    }

    struct Coord
    {
        public int x;
        public int y;
        public Coord(int x, int y)
        {
            this.x = x;
            this.y = y;
        }
        
        public static bool operator ==(Coord a, Coord b)
        {
            if (a.x == b.x && a.y == b.y)
                return true;
            else
                return false;
        }
        public static bool operator !=(Coord a, Coord b)
        {
            if (a.x == b.x && a.y == b.y)
                return false;
            else
                return true;
        }

        public static Coord operator +(Coord a, Cardinal b)
        {
            if (b == Cardinal.NORD)
                return new Coord(a.x, a.y - 1);
            else if (b == Cardinal.SUD)
                return new Coord(a.x, a.y + 1);
            else if (b == Cardinal.EST)
                return new Coord(a.x - 1, a.y);
            else if (b == Cardinal.OUEST)
                return new Coord(a.x + 1, a.y);
            else
                return a;
        }
    }
}
