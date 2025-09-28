using FunPlusEssentials.CustomContent;
using FunPlusEssentials.Other;
using MelonLoader;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.UI;
using UnityEngine;
using Il2Cpp;
using Il2CppSystem.Collections.Generic;
using System.Collections;

namespace FunPlusEssentials.Patches
{
    [RegisterTypeInIl2Cpp]
    public class VolumePatch : MonoBehaviour
    {
        public VolumePatch(IntPtr ptr) : base(ptr) { }
        public float currentValue = 0f;
        public float minValue;
        public float maxValue;
        public float scrollSensitivity = 1f;
        public RectTransform rect;
        public void Start()
        {
            rect = transform.FindChild("Canvas/Console/Options_UI/Options_Root").GetComponent<RectTransform>();
            minValue = -60f;
            maxValue = rect.GetComponent<GridLayoutGroup>().preferredHeight - 500f;
        }
        void Update()
        {
            float scrollInput = Input.GetAxis("Mouse Y");

            if (scrollInput != 0)
            {
                float oldValue = currentValue;
                currentValue += scrollInput * scrollSensitivity;
                currentValue = Mathf.Clamp(currentValue, minValue, maxValue);

                // Вызываем событие если значение изменилось
                if (oldValue != currentValue)
                {

                    rect.offsetMax.Set(-20f, currentValue);
                }
            }
        }
    }

    [HarmonyLib.HarmonyPatch(typeof(Volume), "Awake")]
    public static class VolumeStart
    {
        static void Postfix(Volume __instance)
        {
            if (PhotonNetwork.isMasterClient)
            {
                __instance.POJLLLGLPKL = 100;
            }
            else
            {
                __instance.POJLLLGLPKL = 50;
            }
            if (MapManager.useCustomNPCs)
            {
                __instance.transform.GetComponentInChildren<CanvasScaler>().screenMatchMode = CanvasScaler.ScreenMatchMode.Expand;
                MelonCoroutines.Start(NPCManager.AddNPCInfos(__instance));

                CustomWeapons.AddWeaponsToCatagory();
            }
            /*var o = __instance.transform.FindChild("Canvas/Console/Options_UI");
            var s = o.gameObject.AddComponent<ScrollRect>();
            var clone = GameObject.Instantiate(o,o.position, o.rotation, o);
            GameObject.Destroy(clone.transform.GetChild(0).gameObject);
            clone.SetParent(o);
            o.transform.GetChild(0).SetParent(clone);
            for (int i = 0; i < 70; i++)
            {
                __instance.PDKPIOHFCCK[0].options.Add(new Volume.option() { image = null, optionName = $"test{i}", resourcePath = "test" });
            }
            __instance.gameObject.AddComponent<VolumePatch>();*/
        }
    }
    [HarmonyLib.HarmonyPatch(typeof(Volume), "SendOption")]
    public static class VolumeSendOption
    {
        [HarmonyLib.HarmonyFinalizer]
        static Exception Finalizer(Exception __exception)
        {
            if (__exception != null)
            {
                if (Helper.RoomMultiplayerMenu.CNLHJAICIBH == null)
                {
                    var c = GameObject.FindObjectOfType<BossCam>();
                    if (c != null)
                    {
                        if (c.INHFFFAKNNG != null)
                        {
                            Helper.RoomMultiplayerMenu.CNLHJAICIBH = c.INHFFFAKNNG.gameObject;
                        }
                        else if (c.OMIOOOFAJNP != null)
                        {
                            Helper.RoomMultiplayerMenu.CNLHJAICIBH = c.OMIOOOFAJNP.gameObject;
                        }
                    }
                }
            }
            return null;
        }
    }
}
