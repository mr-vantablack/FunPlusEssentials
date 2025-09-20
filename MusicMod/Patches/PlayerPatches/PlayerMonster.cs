using FunPlusEssentials.CustomContent;
using FunPlusEssentials.Other;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace FunPlusEssentials.Patches
{
    [HarmonyLib.HarmonyPatch(typeof(PlayerMonster), "Start")]
    public static class PlayerMonsterStart
    {
        static void Postfix(PlayerMonster __instance)
        {
            var data = __instance.photonView.instantiationData;
            if (data != null && data[0].ToString() == "CustomNPC")
            {
                CuteLogger.Meow("data " + data[0].ToString());
                NPCManager.SetUpCustomNPC(data[1].ToString(), __instance.gameObject, NPCType.PlayerMonster);
            }
        }
    }
    [HarmonyLib.HarmonyPatch(typeof(PlayerMonster), "Awake")]
    public static class ColliderFix2
    {
        [HarmonyLib.HarmonyPostfix]
        static void Postfix(PlayerMonster __instance)
        {
            if (__instance.DFLJPEDEMDH)
            {
                GameObject.Destroy(__instance.GetComponent<CharacterController>());
                GameObject.Destroy(__instance.GetComponent<CapsuleCollider>());
            }
        }
    }

    [HarmonyLib.HarmonyPatch(typeof(PlayerMonster), "KOFOOHFOGHL")]
    public static class ColliderFix
    {
        [HarmonyLib.HarmonyPostfix]
        static void Postfix(PlayerMonster __instance)
        {
            if (__instance.DFLJPEDEMDH)
            {
                GameObject.Destroy(__instance.GetComponent<CapsuleCollider>());
            }
        }
    }
}
