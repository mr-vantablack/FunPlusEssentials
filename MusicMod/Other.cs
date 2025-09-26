﻿using UnityEngine;
using System.Runtime.InteropServices;
using System.Text;
using System.Reflection;
using MelonLoader;
using System.IO;
using System;
using FunPlusEssentials.Essentials;
using Hashtable = Il2CppExitGames.Client.Photon.Hashtable;
using System.Collections.Generic;
using FunPlusEssentials.CustomContent;
using Random = System.Random;
using Il2Cpp;

namespace FunPlusEssentials.Other
{
    public static class CuteLogger
    {
        public static void Meow(string msg)
        {
            if (!Config.logsEnabled) return;
            MelonLogger.Msg(msg);
            Log("[Meow] " + msg);
        }
        public static void Meow(ConsoleColor color, string msg)
        {
            if (!Config.logsEnabled) return;
            MelonLogger.Msg(color, msg);
            Log("[Meow] " + msg);
        }
        public static void Quack(string msg)
        {
            if (!Config.logsEnabled) return;
            MelonLogger.Warning(msg);
            Log("[Quack] " + msg);
        }
        public static void Bark(string msg)
        {
            if (!Config.logsEnabled) return;
            MelonLogger.Error(msg);
            Log("[Bark] " + msg);
        }

        public static void Log(string msg)
        {
            using (var file = new StreamWriter(Config.mainPath + @"\fpe_log.txt", true))
            {
                file.WriteLine(msg);
                file.Close();
            }
        }
        public static void ClearLogs()
        {
            File.WriteAllText(Config.mainPath + @"\fpe_log.txt", "");
        }
    }

    public static class Loader
    {

        //Static class instead of _instance
        // Usage from any other script:
        // MySprite = IMG2Sprite.LoadNewSprite(FilePath, [PixelsPerUnit (optional)], [spriteType(optional)])

        public static Sprite LoadNewSprite(string FilePath, float PixelsPerUnit = 100.0f, SpriteMeshType spriteType = SpriteMeshType.Tight)
        {

            // Load a PNG or JPG image from disk to a Texture2D, assign this texture to a new sprite and return its reference

            Texture2D SpriteTexture = LoadTexture(FilePath);
            Sprite NewSprite = Sprite.Create(SpriteTexture, new Rect(0, 0, SpriteTexture.width, SpriteTexture.height), new Vector2(0.5f, 0.5f), PixelsPerUnit);

            return NewSprite;
        }

        public static Sprite ConvertTextureToSprite(Texture2D texture, float PixelsPerUnit = 100.0f, SpriteMeshType spriteType = SpriteMeshType.Tight)
        {
            // Converts a Texture2D to a sprite, assign this texture to a new sprite and return its reference

            Sprite NewSprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0, 0), PixelsPerUnit, 0, spriteType);

            return NewSprite;
        }
        public static Sprite ConvertTextureToSprite(Texture2D texture, Vector2 pivot)
        {
            // Converts a Texture2D to a sprite, assign this texture to a new sprite and return its reference

            Sprite NewSprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), pivot, 100f, 0, SpriteMeshType.Tight);

            return NewSprite;
        }
        public static Texture2D LoadTexture(string FilePath)
        {
            Texture2D Tex2D;
            if (File.Exists(FilePath))
            {
                FilePath = @"file://" + FilePath;
                var req = new WWW(FilePath);
                Tex2D = req.texture;
                if (Tex2D != null)
                {
                    return Tex2D;
                }
            }
            return null;
        }

        public static AudioClip LoadAudioClip(string FilePath)
        {
            AudioClip clip;
            if (File.Exists(FilePath))
            {
                FilePath = @"file://" + FilePath;
                var req = new WWW(FilePath);
                clip = req.GetAudioClip();
                if (clip != null)
                {
                    return clip;
                }
            }
            return null;
        }

    }

    class IniFile   // revision 11
    {
        string Path;
        string EXE = Assembly.GetExecutingAssembly().GetName().Name;
        [DllImport("kernel32", CharSet = CharSet.Unicode)]
        static extern long WritePrivateProfileString(string Section, string Key, string Value, string FilePath);

        [DllImport("kernel32", CharSet = CharSet.Unicode)]
        static extern int GetPrivateProfileString(string Section, string Key, string Default, StringBuilder RetVal, int Size, string FilePath);

        public IniFile(string IniPath = null)
        {
            Path = new System.IO.FileInfo(IniPath ?? EXE + ".ini").FullName;
        }

        public string Read(string Key, string Section = null)
        {
            var RetVal = new StringBuilder(255);
            GetPrivateProfileString(Section ?? EXE, Key, "", RetVal, 255, Path);
            return RetVal.ToString();
        }

        public void Write(string Key, string Value, string Section = null)
        {
            WritePrivateProfileString(Section ?? EXE, Key, Value, Path);
        }

        public void DeleteKey(string Key, string Section = null)
        {
            Write(Key, null, Section ?? EXE);
        }

        public void DeleteSection(string Section = null)
        {
            Write(null, null, Section ?? EXE);
        }

        public bool KeyExists(string Key, string Section = null)
        {
            return Read(Key, Section).Length > 0;
        }
    }


    public static class BundleManager
    {
        public static Il2CppAssetBundle loadedScene;
        public static GameObject LoadAssetBundle(string bundleName, string prefabName, string path)
        {
            CuteLogger.Meow("loading bundle " + prefabName);
            var bundle = Il2CppAssetBundleManager.LoadFromFile(Path.Combine(path, bundleName));
            if (bundle == null)
            {
                CuteLogger.Bark("Failed to load AssetBundle!");
                return null;
            }
            // loadedBundles.Add(bundle);
            return bundle.LoadAsset<GameObject>(prefabName);
            //GameObject prefab = bundle.LoadAsset<GameObject>(prefabName);
            //return Instantiate(prefab);
        }
        public static Sprite LoadSpriteBundle(string prefabName, string path)
        {
            CuteLogger.Meow("loading bundle " + prefabName);
            var bundle = Il2CppAssetBundleManager.LoadFromFile(path);
            if (bundle == null)
            {
                CuteLogger.Bark("Failed to load AssetBundle!");
                return null;
            }
            return bundle.LoadAsset<Sprite>(prefabName);
        }
        public static AudioClip LoadAudioBundle(string prefabName, string path)
        {
            CuteLogger.Meow("loading audio bundle " + prefabName);
            var bundle = Il2CppAssetBundleManager.LoadFromFile(path);
            if (bundle == null)
            {
                CuteLogger.Bark("Failed to load AssetBundle!");
                return null;
            }
            return bundle.LoadAsset<AudioClip>(prefabName);
        }
        public static Il2CppAssetBundle LoadBundle(string path)
        {
            CuteLogger.Meow("loading bundle " + path);
            var bundle = Il2CppAssetBundleManager.LoadFromFile(path);
            if (bundle == null)
            {
                CuteLogger.Bark("Failed to load AssetBundle!");
                return null;
            }
            return bundle;
        }
        public static void LoadSceneBundle(string bundleName, string prefabName, string path)
        {
            CuteLogger.Meow("loading scene bundle " + prefabName);
            var bundle = Il2CppAssetBundleManager.LoadFromFile(Path.Combine(path, bundleName));
            if (bundle == null)
            {
                CuteLogger.Bark("Failed to load AssetBundle!");
            }
            loadedScene = bundle;
            //GameObject prefab = bundle.LoadAsset<GameObject>(prefabName);
            //return Instantiate(prefab);
        }

    }
    [RegisterTypeInIl2Cpp]
    public class LobbyHelper : MonoBehaviour
    {
        public LobbyHelper(IntPtr ptr) : base(ptr) { }
        public LobbyMenu lobby;
        public int depth = 0;
        private void Start()
        {
        }
        private void OnGUI()
        {
            if (lobby.FANLKBJODCL && !lobby.EJLDOIOJGPC && (lobby.NNKFEAPBLCK == 4 || lobby.NNKFEAPBLCK == 2))
            {
                GUI.depth = depth;
                GUI.skin = lobby.BIPMFIBNFBC;
                int num = Screen.height / 17;
                int num2 = Screen.width / 17;
                string text = "<color=grey>☐</color>";
                if (MapManager.useCustomNPCs)
                {
                    text = "<color=green>☑</color>";
                }
                MapManager.useCustomNPCs = GUI.Toggle(new Rect(Screen.width - num * 13, Screen.height - num * 4, num * 6, num), MapManager.useCustomNPCs, string.Concat(new string[]
                {
                    "<size=",
                    ((float)num / 1.75f).ToString(),
                    ">",
                    text,
                    " ",
                    "<color=white>Custom Content</color>",
                    "</size>"
                }));
            }
            if (lobby.FANLKBJODCL && !lobby.EJLDOIOJGPC && (lobby.NNKFEAPBLCK == 3) && Config.plagueEnabled && PlagueAssets._inited)
            {
                GUI.depth = depth;
                GUI.skin = lobby.BIPMFIBNFBC;
                int num = Screen.height / 17;
                int num2 = Screen.width / 17;
                string text = "<color=grey>☐</color>";
                if (Plague.Enabled)
                {
                    text = "<color=green>☑</color>";
                }
                Plague.Enabled = GUI.Toggle(new Rect(Screen.width - num * 13, Screen.height - num * 4, num * 6, num), Plague.Enabled, string.Concat(new string[]
                {
                    "<size=",
                    ((float)num / 1.75f).ToString(),
                    ">",
                    text,
                    " ",
                    "<color=white>Plague mode</color>",
                    "</size>"
                }));
            }
        }
    }
    public static class Helper //все дерьмо из этого класса нужно в отедльную библиотеку чтобы не засорять мод
    {
        #region FIELDS
        private static GameObject _roomGo;
        private static GameObject _playerGo;
        private static MultiplayerChat _multiplayerChat;
        private static PhotonView _rpcView;
        private static WhoKilledWho _whoKilledWho;
        private static RoomMultiplayerMenu _roomMultiplayerMenu;
        private static SurvivalMechanics _survivalMechanics;
        private static WeaponManager _weaponManager;
        private static LobbyMenu _lobbyMenu;
        private static PlayerDamage _playerDamage;
        private static PlayerMonster _playerMonster;
        private static FPScontroller _fpsController;
        private static AmmoDisplay _ammoDisplay;
        private static UpdaterV2 _updaterV2;
        private static Volume _console;

        private static Random _random = new Random();


        #endregion

        #region PROPERTIES
        public static Volume Console
        {
            get
            {
                if (_console == null)
                {
                    _console = GameObject.FindObjectOfType<Volume>();
                }
                return _console;
            }
        }
        public static PhotonView RPCView
        {
            get
            {
                if (_rpcView == null)
                {
                    _rpcView = RoomMultiplayerMenu.photonView;
                }
                return _rpcView;
            }
        }
        public static UpdaterV2 UpdaterV2
        {
            get
            {
                if (_updaterV2 == null)
                {
                    _updaterV2 = GameObject.FindObjectOfType<UpdaterV2>();
                }
                return _updaterV2;
            }
        }
        public static SurvivalMechanics SurvivalMechanics
        {
            get
            {
                if (_survivalMechanics == null)
                {
                    _survivalMechanics = Room.GetComponent<SurvivalMechanics>();
                }
                return _survivalMechanics;
            }
        }
        public static WhoKilledWho WhoKilledWho
        {
            get
            {
                if (_whoKilledWho == null)
                {
                    _whoKilledWho = Room.GetComponent<WhoKilledWho>();
                }
                return _whoKilledWho;
            }
        }
        public static AmmoDisplay AmmoDisplay
        {
            get
            {
                if (_ammoDisplay == null)
                {
                    _ammoDisplay = GameObject.FindObjectOfType<AmmoDisplay>();
                }
                return _ammoDisplay;
            }
        }
        public static FPScontroller FPSController
        {
            get
            {
                if (_fpsController == null)
                {
                    _fpsController = Player.GetComponent<FPScontroller>();
                }
                return _fpsController;
            }
        }

        public static bool IsPlayer
        {
            get
            {
                return PlayerDamage != null;
            }
        }
        public static bool IsMonster
        {
            get
            {
                return PlayerMonster != null;
            }
        }

        public static PlayerDamage PlayerDamage
        {
            get
            {
                if (_playerDamage == null)
                {
                    _playerDamage = Player.GetComponent<PlayerDamage>();
                }
                return _playerDamage;
            }
        }
        public static PlayerMonster PlayerMonster
        {
            get
            {
                if (_playerMonster == null)
                {
                    _playerMonster = Player.GetComponent<PlayerMonster>();
                }
                return _playerMonster;
            }
        }

        public static WeaponManager WeaponManager
        {
            get
            {
                if (_weaponManager == null)
                {
                    _weaponManager = Player.GetComponentInChildren<WeaponManager>();
                }
                return _weaponManager;
            }
        }
        public static GameObject Player
        {
            get
            {
                if (_playerGo == null)
                {
                    _playerGo = GameObject.FindObjectOfType<FPSinput>().gameObject;
                }
                return _playerGo;
            }
        }
        public static MultiplayerChat MultiplayerChat
        {
            get
            {
                if (_multiplayerChat == null)
                {
                    _multiplayerChat = GameObject.FindObjectOfType<MultiplayerChat>();
                }
                return _multiplayerChat;
            }
        }
        public static GameObject Room
        {
            get
            {
                if (_roomGo == null)
                {
                    _roomGo = GameObject.FindObjectOfType<RoomMultiplayerMenu>().gameObject;

                }
                return _roomGo;
            }
        }
        public static RoomMultiplayerMenu RoomMultiplayerMenu
        {
            get
            {
                if (_roomMultiplayerMenu == null)
                {
                    _roomMultiplayerMenu = GameObject.FindObjectOfType<RoomMultiplayerMenu>();
                }
                return _roomMultiplayerMenu;
            }
        }
        public static LobbyMenu LobbyMenu
        {
            get
            {
                if (_lobbyMenu == null)
                {
                    _lobbyMenu = GameObject.FindObjectOfType<LobbyMenu>();
                }
                return _lobbyMenu;
            }
        }


        #endregion

        public static int Random(int min, int max)
        {
            return _random.Next(min, max);
        }
        public static void GiveWeapon(WeaponManager weaponManager, string weaponName)
        {
            weaponManager.DHFOMKJHLBM.Add(WeaponManager.transform.Find(weaponName).GetComponent<WeaponScript>());
        }
        public static void RemoveWeapons()
        {
            foreach (WeaponScript weapon in WeaponManager.DHFOMKJHLBM)
            {
                WeaponManager.DHFOMKJHLBM.Remove(weapon);
            }
        }
        public static AudioClip RandomSound(List<AudioClip> sounds)
        {
            return sounds[UnityEngine.Random.Range(0, sounds.Count)];
        }
        static T CopyComponent<T>(T original, GameObject destination) where T : Component
        {
            Il2CppSystem.Type type = original.GetIl2CppType();
            Component copy = destination.AddComponent(type);
            var fields = type.GetFields(Il2CppSystem.Reflection.BindingFlags.Default);
            foreach (Il2CppSystem.Reflection.FieldInfo field in fields)
                field.SetValue(copy, field.GetValue(original));
            return copy as T;
        }

        public static List<FileSystemInfo> GetAllDirectories(string dir)
        {
            DirectoryInfo dirInfo = new DirectoryInfo(dir);
            List<FileSystemInfo> allDirectories = new List<FileSystemInfo>();
            allDirectories.AddRange(dirInfo.GetDirectories("*", SearchOption.AllDirectories));

            return allDirectories;
        }
        public static void SendChatMessage(string senderName, string text, string nameColor = "", string textColor = "")
        {
            // RoomMultiplayerMenu.PGIHKEOJBCH = color;
            MultiplayerChat.HLIDELGJEON(Paint(senderName, nameColor), Paint(text, textColor), "Team A");
            // RoomMultiplayerMenu.PGIHKEOJBCH = new Color(1, 0, 0, 1);
        }
        public static void SetRoomProperty(string property, Il2CppSystem.Object parameter)
        {
            Hashtable newsettings = new Hashtable();
            newsettings[property] = parameter;
            PhotonNetwork.room.SetCustomProperties(newsettings);
        }
        public static void SetRoomProperty(string property, string parameter)
        {
            Hashtable newsettings = new Hashtable();
            newsettings[property] = parameter;
            PhotonNetwork.room.SetCustomProperties(newsettings);
        }
        public static void SetPropertyV2(string property, string parameter)
        {
            Hashtable hashtable = new Hashtable
            {
                [property] = parameter
            };
            PhotonNetwork.player.SetCustomProperties(hashtable);
        }
        public static void SetProperty(string property, string parameter)
        {
            Hashtable hashtable = new Hashtable
            {
                [property] = parameter
            };
            foreach (PlayerProperties pp in GameObject.FindObjectsOfType<PlayerProperties>())
            {
                if (pp.GetComponent<PhotonView>().ownerId == PhotonNetwork.player.ID)
                {
                    pp.SetProperties(hashtable);
                }
            }
        }
        public static void SetProperty(string property, Il2CppSystem.Object parameter)
        {
            Hashtable hashtable = new Hashtable
            {
                [property] = parameter
            };
            foreach (PlayerProperties pp in GameObject.FindObjectsOfType<PlayerProperties>())
            {
                if (pp.GetComponent<PhotonView>().ownerId == PhotonNetwork.player.ID)
                {
                    pp.SetProperties(hashtable);
                }
            }
        }

        public static Il2CppSystem.Object GetRoomProperty(string propertyName)
        {
            return PhotonNetwork.room.CustomProperties[propertyName];
        }
        public static Il2CppSystem.Object GetProperty(PhotonPlayer player, string property)
        {
            try
            {
                foreach (PlayerProperties pp in GameObject.FindObjectsOfType<PlayerProperties>())
                {
                    var pv = pp.gameObject.GetComponent<PhotonView>();
                    if (pv != null && pv.owner.ID == player.ID)
                    {
                        return pp.GetProperties(property);
                    }
                }
                return null;
            }
            catch (Exception e) 
            {
                CuteLogger.Quack($"ебучий нулл референс в GetProperty: {e.Message}");
                return null;
            }
        }
        public static object GetPropertyV2(PhotonPlayer player, string keyName)
        {
            foreach (PlayerProperties pp in GameObject.FindObjectsOfType<PlayerProperties>())
            {
                if (pp.gameObject.GetComponent<PhotonView>().ownerId == player.ID)
                {
                    return pp.photonView.owner.GetCustomPropertiesV2(keyName);
                }
            }
            return null;
        }

        public static string Paint(string text, string color)
        {
            if (color != "") return $"<color={color}>{text}</color>";
            else return text;
        }

        public static PhotonPlayer FindPlayer(string nickName)
        {
            foreach (PhotonPlayer player in PhotonNetwork.playerList)
            {
                if (player.name == nickName) return player;
            }
            return PhotonNetwork.player;
        }
    }
}
