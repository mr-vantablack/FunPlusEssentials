using FunPlusEssentials.CustomContent;
using FunPlusEssentials.Other;
using MelonLoader;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.UI;
using UnityEngine;
using Il2Cpp;

namespace FunPlusEssentials.Patches
{
    [HarmonyLib.HarmonyPatch(typeof(Volume), "Awake")]
    public static class VolumeStart
    {
        static void Postfix(Volume __instance)
        {
            if (PhotonNetwork.isMasterClient)
            {
                __instance.POJLLLGLPKL = 100;
            }
            else
            {
                __instance.POJLLLGLPKL = 50;
            }
            if (MapManager.useCustomNPCs)
            {
                __instance.transform.GetComponentInChildren<CanvasScaler>().screenMatchMode = CanvasScaler.ScreenMatchMode.Expand;
                MelonCoroutines.Start(NPCManager.AddNPCInfos(__instance));

                CustomWeapons.AddWeaponsToCatagory();
            }
        }
    }
    [HarmonyLib.HarmonyPatch(typeof(Volume), "SendOption")]
    public static class VolumeSendOption
    {
        [HarmonyLib.HarmonyFinalizer]
        static Exception Finalizer(Exception __exception)
        {
            if (__exception != null)
            {
                if (Helper.RoomMultiplayerMenu.CNLHJAICIBH == null)
                {
                    var c = GameObject.FindObjectOfType<BossCam>();
                    if (c != null)
                    {
                        if (c.INHFFFAKNNG != null)
                        {
                            Helper.RoomMultiplayerMenu.CNLHJAICIBH = c.INHFFFAKNNG.gameObject;
                        }
                        else if (c.OMIOOOFAJNP != null)
                        {
                            Helper.RoomMultiplayerMenu.CNLHJAICIBH = c.OMIOOOFAJNP.gameObject;
                        }
                    }
                }
            }
            return null;
        }
    }
}
