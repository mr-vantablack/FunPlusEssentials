using UnityEngine;
using MelonLoader;
using System.IO;
using System;
using System.Collections.Generic;
using FunPlusEssentials.Essentials;
using FunPlusEssentials.Other;
using System.Collections;
using IniFile = FunPlusEssentials.Other.IniFile;
using MelonLoader.TinyJSON;
using UnhollowerBaseLib;

namespace FunPlusEssentials.CustomContent
{
    public class MapInfo
    {
        public LobbyMenu.AllMaps map;
        public List<CustomWave> waves;
        public string[] monsters;
        public bool isOutside;
        public string version;
        public string bundlePath;
        public string assetsPath;
        public AudioClip ambient;
    }
    public class CustomWave
    {
        public int maxNPC = 15;
        public CustomWaveInfo waveInfo;


        public class CustomWaveInfo
        {
            public int totalCount = 5;

            public AudioClip music;

            public string defaultNPC = "Newborn_Bot";

            public specialNPC[] npc;

        }
        public class specialNPC
        {
            public string npcName;
            public int spawnChance;
            public bool isBoss;
        }
    }

    public static class MapManager
    {
        public static bool isCustomMap;
        public static MapInfo currentMap;
        public static string customMapsDirectory = Config.mainPath + @"\CustomMaps";
        public static string mainMenuDirectory = Config.mainPath + @"\MainMenu";
        public static List<FileSystemInfo> mapsFiles = new List<FileSystemInfo>();
        public static List<MapInfo> customMaps = new List<MapInfo>();
        public static AudioClip menuMusic;
        public static Sprite menuBackground;
        internal static bool m_loaded;
        internal static List<LobbyMenu.AllModes> m_allModes = new List<LobbyMenu.AllModes>();
        internal static Il2CppSystem.Collections.Generic.List<LobbyMenu.AllModes> m_lastModes = new Il2CppSystem.Collections.Generic.List<LobbyMenu.AllModes>();
        internal static Il2CppAssetBundle m_loadedBundle;

        static List<FileSystemInfo> GetAllDirectories(string dir)
        {
            DirectoryInfo dirInfo = new DirectoryInfo(dir);
            List<FileSystemInfo> allDirectories = new List<FileSystemInfo>();
            allDirectories.AddRange(dirInfo.GetDirectories("*", SearchOption.AllDirectories));

            return allDirectories;
        }
        public static void SetUpMainMenu()
        {
            
            if (menuBackground != null)
            {
                GameObject.Find("Scene Elements/BG/Background").GetComponent<SpriteRenderer>().sprite = menuBackground;
            }
            if (menuMusic != null)
            {
                var source = GameObject.Find("Sound").GetComponent<AudioSource>();
                source.clip = menuMusic;
                source.Play();
            }
        }
        public static IEnumerator CheckMainMenuOverride()
        {
            Directory.CreateDirectory(mainMenuDirectory);
            if (File.Exists(mainMenuDirectory + @"\ambient.mp3"))
            {
                WWW www = new WWW(@"file://" + mainMenuDirectory + @"\ambient.mp3");
                yield return www;
                if (menuMusic == null)
                {
                    menuMusic = www.GetAudioClip(true, true);
                }
                
            }
            if (File.Exists(mainMenuDirectory + @"\background.png"))
            {
                WWW www = new WWW(@"file://" + mainMenuDirectory + @"\background.png");
                yield return www;
                CuteLogger.Meow(www.texture.width.ToString() + " " + www.texture.height.ToString());
                menuBackground = Loader.ConvertTextureToSprite(www.texture, new Vector2(.5f, .5f));

            }
        }
        public static void CheckMapsFolder()
        {
            Directory.CreateDirectory(customMapsDirectory);
            mapsFiles = GetAllDirectories(customMapsDirectory);
            foreach (FileSystemInfo dir in mapsFiles)
            {
                string iniPath = dir.FullName + @"\map.ini";
                string wavesPath = dir.FullName + @"\waves.json";
                string assets = dir.FullName;
                string bundlePath = dir.FullName + @"\bundle";
                if (!File.Exists(bundlePath))
                {
                    return;
                    //BundleManager.LoadSceneBundle("", mapInfo.map.mapName, bundlePath);
                }              
                if (File.Exists(iniPath))
                {
                    List<CustomWave> w = null;
                    if (File.Exists(wavesPath))
                    {
                        var json = File.ReadAllText(wavesPath);
                        w = JSON.Load(json).Make<List<CustomWave>>();                    
                    }
                    var i = new IniFile(iniPath);
                    var mapInfo = new MapInfo()
                    {
                        map = new LobbyMenu.AllMaps()
                        {
                            mapName = i.Read("mapName", "CustomMap"),
                            useDayAndNight = Convert.ToBoolean(i.Read("useDayAndNight", "CustomMap")),
                            size = Convert.ToInt32(i.Read("size", "CustomMap")),
                            mapPreview = null
                        },
                        monsters = new string[]
                        {
                            i.Read("collectMonster", "CustomMap"),
                            i.Read("versusMonster", "CustomMap"),
                            i.Read("survivalMonster", "CustomMap")
                        },
                        isOutside = Convert.ToBoolean(i.Read("isOutside", "CustomMap")),
                        version = i.Read("version", "CustomMap"),
                        bundlePath = bundlePath,
                        assetsPath = assets,
                        waves = w
                    };
                    customMaps.Add(mapInfo);
                }
            }
        }
        public static void CheckForCustomMap(string mapName)
        {
            m_loaded = false;
            isCustomMap = false;
            var scene = mapName;
            scene = scene.Replace(" (Day)", "");
            scene = scene.Replace(" (Dusk)", "");
            scene = scene.Replace(" (Night)", "");
            foreach (MapInfo map in customMaps)
            {
                if (scene == map.map.mapName)
                {
                    isCustomMap = true;
                    currentMap = customMaps.Find(m => m.map.mapName == scene);
                    MelonCoroutines.Start(SpawnRoom());
                }
            }
        }

        public static void SetUp()
        {
            CheckMapsFolder();
        }
        public static void AddCustomMaps()
        {
            foreach (MapInfo mapInfo in customMaps)
            {
                if (File.Exists(mapInfo.assetsPath + @"\preview.png"))
                {
                    if (mapInfo.map.mapPreview == null)
                    {
                        mapInfo.map.mapPreview = Loader.LoadNewSprite(mapInfo.assetsPath + @"\preview.png");
                    }
                }
                Helper.LobbyMenu.CFJBONPPILK.System_Collections_IList_Add(mapInfo.map);
            }
        }
  
        public static void LoadMap(MapInfo map)
        {
            BundleManager.LoadSceneBundle("", map.map.mapName, map.bundlePath);
        }

        public static IEnumerator SetUpMusic(MapInfo map)
        {
            if (Directory.Exists(map.assetsPath + @"\music\"))
            {
                if (File.Exists(map.assetsPath + @"\music\" + "ambient.mp3"))
                {
                    WWW www = new WWW(@"file://" + map.assetsPath + @"\music\" + "ambient.mp3");
                    yield return www;
                    map.ambient = www.GetAudioClip();
                }
                if (map.waves != null)
                {
                    for (int i = 1; i < map.waves.Count; i++)
                    {
                        string FilePath = map.assetsPath + @"\music\" + "wave" + (i + 1).ToString() + ".mp3";
                        CuteLogger.Meow(FilePath);
                        if (File.Exists(FilePath))
                        {
                            string url = @"file://" + FilePath;
                            WWW www = new WWW(url);
                            yield return www;
                            map.waves[i].waveInfo.music = www.GetAudioClip();
                            map.waves[i].waveInfo.music.name = "0" + i.ToString();
                        }
                    }
                }
            }
            else
            {
                string path = map.assetsPath + @"\music";
                if (File.Exists(path))
                {
                    m_loadedBundle = BundleManager.LoadBundle(path);
                    map.ambient = m_loadedBundle.LoadAsset<AudioClip>("ambient");
                    for (int i = 1; i < map.waves.Count; i++)
                    {
                        CuteLogger.Meow(path);
                        map.waves[i].waveInfo.music = m_loadedBundle.LoadAsset<AudioClip>("wave" + (i + 1).ToString());
                        map.waves[i].waveInfo.music.name = "0" + i.ToString();
                    }
                    m_loadedBundle.Unload(false);
                }
            }
            for (int i = 1; i < map.waves.Count; i++)
            {
                if (GameObject.Find("__Room") != null)
                {
                    Helper.SurvivalMechanics.LKKBNLDMNDL[i].music = map.waves[i].waveInfo.music;
                }
            }
            yield return null;
        }


        public static IEnumerator SetUpWaves(SurvivalMechanics __instance)
        {
            if (isCustomMap)
            {
                if (currentMap.waves != null)
                {
                    __instance.GFHEICDOFEP = currentMap.isOutside;
                    __instance.LKKBNLDMNDL.Clear();
                    int t = 0;
                    foreach (CustomWave wave in currentMap.waves)
                    {
                        t++;
                        var w = new SurvivalMechanics.waveInfo()
                        {
                            defaultNPC = wave.waveInfo.defaultNPC,
                            totalCount = wave.waveInfo.totalCount,
                            music = wave.waveInfo.music

                        };
                        if (wave.waveInfo.npc != null)
                        {
                            var arr = new SurvivalMechanics.specialNPC[wave.waveInfo.npc.Length];
                            for (int i = 0; i < wave.waveInfo.npc.Length; i++)
                            {
                                arr[i] = new SurvivalMechanics.specialNPC()
                                {
                                    npcName = wave.waveInfo.npc[i].npcName,
                                    spawnChance = wave.waveInfo.npc[i].spawnChance,
                                    isBoss = wave.waveInfo.npc[i].isBoss
                                };
                            }
                            w.NPC = new Il2CppReferenceArray<SurvivalMechanics.specialNPC>(arr);
                        }
                        else
                        {
                            w.NPC = new Il2CppReferenceArray<SurvivalMechanics.specialNPC>(0L);
                        }
                        __instance.LKKBNLDMNDL.Add(w);
                    }
                    __instance.LKKBNLDMNDL[0].music = Helper.Room.GetComponent<AudioSource>().clip;
                    yield return null;
                }
                else
                {
                    __instance.FNKMHJJNLJF.defaultNPC = currentMap.monsters[2];
                    var bossMusic = Loader.LoadAudioClip(currentMap.assetsPath + @"\boss.mp3");
                    if (bossMusic != null) __instance.FNKMHJJNLJF.music = bossMusic;
                    __instance.FNKMHJJNLJF.NPC[0].npcName = currentMap.monsters[2];
                    __instance.LKKBNLDMNDL[0].music = Helper.Room.GetComponent<AudioSource>().clip;
                }
            }

        }
        public static IEnumerator SpawnRoom()
        {
            if (PhotonNetwork.isMasterClient)
            {
                Helper.SetRoomProperty("mapVersion", currentMap.version);
                MelonLogger.Msg(PhotonNetwork.room.customProperties["mapVersion"].ToString());
                // yield return new WaitForSeconds(1f);
                PhotonNetwork.NOOU("__Room", Vector3.zero, new Quaternion(0f, 0f, 0f, 0f), 0, null);
                RoomMultiplayerMenu roomMultiplayerMenu = UnityEngine.Object.FindObjectOfType<RoomMultiplayerMenu>();
                roomMultiplayerMenu.gameObject.name = "__Room";
                //FillWeaponsArray(roomMultiplayerMenu);
            }
            SetUpTriggers();
            SetUpAudioSources();
            yield return null;
        }
        public static void SetUpAudioSources()
        {
            var mixer = GameObject.FindObjectOfType<AudioMixerManager>().EEJANKGKOBO;
            foreach (AudioSource s in GameObject.FindObjectsOfType<AudioSource>())
            {
                if (s.gameObject.name.Contains("Room"))
                {
                    s.outputAudioMixerGroup = mixer.FindMatchingGroups("Music")[0];
                    continue;
                }
                s.volume = 1;
                if (s.clip != null && s.clip.length >= 30)
                {
                    s.outputAudioMixerGroup = mixer.FindMatchingGroups("Music")[0];
                }
                if (s.clip != null && s.clip.length < 30)
                {
                    s.outputAudioMixerGroup = mixer.FindMatchingGroups("SFX")[0];
                }
            }
        }
        public static void SetUpTriggers()
        {
            var kz = GameObject.Find("Killzones");
            if (kz != null)
            {
                foreach (Transform t in kz.transform.GetComponentsInChildren<Transform>())
                {
                    t.gameObject.AddComponent<KillZone>();
                }
            }
            var dz = GameObject.Find("Damage zones");
            if (dz != null)
            {
                foreach (Transform t in dz.transform.GetComponentsInChildren<Transform>())
                {
                    var s = t.name.Split(' ');
                    if (!float.TryParse(s[0], out float damage)) { damage = 5f; }
                    if (!float.TryParse(s[1], out float coolDown)) { coolDown = 1f; }
                    var comp = t.gameObject.AddComponent<DamageZone>();
                    if (damage > 0) comp.isDamage = true;
                    comp.damage = damage;
                    comp.coolDown = coolDown;
                }
            }
            var wz = GameObject.Find("Water zones");
            if (wz != null)
            {
                foreach (Transform t in wz.transform.GetComponentsInChildren<Transform>())
                {
                    t.name = t.name + " ";
                    var s = t.name.Split(' ');
                    if (!float.TryParse(s[0], out float gravity)) { gravity = 1f; }
                    if (!float.TryParse(s[1], out float damage)) { damage = 5f; }
                    if (!float.TryParse(s[2], out float coolDown)) { coolDown = 1f; }
                    var comp = t.gameObject.AddComponent<WaterZone>();
                    if (damage > 0) comp.isDamage = true;
                    comp.gravity = gravity;
                    comp.damage = damage;
                    comp.coolDown = coolDown;
                }
            }
        }
    }

    [RegisterTypeInIl2Cpp]
    public class DamageZone : MonoBehaviour
    {
        public DamageZone(IntPtr ptr) : base(ptr) { }

        public bool isDamage;
        public float damage = 5f;
        public float coolDown = 1f;
        private float timer = 0f;
        public virtual void OnTriggerStay(Collider coll)
        {
            if (isDamage)
            {
                if (coll.gameObject.tag == "Player")
                {
                    if (timer > 0)
                    {
                        timer -= Time.deltaTime;
                    }

                    if (timer <= 0)
                    {
                        timer = coolDown;
                        Helper.PlayerDamage.E10030(damage);
                    }
                }
            }
        }
        public virtual void OnTriggerExit(Collider coll)
        {
            if (coll.gameObject.tag == "Player")
            {
                timer = 0f;
            }
        }
    }
    [RegisterTypeInIl2Cpp]
    public class WaterZone : DamageZone
    {
        public WaterZone(IntPtr ptr) : base(ptr) { }
        public float gravity = 4f;

        public virtual void OnTriggerEnter(Collider coll)
        {
            if (coll.gameObject.tag == "Player")
            {
                Camera.main.gameObject.AddComponent<CameraFilterPack_Blur_Blurry>();
                var rgb = Camera.main.gameObject.AddComponent<CameraFilterPack_Color_RGB>();
                rgb.EDGGOPGCNLE = new Color(0.2096f, 0.8476f, 1f, 1f);
                Helper.FPSController.HKCDMBALAAK.gravity = gravity;
                Helper.FPSController.HKCDMBALAAK.maxFallSpeed = gravity;
                Helper.FPSController.LGIGJCDJMNO.fallLimit = 9999999f;
            }
        }
        public override void OnTriggerExit(Collider coll)
        {
            base.OnTriggerExit(coll);
            if (coll.gameObject.tag == "Player")
            {
                Destroy(Camera.main.gameObject.GetComponent<CameraFilterPack_Blur_Blurry>());
                Destroy(Camera.main.gameObject.GetComponent<CameraFilterPack_Color_RGB>());
                Helper.FPSController.HKCDMBALAAK.gravity = 20f;
                Helper.FPSController.HKCDMBALAAK.maxFallSpeed = 100f;
                Helper.FPSController.LGIGJCDJMNO.fallLimit = 0.85f;
            }
        }
    }
}
