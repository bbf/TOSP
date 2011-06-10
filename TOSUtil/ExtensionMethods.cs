using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Terraria.TOSUtil
{
    public static class ExtensionMethods
    {

        public static void WisperMessage(this Player player, string message)
        {
            // Break up messages longer then 60 chars at space characters.
            while (message.Length > 0)
            {
                if (message.Length < 81)
                {
                    player.sendMessage(message);
                    message = "";
                }
                else
                {
                    int firstSpace = message.IndexOf(' ', 80);
                    string section = message.Substring(0, firstSpace);
                    message = message.Substring(firstSpace + 1);
                    player.sendMessage(section);
                }

            }
        }
    }
}
