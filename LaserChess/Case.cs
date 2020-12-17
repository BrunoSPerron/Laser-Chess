using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game
{
    class Case
    {
        public ConsoleColor foregroundColor { get; protected set; }
        public ConsoleColor backgroundColor { get; protected set; }
        public char character { get; protected set; }

        public Case(bool backgroundIsColored = false)
        {
            character = ' ';

            foregroundColor = ConsoleColor.Gray;

            if (backgroundIsColored)
                backgroundColor = ConsoleColor.DarkBlue;
            else
                backgroundColor = ConsoleColor.Black;
        }

        public virtual Cardinal LaserHit(Cardinal c)
        {
            return c;
        }

        public virtual bool CheckIfPassable()
        {
            return true;
        }
    }
}
