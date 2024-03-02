using System;
using System.Collections;
using UnityEngine;
using MelonLoader;
using Convert = System.Convert;
using Hashtable = ExitGames.Client.Photon.Hashtable;
using static MelonLoader.MelonLogger;
using UnityEngine.Networking;
using FunPlusEssentials.Essentials;
using FunPlusEssentials.Other;
using FunPlusEssentials.Patches;
using UnhollowerBaseLib;

namespace FunPlusEssentials.Fun
{
    [RegisterTypeInIl2Cpp] //регистрация MonoBehaviour компонента в il2cpp системе
    public class MusicPlayer : MonoBehaviour
    {
        public MusicPlayer(IntPtr ptr) : base(ptr) { }

        private AudioSource source;
        private AudioClip currentClip;
        private readonly long maxFileSize = 26214400; //25 МБ
        private bool synced;
        private long fileSize;
        public string[] djList;
        public static MusicPlayer Instance { get; private set; }

        private void LoadDjList(string path) // белый список ДОДЕЛАТЬ upd впадлу бля 
        {
            using (System.IO.StreamReader reader = new System.IO.StreamReader(path))
            {

                string keys = reader.ReadToEnd();
                string[] data = keys.Split('\n');
                djList = new string[data.Length];

                for (int i = 0; i < data.Length; i++)
                {
                    djList[i] = data[i];
                    Debug.Log(i.ToString());
                }
            }
        }
        private IEnumerator PlayAudio(string fileName, float volume, bool loop, float time)
        {
            if (!Config.noFileSizeLimit)
            {

                fileSize = 0;
                yield return MelonCoroutines.Start(GetFileSize(fileName, (size) => fileSize = size));
                Debug.Log("File Size: " + fileSize);
                if (fileSize <= 0)
                {
                    CommandHandler.SystemMsg($"Error getting sound file.");
                    yield break;
                }
                if (fileSize > maxFileSize)
                {
                    CommandHandler.SystemMsg($"Error! The file is too big. The maximum allowable size is 15 mb.");
                    yield break;
                }
            }
            WWW request = new WWW(fileName);
            yield return new WaitForSeconds(5f);
            if (!request.isDone)
            {
                yield return request;
            }
            currentClip = request.GetAudioClip();
            currentClip.name = fileName;
            source.clip = currentClip;
            source.loop = loop;
            source.time = GetPosition(); // БАГ !!!!!!!!!!!!! мб пофиксил, проверить надо
            source.volume = volume;
            source.Play();
        }


        private WWW GetAudio(string fileName)
        {
            return new WWW(fileName);
        }
        private IEnumerator GetFileSize(string url, Action<long> result)
        {
            UnityWebRequest uwr = UnityWebRequest.Head(url);
            yield return uwr.SendWebRequest();
            string size = uwr.GetResponseHeader("Content-Length");

            if (uwr.isNetworkError || uwr.isHttpError)
            {
                Debug.Log("Error While Getting Length: " + uwr.error);
                result?.Invoke(-1);
            }
            else
            {
                result?.Invoke(Convert.ToInt64(size));
            }
        }
        private void OnJoinedRoom()
        {
            if (PhotonNetwork.offlineMode) return;
            if (!PhotonNetwork.isMasterClient)
            {
                SyncTrack();
            }
        }
        private void OnLeftRoom()
        {
            ClearProperties();
        }

        public void Play(string link, float volume = 1f, bool loop = false, float time = 0f)
        {
            Stop();
            if (PhotonNetwork.isMasterClient)
            {
                SetProperty("Track", link);
                SetProperty("Volume", volume.ToString());
                SetProperty("Loop", loop.ToString());
                SetProperty("Position", time.ToString());
            }
            CuteLogger.Meow(link);
            MelonCoroutines.Start(PlayAudio(link, volume, loop, time));
        }

        public void Stop()
        {
            source.Stop();
            if (PhotonNetwork.isMasterClient)
            {
                ClearProperties();
            }
        }
        private float GetPosition()
        {
            if (PhotonNetwork.masterClient.customProperties["Position"] != null)
                return float.Parse(PhotonNetwork.masterClient.customProperties["Position"].ToString() ?? "0.0", System.Globalization.CultureInfo.InvariantCulture);
            return 0.0f;
        }
        private IEnumerator SyncPosition() // синхронизация времени трека каждую 1 сек, нужно для тех кто подключается к комнате с играющим треком
        {
            synced = true;
            SetProperty("Position", source.time.ToString());
            yield return new WaitForSeconds(1f);
            synced = false;
        }

        private void SyncTrack() // синхронизация трека после захода в комнату
        {
            if (PhotonNetwork.masterClient.customProperties["Track"] != null)
            {
                Play(PhotonNetwork.masterClient.customProperties["Track"].ToString(),
                    float.Parse(PhotonNetwork.masterClient.customProperties["Volume"].ToString(), System.Globalization.CultureInfo.InvariantCulture),
                    Convert.ToBoolean(PhotonNetwork.masterClient.customProperties["Loop"]));
            }
        }

        #region Unity Callbacks

        private void OnEnable()
        {
            Config.SetUpConfig();
            Helper.SetProperty("nicknameColor", Config.nicknameColor);
            Instance = this;
            source = Helper.Room.GetComponent<AudioSource>();
            OnJoinedRoom();
        }

        private void OnDisable()
        {
            OnLeftRoom();
        }

        private void Update()
        {
            if (PhotonNetwork.isMasterClient && !synced)
            {
                MelonCoroutines.Start(SyncPosition());
            }
        }

        #endregion

        #region Helpers
        public static void SetProperty(string property, string parameter)
        {
            Hashtable hashtable = new Hashtable
            {
                [property] = parameter
            };
            PhotonNetwork.player.SetCustomProperties(hashtable);
        }
        private void ClearProperties()
        {
            if (PhotonNetwork.player.customProperties["Track"] != null)
            {
                SetProperty("Track", null);
                SetProperty("Volume", null);
                SetProperty("Loop", null);
                SetProperty("Position", null);
            }
        }
        #endregion
    }
}
