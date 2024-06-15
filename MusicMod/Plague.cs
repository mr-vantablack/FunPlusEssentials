using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using CodeStage.AntiCheat.Storage;
using FunPlusEssentials.CustomContent;
using FunPlusEssentials.Essentials;
using FunPlusEssentials.Other;
using FunPlusEssentials.Patches;
using HarmonyLib;
using Il2CppSystem;
using Il2CppSystem.Reflection;
using MelonLoader;
using UnhollowerBaseLib;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;
using static Il2CppSystem.Guid;
using static MelonLoader.MelonLogger;
using static PunTeams;
using static ShopSystem;
using BindingFlags = System.Reflection.BindingFlags;
using IntPtr = System.IntPtr;
using MethodInfo = Il2CppSystem.Reflection.MethodInfo;


namespace FunPlusEssentials
{
    public static class Plague
    {
        public static bool Enabled { get; set; }
    }

    [RegisterTypeInIl2Cpp, UsingRPC]
    public class PlagueMonster : MonoBehaviour
    {
        public PlagueMonster(IntPtr ptr) : base(ptr) { }
        public AudioSource _soundsSource;
        public PhotonView _photonView;
        public PlayerMonster _playerMonster;
        public FPScontroller _fps;
        public float _hp;
        public float _currentHp;
        public PlagueClass _class;
        public GameObject _HUD, _healthBar;

        public void Awake()
        {
            
            _soundsSource = gameObject.AddComponent<AudioSource>();
            var mixer = FindObjectOfType<AudioMixerManager>().EEJANKGKOBO;
            _soundsSource.outputAudioMixerGroup = mixer.FindMatchingGroups("SFX")[0];
            _soundsSource.spatialBlend = 1f;
            _soundsSource.rolloffMode = AudioRolloffMode.Linear;
            _soundsSource.minDistance = 4f;
            _soundsSource.maxDistance = 30f;
            _soundsSource.spread = 100f;
            _photonView = GetComponent<PhotonView>();
            _playerMonster = GetComponent<PlayerMonster>();
            _fps = GetComponent<FPScontroller>();
            _photonView.synchronization = ViewSynchronization.UnreliableOnChange;
            PhotonManager.RegisterSerializeView(_photonView, this);
            if (_photonView.isMine)
            {
                _HUD = new GameObject("HUD");
                _HUD.transform.SetParent(base.gameObject.transform);
                _HUD.AddComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
                _HUD.AddComponent<CanvasScaler>().referenceResolution = new Vector2(800f, 600f);
                GameObject gameObject = new GameObject("HealthBarBackground");
                gameObject.transform.SetParent(_HUD.transform);
                gameObject.AddComponent<CanvasRenderer>();
                gameObject.AddComponent<Image>().color = new Color(0f, 0f, 0f, 0.5f);
                gameObject.GetComponent<RectTransform>().anchoredPosition = new Vector2(100f, 16f);
                gameObject.GetComponent<RectTransform>().anchorMax = new Vector2(0.8f, 0.033f);
                gameObject.GetComponent<RectTransform>().anchorMin = new Vector2(0.65f, 0f);
                gameObject.GetComponent<RectTransform>().offsetMax = new Vector2(190f, 22f);
                gameObject.GetComponent<RectTransform>().offsetMin = new Vector2(10f, 10f);
                gameObject.GetComponent<RectTransform>().sizeDelta = new Vector2(180f, 12f);
                gameObject.GetComponent<RectTransform>().pivot = new Vector2(0f, -2f);
                _healthBar = new GameObject("HealthBar");
                _healthBar.transform.SetParent(_HUD.transform);
                _healthBar.AddComponent<CanvasRenderer>();
                _healthBar.AddComponent<Image>().color = new Color(1f, 0f, 0f, 0.5f);
                _healthBar.GetComponent<RectTransform>().anchoredPosition = new Vector2(1f, 0f);
                _healthBar.GetComponent<RectTransform>().anchorMax = new Vector2(0.798f, 0.031f);
                _healthBar.GetComponent<RectTransform>().anchorMin = new Vector2(0.652f, 0.002f);
                _healthBar.GetComponent<RectTransform>().offsetMax = new Vector2(190f, 22f);
                _healthBar.GetComponent<RectTransform>().offsetMin = new Vector2(10f, 10f);
                _healthBar.GetComponent<RectTransform>().sizeDelta = new Vector2(180f, 12f);
                _healthBar.GetComponent<RectTransform>().pivot = new Vector2(0f, -2f);
            }
        }
        public void SetUp()
        {
            _playerMonster.PIIDNIGPCDK = _class.Health;
            _playerMonster.OMFJFIPPGPE = (int)_class.Damage;
            _fps.AALHECCKHFD.baseHeight = _class.JumpHeight;
            _fps.HKCDMBALAAK.RunSpeed = _class.RunSpeed;
        }

        [FunRPC]
        public void InfectedSoundRPC()
        {
            CuteLogger.Meow("InfectedSoundRPC");
            _soundsSource.PlayOneShot(Helper.RandomSound(PlagueAssets.Instance._infectedSounds));
        }
        public void OnPhotonSerializeView(Il2CppSystem.Object stream, Il2CppSystem.Object info)
        {
            var s = stream.Cast<PhotonStream>();
            var i = info.Cast<PhotonMessageInfo>();
            if (_playerMonster != null)
            {
                if (s.isWriting)
                {
                    s.SendNext(new Il2CppSystem.Single() { m_value = _playerMonster.PIIDNIGPCDK }.BoxIl2CppObject());
                }
                else
                {
                    _playerMonster.PIIDNIGPCDK = s.ReceiveNext().Unbox<float>();
                }
            }
        }
        public void OnGUI()
        {
            if (_photonView.isMine)
            {
                if (_playerMonster.PIIDNIGPCDK >= 0f)
                {
                    _healthBar.transform.localScale = new Vector3(_playerMonster.PIIDNIGPCDK / _class.Health, 1f, 1f);
                    return;
                }
                _healthBar.transform.localScale = Vector3.zero;
            }
        }
    }
    public enum PlagueMode
    {
        Infection,
        Cursed,
        LastSurvivor,
        Nemesis,
        Massacre,
        ARMAGEDDON
    }
    public class PlagueClass
    {
        public string ClassName;
        public float Armor;
        public float Health;
        public float RunSpeed;
        public float JumpHeight;
        public float Damage;
        public string Prefab;
        public List<string> Weapons;
    }

    [RegisterTypeInIl2Cpp]
    public class PlayerClass : MonoBehaviour
    {
        public PlayerClass(IntPtr ptr) : base(ptr) { }
        public PhotonView photonView;
        public PlayerMonster playerMonster;
        public string hp;
        public void Awake()
        {
            hp = "s";
            photonView = GetComponent<PhotonView>();
            playerMonster = GetComponent<PlayerMonster>();

            PhotonManager.RegisterSerializeView(photonView, this);
            // foreach (var methodinfo in type.GetMethods((Il2CppSystem.Reflection.BindingFlags)AccessTools.all))
            // {
            //  CuteLogger.Meow(methodinfo.Name);
            //}
        }
        public void Test3(int N, float d)
        { }
        public void Test2(int s)
        { }
        public void Test(Il2CppSystem.Object KOHGOLCGCAN, Il2CppSystem.Object BPDJHBDKJDN)
        {
            //(PhotonStream)KOHGOLCGCAN
        }
        public void OnSerialize()
        {
            CuteLogger.Meow(Il2CppSystem.Reflection.MethodInfo.GetCurrentMethod().Name);
        }
        public void OnPhotonSerializeView(Il2CppSystem.Object KOHGOLCGCAN, Il2CppSystem.Object BPDJHBDKJDN)
        {
            if (KOHGOLCGCAN == null)
            {
                CuteLogger.Meow(Il2CppSystem.Reflection.MethodInfo.GetCurrentMethod().Name);
                return;
            }
            else
            {
                var s = KOHGOLCGCAN.Cast<PhotonStream>();
                var m = BPDJHBDKJDN.Cast<PhotonMessageInfo>();
                if (s.isWriting)
                {
                    s.SendNext(hp);
                }
                else
                {
                    hp = s.ReceiveNext().ToString();
                }
            }
        }
        public void OnGUI()
        {

        }
    }
    [RegisterTypeInIl2Cpp]
    public class PlagueAssets : MonoBehaviour
    {
        public PlagueAssets(IntPtr ptr) : base(ptr) { }
        public List<AudioClip> _infectedWinSounds, _survivorsWinSounds, _infectedSounds, _roundStartSounds;
        public AudioClip _ambience, _countdownSound;
        public GameObject _bullet, _meleeBullet;
        public static bool _inited;
        public static PlagueAssets Instance { get; set; }

        public void Awake()
        {
            // DontDestroyOnLoad(this);
            Instance = this;
            MelonCoroutines.Start(LoadAssets());
            DontDestroyOnLoad(gameObject);
        }
        public IEnumerator LoadAssets()
        {
            _infectedWinSounds = new List<AudioClip>();
            _survivorsWinSounds = new List<AudioClip>();
            _infectedSounds = new List<AudioClip>();
            _roundStartSounds = new List<AudioClip>();
            var assetBundleCreateRequest = Il2CppAssetBundleManager.LoadFromFile(Config.mainPath + @"\Plague\plague assets");
            yield return assetBundleCreateRequest;
            for (int i = 1; i < 5; i++)
            {
                _survivorsWinSounds.Add(assetBundleCreateRequest.Load<AudioClip>($"survivorsWin{i}"));
                _infectedSounds.Add(assetBundleCreateRequest.Load<AudioClip>($"infected{i}"));
                _roundStartSounds.Add(assetBundleCreateRequest.Load<AudioClip>($"roundStart{i}"));
            }
            for (int i = 1; i < 6; i++)
            {
                _infectedWinSounds.Add(assetBundleCreateRequest.Load<AudioClip>($"infectedWin{i}"));
            }
            for (int i = 1; i < 3; i++)
            {
                // _roundStartSounds.Add(assetBundleCreateRequest.Load<AudioClip>($"prepare{i}"));
            }
            _ambience = assetBundleCreateRequest.Load<AudioClip>($"ambience1");
            _countdownSound = assetBundleCreateRequest.Load<AudioClip>($"countdown1");
            //_skybox = assetBundleCreateRequest.Load<Material>($"skybox");
            assetBundleCreateRequest.Unload(false);
            foreach (AudioClip clip in _infectedWinSounds)
            {
                clip.hideFlags = HideFlags.DontUnloadUnusedAsset;
            }
            foreach (AudioClip clip in _survivorsWinSounds)
            {
                clip.hideFlags = HideFlags.DontUnloadUnusedAsset;
            }
            foreach (AudioClip clip in _infectedSounds)
            {
                clip.hideFlags = HideFlags.DontUnloadUnusedAsset;
            }
            foreach (AudioClip clip in _roundStartSounds)
            {
                clip.hideFlags = HideFlags.DontUnloadUnusedAsset;
            }
            _countdownSound.hideFlags = HideFlags.DontUnloadUnusedAsset;
        }
    }

    [RegisterTypeInIl2Cpp, UsingRPC]
    public class PlagueController : MonoBehaviour
    {
        public PlagueController(IntPtr ptr) : base(ptr) { }

        #region PROPERTY_NAMES
        private readonly string RefTime = "RefTime'";
        private readonly string CDTime = "CDTime'";
        #endregion
        public float RestartTime = 10f;
        public float CountdownTime = 10f;
        public float PlayerListUpdateTime = 0.05f;

        private RoomMultiplayerMenu rmm => Helper.RoomMultiplayerMenu;
        public static PlagueController Instance { get; set; }


        private Material _skybox;
        public AudioSource _musicSource, _soundsSource, _otherSource;
        private bool _countdownPlayed, _winPlayed, _roundStartPlayed;

        public float _armor, _jumpHeight, _runSpeed;
        public PlagueClass _playerClass;

        public int _testint;

        private bool _plagueStarted, _restarting, _survivorsWin, _infectedWin;
        private bool _waitingForPlayers, _playersListUpdated, _lpFlag;
        private bool _refTimeSet, _classSet;
        private bool _debugMode;
        private float _referenceTime, _startCdTime;
        private float _plagueTimer, _countDown;
        private float _roundDuration;
        private int _totalRounds;
        private PlagueMode _mode;
        private List<PhotonPlayer> _allPlayers, _teamAPlayers, _teamBPlayers;

        public void SurvivorClassSetUp(int classID)
        {
            CuteLogger.Meow($"SurvivorClassSetUp: {classID.ToString()}");
            _playerClass = new PlagueClass();
            if (classID == 1)
            {
                _playerClass.ClassName = "Soldier";
                _playerClass.Weapons = new List<string>()
                {
                    "XIX",
                    "MP5N"
                };
                _playerClass.Armor = 25;
                _playerClass.RunSpeed = 10f;
                _playerClass.JumpHeight = 1f;
            }
            if (classID == 2)
            {
                _playerClass.ClassName = "Medic";
                _playerClass.Weapons = new List<string>()
                {
                    "VZ61",
                    "MCS870"
                };
                _playerClass.Armor = 10;
                _playerClass.RunSpeed = 10.25f;
                _playerClass.JumpHeight = 1.25f;
            }
            if (classID == 3)
            {
                _playerClass.ClassName = "Heavy";
                _playerClass.Weapons = new List<string>()
                {
                    "Shorty",
                    "MCS870",
                    "Grenade Launcher"
                };
                _playerClass.Armor = 50;
                _playerClass.RunSpeed = 9f;
                _playerClass.JumpHeight = 0.8f;
            }
            if (classID == 4)
            {
                _playerClass.ClassName = "Sniper";
                _playerClass.Weapons = new List<string>()
                {
                    "XIX II",
                    "M40A3"
                };
                _playerClass.Armor = 0;
                _playerClass.RunSpeed = 11f;
                _playerClass.JumpHeight = 2f;
            }
            Helper.FPSController.AALHECCKHFD.baseHeight = _playerClass.JumpHeight;
            Helper.FPSController.HKCDMBALAAK.RunSpeed = _playerClass.RunSpeed;
            // Helper.RemoveWeapons();
            foreach (string weapon in _playerClass.Weapons)
            {
                Helper.GiveWeapon(weapon);
            }
            //Helper.SetProperty("MaxHP", _playerClass.Health.ToString());
            Helper.SetProperty("Armor", new Il2CppSystem.Single() { m_value = _playerClass.Armor }.BoxIl2CppObject());
            //damageMultiplier
            _classSet = true;
        }
        public PlagueClass InfectedClassSetUp(int classID)
        {
            CuteLogger.Meow($"InfectedClassSetUp: {classID.ToString()}");
            _playerClass = new PlagueClass();
            if (classID == 1)
            {
                _playerClass.ClassName = "Runner";
                _playerClass.RunSpeed = 11.5f;
                _playerClass.JumpHeight = 3f;
                _playerClass.Health = 60;
                _playerClass.Damage = 40;
                _playerClass.Prefab = "INF/PlayerNewborn";
            }
            if (classID == 2)
            {
                _playerClass.ClassName = "Jumper";
                _playerClass.RunSpeed = 10.5f;
                _playerClass.JumpHeight = 4.5f;
                _playerClass.Health = 55;
                _playerClass.Damage = 30;
                _playerClass.Prefab = "VS/PlayerNewborn";
            }
            if (classID == 3)
            {
                _playerClass.ClassName = "Tank";
                _playerClass.RunSpeed = 9.5f;
                _playerClass.JumpHeight = 2.5f;
                _playerClass.Health = 400;
                _playerClass.Damage = 101;
                _playerClass.Prefab = "VS/PlayerGeneral";
            }
            if (classID == 4)
            {
                _playerClass.ClassName = "Berserker";
                _playerClass.RunSpeed = 10.5f;
                _playerClass.JumpHeight = 3f;
                _playerClass.Health = 150;
                _playerClass.Damage = 30;
                _playerClass.Prefab = "VS/PlayerDroid";
            }

            _classSet = true;
            return _playerClass;
        }
        void SetUpAudio()
        {
            _musicSource = rmm.gameObject.GetComponent<AudioSource>();
            _soundsSource = rmm.gameObject.AddComponent<AudioSource>();
            _otherSource = rmm.gameObject.AddComponent<AudioSource>();
            var mixer = FindObjectOfType<AudioMixerManager>().EEJANKGKOBO;
            var music = mixer.FindMatchingGroups("Music")[0];
            var sounds = mixer.FindMatchingGroups("SFX")[0];
            _soundsSource.outputAudioMixerGroup = sounds;
            _otherSource.outputAudioMixerGroup = sounds;
        }
        void Awake()
        {
            _countdownPlayed = true;
            _mode = PlagueMode.Infection;
            Instance = this;
            _totalRounds = 0;
            SetUpAudio();
            //  RenderSettings.skybox = _skybox;
            _allPlayers = new List<PhotonPlayer>();
            _teamAPlayers = new List<PhotonPlayer>();
            _teamBPlayers = new List<PhotonPlayer>();
            _roundDuration = float.Parse(Helper.GetRoomProperty("RD004'").ToString());
            if (PhotonNetwork.isMasterClient)
            {
                OnPhotonPlayerConnected.onPhotonPlayerConnected += OnPlayerJoined;
                OnPhotonPlayerDisconnected.onPhotonPlayerDisonnected += OnPlayerLeft;
                _waitingForPlayers = true;
                _musicSource.loop = true;
                _musicSource.PlayOneShot(PlagueAssets.Instance._ambience);
                _startCdTime = (float)PhotonNetwork.time;
                _countDown = _startCdTime;
                Helper.SetRoomProperty(CDTime, new Il2CppSystem.Single() { m_value = _startCdTime }.BoxIl2CppObject());
            }
            else
            {
                _referenceTime = Helper.GetRoomProperty(RefTime).Unbox<float>();
                _startCdTime = Helper.GetRoomProperty(CDTime).Unbox<float>();
                _countDown = _startCdTime;
                _refTimeSet = 20f - ((float)PhotonNetwork.time - _startCdTime) <= 0f;
            }
            //else
            //{
            //    if (Helper.GetRoomProperty(RefTime) == null || Helper.GetRoomProperty(RefTime).ToString() == "0")
            //    {
            //        rmm.photonView.RPC(nameof(TellMasterFirstPlayerConnected), PhotonTargets.MasterClient, null);
            //        CuteLogger.Meow("Ref Time!");
            //    }
            //    CuteLogger.Meow("NON-HOST AWAKE");
            //    _referenceTime = Helper.GetRoomProperty(RefTime).Unbox<float>();
            //}
        }
        void Start()
        {
            if (PhotonNetwork.isMasterClient)
            {
                PhotonNetwork.room.IsOpen = false;
                PhotonNetwork.room.IsVisible = false;
            }
        }

        public void OnPlayerJoined(PhotonPlayer player)
        {

        }
        public void OnPlayerLeft(PhotonPlayer player)
        {

        }


        void LateUpdate()
        {
            if (Input.GetKeyDown(KeyCode.Keypad7))
            {
                rmm.SpawnPlayer("Team B");
            }
            if (Input.GetKeyDown(KeyCode.Keypad8))
            {
                rmm.SpawnPlayer("Team A");
            }
            if (Input.GetKeyDown(KeyCode.Keypad9))
            {
                _testint++;
            }
        }
        void Update()
        {
            Cursor.visible = rmm.IOMIAHNBDOG;
            _plagueStarted = !_waitingForPlayers && _countDown <= 0f && !_restarting;
            _waitingForPlayers = false; //_allPlayers.Count < 2;
            if (!_playersListUpdated && PhotonNetwork.isMasterClient)
            {
                MelonCoroutines.Start(UpdatePlayersList());
            }
            if (!_plagueStarted && !_waitingForPlayers)
            {
                //10 sec count down
                _countDown = 20f - ((float)PhotonNetwork.time - _startCdTime);
                if (!_countdownPlayed)
                {
                    _countdownPlayed = true;
                    PlayMusic(Helper.RandomSound(PlagueAssets.Instance._roundStartSounds));
                    MelonCoroutines.Start(PlaySoundDelayed(PlagueAssets.Instance._countdownSound, 9.8f));
                }
            }
            if (!_classSet)
            {
                // CuteLogger.Meow("_classSet");
                _classSet = true;
                RandomizePlagueClass();
            }
            if (_plagueStarted && !_waitingForPlayers)
            {
                if (!_refTimeSet)
                {
                    if (PhotonNetwork.isMasterClient)
                    {
                        _referenceTime = (float)PhotonNetwork.time;
                        CuteLogger.Meow("SetRoomProperty update");
                        Helper.SetRoomProperty(RefTime, new Il2CppSystem.Single() { m_value = _referenceTime }.BoxIl2CppObject());
                        _refTimeSet = true;
                    }
                    else
                    {
                        CuteLogger.Meow("_referenceTime update");
                        _referenceTime = (float)PhotonNetwork.time;
                        _refTimeSet = true;
                    }
                    if (!_roundStartPlayed)
                    {
                        _roundStartPlayed = true;
                        if (PhotonNetwork.isMasterClient)
                        {
                            CuteLogger.Meow("InfectRandomPlayer update");
                            InfectRandomPlayer();
                            PhotonNetwork.room.IsOpen = true;
                            PhotonNetwork.room.IsVisible = true;
                        }
                        //PlaySound(Helper.RandomSound(PlagueAssets.Instance._roundStartSounds));
                    }
                }
                _plagueTimer = 80 - ((float)PhotonNetwork.time - _referenceTime);
                if (_plagueTimer <= 0 && !_restarting)
                {
                    SurvivorsWin();
                }
            }
        }
        void FixedUpdate()
        {

        }
        public void RandomizePlagueClass()
        {
            SurvivorClassSetUp(UnityEngine.Random.Range(1, 5));
        }
        public void InfectRandomPlayer()
        {
            var target = _allPlayers[UnityEngine.Random.Range(0, _allPlayers.Count)];
            rmm.gameObject.GetComponent<PhotonView>().RPC("InfectPlayer", target, null);
            MelonCoroutines.Start(Govno());
        }

        [FunRPC]
        public void RestartRPC()
        {
            MelonCoroutines.Start(RestartRound());
        }

        public IEnumerator UpdatePlayersList()
        {
            //CuteLogger.Meow("Updated list");
            _playersListUpdated = true;
            _allPlayers.Clear();
            _teamAPlayers.Clear();
            _teamBPlayers.Clear();
            foreach (PhotonPlayer player in PhotonNetwork.playerList)
            {
                if (Helper.GetProperty(player, "joined") != null)
                {
                    if (Helper.GetProperty(player, "joined").ToString() == "true")
                    {
                        _allPlayers.Add(player);
                        if (Helper.GetProperty(player, "TeamName") != null)
                        {
                            if (Helper.GetProperty(player, "TeamName").ToString() == "Team A")
                            {
                                _teamAPlayers.Add(player);
                            }
                            if (Helper.GetProperty(player, "TeamName").ToString() == "Team B")
                            {
                                _teamBPlayers.Add(player);
                            }
                        }
                    }
                }
            }
            if (_teamAPlayers.Count <= 0 && _allPlayers.Count > 1 && !_restarting)
            {
                rmm.gameObject.GetComponent<PhotonView>().RPC("InfectedWin", PhotonTargets.All, null);
                //InfectedWin();
            }
            if (_teamBPlayers.Count <= 0 && _plagueStarted && !_restarting && _lpFlag)
            {
                rmm.gameObject.GetComponent<PhotonView>().RPC("SurvivorsWin", PhotonTargets.All, null);
                //SurvivorsWin();
            }
            yield return new WaitForSeconds(PlayerListUpdateTime);
            _playersListUpdated = false;
        }


        IEnumerator Govno()
        {
            yield return new WaitForSeconds(3f);
            _lpFlag = true;
        }
        public void PlayMusic(AudioClip clip, bool loop = false)
        {
            _musicSource.loop = loop;
            _musicSource.PlayOneShot(clip, 1f);
        }
        public void PlaySound(AudioClip clip, bool loop = false)
        {
            _soundsSource.loop = loop;
            _soundsSource.PlayOneShot(clip, 1f);
        }
        public IEnumerator PlaySoundDelayed(AudioClip clip, float delay, bool loop = false)
        {
            yield return new WaitForSeconds(delay);
            _soundsSource.loop = loop;
            _soundsSource.PlayOneShot(clip, 1f);
        }

        [FunRPC]
        public void InfectPlayer()
        {
            rmm.SpawnPlayer("Team B");
        }

        public void SpawnInfectedPlayer()
        {
            var team_2 = rmm.KGLOGDGOELM;
            int num = UnityEngine.Random.Range(0, team_2.spawnPoints.Length);
            int num2 = UnityEngine.Random.Range(0, 5);
            var infClass = InfectedClassSetUp(num2);
            rmm.CNLHJAICIBH = PhotonNetwork.NOOU(infClass.Prefab, team_2.spawnPoints[num].position + new Vector3(0f, 4f, 0f), team_2.spawnPoints[num].rotation, 0);
            var pm = rmm.CNLHJAICIBH.AddComponent<PlagueMonster>();
            pm._class = infClass;
            pm.SetUp();
            rmm.CNLHJAICIBH.name = PhotonNetwork.player.NickName;
            rmm.CNLHJAICIBH.GetComponent<PhotonView>().RPC("InfectedSoundRPC", PhotonTargets.All, null);
            _playerClass = infClass;
            //PlaySound(Helper.RandomSound(_infectedSounds));
        }
        [FunRPC]
        public void InfectedWin()
        {
            CuteLogger.Meow("InfectedWin");
            MelonCoroutines.Start(RestartRound());
            _infectedWin = true;
            PlayMusic(Helper.RandomSound(PlagueAssets.Instance._infectedWinSounds));
        }
        [FunRPC]
        public void SurvivorsWin()
        {
            CuteLogger.Meow("SurvivorsWin");
            MelonCoroutines.Start(RestartRound());
            _survivorsWin = true;
            PlayMusic(Helper.RandomSound(PlagueAssets.Instance._survivorsWinSounds));
        }
        IEnumerator RestartRound()
        {
            CuteLogger.Meow("RestartRound");
            _restarting = true;
            if (PhotonNetwork.isMasterClient) PhotonNetwork.room.IsOpen = false;
            yield return new WaitForSeconds(RestartTime);
            if (!_waitingForPlayers)
            {
                if (PhotonNetwork.isMasterClient)
                {
                    _startCdTime = (float)PhotonNetwork.time;
                    _referenceTime = (float)PhotonNetwork.time;
                    Helper.SetRoomProperty(CDTime, new Il2CppSystem.Single() { m_value = _startCdTime }.BoxIl2CppObject());
                    CuteLogger.Meow("SetRoomProperty restart");
                    Helper.SetRoomProperty(RefTime, new Il2CppSystem.Single() { m_value = _referenceTime }.BoxIl2CppObject());
                }
                else
                {
                    //_referenceTime = (float)PhotonNetwork.time;
                    _startCdTime = (float)PhotonNetwork.time;
                }
                _countDown = _startCdTime;
                _refTimeSet = false;
            }
            _classSet = false;
            _lpFlag = false;
            _winPlayed = false;
            _survivorsWin = false;
            _playerClass = null;
            _infectedWin = false;
            _roundStartPlayed = false;
            _countdownPlayed = false;
            _restarting = false;
            _totalRounds++;
            if (_totalRounds > 0 && PhotonNetwork.isMasterClient)
            {
                PhotonNetwork.room.IsOpen = true;
                PhotonNetwork.room.IsVisible = true;
            }
            rmm.SpawnPlayer("Team A");

        }
        [FunRPC]
        private void TellMasterFirstPlayerConnected()
        {
            if (_waitingForPlayers)
            {
                CuteLogger.Meow("hOST RECEIVED RPC");
                _waitingForPlayers = false;
                _referenceTime = (float)PhotonNetwork.time;
                Helper.SetRoomProperty(RefTime, _referenceTime.ToString());
                //smth here
            }
        }

        private void CheckPlayersCount()
        {
            if (PhotonNetwork.room.PlayerCount < 2)
            {
                _waitingForPlayers = true;
            }
            //yield return new WaitForSeconds(0.01f);
        }

        void OnGUI()
        {
            GUIStyle timerStyle = new GUIStyle(rmm.BIPMFIBNFBC.label)
            {
                fontSize = checked(75 * (Screen.width + Screen.height)) / 6000,
                alignment = TextAnchor.MiddleCenter
            };

            GUI.color = Color.white;
            if (_plagueStarted && _classSet && _playerClass != null)
            {
                GUI.Label(new Rect(Screen.width - 150f, Screen.height - 200f, 100f, 100f), "Class: " + _playerClass.ClassName, timerStyle);
            }
            if (_countDown > 0f && !_waitingForPlayers && !_restarting)
            {
                //The virus has been set loose...
                DrawOutline(new Rect(Screen.width / 2f, Screen.height * 0.010563f + 50f, 100f, 50f), "The virus is in the air...", 1, timerStyle);
                DrawOutline(new Rect(Screen.width / 2f, Screen.height * 0.010563f + 100f, 100f, 50f), string.Format("Round starts in {0} seconds", Mathf.CeilToInt(_countDown) % 60), 1, timerStyle);
            }
            else if (_plagueStarted && !_restarting)
                DrawOutline(new Rect(Screen.width / 2f, Screen.height * 0.010563f, 100f, 50f), string.Format("{0:00}:{1:00}", Mathf.CeilToInt(_plagueTimer) / 60, Mathf.CeilToInt(_plagueTimer) % 60), 1, timerStyle);
            if (_restarting && !_waitingForPlayers)
            {
                DrawOutline(new Rect(Screen.width / 2f, Screen.height * 0.010563f, 100f, 50f), "Restarting...", 1, timerStyle);
            }
            if (_survivorsWin)
            {
                DrawOutline(new Rect(Screen.width / 2f, Screen.height * 0.010563f + 50f, 100f, 50f), "Survivors defeated the plague!", 1, timerStyle);
            }
            if (_infectedWin)
            {
                DrawOutline(new Rect(Screen.width / 2f, Screen.height * 0.010563f + 50f, 100f, 50f), "Infected have taken over the world...", 1, timerStyle);
            }
            if (_waitingForPlayers)
            {
                DrawOutline(new Rect(Screen.width / 2f, Screen.height * 0.010563f, 100f, 50f), "No signs of trouble...", 1, timerStyle);
            }
            if (_debugMode)
            {
                DrawOutline(new Rect((float)Screen.width / 2f, (float)Screen.height * 0.138888f + 150f, 100f, 50f), string.Concat(new object[]
                {
                    _roundDuration
                }), 1, timerStyle);
            }

        }
        private void DrawOutline(Rect r, string t, int strength, GUIStyle style)
        {
            GUI.color = new Color(0f, 0f, 0f, 1f);
            checked
            {
                for (int i = 0 - strength; i <= strength; i++)
                {
                    unchecked
                    {
                        GUI.Label(new Rect(r.x - strength, r.y + i, r.width, r.height), t, style);
                        GUI.Label(new Rect(r.x + strength, r.y + i, r.width, r.height), t, style);
                    }
                }
                for (int j = 0 - strength + 1; j <= strength - 1; j++)
                {
                    unchecked
                    {
                        GUI.Label(new Rect(r.x + j, r.y - strength, r.width, r.height), t, style);
                        GUI.Label(new Rect(r.x + j, r.y + strength, r.width, r.height), t, style);
                    }
                }
                GUI.color = new Color(1f, 1f, 1f, 1f);
                GUI.Label(new Rect(r.x, r.y, r.width, r.height), t, style);
            }
        }
    }
}
