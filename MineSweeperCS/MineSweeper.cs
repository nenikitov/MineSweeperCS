using System;
using System.Collections.Generic;
using System.Linq;

namespace MineSweeper {

    public class MineSweeper {
        #region Properities

        public Tile[] Tiles { get; set; }
        public int Width { get; set; }
        public int Cursor { get; set; }
        public Dictionary<TileStatus, ConsoleColor> Colors { get; set; }
        public Player Player { get; set; }

        private readonly int[] delta;
        #endregion

        #region Constructor

        public MineSweeper(int width, int height, int density, Dictionary<TileStatus, ConsoleColor> colors) {
            this.Width = width;
            this.Colors = colors;
            this.Cursor = 0;

            // Initialize 'this.Tiles' and fill it with unopened tiles
            this.Tiles = Enumerable.Range(0, width * height)
                .Select(_ => new Tile(TileStatus.Unopened))
                .ToArray();


            // Initialize player
            this.Player = new Player();

            // Put the mines in the field
            this.delta = new int[] {
                -this.Width - 1 , -this.Width,  -this.Width + 1,
                -1, 1,
                this.Width - 1, this.Width, this.Width + 1
            };
            this.GenerateMines(this.Tiles.Length * density / 100);
        }
        #endregion

        #region Public Methods

        public void PrintField() {
            Tile t;

            Console.SetCursorPosition(0, 0);
            for(int i = 0; i < this.Tiles.Length; i++) {
                t = this.Tiles[i];
                // Minus 8 to print the darker version of the color
                // expect Gray (Gray = 7, DarkGray = 8)
                this.PrintTile(t, this.Colors[t.Status] == ConsoleColor.Gray ? 1 : -8);

                // if at last column
                if((i + 1) % this.Width == 0) {
                    Console.WriteLine();
                }
            }

            // Highlight the tile under cursor
            Console.SetCursorPosition(this.Cursor % this.Width, this.Cursor / this.Width);
            this.PrintTile(this.Tiles[this.Cursor]);
        }

        public void MoveCursor(int offset) {
            // Similar to 'this.SearchAdjacent'

            if(this.Cursor % this.Width == 0 && offset == -1) return;
            if((this.Cursor + 1) % this.Width == 0 && offset == 1) return;

            this.Cursor += offset;

            if(this.Cursor < 0 || this.Cursor >= this.Tiles.Length) this.Cursor -= offset;
        }

        public void OpenTile(int i) {
            int[] delta = { -this.Width, -1, 1, this.Width };
            // Can't use List<Tile> because it copies the values
            List<int> ts = new List<int> { i };
            int cur;
            while(ts.Count > 0) {
                cur = ts[0];
                ts.RemoveAt(0);
                this.Tiles[cur].Open();
                if(this.Tiles[cur].Status == TileStatus.Mine) {
                    // DEATH !!
                    this.Player.IsDead = true;
                    this.Player.IsPlaying = false;
                    return;
                }

                // if only mine tiles are left
                if(!this.Tiles.Any(t => !t.IsMine && t.Status == TileStatus.Unopened)) {
                    // Victory
                    this.Player.IsPlaying = false;
                    return;
                }

                if(this.Tiles[cur].AdjacentMinesCount == 0) {
                    // Add nearby tiles to list to open
                    ts.AddRange(this.SearchAdjacent(cur)
                        .Where(ind => !this.Tiles[ind].IsMine && this.Tiles[ind].Status == TileStatus.Unopened));
                }
            }
        }

        public void FlagTile(int i) {
            this.Tiles[i].Flag();
        }
        #endregion

        #region Private Methods

        /// <summary>
        /// Search through the adjacent tiles of a tile
        /// </summary>
        /// <param name="i">Index of tile</param>
        /// <param name="action">Action to do to the adjacent tiles</param>
        private IEnumerable<int> SearchAdjacent(int i) {
            foreach(int d in this.delta) {
                // if i is at first column and i+d is at last column
                if(i % this.Width == 0 && (i + d) % this.Width == this.Width - 1) continue;
                // if i is at last column and i+d is at first column
                if((i + 1) % this.Width == 0 && (i + d) % this.Width == 0) continue;
                // if outside the array
                if(i + d < 0 || i + d >= this.Tiles.Length) continue;

                yield return i+d;
            }
        }

        /// <summary>
        /// Generate random array of indexes where the mine is
        /// </summary>
        /// <param name="numMines">Number of mines to create</param>
        private void GenerateMines(int numMines) {
            Random r = new Random();
            // Generate array of all indexes
            int[] mines = Enumerable.Range(0, this.Tiles.Length)
                // Shuffle them
                .OrderBy(_ => r.Next())
                // Choose the first {numMines} 
                .Where((_, i) => i < numMines).OrderBy(i => i)
                // Convert to int[]
                .ToArray();

            // Add mines
            for(int i = 0; i < this.Tiles.Length; i++) {
                this.Tiles[i].IsMine = mines.Contains(i);
            }

            // Count adjacent mines

            for(int i = 0; i < this.Tiles.Length; i++) {
                this.Tiles[i].AdjacentMinesCount = this.SearchAdjacent(i)
                    .Where(ind => this.Tiles[ind].IsMine)
                    .Count();
            }
        }

        /// <summary>
        /// Customizable method to print a tile
        /// </summary>
        /// <param name="tile"></param>
        private void PrintTile(Tile tile, int colorOffset = 0) {
            Console.ForegroundColor = this.Colors[tile.Status] + colorOffset;
            // Print number if opened and num > 0
            Console.Write(tile.Status == TileStatus.Opened && tile.AdjacentMinesCount != 0
                ? tile.AdjacentMinesCount.ToString()
                : ((char)tile.Status).ToString());
        }
        #endregion
    }
}
