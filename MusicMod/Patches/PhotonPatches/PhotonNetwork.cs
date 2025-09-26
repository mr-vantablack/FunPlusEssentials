using Il2CppExitGames.Client.Photon;
using FunPlusEssentials.CustomContent;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Il2Cpp;

namespace FunPlusEssentials.Patches
{
    [HarmonyLib.HarmonyPatch(typeof(PhotonNetwork), "NOOU2")]
    public static class PhotonNetworkInstantiateRoomPrefab
    {
        static bool Prefix(ref string prefabName, ref Vector3 position, ref Quaternion rotation)
        {
            return !NPCManager.Instantiate(prefabName, position, rotation);
        }
    }
    [HarmonyLib.HarmonyPatch(typeof(PhotonNetwork), "NOOU", new Type[] { typeof(string), typeof(Vector3), typeof(Quaternion), typeof(byte) })]
    public static class PhotonNetworkInstantiate
    {
        static bool Prefix(ref string prefabName, ref Vector3 position, ref Quaternion rotation)
        {
            return !NPCManager.Instantiate(prefabName, position, rotation);
        }
    }
    [HarmonyLib.HarmonyPatch(typeof(SupportClass), "GetMethods")]
    public static class RPCPatch
    {
        [HarmonyLib.HarmonyPostfix]
        static void Postfix(ref Il2CppSystem.Collections.Generic.List<Il2CppSystem.Reflection.MethodInfo> __result, ref Il2CppSystem.Type type)
        {
            if (PhotonManager.IsHandleRPC(type))
            {
                var t = type;
                var list = type.GetMethods((Il2CppSystem.Reflection.BindingFlags)(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic));
                PhotonManager.rpcMethodsCache.TryGetValue(PhotonManager.rpcMethodsCache.Keys.Where(n => n.Name == t.Name).FirstOrDefault(), out var methods);
                foreach (Il2CppSystem.Reflection.MethodInfo methodInfo in list)
                {
                    foreach (MethodInfo method in methods)
                    {
                        if (method.Name == methodInfo.Name && method.GetParameters().Length == methodInfo.GetParameters().Length)
                        {
                            __result.Add(methodInfo);
                        }
                    }
                }
            }
        }
    }
}
