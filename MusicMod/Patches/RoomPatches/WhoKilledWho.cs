using FunPlusEssentials.CustomContent;
using FunPlusEssentials.Essentials;
using FunPlusEssentials.Other;
using Il2Cpp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FunPlusEssentials.Patches
{
    [HarmonyLib.HarmonyPatch(typeof(WhoKilledWho), "OnPhotonPlayerConnected")]
    public static class OnPhotonPlayerConnected
    {
        public delegate void Event(PhotonPlayer otherPlayer);
        public static event Event onPhotonPlayerConnected;

        [HarmonyLib.HarmonyPrefix]
        static void Prefix(ref PhotonPlayer otherPlayer, WhoKilledWho __instance)
        {
            onPhotonPlayerConnected?.Invoke(otherPlayer);
            string clearName = otherPlayer.name.Split('|')[0];
            if (Config.blacklistEnabled)
            {
                CuteLogger.Meow(ConsoleColor.Green, $"{clearName} connected.");
                if (Blacklist.CheckPlayer(clearName) && PhotonNetwork.isMasterClient)
                {

                    Il2CppSystem.Object content = new Il2CppSystem.Int32() { m_value = otherPlayer.actorID }.BoxIl2CppObject();
                    RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.MasterClient };
                    PhotonNetwork.RaiseEvent(1, content, true, raiseEventOptions);
                    CuteLogger.Meow(ConsoleColor.Red, $"{clearName} was kicked.");
                }
            }
            if (MapManager.useCustomNPCs && PhotonNetwork.isMasterClient)
            {
                var fpe = otherPlayer.customProperties["FPE"] != null ? otherPlayer.customProperties["FPE"].ToString() : "";
                if (fpe == "")
                {
                    PhotonNetwork.CloseConnection(otherPlayer);
                    CuteLogger.Meow(ConsoleColor.Red, $"{clearName} was kicked.");
                }
            }
        }
    }
    [HarmonyLib.HarmonyPatch(typeof(WhoKilledWho), "OnPhotonPlayerDisconnected")]
    public static class OnPhotonPlayerDisconnected
    {
        public delegate void Event(PhotonPlayer otherPlayer);
        public static event Event onPhotonPlayerDisonnected;

        [HarmonyLib.HarmonyPrefix]
        static void Prefix(ref PhotonPlayer ANLAKOKFGCJ, WhoKilledWho __instance)
        {
            onPhotonPlayerDisonnected?.Invoke(ANLAKOKFGCJ);
            string clearName = ANLAKOKFGCJ.name.Split('|')[0];
            CuteLogger.Meow(ConsoleColor.Red, $"{clearName} disconnected.");
        }
    }
}
