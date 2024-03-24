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

namespace FunPlusEssentials.CustomContent
{
    public static class CustomWeapons
    {
        public static string directory = Config.mainPath + @"\CustomWeapons";
        public static List<WeaponInfo> customWeapons;
        public static Dictionary<string, GameObject[]> loadedBundles = new Dictionary<string, GameObject[]>();
        public static GameObject weaponRoot;
        public static GameObject[] weaponsDummies;

        public static bool CheckForWeapons(GameObject root)
        {
            if (root.transform.FindChild("LookObject/Main Camera/Weapon Camera/WeaponManager").childCount > 24) return true;
            return false;
        }

        public static IEnumerator LoadBundles()
        {
            loadedBundles.Clear();
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
                    });
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
                        weapon.shotSound = assetBundleCreateRequest.Load<AudioClip>("fire");
                        weapon.reloadSound = assetBundleCreateRequest.Load<AudioClip>("reload");
                        assetBundleCreateRequest.Unload(false);
                    }
                }
            }
        }

        public static IEnumerator InstantiateFPSWeapons(GameObject source)
        {
            yield return new WaitForSeconds(0.05f);
            yield return MelonCoroutines.Start(LoadWeaponsIcons());

            // customWeapons.Add(new WeaponInfo() { name = "M4A1", clips = 105, fireRate = 0.05f, type = WeaponScript.DOEEBEFKIJI.MACHINE_GUN });
            weaponRoot = source.transform.FindChild("LookObject/Main Camera/Weapon Camera/WeaponManager").gameObject;
            weaponsDummies = new GameObject[]
            {
                weaponRoot.transform.FindChild("AKM").gameObject,
                weaponRoot.transform.FindChild("RPG").gameObject,
                weaponRoot.transform.FindChild("MCS870").gameObject
            };
            CuteLogger.Meow("1");

            foreach (WeaponInfo weapon in customWeapons)
            {            

                CuteLogger.Meow("2");
                GameObject dummyWeapon = GameObject.Instantiate(weaponsDummies[(int)weapon.type]);
                CuteLogger.Meow("3");
                dummyWeapon.transform.SetParent(weaponRoot.transform);
                dummyWeapon.name = weapon.name;
                GameObject.Destroy(dummyWeapon.transform.GetChild(1).gameObject);
                GameObject bundleWeapon = GameObject.Instantiate(loadedBundles[weapon.name][0]);
                GameObject newModel = bundleWeapon.transform.GetChild(0).gameObject;
                CuteLogger.Meow("4");
                newModel.transform.SetParent(dummyWeapon.transform);
                dummyWeapon.transform.localPosition = new Vector3(0, 0, 0);
                var wa = newModel.AddComponent<WeaponAnimation>();
                CuteLogger.Meow("5");
                wa.EBJNIFNOGJA = "reload";
                wa.FEJAHOOPHPD = "select";
                wa.HPKHKABOFOO = "put-away";
                wa.IBHAFEOOCFC = "fire";
                wa.LFKDOIFADHK = "idle";

                if (dummyWeapon.GetComponent<WeaponSync_Catcher>() == null)
                {
                    dummyWeapon.AddComponent<WeaponSync_Catcher>();
                }
                var ws = dummyWeapon.GetComponent<WeaponScript>();
                ws.JIFANOLAADI = weapon.name;
                ws.FLIAJIAOBHA.aimPosition = weapon.aimPosition;

                if (weapon.type == WeaponScript.DOEEBEFKIJI.MACHINE_GUN)
                {
                    ws.FMPLNFBLHKJ.bulletsPerClip = weapon.bulletsPerClip;
                    ws.FMPLNFBLHKJ.clips = weapon.clips;
                    ws.FMPLNFBLHKJ.fireRate = weapon.fireRate;
                    ws.FMPLNFBLHKJ.AimErrorAngle = weapon.aimRecoil;
                    ws.FMPLNFBLHKJ.NoAimErrorAngle = weapon.recoil;
                    ws.FMPLNFBLHKJ.reloadSound = weapon.reloadSound;
                    ws.FMPLNFBLHKJ.fireSound = weapon.shotSound;
                }
                else if (weapon.type == WeaponScript.DOEEBEFKIJI.SHOTGUN)
                {
                    ws.MMIKHOPCECE.bulletsPerClip = weapon.bulletsPerClip;
                    ws.MMIKHOPCECE.clips = weapon.clips;
                    ws.MMIKHOPCECE.fireRate = weapon.fireRate;
                    ws.MMIKHOPCECE.errorAngle = weapon.aimRecoil;
                    ws.MMIKHOPCECE.reloadSound = weapon.reloadSound;
                    ws.MMIKHOPCECE.fireSound = weapon.shotSound;
                }
                else if (weapon.type == WeaponScript.DOEEBEFKIJI.GRENADE_LAUNCHER)
                {
                    // ДОДЕЛАТЬ НАДА ПАТОМ
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
                if (ca != null) ca.DOODHOGLOJO.Add(ws); //two handled weapon

                foreach (var t in dummyWeapon.GetComponentsInChildren<Renderer>())
                {
                    if (t.material.name.ToUpper() == "FUR (INSTANCE)")
                    {
                        var a = t.gameObject.AddComponent<AntennaColor>();
                        a.DCMJNPMPBIK = source.transform.FindChild("PLAYER_MODEL/model/leftarmmesh").gameObject.GetComponent<SkinnedMeshRenderer>();
                    }
                }

                GameObject.Destroy(bundleWeapon);
                CuteLogger.Meow("6");
                //ws.HANLPABBHKN = 
                MelonCoroutines.Start(zaebalo(wa, ws));
                MelonCoroutines.Start(CustomWeapons.InstantiateTPSWeapons(source.GetComponent<PlayerNetworkController>()));
            }
        }

        public static IEnumerator InstantiateTPSWeapons(PlayerNetworkController source)
        {
            yield return new WaitForSeconds(0.05f);

            Transform p = source.HDCJMMEMDEJ;

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
            CuteLogger.Meow("1");
            var weapons = Helper.GetAllDirectories(directory);
            var weapon = new WeaponInfo()
            {
                name = "M4A1",
                clips = 275,
                bulletsPerClip = 35,
                fireRate = 0.08f,
                reloadTime = 2.5f,
                type = WeaponScript.DOEEBEFKIJI.MACHINE_GUN,
                aimPosition = new Vector3(-0.147f, 0.036f, -0.6f),
                recoilPower = 0.35f,
                shakePower = 0.5f,
                recoil = 1f,
                aimRecoil = 0.2f
            };
            var weapon2 = new WeaponInfo()
            {
                name = "M87T",
                fractions = 10,
                clips = 48,
                bulletsPerClip = 6,
                fireRate = 0.8f,
                reloadTime = 4.4f,
                type = WeaponScript.DOEEBEFKIJI.SHOTGUN,
                aimPosition = new Vector3(-0.2f, 0.05f, 0),
                recoilPower = 0.35f,
                shakePower = 0.5f,
                recoil = 1f,
                aimRecoil = 6f
            };
            customWeapons.Add(weapon);
            customWeapons.Add(weapon2);
            CuteLogger.Meow("2");
            for (int i = 0; i < weapons.Count; i++)
            {
                CuteLogger.Meow("3");
                string bundlePath = weapons[i].FullName + @"\bundle";
                if (!File.Exists(bundlePath))
                {
                    return;
                }
                CuteLogger.Meow("4");
                //var json = File.ReadAllText(weapons[i].FullName + @"\npc.json");
                //var weapon = JSON.Load(json).Make<WeaponInfo>();
                
                CuteLogger.Meow("5");
                customWeapons[i].path = weapons[i].FullName;
                
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
            }
        }
    }

    

    public class WeaponInfo
    {
        public string name;
        public string path;

        public WeaponScript.DOEEBEFKIJI type;

        //SHOTGUN
        public int fractions;

        //GRENADE LAUNCHER
        public int projectileSpeed;
        public float waitBeforeReload;

        public float fireRate;
        public int bulletsPerClip;
        public int clips;
        public float reloadTime;
        public Vector3 aimPosition;
        public float recoil;
        public float aimRecoil;

        public float recoilPower;
        public float shakePower;

        public AudioClip shotSound;
        public AudioClip reloadSound;

        public Texture2D icon;
        public Texture2D crosshair;
        //blabla
    }
}
