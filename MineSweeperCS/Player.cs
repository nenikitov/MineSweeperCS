using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MineSweeper {
    public class Player {
        #region Properties

        public bool IsDead { get; set; }
        public bool IsPlaying { get; set; }
        public int Position { get; set; }
        #endregion
    }
}
