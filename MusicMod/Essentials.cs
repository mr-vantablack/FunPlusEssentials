﻿using System;
using Convert = System.Convert;
using File = System.IO.File;
using FunPlusEssentials.Fun;
using FunPlusEssentials.Other;
using IniFile = FunPlusEssentials.Other.IniFile;
using UnityEngine;
using System.Text.RegularExpressions;
using MelonLoader;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.IO;
using Il2CppCodeStage.AntiCheat.Storage;
using UnityEngine.SceneManagement;
using FunPlusEssentials.Patches;
using System.Xml.Linq;
using System.Reflection;
using static MelonLoader.MelonLogger;
using Il2Cpp;
using Il2CppSystem.Text.RegularExpressions;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using Il2CppPhoton;
using MonoBehaviour = UnityEngine.MonoBehaviour;
using FunPlusEssentials.CustomContent;

namespace FunPlusEssentials.Essentials
{
    public static class Config
    {
        public static string mainPath = Environment.CurrentDirectory + @"\Mods\FunPlusEssentials";
        public static string blackListPath = mainPath + @"\blacklist.txt";
        public static string configPath = mainPath + @"\config.ini";
        public static string[,] keys = {
            { "EnableModLogs", "true" },
            { "EnableBlackList", "true" },
            { "EnableMusicPlayer", "true" },
            { "EnableSecurityPatches", "false" },
            { "NoFileSizeLimit", "false" },
            { "NoRichText", "false" },
            { "ChatNickNameColor", "#87CEEB" },
            { "EnableScoreboard", "true" },
            { "ScoreboardKeyCode", "F1" },
            { "HideHUDKeyCode", "H" },
            { "ProneKeyCode", "C" },
            { "FOV", "65" },
            { "EnableInfectedFilters", "true" },
            { "EnablePostProcessing", "true" },
            { "EnableCustomServers", "false" },
            { "CustomServerAppID", "" },
            { "FPSLock", "60" },
            { "V-SyncCount", "1" }
        };
        public static string[,] plagueKeys =
        {
            { "Enabled", "false" },
            { "Language", "en" }
        };
        public static bool logsEnabled, blacklistEnabled, 
        musicEnabled, securityEnabled, noFileSizeLimit, 
        noRichText, scoreboardEnabled, filtersEnabled, 
        postProcessingEnabled, customServersEnabled, plagueEnabled;
        public static KeyCode scoreboardKey, hideHudKey, proneKey;
        public static float fov;
        public static int fpsLock = 60;
        public static int vSyncCount = 1;
        public static string nicknameColor = "", customServerAppId = "";

        public static void SetUpConfig()
        {
            Directory.CreateDirectory(mainPath);
            var config = new IniFile(configPath);
            for (int i = 0; i < keys.GetLength(0); i++)
            {
                if (!config.KeyExists(keys[i, 0])) { config.Write(keys[i, 0], keys[i, 1]); }
            }
            for (int i = 0; i < plagueKeys.GetLength(0); i++)
            {
                if (!config.KeyExists(plagueKeys[i, 0], "PlagueMode")) { config.Write(plagueKeys[i, 0], plagueKeys[i, 1], "PlagueMode"); }
            }
            logsEnabled = Convert.ToBoolean(config.Read("EnableModLogs"));
            blacklistEnabled = Convert.ToBoolean(config.Read("EnableBlackList"));
            musicEnabled = Convert.ToBoolean(config.Read("EnableMusicPlayer"));
            securityEnabled = Convert.ToBoolean(config.Read("EnableSecurityPatches"));
            noFileSizeLimit = Convert.ToBoolean(config.Read("NoFileSizeLimit"));
            nicknameColor = config.Read("ChatNickNameColor");
            noRichText = Convert.ToBoolean(config.Read("NoRichText"));
            scoreboardEnabled = Convert.ToBoolean(config.Read("EnableScoreboard"));
            fov = Convert.ToSingle(config.Read("FOV"));
            filtersEnabled = Convert.ToBoolean(config.Read("EnableInfectedFilters"));
            postProcessingEnabled = Convert.ToBoolean(config.Read("EnablePostProcessing"));
            customServersEnabled = Convert.ToBoolean(config.Read("EnableCustomServers"));
            customServerAppId = config.Read("CustomServerAppID");
            fpsLock = Convert.ToInt32(config.Read("FPSLock"));
            vSyncCount = Convert.ToInt32(config.Read("V-SyncCount"));
            plagueEnabled = Convert.ToBoolean(config.Read("Enabled", "PlagueMode"));
            PlagueLocales.SetLanguage(config.Read("Language", "PlagueMode"));
            if (!Enum.TryParse(config.Read("ScoreboardKeyCode"), out scoreboardKey)) { scoreboardKey = KeyCode.F1; }
            if (!Enum.TryParse(config.Read("HideHUDKeyCode"), out hideHudKey)) { hideHudKey = KeyCode.H; }
            if (!Enum.TryParse(config.Read("ProneKeyCode"), out proneKey)) { proneKey = KeyCode.C; }
        }
    }

    public static class Blacklist
    {
        public static string[] blackList;
        public static bool CheckPlayer(string playerName)
        {
            System.IO.Directory.CreateDirectory(Config.mainPath);
            if (!File.Exists(Config.blackListPath)) File.Create(Config.blackListPath).Close();
            string data = File.ReadAllText(Config.blackListPath);
            if (string.IsNullOrEmpty(data)) return false;
            blackList = data.Split(new string[] { Environment.NewLine }, StringSplitOptions.None);
            foreach (string line in blackList)
            {
                if (playerName == line) return true;
            }
            return false;
        }
        private static bool TargetPlayerExist(string clearName)
        {
            // string nickName = clearName + "|2"; 
            foreach (PhotonPlayer player in PhotonNetwork.playerList)
            {
                if (player.name.Split('|')[0] == clearName)
                {
                    return true;
                }
            }
            return false;
        }
        public static string Translit(string str)
        {
            string[] lat_up = { "A", "B", "V", "G", "D", "E", "Yo", "Zh", "Z", "I", "Y", "K", "L", "M", "N", "O", "P", "R", "S", "T", "U", "F", "Kh", "Ts", "Ch", "Sh", "Shch", "\"", "Y", "'", "E", "Yu", "Ya" };
            string[] lat_low = { "a", "b", "v", "g", "d", "e", "yo", "zh", "z", "i", "y", "k", "l", "m", "n", "o", "p", "r", "s", "t", "u", "f", "kh", "ts", "ch", "sh", "shch", "\"", "y", "'", "e", "yu", "ya" };
            string[] rus_up = { "А", "Б", "В", "Г", "Д", "Е", "Ё", "Ж", "З", "И", "Й", "К", "Л", "М", "Н", "О", "П", "Р", "С", "Т", "У", "Ф", "Х", "Ц", "Ч", "Ш", "Щ", "Ъ", "Ы", "Ь", "Э", "Ю", "Я" };
            string[] rus_low = { "а", "б", "в", "г", "д", "е", "ё", "ж", "з", "и", "й", "к", "л", "м", "н", "о", "п", "р", "с", "т", "у", "ф", "х", "ц", "ч", "ш", "щ", "ъ", "ы", "ь", "э", "ю", "я" };
            for (int i = 0; i <= 32; i++)
            {
                str = str.Replace(rus_up[i], lat_up[i]);
                str = str.Replace(rus_low[i], lat_low[i]);
            }
            return str;
        }
    }

    public static class ServerManager
    {
        public static Il2CppSystem.Collections.Generic.List<UpdaterV2.serverInfo> customServers = new Il2CppSystem.Collections.Generic.List<UpdaterV2.serverInfo>();
        public static Il2CppSystem.Collections.Generic.List<UpdaterV2.serverInfo> serverList
        {
            get { return GameObject.FindObjectOfType<UpdaterV2>().OJKJNHPHFFO; }
        }

        public static void AddCustomServers()
        {
            GameObject.FindObjectOfType<UpdaterV2>().CBGJGAGOEJE = true;
            PhotonNetwork.PhotonServerSettings.HostType = ServerSettings.HostingOption.PhotonCloud;
            PhotonNetwork.player.name = ObscuredPrefs.GetString("ZWName0001", "Player " + UnityEngine.Random.Range(1, 999)) + "|2";
            serverList.Clear();
            foreach (UpdaterV2.serverInfo server in customServers)
            {
                serverList.Add(server);
            }
            CuteLogger.Meow("added servers");
        }
        public static void AddCustomServer(string serverName, string serverType, string serverIP, CloudRegionCode cloudRegion)
        {
            var server = new UpdaterV2.serverInfo() { cloudRegion = cloudRegion, serverType = serverType, serverName = serverName, serverIP = serverIP };
            serverList.Add(server);
            customServers.Add(server);
        }
        public static IEnumerator GetFPEServers()
        {
            if (!Config.customServersEnabled) { yield break; }
            WWW w = new WWW("https://ippls.lh1.in/fpe-servers");
            yield return w;
            if (w.error != null)
            {
                CuteLogger.Bark("Error getting servers... " + w.error);
            }
            else
            {
                foreach (string line in w.text.Split('\n'))
                {
                    CloudRegionCode r;
                    if (!Enum.TryParse(line.Split('|')[2], out r)) { r = CloudRegionCode.eu; }
                    customServers.Add(new UpdaterV2.serverInfo()
                    {
                        serverName = line.Split('|')[0],
                        serverType = "APP",
                        serverIP = line.Split('|')[1],
                        cloudRegion = r
                    });
                }
            }
            if (Config.customServerAppId != "")
            {
                customServers.Add(new UpdaterV2.serverInfo()
                {
                    serverName = "CUSTOM",
                    serverType = "APP",
                    serverIP = Config.customServerAppId,
                    cloudRegion = CloudRegionCode.eu
                });
            }
        }
    }
    [RegisterTypeInIl2Cpp]
    public class Callbacker : MonoBehaviour
    {
        public Callbacker(IntPtr ptr) : base(ptr) { }
        public static Callbacker Instance;

        public void Start()
        {
            Instance = this;
            CuteLogger.Quack("callbacker start");
        }
        private void OnConnectedToPhoton()
        {
            CuteLogger.Quack("callbacker connected to photon cloud");
        }
        private void OnJoinedRoom()
        {
            CuteLogger.Meow("Joined room");
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
                    }
                }
            }
        }
    }
    public static class CommandHandler
    {
        public static bool canRespawn = true;
        public static class Commands
        {
            public static void ME(PhotonPlayer sender, string[] args)
            {
                string text = "";
                for (int i = 1; i < args.Length; i++)
                {
                    text += args[i] + " ";
                }
                Helper.SendChatMessage("  " + sender.name.Split('|')[0] + "  ", "<i>" + text + "</i>", "", "silver");
            }
            public static void RESPAWN(PhotonPlayer sender, string[] args)
            {
                if (sender.name == PhotonNetwork.player.name)
                {
                    if (canRespawn)
                    {
                        if (Helper.IsMonster)
                        {
                            Helper.RoomMultiplayerMenu.SpawnPlayer("Team B");
                            MelonCoroutines.Start(RespawnCooldown(200f));
                        }
                        if (Helper.IsPlayer)
                        {
                            Helper.PlayerDamage.E100050(1000f, "");
                            MelonCoroutines.Start(RespawnCooldown(200f));
                        }
                    }
                    else { SystemMsg("Please wait a while before using this command again."); }
                }
            }
            public static void PLAY(PhotonPlayer sender, string[] args)
            {
                CuteLogger.Meow(ConsoleColor.Green, "Play");
                CommandHandler.SystemMsg($"{sender.name} changed the track...");
                if (args.Length > 2) { MusicPlayer.Instance.Play(args[1], Convert.ToSingle(args[2]), Convert.ToBoolean(args[3])); }
                else { MusicPlayer.Instance.Play(args[1]); }
            }
            public static void STOP(PhotonPlayer sender, string[] args)
            {
                CuteLogger.Meow(ConsoleColor.Red, "Stop");
                CommandHandler.SystemMsg($"{sender.name} stopped the current track...");
                MusicPlayer.Instance.Stop();
            }
        }
        public static void HandleCommand(PhotonPlayer sender, string[] args)
        {
            string command = args[0].ToUpper();
            MethodInfo method = typeof(CommandHandler.Commands).GetMethod(command);
            method.Invoke(typeof(CommandHandler.Commands), new object[] { sender, args });
        }
        public static IEnumerator RespawnCooldown(float time)
        {
            canRespawn = false;
            yield return new WaitForSeconds(time);
            canRespawn = true;
        }
        public static bool HandleChat(string nickName, string inputText)
        {
            if (inputText[1] != '/') return false;
            string[] args = inputText.Remove(0, 2).Split(' ');
            MelonLogger.Msg(nickName);
            HandleCommand(GetSenderPlayer(nickName), args);
            return true;
        }

        public static void SystemMsg(string text)
        {
            Helper.SendChatMessage(FPE.AppInfo.Name, text, "orange");
        }

        public static PhotonPlayer GetSenderPlayer(string nickName)
        {
            string clearName = nickName.Substring(2, nickName.Length - 4);
            clearName = Regex.Replace(clearName, "<.*?>", string.Empty);
            foreach (PhotonPlayer player in PhotonNetwork.playerList)
            {
                if (player.name.Split('|')[0] == clearName)
                {
                    return player;
                }
            }
            return null;
        }
    }
    [RegisterTypeInIl2Cpp]
    public class Rotator : MonoBehaviour
    {
        public Rotator(IntPtr ptr) : base(ptr) { }
        public float speed = -15f;
        public bool rotate;
        void Update()
        {
            if (rotate && Input.GetKey(KeyCode.LeftControl))
                transform.Rotate(Input.GetAxis("Mouse Y") * Time.deltaTime * speed, Input.GetAxis("Mouse X") * Time.deltaTime * speed, 0f);
        }
    }

    [RegisterTypeInIl2Cpp]
    public class HudHider : MonoBehaviour
    {
        public HudHider(IntPtr ptr) : base(ptr) { }
        public bool hidden;
        public static HudHider Instance { get; set; }

        void Start()
        {
            Instance = this;
        }
        void Hide(bool hide)
        {
            if (Helper.IsPlayer)
            {
                hidden = hide;
                Helper.AmmoDisplay.enabled = !hide;
                Helper.MultiplayerChat.IOMLGNJCHHK = hide;
                Helper.WhoKilledWho.JOMPBHFEANP = hide;
                Helper.Player.transform.Find("HUD").gameObject.SetActive(!hide);
            }
            else
            {
                hidden = hide;
                Helper.MultiplayerChat.IOMLGNJCHHK = hide;
                Helper.WhoKilledWho.JOMPBHFEANP = hide;
            }
        }
        void Update()
        {
            if (Input.GetKeyDown(Config.hideHudKey) && !Helper.MultiplayerChat.GJAEHDGOIBI)
            {
                Hide(!hidden);
            }
        }
    }
    [RegisterTypeInIl2Cpp]
    public class FlyCamControllerAddon : MonoBehaviour
    {
        public FlyCamControllerAddon(IntPtr ptr) : base(ptr) { }
        List<GameObject> filters = new List<GameObject>();
        FPSMouseLook mouseLook;
        KeyCode[] keys = new KeyCode[]
        {
            KeyCode.Alpha1,
            KeyCode.Alpha2,
            KeyCode.Alpha3,
            KeyCode.Alpha4,
            KeyCode.Alpha5,
            KeyCode.Alpha6,
            KeyCode.Alpha7,
            KeyCode.Alpha8,
            KeyCode.Alpha9
        };
        void Start()
        {
            var camRoot = GameObject.Find("__Room/TheaterMode/TheatreMechanics/CameraFilters");
            for (int i = 0; i < camRoot.transform.childCount; i++)
            {
                filters.Add(camRoot.transform.GetChild(i).gameObject);
            }
            mouseLook = GetComponent<FPSMouseLook>();
        }
        void Update()
        {
            float scroll = Input.GetAxis("Mouse ScrollWheel");
            if (scroll < 0f)
            {
                if (mouseLook.KOGHEFBJNCJ > 0)
                {
                    mouseLook.KOGHEFBJNCJ -= 1;
                }
            }
            if (scroll > 0f)
            {
                mouseLook.KOGHEFBJNCJ += 1;
            }
            for (int i = 0; i < filters.Count; i++)
            {
                if (Input.GetKeyDown(keys[i]))
                {
                    ChangeFilter(i);
                }
            }
        }
        void ChangeFilter(int id)
        {
            for (int i = 0; i < filters.Count; i++)
            {
                if (i != id)
                {
                    filters[i].SetActive(false);
                }
                else
                {
                    filters[i].SetActive(!filters[i].gameObject.activeSelf);
                }
            }
        }
    }

    [RegisterTypeInIl2Cpp]
    public class RMMFix : MonoBehaviour
    {
        public RMMFix(IntPtr ptr) : base(ptr) { }
        public string leadingPlayer = "";
        public Dictionary<PhotonPlayer, PlayerEntry> teamAPlayers = new Dictionary<PhotonPlayer, PlayerEntry>();
        public Dictionary<PhotonPlayer, PlayerEntry> teamBPlayers = new Dictionary<PhotonPlayer, PlayerEntry>();
        public bool updated;

        public static RMMFix Instance { get; private set; }
        void Start()
        {
            Instance = this;
            Helper.SetProperty("nicknameColor", Config.nicknameColor);
        }
        void OnDisable()
        {
            if (PhotonNetwork.connected)
            {
                SceneManager.LoadScene("Updater");
                PhotonNetwork.LeaveRoom();
                PhotonNetwork.Disconnect();
            }
        }
        public IEnumerator LeaveRoom(float seconds)
        {
            yield return new WaitForSeconds(seconds);
            PhotonNetwork.Disconnect();
        }
        public void SortList()
        {
            //   updated = true;
            //yield return new WaitForSeconds(2);
            teamAPlayers.Clear();
            teamBPlayers.Clear();
            foreach (PhotonPlayer photonPlayer in PhotonNetwork.playerList)
            {
                if (Helper.GetProperty(photonPlayer, "TeamName") != null && Helper.GetProperty(photonPlayer, "TeamName").ToString() == "Team A")
                {
                    teamAPlayers.Add(photonPlayer, new PlayerEntry(photonPlayer, Helper.GetProperty(photonPlayer, "Kills").Unbox<int>(), Helper.GetProperty(photonPlayer, "Deaths").Unbox<int>(), Helper.GetProperty(photonPlayer, "TeamName").ToString()));
                }
                if (Helper.GetProperty(photonPlayer, "TeamName") != null && Helper.GetProperty(photonPlayer, "TeamName").ToString() == "Team B")
                {
                    teamBPlayers.Add(photonPlayer, new PlayerEntry(photonPlayer, Helper.GetProperty(photonPlayer, "Kills").Unbox<int>(), Helper.GetProperty(photonPlayer, "Deaths").Unbox<int>(), Helper.GetProperty(photonPlayer, "TeamName").ToString()));
                }
            }
            if (teamAPlayers != null && teamAPlayers.Count > 0)
            {
                teamAPlayers = teamAPlayers.OrderByDescending(x => x.Value.kills).ToDictionary(x => x.Key, x => x.Value);
                teamBPlayers = teamBPlayers.OrderByDescending(x => x.Value.kills).ToDictionary(x => x.Key, x => x.Value);
                leadingPlayer = teamAPlayers.Keys.First().name;
                if (TabMenu.Instance != null) TabMenu.Instance.UpdatePlayersInfo(teamAPlayers, teamBPlayers);
            }
            // updated = false;
        }
    }

    public record PlayerEntry
    {
        public PlayerEntry(PhotonPlayer player, int kills, int death, string teamName)
        {
            this.player = player;
            this.kills = kills;
            this.death = death;
            this.teamName = teamName;
        }
        public PhotonPlayer player;
        public int kills = 0;
        public int death = 0;
        public string teamName;
    }

    [RegisterTypeInIl2Cpp]
    public class TabMenu : MonoBehaviour
    {
        public TabMenu(IntPtr ptr) : base(ptr) { }
        public bool isTabMenuShown;
        public Dictionary<PhotonPlayer, PlayerEntry> m_teamAPlayers = new Dictionary<PhotonPlayer, PlayerEntry>();
        public Dictionary<PhotonPlayer, PlayerEntry> m_teamBPlayers = new Dictionary<PhotonPlayer, PlayerEntry>();
        private Vector2 m_scrollView;
        public static TabMenu Instance;
        private Il2CppReferenceArray<GUILayoutOption> m_style;
        private GameObject m_cursorLock;

        void Start()
        {
            Instance = this;
            m_style = new Il2CppReferenceArray<GUILayoutOption>(0L);
        }

        public void UpdatePlayersInfo(Dictionary<PhotonPlayer, PlayerEntry> teamA, Dictionary<PhotonPlayer, PlayerEntry> teamB)
        {
            m_teamAPlayers = teamA;
            m_teamBPlayers = teamB;
        }
        public void Update()
        {
            isTabMenuShown = Input.GetKey(Config.scoreboardKey);
            if (Input.GetKeyDown(Config.scoreboardKey))
            {
                RMMFix.Instance.SortList();
                m_cursorLock = new GameObject("cursorLock");
                m_cursorLock.tag = "Menu";
            }
            if (Input.GetKeyUp(Config.scoreboardKey))
            {
                Destroy(m_cursorLock);
            }
        }
        public bool BanPlayer(string playerName)
        {
            playerName = playerName.Split('|')[0];
            if (Blacklist.CheckPlayer(playerName))
            {
                CuteLogger.Quack("The player is already blacklisted.");
                return false;
            }
            else
            {
                File.AppendAllLines(Config.blackListPath, new string[] { playerName });
                CuteLogger.Meow(ConsoleColor.Red, playerName + " was banned.");
                return true;
            }
        }
        void OnGUI()
        {
            if (isTabMenuShown)
            {
                GUIStyle guistyle = new GUIStyle(GUI.skin.label)
                {
                    alignment = TextAnchor.MiddleCenter
                };
                GUILayout.BeginArea(new Rect((float)Screen.width / 2f - 250f, (float)Screen.height / 2f - 250f, 500f, 500f));
                GUILayout.BeginVertical("Box", m_style);
                m_scrollView = GUILayout.BeginScrollView(m_scrollView, m_style);
                GUILayout.Label(Helper.Paint("[FPE] Scoreboard", "Red"), guistyle, m_style);
                GUILayout.Label(Helper.Paint("Master: " + PhotonNetwork.masterClient.name.Split('|')[0], "White"), m_style);
                GUILayout.Label(Helper.Paint("Ping: " + PhotonNetwork.GetPing().ToString(), "White"), m_style);
                GUILayout.BeginHorizontal(m_style);
                GUILayout.Label(Helper.Paint("Name", "White"), m_style);
                GUILayout.Space(10f);
                GUILayout.Label(Helper.Paint("Score", "White"), m_style);
                GUILayout.Label(Helper.Paint("Deaths", "White"), m_style);
                GUILayout.EndHorizontal();
                foreach (PlayerEntry entry in m_teamAPlayers.Values)
                {
                    GUILayout.BeginHorizontal("Box", m_style);
                    if (entry.player.name == PhotonNetwork.player.name)
                    {
                        GUILayout.Label(Helper.Paint(entry.player.name.Split('|')[0], "Yellow"), m_style);
                    }
                    else
                    {
                        GUILayout.Label(Helper.Paint(entry.player.name.Split('|')[0], "Cyan"), m_style);
                    }
                    GUILayout.Space(10f);
                    GUILayout.Label(Helper.Paint(entry.kills.ToString(), "White"), m_style);
                    GUILayout.Label(Helper.Paint(entry.death.ToString(), "White"), m_style);
                    if (PhotonNetwork.isMasterClient && entry.player.name != PhotonNetwork.player.name)
                    {
                        if (GUILayout.Button("Ban", m_style))
                        {
                            if (BanPlayer(entry.player.name))
                            {
                                Il2CppSystem.Object content = new Il2CppSystem.Int32() { m_value = entry.player.actorID }.BoxIl2CppObject();
                                RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.MasterClient };
                                PhotonNetwork.RaiseEvent(1, content, true, raiseEventOptions);
                            }
                        }
                    }
                    GUILayout.EndHorizontal();
                }
                foreach (PlayerEntry entry in m_teamBPlayers.Values)
                {
                    GUILayout.BeginHorizontal("Box", m_style);
                    if (entry.player.name == PhotonNetwork.player.name)
                    {
                        GUILayout.Label(Helper.Paint(entry.player.name.Split('|')[0], "Yellow"), m_style);
                    }
                    else
                    {
                        GUILayout.Label(Helper.Paint(entry.player.name.Split('|')[0], "Red"), m_style);
                    }
                    GUILayout.Space(10f);
                    GUILayout.Label(Helper.Paint(entry.kills.ToString(), "White"), m_style);
                    GUILayout.Label(Helper.Paint(entry.death.ToString(), "White"), m_style);
                    if (PhotonNetwork.isMasterClient && entry.player.name != PhotonNetwork.player.name)
                    {
                        if (GUILayout.Button("Ban", m_style))
                        {
                            if (BanPlayer(entry.player.name))
                            {
                                Il2CppSystem.Object content = new Il2CppSystem.Int32() { m_value = entry.player.actorID }.BoxIl2CppObject();
                                RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.MasterClient };
                                PhotonNetwork.RaiseEvent(1, content, true, raiseEventOptions);
                            }
                        }
                    }
                    GUILayout.EndHorizontal();
                }
                GUILayout.EndScrollView();
                GUILayout.EndVertical();
                GUILayout.EndArea();
            }
        }
    }
}
