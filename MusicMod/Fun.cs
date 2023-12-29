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

namespace FunPlusEssentials.Fun
{
    [RegisterTypeInIl2Cpp] //регистрация MonoBehaviour компонента в il2cpp системе
    public class MusicPlayer : MonoBehaviour
    {
        public MusicPlayer(IntPtr ptr) : base(ptr) { }

        private AudioSource source;
        private AudioClip currentClip;
        private readonly long maxFileSize = 15728640; //15 МБ
        private bool synced;
        private long fileSize;
        public string[] djList;
        public static MusicPlayer Instance { get; private set; }

        private void LoadDjList(string path) // белый список ДОДЕЛАТЬ
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

    public static class ResearchLabV2
    {
        public static GameObject doorPrefab;
        public static GameObject parentObj;
        public static GameObject elevator;
        public static KeyCode interactionKey = KeyCode.E;

        public static void SpawnObjects()
        {
            parentObj = new GameObject();
            parentObj.transform.position = new Vector3(71.417f, 25.98f, -23.19f);
            parentObj.transform.localScale = new Vector3(1.25f, 1.25f, 1.25f);

            BundleManager.LoadAssetBundle("controlroom", "ControlRoom", Application.streamingAssetsPath);
            doorPrefab = BundleManager.LoadAssetBundle("autodoor", "Movable", Application.streamingAssetsPath);

            elevator = GameObject.Find("AllData (1)/Map/Map_part2/Elevator");
            elevator.AddComponent<Elevator>();

            CreateDoor(new Vector3(-52.6f, -16.8f, -36.7f), 90f);
            CreateDoor(new Vector3(-55.1f, -20.5f, -9.3f));
            CreateDoor(new Vector3(-35.4f, -20.5248f, -4.496f));
            CreateDoor(new Vector3(-65.343f, -20.5248f, -4.45f));
            CreateDoor(new Vector3(-25.5112f, -20.5248f, 5.032802f));
            CreateDoor(new Vector3(-64.077f, -20.58f, 3.012f), 90f);
            CreateDoor(new Vector3(-40.322f, -20.5248f, 20.042f));
            CreateDoor(new Vector3(-69.925f, -20.58f, 17.854f), 90f);
            CreateDoor(new Vector3(-67.98f, -20.58f, 22.888f), 90f);
            CreateDoor(new Vector3(-35.24f, -20.5248f, 35.624f));
            CreateDoor(new Vector3(-59.898f, -20.5248f, 35.624f));
            CreateDoor(new Vector3(-35.24f, -20.5248f, 44.326f));
            CreateDoor(new Vector3(-27.812f, -20.545f, 45.551f));
            CreateDoor(new Vector3(-12.544f, -0.554f, 7.791f), 90f);
            CreateDoor(new Vector3(-12.544f, -0.554f, 2.211f), 90f);
            CreateDoor(new Vector3(-0.164f, -20.5248f, 5.032802f));
            CreateDoor(new Vector3(-0.1640015f, -0.541f, 5.032802f));


            #region first floor buttons
            CreateElevatorButton(1, new Vector3(71.15f, 0.4f, -18.5f));
            CreateElevatorButton(2, new Vector3(71.15f, 0.8f, -18.5f));
            #endregion
            #region elevator buttons
            CreateElevatorButton(1, new Vector3(73f, 0.4f, -18.7f), elevator.transform);
            CreateElevatorButton(2, new Vector3(73f, 0.8f, -18.7f), elevator.transform);
            #endregion
        }

        public static void CreateElevatorButton(int floor, Vector3 position, Transform parent = null)
        {
            GameObject button = GameObject.CreatePrimitive(PrimitiveType.Cube);
            button.transform.position = position;
            button.transform.localScale = new Vector3(0.3f, 0.3f, 0.3f);
            button.transform.parent = parent;
            button.AddComponent<ElevatorButton>().floor = floor;
        }
        
        public static void CreateDoor(Vector3 position, float rotation = 0f)
        {
            var go = GameObject.Instantiate(doorPrefab);
            go.transform.parent = parentObj.transform;
            var c = go.AddComponent<AutoDoor>();
            c.position = position;
            c.openPosition = new Vector3(c.position.x, c.position.y + 2.7f, c.position.z);
            go.transform.localPosition = position;
            go.transform.Rotate(0.0f, rotation, 0.0f, Space.Self); ;
        }

    }

    [RegisterTypeInIl2Cpp]
    public class AutoDoor : MonoBehaviour
    {
        public AutoDoor(IntPtr ptr) : base(ptr) { }
        public Vector3 position;
        public Vector3 openPosition;
        public float speed = 3f;
        public bool open;

        void Start()
        {
            gameObject.AddComponent<MeshCollider>();
            var bbox = GameObject.CreatePrimitive(PrimitiveType.Cube);
            bbox.transform.parent = ResearchLabV2.parentObj.transform;
            bbox.transform.localPosition = position;
            bbox.AddComponent<AutoDoorBbox>().door = this;

            open = Convert.ToBoolean(UnityEngine.Random.RandomRange(0, 2));
        }
        void Update()
        {
            if (open)
            {
                if (transform.localPosition != openPosition)
                {
                    transform.localPosition = Vector3.MoveTowards(transform.localPosition, openPosition, speed * Time.deltaTime);
                }
            }
            else
            {
                if (transform.localPosition != position)
                {
                    transform.localPosition = Vector3.MoveTowards(transform.localPosition, position, speed * Time.deltaTime);
                }
            }
        }
    }

    [RegisterTypeInIl2Cpp]
    public class AutoDoorBbox : MonoBehaviour
    {
        public AutoDoorBbox(IntPtr ptr) : base(ptr) { }
        public AutoDoor door;
        private bool inTrigger;
        void Start()
        {
            Destroy(GetComponent<MeshFilter>());
            Destroy(GetComponent<MeshRenderer>());
            transform.localScale = new Vector3(2f, 2f, 2f);
            var c = gameObject.GetComponent<BoxCollider>();
            c.size = new Vector3(2.28f, 2.53f, 1.18f);
            c.center = new Vector3(-0.076f, -1.60f, 0);
            c.isTrigger = true;
        }
        void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.tag == "Player")
            {
                inTrigger = true;
            }
        }
        void OnTriggerExit(Collider other)
        {
            if (other.gameObject.tag == "Player")
            {
                inTrigger = false;
            }
        }
        void Update()
        {
            if (Input.GetKeyDown(ResearchLabV2.interactionKey) && inTrigger)
            {
                door.open = !door.open;
            }
        }
    }

        [RegisterTypeInIl2Cpp]
    public class ElevatorButton : MonoBehaviour
    {
        public ElevatorButton(IntPtr ptr) : base(ptr) { }
        
        public int floor = 1;
        void Start()
        {
            name = "ElevatorButton";
        }
        public void Interaction()
        {
            Msg("pressed");
            Helper.SendChatMessage("debug", $"pressed {floor.ToString()}");
            //ResearchLabV2.Elevator.floor = floor;
        }
    }

    [RegisterTypeInIl2Cpp]
    public class Elevator : MonoBehaviour
    {
        public Elevator(IntPtr ptr) : base(ptr) { }
        private float speed = 2f;
        public int floor = 1;
        
        private Vector3[] points =
        {
            new Vector3(),
            new Vector3(60f, 1.5f, -30.4f),
            new Vector3(60f, 21.3f, -30.4f)
        };
        public bool checkDoorsOpen, elevatorMoving;
        public GameObject doorPrefab, roomPrefab;
        private GameObject player => Helper.Player;
        private Transform position => Helper.WeaponManager.transform;
        void Start()
        {
            player.AddComponent<Rigidbody>().isKinematic = true;
            gameObject.AddComponent<BoxCollider>().isTrigger = true;
        }

        void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.tag == "Player")
            {
                player.transform.parent = transform;
            }
        }
        void OnTriggerExit(Collider other)
        {
            if (other.gameObject.tag == "Player")
            {
                player.transform.parent = null;
            }
        }
        void Update()
        {
            if (transform.localPosition != points[floor])
            {
                elevatorMoving = true;
                transform.localPosition = Vector3.MoveTowards(transform.localPosition, points[floor], speed * Time.deltaTime);
            }
            else
            {
                elevatorMoving = false;
            }
            //if (!checkDoorsOpen) { MelonCoroutines.Start(CheckForOpenDoors()); }
            if (Input.GetKeyDown(ResearchLabV2.interactionKey))
            {
                Msg("try");
                RaycastHit hit;
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(ray, out hit, 2))
                {
                    if (hit.collider.name == "ElevatorButton")
                    {
                        hit.collider.GetComponent<ElevatorButton>().Interaction();
                    }
                }
            }
        }

        /*private IEnumerator CheckForOpenDoors()
        {
            checkDoorsOpen = true;
            yield return new WaitForSeconds(1f);
            if (ResearchLabV2.elevatorEnterDoors[0] != null)
            {
                if (elevatorMoving)
                {
                    ResearchLabV2.elevatorEnterDoors[floor - 1].GetComponent<AutoDoor>().open = false;
                    ResearchLabV2.elevatorDoor.GetComponent<AutoDoor>().open = false;
                }
                else
                {
                    ResearchLabV2.elevatorEnterDoors[floor - 1].GetComponent<AutoDoor>().open = true;
                    ResearchLabV2.elevatorDoor.GetComponent<AutoDoor>().open = true;
                }
            }
            checkDoorsOpen = false;
        }*/
    }
}
