using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Il2CppCodeStage.AntiCheat.Storage;
using FunPlusEssentials.CustomContent;
using FunPlusEssentials.Essentials;
using FunPlusEssentials.Other;
using FunPlusEssentials.Patches;
using HarmonyLib;
using Il2CppSystem;
using Il2CppSystem.Reflection;
using MelonLoader;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Audio;
using UnityEngine.UI;
using UnityStandardAssets.ImageEffects;
using static Il2Cpp.CharacterCustomization;
using static Il2CppSystem.Guid;
using static MelonLoader.MelonLogger;
using static Il2Cpp.PunTeams;
using static Il2Cpp.ShopSystem;
using static UnityEngine.UI.Navigation;
using BindingFlags = System.Reflection.BindingFlags;
using IntPtr = System.IntPtr;
using MethodInfo = Il2CppSystem.Reflection.MethodInfo;
using Random = UnityEngine.Random;
using Il2Cpp;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using UnityEngine.Rendering;


namespace FunPlusEssentials
{
    public static class Plague
    {
        public static bool CanBeEnabled { get; set; }
        public static bool Enabled { get; set; }

    }
    [RegisterTypeInIl2Cpp, HandleRPC]
    public class SupplyCrate : MonoBehaviour
    {
        public SupplyCrate(IntPtr ptr) : base(ptr) { }
        public PhotonView pv;
        public Animation anim;
        public Transform c;
        public bool opened, canOpen;
        public List<List<string>> weapons = new List<List<string>>
        {
            new List < string > { "AKM", "Katana" },
            new List < string > { "MK16", "44 Combat" },
            new List < string > { "M249-Saw", "Katana" },
            new List < string > { "Skull-1" },
            new List < string > { "Skull-5" },
            new List < string > { "SF Gun" },
            new List < string > { "Turbulent-7" },
            new List < string > { "Balrog-1", "Katana" },
            new List < string > { "Balrog-1" }
        };
        public void Awake()
        {
            opened = false;
            gameObject.AddComponent<Light>().color = Color.red;
            pv = GetComponent<PhotonView>();
            c = transform.GetChild(1);
            anim = c.GetComponent<Animation>();
        }
        public void Update()
        {
            canOpen = !opened && !PlagueController.Instance._playerClass.Infected && Vector3.Distance(Helper.Player.transform.position, c.position) <= 1.5f;
            if (canOpen && Input.GetKeyDown(KeyCode.E))
            {
                opened = true;
                PlagueController.Instance.PlaySound(PlagueAssets.Instance._supplyPickupSound);
                pv.RPC("OpenRPC", PhotonTargets.All, new Il2CppReferenceArray<Il2CppSystem.Object>(0));
                Heal(100f);
                GiveWeapons();
            }
        }
        public void OnGUI()
        {
            if (canOpen)
            {
                GUI.skin = PlagueController.Instance.rmm.BIPMFIBNFBC;
                int fontSize = checked(50 * (Screen.width + Screen.height)) / 6000;
                GUI.color = Color.white;
                GUIStyle style = new GUIStyle(GUI.skin.label)
                {
                    fontSize = fontSize
                };
                GUI.Label(new Rect((float)Screen.width * 0.460937f, (float)Screen.height * 0.5f, 300f, 200f), "Press E to open", style);
            }
        }
        [FunRPC]
        public void OpenRPC()
        {
            opened = true;
            if (pv.isMine) MelonCoroutines.Start(DestroyAfter(10f));
            anim.Play();
        }
        public void Heal(float hp)
        {
            var maxHp = Helper.PlayerDamage.NILGDNBIFDC;
            var cHp = Helper.PlayerDamage.PIIDNIGPCDK;
            var value = maxHp - cHp;
            if (value < hp)
            {
                Helper.PlayerDamage.PIIDNIGPCDK += value;
            }
            else
            {
                Helper.PlayerDamage.PIIDNIGPCDK += hp;
            }
        }
        public void GiveWeapons()
        {
            var r = Random.Range(0, weapons.Count - 1);
            foreach (var weapon in weapons[r])
            {
                Helper.GiveWeapon(Helper.WeaponManager, weapon);
            }
            if (weapons[r].Count == 1)
            {
                PlagueController.Instance.Message($"{PlagueLocales.pickupSupplyText[0]} {weapons[r][0].ToUpper()} {PlagueLocales.pickupSupplyText[1]}", PhotonNetwork.player.name.Split('|')[0]);
            }
            else
            {
                PlagueController.Instance.Message(PlagueLocales.openSupplyText, PhotonNetwork.player.name.Split('|')[0]);
            }
        }
        public IEnumerator DestroyAfter(float seconds)
        {
            yield return new WaitForSeconds(seconds);
            PhotonNetwork.Destroy(gameObject);
        }
    }
    [RegisterTypeInIl2Cpp, HandleRPC]
    public class WireTrapRPCHandler : MonoBehaviour
    {
        public WireTrapRPCHandler(IntPtr ptr) : base(ptr) { }
        public WireTrap wire;
        public PhotonView view;
        public void Awake()
        {
            view = GetComponent<PhotonView>();
            view.synchronization = ViewSynchronization.UnreliableOnChange;
            PhotonManager.RegisterSerializeView(gameObject.GetComponent<PhotonView>(), this);
        }
        [FunRPC]
        public void Break()
        {
            wire.durability--;
        }
        public void OnPhotonSerializeView(Il2CppSystem.Object stream, Il2CppSystem.Object info)
        {
            if (wire == null) return;
            var s = stream.Cast<PhotonStream>();
            var i = info.Cast<PhotonMessageInfo>();
            if (s.isWriting)
            {
                s.SendNext(new Il2CppSystem.Int32() { m_value = wire.durability }.BoxIl2CppObject());
            }
            else
            {
                wire.durability = s.ReceiveNext().Unbox<int>();
            }
        }
    }

    [RegisterTypeInIl2Cpp]
    public class WireTrap : MonoBehaviour
    {
        public WireTrap(IntPtr ptr) : base(ptr) { }
        public bool isDamage = true;
        public PhotonPlayer owner;
        public float survivorsDamage = 1f;
        public float infectedDamage = 5f;
        public float coolDown = 0.1f;
        private float timer = 0f;
        public int durability;
        public PhotonView view;

        public void Start()
        {
            durability = 100;
            transform.parent.gameObject.AddComponent<WireTrapRPCHandler>().wire = this;
            view = transform.parent.gameObject.GetComponent<PhotonView>();
        }
        public void Update()
        {
            if (durability <= 0 && owner.ID == PhotonNetwork.player.ID)
            {
                PhotonNetwork.Destroy(gameObject.transform.parent.gameObject);
            }
        }
        public void OnTriggerStay(Collider coll)
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
                        view.RPC("Break", owner, new Il2CppReferenceArray<Il2CppSystem.Object>(0));                     
                        Helper.PlayerDamage.E10030(survivorsDamage);
                    }
                }
                if (coll.gameObject.tag == "monster" && coll.gameObject.name == PhotonNetwork.player.name)
                {
                    if (timer > 0)
                    {
                        timer -= Time.deltaTime;
                    }

                    if (timer <= 0)
                    {
                        if (owner == null) owner = PhotonNetwork.player;
                        timer = coolDown;
                        view.RPC("Break", owner, new Il2CppReferenceArray<Il2CppSystem.Object>(0));
                        Il2CppSystem.Object[] array = new Il2CppSystem.Object[2];
                        array[0] = new Il2CppSystem.Single() { m_value = infectedDamage }.BoxIl2CppObject();
                        array[1] = owner;
                        Helper.PlayerMonster.photonView.RPC("KOFOOHFOGHL", PhotonTargets.All, array);
                    }
                }
            }
        }
        public void OnTriggerExit(Collider coll)
        {
            if (coll.gameObject.tag == "Player")
            {
                timer = 0f;
            }
            if (coll.gameObject.tag == "monster" && coll.gameObject.name == PhotonNetwork.player.name)
            {
                timer = 0f;
            }
        }
    }

    [RegisterTypeInIl2Cpp]
    public class MedKit : MonoBehaviour
    {
        public MedKit(IntPtr ptr) : base(ptr) { }
        public PhotonView pv;
        public Transform c;
        public bool canOpen;
        public void Awake()
        {
            pv = GetComponent<PhotonView>();
            c = transform.GetChild(1);
        }
        public void Update()
        {
            if (Helper.Player != null) canOpen = !PlagueController.Instance._playerClass.Infected && Vector3.Distance(Helper.Player.transform.position, c.position) <= 1.5f;
            if (!PlagueController.Instance._playerClass.Infected && Vector3.Distance(Helper.Player.transform.position, c.position) <= 1.5f && Input.GetKeyDown(KeyCode.E))
            {
                Heal(25f);
                pv.ownerId = PhotonNetwork.player.ID;
                PhotonNetwork.Destroy(gameObject);
            }
        }
        public void Heal(float hp)
        {
            var maxHp = Helper.PlayerDamage.NILGDNBIFDC;
            var cHp = Helper.PlayerDamage.PIIDNIGPCDK;
            if (maxHp - cHp < hp)
            {
                Helper.PlayerDamage.PIIDNIGPCDK += maxHp - cHp;
            }
            else
            {
                Helper.PlayerDamage.PIIDNIGPCDK += hp;
            }
        }
        public void OnGUI()
        {
            if (canOpen)
            {
                GUI.skin = PlagueController.Instance.rmm.BIPMFIBNFBC;
                int fontSize = checked(50 * (Screen.width + Screen.height)) / 6000;
                GUI.color = Color.white;
                GUIStyle style = new GUIStyle(GUI.skin.label)
                {
                    fontSize = fontSize
                };
                GUI.Label(new Rect((float)Screen.width * 0.460937f, (float)Screen.height * 0.5f, 300f, 200f), PlagueLocales.usePresstext, style);
            }
        }
    }
    [RegisterTypeInIl2Cpp]
    public class ExplosionZone : MonoBehaviour
    {
        public ExplosionZone(IntPtr ptr) : base(ptr) { }
        public PhotonPlayer killer;
        public bool activated;
        public void Start()
        {
            activated = false;
            var mixer = FindObjectOfType<AudioMixerManager>().EEJANKGKOBO;
            GetComponent<AudioSource>().outputAudioMixerGroup = mixer.FindMatchingGroups("SFX")[0];
            MelonCoroutines.Start(Activated());
        }
        private IEnumerator Activated()
        {
            yield return new WaitForSeconds(0.1f);
            activated = true;
        }
        public void OnTriggerEnter(Collider coll)
        {
            if (activated) return;
            if (coll.gameObject.tag == "Player")
            {
                Helper.PlayerDamage.E10030(200f);
            }
            if (coll.gameObject.tag == "monster" && coll.gameObject.name == PhotonNetwork.player.name)
            {
                if (killer == null) killer = PhotonNetwork.player;
                Il2CppSystem.Object[] array = new Il2CppSystem.Object[2];
                array[0] = new Il2CppSystem.Single() { m_value = 200f }.BoxIl2CppObject();
                array[1] = killer;
                Helper.PlayerMonster.photonView.RPC("KOFOOHFOGHL", PhotonTargets.All, array);
            }
            activated = true;
        }
    }

    [RegisterTypeInIl2Cpp]
    public class MineTrap : MonoBehaviour
    {
        public MineTrap(IntPtr ptr) : base(ptr) { }

        public AudioSource source;
        public Animator animator;
        public int type=1;
        public GameObject parent;
        public bool ready, activated;
        public float lifetime = 90f;

        public void Start()
        {
            ready = false;
            activated = false;
            parent = transform.parent.gameObject;
            source = GetComponent<AudioSource>();
            animator = GetComponent<Animator>();
            var mixer = FindObjectOfType<AudioMixerManager>().EEJANKGKOBO;
            source.outputAudioMixerGroup = mixer.FindMatchingGroups("SFX")[0];
            source.maxDistance = 10;
            source.PlayOneShot(PlagueAssets.Instance._landMineSounds[0]);
            var handler = parent.AddComponent<LandMineRPCHandler>();
            handler.animator = animator;
            handler.source = source;
            handler.mine = this;
            var coll = GetComponent<SphereCollider>();
            if (type == 1)
            {
                animator.Play("MineDeploy1");
                coll.radius = 1.5f;
            }
            if (type == 0)
            {
                animator.Play("MineDeploy0");
                coll.radius = 3.5f;
            }
            MelonCoroutines.Start(Deploying());
        }
        public IEnumerator Deploying()
        {
            yield return new WaitForSeconds(4.7f);
            ready = true;
        }
        public void Update()
        {
            if (!ready) return;
            if (type == 1) return;
            if (lifetime > 0)
            {
                lifetime -= Time.deltaTime;
            }
            else
            {
                if (parent.GetComponent<PhotonView>().isMine)
                {
                    PhotonNetwork.Destroy(parent);
                }
            }
        }
        public void OnTriggerEnter(Collider coll)
        {
            if (!ready) return;
           
        }
        public void OnTriggerStay(Collider coll)
        {
            if (!ready) return;
            if (type == 1)
            {
                if (activated) return;
                if (coll.gameObject.tag == "Player" && PlagueController.Instance._mode == PlagueMode.Cursed)
                {
                    var s = new Il2CppSystem.Single() { m_value = Random.Range(0.25f, 0.5f) }.BoxIl2CppObject();
                    parent.GetComponent<PhotonView>().RPC("Explode", PhotonTargets.All, new Il2CppReferenceArray<Il2CppSystem.Object>(new Il2CppSystem.Object[] { s }));
                }
                if (coll.gameObject.tag == "monster" && coll.gameObject.name == PhotonNetwork.player.name)
                {
                    var inv = PhantomInvisibility.Instance;
                    if (inv != null)
                    {
                        if (!inv.active)
                        {
                            var s = new Il2CppSystem.Single() { m_value = Random.Range(0.25f, 0.5f) }.BoxIl2CppObject();
                            parent.GetComponent<PhotonView>().RPC("Explode", PhotonTargets.All, new Il2CppReferenceArray<Il2CppSystem.Object>(new Il2CppSystem.Object[] { s }));
                        }
                    }
                    else
                    {
                        var s = new Il2CppSystem.Single() { m_value = Random.Range(0.25f, 0.5f) }.BoxIl2CppObject();
                        parent.GetComponent<PhotonView>().RPC("Explode", PhotonTargets.All, new Il2CppReferenceArray<Il2CppSystem.Object>(new Il2CppSystem.Object[] { s }));
                    }
                }
            }
            if (type == 0)
            {
                if (coll.gameObject.tag == "Player")
                {

                }
                if (coll.gameObject.tag == "monster")
                {
                    if (animator.GetCurrentAnimatorClipInfo(0)[0].clip.name != "MineScan")
                    {
                        animator.Play("MineScan");
                    }
                    if (!source.isPlaying)
                    {
                        source.maxDistance = 25;
                        source.PlayOneShot(PlagueAssets.Instance._landMineSounds[1]);
                        if (coll.gameObject.name == PhotonNetwork.player.name)
                        {
                            var inv = PhantomInvisibility.Instance;
                            if (inv != null)
                            {
                                if (inv.active) { inv.pv.RPC("ExposedRPC", PhotonTargets.All, new Il2CppReferenceArray<Il2CppSystem.Object>(0)); }
                            }
                        }
                    }
                }
            }
        }
        public void E100010(Il2CppReferenceArray<Il2CppSystem.Object> tempStorage)
        {
            if (!ready) return;
            if (activated) return;
            var s = new Il2CppSystem.Single() { m_value = Random.Range(0.25f, 0.5f) }.BoxIl2CppObject();
            parent.GetComponent<PhotonView>().RPC("Explode", PhotonTargets.All, new Il2CppReferenceArray<Il2CppSystem.Object>(new Il2CppSystem.Object[] { s }));
        }
    }
    [RegisterTypeInIl2Cpp, HandleRPC]
    public class LandMineRPCHandler : MonoBehaviour
    {
        public LandMineRPCHandler(IntPtr ptr) : base(ptr) { }
        public Animator animator;
        public AudioSource source;
        public MineTrap mine;
        [FunRPC]
        public void Explode(Il2CppSystem.Object delay)
        {
            CuteLogger.Meow("kaboom");
            mine.activated = true;
            MelonCoroutines.Start(ActivateMine(delay.Unbox<float>()));
        }
        public IEnumerator ActivateMine(float delay)
        {
            animator.Play("MineActivate");
            source.PlayOneShot(PlagueAssets.Instance._landMineSounds[2]);
            yield return new WaitForSeconds(delay);
            var expl = GameObject.Instantiate(PlagueAssets.Instance._explosion, transform.position, transform.rotation);
            var e = expl.AddComponent<ExplosionZone>();
            var owner = GetComponent<PhotonView>().owner;
            e.killer = owner;
            if (owner.ID == PhotonNetwork.player.ID)
            {
                PhotonNetwork.Destroy(gameObject);
            }
        }      
    }

    [RegisterTypeInIl2Cpp]
    public class MineSpawner : MonoBehaviour
    {
        public MineSpawner(IntPtr ptr) : base(ptr) { }
        public float cooldown = 30f;
        public float timer;
        public bool ready;
        public static MineSpawner Instance;
        public GameObject mine, previewMine;
        private bool isInPreviewMode = false, validPos;
        private float rotationAmount = 0f;
        public int selectedType = 1;
        public void Awake()
        {
            ready = true;
            Instance = this;
            timer = 21;
            mine = PlagueAssets.Instance._landMine;
        }
        public void OnDisable()
        {
            if (previewMine != null) GameObject.Destroy(previewMine);
        }
        public void Update()
        {
            if (timer > 0)
            {
                timer -= Time.deltaTime;
                ready = false;
            }
            else
            {
                ready = true;
            }
            if (ready)
            {
                HandleObjectPlacement();
                // PlaceWire();
            }
            /*if (Input.GetKeyDown(KeyCode.N))
            {
                timer = 0;
            }*/
            if (Input.GetKeyDown(KeyCode.B))
            {
                if (selectedType == 1) { selectedType = 0; return; }
                if (selectedType == 0) { selectedType = 1; return; }
            }
        }
        void HandleObjectPlacement()
        {
            if (Input.GetKeyDown(KeyCode.G) && !isInPreviewMode)
            {
                EnterPreviewMode();
            }

            if (isInPreviewMode)
            {
                UpdatePreviewPosition();
                RotatePreviewWithScroll();

                if (Input.GetKeyUp(KeyCode.G) && isInPreviewMode)
                {
                    isInPreviewMode = false;                    
                    if (validPos) PlaceMine();
                }
            }
        }
        public void PlaceMine()
        {
            timer = cooldown;
            CuteLogger.Quack("mine spawned");
            var r = PhotonNetwork.NOOU("Custard", previewMine.transform.position, previewMine.transform.rotation, 0, new Il2CppReferenceArray<Il2CppSystem.Object>(new Il2CppSystem.Object[] { "LandMine", PhotonNetwork.player.ID.ToString(), selectedType.ToString() }));
            GameObject.Destroy(previewMine);
        }
        void EnterPreviewMode()
        {
            previewMine = Instantiate(mine);
            Collider[] colliders = previewMine.GetComponentsInChildren<Collider>();
            foreach (Collider col in colliders)
            {
                col.enabled = false;
            }
            isInPreviewMode = true;
        }

        void UpdatePreviewPosition()
        {
            int layerMask = 1 << 0;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            validPos = Physics.Raycast(ray, out hit, 5f, layerMask, QueryTriggerInteraction.Ignore);
            previewMine.transform.rotation = Quaternion.Euler(0, rotationAmount, 0);
            if (validPos)
            {
                var currentPosition = hit.point + hit.normal * 0.1f;
                previewMine.transform.position = currentPosition;
                Quaternion baseRotation = Quaternion.FromToRotation(Vector3.up, hit.normal);
                Quaternion additionalRotation = Quaternion.Euler(0, rotationAmount, 0);
                previewMine.transform.rotation = additionalRotation * baseRotation;
            }
            else
            {
                previewMine.transform.position = new Vector3(0, -1000, 0);
            }
        }

        void RotatePreviewWithScroll()
        {
           // rotationAmount += Input.mouseScrollDelta.y * 10f;
        }
        public void OnGUI()
        {
        }
    }

    [RegisterTypeInIl2Cpp]
    public class WireSpawner : MonoBehaviour
    {
        public WireSpawner(IntPtr ptr) : base(ptr) { }
        public float cooldown = 40f;
        public float timer;
        public bool ready;
        public static WireSpawner Instance;
        public GameObject wire, previewWire;
        private bool isInPreviewMode = false, rotated = false, validPos;
        private float rotationAmount = 0f;
        public void Awake()
        {
            ready = true;
            Instance = this;
            timer = 21;
            wire = PlagueAssets.Instance._wire;
        }
        public void OnDisable()
        {
            if (previewWire != null) GameObject.Destroy(previewWire);
        }
        public void Update()
        {
            if (timer > 0)
            {
                timer -= Time.deltaTime;
                ready = false;
            }
            else
            {
                ready = true;
            }
            if (ready)
            {
                HandleObjectPlacement();
               // PlaceWire();
            }
            if (Input.GetKeyDown(KeyCode.B))
            {
                rotated = true;
            }
            if (Input.GetKeyUp(KeyCode.B))
            {
                rotated = false;
            }
        }
        void HandleObjectPlacement()
        {
            if (Input.GetKeyDown(KeyCode.G) && !isInPreviewMode)
            {
                EnterPreviewMode();
            }

            if (isInPreviewMode)
            {
                UpdatePreviewPosition();
                RotatePreviewWithScroll();

                if (Input.GetKeyUp(KeyCode.G) && isInPreviewMode)
                {
                    isInPreviewMode = false;
                    if (validPos) PlaceWire();
                }
            }
        }
        public void PlaceWire()
        {
            timer = cooldown;
            CuteLogger.Quack("wire spawned");
            var r = PhotonNetwork.NOOU("Custard", previewWire.transform.position, previewWire.transform.rotation, 0, new Il2CppReferenceArray<Il2CppSystem.Object>(new Il2CppSystem.Object[] { "Wire", PhotonNetwork.player.ID.ToString() }));
            GameObject.Destroy(previewWire);
        }
        void EnterPreviewMode()
        {
            previewWire = Instantiate(wire);
            Collider[] colliders = previewWire.GetComponentsInChildren<Collider>();
            foreach (Collider col in colliders)
            {
                col.enabled = false;
            }
            isInPreviewMode = true;
        }

        void UpdatePreviewPosition()
        {
            int layerMask = 1 << 0;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            validPos = Physics.Raycast(ray, out hit, 5f, layerMask, QueryTriggerInteraction.Ignore);
            previewWire.transform.rotation = Quaternion.Euler(0, rotationAmount, 0);
            if (validPos)
            {
                var currentPosition = hit.point + hit.normal * 0.6f;
                previewWire.transform.position = currentPosition;
                if (rotated)
                {
                    Quaternion baseRotation = Quaternion.FromToRotation(Vector3.down, hit.normal);
                    Quaternion additionalRotation = Quaternion.Euler(0, rotationAmount, 0);
                    previewWire.transform.rotation = additionalRotation * baseRotation;
                }
            }
            else
            {
                previewWire.transform.position = new Vector3(0, -1000, 0);
            }
        }

        void RotatePreviewWithScroll()
        {
            if (!rotated) rotationAmount += Input.mouseScrollDelta.y * 10f;
        }
        public void OnGUI()
        {
        }
    }
    [RegisterTypeInIl2Cpp]
    public class MedKitSpawner : MonoBehaviour
    {
        public MedKitSpawner(IntPtr ptr) : base(ptr) { }
        public float cooldown = 30f;
        public float timer;
        public bool ready;
        public static MedKitSpawner Instance;
        public void Awake()
        {
            ready = true;
            Instance = this;
            timer = 21;
        }
        public void Update()
        {
            if (timer > 0)
            {
                timer -= Time.deltaTime;
                ready = false;
            }
            else
            {
                ready = true;
            }
            if (Input.GetKeyDown(KeyCode.G) && ready)
            {
                timer = cooldown;
                var r = PhotonNetwork.NOOU("Custard", gameObject.transform.position + transform.forward + transform.up, gameObject.transform.rotation, 0, new Il2CppReferenceArray<Il2CppSystem.Object>(new Il2CppSystem.Object[] { "Medkit" }));
            }
        }
        public void OnGUI()
        {
        }
    }
    [RegisterTypeInIl2Cpp, HandleRPC]
    public class PhantomInvisibility : MonoBehaviour
    {
        public PhantomInvisibility(IntPtr ptr) : base(ptr) { }
        public bool ready, _enabled, active;
        public PhotonView pv;
        public PlagueMonster plagueMonster;
        public GameObject body;
        public GameObject blackExplosion, blackSmoke;
        public float cooldown = 35f, duration = 10f, timer;
        public static PhantomInvisibility Instance;
        private object _ctoken;

        public void Start()
        {
            timer = 5;
            active = false;
            pv = gameObject.GetComponent<PhotonView>();
            if (pv.isMine) Instance = this;
            if (pv.isMine) plagueMonster = gameObject.GetComponent<PlagueMonster>();
            ready = true;
            if (transform.FindChild("Phantom(Clone)") != null)
            {
                _enabled = true;
                body = transform.FindChild("Phantom(Clone)/Model").gameObject;
                blackExplosion = transform.FindChild("Phantom(Clone)/Smoke Spikes").gameObject;
                blackSmoke = transform.FindChild("Phantom(Clone)/Smoke").gameObject;
                blackSmoke.SetActive(false);
            }
        }
        public void Update()
        {
            if (timer > 0)
            {
                timer -= Time.deltaTime;
                ready = false;
            }
            else
            {
                ready = true;
            }
            if (ready && Input.GetKeyDown(KeyCode.G) && pv.isMine && _enabled)
            {
                timer = cooldown;
                pv.RPC("InvisRPC", PhotonTargets.All, new Il2CppReferenceArray<Il2CppSystem.Object>(0));
            }
        }
        [FunRPC]
        public void ExposedRPC()
        {
            if (active)
            {
                MelonCoroutines.Stop(_ctoken);
                active = false;
                if (pv.isMine)
                {
                    pv.RPC("InvisSoundRPC", PhotonTargets.All, new Il2CppReferenceArray<Il2CppSystem.Object>(new Il2CppSystem.Object[] { new Il2CppSystem.Boolean() { m_value = false }.BoxIl2CppObject() }));
                    plagueMonster._fps.HKCDMBALAAK.RunSpeed = plagueMonster._class.RunSpeed;
                }
                body.SetActive(true);
                blackExplosion.SetActive(false);
                blackExplosion.SetActive(true);
                blackSmoke.SetActive(false);
            }
        }
        public IEnumerator Invisibility()
        {
            active = true;
            ready = false;
            body.SetActive(false);
            blackExplosion.SetActive(false);
            blackExplosion.SetActive(true);
            blackSmoke.SetActive(true);
            if (pv.isMine)
            {
                pv.RPC("InvisSoundRPC", PhotonTargets.All, new Il2CppReferenceArray<Il2CppSystem.Object>(new Il2CppSystem.Object[] { new Il2CppSystem.Boolean() { m_value = true }.BoxIl2CppObject() }));
                plagueMonster._fps.HKCDMBALAAK.RunSpeed = plagueMonster._class.WalkSpeed;

            }
            yield return new WaitForSeconds(duration);
            active = false;
            if (pv.isMine)
            {
                pv.RPC("InvisSoundRPC", PhotonTargets.All, new Il2CppReferenceArray<Il2CppSystem.Object>(new Il2CppSystem.Object[] { new Il2CppSystem.Boolean() { m_value = false }.BoxIl2CppObject() }));
                plagueMonster._fps.HKCDMBALAAK.RunSpeed = plagueMonster._class.RunSpeed;
            }
            body.SetActive(true);
            blackExplosion.SetActive(false);
            blackExplosion.SetActive(true);
            blackSmoke.SetActive(false);
        }
        [FunRPC]
        public void InvisRPC()
        {
            _ctoken = MelonCoroutines.Start(Invisibility());
        }
    }
    [RegisterTypeInIl2Cpp, HandleRPC]
    public class InfectedRage : MonoBehaviour
    {
        public InfectedRage(IntPtr ptr) : base(ptr) { }
        public Light light;
        public bool _enabled, rage, ready;
        public PlagueMonster plagueMonster;
        public PhotonView pv;
        public float cooldown = 35f, duration = 5f, timer;
        public static InfectedRage Instance;

        public void Start()
        {
            pv = gameObject.GetComponent<PhotonView>();
            if (pv.isMine) Instance = this;
            if (pv.isMine) plagueMonster = gameObject.GetComponent<PlagueMonster>();
            ready = true;
            var killmepls = new GameObject();
            killmepls.transform.SetParent(transform, false);
            light = killmepls.AddComponent<Light>();
            light.color = Color.red;
            light.intensity = 5f;
            light.enabled = false;
            timer = 10;
           
        }
        public void Update()
        {
            if (timer > 0)
            {
                timer -= Time.deltaTime;
                ready = false;
            }
            else
            {
                ready = true;
            }
            if (ready && Input.GetKeyDown(KeyCode.G) && pv.isMine && _enabled /*&& plagueMonster._playerMonster.PIIDNIGPCDK <= plagueMonster._class.Health / 2*/)
            {
                timer = cooldown;
                pv.RPC("RageRPC", PhotonTargets.All, new Il2CppReferenceArray<Il2CppSystem.Object>(0));
                pv.RPC("RageSoundRPC", PhotonTargets.All, new Il2CppReferenceArray<Il2CppSystem.Object>(0));
            }
        }
        public IEnumerator Rage()
        {
            rage = true;
            if (pv.isMine) plagueMonster._fps.HKCDMBALAAK.RunSpeed = plagueMonster._class.RunSpeed * 1.2f;
            light.enabled = true;
            yield return new WaitForSeconds(duration);
            if (pv.isMine) plagueMonster._fps.HKCDMBALAAK.RunSpeed = plagueMonster._class.RunSpeed;
            rage = false;
            light.enabled = false;
        }
        [FunRPC]
        public void RageRPC()
        {
            MelonCoroutines.Start(Rage());
        }
    }

    [RegisterTypeInIl2Cpp]
    public class InfectedVision : MonoBehaviour
    {
        public InfectedVision(IntPtr ptr) : base(ptr) { }
        private CameraFilterPack_Colors_Adjust_PreFilters filter;
        private Light light;
        public void Start()
        {
            MelonCoroutines.Start(Govno3());
        }
        public IEnumerator Govno3()
        {
            yield return new WaitForSeconds(0.1f);
            filter = gameObject.AddComponent<CameraFilterPack_Colors_Adjust_PreFilters>();
            CuteLogger.Quack("Added CFP");
            var killmepls = new GameObject("fucking light");
            killmepls.transform.SetParent(transform, false);
            light = killmepls.AddComponent<Light>();
            CuteLogger.Quack("Added Light");
            light.enabled = true;
            light.range = 500f;
            CuteLogger.Quack("500");
            light.renderMode = LightRenderMode.ForcePixel;
            CuteLogger.Quack("Light configured");
            light.enabled = false;
            CuteLogger.Quack("Light disabled");
            filter.IAIGCDCDAFP = CameraFilterPack_Colors_Adjust_PreFilters.HILHDCJPDFP.BlackAndWhite_Red;
            filter.enabled = false;
            CuteLogger.Quack("Filter disabled");
            var l = gameObject.GetComponent<Light>();
            if (l != null) { GameObject.Destroy(l); }
        }
        public void Update()
        {
            if (Input.GetKeyDown(KeyCode.F))
            {
                filter.enabled = !filter.enabled;
                light.enabled = filter.enabled;
            }
        }

    }
    [RegisterTypeInIl2Cpp, HandleRPC]
    public class PlagueMonster : MonoBehaviour
    {
        public PlagueMonster(IntPtr ptr) : base(ptr) { }
        public AudioSource _soundsSource;
        public PhotonView _photonView;
        public PlayerMonster _playerMonster;
        public FPScontroller _fps;
        public float _hp;
        public Vector3 _fpvCam, _tpvCam;
        public Transform _fpvCamTarget, _tpvCamTarget;
        public TPSCamera _cam;
        public float _currentHp, _seekTimer;
        public bool _randomized, _seekReady, _knockbacked, _isFpvCam;
        public PlagueClass _class;
        public GameObject _HUD, _healthBar;
        public static PlagueMonster Instance;

        public void Awake()
        {
            
            _soundsSource = gameObject.AddComponent<AudioSource>();
            var mixer = FindObjectOfType<AudioMixerManager>().EEJANKGKOBO;
            _soundsSource.outputAudioMixerGroup = mixer.FindMatchingGroups("SFX")[0];
            _soundsSource.spatialBlend = 1f;
            _soundsSource.rolloffMode = AudioRolloffMode.Linear;
            _seekTimer = 5;
            _knockbacked = false;
            _soundsSource.spread = 100f;
            _photonView = GetComponent<PhotonView>();
            if (_photonView.isMine ) Instance = this;
            _playerMonster = GetComponent<PlayerMonster>();
            _playerMonster.HFKADHCBMFM = Helper.RandomSound(PlagueAssets.Instance._hitSounds);
            _playerMonster.FKGANOHLFLH = _soundsSource;
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
        public void ChangeCameraView(Transform target, Vector3 pos)
        {
            _cam.PFDKNPELPNC = target;
            _cam.BIFDOBDPKKE = pos.z;
            _cam.IHCOCLHHAGD = pos.x;
            _cam.LOCIMKAGAMF = pos.y;
        }
        public IEnumerator SetUpCamera()
        {
            yield return new WaitForSeconds(1f);
            _cam = GameObject.FindObjectOfType<TPSCamera>();
            _tpvCam = new Vector3(_cam.IHCOCLHHAGD, _cam.LOCIMKAGAMF, _cam.BIFDOBDPKKE);
            _tpvCamTarget = _cam.PFDKNPELPNC;
            if (_class.FPVCam != Vector3.zero) { _fpvCam = _class.FPVCam; }
            else { _fpvCam = _tpvCam; }
            if (_class.FPVCamTarget != null) { _fpvCamTarget = transform.FindChild(_class.FPVCamTarget); }
            else { _fpvCamTarget = _tpvCamTarget; }
        }
        public IEnumerator RandomizeHitSound()
        {
            _randomized = true;
            yield return new WaitForSeconds(0.25f);
            _playerMonster.HFKADHCBMFM = Helper.RandomSound(PlagueAssets.Instance._hitSounds);
            _randomized = false;
        }
        public void Knockback()
        {
            if (_photonView.isMine && !_knockbacked)
            {
                if (_class.ClassName == "???") return;
                var r = GetComponent<InfectedRage>(); if (r != null) { if (r.rage) return; }
                var i = GetComponent<PhantomInvisibility>(); if (i != null) { if (i.active) return; }
                MelonCoroutines.Start(DoKnockback());
            }
        }
        public IEnumerator DoKnockback()
        {
            _knockbacked = true;
            _fps.HKCDMBALAAK.RunSpeed = 4f;
            yield return new WaitForSeconds(Random.RandomRange(0.2f, 0.4f));
            _fps.HKCDMBALAAK.RunSpeed = _class.RunSpeed;
            _knockbacked = false;
        }
        public void SetUp()
        {
            _playerMonster.PIIDNIGPCDK = _class.Health;
            _playerMonster.OMFJFIPPGPE = (int)_class.Damage;
            if (_class.CameraX != 0)
            {
                var camera = _playerMonster.AJAKADNJOML.GetComponent<TPSCamera>();
                camera.BIFDOBDPKKE = _class.CameraX;
                camera.LOCIMKAGAMF = _class.CameraY;
            }
            _fps.AALHECCKHFD.baseHeight = _class.JumpHeight;
            _fps.HKCDMBALAAK.RunSpeed = _class.RunSpeed;
            if (_class.Rage) gameObject.GetComponent<InfectedRage>()._enabled = true;
            MelonCoroutines.Start(SetUpCamera());
        }

        [FunRPC]
        public void HealRPC()
        {
            _soundsSource.minDistance = 4f;
            _soundsSource.maxDistance = 30f;
            _soundsSource.PlayOneShot(PlagueAssets.Instance._healSound);
            GameObject.Instantiate(PlagueAssets.Instance._greenSmoke, gameObject.transform.position, Quaternion.identity);
            if (_photonView.isMine)
            {
                _playerMonster.PIIDNIGPCDK += 100f;
            }
        }
        [FunRPC]

        public void InfectedSoundRPC(string className)
        {
            CuteLogger.Meow("InfectedSoundRPC");
            _soundsSource.minDistance = 4f;
            _soundsSource.maxDistance = 30f;
            _soundsSource.dopplerLevel = 0;
            if (className == "Hunter")
            {
                _soundsSource.PlayOneShot(PlagueAssets.Instance._hunterSound);
            }
            else if (className == "Phobos")
            {
                _soundsSource.PlayOneShot(PlagueAssets.Instance._nemesisSound);
            }
            else
            {
                _soundsSource.PlayOneShot(Helper.RandomSound(PlagueAssets.Instance._infectedSounds));
            }
        }
        [FunRPC]
        public void RageSoundRPC()
        {
            _soundsSource.minDistance = 4f;
            _soundsSource.maxDistance = 30f;
            _soundsSource.PlayOneShot(PlagueAssets.Instance._rageSound);
        }
        [FunRPC]
        public void InvisSoundRPC(bool start)
        {
            _soundsSource.minDistance = 4f;
            _soundsSource.maxDistance = 25f;
            if (start) _soundsSource.PlayOneShot(PlagueAssets.Instance._invisStart);
            else _soundsSource.PlayOneShot(PlagueAssets.Instance._invisEnd);
        }
        public void LateUpdate()
        {
            if (Input.GetKeyDown(KeyCode.V))
            {
                if (!_isFpvCam)
                {
                    _isFpvCam = true;
                    ChangeCameraView(_fpvCamTarget, _fpvCam);
                }
                else
                {
                    _isFpvCam = false;
                    ChangeCameraView(_tpvCamTarget, _tpvCam);
                }
            }
        }
        public void Update()
        {
            
            if (!_randomized && _playerMonster.DGMIHBAHNDN)
            {
                MelonCoroutines.Start(RandomizeHitSound());
            }
            if (_fps.PKHECMAENAP != Vector3.zero)
            {
                _seekTimer = 5;
                _seekReady = false;
                return;
            }
            if (_seekTimer > 0)
            {
                _seekTimer -= Time.deltaTime;
                _seekReady = false;
            }
            else
            {
                _seekReady = true;
            }
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
            if (_photonView.isMine && HudHider.Instance != null && !HudHider.Instance.hidden)
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
        public float WalkSpeed;
        public bool CustomPrefab;
        public float JumpHeight;
        public float Damage;
        public string Prefab;
        public bool Rage;
        public float CameraX = 0;
        public float CameraY = 0;
        public Vector3 FPVCam;
        public string FPVCamTarget;
        public List<string> Weapons;
        public bool Infected;
        public bool Boss;
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
                return;
            }
            else
            {
                var s = KOHGOLCGCAN.Cast<PhotonStream>();
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

    [RegisterTypeInIl2Cpp, HandleRPC]
    public class PlagueController : MonoBehaviour
    {
        public PlagueController(IntPtr ptr) : base(ptr) { }

        #region PROPERTY_NAMES
        private readonly string RefTime = "RefTime'";
        private readonly string CDTime = "CDTime'";
        #endregion
        public float RestartTime = 10f;
        public float CountdownTime = 20f;
        public float PlayerListUpdateTime = 0.05f;
        public float RoundDuration = 1;

        public RoomMultiplayerMenu rmm => Helper.RoomMultiplayerMenu;
        public static PlagueController Instance { get; set; }


        private Material _skybox;
        public AudioSource _musicSource, _soundsSource, _otherSource;
        private bool _countdownPlayed, _winPlayed, _roundStartPlayed, _roundEndPlayed;

        public float _armor, _jumpHeight, _runSpeed;
        public PlagueClass _playerClass;

        public int _testint = 1;
        public float _testx = 200, _testy = 250;
        public bool canBlood { get { return _bloodTimer <= 0; } }
        private bool _plagueStarted, _restarting, _survivorsWin, _infectedWin, _draw;
        private bool _waitingForPlayers, _playersListUpdated, _lpFlag;
        private bool _refTimeSet, _classSet, _supplyDrop, _showMessage;
        private bool _debugMode;
        private float _referenceTime, _startCdTime, _bloodTimer;
        private float _plagueTimer, _countDown;
        private float _roundDuration;
        private int _totalRounds;
        private string _message;
        public PlagueMode _mode;
        private PlagueMode _testMode;
        private AmbientMode _d_AmbientMode;
        private Color _d_AmbientLight, _d_FogColor;
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
                _playerClass.Health = 125;
                _playerClass.Armor = 15;
                _playerClass.RunSpeed = 10f;
                _playerClass.JumpHeight = 1.5f;
            }
            if (classID == 2)
            {
                _playerClass.ClassName = "Medic";
                _playerClass.Weapons = new List<string>()
                {
                    "VZ61",
                    "MCS870"
                };
                _playerClass.Health = 100;
                _playerClass.Armor = 0;
                _playerClass.RunSpeed = 10.25f;
                _playerClass.JumpHeight = 1.3f;
            }
            if (classID == 3)
            {
                _playerClass.ClassName = "Heavy";
                _playerClass.Weapons = new List<string>()
                {
                    "MCS870",
                    "44 Combat"
                };
                _playerClass.Health = 150;
                _playerClass.Armor = 30;
                _playerClass.RunSpeed = 9f;
                _playerClass.JumpHeight = 1f;
            }
            if (classID == 4)
            {
                _playerClass.ClassName = "Scout";
                _playerClass.Weapons = new List<string>()
                {
                    "XIX II",
                    "M40A3"
                };
                _playerClass.Health = 100;
                _playerClass.Armor = 0;
                _playerClass.RunSpeed = 11f;
                _playerClass.JumpHeight = 2f;
            }
            if (classID == 777)
            {
                _playerClass.ClassName = "Last Survivor";
                _playerClass.Weapons = new List<string>()
                {
                    "M249-Saw",
                    "Turbulent-7",
                    "Balrog-1",
                    "Katana"
                };
                _playerClass.Health = 100;
                _playerClass.Armor = 85 + int.Parse(Helper.GetRoomProperty("Players").ToString());
                _playerClass.RunSpeed = 11f;
                _playerClass.JumpHeight = 2f;
                _playerClass.Boss = true;
            }
            _playerClass.Infected = false;
            // Helper.RemoveWeapons();
            MelonCoroutines.Start(Govno2());
            //Helper.SetProperty("MaxHP", _playerClass.Health.ToString());
            Helper.SetProperty("Armor", new Il2CppSystem.Single() { m_value = _playerClass.Armor }.BoxIl2CppObject());
            Helper.SetProperty("Class", _playerClass.ClassName);
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
                _playerClass.WalkSpeed = 4f;
                _playerClass.JumpHeight = 3f;
                _playerClass.Health = 80;
                _playerClass.Damage = 40;
                _playerClass.Prefab = "INF/PlayerNewborn";
                _playerClass.FPVCamTarget = "Model/Bip001/Bip001 Pelvis/Bip001 Spine/Bip001 Spine1/Bip001 Spine2/Bip001 Spine3/Bip001 Neck/head_Slendy";
                _playerClass.FPVCam = new Vector3(0f, 0.3f, -0.5f);
            }
            if (classID == 2)
            {
                _playerClass.ClassName = "Jumper";
                _playerClass.RunSpeed = 10.5f;
                _playerClass.WalkSpeed = 4f;
                _playerClass.JumpHeight = 6f;
                _playerClass.Health = 100;
                _playerClass.Damage = 30;
                _playerClass.Prefab = "VS/PlayerNewborn";
                _playerClass.FPVCamTarget = "Model/Bip001/Bip001 Pelvis/Bip001 Spine/Bip001 Spine1/Bip001 Spine2/Bip001 Spine3/Bip001 Neck/head_Slendy";
                _playerClass.FPVCam = new Vector3(0f, 0.3f, -0.5f);
            }
            if (classID == 3)
            {
                _playerClass.ClassName = "Tank";
                _playerClass.RunSpeed = 9.5f;
                _playerClass.WalkSpeed = 4f;
                _playerClass.JumpHeight = 2f;
                _playerClass.Health = 300;
                _playerClass.Damage = 70;
                _playerClass.Rage = true;
                _playerClass.CustomPrefab = true;
                _playerClass.Prefab = "Tank";
                _playerClass.FPVCamTarget = "Tank(Clone)/Model/mixamorig:Hips/mixamorig:Spine/mixamorig:Spine1/mixamorig:Spine2/tubby_head (3)";
                _playerClass.FPVCam = new Vector3(0f, 0.3f, -0.4f);
            }
            if (classID == 4)
            {
                _playerClass.ClassName = "Hunter";
                _playerClass.RunSpeed = 10.5f;
                _playerClass.WalkSpeed = 4f;
                _playerClass.JumpHeight = 3f;
                _playerClass.Health = 150;
                _playerClass.Damage = 40;
                _playerClass.CustomPrefab = true;
                _playerClass.Prefab = "Hunter";
                _playerClass.FPVCamTarget = "MutantTubby(Clone)/Model/Idle/mixamorig:Hips/mixamorig:Spine/mixamorig:Spine1/mixamorig:Spine2/mixamorig:Neck/mixamorig:Head/Low_Poly_Mask_Model07";
                _playerClass.FPVCam = new Vector3(0f, 0.7f, -0.5f);
            }
            if (classID == 5)
            {
                _playerClass.ClassName = "Phantom";
                _playerClass.RunSpeed = 10.5f;
                _playerClass.WalkSpeed = 4f;
                _playerClass.JumpHeight = 3f;
                _playerClass.Health = 120;
                _playerClass.Damage = 30;
                _playerClass.CustomPrefab = true;
                _playerClass.Prefab = "Phantom";
                _playerClass.FPVCamTarget = "Phantom(Clone)/Model/mixamorig:Hips/mixamorig:Spine/mixamorig:Spine1/mixamorig:Spine2/mixamorig:Neck/mixamorig:Head";
                _playerClass.FPVCam = new Vector3(0f, 0.1f, -0.4f);
            }
            if (classID == 665)
            {
                _playerClass.ClassName = "???";
                _playerClass.RunSpeed = 10.5f;
                _playerClass.JumpHeight = 3f;
                _playerClass.Health = 200;
                _playerClass.Damage = 200;
                _playerClass.Rage = false;
                _playerClass.Prefab = "VS/PlayerTinkyOld";
                _playerClass.FPVCamTarget = null;
                _playerClass.FPVCam = Vector3.zero;
            }
            if (classID == 666)
            {
                _playerClass.ClassName = "Phobos";
                _playerClass.RunSpeed = 12f;
                _playerClass.JumpHeight = 3f;
                _playerClass.Health = 2000 * int.Parse(Helper.GetRoomProperty("Players").ToString());
                _playerClass.Damage = 150;
                _playerClass.CustomPrefab = true;
                _playerClass.Prefab = "Phobos";
                _playerClass.CameraX = 8.25f;
                _playerClass.CameraY = 3.35f;
                _playerClass.Boss = true;
            }
            _playerClass.Infected = true;
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
            _roundDuration = float.Parse(Helper.GetRoomProperty("RD004'").Unbox<int>().ToString()) / 2;
            if (PhotonNetwork.isMasterClient)
            {
                OnPhotonPlayerConnected.onPhotonPlayerConnected += OnPlayerJoined;
                OnPhotonPlayerDisconnected.onPhotonPlayerDisonnected += OnPlayerLeft;
                _waitingForPlayers = true;
                _musicSource.loop = true;
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
            _d_AmbientMode = RenderSettings.ambientMode;
            _d_AmbientLight = new Color(RenderSettings.ambientLight.r, RenderSettings.ambientLight.g, RenderSettings.ambientLight.b, RenderSettings.ambientLight.a);
            _d_FogColor = new Color(RenderSettings.fogColor.r, RenderSettings.fogColor.g, RenderSettings.fogColor.b, RenderSettings.fogColor.a);
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

        /*void LateUpdate()
        {
            if (_testint != 1) return;
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
                PhotonNetwork.NOOU("Custard", Helper.Player.transform.position + Helper.Player.gameObject.transform.forward, Helper.Player.transform.rotation, 0, new Il2CppReferenceArray<Il2CppSystem.Object>(new Il2CppSystem.Object[] { "Supply" }));
            }
            if (Input.GetKeyDown(KeyCode.Keypad4))
            {
                PhotonNetwork.NOOU("Custard", Helper.Player.transform.position + Helper.Player.gameObject.transform.forward, Helper.Player.transform.rotation, 0, new Il2CppReferenceArray<Il2CppSystem.Object>(new Il2CppSystem.Object[] { "Medkit" }));
            }
            if (Input.GetKeyDown(KeyCode.Keypad5))
            {
                Helper.GiveWeapon(Helper.WeaponManager, "Skull-1");
            }
            if (Input.GetKeyDown(KeyCode.Keypad6))
            {
                Helper.GiveWeapon(Helper.WeaponManager, "SF Gun");
            }
            if (Input.GetKeyDown(KeyCode.Keypad1))
            {
                Helper.GiveWeapon(Helper.WeaponManager, "Skull-5");
            }
            if (Input.GetKeyDown(KeyCode.Keypad2))
            {
                Helper.GiveWeapon(Helper.WeaponManager, "Balrog-1");
            }
            if (Input.GetKeyDown(KeyCode.Keypad3))
            {
                Helper.GiveWeapon(Helper.WeaponManager, "Turbulent-7");
            }
            if (Input.GetKeyDown(KeyCode.Keypad0))
            {
                _mode = PlagueMode.Cursed;
                Helper.SetRoomProperty("PlagueMode", "Cursed");
            }
        }*/

        void Update()
        {
            Cursor.visible = rmm.IOMIAHNBDOG;
            if (_bloodTimer > 0)
            {
                _bloodTimer -= Time.deltaTime;
            }
            _plagueStarted = !_waitingForPlayers && _countDown <= 0f && !_restarting;
            _waitingForPlayers = false; //_allPlayers.Count < 2;
            if (!_playersListUpdated && PhotonNetwork.isMasterClient)
            {
                MelonCoroutines.Start(UpdatePlayersList());
            }

            if (!_plagueStarted && !_waitingForPlayers)
            {
                //10 sec count down
                _countDown = CountdownTime - ((float)PhotonNetwork.time - _startCdTime);
                if (!_countdownPlayed)
                {
                    _countdownPlayed = true;
                    if (PhotonNetwork.isMasterClient) RandomizeGameMode();
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
                            StartInfection();
                            CuteLogger.Meow("InfectRandomPlayer update");
                            PhotonNetwork.room.IsOpen = true;
                            PhotonNetwork.room.IsVisible = true;
                        }
                        //PlaySound(Helper.RandomSound(PlagueAssets.Instance._roundStartSounds));
                    }
                }
                _plagueTimer = _roundDuration - ((float)PhotonNetwork.time - _referenceTime);
                if (_plagueTimer <= 0 && !_restarting)
                {
                    SurvivorsWin();
                }
                if (Mathf.CeilToInt(_plagueTimer) == 20 && !_roundEndPlayed && _plagueStarted && _playerClass != null)
                {
                    _roundEndPlayed = true;
                    if (!_playerClass.Infected) PlayMusic(Helper.RandomSound(PlagueAssets.Instance._roundEndSounds));
                }
                if (Mathf.CeilToInt(_plagueTimer) == _roundDuration / 2 && !_supplyDrop && _plagueStarted && _playerClass != null)
                {
                    _supplyDrop = true;
                    if (!_playerClass.Infected) PlaySound(PlagueAssets.Instance._supplyDropSound);
                    if (PhotonNetwork.isMasterClient)
                    {
                        var obj = GameObject.FindGameObjectsWithTag("custardPos");
                        var pos = obj[Random.RandomRange(0, obj.Count - 1)].transform;
                        PhotonNetwork.NOOU("Custard", pos.position + Helper.Player.gameObject.transform.forward, pos.rotation, 0, new Il2CppReferenceArray<Il2CppSystem.Object>(new Il2CppSystem.Object[] { "Supply" }));
                    }
                }
            }
        }
        void FixedUpdate()
        {

        }
        public void BloodSplash(Vector3 pos)
        {
            if (canBlood) 
            {
                GameObject.Instantiate(PlagueAssets.Instance._blood, pos, Quaternion.identity);
                _bloodTimer = 0.5f;
            }
        }
        public void Message(string message, string player = "", string color = "")
        {
            rmm.photonView.RPC("networkAddMessage", PhotonTargets.All, new Il2CppReferenceArray<Il2CppSystem.Object>(new Il2CppSystem.String[] { player, "", message, color }));
        }
        public void RandomizePlagueClass()
        {
            SurvivorClassSetUp(UnityEngine.Random.Range(1, 5));
        }
        public void StartInfection()
        {
            if (_mode == PlagueMode.Infection || _mode == PlagueMode.Cursed) { InfectRandomPlayer(); }
            if (_mode == PlagueMode.LastSurvivor)
            {
                var target = _allPlayers[UnityEngine.Random.Range(0, _allPlayers.Count)];
                rmm.gameObject.GetComponent<PhotonView>().RPC("SetPlayerAsSurvivor", target, new Il2CppReferenceArray<Il2CppSystem.Object>(0));
                Helper.SetRoomProperty("PlagueSurvivor", target.name.Split('|')[0]);
            }
            if (_mode == PlagueMode.Nemesis)
            {
                var target = _allPlayers[UnityEngine.Random.Range(0, _allPlayers.Count)];
                rmm.gameObject.GetComponent<PhotonView>().RPC("SetPlayerAsNemesis", target, new Il2CppReferenceArray<Il2CppSystem.Object>(0));
                Helper.SetRoomProperty("PlagueNemesis", target.name.Split('|')[0]);
            }
            rmm.photonView.RPC("StartInfectionRPC", PhotonTargets.All, new Il2CppReferenceArray<Il2CppSystem.Object>(0));
        }   
        [FunRPC]
        public void StartInfectionRPC()
        {
            if (Helper.GetRoomProperty("PlagueMode") == null) return;
            var gm = Helper.GetRoomProperty("PlagueMode").ToString();
            CuteLogger.Meow("StartInfectionRPC GM " + gm);
            if (gm == "Cursed")
            {
                PlayMusic(Helper.RandomSound(PlagueAssets.Instance._cursedStart));
             //   MelonCoroutines.Start(PlayAmbience(PlagueAssets.Instance._cursedAmbience, 13f));
                MelonCoroutines.Start(ShowMessage("a horrible chill goes down your spine...", 10f));
              //  RenderSettings.ambientMode = AmbientMode.Flat;
              //  RenderSettings.ambientLight = new Color(1f, 0f, 0f, 1f);
              //  RenderSettings.fogColor = new Color(1f, 0f, 0f, 1f);
            }
            if (gm == "LastSurvivor")
            {
                PlayMusic(Helper.RandomSound(PlagueAssets.Instance._survivorStart));
                MelonCoroutines.Start(ShowMessage($"{Helper.GetRoomProperty("PlagueSurvivor").ToString()} is the last survivor!", 10f));
            }
            if (gm == "Nemesis")
            {
                PlayMusic(Helper.RandomSound(PlagueAssets.Instance._nemesisStart));
                MelonCoroutines.Start(ShowMessage($"{Helper.GetRoomProperty("PlagueNemesis").ToString()} is the Nemesis!", 10f));
            }
            if (gm == "Massacre")
            {
                PlayMusic(Helper.RandomSound(PlagueAssets.Instance._swarmStart));
                MelonCoroutines.Start(ShowMessage($"Massacre!", 10f));
            }
            if (gm == "Armageddon")
            {
                PlayMusic(Helper.RandomSound(PlagueAssets.Instance._swarmStart));
                MelonCoroutines.Start(ShowMessage($"ARMAGEDDON!!!", 10f));
            }
        }
        public IEnumerator PlayAmbience(AudioClip ambience, float delay)
        {
            yield return new WaitForSeconds(delay);
            PlayMusic(ambience, true);
        }
        public IEnumerator ShowMessage(string message, float time)
        {
            _message = message;
            _showMessage = true;
            yield return new WaitForSeconds(time);
            _showMessage = false;
        }
        public void RandomizeGameMode()
        {
            PlagueMode mode = PlagueMode.Infection;
            var value1 = UnityEngine.Random.Range(0, 100);
            if (value1 <= 10)
            {
                if (_allPlayers.Count >= 5) { mode = PlagueMode.Cursed; }
              //  if (_allPlayers.Count >= 7 && _allPlayers.Count < 10) { mode = (PlagueMode)UnityEngine.Random.Range(1, 4); }
               // if (_allPlayers.Count >= 10) { mode = (PlagueMode)UnityEngine.Random.Range(1, 6); }
            }
            _mode = mode;
            if (_testMode != 0) { _mode = _testMode; }
            Helper.SetRoomProperty("PlagueMode", _mode.ToString());
            CuteLogger.Meow("Randomized GM " + Helper.GetRoomProperty("PlagueMode").ToString());

        }
        public void InfectRandomPlayer()
        {
            var target = _allPlayers[UnityEngine.Random.Range(0, _allPlayers.Count)];
            rmm.gameObject.GetComponent<PhotonView>().RPC("InfectPlayer", target, new Il2CppReferenceArray<Il2CppSystem.Object>(new Il2CppSystem.Object[] { new Il2CppSystem.Boolean() { m_value = true }.BoxIl2CppObject() }));
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
            if (PhotonNetwork.isMasterClient)
            {
                Helper.SetRoomProperty("Players", _allPlayers.Count.ToString());
                if (_teamAPlayers.Count <= 0 && _allPlayers.Count > 1 && !_restarting)
                {
                    rmm.gameObject.GetComponent<PhotonView>().RPC("InfectedWin", PhotonTargets.All, new Il2CppReferenceArray<Il2CppSystem.Object>(0));
                    //InfectedWin();
                }
                if (_teamBPlayers.Count <= 0 && _plagueStarted && !_restarting && _lpFlag)
                {
                    rmm.gameObject.GetComponent<PhotonView>().RPC("SurvivorsWin", PhotonTargets.All, new Il2CppReferenceArray<Il2CppSystem.Object>(0));
                    //SurvivorsWin();
                }
            }
            yield return new WaitForSeconds(PlayerListUpdateTime);
            _playersListUpdated = false;
        }


        IEnumerator Govno()
        {
            yield return new WaitForSeconds(3f);
            _lpFlag = true;
        }
        IEnumerator Govno2()
        {
            yield return new WaitForSeconds(0.3f);
            foreach (string weapon in _playerClass.Weapons)
            {
                Helper.GiveWeapon(FindObjectOfType<WeaponManager>(), weapon);
            }
            Helper.PlayerDamage.NILGDNBIFDC = _playerClass.Health;
            Helper.PlayerDamage.PIIDNIGPCDK = _playerClass.Health;
            Helper.FPSController.AALHECCKHFD.baseHeight = _playerClass.JumpHeight;
            Helper.FPSController.HKCDMBALAAK.RunSpeed = _playerClass.RunSpeed;
            Helper.SetProperty("MaxHP", new Il2CppSystem.Single() { m_value = _playerClass.Health }.BoxIl2CppObject());
            if (_playerClass.ClassName == "Medic")
            {
                Helper.Player.AddComponent<MedKitSpawner>();
            }
            if (_playerClass.ClassName == "Heavy")
            {
                Helper.Player.AddComponent<WireSpawner>();
            }
            if (_playerClass.ClassName == "Scout")
            {
                Helper.Player.AddComponent<MineSpawner>();
            }
        }
        public void PlayMusic(AudioClip clip, bool loop = false)
        {
            _musicSource.PlayOneShot(clip, 1f);
            _musicSource.loop = loop;
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
        public void SetPlayerAsSurvivor()
        {
            rmm.SpawnPlayer("LastSurvivor");
        }
        [FunRPC]
        public void SetPlayerAsNemesis()
        {
            rmm.SpawnPlayer("Nemesis");
        }
        [FunRPC]
        public void InfectPlayer(bool firstInfected = false)
        {
            if (firstInfected) { Message("is the first infected", PhotonNetwork.player.name.Split('|')[0], "b"); }
            rmm.SpawnPlayer("Team B");
        }

        public void SpawnInfectedPlayer(int classId = 0)
        {
            var team_2 = rmm.KGLOGDGOELM;
            var c = Helper.GetRoomProperty("PlagueMode");
            string gm = "Infection";
            if (c != null) gm = c.ToString();
            CuteLogger.Meow(gm);
            int num = UnityEngine.Random.Range(0, team_2.spawnPoints.Length);
            int num2 = UnityEngine.Random.Range(1, 6);
            PlagueClass infClass = null;

            if (classId == 0) infClass = InfectedClassSetUp(num2);
            else infClass = InfectedClassSetUp(classId);
            if (gm == "Cursed") infClass = InfectedClassSetUp(665);

            if (infClass.CustomPrefab)
            {
                rmm.CNLHJAICIBH = PhotonNetwork.NOOU("VS/PlayerImposter", team_2.spawnPoints[num].position + new Vector3(0f, 3f, 0f), team_2.spawnPoints[num].rotation, 0, new Il2CppReferenceArray<Il2CppSystem.Object>(new Il2CppSystem.Object[] { "CustomNPC", infClass.Prefab }));
            }
            else
            {
                rmm.CNLHJAICIBH = PhotonNetwork.NOOU(infClass.Prefab, team_2.spawnPoints[num].position + new Vector3(0f, 3f, 0f), team_2.spawnPoints[num].rotation, 0);
            }
            var pm = rmm.CNLHJAICIBH.AddComponent<PlagueMonster>();
            pm._class = infClass;
            rmm.CNLHJAICIBH.name = PhotonNetwork.player.NickName;
            rmm.CNLHJAICIBH.GetComponent<PhotonView>().RPC("InfectedSoundRPC", PhotonTargets.All, new Il2CppReferenceArray<Il2CppSystem.Object>(new Il2CppSystem.Object[] { infClass.ClassName } ));
            _playerClass = infClass;
            pm.SetUp();
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
        public void RoundTimeLeft()
        {
            CuteLogger.Meow("NoOneWin");
            MelonCoroutines.Start(RestartRound());
            _draw = true;
            PlayMusic(Helper.RandomSound(PlagueAssets.Instance._drawSounds));
        }
        public void DestroyAllRagdolls()
        {
            var s = GameObject.FindObjectsOfType<RagdollMode>();
            if (s != null)
            {
                foreach (RagdollMode ragdoll in s)
                {
                    UnityEngine.Object.Destroy(ragdoll.gameObject);
                }
            }
        }
        public void DestroyAllCrates()
        {
            var s = GameObject.FindObjectsOfType<SupplyCrate>();
            var m = GameObject.FindObjectsOfType<MedKit>();
            var w = GameObject.FindObjectsOfType<WireTrap>();
            var t = GameObject.FindObjectsOfType<MineTrap>();

            if (s != null)
            {
                foreach (SupplyCrate supply in s)
                {
                    supply.GetComponent<PhotonView>().ownerId = PhotonNetwork.player.ID;
                    PhotonNetwork.Destroy(supply.gameObject);
                }
            }
            if (m != null)
            {
                foreach (MedKit medkit in m)
                {
                    medkit.GetComponent<PhotonView>().ownerId = PhotonNetwork.player.ID;
                    PhotonNetwork.Destroy(medkit.gameObject);
                }
            }
            if (w != null)
            {
                foreach (WireTrap wire in w)
                {
                    wire.transform.parent.gameObject.GetComponent<PhotonView>().ownerId = PhotonNetwork.player.ID;
                    PhotonNetwork.Destroy(wire.transform.parent.gameObject);
                }
            }
            if (t != null)
            {
                foreach (MineTrap mine in t)
                {
                    mine.transform.parent.gameObject.GetComponent<PhotonView>().ownerId = PhotonNetwork.player.ID;
                    PhotonNetwork.Destroy(mine.transform.parent.gameObject);
                }
            }
        }
        IEnumerator RestartRound()
        {
            CuteLogger.Meow("RestartRound");
            _restarting = true;
            if (PhotonNetwork.isMasterClient) PhotonNetwork.room.IsOpen = false;
           // CuteLogger.Meow($"changing light to {_d_AmbientLight}");
           // RenderSettings.ambientMode = _d_AmbientMode;
           // RenderSettings.ambientLight = _d_AmbientLight;
          //  RenderSettings.fogColor = _d_FogColor;
            Helper.SurvivalMechanics.EGDOBKKCGFC.volume = 0.4f;
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
            _draw = false;
            _playerClass = null;
            _infectedWin = false;
            _roundStartPlayed = false;
            _countdownPlayed = false;
            _roundEndPlayed = false;
            _restarting = false;
            _totalRounds++;
            _supplyDrop = false;
            DestroyAllRagdolls();
            if (_totalRounds > 0 && PhotonNetwork.isMasterClient)
            {
                Helper.SetRoomProperty("PlagueMode", "Infection");
                PhotonNetwork.room.IsOpen = true;
                PhotonNetwork.room.IsVisible = true;
                DestroyAllCrates();
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
            if (HudHider.Instance != null && HudHider.Instance.hidden) return;
            GUIStyle timerStyle = new GUIStyle(rmm.BIPMFIBNFBC.label)
            {
                fontSize = checked(75 * (Screen.width + Screen.height)) / 6000,
                alignment = TextAnchor.MiddleCenter
            };          
            GUI.color = Color.white;
            if (/*_plagueStarted && */_classSet && _playerClass != null)
            {       
                DrawAbilities();
            }
            if (_countDown > 0f && !_waitingForPlayers && !_restarting)
            {
                //The virus has been set loose...
                //if (Time.time % 2 < 1)
                DrawOutline(new Rect(Screen.width / 2f, Screen.height * 0.010563f + 50f, 100f, 50f), PlagueLocales.virusText, 1, timerStyle);
                DrawOutline(new Rect(Screen.width / 2f, Screen.height * 0.010563f + 100f, 100f, 50f), string.Format(PlagueLocales.roundStartsText +" {0}s", Mathf.CeilToInt(_countDown) % 60), 1, timerStyle);
            }
            else if (_plagueStarted && !_restarting)
                DrawOutline(new Rect(Screen.width / 2f, Screen.height * 0.010563f, 100f, 50f), string.Format("{0:00}:{1:00}", Mathf.CeilToInt(_plagueTimer) / 60, Mathf.CeilToInt(_plagueTimer) % 60), 1, timerStyle);
            if (_restarting && !_waitingForPlayers)
            {
                DrawOutline(new Rect(Screen.width / 2f, Screen.height * 0.010563f, 100f, 50f), PlagueLocales.restartingText, 1, timerStyle);
            }
            if (_showMessage)
            {
                DrawOutline(new Rect(Screen.width / 2f, Screen.height * 0.010563f + 50f, 100f, 50f), _message, 1, timerStyle);
            }
            if (_survivorsWin)
            {
                DrawOutline(new Rect(Screen.width / 2f, Screen.height * 0.010563f + 50f, 100f, 50f), PlagueLocales.survivorsWinText, 1, timerStyle);
            }
            if (_infectedWin)
            {
                DrawOutline(new Rect(Screen.width / 2f, Screen.height * 0.010563f + 50f, 100f, 50f), PlagueLocales.infectedWinText, 1, timerStyle);
            }
            if (_draw)
            {
                DrawOutline(new Rect(Screen.width / 2f, Screen.height * 0.010563f + 50f, 100f, 50f), PlagueLocales.drawText, 1, timerStyle);
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
        public void DrawAbilities()
        {
            GUIStyle infoStyle = new GUIStyle(rmm.BIPMFIBNFBC.label)
            {
                fontSize = checked(75 * (Screen.width + Screen.height)) / 6000,
                alignment = TextAnchor.MiddleRight
            };
            GUI.Label(new Rect(Screen.width - 200f, Screen.height - 200f, 100f, 100f), PlagueLocales.classText + " " + _playerClass.ClassName, infoStyle);
            if (_playerClass.ClassName == "Medic")
            {
                if (MedKitSpawner.Instance != null)
                {
                    var x = Screen.width - 200;
                    var y = Screen.height - 250;

                    if (MedKitSpawner.Instance.ready) GUI.Label(new Rect(x - 120, y, 100f, 100f), PlagueLocales.abilityMedkitText, infoStyle);
                    else GUI.Label(new Rect(x - 120, y, 100f, 100f), $"[{Mathf.FloorToInt(MedKitSpawner.Instance.timer % 60).ToString()}s]", infoStyle);
                    GUI.DrawTexture(new Rect(Screen.width - 200, y, 64, 64), PlagueAssets.Instance._medkitIcon);
                }
            }
            if (_playerClass.ClassName == "Heavy")
            {
                if (WireSpawner.Instance != null)
                {
                    var x = Screen.width - 200;
                    var y = Screen.height - 250;

                    if (WireSpawner.Instance.ready) GUI.Label(new Rect(x - 120, y, 100f, 100f), PlagueLocales.abilityWireText, infoStyle);
                    else GUI.Label(new Rect(x - 120, y, 100f, 100f), $"[{Mathf.FloorToInt(WireSpawner.Instance.timer % 60).ToString()}s]", infoStyle);
                    GUI.DrawTexture(new Rect(Screen.width - 200, y, 64, 64), PlagueAssets.Instance._wireIcon);
                }
            }
            if (_playerClass.ClassName == "Scout")
            {
                if (MineSpawner.Instance != null)
                {
                    var x = Screen.width - 200;
                    var y = Screen.height - 250;

                    if (MineSpawner.Instance.ready) GUI.Label(new Rect(x - 120, y, 100f, 100f), PlagueLocales.abilityMineText, infoStyle);
                    else GUI.Label(new Rect(x - 120, y, 100f, 100f), $"[{Mathf.FloorToInt(MineSpawner.Instance.timer % 60).ToString()}s]", infoStyle);
                    if (MineSpawner.Instance.selectedType == 1) GUI.DrawTexture(new Rect(Screen.width - 200, y, 64, 64), PlagueAssets.Instance._landMineIcon);
                    if (MineSpawner.Instance.selectedType == 0) GUI.DrawTexture(new Rect(Screen.width - 200, y, 64, 64), PlagueAssets.Instance._scanMineIcon);
                    GUI.Label(new Rect(x - 120, y-40, 100f, 100f), PlagueLocales.changeAbilityText, infoStyle);
                }
            }
            if (_playerClass.ClassName == "Phantom")
            {
                if (PhantomInvisibility.Instance != null)
                {
                    var x = Screen.width - 200;
                    var y = Screen.height - 250;

                    if (PhantomInvisibility.Instance.ready) GUI.Label(new Rect(x - 120, y, 100f, 100f), PlagueLocales.abilityInvText, infoStyle);
                    else GUI.Label(new Rect(x - 120, y, 100f, 100f), $"[{Mathf.FloorToInt(PhantomInvisibility.Instance.timer % 60).ToString()}s]", infoStyle);
                    GUI.DrawTexture(new Rect(Screen.width - 200, y, 64, 64), PlagueAssets.Instance._invisibleIcon);
                }
            }
            if (_playerClass.ClassName == "Tank")
            {
                if (InfectedRage.Instance != null)
                {
                    var x = Screen.width - 200;
                    var y = Screen.height - 250;

                    if (InfectedRage.Instance.ready) GUI.Label(new Rect(x - 120, y, 100f, 100f), PlagueLocales.abilityRageText, infoStyle);
                    else GUI.Label(new Rect(x - 120, y, 100f, 100f), $"[{Mathf.FloorToInt(InfectedRage.Instance.timer % 60).ToString()}s]", infoStyle);
                    GUI.DrawTexture(new Rect(Screen.width - 200, y, 64, 64), PlagueAssets.Instance._rageIcon);
                }
            }
            if (_playerClass.ClassName == "Hunter")
            {
                var x = Screen.width - 200;
                var y = Screen.height - 250;

                GUI.Label(new Rect(x - 120, y, 100f, 100f), "", infoStyle);
                GUI.DrawTexture(new Rect(Screen.width - 200, y, 64, 64), PlagueAssets.Instance._lustIcon);
            }
        }
        public void DrawOutline(Rect r, string t, int strength, GUIStyle style)
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
