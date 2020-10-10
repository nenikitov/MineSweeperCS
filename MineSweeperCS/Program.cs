using System;
using System.Collections.Generic;
using System.Linq;

namespace MineSweeper {
    class Program {
        private static Dictionary<TileStatus, ConsoleColor> colors = new Dictionary<TileStatus, ConsoleColor> {
            { TileStatus.Unopened, ConsoleColor.Gray },
            { TileStatus.Opened, ConsoleColor.Yellow },
            { TileStatus.Flagged, ConsoleColor.Cyan },
            { TileStatus.Questionned,ConsoleColor.Magenta },
            { TileStatus.Mine,ConsoleColor.Red }
        };
        static void Main(string[] args) {
            // Get arguments
            (int w, int h, int d) = ParseArguments(args);

            #region Initialize Game

            MineSweeper mine = new MineSweeper(w, h, d, colors);
            Console.SetWindowSize(w * 2, h + 2);
            mine.PrintField();
            Console.ForegroundColor = ConsoleColor.White;
            Console.SetCursorPosition(0, h);
            Console.WriteLine("Mines: " + mine.Tiles.Where(t => t.IsMine).Count());
            #endregion

            #region Start Game

            mine.Player.IsPlaying = true;
            while(mine.Player.IsPlaying) {
                switch(Console.ReadKey().Key) {
                    case ConsoleKey.UpArrow:
                        mine.MoveCursor(-w);
                        break;
                    case ConsoleKey.DownArrow:
                        mine.MoveCursor(w);
                        break;
                    case ConsoleKey.LeftArrow:
                        mine.MoveCursor(-1);
                        break;
                    case ConsoleKey.RightArrow:
                        mine.MoveCursor(1);
                        break;
                    case ConsoleKey.Z:
                        mine.OpenTile(mine.Cursor);
                        break;
                    case ConsoleKey.X:
                        mine.FlagTile(mine.Cursor);
                        break;
                }
                mine.PrintField();
                Console.SetCursorPosition(0, h);
            }
            #endregion

            #region End Game

            Console.WriteLine(mine.Player.IsDead ? "\nDead" : "\nWin");
            Console.ReadLine();
            #endregion
        }

        static (int width, int height, int density) ParseArguments(string[] args, int w=10, int h=5, int d=15) {
            // Density
            if(args.Length > 0) if(!int.TryParse(args[0], out d)) Help();

            // Size
            if(args.Length > 1) if(!int.TryParse(args[1], out w)) Help(); else h = w;

            // Height
            if(args.Length > 2) if(!int.TryParse(args[2], out h)) Help();

            return (w, h, d);
        }

        static void Help() {
            Console.WriteLine("Mine sweeper, arrow keys to navigate, 'Z' to open tile, 'X' to flag tile");
            Console.WriteLine("Arguments: [<density>] [<width>] [<height>]");
            Console.WriteLine("\t<density> : Percentage of mines in the field (default: 15)");
            Console.WriteLine("\t<width> : Number of columns (default: 10)");
            Console.WriteLine("\t<height> : Number of rows (default: 5)");
            Console.WriteLine("\t\t height will be the same as width if not specified");
            Environment.Exit(1);
        }
    }
}
