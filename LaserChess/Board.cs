using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game
{
    partial class Board
    {
        public static int width { get; private set; }
        public static int height { get; private set; }

        Case[,] cases;
        public King redKing;
        public King blueKing;

        public Board()
        {
            ExtendedConsole.VirtualClear();
            cases = new Case[49, 25];
            width = 49;
            height = 25;
            redKing.hitPoint = Settings.HitPoint;
            blueKing.hitPoint = Settings.HitPoint;
        }

        internal void LoadBoard(List<string> lines)
        {
            blueKing.hasBeenPlaced = false;
            redKing.hasBeenPlaced = false;

            for (int i = 0; i < lines.Count; i++)
            {
                for (int j = 0; j < lines[i].Length; j++)
                {
                    if (lines[i][j] == '#')
                        cases[j, i] = new Wall();
                    else if (lines[i][j] == '/')
                        cases[j, i] = new FixedMirror(false);
                    else if (lines[i][j] == '\\')
                        cases[j, i] = new FixedMirror(true);
                    else
                    {
                        if (lines[i][j] == 'K')
                        {
                            if (!redKing.hasBeenPlaced)
                            {
                                redKing.MoveTo(j, i);
                                redKing.hasBeenPlaced = true;
                            }
                            else if (!blueKing.hasBeenPlaced)
                            {
                                blueKing.MoveTo(j, i);
                                blueKing.hasBeenPlaced = true;
                            }
                        }
                        cases[j, i] = new Case((i % 2 + j % 2) % 2 == 0);
                    }
                }
            }

            if (!redKing.hasBeenPlaced)
                throw new KingPositionException(redKing, "ERROR: No Starting position for Red King found");
            else if (!blueKing.hasBeenPlaced)
                throw new KingPositionException(blueKing, "ERROR: No Starting position for Blue King found");

            //DelimitateBoard();
            Draw();
        }


        public void ShootLaser(King king, ConsoleColor color, Cardinal directionLaser)
        {
            int laserDamage = 1;
            Console.ForegroundColor = color;
            Console.BackgroundColor = ConsoleColor.Black;
            Laser laser = new Laser(color);
            Coord nextLaserPos = king.pos + directionLaser;

            List<Coord> mirrorsHit = new List<Coord>();

            while (directionLaser != Cardinal.NULL)
            {
                if (nextLaserPos.x < 0)
                    nextLaserPos.x = 48;
                else if (nextLaserPos.x > 48)
                    nextLaserPos.x = 0;

                if (nextLaserPos.y < 0)
                    nextLaserPos.y = 24;
                else if (nextLaserPos.y > 24)
                    nextLaserPos.y = 0;


                Cardinal newDirectionLaser = cases[nextLaserPos.x, nextLaserPos.y].LaserHit(directionLaser);

                if (cases[nextLaserPos.x, nextLaserPos.y] is Mirror)
                {
                    mirrorsHit.Add(nextLaserPos);
                    laserDamage++;
                }
                if (cases[nextLaserPos.x, nextLaserPos.y] is Wall)
                {
                    cases[nextLaserPos.x, nextLaserPos.y] = new Case((nextLaserPos.x % 2 + nextLaserPos.y % 2) % 2 == 0);
                    UpdatePos(nextLaserPos);

                    if (nextLaserPos.x == 0)
                    {
                        cases[48, nextLaserPos.y] = new Case(nextLaserPos.y % 2 == 0);
                        UpdatePos(new Coord(48, nextLaserPos.y));
                    }
                    else if (nextLaserPos.x == 48)
                    {
                        cases[0, nextLaserPos.y] = new Case(nextLaserPos.y % 2 == 0);
                        UpdatePos(new Coord(0, nextLaserPos.y));
                    }
                    if (nextLaserPos.y == 0)
                    {
                        cases[nextLaserPos.x, 24] = new Case(nextLaserPos.x % 2 == 0);
                        UpdatePos(new Coord(nextLaserPos.x, 24));
                    }
                    if (nextLaserPos.y == 24)
                    {
                        cases[nextLaserPos.x, 0] = new Case(nextLaserPos.x % 2 == 0);
                        UpdatePos(new Coord(nextLaserPos.x, 0));
                    }
                }

                laser.Add(nextLaserPos, directionLaser.Reverse(), newDirectionLaser);
                directionLaser = newDirectionLaser;
                if (redKing.pos.x == nextLaserPos.x && redKing.pos.y == nextLaserPos.y)
                {
                    directionLaser = Cardinal.NULL;
                    redKing.hitPoint -= laserDamage;
                }
                else if (blueKing.pos.x == nextLaserPos.x && blueKing.pos.y == nextLaserPos.y)
                {
                    directionLaser = Cardinal.NULL;
                    blueKing.hitPoint -= laserDamage;
                }

                nextLaserPos += directionLaser;
            }

            Console.ForegroundColor = color;
            laser.Draw();

            foreach (Coord c in mirrorsHit)
            {
                ((Mirror)cases[c.x, c.y]).Reverse(); ;
                UpdatePos(c);
            }

            Console.ForegroundColor = color;
            laser.Erase();
        }

        private void DelimitateBoard()
        {

            for (int i = 0; i < cases.GetLength(0); i++)
            {
                cases[i, 0] = new Wall();
                cases[i, cases.GetLength(1) - 1] = new Wall();
            }

            for (int j = 1; j < cases.GetLength(1) - 1; j++)
            {
                cases[0, j] = new Wall();
                cases[cases.GetLength(0) - 1, j] = new Wall();
            }
        }

        public bool getPassable(int x, int y)
        {
            if (y < 0)
                y = height - 1;
            if (y >= height)
                y = 0;

            if (x < 0)
                x = width - 1;
            if (x >= width)
                x = 0;

            if ((redKing.pos.x == x && redKing.pos.y == y) || (blueKing.pos.x == x && blueKing.pos.y == y))
                return false;
            return cases[x, y].CheckIfPassable();
        }

        public void PlaceMirror(int x, int y, bool reversed)
        {
            cases[x, y] = new Mirror((x % 2 + y % 2) % 2 == 0, reversed);
        }

        public void MoveKing(bool moveRedKing, Coord targetPos)
        {
            if (moveRedKing)
            {
                Coord oldPos = redKing.pos;
                redKing.pos = targetPos;
                UpdatePos(oldPos);
                UpdatePos(redKing.pos);
            }
            else
            {
                Coord oldPos = blueKing.pos;
                blueKing.pos = targetPos;
                UpdatePos(oldPos);
                UpdatePos(blueKing.pos);
            }
        }

        public byte[] BoardAsBytes()
        {
            byte[] toReturn = new byte[1231];

            for (int i = 0; i < cases.GetLength(0); i++)
                for (int j = 0; j < cases.GetLength(1); j++)
                    toReturn[j * cases.GetLength(0) + i] = (byte)cases[i, j].character;

            toReturn[1225] = (byte)redKing.pos.x;
            toReturn[1226] = (byte)redKing.pos.y;
            toReturn[1227] = (byte)blueKing.pos.x;
            toReturn[1228] = (byte)blueKing.pos.y;
            toReturn[1229] = (byte)redKing.hitPoint;
            toReturn[1230] = (byte)blueKing.hitPoint;

            return toReturn;
        }

        public void LoadFromBytes(byte[] b)
        {
            if (b.Length < 1231)
            {
                throw new BoardBytesSizeException(b, "ERREUR: La nombre de bytes recu (" + b.Length + ") est insuffisant pour generer la plateau.");
            }
            for (int i = 0; i < cases.GetLength(0); i++)
                for (int j = 0; j < cases.GetLength(1); j++)
                {
                    char current = (char)b[j * cases.GetLength(0) + i];
                    if (current == '#')
                        cases[i, j] = new Wall();
                    else if (current == '/')
                        cases[i, j] = new FixedMirror(false);
                    else if (current == '\\')
                        cases[i, j] = new FixedMirror(true);
                    else

                        cases[i, j] = new Case((i % 2 + j % 2) % 2 == 0);
                }
            redKing.MoveTo(b[1225], b[1226]);
            blueKing.MoveTo(b[1227], b[1228]);
            redKing.hitPoint = b[1229];
            blueKing.hitPoint = b[1230];

            Draw();
        }
    }
}
