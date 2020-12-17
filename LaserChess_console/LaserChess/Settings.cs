using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Game
{
    class Settings
    {
        public static ConsoleColor PlayerOneColor { get; private set; } = ConsoleColor.Red;
        public static ConsoleColor PlayerOneSelectionColor { get; private set; } = ConsoleColor.DarkRed;
        public static ConsoleColor PlayerTwoColor { get; private set; } = ConsoleColor.Cyan;
        public static ConsoleColor PlayerTwoSelectionColor { get; private set; } = ConsoleColor.DarkCyan;
        public static string PlayerOneName { get; private set; } = "Red King";
        public static string PlayerTwoName { get; private set; } = "Blue King";

        public static int BoardSelection { get; private set; } = 0;
        public static int LaserTickTime { get; private set; } = 5;
        public static int ConnectionPort { get; private set; } = 13001;

        public static int HitPoint { get; private set; } = 5;

        public Settings()
        {
            Draw();
            SelectOption();
            SaveToXml();
            Console.BackgroundColor = ConsoleColor.Black;
        }

        private void Draw(int selected = 0)
        {
            Console.BackgroundColor = ConsoleColor.Black;
            ExtendedConsole.VirtualClear();
            ExtendedConsole.VirtualDrawBox(0, 0, 49, 29);
            ExtendedConsole.VirtualWrite("SETTINGS", 20, 2);

            if (selected == 0)
                Console.BackgroundColor = ConsoleColor.DarkGreen;
            else
                Console.BackgroundColor = ConsoleColor.Black;
            ExtendedConsole.VirtualWrite("Player One Name = " + PlayerOneName, 2, 4);

            if (selected == 1)
                Console.BackgroundColor = ConsoleColor.DarkGreen;
            else
                Console.BackgroundColor = ConsoleColor.Black;
            ExtendedConsole.VirtualWrite("Player One Color = ", 2, 5);
            Console.ForegroundColor = PlayerOneColor;
            ExtendedConsole.VirtualWrite("" + PlayerOneColor, 21, 5);
            Console.ForegroundColor = ConsoleColor.Gray;

            if (selected == 2)
                Console.BackgroundColor = ConsoleColor.DarkGreen;
            else
                Console.BackgroundColor = ConsoleColor.Black;
            ExtendedConsole.VirtualWrite("Player One Selection Color = ", 2, 6);
            Console.ForegroundColor = PlayerOneSelectionColor;
            ExtendedConsole.VirtualWrite("" + PlayerOneSelectionColor, 31, 6);
            Console.ForegroundColor = ConsoleColor.Gray;



            if (selected == 3)
                Console.BackgroundColor = ConsoleColor.DarkGreen;
            else
                Console.BackgroundColor = ConsoleColor.Black;
            ExtendedConsole.VirtualWrite("Player Two Name = " + PlayerTwoName, 2, 8);

            if (selected == 4)
                Console.BackgroundColor = ConsoleColor.DarkGreen;
            else
                Console.BackgroundColor = ConsoleColor.Black;
            ExtendedConsole.VirtualWrite("Player Two Color = ", 2, 9);
            Console.ForegroundColor = PlayerTwoColor;
            ExtendedConsole.VirtualWrite("" + PlayerTwoColor, 21, 9);
            Console.ForegroundColor = ConsoleColor.Gray;

            if (selected == 5)
                Console.BackgroundColor = ConsoleColor.DarkGreen;
            else
                Console.BackgroundColor = ConsoleColor.Black;
            ExtendedConsole.VirtualWrite("Player Two Selection Color = ", 2, 10);
            Console.ForegroundColor = PlayerTwoSelectionColor;
            ExtendedConsole.VirtualWrite("" + PlayerTwoSelectionColor, 31, 10);
            Console.ForegroundColor = ConsoleColor.Gray;

            if (selected == 6)
                Console.BackgroundColor = ConsoleColor.DarkGreen;
            else
                Console.BackgroundColor = ConsoleColor.Black;
            ExtendedConsole.VirtualWrite("Kings HP = " + HitPoint, 2, 12);

            if (selected == 7)
                Console.BackgroundColor = ConsoleColor.DarkGreen;
            else
                Console.BackgroundColor = ConsoleColor.Black;
            if (BoardSelection == 0)
                ExtendedConsole.VirtualWrite("Board = Random", 2, 14);
            else
                ExtendedConsole.VirtualWrite("Board = Manual Selection", 2, 14);

            if (selected == 8)
                Console.BackgroundColor = ConsoleColor.DarkGreen;
            else
                Console.BackgroundColor = ConsoleColor.Black;
            ExtendedConsole.VirtualWrite("Laser Tick Time = " + LaserTickTime, 2, 16);

            if (selected == 9)
                Console.BackgroundColor = ConsoleColor.DarkGreen;
            else
                Console.BackgroundColor = ConsoleColor.Black;
            ExtendedConsole.VirtualWrite("Connection Port = " + ConnectionPort, 2, 18);


            ExtendedConsole.Update();
        }

        private string EnterString(string forSetting, string currenttext)
        {
            Console.BackgroundColor = ConsoleColor.Black;
            ExtendedConsole.VirtualClear();
            ExtendedConsole.VirtualDrawBox(0, 0, 49, 27);
            ExtendedConsole.VirtualWrite("SETTINGS", 20, 2);
            ExtendedConsole.VirtualWrite(forSetting, 24 - forSetting.Length / 2, 4);
            ExtendedConsole.Update();

            Console.SetCursorPosition(2, 6);
            Console.CursorVisible = true;
            string toReturn = Console.ReadLine();
            if (toReturn == "")
                toReturn = currenttext;
            Console.CursorVisible = false;
            return toReturn;
        }

        private int EnterNumber(string forSetting, int currentNumber)
        {
            Console.BackgroundColor = ConsoleColor.Black;
            ExtendedConsole.VirtualClear();
            ExtendedConsole.VirtualDrawBox(0, 0, 49, 27);
            ExtendedConsole.VirtualWrite("SETTINGS", 20, 2);
            ExtendedConsole.VirtualWrite(forSetting, 24 - forSetting.Length / 2, 4);
            ExtendedConsole.Update();

            Console.SetCursorPosition(2, 6);
            Console.CursorVisible = true;
            int toReturn = 0;
            Int32.TryParse(Console.ReadLine(), out toReturn);
            if (toReturn <= 0)
                toReturn = currentNumber;
            Console.CursorVisible = false;
            return toReturn;
        }

        private ConsoleColor ChooseColor(string forSetting, ConsoleColor currentColor)
        {
            Console.BackgroundColor = ConsoleColor.Black;
            ExtendedConsole.VirtualClear();
            ExtendedConsole.VirtualDrawBox(0, 0, 49, 27);
            ExtendedConsole.VirtualWrite("SETTINGS", 20, 2);
            ExtendedConsole.VirtualWrite(forSetting, 25 - forSetting.Length / 2, 4);

            int currentPosition = 0;
            bool isChoosen = false;
            while (!isChoosen)
            {
                DrawColorChoice(currentPosition);
                ConsoleKey input = Console.ReadKey(true).Key;
                switch (input)
                {
                    case ConsoleKey.UpArrow:
                    case ConsoleKey.NumPad8:
                        currentPosition--;
                        if (currentPosition < 0)
                            currentPosition = 15;
                        break;
                    case ConsoleKey.DownArrow:
                    case ConsoleKey.NumPad2:
                        currentPosition++;
                        if (currentPosition > 15)
                            currentPosition = 0;
                        break;
                    case ConsoleKey.Enter:
                    case ConsoleKey.NumPad5:
                    case ConsoleKey.Spacebar:
                        isChoosen = true;
                        break;
                    case ConsoleKey.Escape:
                        isChoosen = true;
                        currentPosition = (int)currentColor;
                        break;
                }
            }

            return (ConsoleColor)currentPosition;
        }

        private void DrawColorChoice(int currentPosition)
        {
            for (int i = 0; i < 16; i++)
            {
                Console.ForegroundColor = (ConsoleColor)i;
                if (currentPosition == i)
                    Console.BackgroundColor = ConsoleColor.DarkGreen;
                else
                    Console.BackgroundColor = ConsoleColor.Black;
                ExtendedConsole.VirtualWrite("" + (ConsoleColor)i, 2, 6 + i);
            }
            ExtendedConsole.Update();
        }

        private void SelectOption()
        {
            int currentPosition = 0;
            ConsoleKey input = Console.ReadKey(true).Key;
            while (input != ConsoleKey.Escape)
            {

                switch (input)
                {
                    case ConsoleKey.UpArrow:
                    case ConsoleKey.NumPad8:
                        currentPosition--;
                        if (currentPosition < 0)
                            currentPosition = 9;
                        break;
                    case ConsoleKey.DownArrow:
                    case ConsoleKey.NumPad2:
                        currentPosition++;
                        if (currentPosition > 9)
                            currentPosition = 0;
                        break;
                    case ConsoleKey.Enter:
                        ChangeSetting(currentPosition);
                        break;
                }

                Draw(currentPosition);
                input = Console.ReadKey(true).Key;
            }
        }

        public void ChangeSetting(int settingsPosition)
        {
            switch (settingsPosition)
            {
                case 0:
                    PlayerOneName = EnterString("Enter player one's name", PlayerOneName);
                    break;
                case 1:
                    PlayerOneColor = ChooseColor(PlayerOneName + "'s Color", PlayerOneColor);
                    break;
                case 2:
                    PlayerOneSelectionColor = ChooseColor(PlayerOneSelectionColor + "'s Color", PlayerOneSelectionColor);
                    break;
                case 3:
                    PlayerTwoName = EnterString("Enter player two's name", PlayerTwoName);
                    break;
                case 4:
                    PlayerTwoColor = ChooseColor(PlayerTwoColor + "'s Color", PlayerTwoColor);
                    break;
                case 5:
                    PlayerTwoSelectionColor = ChooseColor(PlayerTwoSelectionColor + "'s Color", PlayerTwoSelectionColor);
                    break;
                case 6:
                    HitPoint = EnterNumber("Enter kings starting hit points", HitPoint);
                    break;
                case 7:
                    string[] choices = new string[] { "Random", "Manual Selection" };
                    Console.BackgroundColor = ConsoleColor.Black;
                    ExtendedConsole.VirtualClear();
                    ExtendedConsole.VirtualDrawBox(0, 0, 49, 27);
                    ExtendedConsole.VirtualWrite("SETTINGS", 20, 2);
                    ExtendedConsole.VirtualWrite("Board Selection", 17, 4);
                    int i = ExtendedConsole.ShowMenuAndGetChoice(choices, 16, 6, 0, true, null, false);
                    if (i != -1)
                        BoardSelection = i;

                    break;
                case 8:
                    LaserTickTime = EnterNumber("Enter wait time between laser movement in ms", LaserTickTime);
                    break;
                case 9:
                    ConnectionPort = EnterNumber("Enter new connection port", ConnectionPort);
                    break;
            }
        }

        public static void Load()
        {
            try
            {
                XDocument xml = new XDocument();
                if (File.Exists("Settings.xml"))
                {
                    xml = XDocument.Load("Settings.xml");
                    PlayerOneName = xml.Descendants("PlayerOneName").Select(element => element.Value).ToArray()[0];
                    PlayerOneColor = (ConsoleColor)Int32.Parse(xml.Descendants("PlayerOneColor").Select(element => element.Value).ToArray()[0]);
                    PlayerOneSelectionColor = (ConsoleColor)Int32.Parse(xml.Descendants("PlayerOneSelectionColor").Select(element => element.Value).ToArray()[0]);
                    PlayerTwoName = xml.Descendants("PlayerTwoName").Select(element => element.Value).ToArray()[0];
                    PlayerTwoColor = (ConsoleColor)Int32.Parse(xml.Descendants("PlayerTwoColor").Select(element => element.Value).ToArray()[0]);
                    PlayerTwoSelectionColor = (ConsoleColor)Int32.Parse(xml.Descendants("PlayerTwoSelectionColor").Select(element => element.Value).ToArray()[0]);
                    BoardSelection = Int32.Parse(xml.Descendants("BoardSelection").Select(element => element.Value).ToArray()[0]);
                    LaserTickTime = Int32.Parse(xml.Descendants("LaserStepTime").Select(element => element.Value).ToArray()[0]);
                    ConnectionPort = Int32.Parse(xml.Descendants("ConnectionPort").Select(element => element.Value).ToArray()[0]);
                    HitPoint = Int32.Parse(xml.Descendants("HitPoint").Select(element => element.Value).ToArray()[0]);
                }
                else
                    SaveToXml();
            }
            catch
            {
                SaveToXml();
            }
        }

        static private void SaveToXml()
        {
            XDocument newXml = new XDocument();
            XElement root = new XElement("root");
            newXml.Add(root);
            root.Add(new XElement("PlayerOneName", PlayerOneName));
            root.Add(new XElement("PlayerOneColor", (int)PlayerOneColor));
            root.Add(new XElement("PlayerOneSelectionColor", (int)PlayerOneSelectionColor));
            root.Add(new XElement("PlayerTwoName", PlayerTwoName));
            root.Add(new XElement("PlayerTwoColor", (int)PlayerTwoColor));
            root.Add(new XElement("PlayerTwoSelectionColor", (int)PlayerTwoSelectionColor));
            root.Add(new XElement("HitPoint", HitPoint));
            root.Add(new XElement("BoardSelection", BoardSelection));
            root.Add(new XElement("LaserStepTime", LaserTickTime));
            root.Add(new XElement("ConnectionPort", ConnectionPort));

            newXml.Save("Settings.xml");
        }
    }
}
