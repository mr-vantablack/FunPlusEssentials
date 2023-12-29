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
    class FunRPC : Attribute
    {
    }
    class UsingRPC : Attribute
    {
    }
    public static class RPCManager
    {
        public static Dictionary<Type, List<MethodInfo>> rpcMethodsCache = new Dictionary<Type, List<MethodInfo>>();

        public static void Init()
        {
            foreach (Type type in Assembly.GetExecutingAssembly().GetTypes())
            {
                if (type.IsClass && type.GetCustomAttribute<UsingRPC>() != null)
                {
                    rpcMethodsCache.Add(type, type.GetMethods().Where(m => m.GetCustomAttribute<FunRPC>() != null).ToList());
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
}
