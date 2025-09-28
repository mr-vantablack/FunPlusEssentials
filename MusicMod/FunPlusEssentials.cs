
using MelonLoader;
using FunPlusEssentials.Fun;
using FunPlusEssentials.Essentials;
using FunPlusEssentials.CustomContent;
using UnityEngine;
using System.Collections;
using FunPlusEssentials.Other;
using Il2Cpp;


[assembly: MelonInfo(typeof(FunPlusEssentials.FPE), "Fun Plus Essentials", "5.0", "Vantablack", "https://discord.gg/rYe2ayy8zA")]
[assembly: MelonGame("ZeoWorks", "Slendytubbies 3")]
[assembly: MelonIncompatibleAssemblies("Fun Plus Essentials: Recalled")]
[assembly: MelonColor(255, 255, 128, 66)]
[assembly: MelonPlatform(MelonPlatformAttribute.CompatiblePlatforms.WINDOWS_X64)]
[assembly: MelonPlatformDomain(MelonPlatformDomainAttribute.CompatibleDomains.IL2CPP)]

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
                if (!(bool)MelonPreferences.GetEntry("fpe", "warning").BoxedValue)
                {
                    MelonPreferences.SetEntryValue<bool>("fpe", "warning", true);
                    Notifier.Show("Warning! The FunPlusEssentials may conflict with other installed mods.\nMake sure that only FunPlusEssentials files are located in the mods folder. \nYou will not see this message again.", "I understand");
                }
                Plague.Enabled = false;
                if (!PlagueAssets._inited && Config.plagueEnabled)
                {
                    if (!MelonUtils.IsGame32Bit())
                    {
                        var pa = new GameObject();
                        pa.AddComponent<PlagueAssets>();
                        pa.hideFlags = HideFlags.HideAndDontSave;
                    }
                    else
                    {
                        Notifier.Show("Plague mode is not supported in the 32-bit version of the game.", "Okay :(");
                        Config.plagueEnabled = false;
                    }
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
            MelonPreferences.CreateCategory("fpe", "FunPlusEssentials").CreateEntry<bool>("warning", false, "IncompatibleModsWarning");
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
            WWW w = new WWW("https://raw.githubusercontent.com/mr-vantablack/s/refs/heads/main/version.txt");
            yield return w;
            WWW d = new WWW("https://raw.githubusercontent.com/mr-vantablack/s/refs/heads/main/discord.txt");
            yield return d;
            if (w.error != null && d.error != null)
            {
                CuteLogger.Bark("Version check failed... " + w.error);
            }
            else
            {
                var version = w.text.Remove(3);
                var discord = d.text.Remove(30);
                AppInfo.DiscordLink = discord;
                if (AppInfo.Version != version)
                {
                    AppInfo.UpdateAvailable = true;
                    CuteLogger.Quack($"Your version of the mod is outdated (v{AppInfo.Version}). It's recommended to install the latest (v{version}) version from our Discord ({discord}).");
                }
                else
                {
                    CuteLogger.Meow(System.ConsoleColor.Green, $"You have installed the latest version of the FunPlusEssentials mod.");
                }
            }
        }
    }
}
