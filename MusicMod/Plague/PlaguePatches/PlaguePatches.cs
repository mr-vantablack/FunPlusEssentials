using FunPlusEssentials.Essentials;
using FunPlusEssentials.Other;
using Il2CppSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnhollowerBaseLib;
using UnityEngine;

namespace FunPlusEssentials.Patches
{
    [HarmonyLib.HarmonyPatch(typeof(UnityEngine.Object), "Destroy", new System.Type[] { typeof(GameObject), typeof(float) })]
    public static class lp2
    {
        static void Prefix(ref UnityEngine.Object obj, ref float t)
        {
            if (Plague.Enabled)
            {
                if (obj.name == "Ragdoll")
                {
                    t = 10000f;
                    obj.name = "Ragdoll?";
                }
            }
        }
    }
    [HarmonyLib.HarmonyPatch(typeof(PlayerDamage), "Update")]
    public static class PlagueMaxHPPatch
    {
        static bool Prefix(PlayerDamage __instance)
        {
            if (!Plague.Enabled) return true;
            else
            {
                __instance.CCPDLEAFFLE = Mathf.Lerp(__instance.CCPDLEAFFLE, 0f, Time.deltaTime * 2f);
                __instance.IHIPHCLMCOG = Mathf.Lerp(__instance.IHIPHCLMCOG, 0f, Time.deltaTime * 2f);
                if (Camera.main)
                {
                    Camera.main.transform.localRotation = Quaternion.Slerp(Camera.main.transform.localRotation, __instance.GKNFEDOCILC, Time.deltaTime * 15f);
                }
                if (Helper.GetProperty(__instance.photonView.owner, "MaxHP") != null)
                {
                    __instance.NILGDNBIFDC = Helper.GetProperty(__instance.photonView.owner, "MaxHP").Unbox<float>();
                }
            }
            return false;
        }
    }
    /*[HarmonyLib.HarmonyPatch(typeof(PlayerDamage), "KOFOOHFOGHL")]
    public static class PlagueBloodPatch
    {
        static void Postfix(ref float CJKBHAHOLMJ, ref PhotonPlayer HMBMNJKMGLD, PlayerDamage __instance)
        {
            if (Plague.Enabled)
            {
                GameObject.Instantiate(PlagueAssets.Instance._blood, __instance.transform.position + new Vector3(0f, 2f, 0f), Quaternion.identity);
                if (__instance.DFLJPEDEMDH && HMBMNJKMGLD.ID == PhotonNetwork.player.ID)
                {
                    if (PlagueController.Instance._playerClass.ClassName == "Hunter")
                    {
                        var pm = Helper.RoomMultiplayerMenu.CNLHJAICIBH.GetComponent<PhotonView>();
                        if (pm != null)
                        {
                            CuteLogger.Meow("Healed");
                            pm.RPC("HealRPC", PhotonTargets.All, null);
                        }
                    }
                }
            }
        }
    }*/
    [HarmonyLib.HarmonyPatch(typeof(WhoKilledWho), "AddKillNotification")]
    public static class PlagueOnKilledPatch
    {
        static void Postfix(WhoKilledWho __instance)
        {
            if (Plague.Enabled)
            {
                if (PlagueController.Instance._playerClass.ClassName == "Hunter")
                {
                    var pm = Helper.RoomMultiplayerMenu.CNLHJAICIBH.GetComponent<PhotonView>();
                    if (pm != null)
                    {
                        CuteLogger.Meow("Healed");
                        pm.RPC("HealRPC", PhotonTargets.All, null);
                    }
                }
            }
        }
    }
    [HarmonyLib.HarmonyPatch(typeof(PlayerDamage), "Awake")]
    public static class PlagueSurvivorSpawnedPatch
    {
        [Harmony.HarmonyPostfix]
        static void Postfix(PlayerDamage __instance)
        {
            if (Plague.Enabled)
            {
                var t = __instance.transform.FindChild("PLAYER_MODEL/Bip001/Bip001 Pelvis/Bip001 Spine/Bip001 Spine1/Bip001 Spine2/Bip001 Spine3");
                var vest = GameObject.Instantiate(PlagueAssets.Instance._equipmentVest[UnityEngine.Random.Range(0, PlagueAssets.Instance._equipmentVest.Count)], t);
            }
        }
    }
    [HarmonyLib.HarmonyPatch(typeof(PlayerDamage), "KOFOOHFOGHL")]
    public static class PlagueBloodPatch
    {
        [Harmony.HarmonyPostfix]
        static void Postfix(ref float CJKBHAHOLMJ, ref PhotonPlayer HMBMNJKMGLD, PlayerDamage __instance)
        {
            if (Plague.Enabled)
            {
                var pos = __instance.GetComponentInChildren<MeshRenderer>().bounds.center;
                PlagueController.Instance.BloodSplash(pos);
            }
        }
    }
    [HarmonyLib.HarmonyPatch(typeof(Custard), "Start")]
    public static class CustardStart
    {
        static bool Prefix(Custard __instance)
        {
            if (!Plague.Enabled) return true;
            var data = __instance.photonView.instantiationData;
            if (data != null && data[0].ToString() == "Medkit")
            {
                __instance.transform.GetChild(0).gameObject.SetActive(false);
                GameObject.Destroy(__instance.GetComponent<Custard>());
                GameObject.Instantiate(PlagueAssets.Instance._medKit, __instance.transform);
                __instance.gameObject.name = "Medkit";
                var c = __instance.gameObject.AddComponent<MedKit>();
                if (__instance.photonView.isMine) __instance.gameObject.AddComponent<Rigidbody>();
                __instance.photonView.onSerializeTransformOption = OnSerializeTransform.PositionAndRotation;
                return false;
            }
            if (data != null && data[0].ToString() == "Wire")
            {
                __instance.transform.GetChild(0).gameObject.SetActive(false);
                GameObject.Destroy(__instance.GetComponent<Custard>());
                GameObject.Destroy(__instance.GetComponent<BoxCollider>());
                GameObject.Instantiate(PlagueAssets.Instance._wire, __instance.transform);
                __instance.gameObject.name = "Wire";
                var c = __instance.transform.FindChild("Wire(Clone)").gameObject.AddComponent<WireTrap>();
                int p = int.Parse(data[1].ToString());
                foreach (var player in PhotonNetwork.playerList)
                {
                    if (player.ID == p)
                    {
                        c.owner = player;
                    }
                }
               // if (__instance.photonView.isMine) __instance.gameObject.AddComponent<Rigidbody>();
                __instance.photonView.onSerializeTransformOption = OnSerializeTransform.PositionAndRotation;
                return false;
            }
            if (data != null && data[0].ToString() == "LandMine")
            {
                __instance.transform.GetChild(0).gameObject.SetActive(false);
                GameObject.Destroy(__instance.GetComponent<Custard>());
                GameObject.Destroy(__instance.GetComponent<BoxCollider>());
                GameObject.Instantiate(PlagueAssets.Instance._landMine, __instance.transform);
                __instance.gameObject.name = "LandMine";
                var c = __instance.transform.FindChild("LandMine(Clone)").gameObject.AddComponent<MineTrap>();
                int p = int.Parse(data[1].ToString());
                int type = int.Parse(data[2].ToString());
                c.type = type;
                // if (__instance.photonView.isMine) __instance.gameObject.AddComponent<Rigidbody>();
                __instance.photonView.onSerializeTransformOption = OnSerializeTransform.PositionAndRotation;
                return false;
            }
            if (data != null && data[0].ToString() == "Supply")
            {
                __instance.transform.GetChild(0).gameObject.SetActive(false);
                GameObject.Destroy(__instance.GetComponent<Custard>());
                GameObject.Instantiate(PlagueAssets.Instance._supplyBox, __instance.transform);
                __instance.gameObject.name = "SupplyCrate";
                var c = __instance.gameObject.AddComponent<SupplyCrate>();
                if (__instance.photonView.isMine) __instance.gameObject.AddComponent<Rigidbody>();
                __instance.photonView.onSerializeTransformOption = OnSerializeTransform.PositionAndRotation;
                return false;
            }
            return true;
        }
    }
   /* [HarmonyLib.HarmonyPatch(typeof(PlayerMonster.ANHAKMDJHAN), "MoveNext")]
    public static class PlagueHitSoundPatch
    {
        static void Postfix()
        {
            if (Plague.Enabled)
            {
                GameObject[] array = GameObject.FindGameObjectsWithTag("Player");
                for (int i = 0; i < array.Length; i++)
                {
                    if (Vector3.Distance(Helper.RoomMultiplayerMenu.CNLHJAICIBH.transform.position, array[i].transform.position) < 2.75f)
                    {
                        PlagueController.Instance.PlaySound();
                    }
                }
            }
        }
    }*/
    [HarmonyLib.HarmonyPatch(typeof(PlayerMonster), "KOFOOHFOGHL")]
    public static class PlagueBloodPatch2
    {
        static void Postfix(PlayerMonster __instance)
        {
            if (Plague.Enabled)
            {
                var pos = __instance.GetComponentInChildren<MeshRenderer>().bounds.center;
                PlagueController.Instance.BloodSplash(pos);
                var pm = __instance.GetComponent<PlagueMonster>();
                if (pm != null) { pm.Knockback(); }
            }
        }
    }
    [HarmonyLib.HarmonyPatch(typeof(PlayerMonster), "E100010")]
    public static class PlagueMonsterDamagePatch
    {
        static void Prefix(ref Il2CppReferenceArray<Il2CppSystem.Object> tempStorage, PlayerMonster __instance)
        {
            if (Plague.Enabled)
            {
                var r = __instance.gameObject.GetComponent<InfectedRage>();
                if (r != null)
                {
                    if (r.rage)
                    {
                        tempStorage[0] = new Il2CppSystem.Int32() { m_value = tempStorage[0].Unbox<int>() / 2 }.BoxIl2CppObject();
                    }
                }
                CuteLogger.Meow($"e100010: { tempStorage[0].Unbox<int>().ToString() }");
            }
        }
    }
    [HarmonyLib.HarmonyPatch(typeof(PlayerDamage), "E100050")]
    public static class PlagueDamagePatch
    {
        static void Prefix(ref float damage, ref string botName, PlayerDamage __instance)
        {
            if (Plague.Enabled && Helper.GetProperty(__instance.GetComponent<PhotonView>().owner, "Armor") != null)
            {
                damage = damage - (damage * Helper.GetProperty(__instance.GetComponent<PhotonView>().owner, "Armor").Unbox<float>() / 100);
                CuteLogger.Meow(damage.ToString());
            }
        }
    }

    [HarmonyLib.HarmonyPatch(typeof(PlayerMonster), "Awake")]
    public static class PlaguePlayerMonsterSpawnPatch
    {
        static void Postfix(PlayerMonster __instance)
        {
            if (Plague.Enabled)
            {
                if (!__instance.GetComponent<PhotonView>().isMine) __instance.gameObject.AddComponent<PlagueMonster>();
                __instance.gameObject.AddComponent<InfectedRage>();
                 __instance.gameObject.AddComponent<PhantomInvisibility>();
                __instance.OnDisable();
            }
        }
    }
    //
    [HarmonyLib.HarmonyPatch(typeof(RagdollController), "BLACJABKHFN")]
    public static class PlagueRespawnPatch
    {
        static bool Prefix(RagdollController __instance)
        {
            if (!Plague.Enabled) return true;
            __instance.NADEDDKIIJL--;
            if (__instance.NADEDDKIIJL == 0)
            {
                __instance.ODAHLNNDKEJ();
                if (PhotonNetwork.offlineMode)
                {
                    PhotonNetwork.Disconnect();
                }
                GameObject.FindWithTag("Network").GetComponent<RoomMultiplayerMenu>().SpawnPlayer("Team B");
                UnityEngine.Object.Destroy(__instance.gameObject);
            }
            return false;
        }
    }
    [HarmonyLib.HarmonyPatch(typeof(RoomMultiplayerMenu), "SpawnPlayer")]
    public static class PlagueSpawnPatch
    {
        static bool Prefix(ref string teamName, RoomMultiplayerMenu __instance)
        {
            if (!Plague.Enabled) return true;
            var team_1 = __instance.OJPBAOICLJK;
            var team_2 = __instance.KGLOGDGOELM;
            var Player = __instance.CNLHJAICIBH;
            var playerPrefab = __instance.BGOEEADMCBE;
            if (Player != null)
            {
                PhotonNetwork.Destroy(Player);
            }
            if (teamName == string.Empty)
            {
                teamName = team_1.teamName;
            }
            if (teamName == team_1.teamName)
            {
                Helper.SetProperty("TeamName", teamName);
                int num = UnityEngine.Random.Range(0, team_1.spawnPoints.Length);
                __instance.CNLHJAICIBH = PhotonNetwork.NOOU(playerPrefab.name, team_1.spawnPoints[num].position, team_1.spawnPoints[num].rotation, 0);
                __instance.CNLHJAICIBH.name = PhotonNetwork.player.name;
                PlagueController.Instance.RandomizePlagueClass();
            }
            if (teamName == team_2.teamName)
            {
                Helper.SetProperty("TeamName", teamName);
                PlagueController.Instance.SpawnInfectedPlayer();
            }
            if (teamName == "LastSurvivor")
            {
                Helper.SetProperty("TeamName", team_1.teamName);
                int num = UnityEngine.Random.Range(0, team_1.spawnPoints.Length);
                __instance.CNLHJAICIBH = PhotonNetwork.NOOU(playerPrefab.name, team_1.spawnPoints[num].position, team_1.spawnPoints[num].rotation, 0);
                __instance.CNLHJAICIBH.name = PhotonNetwork.player.name;
                PlagueController.Instance.SurvivorClassSetUp(777);
            }
            if (teamName == "Nemesis")
            {
                Helper.SetProperty("TeamName", team_2.teamName);
                PlagueController.Instance.SpawnInfectedPlayer(666);
            }
            __instance.DIAFJILHGPC.SetActive(false);
            return false;
        }
        //
    }
    [HarmonyLib.HarmonyPatch(typeof(RoomMultiplayerMenu), "Update")]
    public static class PlagueRoomPatch
    {
        static void Postfix(RoomMultiplayerMenu __instance)
        {
            if (Plague.Enabled) Cursor.visible = __instance.IOMIAHNBDOG;
            Application.targetFrameRate = Config.fpsLock;
        }
    }
    [HarmonyLib.HarmonyPatch(typeof(DrawPlayerName), "OnGUI")]
    public static class PlagueDrawNamePatch
    {
        static bool Prefix(DrawPlayerName __instance)
        {
            if (Plague.Enabled && PlagueController.Instance._playerClass.Infected && PlagueController.Instance._playerClass.ClassName != "Hunter")
            {
                if (!PlagueMonster.Instance._seekReady)
                {
                    return false;
                }
            }
            return true;
        }
    }
    [HarmonyLib.HarmonyPatch(typeof(Bullet), "Start")]
    public static class BulletStart
    {
        [HarmonyLib.HarmonyPostfix]
        static void Postfix(Bullet __instance)
        {
            //machinegun FMPLNFBLHKJ
            //shotgun MMIKHOPCECE
            //knife EKODJEHEFDE
            if (Helper.WeaponManager != null)
            {
                var weapon = Helper.WeaponManager.FGGENNCGLJI;
                if (weapon.name == "XIX")
                {
                    __instance.CJKBHAHOLMJ = 20; //damage
                    //__instance.HHBHJIDDGIL = weapon.bulletForce; //force
                    //__instance.DHHNMAIFIFB = weapon.bulledSpeed; //speed
                    //__instance.LNEDFJIMHDL = weapon.bulletLifeTime; //lifetime
                }
                if (weapon.name == "AKM")
                {
                    __instance.CJKBHAHOLMJ = 15;
                }
                if (weapon.name == "MK16")
                {
                    __instance.CJKBHAHOLMJ = 30;
                }
                if (weapon.name == "MP5N")
                {
                    __instance.CJKBHAHOLMJ = 10;
                }
                if (weapon.name == "XIX II")
                {
                    __instance.CJKBHAHOLMJ = 20;
                }
                if (weapon.name == "M40A3")
                {
                    __instance.CJKBHAHOLMJ = 100;
                }
                if (weapon.name == "Fireaxe")
                {
                    __instance.CJKBHAHOLMJ = 100;
                }
                if (weapon.name == "M249-Saw")
                {
                    __instance.CJKBHAHOLMJ = 15;
                }
                if (weapon.name == "44 Combat")
                {
                    __instance.CJKBHAHOLMJ = 30;
                }
                if (weapon.name == "VZ61")
                {
                    __instance.CJKBHAHOLMJ = 10;
                }
                if (weapon.name == "Katana")
                {
                    __instance.CJKBHAHOLMJ = 300;
                }
                if (weapon.name == "Knife")
                {
                    __instance.CJKBHAHOLMJ = 100;
                }
                if (weapon.name == "Machete")
                {
                    __instance.CJKBHAHOLMJ = 200;
                }
                if (weapon.name == "Shorty")
                {
                    __instance.CJKBHAHOLMJ = 10;
                }
            }
        }
    }
}
