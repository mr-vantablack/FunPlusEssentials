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

        public static IEnumerator InstantiatePrefabs()
        {
            yield return new WaitForSeconds(0.05f);
            yield return MelonCoroutines.Start(LoadWeaponsIcons());

            // customWeapons.Add(new WeaponInfo() { name = "M4A1", clips = 105, fireRate = 0.05f, type = WeaponScript.DOEEBEFKIJI.MACHINE_GUN });
            weaponRoot = Helper.WeaponManager.gameObject;
            weaponsDummies = new GameObject[]
            {
                weaponRoot.transform.FindChild("AKM").gameObject
            };
            CuteLogger.Meow("1");
            foreach (WeaponInfo weapon in customWeapons)
            {
                if (!loadedBundles.ContainsKey(weapon.name))
                {
                    CuteLogger.Meow("11");
                    var assetBundleCreateRequest = Il2CppAssetBundleManager.LoadFromFile(weapon.path + @"\bundle");
                    loadedBundles.Add(weapon.name, new GameObject[]
                    {
                                assetBundleCreateRequest.Load<GameObject>(weapon.name),
                    });
                    weapon.shotSound = assetBundleCreateRequest.Load<AudioClip>("fire");
                    weapon.reloadSound = assetBundleCreateRequest.Load<AudioClip>("reload");
                    assetBundleCreateRequest.Unload(false);
                }
                else if (loadedBundles.TryGetValue(weapon.name, out var bundle))
                {
                    CuteLogger.Meow("22");
                    if (bundle[0] == null)
                    {
                        loadedBundles.Remove(weapon.name);
                        var assetBundleCreateRequest = Il2CppAssetBundleManager.LoadFromFile(weapon.path + @"\bundle");
                        yield return assetBundleCreateRequest;
                        loadedBundles.Add(weapon.name, new GameObject[]
                        {
                                assetBundleCreateRequest.Load<GameObject>(weapon.name),
                        });
                        weapon.shotSound = assetBundleCreateRequest.Load<AudioClip>("fire");
                        weapon.reloadSound = assetBundleCreateRequest.Load<AudioClip>("reload");
                        assetBundleCreateRequest.Unload(false);
                    }
                }
                CuteLogger.Meow("2");
                GameObject dummyWeapon = GameObject.Instantiate(weaponsDummies[0]);
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
                var ws = dummyWeapon.GetComponent<WeaponScript>();

                ws.FLIAJIAOBHA.aimPosition = weapon.aimPosition;
                ws.FMPLNFBLHKJ.bulletsPerClip = weapon.bulletsPerClip;
                ws.FMPLNFBLHKJ.clips = weapon.clips;
                ws.FMPLNFBLHKJ.fireRate = weapon.fireRate;
                ws.FMPLNFBLHKJ.AimErrorAngle = weapon.aimRecoil;
                ws.FMPLNFBLHKJ.NoAimErrorAngle = weapon.recoil;

                ws.APFMIOFJJNC.recoilPower = weapon.recoilPower;
                ws.APFMIOFJJNC.shakeAmount = weapon.shakePower;

                ws.FMPLNFBLHKJ.reloadSound = weapon.reloadSound;
                ws.FMPLNFBLHKJ.fireSound = weapon.shotSound;

                ws.MGFCKCEIEKM = weapon.icon;
                ws.HIHFCMAJBJK = weapon.crosshair;

                GameObject.Destroy(bundleWeapon);
                CuteLogger.Meow("6");
                //ws.HANLPABBHKN = 
                MelonCoroutines.Start(zaebalo(wa, ws));
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
                var weapon = new WeaponInfo() 
                { 
                    name = "M4A1", 
                    clips = 275,
                    bulletsPerClip = 35,
                    fireRate = 0.08f,
                    reloadTime = 2.5f,
                    type = WeaponScript.DOEEBEFKIJI.MACHINE_GUN,
                    aimPosition = new Vector3(-0.147f, 0.036f, - 0.6f),
                    recoilPower = 0.35f,
                    shakePower = 0.5f,
                    recoil = 1f,
                    aimRecoil = 0.2f
                };
                CuteLogger.Meow("5");
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
            }
        }
    }

    

    public class WeaponInfo
    {
        public string name;
        public string path;

        public WeaponScript.DOEEBEFKIJI type;
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
