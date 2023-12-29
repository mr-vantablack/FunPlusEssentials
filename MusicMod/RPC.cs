using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using FunPlusEssentials.Other;
using MelonLoader;
using UnhollowerBaseLib;
using UnityEngine;

namespace FunPlusEssentials
{
    class RegisterRPC : Attribute
    {
    }
    class UsingRPC : Attribute
    {
    }
    public static class RPC
    {
        public const string rpcPrefix = "RPC_";
        public static Dictionary<Type, List<MethodInfo>> rpcMethodsCache = new Dictionary<Type, List<MethodInfo>>();


        public static void Init()
        {
            foreach (Type type in Assembly.GetExecutingAssembly().GetTypes())
            {
                if (type.IsClass && type.GetCustomAttribute<UsingRPC>() != null)
                {
                    CuteLogger.Meow(type.Name);
                    rpcMethodsCache.Add(type, type.GetMethods().Where(m => m.GetCustomAttribute<RegisterRPC>() != null).ToList());
                }
            }
        }
        public static void SendAll(Type type, string methodName, params object[] parameters)
        {
            string rpcParams = "";
            foreach (var parameter in parameters)
            {
                rpcParams += $":{parameter.ToString()}";
            }
            Il2CppReferenceArray<Il2CppSystem.Object> rpcData = new Il2CppReferenceArray<Il2CppSystem.Object>(new Il2CppSystem.Object[]
            {
                rpcPrefix,
                type.GetType().Name,
                methodName,
                rpcParams
            });
            Helper.RPCView.RPC("networkAddMessage", PhotonTargets.All, rpcData);
        }
        public static bool IsUsingRPC(Il2CppSystem.Type type)
        {
            MelonLogger.Msg(type.Name);
            foreach (Type t in rpcMethodsCache.Keys)
            {
                if (t.Name == type.Name) return true;
            }
            return false;
        }
        // internal static List<PhotonPlayer> GetModdedViews
    }
}
