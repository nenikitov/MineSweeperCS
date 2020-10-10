using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MineSweeper {
    public enum TileStatus {
        Unopened = '*',
        Opened = '_',
        Flagged = 'P',
        Questionned = '?',
        Mine = 'X'
    };

    public class Tile {
        #region Properties

        /// <summary>
        /// Current status of the tile, can be converted to <see cref="char"/> implictly
        /// </summary>
        public TileStatus Status { get; set; }
        /// <summary>
        /// Indicates if the tile is a mine, default value is false
        /// </summary>
        public bool IsMine { get; set; }
        /// <summary>
        /// Number of adjacent mines, maximum 8, default value is 0
        /// </summary>
        public int AdjacentMinesCount { get; set; }
        #endregion

        #region Contructor

        public Tile(TileStatus status) {
            this.Status = status;
        }
        #endregion

        #region Public Methods

        public void Open() {
            this.Status = this.IsMine?TileStatus.Mine:TileStatus.Opened;
        }

        public void Flag() {
            switch(this.Status) {
                case TileStatus.Unopened:
                    this.Status = TileStatus.Flagged;
                    break;
                case TileStatus.Flagged:
                    this.Status = TileStatus.Questionned;
                    break;
                case TileStatus.Questionned:
                    this.Status = TileStatus.Unopened;
                    break;
            }
        }
        #endregion
    }
}
