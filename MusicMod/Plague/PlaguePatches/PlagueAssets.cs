using FunPlusEssentials.CustomContent;
using FunPlusEssentials.Essentials;
using FunPlusEssentials.Other;
using MelonLoader;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.AI;
using UnityEngine;
using System.Collections;
using Harmony;
using Il2CppSystem.Reflection;
using static ShopSystem;
using UnhollowerBaseLib;

namespace FunPlusEssentials
{
    [RegisterTypeInIl2Cpp]
    public class PlagueAssets : MonoBehaviour
    {
        public PlagueAssets(IntPtr ptr) : base(ptr) { }
        public List<AudioClip> _infectedWinSounds, _survivorsWinSounds, _drawSounds, _infectedSounds, _roundStartSounds, _hitSounds, _cursedStart, _survivorStart, _nemesisStart, _swarmStart, _armageddonStart, _landMineSounds;
        public AudioClip _ambience, _countdownSound, _rageSound, _healSound, _supplyDropSound, _supplyPickupSound;
        public AudioClip _hunterSound, _nemesisSound, _invisStart, _invisEnd;
        public GameObject _bullet, _meleeBullet, _blood, _greenSmoke, _medKit, _supplyBox, _wire, _landMine, _explosion;
        public Dictionary<string, GameObject[]> _loadedPrefabs;
        public List<WeaponInfo> _weapons;
        public List<GameObject> _equipmentVest;
        public Texture2D _medkitIcon, _invisibleIcon, _rageIcon, _lustIcon, _wireIcon, _landMineIcon, _scanMineIcon;
        public static bool _inited;
        public static PlagueAssets Instance { get; set; }

        public void Awake()
        {
            // DontDestroyOnLoad(this);
            Instance = this;
            SetUpWeapons();
            MelonCoroutines.Start(LoadAssets());
            DontDestroyOnLoad(gameObject);
        }
        public void SetUpWeapons()
        {
            _weapons = new List<WeaponInfo>
            {
                new WeaponInfo()
                {
                    name = "Skull-1",
                    animationType = WeaponInfo.AnimationType.TwoHandPistol,
                    type = WeaponScript.DOEEBEFKIJI.MACHINE_GUN,
                    fireRate = 0.3f,
                    bulletsPerClip = 7,
                    clips = 28,
                    reloadTime = 2.4f,
                    aimPosX = 0,
                    aimPosY = 0,
                    aimPosZ = 0,
                    recoil = 0.3f,
                    aimRecoil = 0.1f,
                    recoilPower = 1.5f,
                    shakePower = 1.5f,
                    aimFov = 45,
                    bulletLifeTime = 10,
                    bulledSpeed = 500,
                    bulletDamage = 45,
                    bulletForce = 15,
                    singleFire = true,
                },
                new WeaponInfo()
                {
                    name = "SF Gun",
                    animationType = WeaponInfo.AnimationType.TwoHandRifle,
                    type = WeaponScript.DOEEBEFKIJI.MACHINE_GUN,
                    fireRate = 0.09f,
                    bulletsPerClip = 45,
                    clips = 135,
                    reloadTime = 2.2f,
                    aimPosX = 0,
                    aimPosY = 0,
                    aimPosZ = 0,
                    recoil = 0.6f,
                    aimRecoil = 0.1f,
                    recoilPower = 0.6f,
                    shakePower = 0.8f,
                    aimFov = 45,
                    bulletLifeTime = 10,
                    bulledSpeed = 500,
                    bulletDamage = 20,
                    bulletForce = 10,
                    singleFire = false,
                },
                new WeaponInfo()
                {
                    name = "Skull-5",
                    animationType = WeaponInfo.AnimationType.TwoHandRifle,
                    type = WeaponScript.DOEEBEFKIJI.MACHINE_GUN,
                    fireRate = 0.5f,
                    bulletsPerClip = 24,
                    clips = 48,
                    reloadTime = 2.2f,
                    aimPosX = 0.1f,
                    aimPosY = 0,
                    aimPosZ = 0,
                    recoil = 2.2f,
                    aimRecoil = 0.1f,
                    recoilPower = 2.1f,
                    shakePower = 2.2f,
                    aimFov = 45,
                    bulletLifeTime = 10,
                    bulledSpeed = 500,
                    bulletDamage = 45,
                    bulletForce = 30,
                    useScope = true,
                    singleFire = false,
                },
                new WeaponInfo()
                {
                    name = "Balrog-1",
                    animationType = WeaponInfo.AnimationType.TwoHandPistol,
                    type = WeaponScript.DOEEBEFKIJI.MACHINE_GUN,
                    fireRate = 0.1f,
                    bulletsPerClip = 10,
                    clips = 40,
                    reloadTime = 2.1f,
                    aimPosX = 0,
                    aimPosY = 0,
                    aimPosZ = 0,
                    recoil = 0.2f,
                    aimRecoil = 0.1f,
                    recoilPower = 1.1f,
                    shakePower = 1.1f,
                    aimFov = 45,
                    bulletLifeTime = 10,
                    bulledSpeed = 500,
                    bulletDamage = 30,
                    bulletForce = 10,
                    singleFire = true,
                },
                new WeaponInfo()
                {
                    name = "Turbulent-7",
                    animationType = WeaponInfo.AnimationType.TwoHandRifle,
                    type = WeaponScript.DOEEBEFKIJI.MACHINE_GUN,
                    fireRate = 0.1f,
                    bulletsPerClip = 100,
                    clips = 200,
                    reloadTime = 4.1f,
                    aimPosX = 0,
                    aimPosY = 0,
                    aimPosZ = 0,
                    recoil = 0.3f,
                    aimRecoil = 0.1f,
                    recoilPower = 1.5f,
                    shakePower = 1.5f,
                    aimFov = 45,
                    bulletLifeTime = 10,
                    bulledSpeed = 500,
                    bulletDamage = 20,
                    bulletForce = 10,
                },
            };
        }
        public static void SetUpCustomNPC(string npcName, GameObject dummy, NPCType type)
        {
            CuteLogger.Meow("Loading custom NPC: " + npcName);
            //GameObject go = GameObject.Instantiate(Resources.Load("SUR/BossShadow").Cast<GameObject>());
            dummy.transform.Find("Model").gameObject.SetActive(false);
            GameObject newModel = GameObject.Instantiate(PlagueAssets.Instance._loadedPrefabs[npcName][0]);
            newModel.transform.SetParent(dummy.transform);
            dummy.transform.localScale = newModel.transform.localScale;
            newModel.transform.localScale = new Vector3(1f, 1f, 1f);
            newModel.transform.localPosition = new Vector3(0f, 0f, 0f);
            newModel.transform.localEulerAngles = Vector3.zero;
            NPCManager.SetLayerAllChildren(newModel.transform, 8);
            var mc = newModel.AddComponent<MecanimControl>();         
            if (type == NPCType.PlayerMonster)
            {
                var pm = dummy.GetComponent<PlayerMonster>();
                pm.ELPJDJFCCNJ = mc;
                var deadPrefab = PlagueAssets.Instance._loadedPrefabs[npcName][1];
                pm.OGJJNCKKILE = deadPrefab.transform;
                if (pm.OGJJNCKKILE.gameObject.GetComponent<DeadBot>() == null) pm.OGJJNCKKILE.gameObject.AddComponent<DeadBot>().CPBLCBMLFAJ = 15f;
            }
            
            var a = newModel.GetComponent<Animator>();
            if (a != null)
            {
                foreach (AnimationClip clip in newModel.GetComponent<Animator>().runtimeAnimatorController.animationClips)
                {
                    if (clip.name == "Idle") mc.AMMLJEHHILA(clip, clip.name, 1f, clip.isLooping ? WrapMode.Loop : WrapMode.Clamp);
                }
                foreach (AnimationClip clip in newModel.GetComponent<Animator>().runtimeAnimatorController.animationClips)
                {
                    if (clip.name != "Idle") mc.AMMLJEHHILA(clip, clip.name, 1f, clip.isLooping ? WrapMode.Loop : WrapMode.Clamp);
                }
            }
            CuteLogger.Meow("7");
            var comp = dummy.GetComponent<NavMeshAgent>();
            if (comp != null) { comp.enabled = comp.gameObject.GetComponent<PhotonView>().isMine; }
            dummy.AddComponent<CustomNPC>();
            CuteLogger.Meow("Loaded.");
        }

        public IEnumerator InstantiateFPSWeapons(GameObject source)
        {

            yield return new WaitForSeconds(0.05f);
            if (CustomWeapons.CheckForWeapons(source)) { yield break; }
            // customWeapons.Add(new WeaponInfo() { name = "M4A1", clips = 105, fireRate = 0.05f, type = WeaponScript.DOEEBEFKIJI.MACHINE_GUN });
            var weaponRoot = source.transform.FindChild("LookObject/Main Camera/Weapon Camera/WeaponManager").gameObject;
            var weaponsDummies = new GameObject[]
            {
                weaponRoot.transform.FindChild("AKM").gameObject,
                weaponRoot.transform.FindChild("Grenade Launcher").gameObject,
                weaponRoot.transform.FindChild("MCS870").gameObject,
                weaponRoot.transform.FindChild("Knife").gameObject
            };

            foreach (WeaponInfo weapon in _weapons)
            {
                GameObject dummyWeapon = GameObject.Instantiate(weaponsDummies[(int)weapon.type]);
                dummyWeapon.transform.SetParent(weaponRoot.transform);
                dummyWeapon.name = weapon.name;
                GameObject.Destroy(dummyWeapon.transform.GetChild(1).gameObject);
                GameObject bundleWeapon = GameObject.Instantiate(_loadedPrefabs[weapon.name][0]);
                GameObject newModel = bundleWeapon.transform.GetChild(0).gameObject;
                var muzzleFlash = newModel.transform.FindChild("SpecialEffects/MuzzleFlash");
                var pointLight = newModel.transform.FindChild("SpecialEffects/PointLight");

                newModel.transform.SetParent(dummyWeapon.transform);
                dummyWeapon.transform.localPosition = new Vector3(0, 0, 0);
                var wa = newModel.AddComponent<WeaponAnimation>();
                wa.EBJNIFNOGJA = "reload";
                wa.FEJAHOOPHPD = "take-in";
                wa.HPKHKABOFOO = "take-out";
                wa.IBHAFEOOCFC = "fire";
                wa.LFKDOIFADHK = "idle";

                if (dummyWeapon.GetComponent<WeaponSync_Catcher>() == null)
                {
                    dummyWeapon.AddComponent<WeaponSync_Catcher>();
                }
                var ws = dummyWeapon.GetComponent<WeaponScript>();
                ws.JIFANOLAADI = weapon.name;
                ws.FLIAJIAOBHA.aimPosition = new Vector3(weapon.aimPosX, weapon.aimPosY, weapon.aimPosZ);
                ws.FLIAJIAOBHA.playAnimation = weapon.aimAnimation;
                ws.HHAMHPCAHNK = weapon.singleFire;
                if (weapon.aimFov != 0) ws.FLIAJIAOBHA.toFov = weapon.aimFov;
                GameObject bullet = null;
                if (weapon.type == WeaponScript.DOEEBEFKIJI.MACHINE_GUN)
                {
                    ws.FMPLNFBLHKJ.bulletsPerClip = weapon.bulletsPerClip;
                    ws.FMPLNFBLHKJ.clips = weapon.clips;
                    ws.FMPLNFBLHKJ.fireRate = weapon.fireRate;
                    ws.FMPLNFBLHKJ.AimErrorAngle = weapon.aimRecoil;
                    ws.FMPLNFBLHKJ.NoAimErrorAngle = weapon.recoil;
                    ws.FMPLNFBLHKJ.reloadSound = weapon.reloadSound;
                    ws.FMPLNFBLHKJ.reloadTime = weapon.reloadTime;
                    ws.FMPLNFBLHKJ.fireSound = weapon.shotSound;

                    if (muzzleFlash != null)
                    {
                        ws.FMPLNFBLHKJ.muzzleFlash = muzzleFlash.gameObject;
                        GameObject.Destroy(dummyWeapon.transform.GetChild(0).gameObject);
                    }
                    if (pointLight != null) ws.FMPLNFBLHKJ.pointLight = pointLight.GetComponent<Light>();

                    if (weapon.useScope && weapon.scope != null)
                    {
                        var s = dummyWeapon.AddComponent<SniperScope>();
                        List<GameObject> arr = new List<GameObject>();
                        foreach (Renderer r in newModel.GetComponentsInChildren<Renderer>())
                        {
                            arr.Add(r.gameObject);
                        }
                        s.GKHNJCJJDCJ = new Il2CppReferenceArray<GameObject>(arr.ToArray());
                        s.MGEKFJPKEDI = weapon.scope;
                        s.OMDBGECMMOD = ws;
                    }
                    bullet = _loadedPrefabs[weapon.name][2];
                    if (bullet != null) ws.FMPLNFBLHKJ.bullet = CustomWeapons.SetUpCustomProjectile(ws.FMPLNFBLHKJ.bullet, bullet.transform, weapon);
                }
                else if (weapon.type == WeaponScript.DOEEBEFKIJI.SHOTGUN)
                {
                    ws.MMIKHOPCECE.bulletsPerClip = weapon.bulletsPerClip;
                    ws.MMIKHOPCECE.clips = weapon.clips;
                    ws.MMIKHOPCECE.fireRate = weapon.fireRate;
                    ws.MMIKHOPCECE.errorAngle = weapon.aimRecoil;
                    ws.MMIKHOPCECE.reloadSound = weapon.reloadSound;
                    ws.MMIKHOPCECE.reloadTime = weapon.reloadTime;
                    ws.MMIKHOPCECE.fireSound = weapon.shotSound;

                    if (muzzleFlash != null) ws.FMPLNFBLHKJ.muzzleFlash = muzzleFlash.GetChild(0).gameObject;
                    if (pointLight != null) ws.FMPLNFBLHKJ.pointLight = pointLight.GetComponent<Light>();
                    bullet = _loadedPrefabs[weapon.name][2];
                    if (bullet != null) ws.MMIKHOPCECE.bullet = CustomWeapons.SetUpCustomProjectile(ws.MMIKHOPCECE.bullet, bullet.transform, weapon);
                }

                else if (weapon.type == WeaponScript.DOEEBEFKIJI.GRENADE_LAUNCHER)
                {
                    ws.MJIKHNADGAG.ammoCount = weapon.clips;
                    ws.MJIKHNADGAG.initialSpeed = weapon.projectileSpeed;
                    ws.MJIKHNADGAG.reloadTime = weapon.reloadTime;
                    ws.MJIKHNADGAG.shotDelay = weapon.shotDelay;
                    ws.MJIKHNADGAG.reloadSound = weapon.reloadSound;
                    ws.MJIKHNADGAG.fireSound = weapon.shotSound;
                }
                else if (weapon.type == WeaponScript.DOEEBEFKIJI.KNIFE)
                {
                    ws.EKODJEHEFDE.delayTime = weapon.hitDelay;
                    ws.EKODJEHEFDE.fireRate = weapon.fireRate;
                    ws.EKODJEHEFDE.fireSound = weapon.shotSound;
                    ws.HHAMHPCAHNK = true;
                    bullet = _loadedPrefabs[weapon.name][2];
                    if (bullet != null) ws.EKODJEHEFDE.bullet = CustomWeapons.SetUpCustomProjectile(ws.EKODJEHEFDE.bullet, bullet.transform, weapon);
                }
                ws.APFMIOFJJNC.recoilPower = weapon.recoilPower;
                ws.APFMIOFJJNC.shakeAmount = weapon.shakePower;
                //  Transform fp = bundleWeapon.transform.FindChild("FirePoint");
                // if (fp != null)
                //  {
                //  ws.JDDPMKHOGEN = fp;
                // }
                ws.MGFCKCEIEKM = weapon.icon;
                ws.HIHFCMAJBJK = weapon.crosshair;
                var ca = source.transform.FindChild("MainAnimData").gameObject.GetComponent<CharacterAnimation>();
                if (ca != null)
                {
                    if (weapon.animationType == WeaponInfo.AnimationType.TwoHandRifle)
                    {
                        ca.DOODHOGLOJO.Add(ws);
                    }
                    if (weapon.animationType == WeaponInfo.AnimationType.TwoHandPistol)
                    {
                        ca.NKMALFJABNF.Add(ws);
                    }
                    if (weapon.animationType == WeaponInfo.AnimationType.OneHand)
                    {
                        ca.JAEHDJFGHEB.Add(ws);
                    }
                }
                foreach (var t in dummyWeapon.GetComponentsInChildren<Renderer>())
                {
                    if (t.material.name.ToUpper() == "FUR (INSTANCE)")
                    {
                        var a = t.gameObject.AddComponent<AntennaColor>();
                        a.DCMJNPMPBIK = source.transform.FindChild("PLAYER_MODEL/model/leftarmmesh").gameObject.GetComponent<SkinnedMeshRenderer>();
                    }
                }
                GameObject.Destroy(bundleWeapon);
                //ws.HANLPABBHKN = 
                MelonCoroutines.Start(InstantiateTPSWeapons(source.GetComponent<PlayerNetworkController>()));
                MelonCoroutines.Start(zaebalo(wa, ws));
            }
        }
        public IEnumerator InstantiateTPSWeapons(PlayerNetworkController source)
        {
            yield return new WaitForSeconds(0.05f);
            Transform p = source.HDCJMMEMDEJ;
            var weaponRoot = source.transform.FindChild("LookObject/Main Camera/Weapon Camera/WeaponManager").gameObject;
            if (p.childCount > 30) yield break;

            foreach (WeaponInfo weapon in _weapons)
            {
                GameObject bundleWeapon = GameObject.Instantiate(_loadedPrefabs[weapon.name][1]);
                bundleWeapon.transform.SetParent(p, false);
                bundleWeapon.name = weapon.name;
                var s = weaponRoot.transform.FindChild(weapon.name).gameObject.GetComponent<WeaponScript>();

                var ws = bundleWeapon.AddComponent<WeaponSync>();
                ws.LIFHNEDIKHM = s;
                ws.JDDPMKHOGEN = source.transform.FindChild("PLAYER_MODEL/Bip001/Bip001 Pelvis/Bip001 Spine/Bip001 Spine1/Bip001 Spine2/Bip001 Spine3/Bip001 Neck/Bip001 Head/FirePoint(ThirdPerson)");
                var muzzle = bundleWeapon.transform.FindChild("MuzzleFlash");
                if (muzzle != null)
                {
                    ws.NDGIAAEEGFO = muzzle.GetComponent<MeshRenderer>();
                }
                var mixer = FindObjectOfType<AudioMixerManager>().EEJANKGKOBO;
                var sfx = mixer.FindMatchingGroups("SFX")[0];
                ws.GetComponent<AudioSource>().outputAudioMixerGroup = sfx;
                bundleWeapon.SetActive(false);
            }
        }

        public static IEnumerator zaebalo(WeaponAnimation aaaa, WeaponScript weaponScript)
        {
            yield return new WaitForSeconds(0.1f);
            weaponScript.HANLPABBHKN = aaaa.GetComponent<WeaponAnimation>();
            weaponScript.gameObject.SetActive(false);
        }
        public IEnumerator LoadAssets()
        {
            _loadedPrefabs = new Dictionary<string, GameObject[]>();
            _infectedWinSounds = new List<AudioClip>();
            _survivorsWinSounds = new List<AudioClip>();
            _infectedSounds = new List<AudioClip>();
            _roundStartSounds = new List<AudioClip>();
            _hitSounds = new List<AudioClip>();
            _swarmStart = new List<AudioClip>();
            _armageddonStart = new List<AudioClip>();
            _cursedStart = new List<AudioClip>();
            _survivorStart = new List<AudioClip>();
            _nemesisStart = new List<AudioClip>();
            _drawSounds = new List<AudioClip>();
            _landMineSounds = new List<AudioClip>();
            _equipmentVest = new List<GameObject>();
            yield return MelonCoroutines.Start(LoadWeaponsAssets());
            var assetBundleCreateRequest = Il2CppAssetBundleManager.LoadFromFile(Config.mainPath + @"\Plague\plague assets");
            yield return assetBundleCreateRequest;
            for (int i = 1; i < 6; i++)
            {
                _roundStartSounds.Add(assetBundleCreateRequest.Load<AudioClip>($"roundStart{i}"));
            }
            for (int i = 1; i < 6; i++)
            {
                _infectedWinSounds.Add(assetBundleCreateRequest.Load<AudioClip>($"infectedWin{i}"));
                _survivorsWinSounds.Add(assetBundleCreateRequest.Load<AudioClip>($"survivorsWin{i}"));
            }
            for (int i = 1; i < 4; i++)
            {
                _infectedSounds.Add(assetBundleCreateRequest.Load<AudioClip>($"infected{i}"));
                _hitSounds.Add(assetBundleCreateRequest.Load<AudioClip>($"hit{i}"));
                _landMineSounds.Add(assetBundleCreateRequest.Load<AudioClip>($"mineSound{i}"));
            }
            for (int i = 1; i < 3; i++)
            {
                _survivorStart.Add(assetBundleCreateRequest.Load<AudioClip>($"survivorStart{i}"));
                _nemesisStart.Add(assetBundleCreateRequest.Load<AudioClip>($"nemesisStart{i}"));
                _armageddonStart.Add(assetBundleCreateRequest.Load<AudioClip>($"armageddonStart{i}"));
                _swarmStart.Add(assetBundleCreateRequest.Load<AudioClip>($"swarmStart{i}"));
                _drawSounds.Add(assetBundleCreateRequest.Load<AudioClip>($"draw{i}"));
            }
            for (int i = 1; i < 3; i++)
            {
                _equipmentVest.Add(assetBundleCreateRequest.Load<GameObject>($"Vest{i}"));
            }
            _cursedStart.Add(assetBundleCreateRequest.Load<AudioClip>($"cursedStart"));
            _supplyDropSound = assetBundleCreateRequest.Load<AudioClip>($"supplyboxDrop");
            _supplyPickupSound = assetBundleCreateRequest.Load<AudioClip>($"supplyboxPickup");
            _hunterSound = assetBundleCreateRequest.Load<AudioClip>($"infectedHunter");
            _nemesisSound = assetBundleCreateRequest.Load<AudioClip>($"infectedNemesis");
            _ambience = assetBundleCreateRequest.Load<AudioClip>($"ambience1");
            _countdownSound = assetBundleCreateRequest.Load<AudioClip>($"countdown1");
            _rageSound = assetBundleCreateRequest.Load<AudioClip>($"rage1");
            _healSound = assetBundleCreateRequest.Load<AudioClip>($"infectedHeal1");
            _blood = assetBundleCreateRequest.Load<GameObject>("Blood");
            _greenSmoke = assetBundleCreateRequest.Load<GameObject>("InfectedSmoke");
            _medKit = assetBundleCreateRequest.Load<GameObject>("Medkit");
            _invisStart = assetBundleCreateRequest.Load<AudioClip>("invisibleStart");
            _invisEnd = assetBundleCreateRequest.Load<AudioClip>("invisibleEnd");
            _supplyBox = assetBundleCreateRequest.Load<GameObject>("SupplyCrate");
            _wire = assetBundleCreateRequest.Load<GameObject>("Wire");
            _landMine = assetBundleCreateRequest.Load<GameObject>("LandMine");
            _explosion = assetBundleCreateRequest.Load<GameObject>("Explosion");
            _medkitIcon = assetBundleCreateRequest.Load<Texture2D>("medkitIcon");
            _invisibleIcon = assetBundleCreateRequest.Load<Texture2D>("invisibleIcon");
            _rageIcon = assetBundleCreateRequest.Load<Texture2D>("rageIcon");
            _lustIcon = assetBundleCreateRequest.Load<Texture2D>("lustIcon");
            _wireIcon = assetBundleCreateRequest.Load<Texture2D>("wireIcon");
            _landMineIcon = assetBundleCreateRequest.Load<Texture2D>("landMineIcon");
            _scanMineIcon = assetBundleCreateRequest.Load<Texture2D>("scanMineIcon");
            //_skybox = assetBundleCreateRequest.Load<Material>($"skybox");
            assetBundleCreateRequest.Unload(false);
            assetBundleCreateRequest = Il2CppAssetBundleManager.LoadFromFile(Config.mainPath + @"\Plague\hunter");
            yield return assetBundleCreateRequest;
            _loadedPrefabs.Add("Hunter", new GameObject[] { assetBundleCreateRequest.Load<GameObject>($"MutantTubby"), assetBundleCreateRequest.Load<GameObject>($"MutantTubby(Dead)") });
            assetBundleCreateRequest.Unload(false);
            assetBundleCreateRequest = Il2CppAssetBundleManager.LoadFromFile(Config.mainPath + @"\Plague\phantom");
            yield return assetBundleCreateRequest;
            _loadedPrefabs.Add("Phantom", new GameObject[] { assetBundleCreateRequest.Load<GameObject>($"Phantom"), assetBundleCreateRequest.Load<GameObject>($"Phantom(Dead)") });
            assetBundleCreateRequest.Unload(false);
            assetBundleCreateRequest = Il2CppAssetBundleManager.LoadFromFile(Config.mainPath + @"\Plague\phobos");
            yield return assetBundleCreateRequest;
            _loadedPrefabs.Add("Phobos", new GameObject[] { assetBundleCreateRequest.Load<GameObject>($"Phobos"), assetBundleCreateRequest.Load<GameObject>($"Phobos(Dead)") });
            assetBundleCreateRequest.Unload(false);
            assetBundleCreateRequest = Il2CppAssetBundleManager.LoadFromFile(Config.mainPath + @"\Plague\tank");
            yield return assetBundleCreateRequest;
            _loadedPrefabs.Add("Tank", new GameObject[] { assetBundleCreateRequest.Load<GameObject>($"Tank"), assetBundleCreateRequest.Load<GameObject>($"Tank(Dead)") });
            assetBundleCreateRequest.Unload(false);
            foreach (GameObject[] obj in _loadedPrefabs.Values)
            {
                foreach (GameObject obj2 in obj)
                {
                    obj2.hideFlags = HideFlags.DontUnloadUnusedAsset;
                }
            }
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
            foreach (AudioClip clip in _hitSounds)
            {
                clip.hideFlags = HideFlags.DontUnloadUnusedAsset;
            }
            foreach (AudioClip clip in _swarmStart)
            {
                clip.hideFlags = HideFlags.DontUnloadUnusedAsset;
            }
            foreach (AudioClip clip in _survivorStart)
            {
                clip.hideFlags = HideFlags.DontUnloadUnusedAsset;
            }
            foreach (AudioClip clip in _nemesisStart)
            {
                clip.hideFlags = HideFlags.DontUnloadUnusedAsset;
            }
            foreach (AudioClip clip in _armageddonStart)
            {
                clip.hideFlags = HideFlags.DontUnloadUnusedAsset;
            }
            foreach (AudioClip clip in _cursedStart)
            {
                clip.hideFlags = HideFlags.DontUnloadUnusedAsset;
            }
            foreach (AudioClip clip in _drawSounds)
            {
                clip.hideFlags = HideFlags.DontUnloadUnusedAsset;
            }
            foreach (AudioClip clip in _landMineSounds)
            {
                clip.hideFlags = HideFlags.DontUnloadUnusedAsset;
            }
            foreach (GameObject obj in _equipmentVest)
            {
                obj.hideFlags = HideFlags.DontUnloadUnusedAsset;
            }
            _hunterSound.hideFlags = HideFlags.DontUnloadUnusedAsset;
            _invisStart.hideFlags = HideFlags.DontUnloadUnusedAsset;
            _invisEnd.hideFlags = HideFlags.DontUnloadUnusedAsset;
            _supplyDropSound.hideFlags= HideFlags.DontUnloadUnusedAsset;
            _supplyPickupSound.hideFlags=HideFlags.DontUnloadUnusedAsset;
            _nemesisSound.hideFlags = HideFlags.DontUnloadUnusedAsset ;
            _healSound.hideFlags = HideFlags.DontUnloadUnusedAsset;
            _countdownSound.hideFlags = HideFlags.DontUnloadUnusedAsset;
            _rageSound.hideFlags = HideFlags.DontUnloadUnusedAsset;
            _blood.hideFlags = HideFlags.DontUnloadUnusedAsset;
            _greenSmoke.hideFlags = HideFlags.DontUnloadUnusedAsset;
            _medKit.hideFlags = HideFlags.DontUnloadUnusedAsset;
            _supplyBox.hideFlags = HideFlags.DontUnloadUnusedAsset;
            _wire.hideFlags = HideFlags.DontUnloadUnusedAsset;
            _landMine.hideFlags = HideFlags.DontUnloadUnusedAsset;
            _explosion.hideFlags = HideFlags.DontUnloadUnusedAsset;
            _wireIcon.hideFlags = HideFlags.DontUnloadUnusedAsset;
            _medkitIcon.hideFlags = HideFlags.DontUnloadUnusedAsset;
            _invisibleIcon.hideFlags= HideFlags.DontUnloadUnusedAsset;
            _rageIcon.hideFlags = HideFlags.DontUnloadUnusedAsset;
            _lustIcon.hideFlags = HideFlags.DontUnloadUnusedAsset;
            _landMineIcon.hideFlags = HideFlags.DontUnloadUnusedAsset;
            _scanMineIcon.hideFlags = HideFlags.DontUnloadUnusedAsset;
            SetUpShaders();
            PlagueAssets._inited = true;
            foreach (GameObject[] obj in _loadedPrefabs.Values)
            {
                foreach (GameObject obj2 in obj)
                {
                    if (obj2 == null) { PlagueAssets._inited = false; }
                }
            }
            if (_loadedPrefabs.Count < 8) { PlagueAssets._inited = false; }
        }
        public IEnumerator LoadWeaponsAssets()
        {
            foreach (var weapon in _weapons)
            {
                var assetBundleCreateRequest = Il2CppAssetBundleManager.LoadFromFile(Config.mainPath + @"\Plague\" + weapon.name.ToLower());
                yield return assetBundleCreateRequest;
                _loadedPrefabs.Add(weapon.name, new GameObject[]
                {
                                assetBundleCreateRequest.Load<GameObject>(weapon.name),
                                assetBundleCreateRequest.Load<GameObject>(weapon.name + " (TPS)"),
                                assetBundleCreateRequest.Load<GameObject>(weapon.name + " (Projectile)"),
                });
                weapon.shotSound = assetBundleCreateRequest.Load<AudioClip>("fire");
                weapon.shotSound.hideFlags = HideFlags.DontUnloadUnusedAsset;
                weapon.reloadSound = assetBundleCreateRequest.Load<AudioClip>("reload");
                weapon.reloadSound.hideFlags = HideFlags.DontUnloadUnusedAsset;
                weapon.icon = assetBundleCreateRequest.Load<Texture2D>("icon");
                weapon.icon.hideFlags = HideFlags.DontUnloadUnusedAsset;
                weapon.crosshair = assetBundleCreateRequest.Load<Texture2D>("crosshair");
                weapon.crosshair.hideFlags = HideFlags.DontUnloadUnusedAsset;
                if (weapon.useScope)
                {
                    weapon.scope = assetBundleCreateRequest.Load<Texture2D>("scope");
                    weapon.scope.hideFlags = HideFlags.DontUnloadUnusedAsset;
                }
                assetBundleCreateRequest.Unload(false);
            }
        }

        public void SetUpShaders()
        {
            var shader = Shader.Find("Standard");
            foreach (var s in _medKit.GetComponentsInChildren<Renderer>())
            {
                s.material.shader = shader;
            }
            foreach (var s in _supplyBox.GetComponentsInChildren<Renderer>())
            {
                s.material.shader = shader;
            }
            foreach (var s in _wire.GetComponentsInChildren<Renderer>())
            {
                s.material.shader = shader;
            }
            foreach (var s in _blood.GetComponentsInChildren<Renderer>(true))
            {
                s.material.shader = Shader.Find(s.material.shader.name);
            }
            foreach (var p in _loadedPrefabs.Values)
            {
                foreach (var o in p)
                {
                    foreach (var s in o.GetComponentsInChildren<Renderer>())
                    {
                        s.material.shader = Shader.Find(s.material.shader.name);
                    }
                }
            }
            _landMine.GetComponent<Renderer>().material.shader = shader;
            foreach (var p in _equipmentVest)
            {
                foreach (var s in p.GetComponentsInChildren<Renderer>())
                {
                    s.material.shader = shader;
                }
            }
        }
    }
}
