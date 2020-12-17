using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Game
{
    class Game
    {
        bool isAnOnlineGame;
        bool isHost;
        TcpClient client;

        Board board;

        public Game()
        {
            isAnOnlineGame = false;
            Start();
        }
        public Game(TcpClient tcp, bool isHost)
        {
            client = tcp;
            this.isHost = isHost;
            isAnOnlineGame = true;
            Start();
        }

        void Start()
        {
            board = new Board();
            if (isAnOnlineGame)
            {
                if (isHost)
                {
                    SelectBoard(); 
                    byte[] data = board.BoardAsBytes();
                    NetworkStream stream = client.GetStream();
                    stream.Write(data, 0, data.Length);
                }
                else
                {
                    Byte[] data = new Byte[1231];
                    NetworkStream stream = client.GetStream();
                    stream.Read(data, 0, data.Length);
                    board.LoadFromBytes(data);
                }
            }
            else
            {
                SelectBoard();
            }

            ShowHP();
            GameLoop();
        }

        public void SelectBoard()
        {
            string[] files = Directory.GetFiles(@"Boards", "*.txt", SearchOption.AllDirectories); Random rand = new Random();
            int currentBoard = rand.Next(0, files.Length);

            List<string> lines = new List<string>();


            bool boardChoosen = false;
            while (!boardChoosen)
            {
                string currentBoardName = files[currentBoard];
                IEnumerable<string> linesReader = File.ReadLines(currentBoardName, Encoding.UTF8);

                lines = new List<string>();
                foreach (string s in linesReader)
                {
                    string toAdd = s;
                    while (toAdd.Length < Board.width)
                        toAdd += " ";
                    lines.Add(toAdd.Substring(0, Board.width));
                }

                while (lines.Count < Board.height)
                {
                    string toAdd = "";
                    while (toAdd.Length < Board.width)
                        toAdd += " ";
                    lines.Add(toAdd);
                }

                board.LoadBoard(lines);

                if (Settings.BoardSelection != 0)
                {
                    ShowMessage("<< " + currentBoardName.Split('\\')[1] + " >>");
                    ConsoleKey input = Console.ReadKey(true).Key;

                    switch (input)
                    {
                        case ConsoleKey.LeftArrow:
                        case ConsoleKey.NumPad4:
                            currentBoard--;
                            if (currentBoard < 0)
                                currentBoard = files.Length - 1;
                            break;
                        case ConsoleKey.RightArrow:
                        case ConsoleKey.NumPad6:
                            currentBoard++;
                            if (currentBoard >= files.Length)
                                currentBoard = 0;
                            break;
                        case ConsoleKey.Enter:
                        case ConsoleKey.NumPad5:
                        case ConsoleKey.Spacebar:
                            boardChoosen = true;
                            break;
                    }
                }
                else
                    boardChoosen = true;
            }
        }

        void GameLoop()
        {
            bool isRedTurn = false;
            Random rand = new Random();
            if (isAnOnlineGame)
            {
                if (isHost)
                {
                    byte[] data = new byte[1];
                    if (rand.Next(0, 2) < 1)
                    {
                        isRedTurn = true;
                        data[0] = 1;
                    }
                    else
                    {
                        data[0] = 0;
                    }
                    NetworkStream stream = client.GetStream();
                    stream.Write(data, 0, data.Length);
                }
                else
                {
                    Byte[] data = new Byte[1];
                    NetworkStream stream = client.GetStream();
                    stream.Read(data, 0, data.Length);
                    if (data[0] == 1)
                        isRedTurn = true;
                }
            }
            else
            {
                if (rand.Next(0, 2) < 1)
                    isRedTurn = true;
            }

            bool gameIsOver = false;
            while (!gameIsOver)
            {
                gameIsOver = NextTurn(isRedTurn);
                isRedTurn = !isRedTurn;
            }
        }

        bool NextTurn(bool isRedTurn)
        {
            bool toReturn = false;
            King king;
            ConsoleColor color;
            ConsoleColor selectionColor;
            if (isRedTurn)
            {
                color = Settings.PlayerOneColor;
                selectionColor = Settings.PlayerOneSelectionColor;
                king = board.redKing;
            }
            else
            {
                color = Settings.PlayerTwoColor;
                selectionColor = Settings.PlayerTwoSelectionColor;
                king = board.blueKing;
            }

            while (Console.KeyAvailable)
                Console.ReadKey(true);

            //Movement
            ShowMessage("Movement", color);
            Console.ForegroundColor = color;
            Coord targetPosition = new Coord();
            if (isAnOnlineGame)
            {
                if ((isRedTurn && isHost) || (!isRedTurn && !isHost))
                {
                    targetPosition = MovementChoice(king);
                    byte[] data = new byte[2];
                    data[0] = (byte)targetPosition.x;
                    data[1] = (byte)targetPosition.y;
                    NetworkStream stream = client.GetStream();
                    stream.Write(data, 0, data.Length);
                }
                else
                {
                    Byte[] data = new Byte[2];
                    NetworkStream stream = client.GetStream();
                    stream.Read(data, 0, data.Length);
                    targetPosition.x = data[0];
                    targetPosition.y = data[1];
                }
            }
            else
            {
                targetPosition = MovementChoice(king);
            }
            if (targetPosition.x < 0)
                targetPosition.x = Board.width - 1;
            else if (targetPosition.x >= Board.width)
                targetPosition.x = 0;

            if (targetPosition.y < 0)
                targetPosition.y = Board.height - 1;
            else if (targetPosition.y >= Board.height)
                targetPosition.y = 0;

            board.MoveKing(isRedTurn, targetPosition);
            king.pos = targetPosition;

            //Mirror placement
            ShowMessage("Summoning", color);
            int[] mirrorToPlace = new int[3];

            mirrorToPlace = PositionMirror(king.pos, selectionColor, isRedTurn);

            board.PlaceMirror(mirrorToPlace[0], mirrorToPlace[1], mirrorToPlace[2] == 1);
            board.UpdatePos(new Coord(mirrorToPlace[0], mirrorToPlace[1]));

            //Laser
            ShowMessage("Shooting", color);
            Console.ForegroundColor = color;
            Console.BackgroundColor = ConsoleColor.Black;
            Cardinal directionLaser = Cardinal.NULL;
            if (isAnOnlineGame)
            {
                if ((isRedTurn && isHost) || (!isRedTurn && !isHost))
                {
                    directionLaser = DirectionLaserChoice(king);
                    byte[] data = new byte[1];
                    data[0] = (byte)directionLaser;
                    NetworkStream stream = client.GetStream();
                    stream.Write(data, 0, data.Length);
                }
                else
                {
                    Byte[] data = new Byte[1];
                    NetworkStream stream = client.GetStream();
                    stream.Read(data, 0, data.Length);
                    directionLaser = (Cardinal)data[0];
                }
            }
            else
            {
                directionLaser = DirectionLaserChoice(king);
            }

            board.ShootLaser(king, color, directionLaser);
            ShowHP();
            if (board.redKing.hitPoint < 1)
            {
                Popup(Settings.PlayerTwoName + " Win");
                toReturn = true;
            }
            else if (board.blueKing.hitPoint < 1)
            {
                Popup(Settings.PlayerOneName + " Win");
                toReturn = true;
            }
            return toReturn;
        }

        void Popup(string message)
        {
            ExtendedConsole.SetActiveLayer(2);
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.BackgroundColor = ConsoleColor.Black;
            ExtendedConsole.VirtualFill(25 - message.Length / 2 - 3, 12, message.Length + 6, 5);
            ExtendedConsole.VirtualDrawBox(25 - message.Length / 2 - 3, 12, message.Length + 6, 5);
            ExtendedConsole.VirtualWrite(message, 25 - message.Length / 2, 14);
            ExtendedConsole.Update();
            Console.ReadKey(true);
            ExtendedConsole.VirtualLayerReset(2);
            ExtendedConsole.Update();
        }
        void ShowMessage(string message, ConsoleColor color = ConsoleColor.Gray)
        {
            ExtendedConsole.SetActiveLayer(2);
            ExtendedConsole.VirtualLayerReset(2);
            Console.ForegroundColor = color;
            Console.BackgroundColor = ConsoleColor.Black;
            ExtendedConsole.VirtualWrite(message, 25 - message.Length / 2, 27);
            ExtendedConsole.Update(0, 27, 49, 1);
        }

        void ShowHP()
        {
            Console.BackgroundColor = ConsoleColor.Black;
            Console.ForegroundColor = Settings.PlayerOneColor;
            ExtendedConsole.VirtualWrite("Hit: " + board.redKing.hitPoint, 1, 1);
            Console.ForegroundColor = Settings.PlayerTwoColor;
            if (board.blueKing.hitPoint < 0)
                ExtendedConsole.VirtualWrite("Hit: " + board.blueKing.hitPoint, Board.width - (int)Math.Floor(Math.Log10(Math.Abs(board.blueKing.hitPoint) + 1)) - 8, 1);
            else
                ExtendedConsole.VirtualWrite("Hit: " + board.blueKing.hitPoint, Board.width - (int)Math.Floor(Math.Log10(Math.Abs(board.blueKing.hitPoint) + 1)) - 7, 1);
            ExtendedConsole.Update(0, 1, Board.width, 1);
        }

        Coord MovementChoice(King k)
        {
            Coord toReturn = new Coord(0, 0);

            List<int> possibleMovements = new List<int>();
            if (board.getPassable(k.pos.x - 1, k.pos.y + 1)) possibleMovements.Add(1);
            if (board.getPassable(k.pos.x, k.pos.y + 1)) possibleMovements.Add(2);
            if (board.getPassable(k.pos.x + 1, k.pos.y + 1)) possibleMovements.Add(3);
            if (board.getPassable(k.pos.x - 1, k.pos.y)) possibleMovements.Add(4);
            possibleMovements.Add(5);
            if (board.getPassable(k.pos.x + 1, k.pos.y)) possibleMovements.Add(6);
            if (board.getPassable(k.pos.x - 1, k.pos.y - 1)) possibleMovements.Add(7);
            if (board.getPassable(k.pos.x, k.pos.y - 1)) possibleMovements.Add(8);
            if (board.getPassable(k.pos.x + 1, k.pos.y - 1)) possibleMovements.Add(9);
            int flickering = 10;

            bool choiceIsMade = false;
            while (!choiceIsMade)
            {
                if (flickering > 5)
                    board.ShowPossibleDirections(k, possibleMovements);
                else
                    board.HidePossibleDirections(k);
                flickering--;
                if (flickering == 0)
                    flickering = 10;

                ConsoleKeyInfo choice = new ConsoleKeyInfo();
                while (Console.KeyAvailable)
                    choice = Console.ReadKey(true);

                if (possibleMovements.Contains((int)Char.GetNumericValue(choice.KeyChar)))
                    switch (choice.Key)
                    {
                        case ConsoleKey.NumPad1: toReturn.x = k.pos.x - 1; toReturn.y = k.pos.y + 1; choiceIsMade = true; break;
                        case ConsoleKey.NumPad2: toReturn.x = k.pos.x; toReturn.y = k.pos.y + 1; choiceIsMade = true; break;
                        case ConsoleKey.NumPad3: toReturn.x = k.pos.x + 1; toReturn.y = k.pos.y + 1; choiceIsMade = true; break;
                        case ConsoleKey.NumPad4: toReturn.x = k.pos.x - 1; toReturn.y = k.pos.y; choiceIsMade = true; break;
                        case ConsoleKey.NumPad5: toReturn.x = k.pos.x; toReturn.y = k.pos.y; choiceIsMade = true; break;
                        case ConsoleKey.NumPad6: toReturn.x = k.pos.x + 1; toReturn.y = k.pos.y; choiceIsMade = true; break;
                        case ConsoleKey.NumPad7: toReturn.x = k.pos.x - 1; toReturn.y = k.pos.y - 1; choiceIsMade = true; break;
                        case ConsoleKey.NumPad8: toReturn.x = k.pos.x; toReturn.y = k.pos.y - 1; choiceIsMade = true; break;
                        case ConsoleKey.NumPad9: toReturn.x = k.pos.x + 1; toReturn.y = k.pos.y - 1; choiceIsMade = true; break;
                    }

                System.Threading.Thread.Sleep(100);
            }

            board.HidePossibleDirections(k);
            return toReturn;
        }

        Cardinal DirectionLaserChoice(King k)
        {
            Cardinal toReturn = Cardinal.NULL;

            List<int> possibleDirections = new List<int>();
            possibleDirections.Add(2);
            possibleDirections.Add(4);
            possibleDirections.Add(6);
            possibleDirections.Add(8);
            int flickering = 10;

            bool choiceIsMade = false;
            while (!choiceIsMade)
            {
                if (flickering > 5)
                    board.ShowPossibleDirections(k, possibleDirections);
                else
                    board.HidePossibleDirections(k);
                flickering--;
                if (flickering == 0)
                    flickering = 10;

                ConsoleKeyInfo choice = new ConsoleKeyInfo();
                while (Console.KeyAvailable)
                    choice = Console.ReadKey(true);

                if (possibleDirections.Contains((int)Char.GetNumericValue(choice.KeyChar)))
                    switch (choice.Key)
                    {
                        case ConsoleKey.NumPad2: toReturn = Cardinal.SUD; choiceIsMade = true; break;
                        case ConsoleKey.NumPad4: toReturn = Cardinal.EST; choiceIsMade = true; break;
                        case ConsoleKey.NumPad6: toReturn = Cardinal.OUEST; choiceIsMade = true; break;
                        case ConsoleKey.NumPad8: toReturn = Cardinal.NORD; choiceIsMade = true; break;
                    }

                System.Threading.Thread.Sleep(100);
            }

            board.HidePossibleDirections(k);
            return toReturn;
        }

        int[] PositionMirror(Coord c, ConsoleColor selectionColor, bool isRedTurn = false)
        {
            Coord currentPos = c;
            int[] toReturn = new int[3];
            bool mirrorIsReversed = false;

            bool positionChoosen = false;
            while (!positionChoosen)
            {
                ConsoleKey input = new ConsoleKey();
                Console.BackgroundColor = selectionColor;
                board.UpdateMirrorPointer(currentPos, mirrorIsReversed);
                Coord oldPosition = currentPos;
                if (isAnOnlineGame)
                {
                    if ((isRedTurn && isHost) || (!isRedTurn && !isHost))
                    {
                        input = Console.ReadKey(true).Key;

                        byte[] data = new byte[1];
                        data[0] = (byte)input;
                        NetworkStream stream = client.GetStream();
                        stream.Write(data, 0, data.Length);
                    }
                    else
                    {
                        Byte[] data = new Byte[1];
                        NetworkStream stream = client.GetStream();
                        stream.Read(data, 0, data.Length);
                        input = (ConsoleKey)data[0];
                    }
                }
                else
                {
                    input = Console.ReadKey(true).Key;
                }

                switch (input)
                {
                    case ConsoleKey.NumPad1:
                        if (currentPos.x > 1 && currentPos.y < 23)
                        {
                            currentPos.x -= 1;
                            currentPos.y += 1;
                        }
                        break;
                    case ConsoleKey.NumPad2:
                    case ConsoleKey.DownArrow:
                        if (currentPos.y < 23)
                        {
                            currentPos.y += 1;
                        }
                        break;
                    case ConsoleKey.NumPad3:
                        if (currentPos.x < 47 && currentPos.y < 23)
                        {
                            currentPos.x += 1;
                            currentPos.y += 1;
                        }
                        break;
                    case ConsoleKey.NumPad4:
                    case ConsoleKey.LeftArrow:
                        if (currentPos.x > 1)
                        {
                            currentPos.x -= 1;
                        }
                        break;
                    case ConsoleKey.NumPad6:
                    case ConsoleKey.RightArrow:
                        if (currentPos.x < 47)
                        {
                            currentPos.x += 1;
                        }
                        break;
                    case ConsoleKey.NumPad7:
                        if (currentPos.x > 1 && currentPos.y > 1)
                        {
                            currentPos.x -= 1;
                            currentPos.y -= 1;
                        }
                        break;
                    case ConsoleKey.NumPad8:
                    case ConsoleKey.UpArrow:
                        if (currentPos.y > 1)
                        {
                            currentPos.y -= 1;
                        }
                        break;
                    case ConsoleKey.NumPad9:
                        if (currentPos.x < 47 && currentPos.y > 1)
                        {
                            currentPos.x += 1;
                            currentPos.y -= 1;
                        }
                        break;
                    case ConsoleKey.Tab:
                    case ConsoleKey.Divide:
                        mirrorIsReversed = !mirrorIsReversed;
                        break;
                    //case ConsoleKey.NumPad5:
                    case ConsoleKey.Enter:
                    case ConsoleKey.Spacebar:
                        if (board.getPassable(currentPos.x, currentPos.y))
                        {
                            toReturn[0] = currentPos.x;
                            toReturn[1] = currentPos.y;
                            if (mirrorIsReversed)
                                toReturn[2] = 1;
                            else
                                toReturn[2] = 0;
                            positionChoosen = true;
                        }
                        break;
                }
                board.ErasePointer(oldPosition);
            }

            return toReturn;
        }
    }
}
