using FunPlusEssentials.Essentials;
using FunPlusEssentials.Other;
using Il2Cpp;
using Il2CppSystem.Text.RegularExpressions;
using MelonLoader;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FunPlusEssentials.Patches
{
    [HarmonyLib.HarmonyPatch(typeof(MultiplayerChat), "HLIDELGJEON")]
    public static class MPChatPatch
    {
        //Prefix - Код ДО выполнения метода, Postfix - код ПОСЛЕ выполнения метода
        [HarmonyLib.HarmonyPrefix]
        static bool Prefix(ref string NAEFFPHCJKL, ref string NOFLIGCKLDF, ref string PBPBALMOMEM, MultiplayerChat __instance)
        {
            string name = NAEFFPHCJKL;
            if (!NAEFFPHCJKL.Contains(FPE.AppInfo.Name))
            {
                try
                {
                    PhotonPlayer sender = CommandHandler.GetSenderPlayer(NAEFFPHCJKL);
                    string c = Helper.GetProperty(sender, "nicknameColor").ToString();
                    if (sender.isMasterClient) { NAEFFPHCJKL = NAEFFPHCJKL.Remove(NAEFFPHCJKL.Length - 2) + " (master):"; }
                    if (c != "" && !Config.noRichText) { NAEFFPHCJKL = Helper.Paint(NAEFFPHCJKL, c); }
                }
                catch { }
            }
            if (Config.noRichText) NOFLIGCKLDF = Regex.Replace(NOFLIGCKLDF, "<.*?>", string.Empty);
            return !CommandHandler.HandleChat(name, NOFLIGCKLDF);
        }

        static void Postfix(ref string NAEFFPHCJKL, ref string NOFLIGCKLDF, ref string PBPBALMOMEM, MultiplayerChat __instance)
        {
            MelonLogger.Msg($"{Blacklist.Translit(NAEFFPHCJKL)} {Blacklist.Translit(NOFLIGCKLDF)}");
            string clearName = Regex.Replace(NAEFFPHCJKL, "<.*?>", string.Empty);
            string clearMsg = Regex.Replace(NOFLIGCKLDF, "<.*?>", string.Empty);
            CuteLogger.Log($"[Chat]{clearName} {clearMsg}");
        }
    }

    [HarmonyLib.HarmonyPatch(typeof(MultiplayerChat), "Start")]
    public static class MultiplayerChatStart
    {
        [HarmonyLib.HarmonyPrefix]
        static void Prefix(MultiplayerChat __instance)
        {
            __instance.JELNPHBIKHK.richText = true;
            CommandHandler.SystemMsg($" v{FPE.AppInfo.Version} was loaded.");
            if (FPE.AppInfo.UpdateAvailable) CommandHandler.SystemMsg($"Your version of the mod is outdated. It's recommended to install the latest version from our Discord ({FPE.AppInfo.DiscordLink})."); ;
        }
    }
}
