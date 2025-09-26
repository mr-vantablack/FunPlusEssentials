using FunPlusEssentials.CustomContent;
using FunPlusEssentials.Essentials;
using Il2Cpp;
using MelonLoader;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FunPlusEssentials.Patches
{
    [HarmonyLib.HarmonyPatch(typeof(SurvivalMechanics), "BGHBEIGBGAA")]
    public static class NextWave
    {
        [HarmonyLib.HarmonyPostfix]
        static void Postfix(SurvivalMechanics __instance)
        {
            if (MapManager.isCustomMap && MapManager.currentMap.waves != null)
            {
                __instance.POJLLLGLPKL = MapManager.currentMap.waves[__instance.PKONLONPGNP].maxNPC;
            }
        }
    }
    [HarmonyLib.HarmonyPatch(typeof(SurvivalMechanics), "OnGUI")]
    public static class SurvivalMechanicsGUI
    {
        [HarmonyLib.HarmonyPrefix]
        static bool Prefix(SurvivalMechanics __instance)
        {
            if (HudHider.Instance != null)
            {
                if (HudHider.Instance.hidden)
                {
                    return false;
                }
            }
            return true;
        }
    }
    [HarmonyLib.HarmonyPatch(typeof(SurvivalMechanics), "Awake")]
    public static class SurvivalMechanicsAwake
    {
        [HarmonyLib.HarmonyPostfix]
        static void Postfix(SurvivalMechanics __instance)
        {
            MelonCoroutines.Start(MapManager.SetUpWaves(__instance));
        }
    }

    [HarmonyLib.HarmonyPatch(typeof(ShopSystem), "Awake")]
    public static class ShopSystemAwake
    {
        [HarmonyLib.HarmonyPostfix]
        static void Postfix(ShopSystem __instance)
        {
            if (MapManager.useCustomNPCs)
            {
                foreach (WeaponInfo weapon in CustomWeapons.customWeapons)
                {
                    if (weapon.shopAvailable)
                        __instance.JDCKBMLGDJD.Add(new ShopSystem.weapon() { name = weapon.name, price = weapon.shopPrice, bulletPrice = weapon.bulletDamage / 2, type = weapon.shopInfo });
                }
            }
        }
    }
}
