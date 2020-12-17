using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game
{
    class Mirror : Case
    {
        bool isReversed;
        public Mirror(bool backgroundIsColored, bool isReversed) : base(backgroundIsColored)
        {
            this.isReversed = isReversed;
            if (isReversed)
                character = '\\';
            else
                character = '/';

            foregroundColor = ConsoleColor.White;
        }

        public override Cardinal LaserHit(Cardinal c)
        {
            if (c == Cardinal.SUD)
                return isReversed ? Cardinal.OUEST : Cardinal.EST;
            else if (c == Cardinal.NORD)
                return isReversed ? Cardinal.EST : Cardinal.OUEST;
            else if (c == Cardinal.OUEST)
                return isReversed ? Cardinal.SUD : Cardinal.NORD;
            else
                return isReversed ? Cardinal.NORD : Cardinal.SUD;
        }

        public override bool CheckIfPassable()
        {
            return false;
        }

        public virtual void Reverse()
        {
            isReversed = !isReversed;
            if (isReversed)
                character = '\\';
            else
                character = '/';
        }
    }
}
