﻿using System;
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

namespace FunPlusEssentials.Patches
{

    #region MultiplayerChat
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
    #endregion

    #region PlayerNetworkController

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
    #endregion

    #region WhoKilledWho
    [HarmonyLib.HarmonyPatch(typeof(WhoKilledWho), "OnPhotonPlayerConnected")]
    public static class OnPhotonPlayerConnected
    {
        public delegate void Event(PhotonPlayer otherPlayer);
        public static event Event onPhotonPlayerConnected;

        [HarmonyLib.HarmonyPrefix]
        static void Prefix(ref PhotonPlayer otherPlayer, WhoKilledWho __instance)
        {
            onPhotonPlayerConnected?.Invoke(otherPlayer);
            string clearName = otherPlayer.name.Split('|')[0];
            if (Config.blacklistEnabled)
            {
                CuteLogger.Meow(ConsoleColor.Green, $"{clearName} connected.");
                if (Blacklist.CheckPlayer(clearName) && PhotonNetwork.isMasterClient)
                {

                    Il2CppSystem.Object content = new Il2CppSystem.Int32() { m_value = otherPlayer.actorID }.BoxIl2CppObject();
                    RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.MasterClient };
                    PhotonNetwork.RaiseEvent(1, content, true, raiseEventOptions);
                    CuteLogger.Meow(ConsoleColor.Red, $"{clearName} was kicked.");
                }
            }
            if (MapManager.useCustomNPCs && PhotonNetwork.isMasterClient)
            {
                var fpe = otherPlayer.customProperties["FPE"] != null ? otherPlayer.customProperties["FPE"].ToString() : "";
                if (fpe == "")
                {
                    PhotonNetwork.CloseConnection(otherPlayer);
                    CuteLogger.Meow(ConsoleColor.Red, $"{clearName} was kicked.");
                }
            }
        }
    }
    [HarmonyLib.HarmonyPatch(typeof(WhoKilledWho), "OnPhotonPlayerDisconnected")]
    public static class OnPhotonPlayerDisconnected
    {
        public delegate void Event(PhotonPlayer otherPlayer);
        public static event Event onPhotonPlayerDisonnected;

        [HarmonyLib.HarmonyPrefix]
        static void Prefix(ref PhotonPlayer ANLAKOKFGCJ, WhoKilledWho __instance)
        {
            onPhotonPlayerDisonnected?.Invoke(ANLAKOKFGCJ);
            string clearName = ANLAKOKFGCJ.name.Split('|')[0];
            CuteLogger.Meow(ConsoleColor.Red, $"{clearName} disconnected.");
        }
    }
    #endregion

    #region RoomMultiplayerMenu
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

        static void Prefix()
        {
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
            if (Plague.Enabled) Helper.Room.AddComponent<PlagueController>();
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
    #endregion

    #region LobbyMenu
    [HarmonyLib.HarmonyPatch(typeof(LobbyMenu), "OnJoinedRoom")]
    public static class LobbyOnJoinedRoom
    {
        public delegate void Event();
        public static event Event onJoinedRoom;
        static bool Prefix(LobbyMenu __instance)
        {
            __instance.EJLDOIOJGPC = true;
            if (onJoinedRoom != null) onJoinedRoom.Invoke();
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
                        else { __instance.CPFFBHEDEPF.ReturnToMenu(); }
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
                    if (PhotonNetwork.room.customProperties["Plague"].ToString() == "true")
                    {
                        Plague.Enabled = true;
                    }
                }
            }

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
    [HarmonyLib.HarmonyPatch(typeof(LobbyMenu), "OnDisable")]
    public static class LobbyMenuOnDisabled
    {
        public delegate void Event();
        public static event Event onLobbyDisabled;

        [HarmonyLib.HarmonyPostfix]
        static void Prefix()
        {
            //onLobbyDisabled?.Invoke();
        }
    }
    #endregion

    #region SurvivalMechanics
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
    #endregion

    #region PhotonNetwork
    [HarmonyLib.HarmonyPatch(typeof(PhotonNetwork), "NOOU2")]
    public static class PhotonNetworkInstantiateRoomPrefab
    {
        static bool Prefix(ref string prefabName, ref Vector3 position, ref Quaternion rotation)
        {
            return !NPCManager.Instantiate(prefabName, position, rotation);
        }
    }
    [HarmonyLib.HarmonyPatch(typeof(PhotonNetwork), "NOOU", new Type[] { typeof(string), typeof(Vector3), typeof(Quaternion), typeof(byte) })]
    public static class PhotonNetworkInstantiate
    {
        static bool Prefix(ref string prefabName, ref Vector3 position, ref Quaternion rotation)
        {         
            return !NPCManager.Instantiate(prefabName, position, rotation);
        }
    }
    /* [HarmonyLib.HarmonyPatch(typeof(NetworkingPeer), "DoInstantiate")]
     public static class PhotonInstantiate
     {
         static void Prefix(ref Hashtable evData, ref PhotonPlayer photonPlayer, ref GameObject resourceGameObject)
         {
             var hashtable = evData[new Il2CppSystem.Byte() { m_value = 5 }.BoxIl2CppObject()];
             if (hashtable != null)
             {
                 var data = hashtable.Cast<Il2CppReferenceArray<Il2CppSystem.Object>>();
                 CuteLogger.Meow("data " + data[0].ToString());
                 if (data[0].ToString() == "CustomNPC")
                 {
                     NPCManager.SetUpCustomNPC(data[1].ToString(), resourceGameObject);
                 }

             //FunNPCInfo npc = NPCManager.CheckNPCInfos(prefabName);
             //if (npc == null) return;
             // NPCManager.SpawnNPCInfo();
         }
     }*/
    #endregion

    [HarmonyLib.HarmonyPatch(typeof(PlayerDamage), "Awake")]
    public static class OnPlayerSpawned
    {
        public delegate void Event(PlayerDamage player);
        public static event Event onPlayerSpawned;
        static void Postfix(PlayerDamage __instance)
        {
            onPlayerSpawned?.Invoke(__instance);
        }
    }

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

    [HarmonyLib.HarmonyPatch(typeof(WeaponScript), "Start")]
    public static class WeaponScriptStart
    {
        [HarmonyLib.HarmonyPostfix]
        static void Postfix(WeaponScript __instance)
        {
            if (Config.fov > 65 && Config.fov <= 100)
            {
                __instance.KKCIIEDNLEO = new Vector3(0f, 0f, ((65 - Config.fov) / 75) * -1);
                __instance.FFGIIOODMGK = Config.fov;

            }
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

    [HarmonyLib.HarmonyPatch(typeof(WeaponPickUp), "Start")]
    public static class WeaponPickUpStart
    {
        [HarmonyLib.HarmonyPrefix]
        static void Prefix(WeaponPickUp __instance)
        {

        }
    }
    //KHCH - knife, GHLH -shotgun
    [HarmonyLib.HarmonyPatch(typeof(WeaponScript.OKEJDECKCDJ), "MoveNext")]
    public static class MachineGunOneShot
    {
        [HarmonyLib.HarmonyPostfix]
        static void Postfix()
        {

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
                GameObject.Destroy(__instance.gameObject);
                Helper.Player.AddComponent<AudioListener>();
            }
        }
    }
    [HarmonyLib.HarmonyPatch(typeof(SupportClass), "GetMethods")]
    public static class RPCPatch
    {
        [HarmonyLib.HarmonyPostfix]
        static void Postfix(ref Il2CppSystem.Collections.Generic.List<Il2CppSystem.Reflection.MethodInfo> __result, ref Il2CppSystem.Type type)
        {
            if (RPCManager.IsUsingRPC(type))
            {
                var t = type;
                var list = type.GetMethods((Il2CppSystem.Reflection.BindingFlags)(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic));
                RPCManager.rpcMethodsCache.TryGetValue(RPCManager.rpcMethodsCache.Keys.Where(n => n.Name == t.Name).FirstOrDefault(), out var methods);
                foreach (Il2CppSystem.Reflection.MethodInfo methodInfo in list)
                {
                    foreach (MethodInfo method in methods)
                    {
                        if (method.Name == methodInfo.Name && method.GetParameters().Length == methodInfo.GetParameters().Length)
                        {
                            __result.Add(methodInfo);
                        }
                    }
                }
            }
        }
    }
}
