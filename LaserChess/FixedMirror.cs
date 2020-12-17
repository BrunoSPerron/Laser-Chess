using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game
{
    class FixedMirror : Mirror
    {
        public FixedMirror(bool isReversed) : base(false, isReversed)
        {
            backgroundColor = ConsoleColor.DarkGray;
            foregroundColor = ConsoleColor.Black;
        }

        public override void Reverse()
        {
        }
    }
}
