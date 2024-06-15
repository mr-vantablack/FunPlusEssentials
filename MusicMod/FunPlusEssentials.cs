
using MelonLoader;
using FunPlusEssentials.Fun;
using FunPlusEssentials.Essentials;
using FunPlusEssentials.CustomContent;
using UnityEngine;
using System.Collections;
using FunPlusEssentials.Other;
using UnhollowerRuntimeLib;

[assembly: MelonInfo(typeof(FunPlusEssentials.FPE), "Fun Plus Essentials", "4.2", "Vantablack")]
[assembly: MelonGame("ZeoWorks", "Slendytubbies 3")]

namespace FunPlusEssentials
{
    public struct AppInfo
    {
        public string Name;
        public string Version;
        public bool UpdateAvailable;
        public string DiscordLink;
    }
    
    public class FPE : MelonMod
    {
        public static AppInfo AppInfo;
        public override void OnSceneWasInitialized(int buildIndex, string sceneName)
        {
            if (sceneName != "Updater" && sceneName != "MainMenu")
            {
                if (MapManager.useCustomNPCs)
                {
                    NPCManager.LoadBundles();
                }
            }
        }
        public override void OnSceneWasLoaded(int buildIndex, string sceneName)
        {
            if (sceneName != "Updater" && sceneName != "MainMenu")
            {
                // if (MapManager.useCustomNPCs) NPCManager.LoadBundles();
                MapManager.CheckForCustomMap(sceneName);             
                PhotonNetwork.isMessageQueueRunning = true;
            }
            if (sceneName == "MainMenu")
            {
                Plague.Enabled = false;
                if (!PlagueAssets._inited) 
                {
                    var pa = new GameObject();
                    pa.AddComponent<PlagueAssets>();
                    pa.hideFlags = HideFlags.HideAndDontSave;
                    PlagueAssets._inited = true;
                }
                PhotonNetwork.automaticallySyncScene = true;
                Helper.SetPropertyV2("FPE", FPE.AppInfo.Version.ToString());
                MapManager.useCustomNPCs = false;
                MapManager.isCustomMap = false;
                NPCManager.loadedBundles.Clear();
                if (Config.customServersEnabled) { ServerManager.AddCustomServers(); }
                MelonCoroutines.Start(MapManager.CheckMainMenuOverride());
                MelonCoroutines.Start(CheckModVersion());
            }
            if (sceneName == "Research Lab")
            {
                var t = GameObject.Find("Offlink (7)").transform.GetChild(0);
                GameObject.CreatePrimitive(PrimitiveType.Cube).transform.position = t.position;
            }
            QualitySettings.vSyncCount = Config.vSyncCount;
        }

        public override void OnApplicationStart()
        {
            AppInfo.Name = Info.Name;
            AppInfo.Version = Info.Version;
        }
        public override void OnInitializeMelon()
        {
            
        }
        public override void OnApplicationLateStart()
        {
            Config.SetUpConfig();
            MelonCoroutines.Start(ServerManager.GetFPEServers());
            Blacklist.CheckPlayer(" ");
            MapManager.SetUp();
            NPCManager.Init();
            CustomWeapons.Init();
            PhotonManager.Init();
            CuteLogger.ClearLogs();
        }

        public IEnumerator CheckModVersion()
        {
            WWW w = new WWW("https://ippls.lh1.in/fpe-version");
            yield return w;
            WWW d = new WWW("https://ippls.lh1.in/discord-link");
            yield return d;
            if (w.error != null && d.error != null)
            {
                CuteLogger.Bark("Version check failed... " + w.error);
            }
            else
            {
                AppInfo.DiscordLink = d.text;
                if (AppInfo.Version != w.text)
                {
                    AppInfo.UpdateAvailable = true;
                    CuteLogger.Quack($"Your version of the mod is outdated (v{AppInfo.Version}). It's recommended to install the latest (v{w.text}) version from our Discord ({d.text}).");
                }
                else
                {
                    CuteLogger.Meow(System.ConsoleColor.Green, $"You have installed the latest version of the FunPlusEssentials mod.");
                }
            }
        }
    }
}
