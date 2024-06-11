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
using MelonLoader;
using UnhollowerBaseLib;
using UnityEngine;
using UnityEngine.Audio;
using static Il2CppSystem.Guid;
using static MelonLoader.MelonLogger;
using static PunTeams;
using static ShopSystem;

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
        }
        [FunRPC]
        public void InfectedSoundRPC()
        {
            CuteLogger.Meow("InfectedSoundRPC");
            _soundsSource.PlayOneShot(Helper.RandomSound(PlagueController.Instance._infectedSounds));
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

        public List<AudioClip> _infectedWinSounds, _survivorsWinSounds, _infectedSounds, _roundStartSounds;
        public AudioClip _ambience, _countdownSound;
        private Material _skybox;
        public AudioSource _musicSource, _soundsSource, _otherSource;
        private bool _countdownPlayed, _winPlayed, _roundStartPlayed;

        private bool _plagueStarted, _restarting, _survivorsWin, _infectedWin;
        private bool _waitingForPlayers, _playersListUpdated, _lpFlag;
        private bool _refTimeSet;
        private bool _debugMode;
        private float _referenceTime, _startCdTime;
        private float _plagueTimer, _countDown;
        private float _roundDuration;
        private int _totalRounds;
        private PlagueMode _mode;
        private List<PhotonPlayer> _allPlayers, _teamAPlayers, _teamBPlayers;

        
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
            _mode = PlagueMode.Infection;
            Instance = this;
            _totalRounds = 0;
            SetUpAudio();
            MelonCoroutines.Start(LoadAssets());
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
                _musicSource.PlayOneShot(_ambience);
                _startCdTime = (float)PhotonNetwork.time;
                _countDown = _startCdTime;
                Helper.SetRoomProperty(CDTime, new Il2CppSystem.Single() { m_value = _startCdTime }.BoxIl2CppObject());
            }
            else
            {
                _referenceTime = Helper.GetRoomProperty(RefTime).Unbox<float>();
                _startCdTime = Helper.GetRoomProperty(CDTime).Unbox<float>();
                _countDown = _startCdTime;
                _refTimeSet = 10f - ((float)PhotonNetwork.time - _startCdTime) <= 0f;
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
                _countDown = 10f - ((float)PhotonNetwork.time - _startCdTime);
                if (!_countdownPlayed)
                {
                    _countdownPlayed=true;
                    PlaySound(_countdownSound);
                }
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
                    if (!_roundStartPlayed && PhotonNetwork.isMasterClient)
                    {
                        InfectRandomPlayer();
                        _roundStartPlayed = true;
                        PhotonNetwork.room.IsOpen = true;
                        PhotonNetwork.room.IsVisible = true;
                        PlaySound(Helper.RandomSound(_roundStartSounds));
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
            if (Input.GetKeyDown(KeyCode.Keypad7))
            {
                rmm.SpawnPlayer("Team B");
            }
            if (Input.GetKeyDown(KeyCode.Keypad8))
            {
                rmm.SpawnPlayer("Team A");
            }
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
                _infectedWinSounds.Add(assetBundleCreateRequest.Load<AudioClip>($"infectedWin{i}"));
                _survivorsWinSounds.Add(assetBundleCreateRequest.Load<AudioClip>($"survivorsWin{i}"));
                _infectedSounds.Add(assetBundleCreateRequest.Load<AudioClip>($"infected{i}"));
            }
            for (int i = 1; i < 3; i++)
            {
                _roundStartSounds.Add(assetBundleCreateRequest.Load<AudioClip>($"prepare{i}"));
            }
            _ambience = assetBundleCreateRequest.Load<AudioClip>($"ambience1");
            _countdownSound = assetBundleCreateRequest.Load<AudioClip>($"countdown1");
            _skybox = assetBundleCreateRequest.Load<Material>($"skybox");
            assetBundleCreateRequest.Unload(false);
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

        [FunRPC]
        public void InfectPlayer()
        {
            rmm.SpawnPlayer("Team B");
        }

        public void SpawnInfectedPlayer()
        {
            var team_2 = rmm.KGLOGDGOELM;
            int num = UnityEngine.Random.Range(0, team_2.spawnPoints.Length);
            rmm.CNLHJAICIBH = PhotonNetwork.NOOU("INF/PlayerNewborn", team_2.spawnPoints[num].position + Vector3.up, team_2.spawnPoints[num].rotation, 0);
            rmm.CNLHJAICIBH.name = PhotonNetwork.player.NickName;
            rmm.CNLHJAICIBH.GetComponent<PhotonView>().RPC("InfectedSoundRPC", PhotonTargets.All, null);
            //PlaySound(Helper.RandomSound(_infectedSounds));
        }
        [FunRPC]
        public void InfectedWin()
        {
            CuteLogger.Meow("InfectedWin");
            MelonCoroutines.Start(RestartRound());
            _infectedWin = true;
            PlayMusic(Helper.RandomSound(_infectedWinSounds));
        }
        [FunRPC]
        public void SurvivorsWin()
        {
            CuteLogger.Meow("SurvivorsWin");
            MelonCoroutines.Start(RestartRound());
            _survivorsWin = true;
            PlayMusic(Helper.RandomSound(_survivorsWinSounds));
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
            _lpFlag = false;
            _winPlayed = false;
            _survivorsWin = false;
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

            if (_countDown > 0f && !_waitingForPlayers && !_restarting)
            {
                //The virus has been set loose...
                DrawOutline(new Rect(Screen.width / 2f, Screen.height * 0.010563f + 50f, 100f, 50f), "The virus is in the air...", 1, timerStyle);
                DrawOutline(new Rect(Screen.width / 2f, Screen.height * 0.010563f + 100f, 100f, 50f), string.Format("Round starts in {0} seconds", Mathf.CeilToInt(_countDown) % 60), 1, timerStyle);
            }
            else if(_plagueStarted && !_restarting)
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
                this.DrawOutline(new Rect((float)Screen.width / 2f, (float)Screen.height * 0.138888f + 150f, 100f, 50f), string.Concat(new object[]
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
