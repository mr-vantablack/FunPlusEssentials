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
            if (Config.blacklistEnabled)
            {
                string clearName = otherPlayer.name.Split('|')[0];
                CuteLogger.Meow(ConsoleColor.Green, $"{clearName} connected.");
                if (Blacklist.CheckPlayer(clearName) && PhotonNetwork.isMasterClient)
                {

                    Il2CppSystem.Object content = new Il2CppSystem.Int32() { m_value = otherPlayer.actorID }.BoxIl2CppObject();
                    RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.MasterClient };
                    PhotonNetwork.RaiseEvent(1, content, true, raiseEventOptions);
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
        [HarmonyLib.HarmonyPostfix]
        static void Postfix(RoomMultiplayerMenu __instance)
        {
            if (RMMFix.Instance != null) __instance.HGKEEDKPGOD = RMMFix.Instance.leadingPlayer;
            //Msg("yes");
        }
    }

    [HarmonyLib.HarmonyPatch(typeof(RoomMultiplayerMenu), "Awake")]
    public static class RoomMultiplayerMenuAwake
    {
        public delegate void Event(RoomMultiplayerMenu instance);
        public static event Event onRoomSpawned;

        [HarmonyLib.HarmonyPostfix]
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
    public static class SpawnPlayer
    {       
        [HarmonyLib.HarmonyPrefix]
        static void Prefix(ref string teamName, RoomMultiplayerMenu __instance)
        {

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
        [HarmonyLib.HarmonyPostfix]
        static void Postfix()
        {
            if (onJoinedRoom != null) onJoinedRoom.Invoke();
            if (PhotonNetwork.isOfflineMode) { return; }
            string mapName = PhotonNetwork.room.customProperties["MN002'"].ToString();
            foreach (MapInfo map in MapManager.customMaps)
            {
                if (mapName == map.map.mapName)
                {
                    if (PhotonNetwork.room.customProperties["mapVersion"].ToString() != map.version)
                    {
                        CuteLogger.Meow(ConsoleColor.Red, "Disconnected. Map version mismatch.");
                        PhotonNetwork.LeaveRoom();
                        PhotonNetwork.Disconnect();
                    }
                }
            }
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
            onLobbyDisabled?.Invoke();
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
    #endregion
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
                if (ServerManager.customServers.Count != 0 && ServerManager.enabled)
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
    [HarmonyLib.HarmonyPatch(typeof(Volume), "Start")]
    public static class VolumeStart
    {
        [HarmonyLib.HarmonyPostfix]
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

    [HarmonyLib.HarmonyPatch(typeof(FlyCamController), "Update")]
    public static class FlyCamUpdate
    {
        [HarmonyLib.HarmonyPrefix]
        static void Prefix(FlyCamController __instance)
        {
            if (__instance.photonView.isMine)
            {
                float scroll = Input.GetAxis("Mouse ScrollWheel");
                if (scroll < 0f)
                {
                    if (__instance.GetComponent<FPSMouseLook>().KOGHEFBJNCJ > 0)
                    {
                        __instance.GetComponent<FPSMouseLook>().KOGHEFBJNCJ -= 1;
                    }
                }
                if (scroll > 0f)
                {
                    __instance.GetComponent<FPSMouseLook>().KOGHEFBJNCJ += 1;
                }
            }
        }
    }

    
}
