using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game
{
    class Wall : Case
    {
        public Wall() : base(false)
        {
            character = '#';
            foregroundColor = ConsoleColor.DarkGray;
        }

        public override Cardinal LaserHit(Cardinal c)
        {
            return Cardinal.NULL;
        }
        public override bool CheckIfPassable()
        {
            return false;
        }
    }
}