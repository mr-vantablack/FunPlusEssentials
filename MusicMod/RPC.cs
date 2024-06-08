using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using FunPlusEssentials.CustomContent;
using FunPlusEssentials.Other;
using MelonLoader;
using UnhollowerBaseLib;
using UnityEngine;

namespace FunPlusEssentials
{
    public class FunRPC : Attribute
    {
    }
    public class UsingRPC : Attribute
    {
    }
    public static class RPCManager
    {
        public static Dictionary<Type, List<MethodInfo>> rpcMethodsCache = new Dictionary<Type, List<MethodInfo>>();

        public static void Init()
        {
            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                foreach (Type type in assembly.GetTypes())
                {
                    if (type.IsClass && type.GetCustomAttribute<UsingRPC>() != null)
                    {
                        rpcMethodsCache.Add(type, type.GetMethods().Where(m => m.GetCustomAttribute<FunRPC>() != null).ToList());
                    }
                }
            }
        }

        public static bool IsUsingRPC(Il2CppSystem.Type type)
        {
            foreach (Type t in rpcMethodsCache.Keys)
            {
                if (t.Name == type.Name) return true;
            }
            return false;
        }
    }

    [RegisterTypeInIl2Cpp, UsingRPC]
    public class FunRPCHandler : MonoBehaviour
    {
        public FunRPCHandler(IntPtr ptr) : base(ptr) { }

        public void OnEnable()
        {
            if (PhotonNetwork.isMasterClient)
            {

            }
        }

        [FunRPC]
        public void SyncCustomNPC(string npcName)
        {
            CuteLogger.Meow("3");
            //NPCManager.SetUpCustomNPC(npcName);
            
        }
    }
}
