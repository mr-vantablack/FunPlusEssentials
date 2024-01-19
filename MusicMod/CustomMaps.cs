using UnityEngine;
using MelonLoader;
using System.IO;
using System;
using System.Collections.Generic;
using FunPlusEssentials.Essentials;
using FunPlusEssentials.Other;
using FunPlusEssentials.CustomContent.Triggers;
using System.Collections;
using IniFile = FunPlusEssentials.Other.IniFile;
using MelonLoader.TinyJSON;
using UnhollowerBaseLib;
using UnityEngine.UI;
using UnhollowerRuntimeLib;
using Il2CppSystem.Runtime.Remoting.Messaging;
using static FunPlusEssentials.CustomContent.FunNPCInfo;
using InControl.mod;
using System.Text.RegularExpressions;
using System.Linq;
using UnityEngine.SceneManagement;
using Harmony;

namespace FunPlusEssentials.CustomContent
{
    public static class Notifier
    {
        public static GameObject Canvas => GameObject.FindObjectOfType<LGNSystem>().LHAHEJENBJJ.gameObject;
        public static Text Text => Canvas.transform.FindChild("Image/Text").GetComponent<Text>();
        public static Text ButtonText => Canvas.transform.FindChild("Image/ContinueButton/Button").GetComponent<Text>();
        internal static string defaultText;
        internal static string defaultButtonText;
        public static void Show(string text, string buttonText = "Continue")
        {
            if (Canvas == null) return;
            if (Canvas.active) Canvas.SetActive(false);
            defaultText ??= Text.text;
            defaultButtonText ??= ButtonText.text;
            Text.text = text;
            ButtonText.text = buttonText;
            Canvas.SetActive(true);
            ButtonText.gameObject.GetComponent<Button>().onClick.AddListener(DelegateSupport.ConvertDelegate<UnityEngine.Events.UnityAction>(new Action(ApplyDefault)));

        }
        private static void ApplyDefault()
        {
            Text.text = defaultText;
            ButtonText.text = defaultButtonText;
        }
    }

    public class MapInfo
    {
        public LobbyMenu.AllMaps map;
        public List<CustomWave> waves;
        public string[] monsters;
        public List<string> dependencies;
        public bool isOutside;
        public string version;
        public string bundlePath;
        public string assetsPath;
        public AudioClip ambient;
        public bool usingCustomNPCs => dependencies != null && dependencies.Count > 0;
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
        public static bool useCustomNPCs;
        internal static bool m_loaded;
        internal static Il2CppAssetBundle m_loadedBundle;
    
        public static IEnumerator CheckMainMenuOverride()
        {
            Text text = GameObject.Find("Canvas/MainMenu/VersionText").GetComponent<Text>();
            text.verticalOverflow = VerticalWrapMode.Overflow;
            text.text = $"Slendytubbies 3 v{Helper.UpdaterV2.APGCFNOBPHD}\n<color=orange>FunPlusEssentials</color> v{FPE.AppInfo.Version}";
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
                menuBackground = Loader.ConvertTextureToSprite(www.texture, new Vector2(.5f, .5f));

            }
            if (menuBackground != null)
            {
                GameObject.Find("Scene Elements/BG/Background").GetComponent<SpriteRenderer>().sprite = menuBackground;
                Camera.main.GetComponent<CameraFilterPack_Colors_Adjust_PreFilters>().enabled = false;
            }
            if (menuMusic != null)
            {
                var source = GameObject.Find("Sound").GetComponent<AudioSource>();
                source.clip = menuMusic;
                source.Play();
            }
        }
        public static void CheckMapsFolder()
        {
            Directory.CreateDirectory(customMapsDirectory);
            mapsFiles = Helper.GetAllDirectories(customMapsDirectory);
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
                    var dep = i.Read("dependencies", "CustomMap").Split('|');
                    if (dep != null)
                    {
                        mapInfo.dependencies = new List<string>();
                        foreach (string npcName in dep)
                        {
                            if (npcName != null && npcName != "")
                            {
                                mapInfo.dependencies.Add(npcName);
                            }
                        }
                    }
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
            //CheckForCustomContent();
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
            bool useBundle = File.Exists(map.assetsPath + @"\music");
            bool useFolder = Directory.Exists(map.assetsPath + @"\music\");
            if (!useBundle && !useFolder) yield break;
            if (useBundle && useFolder)
            {
                CuteLogger.Bark("You are using two audio load systems at the same time. Delete the \\Music\\ folder or the music bundle.");
                yield break;
            }
            if (useFolder)
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
                        if (File.Exists(FilePath))
                        {
                            string url = @"file://" + FilePath;
                            WWW www = new WWW(url);
                            yield return www;
                            map.waves[i].waveInfo.music = www.GetAudioClip();
                            map.waves[i].waveInfo.music.name = "0" + i.ToString();
                            CuteLogger.Meow("Loaded track " + "0" + i.ToString());
                        }
                    }
                }
            }
            if (useBundle)
            {
                m_loadedBundle = BundleManager.LoadBundle(map.assetsPath + @"\music");
                map.ambient = m_loadedBundle.LoadAsset<AudioClip>("ambient");
                for (int i = 1; i < map.waves.Count; i++)
                {
                    map.waves[i].waveInfo.music = m_loadedBundle.LoadAsset<AudioClip>("wave" + (i + 1).ToString());
                    map.waves[i].waveInfo.music.name = "0" + i.ToString();
                    CuteLogger.Meow("Loaded track " + "0" + i.ToString());
                }
                m_loadedBundle.Unload(false);
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
        public static void CheckForCustomContent()
        {
            string customNPCs = "";
            if (isCustomMap)
            {
                if (currentMap.usingCustomNPCs)
                {
                    for (int i = 0; i < currentMap.dependencies.Count; i++)
                    {
                        var npc = NPCManager.CheckNPCInfos(currentMap.dependencies[i]);
                        if (npc != null) customNPCs += npc.name + "|";
                    }
                }
                if (currentMap.waves != null)
                {
                    foreach (CustomWave customWave in currentMap.waves)
                    {
                        var n = NPCManager.CheckNPCInfos(customWave.waveInfo.defaultNPC);
                        if (n != null) customNPCs += n.name + "|";
                        foreach (CustomWave.specialNPC npc in customWave.waveInfo.npc)
                        {
                            var n2 = NPCManager.CheckNPCInfos(npc.npcName);
                            if (n2 != null) customNPCs += n2.name + "|";
                        }
                    }
                }
            }
            if (useCustomNPCs)
            {
                for (int i = 0; i < NPCManager.NPCInfos.Count; i++)
                {
                    var npc = NPCManager.CheckNPCInfos(NPCManager.NPCInfos[i].name);
                    if (npc != null) customNPCs += npc.name + "|";
                }
            }
            string[] words = customNPCs.Split('|');
            string[] distinctWords = words.Distinct().ToArray();
            string output = string.Join("|", distinctWords);
            Helper.SetRoomProperty("customNPCs", output);
            CuteLogger.Meow(PhotonNetwork.room.customProperties["customNPCs"].ToString());
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
}
namespace FunPlusEssentials.CustomContent.Triggers
{
    [RegisterTypeInIl2Cpp]
    public class Trigger : MonoBehaviour
    {
        public Trigger(IntPtr ptr) : base(ptr) { }
        public virtual void OnTriggerStay(Collider coll) { }
        public virtual void OnTriggerEnter(Collider coll) { }
        public virtual void OnTriggerExit(Collider coll) { }
    }
    [RegisterTypeInIl2Cpp]
    public class DamageZone : Trigger
    {
        public DamageZone(IntPtr ptr) : base(ptr) { }

        public bool isDamage;
        public float damage = 5f;
        public float coolDown = 1f;
        private float timer = 0f;
        public override void OnTriggerStay(Collider coll)
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
        public override void OnTriggerExit(Collider coll)
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

    [RegisterTypeInIl2Cpp]
    public class AmbientController : MonoBehaviour
    {
        public AmbientController(IntPtr ptr) : base(ptr) { }

        public static AmbientController Instance { get; set; }
        public Color ambientColor, fogColor;
        public FogMode fogMode { set { RenderSettings.fogMode = value; } }
        public float fogDensity;
        public void Start()
        {
            Instance = this;
            fogColor = RenderSettings.fogColor;
            ambientColor = RenderSettings.ambientLight;
            fogMode = RenderSettings.fogMode;
            fogDensity = RenderSettings.fogDensity;
        }
        public void Update()
        {
            RenderSettings.fogColor = Color.Lerp(RenderSettings.fogColor, fogColor, Mathf.Abs(Mathf.Sin(Time.time)));
            RenderSettings.ambientLight = Color.Lerp(RenderSettings.ambientLight, ambientColor, Mathf.Abs(Mathf.Sin(Time.time)));
            RenderSettings.fogDensity = Mathf.Lerp(RenderSettings.fogDensity, fogDensity, Mathf.Abs(Mathf.Sin(Time.time)));
        }
    }

    [RegisterTypeInIl2Cpp]
    public class AmbientTriggerEnter : Trigger
    {
        public AmbientTriggerEnter(IntPtr ptr) : base(ptr) { }

        public Color defaultAmbientColor, defaultFogColor, newAmbientColor, newFogColor;
        public FogMode defaultFogMode, newFogMode;
        public float defaultFogDensity, newFogDensity;


        public override void OnTriggerEnter(Collider coll)
        {
            AmbientController.Instance.fogColor = newFogColor;
            AmbientController.Instance.fogDensity = newFogDensity;
            AmbientController.Instance.fogMode = newFogMode;
            AmbientController.Instance.ambientColor = newAmbientColor;
        }
        public override void OnTriggerExit(Collider coll)
        {
            AmbientController.Instance.fogColor = defaultFogColor;
            AmbientController.Instance.fogDensity = defaultFogDensity;
            AmbientController.Instance.fogMode = defaultFogMode;
            AmbientController.Instance.ambientColor = defaultAmbientColor;
        }
    }
}