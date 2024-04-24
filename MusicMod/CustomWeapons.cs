using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using FunPlusEssentials;
using FunPlusEssentials.Essentials;
using System.IO;
using MelonLoader;
using FunPlusEssentials.Other;
using MelonLoader.TinyJSON;
using Harmony;
using System.Collections;
using System.CodeDom;
using static MelonLoader.MelonLogger;
using UnityEngine.AI;
using UnhollowerBaseLib;
using static FunPlusEssentials.CustomContent.FunNPCInfo;
using System.Reflection;
using FunPlusEssentials.CustomContent;
using Il2CppSystem.Reflection;
using System.Security.AccessControl;
using static Il2CppSystem.Collections.Hashtable;
using static ShopSystem;

namespace FunPlusEssentials.CustomContent
{
    public static class CustomWeapons
    {
        public static string directory = Config.mainPath + @"\CustomWeapons";
        public static List<WeaponInfo> customWeapons;
        public static Dictionary<string, GameObject[]> loadedBundles = new Dictionary<string, GameObject[]>();
        public static Dictionary<string, GameObject> loadedProjectiles = new Dictionary<string, GameObject>();
        public static GameObject weaponRoot;
        public static GameObject[] weaponsDummies;

        public static bool CheckForWeapons(GameObject root)
        {
            if (root.transform.FindChild("LookObject/Main Camera/Weapon Camera/WeaponManager").childCount > 24) return true;
            return false;
        }

        public static IEnumerator LoadBundles()
        {
            foreach (WeaponInfo weapon in customWeapons)
            {
                if (!loadedBundles.ContainsKey(weapon.name))
                {
                    var assetBundleCreateRequest = Il2CppAssetBundleManager.LoadFromFile(weapon.path + @"\bundle");
                    yield return assetBundleCreateRequest;
                    loadedBundles.Add(weapon.name, new GameObject[]
                    {
                                assetBundleCreateRequest.Load<GameObject>(weapon.name),
                                assetBundleCreateRequest.Load<GameObject>(weapon.name + " (TPS)"),
                                assetBundleCreateRequest.Load<GameObject>(weapon.name + " (Projectile)"),
                    });
                    if (!loadedProjectiles.TryGetValue(weapon.name, out var proj))
                    {
                        var b = assetBundleCreateRequest.Load<GameObject>(weapon.name + " (Projectile)");
                        if (b != null)
                        {
                            b.AddComponent<Bullet>();
                            b.hideFlags = HideFlags.HideAndDontSave;
                        }
                        loadedProjectiles[weapon.name] = b;
                    }
                    weapon.shotSound = assetBundleCreateRequest.Load<AudioClip>("fire");
                    weapon.reloadSound = assetBundleCreateRequest.Load<AudioClip>("reload");
                    assetBundleCreateRequest.Unload(false);
                }
                else if (loadedBundles.TryGetValue(weapon.name, out var bundle))
                {
                    if (bundle == null)
                    {
                        loadedBundles.Remove(weapon.name);
                        var assetBundleCreateRequest = Il2CppAssetBundleManager.LoadFromFile(weapon.path + @"\bundle");
                        yield return assetBundleCreateRequest;
                        loadedBundles.Add(weapon.name, new GameObject[]
                        {
                                assetBundleCreateRequest.Load<GameObject>(weapon.name),
                                assetBundleCreateRequest.Load<GameObject>(weapon.name + " (TPS)"),
                                
                        });
                        if (!loadedProjectiles.TryGetValue(weapon.name, out var proj))
                        {
                            var b = assetBundleCreateRequest.Load<GameObject>(weapon.name + " (Projectile)");
                            if (b != null)
                            {
                                b.AddComponent<Bullet>();
                                b.hideFlags = HideFlags.HideAndDontSave;
                            }
                            loadedProjectiles[weapon.name] = b;
                        }
                        weapon.shotSound = assetBundleCreateRequest.Load<AudioClip>("fire");
                        weapon.reloadSound = assetBundleCreateRequest.Load<AudioClip>("reload");
                        assetBundleCreateRequest.Unload(false);
                    }
                }

            }
            MelonCoroutines.Start(CustomWeapons.InstantiateFPSWeapons(GameObject.FindObjectOfType<LadderPlayer>().gameObject));
        }
        public static void SpawnWeapons()
        {
            //MelonCoroutines.Start(InstantiateFPSWeapons());
        }
        public static WeaponInfo CheckWeaponInfos(string weaponName)
        {
            foreach (WeaponInfo weapon in customWeapons)
            {
                if (weapon.name == weaponName) { return weapon; }
            }
            return null;
        }
        public static IEnumerator InstantiateFPSWeapons(GameObject source)
        {

            yield return new WaitForSeconds(0.05f);
            if (CustomWeapons.CheckForWeapons(source)) { yield break; }
            yield return MelonCoroutines.Start(LoadWeaponsIcons());
            // customWeapons.Add(new WeaponInfo() { name = "M4A1", clips = 105, fireRate = 0.05f, type = WeaponScript.DOEEBEFKIJI.MACHINE_GUN });
            weaponRoot = source.transform.FindChild("LookObject/Main Camera/Weapon Camera/WeaponManager").gameObject;
            weaponsDummies = new GameObject[]
            {
                weaponRoot.transform.FindChild("AKM").gameObject,
                weaponRoot.transform.FindChild("Grenade Launcher").gameObject,
                weaponRoot.transform.FindChild("MCS870").gameObject,
                weaponRoot.transform.FindChild("Knife").gameObject
            };

            foreach (WeaponInfo weapon in customWeapons)
            {
                GameObject dummyWeapon = GameObject.Instantiate(weaponsDummies[(int)weapon.type]);
                dummyWeapon.transform.SetParent(weaponRoot.transform);
                dummyWeapon.name = weapon.name;
                GameObject.Destroy(dummyWeapon.transform.GetChild(1).gameObject);
                GameObject bundleWeapon = GameObject.Instantiate(loadedBundles[weapon.name][0]);
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
                    bullet = loadedProjectiles[weapon.name];
                    if (bullet != null) ws.FMPLNFBLHKJ.bullet = SetUpCustomProjectile(ws.FMPLNFBLHKJ.bullet, bullet.transform, weapon);
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
                    bullet = loadedProjectiles[weapon.name];
                    if (bullet != null) ws.MMIKHOPCECE.bullet = SetUpCustomProjectile(ws.MMIKHOPCECE.bullet, bullet.transform, weapon);
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
                    bullet = loadedProjectiles[weapon.name];
                    if (bullet != null) ws.EKODJEHEFDE.bullet = SetUpCustomProjectile(ws.EKODJEHEFDE.bullet, bullet.transform, weapon);
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
                MelonCoroutines.Start(CustomWeapons.InstantiateTPSWeapons(source.GetComponent<PlayerNetworkController>()));
                MelonCoroutines.Start(zaebalo(wa, ws));
            }
        }
        private static Transform SetUpCustomProjectile(Transform dummy, Transform projectileObj, WeaponInfo weapon)
        {
            if (projectileObj != null && weapon != null)
            {
                var b = dummy.GetComponent<Bullet>();
                var bullet = projectileObj.gameObject.GetComponent<Bullet>();
                foreach (GameObject g in b.FABFPAOEEHE)
                {
                    bullet.FABFPAOEEHE.Add(g);
                }
                if (weapon.bulletDamage != 0) bullet.CJKBHAHOLMJ = weapon.bulletDamage;
                if (weapon.bulletForce != 0) bullet.HHBHJIDDGIL = weapon.bulletForce;
                if (weapon.bulledSpeed != 0) bullet.DHHNMAIFIFB = weapon.bulledSpeed;
                if (weapon.bulletLifeTime != 0) bullet.LNEDFJIMHDL = weapon.bulletLifeTime;
                return projectileObj;
            }
            else
            {
                return dummy;
            }
        }
        public static IEnumerator InstantiateTPSWeapons(PlayerNetworkController source)
        {
            yield return new WaitForSeconds(0.05f);
            Transform p = source.HDCJMMEMDEJ;
            weaponRoot = source.transform.FindChild("LookObject/Main Camera/Weapon Camera/WeaponManager").gameObject;
            if (p.childCount > 30) yield break;

            foreach (WeaponInfo weapon in customWeapons)
            {
                GameObject bundleWeapon = GameObject.Instantiate(loadedBundles[weapon.name][1]);
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
                bundleWeapon.SetActive(false);
            }
        }

        public static IEnumerator zaebalo(WeaponAnimation aaaa, WeaponScript weaponScript)
        {
            yield return new WaitForSeconds(0.1f);
            weaponScript.HANLPABBHKN = aaaa.GetComponent<WeaponAnimation>();
            weaponScript.gameObject.SetActive(false);
        }

        public static void Init()
        {
            Directory.CreateDirectory(directory);
            customWeapons = new List<WeaponInfo>();
            var weapons = Helper.GetAllDirectories(directory);
            /*var weapon4 = new WeaponInfo()
            {
                name = "M79",
                clips = 5,
                reloadTime = 1.5f,
                type = WeaponScript.DOEEBEFKIJI.GRENADE_LAUNCHER,
                animationType = WeaponInfo.AnimationType.TwoHandRifle,
                aimPosition = new Vector3(0, -0.22f, 0.25f),
                recoilPower = 1.35f,
                shakePower = 1f,
                projectileSpeed = 5,
                waitBeforeReload = 0.5f,
                shotDelay = 0
            };*/
            for (int i = 0; i < weapons.Count; i++)
            {
                string bundlePath = weapons[i].FullName + @"\bundle";
                if (!File.Exists(bundlePath))
                {
                    return;
                }
                var json = File.ReadAllText(weapons[i].FullName + @"\weapon.json");
                var weapon = JSON.Load(json).Make<WeaponInfo>();
                weapon.path = weapons[i].FullName;
                customWeapons.Add(weapon);
            }
        }
        public static void AddWeaponsToCatagory()
        {
            foreach (WeaponInfo weapon in customWeapons)
            {
                Helper.Console.PDKPIOHFCCK[5].options.Add(new Volume.option()
                {
                    optionName = weapon.name
                });
                Helper.Console.PDKPIOHFCCK[4].options.Add(new Volume.option()
                {
                    optionName = weapon.name
                });
            }
        }
        public static IEnumerator LoadWeaponsIcons()
        {
            foreach (WeaponInfo weapon in customWeapons)
            {
                string iconPath = weapon.path + @"\icon.png";
                if (File.Exists(iconPath))
                {
                    WWW www = new WWW(@"file://" + iconPath);
                    yield return www;
                    weapon.icon = www.texture;
                }
                string cPath = weapon.path + @"\crosshair.png";
                if (File.Exists(iconPath))
                {
                    WWW www = new WWW(@"file://" + cPath);
                    yield return www;
                    weapon.crosshair = www.texture;
                }
                string scopePath = weapon.path + @"\scope.png";
                if (File.Exists(scopePath))
                {
                    WWW www = new WWW(@"file://" + scopePath);
                    yield return www;
                    weapon.scope = www.texture;
                }
            }
        }
    }

    

    public class WeaponInfo
    {
        public string name;
        public string path;

        public WeaponScript.DOEEBEFKIJI type;
        public AnimationType animationType;

        //SHOTGUN
        public int fractions;

        //GRENADE LAUNCHER
        public int projectileSpeed;
        public float waitBeforeReload;
        public float shotDelay;

        //MELEE
        public float hitDelay;

        public float fireRate;
        public int bulletsPerClip;
        public int clips;
        public float reloadTime;
        public bool singleFire;
        public float aimPosX, aimPosY, aimPosZ;
        public float recoil;
        public float aimRecoil;
        public bool useScope;
        public int aimFov;
        public bool aimAnimation;

        public int shopPrice;
        public bool shopAvailable;
        public string shopInfo;

        public float bulletLifeTime = 0;
        public int bulletDamage = 0;
        public int bulledSpeed = 0;
        public int bulletForce = 0;

        public float recoilPower;
        public float shakePower;

        public AudioClip shotSound;
        public AudioClip reloadSound;

        public Texture2D icon;
        public Texture2D scope;
        public Texture2D crosshair;
        //blabla

        public enum AnimationType
        {
            OneHand,
            TwoHandRifle,
            TwoHandPistol
        }
    }
}
