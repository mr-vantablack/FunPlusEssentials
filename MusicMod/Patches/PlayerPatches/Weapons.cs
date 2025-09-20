using FunPlusEssentials.Essentials;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace FunPlusEssentials.Patches
{
    [HarmonyLib.HarmonyPatch(typeof(WeaponPickUp), "Start")]
    public static class WeaponPickUpStart
    {
        [HarmonyLib.HarmonyPrefix]
        static void Prefix(WeaponPickUp __instance)
        {

        }
    }
    //KHCH - knife, GHLH -shotgun
    [HarmonyLib.HarmonyPatch(typeof(WeaponScript), "Update")]
    public static class WeaponScriptAimPatch
    {
        static void Prefix(WeaponScript __instance)
        {
            //if (Plague.Enabled) __instance.GKNFEDOCILC = new Quaternion(0f, 0f, 0f, 0f);
            if (__instance.FLIAJIAOBHA.aimPosition == Vector3.zero)
            {
                __instance.BHMOCCGGODE = false;

            }
        }
    }
    [HarmonyLib.HarmonyPatch(typeof(WeaponScript), "Start")]
    public static class WeaponScriptStart
    {
        [HarmonyLib.HarmonyPostfix]
        static void Postfix(WeaponScript __instance)
        {
            //if (Plague.Enabled) __instance.GKNFEDOCILC = new Quaternion(0f, 0f, 0f, 0f);
            if (Config.fov > 65 && Config.fov <= 120)
            {
                var cam = GameObject.FindObjectOfType<Flashlight>().gameObject.GetComponent<Camera>();
                if (cam != null) { cam.fieldOfView = Config.fov - 10; }
                __instance.FFGIIOODMGK = Config.fov;

            }
        }
    }
}
