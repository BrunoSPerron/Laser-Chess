using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;
using Microsoft.Win32.SafeHandles;

namespace Game
{
    public class ExtendedConsole
    {
        #region P/invokes
        //Nécéssaire pour avoir accéder au buffer de la console
        [DllImport("Kernel32.dll", SetLastError = true)]
        static extern IntPtr GetStdHandle(uint nStdHandle);

        //Nécéssaire pour écrire dans le buffer de la console
        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool WriteConsoleOutput(
        SafeFileHandle hConsoleOutput,
        [MarshalAs(UnmanagedType.LPArray), In] CHAR_INFO[] lpBuffer,
        COORD dwBufferSize,
        COORD dwBufferCoord,
        ref RECT lpWriteRegion);

        //Nécessaire pour empecher le redimentionnement de la console par l'utilisateur
        [DllImport("user32.dll")]
        public static extern int DeleteMenu(IntPtr hMenu, int nPosition, int wFlags);

        [DllImport("user32.dll")]
        private static extern IntPtr GetSystemMenu(IntPtr hWnd, bool bRevert);

        [DllImport("kernel32.dll", ExactSpelling = true)]
        private static extern IntPtr GetConsoleWindow();
        #endregion

        private static CHAR_INFO[][,] Layers = new CHAR_INFO[3][,];

        private static int activeLayer = 0;

        //public static ConsoleColor ForegroundColor { get; set; } = ConsoleColor.Gray;
        //public static ConsoleColor BackgroundColor { get; set; } = ConsoleColor.Black;

        public static ConsoleColor DisabledColor { get; set; } = ConsoleColor.DarkGray;
        public static ConsoleColor SelectionColor { get; set; } = ConsoleColor.DarkGreen;
        public static ConsoleColor LineUIColor { get; set; } = ConsoleColor.Blue;

        private static bool LoadingComplete = Initiate();

        private static bool Initiate()
        {
            Console.CursorVisible = false;

            //remove window rescaling
            IntPtr handle = GetConsoleWindow();
            IntPtr sysMenu = GetSystemMenu(handle, false);
            DeleteMenu(sysMenu, 0xF000, 0x00000000);    //resize
            DeleteMenu(sysMenu, 0xF030, 0x00000000);    //maximize

            for (int i = 0; i < Layers.Length; i++)
                Layers[i] = new CHAR_INFO[Console.BufferWidth, Console.BufferHeight];

            return true;
        }

        /// <summary> Change la taille de la console, du systeme de layers et du nombre de layers</summary>
        public static void SetConsoleSize(int x, int y, int nbOflayers)
        {
            if (x <= 0 || y <= 0)
                throw new OutOfBoundsParameterException(x, y, "ERROR: can't set console size to lower than 1");

            for (int i = 0; i < nbOflayers; i++)
                Layers[i] = new CHAR_INFO[x, y];

            Console.SetWindowSize(x, y);
            Console.SetBufferSize(x, y);
            Console.SetWindowSize(x, y);
        }
        /// <summary> Change la taille de la console et du systeme de layers</summary>
        public static void SetConsoleSize(int x, int y)
        {
            SetConsoleSize(x, y, Layers.Length);
        }

        /// <summary>Change le nombre de Layers.</summary>
        public static void SetNumberOfLayers(int numberOfLayers)
        {
            if (numberOfLayers <= 0)
                throw new ImpossibleParametersException("ERROR: the number of layers need to be higher than 0");

            CHAR_INFO[][,] oldLayers = Layers;
            Layers = new CHAR_INFO[numberOfLayers][,];
            for (int i = 0; i < Layers.Length; i++)
            {
                if (i < oldLayers.Length)
                    Layers[i] = oldLayers[i];
                else
                    Layers[i] = new CHAR_INFO[Console.BufferWidth, Console.WindowHeight];
            }
        }

        /// <summary>Change le layer actif, celui qui seras modifié par les autres méthodes</summary>
        public static void SetActiveLayer(int index)
        {
            if (index < 0 && index >= Layers.Length)
                throw new ImpossibleParametersException("ERROR: Can't activate nonexistent layer");

            activeLayer = index;
        }

        /// <summary>Écrit quelque chose sur le layer actif sans mettre à jour la console.</summary>
        public static void VirtualWrite(int i, int x = 0, int y = 0)
        {
            VirtualWrite(i.ToString(), x, y);
        }
        public static void VirtualWrite(char c, int x = 0, int y = 0)
        {
            VirtualWrite(c.ToString(), x, y);
        }
        public static void VirtualWrite(string text, int x = 0, int y = 0)
        {
            if (x < 0 || x >= Layers[0].GetLength(0) || y < 0 || y >= Layers[0].GetLength(0))
                throw new OutOfBoundsParameterException(x, y, "ERROR: Parameters point outside the console size");

            short currentAttributes = GetAttributesValue(Console.ForegroundColor, Console.BackgroundColor);
            byte[] eASCII = Encoding.GetEncoding(437).GetBytes(text);   //Converti le texte en un tableau de byte ASCII extended

            for (int i = 0; i < text.Length; i++)
            {
                //Prépare et écrit dans la prochaine case du layer les infos au standard win32
                CHAR_INFO ci = new CHAR_INFO();
                ci.charData = new byte[] { eASCII[i], (byte)text[i] };
                ci.attributes = currentAttributes;
                Layers[activeLayer][x, y] = ci;

                //Décide de la prochaine case où écrire, change de ligne au besoin
                x++;
                if (x >= Console.BufferWidth)
                {
                    x = 0;
                    y++;
                    if (y >= Layers[0].GetLength(1))
                        y = 0;
                }
            }
        }

        /// <summary>Efface le layer actif sans mettre a jour la console.</summary>
        public static void VirtualErase()
        {
            VirtualLayerReset();
        }
        /// <summary>Efface une zone sur le layer actif aux coordonées du pointeur virtuel sans mettre à jour la console.</summary>
        public static void VirtualErase(int x = 0, int y = 0, int width = 1, int height = 1)
        {

            if (x < 0  || y < 0)
                throw new OutOfBoundsParameterException(x, y, "ERROR: Parameters point outside the console limit");
            if (x + width > Layers[0].GetLength(0) || y + height > Layers[0].GetLength(1))
                throw new OutOfBoundsParameterException(x + width, y + width, "ERROR: Parameters result point outside the console limit");
            
            //Réinitialize les positions à effacer
            for (int i = 0; i < width; i++)
                for (int j = 0; j < height; j++)
                    Layers[activeLayer][i + x, j + y] = new CHAR_INFO();
        }

        /// <summary>Réinitialize un layers sans mettre à jour la console. Layer actif par default</summary>
        public static void VirtualLayerReset(int index)
        {
            if (index < 0 && index >= Layers.Length)
                throw new ImpossibleParametersException("ERROR: Can't activate nonexistent layer");

            Layers[index] = new CHAR_INFO[Layers[0].GetLength(0), Layers[0].GetLength(1)];
        }
        /// <summary>Réinitialize le layer actif sans mettre à jour la console.</summary>
        public static void VirtualLayerReset()
        {
            VirtualLayerReset(activeLayer);
        }

        /// <summary>Réinitialize tous les layers sans mettre à jour la console.</summary>
        public static void VirtualClear()
        {
            for (int i = 0; i < Layers.Length; i++)
                Layers[i] = new CHAR_INFO[Console.BufferWidth + 1, Console.WindowHeight + 1];
        }

        public static void VirtualFill(char c, int x, int y, int width, int height)
        {
            if (x < 0 || y < 0)
                throw new OutOfBoundsParameterException(x, y, "ERROR: Parameters point outside the console limit");
            if (x + width > Layers[0].GetLength(0) || y + height > Layers[0].GetLength(1))
                throw new OutOfBoundsParameterException(x + width, y + width, "ERROR: Parameters result point outside the console limit");

            string toWrite = "";
            while (toWrite.Length < width)
                toWrite += c;
            for (int i = 0; i < height; i++)
                VirtualWrite(toWrite, x, i + y);
        }
        public static void VirtualFill(int x, int y, int width, int height)
        {
            VirtualFill(' ', x, y, width, height);
        }

        /// <summary>Update une zone de la console en compilant les données du systeme de layers.</summary>
        public static void Update(int left, int top, int width, int height)
        {
            if (left < 0 || top < 0)
                throw new OutOfBoundsParameterException(left, top, "ERROR: Parameters point outside the console limit");
            if (left + width > Layers[0].GetLength(0) || top + height > Layers[0].GetLength(1))
                throw new OutOfBoundsParameterException(left + width, top + width, "ERROR: Parameters result point outside the console limit");

            //Initie les elements nécéssaires pour communiquer avec l'API Windows
            RECT zoneAUpdater = new RECT(left, top, width, height);
            COORD origin = new COORD(left, top);
            COORD size = new COORD(zoneAUpdater.Right, zoneAUpdater.Bottom);

            //La commande win32 utilisé plus loin copy l'info des cases situé dans les limites d'un RECT (struct) d'un tableau de CHAR_INFO (struct) aux la même position dans le buffer de la console
            //crée un tableau en conséquence et le rempli uniquement des characters à afficher
            CHAR_INFO[] buf = new CHAR_INFO[(left + width) * (top + height)];   //window utilise un tableau à une dimention (optimization de bas niveau)
            for (int i = 0; i < left + width; i++)
                for (int j = 0; j < top + height; j++)
                    if (i >= left && j >= top)
                        buf[i + (j * (left + width))] = GetCHARINFOAtPosition(i, j);

            //envoi ça a window pour qu'il l'écrive dans le GPU
            IntPtr StdHandle = GetStdHandle(unchecked((uint)-11));
            SafeFileHandle handle = new SafeFileHandle(StdHandle, false);
            if (!WriteConsoleOutput(handle, buf, size, origin, ref zoneAUpdater))
                throw new Win32Exception(Marshal.GetLastWin32Error());
            handle.SetHandleAsInvalid();
        }
        /// <summary>Update une position de la console en compilant les données du systeme de layers.</summary>
        public static void Update(int left, int top)
        {
            Update(left, top, 1, 1);
        }
        /// <summary>Update la console en compilant les données du systeme de layers.</summary>
        public static void Update()
        {
            Update(0, 0, Layers[0].GetLength(0), Layers[1].GetLength(1));
        }

        /// <summary>Retourne la prochaine touche "valide" entrée par l'utilisateur</summary>
        public static ConsoleKeyInfo GetUserInput(bool addConfirmInput = false, bool addCancelInput = false, bool addLetterInput = false)
        {
            return GetUserInput(null, addConfirmInput, addCancelInput, addLetterInput);
        }
        /// <summary>Retourne la prochaine touche "valide" entrée par l'utilisateur</summary>
        public static ConsoleKeyInfo GetUserInput(ConsoleKey[] validInput = null, bool addConfirmInput = false, bool addCancelInput = false, bool addLetterInput = false)
        {
            ConsoleKeyInfo userInput = new ConsoleKeyInfo();

            if (validInput == null)     //pour éviter une erreur
                validInput = new ConsoleKey[0];

            if (addConfirmInput)
            {
                ConsoleKey[] _newValidInput = new ConsoleKey[validInput.Length + 3];

                for (int i = 0; i < validInput.Length; i++)
                    _newValidInput[i] = validInput[i];

                _newValidInput[validInput.Length] = ConsoleKey.Enter;
                _newValidInput[validInput.Length + 1] = ConsoleKey.Spacebar;
                _newValidInput[validInput.Length + 2] = ConsoleKey.NumPad5;
                validInput = _newValidInput;
            }

            if (addCancelInput)
            {
                ConsoleKey[] _newValidInput = new ConsoleKey[validInput.Length + 2];

                for (int i = 0; i < validInput.Length; i++)
                    _newValidInput[i] = validInput[i];

                _newValidInput[validInput.Length] = ConsoleKey.Escape;
                _newValidInput[validInput.Length + 1] = ConsoleKey.Backspace;
                validInput = _newValidInput;
            }

            // Vide le buffer de la Console. Les entrées sont stocker dans ce buffer avant d'être traitées.
            while (Console.KeyAvailable)
                Console.ReadKey(true);

            // En cas d'erreur
            if (validInput.Length == 0)
                throw new ImpossibleParametersException("ERROR: Parameters result in no possible input");

            //Accepte seulement une entrée valide
            bool hasFinished = false;
            while (!hasFinished)
            {
                userInput = Console.ReadKey(true);
                foreach (ConsoleKey _keyCode in validInput)
                    if (userInput.Key == _keyCode || (char.IsLetter(userInput.KeyChar) && addLetterInput))
                        hasFinished = true;
            }
            return userInput;
        }

        /// <summary>Affiche une liste d'option et accepte l'input de l'utilisateur</summary>
        /// <returns>index du choix de l'utilisateur</returns>
        public static int ShowMenuAndGetChoice(string[] options, int left = 0, int top = 0, int startingPosition = 0, bool canCancel = true, bool[] disabledOptions = null, bool withBox = true)
        {
            //S'assure que les entrées ne causesont pas d'erreur
            if (disabledOptions == null)
                disabledOptions = new bool[options.Length];

            if (startingPosition < 0 || startingPosition >= options.Length)
                startingPosition = 0;

            if (options.Length == 0)
                throw new Exception("ERROR: No option in menu");
            if (disabledOptions.Length != options.Length)
                throw new Exception("ERROR: Unmatching Array size between options and disabledOptions");

            int width = 0;
            foreach (string s in options)
                if (width < s.Length)
                    width = s.Length;

            //Processus de selection
            ConsoleColor currentBGColor = Console.BackgroundColor;
            ConsoleColor currentFGColor = Console.ForegroundColor;

            int originY = top + 1;
            //Normalize la taille des options et écrit chacune d'elle à l'écran
            for (int i = 0; i < options.Length; i++)
            {
                while (options[i].Length < width)
                    options[i] += " ";

                if (disabledOptions[i])
                    Console.ForegroundColor = DisabledColor;
                if (i == startingPosition)
                    Console.BackgroundColor = SelectionColor;
                top = originY + i;
                VirtualWrite(options[i].Substring(0, width), left + 1, top);

                Console.BackgroundColor = currentBGColor;
                Console.ForegroundColor = currentFGColor;

            }
            if (withBox)
            {
                width = 0;
                foreach (string line in options)
                    if (line.Length > width)
                        width = line.Length;
                VirtualDrawBox(left, originY - 1, width + 2, options.Length + 2);
            }

            Update();
            top = originY;
            left++;

            int choice = startingPosition + 1;
            ConsoleKeyInfo userInput;
            bool hasFinished = false;
            while (!hasFinished)
            {
                userInput = GetUserInput(new ConsoleKey[] { ConsoleKey.UpArrow, ConsoleKey.NumPad8, ConsoleKey.DownArrow, ConsoleKey.NumPad2 }, true, true);

                switch (userInput.Key)
                {
                    case ConsoleKey.UpArrow:
                    case ConsoleKey.NumPad8:
                        if (options.Length == 1)
                            continue;

                        if (choice == 1)
                        {
                            Console.BackgroundColor = currentBGColor;
                            top = originY + choice - 1;
                            VirtualWrite(options[choice - 1], left, top);
                            choice = options.Length;
                            if (SelectionColor == currentBGColor)
                                Console.BackgroundColor = ConsoleColor.DarkGreen;
                            else
                                Console.BackgroundColor = SelectionColor;

                            top = originY + choice - 1;
                            VirtualWrite(options[choice - 1], left, top);
                        }
                        else
                        {
                            Console.BackgroundColor = currentBGColor;

                            top = originY + choice - 1;
                            VirtualWrite(options[choice - 1], left, top);
                            choice--;
                            if (SelectionColor == currentBGColor)
                                Console.BackgroundColor = ConsoleColor.DarkGreen;
                            else
                                Console.BackgroundColor = SelectionColor;

                            top = originY + choice - 1;
                            VirtualWrite(options[choice - 1], left, top);
                        }

                        break;
                    case ConsoleKey.DownArrow:
                    case ConsoleKey.NumPad2:
                        if (options.Length == 1)
                            continue;

                        if (choice == options.Length)
                        {
                            Console.BackgroundColor = currentBGColor;
                            top = originY + choice - 1;
                            VirtualWrite(options[choice - 1], left, top);
                            choice = 1;
                            if (SelectionColor == currentBGColor)
                                Console.BackgroundColor = ConsoleColor.DarkGreen;
                            else
                                Console.BackgroundColor = SelectionColor;

                            top = originY + choice - 1;
                            VirtualWrite(options[choice - 1], left, top);
                        }
                        else
                        {
                            Console.BackgroundColor = currentBGColor;

                            top = originY + choice - 1;
                            VirtualWrite(options[choice - 1], left, top);
                            choice++;
                            if (SelectionColor == currentBGColor)
                                Console.BackgroundColor = ConsoleColor.DarkGreen;
                            else
                                Console.BackgroundColor = SelectionColor;

                            top = originY + choice - 1;
                            VirtualWrite(options[choice - 1], left, top);
                        }
                        break;
                    case ConsoleKey.Spacebar:
                    case ConsoleKey.Enter:
                    case ConsoleKey.NumPad5:
                        if (!disabledOptions[choice - 1])
                            hasFinished = true;
                        break;
                    case ConsoleKey.Escape:
                    case ConsoleKey.Backspace:
                        choice = -1;
                        hasFinished = true;
                        break;
                }
                Update();
            }
            Console.BackgroundColor = currentBGColor;
            return choice - 1;
        }

        /// <summary>Ouverture animée d'une boite de menu sur le layer actif</summary>
        /// <param name="_openingSpeed">En milisecondes. Le résultat est aproximatif</param>
        public static void AnimatedMenuBoxOpening(int x, int y, int width, int height, int _openingSpeed = 500, string[] image = null, bool doubleLines = true)
        {
            ConsoleColor currentColor = Console.ForegroundColor;

            int sleepTimeHorizontal = (int)((float)_openingSpeed / width * ((float)width / (2f * height + width)));
            int sleepTimeVertical = (int)(_openingSpeed / (height / 2f) * ((float)height / (2f * height + width)));

            //Initialize l'image si null.
            if (image == null)
            {
                image = new string[height - 2];
                string emptyline = "";
                while (emptyline.Length < width - 2)
                    emptyline += " ";
                for (int i = 0; i < image.Length; i++)
                {
                    image[i] = emptyline;
                }
            }
            else //Redimentionne l'image aux dimentions spécifiée, si necessaire
            {
                if (image.Length < height - y)
                {
                    string[] _resizedImage = new string[height - y];
                    for (int i = 0; i < _resizedImage.Length; i++)
                    {
                        if (image.Length > i)
                            _resizedImage[i] = image[i];
                        else
                            _resizedImage[i] = "";
                    }
                    image = _resizedImage;

                    for (int i = 0; i < image.Length; i++)
                    {
                        if (image[i].Length > width)
                            image[i] = image[i].Substring(0, width);
                        else while (image[i].Length < width - 2)
                                image[i] += " ";
                    }
                }
            }

            //Aggrandi une ligne horizontale à partir du milieu
            int middlePos = y + (height / 2);
            Console.ForegroundColor = LineUIColor;
            for (int i = 0; i <= width / 2; i++)
            {
                int lineXStartPoint = x + (width / 2) - i;
                int lineLength = i * 2;
                if (lineLength > width)
                    lineLength--;
                if (doubleLines)
                {
                    VirtualWrite("═", lineXStartPoint, middlePos);
                    VirtualWrite("═", lineXStartPoint + lineLength - 1, middlePos);
                }
                else
                {
                    VirtualWrite("─", lineXStartPoint, middlePos);
                    VirtualWrite("─", lineXStartPoint + lineLength - 1, middlePos);
                }
                //Relie la boite au reste de l'UI
                VirtualLinkUILines(lineXStartPoint + 1, middlePos, doubleLines);
                VirtualLinkUILines(lineXStartPoint, middlePos, doubleLines);
                VirtualLinkUILines(lineXStartPoint + lineLength - 2, middlePos, doubleLines);
                VirtualLinkUILines(lineXStartPoint + lineLength - 1, middlePos, doubleLines);

                Update(x, y, width, height);
                Thread.Sleep(sleepTimeHorizontal);
            }

            //Ouvre la boite verticalement
            for (int i = 0; i < (height / 2); i++)
            {
                int lineYStartPoint = y + (height / 2) - i;

                Console.ForegroundColor = currentColor;
                int lineLength = i * 2;
                if (height % 2 == 1)
                    lineLength++;


                if (lineLength > height)
                    lineLength--;
                int lineToDraw = height / 2 - i - 1;
                VirtualWrite(image[lineToDraw], x + 1, lineYStartPoint);
                lineToDraw = height / 2 + i - 2;
                if (height % 2 == 1)
                    lineToDraw++;
                VirtualWrite(image[lineToDraw], x + 1, lineYStartPoint + lineLength - 1);

                Console.ForegroundColor = LineUIColor;
                if (doubleLines)
                {
                    VirtualWrite("║", x, lineYStartPoint);
                    VirtualWrite("║", x, lineYStartPoint + lineLength - 1);
                    VirtualWrite("║", x + width - 1, lineYStartPoint);
                    VirtualWrite("║", x + width - 1, lineYStartPoint + lineLength - 1);
                }
                else
                {
                    VirtualWrite("│", x, lineYStartPoint);
                    VirtualWrite("│", x, lineYStartPoint + lineLength - 1);
                    VirtualWrite("│", x + width - 1, lineYStartPoint);
                    VirtualWrite("│", x + width - 1, lineYStartPoint + lineLength - 1);
                }

                //Complete la boite et la relie au reste de l'UI
                VirtualDrawHorizontalLine(lineYStartPoint - 1, x, width, doubleLines);
                VirtualLinkUILines(x, lineYStartPoint, doubleLines);
                VirtualLinkUILines(x + width - 1, lineYStartPoint, doubleLines);
                VirtualDrawHorizontalLine(lineYStartPoint + lineLength, x, width, doubleLines);
                VirtualLinkUILines(x, lineYStartPoint + lineLength - 1, doubleLines);
                VirtualLinkUILines(x + width - 1, lineYStartPoint + lineLength - 1, doubleLines);
                for (int j = 1; j < width - 1; j++)
                {
                    bool aboveIsLinked = IsLinkedDown(x + j, lineYStartPoint - 2);
                    if (aboveIsLinked)
                        if (doubleLines)
                            VirtualWrite("╩", x + j, lineYStartPoint - 1);
                        else
                            VirtualWrite("┴", x + j, lineYStartPoint - 1);

                    bool belowIsLinked = IsLinkedDown(x + j, lineYStartPoint + lineLength + 1);
                    if (belowIsLinked)
                        if (doubleLines)
                            VirtualWrite("╦", x + j, lineYStartPoint + lineLength);
                        else
                            VirtualWrite("┬", x + j, lineYStartPoint + lineLength);
                }

                Thread.Sleep(sleepTimeVertical);
                Update(x, y, width, height);
            }
            Console.ForegroundColor = currentColor;
        }

        /// <summary>Ouverture animée d'une boite de menu sur le layer actif, adaptée a la taille de l'image</summary>
        /// <param name="_openingSpeed">Le résultat est aproximatif</param>
        public static void AnimatedMenuBoxOpening(int x, int y, string[] image, int _openingSpeed = 500)
        {
            int height = image.Length + 2;
            int width = 0;

            foreach (string s in image)
                if (s != null)
                    if (width < s.Length + 2)
                        width = s.Length + 2;

            AnimatedMenuBoxOpening(x, y, width, height, _openingSpeed, image);
        }

        /// <summary>Animation de la fermeture d'une boite de menu sur le layer actif</summary>
        /// <param name="_closingSpeed">Le résultat est aproximatif</param>
        public static void AnimatedMenuBoxClosing(int x, int y, int width, int height, int closingSpeed = 500, bool doubleLines = true)
        {
            ConsoleColor currentColor = Console.ForegroundColor;
            Console.ForegroundColor = LineUIColor;
            int sleepTimeHorizontal = (int)((float)closingSpeed / width * ((float)width / (2 * height + width)));
            int sleepTimeVertical = (int)(closingSpeed / (height / 2f) * ((float)height / (2 * height + width)));

            //ferme Verticalement
            for (int i = (height / 2); i > 0; i--)
            {
                VirtualErase(x, y + (height / 2) - i, width, 1);
                VirtualErase(x, y + (height / 2) + i, width, 1);
                VirtualDrawHorizontalLine(y + (height / 2) - i + 1, x, width);
                VirtualDrawHorizontalLine(y + (height / 2) + i - 1, x, width);
                //Relie au reste de l'UI
                for (int j = 1; j < width - 1; j++)
                {
                    bool isLinkedUp = IsLinkedDown(x + j, y + (height / 2) - i);
                    bool isLinkedDown = IsLinkedUp(x + j, y + (height / 2) + i);
                    if (i != 1)
                    {
                        if (isLinkedUp)
                        {
                            if (doubleLines)
                                VirtualWrite("╩", x + j, (y + (height / 2) - i + 1));
                            else
                                VirtualWrite("┴", x + j, (y + (height / 2) - i + 1));
                        }

                        if (isLinkedDown && (height / 2) + i <= height)
                        {
                            if (doubleLines)
                                VirtualWrite("╦", x + j, (y + (height / 2) + i - 1));
                            else
                                VirtualWrite("┬", x + j, (y + (height / 2) + i - 1));
                        }
                    }
                    else
                    {
                        if (isLinkedUp && isLinkedDown)
                            if (doubleLines)
                                VirtualWrite("╬", x + j, (y + (height / 2) + i - 1));
                            else
                                VirtualWrite("┼", x + j, (y + (height / 2) + i - 1));
                        else if (isLinkedDown)
                            if (doubleLines)
                                VirtualWrite("╦", x + j, (y + (height / 2) + i - 1));
                            else
                                VirtualWrite("┬", x + j, (y + (height / 2) + i - 1));
                        else if (isLinkedUp)
                            if (doubleLines)
                                VirtualWrite("╩", x + j, (y + (height / 2) + i - 1));
                            else
                                VirtualWrite("┴", x + j, (y + (height / 2) + i - 1));
                    }

                }
                Update();
                Thread.Sleep(sleepTimeVertical);
            }

            //ferme horizontalement
            for (int i = width / 2; i >= 0; i--)
            {
                VirtualErase(x + (width / 2) - i, y + (height / 2), 1, 1);
                VirtualErase(x + (width / 2) + i, y + (height / 2), 1, 1);
                Update();
                Thread.Sleep(sleepTimeHorizontal);
            }
            Console.ForegroundColor = currentColor;
        }

        /// <summary>Animation de la fermeture d'une boite de menu sur le layer actif</summary>
        /// <param name="_closingSpeed">Le résultat est aproximatif</param>
        public static void AnimatedMenuBoxClosing(int x, int y, string[] image, int _openingSpeed = 500)
        {
            int height = image.Length + 2;
            int width = 0;

            foreach (string s in image)
                if (width < s.Length + 2)
                    width = s.Length + 2;

            AnimatedMenuBoxClosing(x, y, width, height, _openingSpeed);
        }

        /// <summary>Dessine une boite en ascii sur le layer actif sans mettre à jour la console</summary>
        public static void VirtualDrawBox(int left, int top, int width, int height, bool doubleLines = true)
        {
            if (width > 1 && height > 1)
            {
                VirtualDrawHorizontalLine(top, left, width, doubleLines);
                VirtualDrawHorizontalLine(top + height - 1, left, width, doubleLines);

                VirtualDrawVerticalLine(left, top, height, doubleLines);
                VirtualDrawVerticalLine(left + width - 1, top, height, doubleLines);

            }
            else if (width > 1)
                VirtualDrawHorizontalLine(top, left, width, doubleLines);
            else if (height > 1)
                VirtualDrawVerticalLine(left, top, height, doubleLines);
        }
        public static void VirtualDrawBox(int left, int top, string[] image, bool doubleLines = true)
        {
            int width = 2;
            int height = image.Length + 2;
            for (int i = 0; i < image.Length; i++)
            {
                if (image[i] != null)
                {
                    if (image[i].Length > width - 2)
                        width = image[i].Length + 2;

                }
                else
                {
                    image[i] = "";
                }
            }
            for (int i = 0; i < image.Length; i++)
                while (image[i].Length < width - 2)
                    image[i] += " ";

            VirtualDrawBox(left, top, width, height, doubleLines);

            for (int i = 0; i < image.Length; i++)
                VirtualWrite(image[i], left + 1, top + i + 1);
        }

        static bool IsLinkedDown(int x, int y)
        {
            bool isLinked = false;
            CHAR_INFO ci = GetCHARINFOAtPosition(x, y);
            switch (ci.charData[0])
            {
                case 186://║
                case 187://╗
                case 203://╦
                case 201://╔
                case 185://╣
                case 204://╠
                case 206://╬
                case 179://│
                case 191://┐
                case 194://┬
                case 218://┌
                case 180://┤
                case 195://├
                case 197://┼
                    isLinked = true;
                    break;
            }
            return isLinked;
        }

        static bool IsLinkedUp(int x, int y)
        {
            bool isLinked = false;
            CHAR_INFO ci = GetCHARINFOAtPosition(x, y);
            switch (ci.charData[0])
            {
                case 186://║
                case 188://╝
                case 202://╩
                case 200://╚
                case 185://╣
                case 204://╠
                case 206://╬
                case 179://│
                case 217://┘
                case 193://┴
                case 192://└
                case 180://┤
                case 195://├
                case 197://┼
                    isLinked = true;
                    break;
            }
            return isLinked;
        }

        static bool IsLinkedLeft(int x, int y)
        {
            bool isLinked = false;
            CHAR_INFO ci = GetCHARINFOAtPosition(x, y);
            switch (ci.charData[0])
            {
                case 205://═
                case 187://╗
                case 202://╩
                case 188://╝
                case 203://╦
                case 185://╣
                case 206://╬
                case 196://─
                case 191://┐
                case 193://┴
                case 217://┘
                case 194://┬
                case 180://┤
                case 197://┼
                    isLinked = true;
                    break;
            }
            return isLinked;
        }

        static bool IsLinkedRight(int x, int y)
        {
            bool isLinked = false;
            CHAR_INFO ci = GetCHARINFOAtPosition(x, y);
            switch (ci.charData[0])
            {
                case 205://═
                case 201://╔
                case 203://╦
                case 200://╚
                case 202://╩
                case 204://╠
                case 206://╬
                case 196://─
                case 192://└
                case 194://┬
                case 218://┌
                case 193://┴
                case 195://├
                case 197://┼
                    isLinked = true;
                    break;
            }
            return isLinked;
        }

        /// <summary>Remplace le symbole ASCII a la position par celui adéquat en fonction des lignes d'UI adjaceantes</summary>
        public static void VirtualLinkUILines(int x, int y, bool doubleLines = true)
        {
            bool aboveIsLinked = IsLinkedDown(x, y - 1);
            bool belowIsLinked = IsLinkedUp(x, y + 1);
            bool leftIsLinked = IsLinkedRight(x - 1, y);
            bool rightIsLinked = IsLinkedLeft(x + 1, y);

            if (aboveIsLinked)
            {
                if (rightIsLinked)
                {
                    if (belowIsLinked)
                    {
                        if (leftIsLinked)
                        {
                            if (doubleLines)
                                VirtualWrite("╬", x, y);
                            else
                                VirtualWrite("┼", x, y);
                        }
                        else
                        {
                            if (doubleLines)
                                VirtualWrite("╠", x, y);
                            else
                                VirtualWrite("├", x, y);
                        }
                    }
                    else //below isn't linked
                    {
                        if (leftIsLinked)
                        {
                            if (doubleLines)
                                VirtualWrite("╩", x, y);
                            else
                                VirtualWrite("┴", x, y);
                        }
                        else
                        {
                            if (doubleLines)
                                VirtualWrite("╚", x, y);
                            else
                                VirtualWrite("└", x, y);
                        }
                    }
                }
                else //right isn't linked
                {
                    if (belowIsLinked)
                    {
                        if (leftIsLinked)
                        {
                            if (doubleLines)
                                VirtualWrite("╣", x, y);
                            else
                                VirtualWrite("┤", x, y);
                        }
                        else
                        {
                            if (doubleLines)
                                VirtualWrite("║", x, y);
                            else
                                VirtualWrite("│", x, y);
                        }
                    }
                    else //below isn't linked
                    {
                        if (leftIsLinked)
                        {
                            if (doubleLines)
                                VirtualWrite("╝", x, y);
                            else
                                VirtualWrite("┘", x, y);
                        }
                    }
                }
            }
            else //above isn't linked
            {
                if (rightIsLinked)
                {
                    if (belowIsLinked)
                    {
                        if (leftIsLinked)
                        {
                            if (doubleLines)
                                VirtualWrite("╦", x, y);
                            else
                                VirtualWrite("┬", x, y);
                        }
                        else
                        {
                            if (doubleLines)
                                VirtualWrite("╔", x, y);
                            else
                                VirtualWrite("┌", x, y);
                        }
                    }
                    else //below isn't linked
                    {
                        if (leftIsLinked)
                        {
                            if (doubleLines)
                                VirtualWrite("═", x, y);
                            else
                                VirtualWrite("─", x, y);
                        }
                    }
                }
                else //right isn't linked
                {
                    if (belowIsLinked)
                    {
                        if (leftIsLinked)
                        {
                            if (doubleLines)
                                VirtualWrite("╗", x, y);
                            else
                                VirtualWrite("┐", x, y);
                        }
                    }
                }
            }
        }

        /// <summary>Desinne un ligne horizontale ASCII sur le layer actif sans updater la console</summary>
        public static void VirtualDrawHorizontalLine(int y, int x, int width, bool doubleLines = true)
        {
            int currentCursorPosX = x;
            int currentCursorPosY = y;
            ConsoleColor currentColor = Console.ForegroundColor;
            Console.ForegroundColor = LineUIColor;

            for (int i = 0; i < width; i++)
            {
                CHAR_INFO ci = GetCHARINFOAtPosition(x + i, y);

                switch (ci.charData[0])
                {
                    case 187: //╗
                    case 201: //╔
                    case 203: //╦
                    case 191: //┐
                    case 218: //┌
                    case 194: //┬
                        if (doubleLines)
                            VirtualWrite("╦", x + i, y);
                        else
                            VirtualWrite("┬", x + i, y);
                        break;

                    case 188: //╝
                    case 200: //╚
                    case 202: //╩
                    case 217: //┘
                    case 192: //└
                    case 193: //┴
                        if (doubleLines)
                            VirtualWrite("╩", x + i, y);
                        else
                            VirtualWrite("┴", x + i, y);
                        break;

                    case 186: //║
                    case 179: //│
                        bool aboveIsLinked = IsLinkedDown(x + i, y - 1);
                        bool belowIsLinked = IsLinkedUp(x + i, y + 1);

                        if (aboveIsLinked)
                        {
                            if (belowIsLinked)
                            {
                                if (doubleLines)
                                    VirtualWrite("╬", x + i, y);
                                else
                                    VirtualWrite("┼", x + i, y);
                            }
                            else
                            {
                                if (doubleLines)
                                    VirtualWrite("╩", x + i, y);
                                else
                                    VirtualWrite("┴", x + i, y);
                            }
                        }
                        else
                        {
                            if (belowIsLinked)
                            {
                                if (doubleLines)
                                    VirtualWrite("╦", x + i, y);
                                else
                                    VirtualWrite("┬", x + i, y);
                            }
                            else
                            {
                                if (doubleLines)
                                    VirtualWrite("═", x + i, y);
                                else
                                    VirtualWrite("─", x + i, y);
                            }
                        }
                        break;

                    case 204://╠
                    case 185://╣
                    case 206://╬
                    case 195://├
                    case 180://┤
                    case 197://┼
                        if (doubleLines)
                            VirtualWrite("╬", x + i, y);
                        else
                            VirtualWrite("┼", x + i, y);
                        break;

                    default:
                        if (doubleLines)
                            VirtualWrite("═", x + i, y);
                        else
                            VirtualWrite("─", x + i, y);
                        break;
                }
            }

            VirtualLinkUILines(x, y, doubleLines);
            VirtualLinkUILines(x + width - 1, y, doubleLines);
            Console.ForegroundColor = currentColor;
        }

        /// <summary>Desinne un ligne verticale ASCII sur le layer actif sans updater la console</summary>
        public static void VirtualDrawVerticalLine(int x, int start, int height, bool doubleLines = true)
        {
            ConsoleColor currentColor = Console.ForegroundColor;
            Console.ForegroundColor = LineUIColor;

            for (int i = 0; i < height; i++)
            {
                CHAR_INFO ci = GetCHARINFOAtPosition(x, start + i);

                switch (ci.charData[0])
                {
                    case 187: //╗
                    case 188: //╝
                    case 185: //╣
                    case 191: //┐
                    case 217: //┘
                    case 180: //┤
                        if (doubleLines)
                            VirtualWrite("╣", x, start + i);
                        else
                            VirtualWrite("┤", x, start + i);
                        break;

                    case 201: //╔
                    case 200: //╚
                    case 204: //╠
                    case 218: //┌
                    case 192: //└
                    case 195: //├
                        if (doubleLines)
                            VirtualWrite("╠", x, start + i);
                        else
                            VirtualWrite("├", x, start + i);
                        break;

                    case 205: //═
                    case 196: //─
                        bool leftIsLinked = IsLinkedRight(x - 1, start + i);
                        bool rightIsLinked = IsLinkedLeft(x + 1, start + i);

                        if (leftIsLinked)
                        {
                            if (rightIsLinked)
                            {
                                if (doubleLines)
                                    VirtualWrite("╬", x, start + i);
                                else
                                    VirtualWrite("┼", x, start + i);
                            }
                            else
                            {
                                if (doubleLines)
                                    VirtualWrite("╣", x, start + i);
                                else
                                    VirtualWrite("┤", x, start + i);
                            }
                        }
                        else
                        {
                            if (rightIsLinked)
                            {
                                if (doubleLines)
                                    VirtualWrite("╠", x, start + i);
                                else
                                    VirtualWrite("├", x, start + i);
                            }
                            else
                            {
                                if (doubleLines)
                                    VirtualWrite("║", x, start + i);
                                else
                                    VirtualWrite("│", x, start + i);
                            }
                        }
                        break;

                    case 203://╦
                    case 202://╩
                    case 206://╬
                    case 194://┬
                    case 193://┴
                    case 197://┼
                        if (doubleLines)
                            VirtualWrite("╬", x, start + i);
                        else
                            VirtualWrite("┼", x, start + i);
                        break;

                    default:
                        if (doubleLines)
                            VirtualWrite("║", x, start + i);
                        else
                            VirtualWrite("│", x, start + i);
                        break;
                }
            }

            VirtualLinkUILines(x, start);
            VirtualLinkUILines(x, start + height - 1);
            Console.ForegroundColor = currentColor;
        }

        /// <returns>la struct CHAR_INFO qui seras visible après la prochaine update</returns>
        static CHAR_INFO GetCHARINFOAtPosition(int _x, int _y)
        {
            CHAR_INFO toReturn = new CHAR_INFO();
            toReturn.charData = new byte[] { 0, 0 };
            toReturn.attributes = 0;

            if (_x > Layers[0].GetLength(0) - 1 || _y > Layers[0].GetLength(1))
                return toReturn;

            if (_x >= 0 && _x < Layers[0].GetLength(0) && _y >= 0 && _y < Layers[0].GetLength(1))
                for (int i = Layers.Length - 1; i >= 0; i--)
                    if (Layers[i][_x, _y].charData != null)
                    {
                        toReturn.charData = Layers[i][_x, _y].charData;
                        toReturn.attributes = Layers[i][_x, _y].attributes;
                        return toReturn;
                    }
            return toReturn;
        }

        public static char GetCharAtPosition(int x, int y)
        {
            char c = Encoding.GetEncoding(437).GetChars(GetCHARINFOAtPosition(x, y).charData)[0];
            if (c == '\0')  //if position is empty
                c = ' ';
            return c;
        }

        public static CHAR_INFO GetCHARINFOOnLayer(int layer, int x, int y)
        {
            return Layers[layer][x, y];
        }

        #region Trucs pour WIN32
        /// <summary>Combine 2 couleurs en une valeur compatible avec le format utiliser par le buffer de la console</summary>
        static short GetAttributesValue(ConsoleColor _foregroundColor, ConsoleColor _backgroundColor)
        {
            short color = 0;
            color += (short)_foregroundColor;
            color += (short)(16 * (int)_backgroundColor);
            return color;
        }

        //Structures nécessaires pour communiquer avec win32
        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public short Left;
            public short Top;
            public short Right;
            public short Bottom;

            public RECT(int _x, int _y, int width, int height)
            {
                Left = (short)_x;
                Top = (short)_y;
                Right = (short)(_x + width);
                Bottom = (short)(_y + height);
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct COORD
        {
            public short X;
            public short Y;

            public COORD(int _x, int _y) : this()
            {
                X = (short)_x;
                Y = (short)_y;
            }

            public COORD(short _x, short _y) : this()
            {
                X = _x;
                Y = _y;
            }

            public void Coord(int _x, int _y)
            {
                X = (short)_x;
                Y = (short)_y;
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct CHAR_INFO
        {
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
            public byte[] charData;
            public short attributes;
        }

        #endregion
    }


    class OutOfBoundsParameterException : Exception
    {
        public int x { get; }
        public int y { get; }
        public OutOfBoundsParameterException()
        {
        }

        public OutOfBoundsParameterException(int x, int y, string message) : base(message)
        {
            this.x = x;
            this.y = y;
        }

        public OutOfBoundsParameterException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected OutOfBoundsParameterException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }

    class ImpossibleParametersException : Exception
    {
        public ImpossibleParametersException()
        {
        }

        public ImpossibleParametersException(string message) : base(message)
        {
        }

        public ImpossibleParametersException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected ImpossibleParametersException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}

/*
 * ASCII codes
 *  ╔ = 201;
 *  ╚ = 200;
 *  ╗ = 187;
 *  ╝ = 188;
 *  ╠ = 204;
 *  ║ = 186;
 *  ╣ = 185;
 *  ╩ = 202;
 *  ═ = 205;
 *  ╦ = 203;
 *  ╬ = 206;
 *  ┌ = 218;
 *  └ = 192;
 *  ┐ = 191;
 *  ┘ = 217;
 *  ├ = 195;
 *  │ = 179;
 *  ┤ = 180;
 *  ┴ = 193;
 *  ─ = 196;
 *  ┬ = 194;
 *  ┼ = 197;
 */
