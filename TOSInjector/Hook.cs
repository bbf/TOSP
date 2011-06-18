using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using Terraria;

namespace TOSP
{
    public class TileChangeEvent : CancelEventArgs
    {
        public Tile tile { get; protected set; }
        public Player player { get; protected set; }

        public TileChangeEvent(Tile tile, Player player)
        {
            this.tile = tile;
            this.player = player;
        }

    }

    public delegate void TileChangeHandler(TileChangeEvent e);

    public class Hook
    {

        public static event TileChangeHandler OnTileChange;

        public static bool OnTileChangeHook(Int32 tileX, Int32 tileY, Int32 playerId, Byte type, Boolean fail)
        {
            Console.WriteLine("Tile at {0}x{1} ({3} : {4}) was changed by player {2}", tileX, tileY, playerId, type, fail);

            Tile tile = Main.tile[tileX, tileY];
            Player player = Main.player[playerId];

            TileChangeEvent e = new TileChangeEvent(tile, player);
            if (OnTileChange != null)
            {
                OnTileChange(e);
            }

            e.Cancel = true;

            return e.Cancel;
        }

    }
}
