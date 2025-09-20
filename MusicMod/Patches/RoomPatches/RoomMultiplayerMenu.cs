using CodeStage.AntiCheat.ObscuredTypes;
using FunPlusEssentials.CustomContent;
using FunPlusEssentials.Essentials;
using FunPlusEssentials.Fun;
using FunPlusEssentials.Other;
using MelonLoader;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnhollowerBaseLib;
using UnityEngine.SceneManagement;
using UnityEngine;

namespace FunPlusEssentials.Patches
{
    [HarmonyLib.HarmonyPatch(typeof(RoomMultiplayerMenu), "FixedUpdate")]
    public static class RoomMultiplayerMenuFixedUpdate
    {
        [HarmonyLib.HarmonyPrefix]
        static bool Prefix(RoomMultiplayerMenu __instance)
        {
            if (Plague.Enabled) return false;
            else return true;
        }
        [HarmonyLib.HarmonyPostfix]
        static void Postfix(RoomMultiplayerMenu __instance)
        {
            if (RMMFix.Instance != null) __instance.HGKEEDKPGOD = RMMFix.Instance.leadingPlayer;
            //Msg("yes");
        }
    }
    [HarmonyLib.HarmonyPatch(typeof(RoomMultiplayerMenu), "OnGUI")]
    public static class RoomMultiplayerMenuOnGUI
    {
        [HarmonyLib.HarmonyPrefix]
        static bool Prefix(RoomMultiplayerMenu __instance)
        {
            if (Plague.Enabled) return false;
            else return true;
        }
    }

    [HarmonyLib.HarmonyPatch(typeof(RoomMultiplayerMenu), "Awake")]
    public static class RoomMultiplayerMenuAwake
    {
        public delegate void Event(RoomMultiplayerMenu instance);
        public static event Event onRoomSpawned;

        static void Prefix(RoomMultiplayerMenu __instance)
        {
            if (PhotonNetwork.room.customProperties["Plague"] != null)
            {
                Plague.Enabled = true;
            }
            if (Plague.Enabled)
            {
                __instance.gameObject.AddComponent<PlagueController>();
                __instance.KCIGKBNBPNN = "INF";
            }
        }
        static void Postfix(RoomMultiplayerMenu __instance)
        {
            onRoomSpawned?.Invoke(__instance);
            if (MapManager.isCustomMap)
            {
                Helper.Room.GetComponent<AudioSource>().clip = MapManager.currentMap.ambient;
                Helper.Room.GetComponent<AudioSource>().Play();
            }
            Helper.Room.AddComponent<MusicPlayer>();
            Helper.Room.AddComponent<HudHider>();
            Helper.Room.AddComponent<FunRPCHandler>();
            if (Plague.Enabled)
            {
                __instance.PPMMEFIMPGL = new Il2CppReferenceArray<ObscuredString>(new ObscuredString[1] { "Knife" });
            }
            if (Config.scoreboardEnabled)
            {
                Helper.Room.AddComponent<RMMFix>();
                Helper.Room.AddComponent<TabMenu>();
            }
        }
    }
    [HarmonyLib.HarmonyPatch(typeof(RoomMultiplayerMenu.ELNOHBJFICO), "MoveNext")]
    public static class RoundEnd
    {
        public delegate void Event();
        public static event Event onRoundEnded;
        [HarmonyLib.HarmonyPostfix]
        static void Postfix()
        {
            string gm = Helper.RoomMultiplayerMenu.KCIGKBNBPNN;
            if (gm == "SUR" || gm == "COOP" || gm == "VS")
            {
                if (PhotonNetwork.isMasterClient) MelonCoroutines.Start(RMMFix.Instance.LeaveRoom(5f));
            }
            onRoundEnded?.Invoke();
        }
    }
    [HarmonyLib.HarmonyPatch(typeof(RoomMultiplayerMenu), "SpawnPlayer")]
    public static class RoomSpawnPlayer
    {
        public delegate void Event();
        public static event Event onPlayerSpawned;
        [HarmonyLib.HarmonyPostfix]
        static void Postfix(RoomMultiplayerMenu __instance)
        {
            onPlayerSpawned?.Invoke();
        }
    }
    [HarmonyLib.HarmonyPatch(typeof(RoomMultiplayerMenu), "OnLeftRoom")]
    public static class RmmOnLeftRoom
    {
        [HarmonyLib.HarmonyPrefix]
        static void Prefix(RoomMultiplayerMenu __instance)
        {
            if (MapManager.isCustomMap)
            {
                PhotonNetwork.Disconnect();
            }

        }
    }
    [HarmonyLib.HarmonyPatch(typeof(RoomMultiplayerMenu), "OnDisconnectedFromPhoton")]
    public static class RmmOnDisconnectedFromPhoton
    {
        public delegate void Event();
        public static event Event onDisconnectedFromPhoton;
        [HarmonyLib.HarmonyPrefix]
        static void Prefix(RoomMultiplayerMenu __instance)
        {
            onDisconnectedFromPhoton?.Invoke();
            if (MapManager.isCustomMap)
                SceneManager.LoadSceneAsync("MainMenu");
        }
    }
}
