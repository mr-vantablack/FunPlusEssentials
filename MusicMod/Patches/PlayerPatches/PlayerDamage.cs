using FunPlusEssentials.Other;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace FunPlusEssentials.Patches
{
    [HarmonyLib.HarmonyPatch(typeof(PlayerDamage), "Awake")]
    public static class OnPlayerSpawned
    {
        public delegate void Event(PlayerDamage player);
        public static event Event onPlayerSpawned;
        static void Postfix(PlayerDamage __instance)
        {
            if (Plague.Enabled) { __instance.AKCNDMPBBJP = 1; }
            __instance.GKNFEDOCILC = new Quaternion(0f, 0f, 0f, 0f);
            Helper.SetProperty("joined", "true");
            onPlayerSpawned?.Invoke(__instance);
            __instance.transform.FindChild("PLAYER_MODEL/Bip001/Bip001 Pelvis/Bip001 Spine/Bip001 Spine1/Bip001 Spine2/Bip001 Spine3/Bip001 Neck/Bip001 Head/tubby_head_Custom").gameObject.GetComponent<Renderer>().lightProbeUsage = UnityEngine.Rendering.LightProbeUsage.BlendProbes;
            foreach (Renderer renderer in __instance.transform.FindChild("PLAYER_MODEL/model").GetComponentsInChildren<Renderer>())
            {
                renderer.lightProbeUsage = UnityEngine.Rendering.LightProbeUsage.BlendProbes;
            }
        }
    }
}
