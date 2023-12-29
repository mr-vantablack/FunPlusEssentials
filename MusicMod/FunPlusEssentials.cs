
using MelonLoader;
using FunPlusEssentials.Fun;
using FunPlusEssentials.Essentials;
using FunPlusEssentials.CustomContent;
using UnityEngine;
using System.Collections;
using FunPlusEssentials.Other;
using UnhollowerRuntimeLib;

[assembly: MelonInfo(typeof(FunPlusEssentials.FPE), "Fun Plus Essentials", "2.9", "Vantablack")]
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

        public override void OnSceneWasLoaded(int buildIndex, string sceneName)
        {
            MapManager.CheckForCustomMap(sceneName);
            if (sceneName != "Updater" && sceneName != "MainMenu")
            {
                PhotonNetwork.isMessageQueueRunning = true;
            }
            if (sceneName == "MainMenu")
            {
                if (ServerManager.enabled) { ServerManager.AddCustomServers(); }
                MelonCoroutines.Start(MapManager.CheckMainMenuOverride());
            }
            if (sceneName == "Research Lab")
            {
                var t = GameObject.Find("Offlink (7)").transform.GetChild(0);
                GameObject.CreatePrimitive(PrimitiveType.Cube).transform.position = t.position;
            }
        }

        public override void OnApplicationStart()
        {
            AppInfo.Name = Info.Name;
            AppInfo.Version = Info.Version;
        }

        public override void OnApplicationLateStart()
        {
            MelonCoroutines.Start(CheckModVersion());
            MelonCoroutines.Start(ServerManager.GetFPEServers());
            Config.SetUpConfig();
            Blacklist.CheckPlayer(" ");
            MapManager.SetUp();
            RPCManager.Init();
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
