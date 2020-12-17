using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game
{
    internal static class ExtensionMethods
    {
        public static Cardinal Reverse(this Cardinal a)
        {
            if (a == Cardinal.NORD)
                return Cardinal.SUD;
            else if (a == Cardinal.SUD)
                return Cardinal.NORD;
            else if (a == Cardinal.OUEST)
                return Cardinal.EST;
            else if (a == Cardinal.EST)
                return Cardinal.OUEST;
            else return Cardinal.NULL;
        }
    }
}
