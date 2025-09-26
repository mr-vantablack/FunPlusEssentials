using UnityEngine;
using MelonLoader;
using System.IO;
using System;
using System.Collections.Generic;
using FunPlusEssentials.Essentials;
using FunPlusEssentials.Other;
using FunPlusEssentials.CustomContent.Triggers;
using System.Collections;
using IniFile = FunPlusEssentials.Other.IniFile;
using MelonLoader.TinyJSON;
using UnityEngine.UI;
using Il2CppSystem.Runtime.Remoting.Messaging;
using static FunPlusEssentials.CustomContent.FunNPCInfo;
using Il2CppInControl.mod;
using System.Text.RegularExpressions;
using System.Linq;
using UnityEngine.SceneManagement;
using Harmony;

namespace FunPlusEssentials
{
    public class FunTriggerInfo
    {

    }

    [RegisterTypeInIl2Cpp]
    public class FunTrigger : MonoBehaviour
    {
        public FunTrigger(IntPtr ptr) : base(ptr) { }
        public string test;

        void Update()
        {

        }
    }
    [RegisterTypeInIl2Cpp]
    public class FunTrigger2 : FunTrigger
    {
        public FunTrigger2(IntPtr ptr) : base(ptr) { }
        public string test2;

    }
}
