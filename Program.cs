using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewMineSweeper
{
    class Program
    {
        static int[] fieldSize = { 10, 5 };
        static double difficulty = 0.2;
        enum DisplayTileStatus
        {
            Unopened,
            Opened,
            Flagged,
            Questionned,
            Mine
        };
        readonly static byte[] displayTilesColors = new byte[]
        {
            15, 8,
            14, 6,
            11, 3,
            13, 5,
            12, 4
        };
        readonly static char[] displayTilesChars = new char[]
        { '*', '_', 'P', '?', 'X' };
        enum PlayerStatus
        {
            InGame,
            Died,
            Won
        }

        static PlayerStatus playerStatus = PlayerStatus.InGame;
        static int totalMines = (int)Math.Floor(difficulty * fieldSize[0] * fieldSize[1]);
        static int cursorIndex = 0;

        static bool[,] mineField;
        static DisplayTileStatus[,] displayTiles = new DisplayTileStatus[fieldSize[0], fieldSize[1]];
        readonly static List<int[]> pattern = generatePatternCoords();

        static void Main(string[] args)
        {
            int inputX, inputY;
            double inputD;
            // Init fieldSize[,] and difficulty if the user entered arguments
            switch (args.Length)
            {
                case 1: // 1 argument - difficulty
                    double.TryParse(args[0], out inputD);
                    difficulty = (inputD > 0.5 || inputD < (1.0 / (fieldSize[0] * fieldSize[1]))) ? 0.15 : inputD;
                    break;
                case 2: // 2 arguments - size and difficulty
                    int.TryParse(args[0], out inputX);

                    fieldSize[0] = inputX < 4 ? 10 : inputX;
                    fieldSize[1] = inputX < 4 ? 5 : inputX;

                    double.TryParse(args[1], out inputD);
                    difficulty = (inputD > 0.5 || inputD < (1.0 / (fieldSize[0] * fieldSize[1]))) ? 0.15 : inputD;
                    break;
                case 3: // 3 arguments - size x, size y and difficulty
                    int.TryParse(args[0], out inputX);
                    int.TryParse(args[1], out inputY);
                    double.TryParse(args[2], out inputD);
                    Console.WriteLine(inputD);

                    fieldSize[0] = inputX < 4 ? 10 : inputX;
                    fieldSize[1] = inputY < 4 ? 5 : inputY;

                    difficulty = (inputD > 0.5 || inputD < (1.0 / (fieldSize[0] * fieldSize[1]))) ? 0.15 : inputD;
                    break;
            }

            // Pre game
            printField();
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("Mines: " + totalMines);

            // Game
            while (playerStatus == PlayerStatus.InGame)
            {
                switch (Console.ReadKey().Key)
                {
                    case ConsoleKey.UpArrow:        // Cursor up
                        if (indexToCoords(cursorIndex)[1] != 0)
                            cursorIndex -= fieldSize[0];
                        else
                            cursorIndex = coordsToIndex(new int[] { indexToCoords(cursorIndex)[0], fieldSize[1] - 1 });
                        break;

                    case ConsoleKey.DownArrow:      // Cursor down
                        if (indexToCoords(cursorIndex)[1] != fieldSize[1] - 1)
                            cursorIndex += fieldSize[0];
                        else
                            cursorIndex = coordsToIndex(new int[] { indexToCoords(cursorIndex)[0], 0 });
                        break;

                    case ConsoleKey.LeftArrow:      // Cursor left
                        if (indexToCoords(cursorIndex)[0] != 0)
                            cursorIndex --;
                        else
                            cursorIndex = coordsToIndex(new int[] { fieldSize[0] - 1, indexToCoords(cursorIndex)[1] });
                        break;

                    case ConsoleKey.RightArrow:     // Cursor right
                        if (indexToCoords(cursorIndex)[0] != fieldSize[0] - 1)
                            cursorIndex++;
                        else
                            cursorIndex = coordsToIndex(new int[] { 0, indexToCoords(cursorIndex)[1] });
                        break;

                    case ConsoleKey.Z:              // Open
                        if (mineField == null)
                            mineField = generateMineField();
                        openTile(cursorIndex);
                        break;

                    case ConsoleKey.X:              // Flag
                        flagTile();
                        break;
                }
                printField();
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine("Mines: " + totalMines);
            }

            // End game
            Console.WriteLine();
            Console.ForegroundColor = playerStatus == PlayerStatus.Died ? ConsoleColor.Red : ConsoleColor.Green;
            Console.WriteLine(playerStatus == PlayerStatus.Died ? "You exploded :(" : "You won :)");
            Console.WriteLine("Press ENTER button to quit");
            Console.ReadLine();
        }
        /// <summary>
        /// Generate open pattern coords
        /// </summary>
        /// <returns>Local coordinates of open pattern</returns>
        static List<int[]> generatePatternCoords()
        {
            // This is normal only 8 adjacent tiles pattern
            List<int[]> patternCoords = new List<int[]>();

            for (int y = -1; y < 2; y++)
            {
                for (int x = -1; x < 2; x++)
                {
                    patternCoords.Add(new int[] { x, y });
                }
            }
            
            return patternCoords;
        }
        /// <summary>
        /// Generate a 2D tile array populated with mines
        /// </summary>
        /// <param name="cursorCoordIndex">The index of the coordinate where the cursor at </param>
        /// <returns>2D array of bool where each represents a mine</returns>
        static bool[,] generateMineField()
        {
            List<int> mineCoordIndexes = new List<int>();
            int numMines = (int)Math.Floor(difficulty * fieldSize[0] * fieldSize[1]);

            // Add mines ONLY with correct index
            for (int i = 0; i < numMines; i++)
            {
                int mineIndex = randomIndex();
                while (!isValidIndex(mineCoordIndexes, cursorIndex, mineIndex))
                    mineIndex = randomIndex();
                mineCoordIndexes.Add(mineIndex);
            }

            // Put indexes inside 2d array
            bool[,] generatedMineField = new bool[fieldSize[0], fieldSize[1]];
            for (int y = 0; y < fieldSize[1]; y++)
                for (int x = 0; x < fieldSize[0]; x++)
                    if (mineCoordIndexes.Contains(coordsToIndex(new int[] { x, y })))
                        generatedMineField[x, y] = true;

            return generatedMineField;


            // Give random coordinates in field size range
            int randomIndex()
            {
                Random r = new Random();
                return r.Next(0, coordsToIndex(new int[] { fieldSize[0], fieldSize[1] }));
            }
            // If the coordinate does not exist in the list and is not a cursor coordinate
            bool isValidIndex(List<int> mcil, int cci, int i)
            {
                return !(mcil.Contains(i) || i == cci);
            }
        }
        /// <summary>
        /// Prints the field in the console
        /// </summary>
        /// <param name="cursorIndex">The index of the coordinate where the cursor is</param>
        /// <param name="displayTiles">The array of tiles</param>
        static void printField()
        {
            Console.SetCursorPosition(0, 0);
            for (int y = 0; y < fieldSize[1]; y++)
            {
                for (int x = 0; x < fieldSize[0]; x++)
                {
                    setConsoleColor(cursorIndex, x, y, displayTiles[x, y]); // Console Color
                    if (displayTiles[x, y] != DisplayTileStatus.Opened) // If not opened
                        Console.Write(displayTilesChars[(int)displayTiles[x, y]] + " ");
                    else // If opened
                    {
                        if (minesNearIndex(coordsToIndex(new int[] { x, y })) == 0) // If has 0 mines near, write _
                            Console.Write(displayTilesChars[(int)displayTiles[x, y]] + " ");
                        else // If has a mine near, write number of mines
                            Console.Write(minesNearIndex(coordsToIndex(new int[] { x, y })) + " ");
                    }
                       
                    // New line
                    if (x == fieldSize[0] - 1)
                        Console.WriteLine();
                }
            }

            // Select and console color if current tile is active
            void setConsoleColor(int cursor, int x, int y, DisplayTileStatus status)
            {
                Console.ForegroundColor = (ConsoleColor) displayTilesColors[(byte) status * 2 + (cursorIndex == coordsToIndex(new int[] { x, y }) ? 0 : 1)];
            }
        }
        /// <summary>
        /// Opens a tile at index
        /// </summary>
        /// <param name="index">Coordinate index to open a tile</param>
        static void openTile(int index)
        {
            if (!(displayTiles[indexToCoords(index)[0], indexToCoords(index)[1]] == DisplayTileStatus.Opened))
            {
                if (mineField[indexToCoords(index)[0], indexToCoords(index)[1]]) // If opened tile was a mine
                {
                    displayTiles[indexToCoords(index)[0], indexToCoords(index)[1]] = DisplayTileStatus.Mine;
                    playerStatus = PlayerStatus.Died;
                }
                else // If it is not a mine
                {
                    displayTiles[indexToCoords(index)[0], indexToCoords(index)[1]] = DisplayTileStatus.Opened;
                    if (minesNearIndex(index) == 0) // Open all nearby tiles if there is 0 mines
                    {
                        int[] validIndexes = generateValidPatternIndexes(index);
                        foreach (int validIndex in validIndexes)
                            openTile(validIndex);
                    }

                    // Check if player won
                    int winCounter = 0;
                    for (int y = 0; y < fieldSize[1]; y++)
                        for (int x = 0; x < fieldSize[0]; x++)
                            if (!mineField[x, y] && displayTiles[x, y] != DisplayTileStatus.Unopened)
                                winCounter++;

                    if (winCounter == fieldSize[0] * fieldSize[1] - totalMines)
                            playerStatus = PlayerStatus.Won;
                }
            }
        }
        /// <summary>
        /// Flags a tile (flag -> question -> unopened)
        /// </summary>
        static void flagTile()
        {
            int[] flagCoords = { indexToCoords(cursorIndex)[0], indexToCoords(cursorIndex)[1] };
            switch (displayTiles[flagCoords[0], flagCoords[1]])
            {
                case DisplayTileStatus.Unopened: // Put a flag
                    displayTiles[flagCoords[0], flagCoords[1]] = DisplayTileStatus.Flagged;
                    break;
                case DisplayTileStatus.Flagged: // Put a question
                    displayTiles[flagCoords[0], flagCoords[1]] = DisplayTileStatus.Questionned;
                    break;
                case DisplayTileStatus.Questionned: // Remove the question
                    displayTiles[flagCoords[0], flagCoords[1]] = DisplayTileStatus.Unopened;
                    break;
            }
        }
        /// <summary>
        /// Find valid tile coordinates near a tile
        /// </summary>
        /// <param name="genIndex">Index of the coordinate of the tile</param>
        /// <returns>Valid indexes</returns>
        static int[] generateValidPatternIndexes(int genIndex)
        {
            int[] genCoords = { indexToCoords(genIndex)[0], indexToCoords(genIndex)[1] };
            List<int> validIndexes = new List<int>();

            foreach (int[] patternCoord in pattern)
            {
                int x = patternCoord[0] + genCoords[0]; // Global coordinate X
                int y = patternCoord[1] + genCoords[1]; // Global coordinate Y

                if ((x >= 0 && x < fieldSize[0]) && (y >= 0 && y < fieldSize[1])) // If coords are inside field
                    validIndexes.Add(coordsToIndex(new int[] { x, y }));
            }

            return validIndexes.ToArray();
        }
        /// <summary>
        /// Counts mines near an index
        /// </summary>
        /// <param name="index">Index of the coordinate</param>
        /// <returns>Number of mines near an index</returns>
        static int minesNearIndex(int index)
        {
            int[] validTilesNear = generateValidPatternIndexes(index);
            int numMines = 0;

            foreach (int patternIndex in validTilesNear)
                if (mineField[indexToCoords(patternIndex)[0], indexToCoords(patternIndex)[1]]) // If it is a mine tile - increment the counter
                    numMines++;

            return numMines;
        }
        /// <summary>
        /// Transforms coordinate index to 2d int array XY
        /// </summary>
        /// <param name="index">Coordinate index</param>
        /// <returns>X and Y</returns>
        static int[] indexToCoords(int index)
        {
            return new int[] { index % fieldSize[0], index / fieldSize[0] };
        }
        /// <summary>
        /// Transforms 2d int array XY into coordinate index to 
        /// </summary>
        /// <param name="coords">X and Y</param>
        /// <returns>Coordinate index</returns>
        static int coordsToIndex(int[] coords)
        {
            return coords[0] + coords[1] * fieldSize[0];
        }
    }
}
