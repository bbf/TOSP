using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Terraria.TOSUtil;
using System.Xml.Serialization;
using System.IO;
using Permissions;
using System.Runtime.InteropServices;

namespace Terraria
{
    public class RegionProtect : Plugin
    {
        // Enums / Consts
        private const string CONFIG_NAME = "ProtectedRegions.xml";
        enum PlayerState { SELECTING_CORNER_A, SELECTING_CORNER_B };

        // Permissions
        const string PERMISSION_USE = "regionprotect.use";
        const string PERMISSION_ADMIN = "regionprotect.admin";
        const string PERMISSION_BIGREGIONS = "regionprotect.bigregions";

        // Internals
        List<ProtectedRegion> regions;
        XmlSerializer configSerializer;
        Dictionary<Player, PlayerState> playerState;
        Dictionary<Player, Vector2> playerCornerA;


        public RegionProtect()
        {
            configSerializer = new XmlSerializer(typeof(List<ProtectedRegion>));
            playerState = new Dictionary<Player, PlayerState>();
            playerCornerA = new Dictionary<Player, Vector2>();
        }

        public void WriteLog(string str)
        {
            Console.WriteLine("[{0}] {1}", pluginName, str);
        }

        public void WisperPlayer(Player player, string message)
        {
            message = String.Format("[{0}] {1}", pluginName, message);
            player.WisperMessage(message);
        }

        public override void Initialize()
        {
            // Init plugin fields
            pluginName = "RegionProtect";
            pluginDescription = "Region Protector";
            pluginVersion = "v1.0";
            pluginAuthor = "BBF";

            // Register hooks
            this.registerHook(Hook.PLAYER_COMMAND);
            this.registerHook(Hook.TILE_CHANGE);
            this.registerHook(Hook.PLAYER_JOIN);

            // Initialization
            LoadRegions();
        }

        public override void Unload()
        {
            SaveRegions();
            WriteLog("Unloaded");
        }

        public override void onTileChanged(TileEvent ev)
        {
            Tile tile = ev.getTile();

            // Allow some tiles to be destroyed.
            // TODO Extend this list with more tile types
            switch (TileUtil.getType(tile))
            {
                case TileType.Vines:
                case TileType.Plants:
                case TileType.Plants2:
                    //case TileType.Plants3:
                    return;
            }

            Player player = ev.getPlayer();
            if (player == null)
            {
                return;
            }


            // Handle if player is currently defining/inspecting a region
            if (playerState.ContainsKey(player))
            {
                PlayerState state = playerState[player];
                Vector2 position = new Vector2(tile.mX, tile.mY);
                if (state == PlayerState.SELECTING_CORNER_A)
                {
                    onSelectCornerA(player, position);
                }
                else
                {
                    onSelectCornerB(player, position);
                }
                return;
            }

            // Allow admins to always change the terrain
            if (player.hasPermissions(PERMISSION_ADMIN))
            {
                return;
            }

            // Check if this change was inside a region
            ProtectedRegion region = GetRegion(new Vector2(tile.mX, tile.mY));
            if (region == null)
            {
                return;
            }


            // Allow member to modify this region
            if (region.Owner == player.name || region.Members.Contains(player.name))
            {
                return;
            }

            //WriteLog(String.Format("[{0}] tried to destroy tile: {1} on protected region.", player.name, TileUtil.getType(tile)));

            if (region.Owner != null)
            {
                WisperPlayer(player, String.Format("This region is owned by: {0}", region.Owner));
            }
            else
            {
                WisperPlayer(player, "This region is protected by the server");
            }
            ev.setState(true);

        }


        public override void onPlayerCommand(CommandEvent ev)
        {
            // Check if this command was for us
            string[] cmds = ev.getCommandArray();
            if (cmds[0] != "/region")
            {
                return;
            }

            Player player = ev.getPlayer();
            ev.setState(true);

            // Always check if the player is registered, and loggged in
            if (AuthManager.hasAccount(player) == false || AuthManager.isLoggedIn(player) == false)
            {
                if (player.hasPermissions(PERMISSION_USE))
                {
                    WisperPlayer(player, "Region commands are only available to players who are logged in.");
                }
                return;
            }

            // Only allow players to use the command if the server admin gave the user permission
            if (player.hasPermissions(PERMISSION_USE) == false)
            {
                return;
            }


            if (cmds.Length < 2)
            {
                PrintCommandHelp(player);
                return;
            }

            switch (cmds[1])
            {
                case "help":
                    PrintCommandUsage(player);
                    return;

                case "create":
                    onCreateRegion(player);
                    return;

                case "cancel":
                    onCreateCancel(player);
                    return;

                case "delete":
                    onDeleteRegion(player);
                    return;

                case "list":
                    onListRegion(player);
                    return;

                case "invite":
                    onRegionInvite(player, cmds[2]);
                    return;

                case "ban":
                    onRegionBan(player, cmds[2]);
                    return;

                // TODO: Implement inspect
                //case "inspect":
                //    return;


                default:
                    PrintCommandHelp(player);
                    return;
            }
        }

        public override void onPlayerJoin(PlayerEvent ev)
        {
            Player player = ev.getPlayer();
            CancelPlayerState(player);

            ProtectedRegion region = GetRegion(player.name);
            if (region == null || player.hasPermissions(PERMISSION_USE))
            {
                WisperPlayer(player, "This server offers region protection. After logging in type: /region help");
            }

        }

        private void PrintCommandHelp(Player player)
        {
            WisperPlayer(player, "Invalid command, type \"/region help\" for help.");
        }

        private void PrintCommandUsage(Player player)
        {
            WisperPlayer(player, String.Format(" Version {0} by {1} - Help:", pluginVersion, pluginAuthor));
            //WisperPlayer(player, "* /region help - shows this help"); // Too Obvious?
            WisperPlayer(player, "* /region create - creates your protected region");
            WisperPlayer(player, "* /region cancel - cancel the region creation/inspection");
            WisperPlayer(player, "* /region delete - deletes your protected region");
            WisperPlayer(player, "* /region list - list members of your region");
            WisperPlayer(player, "* /region invite <name> - invites player to your region");
            WisperPlayer(player, "* /region ban <name> - bans player from your region");
            if (player.hasPermissions(PERMISSION_ADMIN))
            {
                // TODO:
                //WisperPlayer(player, "* /region inspect - inspect a region for owners (admin)");
                //WisperPlayer(player, "* /region screate <region name> - creates a server protected region (admin)");
                //WisperPlayer(player, "* /region sdelete <region name> - deletes a server protected region (admin)");
                //WisperPlayer(player, "* /region slist <region name> - list members of a server protected region (admin)");
                //WisperPlayer(player, "* /region sinvite <region name> <name> - invites players to a server protected region (admin)");
                //WisperPlayer(player, "* /region sban <name> - bans player from a server protected region (admin)");
            }
        }


        private void onCreateRegion(Player player)
        {
            ProtectedRegion region = GetRegion(player.name);
            if (region != null)
            {
                WisperPlayer(player, "You are already an owner of a region, and you must first delete it before you can create a new one.");
                return;
            }

            WisperPlayer(player, "Creating a new protected region. To define to top left corner of your region, please place a block, or remove one.");
            playerState.Add(player, PlayerState.SELECTING_CORNER_A);
        }

        private void onCreateCancel(Player player)
        {
            CancelPlayerState(player);
            WisperPlayer(player, "Region creation cancelled.");
        }

        private void onSelectCornerA(Player player, Vector2 cornerA)
        {
            ProtectedRegion region = GetRegion(cornerA, 3, 3);
            if (region != null)
            {
                if (region.Owner != null)
                {
                    WisperPlayer(player, String.Format("This corner is inside/too close to a region owned by: {0}", region.Owner));
                }
                else
                {
                    WisperPlayer(player, "This corner is inside/too close to a region protected by the System");
                }
                return;
            }

            playerState[player] = PlayerState.SELECTING_CORNER_B;
            playerCornerA.Add(player, cornerA);
            WisperPlayer(player, "Great, now to define the bottom right corner of your region, please place another block, or remove another one.");
        }

        private void onSelectCornerB(Player player, Vector2 cornerB)
        {
            ProtectedRegion region = GetRegion(cornerB, 3, 3);
            if (region != null)
            {
                if (region.Owner != null)
                {
                    WisperPlayer(player, String.Format("This corner is inside/too close to a region owned by: {0}", region.Owner));
                }
                else
                {
                    WisperPlayer(player, "This corner is inside/too close to a region protected by the System");
                }
                return;
            }

            Vector2 cornerA = playerCornerA[player];

            // TODO Move these to a configuration property
            int minX = 5;
            int minY = 4;
            int maxX = 20;
            int maxY = 10;

            int dX = (int)Math.Abs(cornerA.X - cornerB.X);
            int dY = (int)Math.Abs(cornerA.Y - cornerB.Y);

            if (dX < minX || dY < minY)
            {
                WisperPlayer(player, String.Format("Your region is too small ({0}x{1}), it must be at least ({2},{3}), try to place that corner further away.", dX, dY, minX, minY));
                return;
            }

            if ((dX > maxX || dY > maxY) && !(Permissions.hasPermissions(player, PERMISSION_BIGREGIONS)))
            {
                WisperPlayer(player, String.Format("Your region is too big ({0}x{1}), it must be at most ({2},{3}), try to place that corner closer apart.", dX, dY, maxX, maxY));
                return;
            }

            WisperPlayer(player, String.Format("Congratulations, you region ({0}x{1}) has been created. You can now invite your friends to join it if you wish.", dX, dY));
            region = new ProtectedRegion(player.name, cornerA, cornerB);
            regions.Add(region);

            CancelPlayerState(player);
            SaveRegions();
        }

        private void CancelPlayerState(Player player)
        {
            if (playerCornerA.ContainsKey(player))
            {
                playerCornerA.Remove(player);
            }
            if (playerState.ContainsKey(player))
            {
                playerState.Remove(player);
            }
        }


        private void LoadRegions()
        {
            try
            {
                regions = null;
                FileStream fileStream = new FileStream(CONFIG_NAME, FileMode.Open);
                regions = (List<ProtectedRegion>)configSerializer.Deserialize(fileStream);
                WriteLog("Loaded " + regions.Count + " regions from config.");
            }
            catch (FileNotFoundException)
            {
            }

            if (regions == null)
            {
                WriteLog("No regions found on config, starting from scratch.");
                regions = new List<ProtectedRegion>();
            }
        }

        private void SaveRegions()
        {
            try
            {
                FileStream fileStream = new FileStream(CONFIG_NAME, FileMode.Create);
                configSerializer.Serialize(fileStream, regions);
                WriteLog("Wrote " + regions.Count + " regions to config.");
                return;
            }
            catch (Exception)
            {
            }
            WriteLog("Could not save regions to file.");
        }

        private void onDeleteRegion(Player player)
        {
            ProtectedRegion region = GetRegion(player.name);
            if (region == null)
            {
                WisperPlayer(player, "You don't have a region defined.");
                return;
            }

            regions.Remove(region);
            SaveRegions();

            WisperPlayer(player, "Your region has been deleted.");
        }

        private void onListRegion(Player player)
        {
            ProtectedRegion region = GetRegion(player.name);
            if (region == null)
            {
                WisperPlayer(player, "You don't have a region defined.");
                return;
            }

            if (region.Members.Count < 1)
            {
                WisperPlayer(player, "You have no players registered on your region.");
                return;
            }

            string members = String.Join(", ", region.Members);
            WisperPlayer(player, String.Format("The following players are registered on your region: {0}", members));
        }

        private void onRegionInvite(Player player, string victim)
        {
            ProtectedRegion region = GetRegion(player.name);
            if (region == null)
            {
                WisperPlayer(player, "You don't have a region defined.");
                return;
            }

            if (region.Members.Contains(victim) == true)
            {
                WisperPlayer(player, String.Format("{0} is already a member of your region.", victim));
                return;
            }

            if (AuthManager.Auths.ContainsKey(victim) == false)
            {
                WisperPlayer(player, String.Format("{0} is not a registered member.", victim));
                return;
            }

            region.Members.Add(victim);
            SaveRegions();

            WisperPlayer(player, String.Format("{0} is now a member of your region.", victim));
        }

        private void onRegionBan(Player player, string victim)
        {
            ProtectedRegion region = GetRegion(player.name);
            if (region == null)
            {
                WisperPlayer(player, "You don't have a region defined.");
                return;
            }

            if (region.Members.Contains(victim) == false)
            {
                WisperPlayer(player, String.Format("{0} was not a member of your region.", victim));
                return;
            }

            region.Members.Remove(victim);
            SaveRegions();

            WisperPlayer(player, String.Format("{0} is now banned from your region.", victim));
        }


        private ProtectedRegion GetRegion(string playerName)
        {
            // TODO Maybe optimize the region lookup method to something more efficient
            foreach (ProtectedRegion region in regions)
            {
                if (region.Owner == playerName)
                {
                    return region;
                }
            }
            return null;
        }

        private ProtectedRegion GetRegion(Vector2 position, int borderX = 0, int borderY = 0)
        {
            foreach (ProtectedRegion region in regions)
            {
                if (region.Contains(position.X, position.Y, borderX, borderY))
                {
                    return region;
                }
            }
            return null;
        }

    }


}
