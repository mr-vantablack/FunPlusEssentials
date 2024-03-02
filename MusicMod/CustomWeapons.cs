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

namespace FunPlusEssentials
{
    public static class CustomWeapons
    {
        public static string directory = Config.mainPath + @"\CustomWeapons";
        public static List<WeaponInfo> customWeapons;
        public static Dictionary<string, GameObject[]> loadedBundles = new Dictionary<string, GameObject[]>();
        public static GameObject weaponRoot;
        public static GameObject[] weaponsDummies;

        public static void Init()
        {
            weaponRoot = Helper.WeaponManager.gameObject;
            weaponsDummies = new GameObject[]
            {
                weaponRoot.transform.FindChild("").gameObject
            };

            foreach (WeaponInfo weapon in customWeapons)
            {

            }
        }

    }
    public class WeaponInfo
    {
        public string name;
        public WeaponScript.DOEEBEFKIJI type;
        //blabla
    }
}
