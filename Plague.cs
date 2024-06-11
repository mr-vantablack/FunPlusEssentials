using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using CodeStage.AntiCheat.Storage;
using FunPlusEssentials.CustomContent;
using FunPlusEssentials.Other;
using MelonLoader;
using UnhollowerBaseLib;
using UnityEngine;

namespace FunPlusEssentials
{
    public static class Plague
    {
        public static bool Enabled { get; set; }
    }

    [RegisterTypeInIl2Cpp, UsingRPC]
    public class PlagueController : MonoBehaviour
    {
        public PlagueController(IntPtr ptr) : base(ptr) { }

        public int roundDuration;
        public bool restarting, roundStarted, playersListUpdated, waitingForPlayers, roundEnded;
        public bool roundTimeSet, countdownTimeSet, timeIsUp, timeOver, randomPlayersInfected, allPlayersInfected, guiFirstInfected;
        public float currentTime, referenceTime, countdownTime, startTime;
        public string guiFirstInfectedName = "";
        public float test;
        public RoomMultiplayerMenu rmm;
        public List<PhotonPlayer> allPlayers, survivingPlayers;

        /*private void OnPhotonSerializeView(PhotonStream stream)
        {
            if (stream.isWriting)
            {
                stream.SendNext(new Il2CppSystem.Single { m_value = test }.BoxIl2CppObject());
            }
            else
            {
                test = stream.ReceiveNext().Unbox<float>();
            }
        }*/
        public void Start()
        {
            
        }
        public void Awake()
        {
            allPlayers = new List<PhotonPlayer>();
            survivingPlayers = new List<PhotonPlayer>();
            Helper.SetProperty("joined", "true");
            currentTime = 1;
            startTime = (float)PhotonNetwork.time;
            countdownTime = 0;
            timeOver = false;
            timeIsUp = false;
            rmm = gameObject.GetComponent<RoomMultiplayerMenu>();
            roundStarted = false;
            guiFirstInfected = false;
            CuteLogger.Meow("1");
            if (PhotonNetwork.room.customProperties["RD004'"] != null)
            {
                roundDuration = PhotonNetwork.room.customProperties["RD004'"].Unbox<int>();
                currentTime = roundDuration;
            }
            if (PhotonNetwork.isMasterClient)
            {
                MelonCoroutines.Start(UpdatePlayersList());
                waitingForPlayers = false;
                referenceTime = (float)PhotonNetwork.time;
                Helper.SetRoomProperty("RefTime", new Il2CppSystem.Single { m_value = referenceTime }.BoxIl2CppObject());
                CuteLogger.Meow("3");
            }
            else
            {
                if (PhotonNetwork.room.customProperties["CDTime"] != null)
                {
                    countdownTime = PhotonNetwork.room.customProperties["CDTime"].Unbox<float>();
                    countdownTimeSet = true;
                }
                if (PhotonNetwork.room.customProperties["RefTime"] != null)
                {
                    referenceTime = PhotonNetwork.room.customProperties["RefTime"].Unbox<float>();
                    roundTimeSet = true;
                }
            }
        }
        [FunRPC]
        public void RestartRPC()
        {
            CuteLogger.Meow("RestartRPC");
            MelonCoroutines.Start(RestartRound());
        }
        public IEnumerator RestartRound()
        {
            if (PhotonNetwork.player.isMasterClient)
            {
                //base.photonView.RPC("DestroyAllCrates", PhotonTargets.AllBuffered, new object[0]);
            }
            restarting = true;
           // Helper.SetProperty("TeamName", "Spectators");
            referenceTime = (float)PhotonNetwork.time;
            if (PhotonNetwork.isMasterClient)
            {
                PhotonNetwork.room.IsOpen = false;
            }
            yield return new WaitForSeconds(10f);
            if (PhotonNetwork.player.isMasterClient)
            {
                Helper.Room.GetComponent<PhotonView>().RPC("StartRoundRPC", PhotonTargets.AllBuffered, new Il2CppReferenceArray<Il2CppSystem.Object>(0));
            }
            yield break;
        }
        [FunRPC]
        public void StartRoundRPC()
        {
            CuteLogger.Meow("StartRoundRPC");
            if (PhotonNetwork.isMasterClient)
            {
                PhotonNetwork.room.IsOpen = true;
            }
            guiFirstInfected = false;
            guiFirstInfectedName = "";
            restarting = false;
            roundStarted = false;
            //_supplyBroadcastTime = 15f;
           // _midOfRound = false;
            //_isSupplyCrateDropped = false;
            //_gameClassSet = false;
            //TeamAName = _rmm.team_1.teamName;
            //TeamBName = _rmm.team_2.teamName;
            countdownTimeSet = false;
            roundTimeSet = false;
            timeIsUp = false;
            allPlayersInfected = false;
            //_gameModeName = "";
            //_gameModeSet = false;      
            Helper.SetProperty("TeamName", "Team A");         
            roundDuration = Convert.ToInt32(PhotonNetwork.room.customProperties["RD004'"].Unbox<int>());
            //SpawnPlayer(TeamAName);
            roundEnded = false;
            timeOver = false;
        }
        public IEnumerator UpdatePlayersList()
        {
            playersListUpdated = true;
            foreach (PhotonPlayer player in PhotonNetwork.playerList)
            {
                allPlayers.Clear();
                survivingPlayers.Clear();
                if (Helper.GetProperty(player, "joined") != null)
                {
                    if (Helper.GetProperty(player, "joined").ToString() == "true")
                    {
                        allPlayers.Add(player);
                    }
                    
                }
                if (Helper.GetProperty(player, "TeamName") != null)
                {
                    if (Helper.GetProperty(player, "TeamName").ToString() == "Team A")
                    {
                        survivingPlayers.Add(player);
                    }
                }
            }
            yield return new WaitForSeconds(2f);
            playersListUpdated = false;
        }
        public void Update()
        {
            Cursor.visible = rmm.IOMIAHNBDOG;
            if (!playersListUpdated && PhotonNetwork.isMasterClient)
            {
                MelonCoroutines.Start(UpdatePlayersList());
            }
            countdownTime = 10f - ((float)PhotonNetwork.time - startTime);
            roundStarted = (!waitingForPlayers && countdownTime <= 0f);
            if (PhotonNetwork.isMasterClient && !waitingForPlayers)
            {
                if (roundEnded)
                {
                    Helper.Room.GetComponent<PhotonView>().RPC("RestartRPC", PhotonTargets.All, null);
                }
                if ((survivingPlayers.Count == 0 && !allPlayersInfected) || (timeIsUp && !timeOver))
                {
                    if (survivingPlayers.Count == 0 && !allPlayersInfected && !restarting)
                    {
                       // Helper.Room.GetComponent<PhotonView>().RPC("PlayInfectedWinSound", PhotonTargets.All, null);
                    }
                    if (timeIsUp && !timeOver)
                    {
                        //base.photonView.RPC("PlaySurvivorsWinSound", PhotonTargets.All, new object[0]);
                    }
                    allPlayersInfected = true;
                    timeOver = true;
                    roundEnded = true;
                }
                else
                {
                    roundEnded = false;
                }
            }
        }
        public void FixedUpdate()
        {
            if (currentTime > 0)
            {
                currentTime -= Time.deltaTime;
            }

            if (!waitingForPlayers)
            {
                if (!countdownTimeSet)
                {
                    if (PhotonNetwork.player.isMasterClient)
                    {
                        startTime = (float)PhotonNetwork.time;
                        Helper.SetRoomProperty("CDTime", new Il2CppSystem.Single { m_value = startTime }.BoxIl2CppObject());
                        countdownTimeSet = true;
                        //_randomPlayersInfected = false;
                        countdownTime = 10f;
                    }
                    else
                    {
                        startTime = (float)PhotonNetwork.time;
                        countdownTimeSet = true;
                        countdownTime = 10f;
                    }
                }
                if (!roundTimeSet && countdownTime <= 0f)
                {
                    if (PhotonNetwork.isMasterClient)
                    {
                        referenceTime = (float)PhotonNetwork.time;
                        Helper.SetRoomProperty("RefTime", new Il2CppSystem.Single { m_value = referenceTime }.BoxIl2CppObject());
                        roundTimeSet = true;
                    }
                    else
                    {
                        referenceTime = (float)PhotonNetwork.time;
                        roundTimeSet = true;
                    }
                }
            }
            if (PhotonNetwork.isMasterClient && waitingForPlayers)
            {
                referenceTime = (float)PhotonNetwork.time;
                Helper.SetRoomProperty("RefTime", new Il2CppSystem.Single { m_value = referenceTime }.BoxIl2CppObject());
                roundTimeSet = true;
                startTime = (float)PhotonNetwork.time;
                randomPlayersInfected = false;
                Helper.SetRoomProperty("CDTime", new Il2CppSystem.Single { m_value = startTime }.BoxIl2CppObject());
                countdownTimeSet = true;
            }
            if (!waitingForPlayers && countdownTime <= 0f)
            {
                float num = (float)PhotonNetwork.time - referenceTime;
                float num2 = roundDuration - num;
                if (num2 >= 0f)
                {
                    currentTime = num2;
                    timeIsUp = false;
                }
                else
                {
                    timeIsUp = true;
                }
                if (num2 < 0f && waitingForPlayers)
                {
                    currentTime = roundDuration;
                }
                if (PhotonNetwork.isMasterClient && !randomPlayersInfected)
                {
                    randomPlayersInfected = true;
                    //InfectRandomPlayer();
                }
            }
        }
        public void OnGUI()
        {
            if (!restarting)
            {
                GUI.color = Color.white;
                GUIStyle style = new GUIStyle(rmm.BIPMFIBNFBC.label)
                {
                    fontSize = checked(75 * (Screen.width + Screen.height)) / 6000,
                    alignment = TextAnchor.MiddleCenter
                };
                float num = (Mathf.CeilToInt(currentTime) / 60);
                float num2 = (Mathf.CeilToInt(currentTime) % 60);
                DrawOutline(new Rect(Screen.width / 2f, Screen.height * 0.010563f, 100f, 50f), string.Format("{0:00}:{1:00}", num, num2), 1, style);
            }
            else
            {
                GUI.color = Color.white;
                GUIStyle style2 = new GUIStyle(rmm.BIPMFIBNFBC.label)
                {
                    fontSize = checked(75 * (Screen.width + Screen.height)) / 6000,
                    alignment = TextAnchor.MiddleCenter
                };
                DrawOutline(new Rect(Screen.width / 2f, Screen.height * 0.010563f, 100f, 50f), "restarting...", 1, style2);
            }
            if (!roundStarted && !waitingForPlayers)
            {
                GUI.color = Color.white;
                GUIStyle style3 = new GUIStyle(rmm.BIPMFIBNFBC.label)
                {
                    fontSize = checked(75 * (Screen.width + Screen.height)) / 6000,
                    alignment = TextAnchor.MiddleCenter
                };
                DrawOutline(new Rect(Screen.width / 2f, Screen.height * 0.138888f, 100f, 50f), string.Format(string.Concat(new object[]
                {
            "{0} seconds left before round starts."
                }), Mathf.CeilToInt(countdownTime)), 1, style3);
                DrawOutline(new Rect(Screen.width / 2f, Screen.height * 0.010563f, 100f, 50f), string.Format(string.Concat(new object[]
                {
            "the virus is in the air..."
                }), Mathf.CeilToInt(countdownTime)), 1, style3);
            }
            if (guiFirstInfected && guiFirstInfectedName != "")
            {
                GUI.color = Color.white;
                GUIStyle style4 = new GUIStyle(rmm.BIPMFIBNFBC.label)
                {
                    fontSize = checked(75 * (Screen.width + Screen.height)) / 6000,
                    alignment = TextAnchor.MiddleCenter
                };
                DrawOutline(new Rect(Screen.width / 2f, Screen.height * 0.138888f, 100f, 50f), guiFirstInfectedName, 1, style4);
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
