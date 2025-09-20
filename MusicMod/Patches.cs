using System;
using FunPlusEssentials.Essentials;
using FunPlusEssentials.Other;
using FunPlusEssentials.CustomContent;
using UnityEngine;
using System.Text.RegularExpressions;
using FunPlusEssentials.Fun;
using UnityEngine.SceneManagement;
using MelonLoader;
using ExitGames.Client.Photon;
using System.Reflection;
using System.Collections.Generic;
using UnhollowerRuntimeLib;
using Harmony;
using System.Linq;
using UnhollowerBaseLib;
using static MelonLoader.MelonLogger;
using UnityEngine.UI;
using BeautifyEffect;
using UnityEngine.PostProcessing;
using CodeStage.AntiCheat.ObscuredTypes;
using static Il2CppSystem.Collections.Hashtable;

namespace FunPlusEssentials.Patches
{
    [HarmonyLib.HarmonyPatch(typeof(TPSCamera), "OnEnable")]
    public static class RemoveFilters
    {
        [HarmonyLib.HarmonyPostfix]
        static void Postfix(TPSCamera __instance)
        {
            if (!Config.filtersEnabled)
            {
                GameObject.Destroy(__instance.GetComponent<CameraFilterPack_Colors_Adjust_PreFilters>());
                GameObject.Destroy(__instance.GetComponent<CameraFilterPack_Blur_Focus>());
                GameObject.Destroy(__instance.GetComponent<CameraFilterPack_Film_Grain>());
                GameObject.Destroy(__instance.GetComponent<CameraFilterPack_Color_RGB>());
                CuteLogger.Quack("d1");
            }
            if (Plague.Enabled && PlagueController.Instance._playerClass.Infected)
            {
                var light = __instance.gameObject.GetComponent<Light>();
                if (light != null)
                {
                    GameObject.Destroy(light);
                }
                __instance.gameObject.AddComponent<InfectedVision>();
            }
        }
    }
    [HarmonyLib.HarmonyPatch(typeof(ToggleLight), "Start")]
    public static class InfectedFlashlightFix
    {
        [HarmonyLib.HarmonyPostfix]
        static void Postfix(TPSCamera __instance)
        {
            if (Plague.Enabled && PlagueController.Instance._playerClass.Infected)
            {
                GameObject.Destroy(__instance);
            }
            __instance.GetComponent<Light>().renderMode = LightRenderMode.ForcePixel;
        }
    }


    [HarmonyLib.HarmonyPatch(typeof(NetworkingPeer), "DebugReturn")]
    public static class OnConnectFailed
    {
        [HarmonyLib.HarmonyPostfix]
        static void Postfix(ref DebugLevel level, ref string message, NetworkingPeer __instance)
        {
            if (level == DebugLevel.ERROR && message.Contains("Receive issue. State:"))
            {
                CuteLogger.Bark("Failed to establish connection with ST3 servers.");
                if (ServerManager.customServers.Count != 0 && Config.customServersEnabled)
                {
                    ServerManager.AddCustomServers();
                }
            }
        }
    }

    [HarmonyLib.HarmonyPatch(typeof(WaitForDestroy), "Awake")]
    public static class WaitForDestroyAwake
    {
        [HarmonyLib.HarmonyPostfix]
        static void Postfix(WaitForDestroy __instance)
        {

        }
    }

    [HarmonyLib.HarmonyPatch(typeof(CharacterCustomization), "Awake")]
    public static class CharacterCustomizationAwake
    {
        [HarmonyLib.HarmonyPostfix]
        static void Postfix(CharacterCustomization __instance)
        {
            GameObject.Find("Scene Elements/PLAYER_MODEL").AddComponent<Rotator>().rotate = true;
        }
    }

    [HarmonyLib.HarmonyPatch(typeof(DrawPlayerName), "OnGUI")]
    public static class DrawPlayerNameGUI
    {
        [HarmonyLib.HarmonyPrefix]
        static bool Prefix(DrawPlayerName __instance)
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

    [HarmonyLib.HarmonyPatch(typeof(Bot), "OnGUI")]
    public static class BotGUI
    {
        [HarmonyLib.HarmonyPrefix]
        static bool Prefix(Bot __instance)
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
    [HarmonyLib.HarmonyPatch(typeof(DeadBot), "Start")]
    public static class DeadBotStart
    {
        static void Postfix(DeadBot __instance)
        {
            if (__instance.CPBLCBMLFAJ > 10f)
            {
                __instance.gameObject.transform.position += Vector3.up;
            }
        }
    }
    [HarmonyLib.HarmonyPatch(typeof(Bot), "Awake")]
    public static class BotAwake
    {
        static void Postfix(Bot __instance)
        {
            var data = __instance.photonView.instantiationData;
            if (data != null && data[0].ToString() == "CustomNPC")
            {
                CuteLogger.Meow("data " + data[0].ToString());
                NPCManager.SetUpCustomNPC(data[1].ToString(), __instance.gameObject, NPCType.Bot);
                if (__instance.KHKDJLKGHDM) { __instance.IDLHJOOAMIA.enabled = false; }
            }
        }
    }
    
    [HarmonyLib.HarmonyPatch(typeof(BossBot), "Awake")]
    public static class BossBotAwake
    {
        static void Postfix(BossBot __instance)
        {
            var data = __instance.photonView.instantiationData;
            if (data != null && data[0].ToString() == "CustomNPC")
            {
                CuteLogger.Meow("data " + data[0].ToString());
                NPCManager.SetUpCustomNPC(data[1].ToString(), __instance.gameObject, NPCType.BossBot);
            }
        }
    }
    
    [HarmonyLib.HarmonyPatch(typeof(CustardBot), "Awake")]
    public static class CustardBotAwake
    {
        static void Postfix(CustardBot __instance)
        {
            var data = __instance.photonView.instantiationData;
           // CuteLogger.Meow("data " + data[0].ToString());
            if (data[0].ToString() == "CustomNPC")
            {
                NPCManager.SetUpCustomNPC(data[1].ToString(), __instance.gameObject, NPCType.CustardBot);
            }
            else if (MapManager.isCustomMap && MapManager.currentMap.usingCustomNPCs)
            {
                PhotonNetwork.Destroy(__instance.gameObject);
            }
        }
    }
    [HarmonyLib.HarmonyPatch(typeof(BossBot), "OnGUI")]
    public static class BossBotGUI
    {
        [HarmonyLib.HarmonyPrefix]
        static bool Prefix(BossBot __instance)
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
  
    [HarmonyLib.HarmonyPatch(typeof(FPScontroller), "Update")]
    public static class FPScontrollerUpdate
    {
        [HarmonyLib.HarmonyPostfix]
        static void Postfix(FPScontroller __instance)
        {
            if (Helper.RoomMultiplayerMenu.KCIGKBNBPNN == "SBX")
            {
                if (Input.GetKeyDown(Config.proneKey))
                {
                    __instance.HKCDMBALAAK.canProne = true;
                    __instance.HKCDMBALAAK.proneHeight = 0.8f;
                    __instance.NDBHHAHAAKN = !__instance.NDBHHAHAAKN;
                }
            }
        }
    }

    [HarmonyLib.HarmonyPatch(typeof(ClassicMechanics), "EHPFKPFLPEO")]
    public static class ClassicMechanicsRestart
    {
        static void Postfix()
        {
            /*if (MapManager.isCustomMap)
            {
                if (MapManager.currentMap.usingCustomNPCs && Helper.RoomMultiplayerMenu.KCIGKBNBPNN == "COOP")
                {
                    var cm = Helper.Room.GetComponent<ClassicMechanics>();
                    int num = UnityEngine.Random.Range(0, cm.MDAPCMPEOOF.Length);
                    PhotonNetwork.NOOU2("COOP/Imposter", cm.MDAPCMPEOOF[num].transform.position, Quaternion.identity, 0, new Il2CppReferenceArray<Il2CppSystem.Object>(new Il2CppSystem.Object[] { "CustomNPC", MapManager.currentMap.monsters[0] })).tag = "monster";
                }
            }*/
        }
    }
    [HarmonyLib.HarmonyPatch(typeof(ClassicMechanics), "Start")]
    public static class ClassicMechanicsAwake
    {
        [HarmonyLib.HarmonyPrefix]
        static void Prefix(ClassicMechanics __instance)
        {

            if (MapManager.isCustomMap)
            {
                __instance.BCCHIGOBOFJ[0].name = MapManager.currentMap.monsters[0];
                __instance.FKEIPFJBJHP = MapManager.currentMap.monsters[1];
            }
        }
    }

    [HarmonyLib.HarmonyPatch(typeof(WeaponManager), "Awake")]
    public static class WeaponManagerAwake
    {
        [HarmonyLib.HarmonyPrefix]
        static void Prefix(WeaponManager __instance)
        {
            var s = GameObject.Find("__Room(Clone)");
            if (s != null) s.name = "__Room";
        }
    }

    [HarmonyLib.HarmonyPatch(typeof(FlyCamController), "Start")]
    public static class FlyCamStart
    {
        [HarmonyLib.HarmonyPrefix]
        static void Prefix(FlyCamController __instance)
        {
            if (__instance.photonView.isMine && !GameObject.FindObjectOfType<TheatreMechanics>().enabled)
            {
                __instance.gameObject.AddComponent<FlyCamControllerAddon>();
            }
        }
    }
    [HarmonyLib.HarmonyPatch(typeof(TrueCameraController), "Awake")]
    public static class TrueCameraControllerAwake
    {
        static void Postfix(TrueCameraController __instance)
        {
            if (!Config.postProcessingEnabled)
            {
                GameObject.Destroy(__instance.transform.GetComponentInChildren<BeautifyEffect.Beautify>());
                GameObject.Destroy(__instance.transform.GetComponentInChildren<PostProcessingBehaviour>());
            }
        }
    }
   
}
