using FunPlusEssentials.CustomContent;
using FunPlusEssentials.Essentials;
using FunPlusEssentials.Other;
using MelonLoader;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FunPlusEssentials.Patches
{
    [HarmonyLib.HarmonyPatch(typeof(LadderPlayer), "Start")]
    public static class OnPlayerSpawned2
    {
        [HarmonyLib.HarmonyPostfix]
        static void Postfix(LadderPlayer __instance)
        {

        }
    }
    [HarmonyLib.HarmonyPatch(typeof(PlayerNetworkController), "Awake")]
    public static class OnPlayerSpawned4
    {
        static void Postfix(PlayerNetworkController __instance)
        {
            if (MapManager.useCustomNPCs)
            {
                MelonCoroutines.Start(CustomWeapons.InstantiateFPSWeapons(__instance.gameObject));
            }
            if (Plague.Enabled)
            {
                MelonCoroutines.Start(PlagueAssets.Instance.InstantiateFPSWeapons(__instance.gameObject));
            }
        }
    }
    [HarmonyLib.HarmonyPatch(typeof(PlayerNetworkController), "BCPFIMDIMJE")]
    public static class AntiCrashPatch
    {
        [HarmonyLib.HarmonyPrefix]
        static bool Prefix(ref string JMOEKIHCLBH, ref float FADNANLHJHF, PlayerNetworkController __instance)
        {
            if (Config.securityEnabled)
            {
                if (FADNANLHJHF > 100)
                {
                    CuteLogger.Meow(ConsoleColor.Red, "[AntiCheat] Crash attempt.");
                    return false;
                }
                else return true;
            }
            return true;
        }
        [HarmonyLib.HarmonyPostfix]
        static void Postfix(ref string JMOEKIHCLBH, ref float FADNANLHJHF, PlayerNetworkController __instance)
        {

        }
    }
}
