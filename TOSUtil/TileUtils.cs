using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Terraria.TOSUtil
{
    /*
     * Credits go to: http://terrariaworldviewer.codeplex.com/
     */
    public enum TileType : byte
    {
        Dirt = 0,
        Stone,
        Grass,
        Plants,
        Torches,
        Trees,
        Iron,
        Copper,
        Gold,
        Silver,
        Door1,
        Door2,
        Heart,
        Bottles,
        Table,
        Chair,
        Anvil,
        Furnance,
        CraftingTable,
        WoodenPlatform,
        PlantsDecorative,
        Chest,
        CorruptionStone1,
        CorruptionGrass,
        CorruptionPlants,
        CorruptionStone2,
        Altar,
        Sunflower,
        Pot,
        PiggyBank,
        BlockWood,
        ShadowOrb,
        CorruptionVines,
        Candle,
        ChandlerCopper,
        ChandlerSilver,
        ChandlerGold,
        Meterorite, // Credit Vib Rib
        BlockStone,
        BlockRedStone,
        Clay,
        BlockBlueStone,
        LightGlobe,
        BlockGreenStone,
        BlockPinkStone,
        BlockGold,
        BlockSilver,
        BlockCopper,
        Spikes,
        CandleBlue,
        Books,
        Web,
        Vines,
        Sand,
        Glass,
        Signs,
        Obsidian,
        Ash, // Credit Infinite Monkeys
        Hellstone, // Credit Vib Rib
        Mud,
        UndergroundJungleGrass,
        UndergroundJunglePlants,
        UndergroundJungleVines,
        Sapphire,
        Ruby,
        Emerald,
        Topaz,
        Amethyst,
        Diamond,
        UndergroundJungleThorns, // Credit Dr VideoGames 0031
        UndergroundMushroomGrass,
        UndergroundMushroomPlants,
        UndergroundMushroomTrees,
        Plants2,
        Plants3,
        BlockObsidian,
        BlockHellstone,
        UnderworldFurnance,
        DecorativePot,
        Bed,

        Unknown = 255,
    };

    public class TileUtil
    {
        public static TileType getType(Tile tile)
        {
            if (tile.type == 255)
            {
                // This should never happen
                Console.WriteLine("## WARNING ## Tile type is 255!");
            }

            if (Enum.IsDefined(typeof(TileType), tile.type))
            {
                return (TileType)Enum.ToObject(typeof(TileType), tile.type);
            }

            // This should never happen as well
            Console.WriteLine("Unknown Tile changed: " + tile.type);
            return TileType.Unknown;
        }


    }
}
