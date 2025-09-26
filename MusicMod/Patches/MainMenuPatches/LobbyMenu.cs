using FunPlusEssentials.CustomContent;
using FunPlusEssentials.Essentials;
using FunPlusEssentials.Other;
using Il2Cpp;
using MelonLoader;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static MelonLoader.MelonLogger;

namespace FunPlusEssentials.Patches
{
    [HarmonyLib.HarmonyPatch(typeof(LobbyMenu), "OnJoinedRoom")]
    public static class LobbyOnJoinedRoom
    {
        public delegate void Event();
        public static event Event onJoinedRoom;
        static bool Prefix(LobbyMenu __instance)
        {
            /*CuteLogger.Meow("Joined room");
            Helper.LobbyMenu.EJLDOIOJGPC = true;
            string mapName = PhotonNetwork.room.customProperties["MN002'"] != null ? PhotonNetwork.room.customProperties["MN002'"].ToString() : "";
            var scene = mapName;
            scene = scene.Replace(" (Day)", "");
            scene = scene.Replace(" (Dusk)", "");
            scene = scene.Replace(" (Night)", "");
            string customNPCs = PhotonNetwork.room.customProperties["customContent"] != null ? PhotonNetwork.room.customProperties["customContent"].ToString() : "";
            CuteLogger.Meow(scene);
            foreach (MapInfo map in MapManager.customMaps)
            {
                if (scene == map.map.mapName)
                {
                    string ver = PhotonNetwork.room.customProperties["mapVersion"] != null ? PhotonNetwork.room.customProperties["mapVersion"].ToString() : "";
                    if (ver != "" && ver != map.version)
                    {
                        CuteLogger.Meow(ConsoleColor.Red, "Disconnected. Map version mismatch.");
                        if (!PhotonNetwork.isOfflineMode) { PhotonNetwork.Disconnect(); }
                        Notifier.Show($"<color=red>Map version mismatch!</color>\nYour program version is {map.version}, while the host map version is {ver}.\nPlease download the required map version.");
                        return false;
                    }
                    string dependencies = "";
                    if (map.usingCustomNPCs)
                    {
                        MapManager.useCustomNPCs = true;
                        for (int i = 0; i < map.dependencies.Count; i++)
                        {
                            CuteLogger.Meow("checking");
                            var npc = NPCManager.CheckNPCInfos(map.dependencies[i]);
                            if (npc == null) dependencies += map.dependencies[i] + ", ";
                        }
                    }
                    if (dependencies != "")
                    {
                        CuteLogger.Meow(ConsoleColor.Red, "Disconnected. Missing custom content.");
                        if (!PhotonNetwork.isOfflineMode) { PhotonNetwork.Disconnect(); }
                        else { Helper.LobbyMenu.CPFFBHEDEPF.ReturnToMenu(); }
                        Notifier.Show("<color=red>Missing custom content!</color>\nThis map uses custom content. Please download the missing files:\n" + dependencies);
                        return false;
                    }
                }
            }
            string missingNPCs = "";
            if (customNPCs != null && customNPCs != "")
            {
                MapManager.useCustomNPCs = true;
                foreach (string npcName in customNPCs.Split('|'))
                {
                    if (npcName != null && npcName != "")
                    {
                        var npc = NPCManager.CheckNPCInfos(npcName);
                        var weapon = CustomWeapons.CheckWeaponInfos(npcName);
                        if (npc == null && weapon == null)
                        {
                            missingNPCs += npcName + ", ";
                        }
                    }
                }
            }
            if (missingNPCs != "")
            {
                CuteLogger.Meow(ConsoleColor.Red, "Disconnected. Missing custom content.");
                if (!PhotonNetwork.isOfflineMode) { PhotonNetwork.Disconnect(); }
                Notifier.Show("<color=red>Missing custom content!</color>\nThis room uses custom content. Please download the missing files:\n" + missingNPCs);
                return false;
            }
            if (PhotonNetwork.isMasterClient)
            {
                MapManager.CheckForCustomContent();
                if (Plague.Enabled) Helper.SetRoomProperty("Plague", "true");
            }
            else
            {
                Plague.Enabled = false;
                if (PhotonNetwork.room.customProperties["Plague"] != null)
                {
                    if (PlagueAssets._inited)
                    {
                        Plague.Enabled = true;
                    }
                    else
                    {
                        CuteLogger.Meow(ConsoleColor.Red, "Disconnected. Missing custom content.");
                        if (!PhotonNetwork.isOfflineMode) { PhotonNetwork.Disconnect(); }
                        Notifier.Show("<color=red>Missing custom content!</color>\nThis room uses custom content. Please download the missing files:\nPlagueMode");
                        return false;
                    }
                }

            }
            */
            return true;
        }
    }
    [HarmonyLib.HarmonyPatch(typeof(LobbyMenu.DBENNALHBEM), "MoveNext")]
    public static class LobbyMenuLoadScene
    {
        public delegate void Event(LobbyMenu.DBENNALHBEM scene);
        public static event Event onLobbyRoomStarted;
        [HarmonyLib.HarmonyPrefix]
        static void Prefix(LobbyMenu.DBENNALHBEM __instance)
        {
            if (onLobbyRoomStarted != null) onLobbyRoomStarted.Invoke(__instance);
            if (!MapManager.m_loaded)
            {
                MapManager.m_loaded = true;
                var scene = __instance.sceneName;
                scene = scene.Replace(" (Day)", "");
                scene = scene.Replace(" (Dusk)", "");
                scene = scene.Replace(" (Night)", "");
                CuteLogger.Meow(scene);
                //NPCManager.LoadBundles();
                foreach (MapInfo map in MapManager.customMaps)
                {
                    if (map.map.mapName == scene)
                    {
                        MelonCoroutines.Start(MapManager.SetUpMusic(map));
                        if (map.map.useDayAndNight)
                        {
                            CuteLogger.Meow(Helper.LobbyMenu.NLAEKGFANJA.ToString() + " " +
                            Helper.LobbyMenu.GGMDFHLODMF.ToString());
                            if (Helper.LobbyMenu.NLAEKGFANJA)
                            {
                                BundleManager.LoadSceneBundle("", map.map.mapName + " (Dusk)", map.bundlePath);
                            }
                            if (Helper.LobbyMenu.GGMDFHLODMF)
                            {
                                BundleManager.LoadSceneBundle("", map.map.mapName + " (Night)", map.bundlePath);
                            }
                            if (!Helper.LobbyMenu.GGMDFHLODMF && !Helper.LobbyMenu.NLAEKGFANJA)
                            {
                                BundleManager.LoadSceneBundle("", map.map.mapName + " (Day)", map.bundlePath);
                            }
                        }
                        else
                        {
                            BundleManager.LoadSceneBundle("", map.map.mapName, map.bundlePath);
                        }
                    }
                }
            }
        }
    }
    [HarmonyLib.HarmonyPatch(typeof(LobbyMenu), "Awake")]
    public static class LobbyMenuAwake
    {
        [HarmonyLib.HarmonyPostfix]
        static void Postfix(LobbyMenu __instance)
        {
            MapManager.AddCustomMaps();
            __instance.gameObject.AddComponent<LobbyHelper>().lobby = __instance;
            //__instance.MHDLCNKEEGN.Add(new LobbyMenu.AllModes() { modeID = "PLG", modeName = "PLAGUE" });
        }
    }
    [HarmonyLib.HarmonyPatch(typeof(LobbyMenu), "OnEnable")]
    public static class LobbyMenuOnEnabled
    {
        public delegate void Event(LobbyMenu lobby);
        public static event Event onLobbyEnabled;

        [HarmonyLib.HarmonyPostfix]
        static void Postfix(LobbyMenu __instance)
        {
            onLobbyEnabled?.Invoke(__instance);
        }
    }
    [HarmonyLib.HarmonyPatch(typeof(MainMenu), "Start")]
    public static class MainMenuStart
    {
        [HarmonyLib.HarmonyPostfix]
        static void Postfix(MainMenu __instance)
        {
            if (Callbacker.Instance == null)
            {
                var c = new GameObject("PhotonCallbacksHandler");
                c.AddComponent<Callbacker>();
                GameObject.DontDestroyOnLoad(c);
            }
        }
    }
    [HarmonyLib.HarmonyPatch(typeof(PhotonNetwork), "JoinRoom", new System.Type[] { typeof(string) })]
    public static class lp1
    {
        [HarmonyLib.HarmonyPrefix]
        static bool Prefix(ref string roomName)
        {
          /*  CuteLogger.Meow("Joined room");
            Helper.LobbyMenu.EJLDOIOJGPC = true;
            string mapName = PhotonNetwork.room.customProperties["MN002'"] != null ? PhotonNetwork.room.customProperties["MN002'"].ToString() : "";
            var scene = mapName;
            scene = scene.Replace(" (Day)", "");
            scene = scene.Replace(" (Dusk)", "");
            scene = scene.Replace(" (Night)", "");
            string customNPCs = PhotonNetwork.room.customProperties["customContent"] != null ? PhotonNetwork.room.customProperties["customContent"].ToString() : "";
            CuteLogger.Meow(scene);
            foreach (MapInfo map in MapManager.customMaps)
            {
                if (scene == map.map.mapName)
                {
                    string ver = PhotonNetwork.room.customProperties["mapVersion"] != null ? PhotonNetwork.room.customProperties["mapVersion"].ToString() : "";
                    if (ver != "" && ver != map.version)
                    {
                        CuteLogger.Meow(ConsoleColor.Red, "Disconnected. Map version mismatch.");
                        if (!PhotonNetwork.isOfflineMode) { PhotonNetwork.Disconnect(); }
                        Notifier.Show($"<color=red>Map version mismatch!</color>\nYour program version is {map.version}, while the host map version is {ver}.\nPlease download the required map version.");
                        return false;
                    }
                    string dependencies = "";
                    if (map.usingCustomNPCs)
                    {
                        MapManager.useCustomNPCs = true;
                        for (int i = 0; i < map.dependencies.Count; i++)
                        {
                            CuteLogger.Meow("checking");
                            var npc = NPCManager.CheckNPCInfos(map.dependencies[i]);
                            if (npc == null) dependencies += map.dependencies[i] + ", ";
                        }
                    }
                    if (dependencies != "")
                    {
                        CuteLogger.Meow(ConsoleColor.Red, "Disconnected. Missing custom content.");
                        if (!PhotonNetwork.isOfflineMode) { PhotonNetwork.Disconnect(); }
                        else { Helper.LobbyMenu.CPFFBHEDEPF.ReturnToMenu(); }
                        Notifier.Show("<color=red>Missing custom content!</color>\nThis map uses custom content. Please download the missing files:\n" + dependencies);
                        return false;
                    }
                }
            }
            string missingNPCs = "";
            if (customNPCs != null && customNPCs != "")
            {
                MapManager.useCustomNPCs = true;
                foreach (string npcName in customNPCs.Split('|'))
                {
                    if (npcName != null && npcName != "")
                    {
                        var npc = NPCManager.CheckNPCInfos(npcName);
                        var weapon = CustomWeapons.CheckWeaponInfos(npcName);
                        if (npc == null && weapon == null)
                        {
                            missingNPCs += npcName + ", ";
                        }
                    }
                }
            }
            if (missingNPCs != "")
            {
                CuteLogger.Meow(ConsoleColor.Red, "Disconnected. Missing custom content.");
                if (!PhotonNetwork.isOfflineMode) { PhotonNetwork.Disconnect(); }
                Notifier.Show("<color=red>Missing custom content!</color>\nThis room uses custom content. Please download the missing files:\n" + missingNPCs);
                return false;
            }
            if (PhotonNetwork.isMasterClient)
            {
                MapManager.CheckForCustomContent();
                if (Plague.Enabled) Helper.SetRoomProperty("Plague", "true");
            }
            else
            {
                Plague.Enabled = false;
                if (PhotonNetwork.room.customProperties["Plague"] != null)
                {
                    if (PlagueAssets._inited)
                    {
                        Plague.Enabled = true;
                    }
                    else
                    {
                        CuteLogger.Meow(ConsoleColor.Red, "Disconnected. Missing custom content.");
                        if (!PhotonNetwork.isOfflineMode) { PhotonNetwork.Disconnect(); }
                        Notifier.Show("<color=red>Missing custom content!</color>\nThis room uses custom content. Please download the missing files:\nPlagueMode");
                        return false;
                    }
                }

            }
          */
            return true;
        }
    }
    
}
