using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using FunPlusEssentials;
using FunPlusEssentials.Essentials;
using System.IO;
using MelonLoader;
using FunPlusEssentials.Other;
using MelonLoader.TinyJSON;
using Harmony;
using System.Collections;
using System.CodeDom;
using static MelonLoader.MelonLogger;
using UnityEngine.AI;
using UnhollowerBaseLib;
using static FunPlusEssentials.CustomContent.FunNPCInfo;
using System.Reflection;

namespace FunPlusEssentials.CustomContent
{
    public class FunNPCInfo
    {
        public string name;
        public string version;
        public Sprite iconSprite;
        public string path;
        public FunBot bot;
        public FunBossBot bossBot;
        public FunCustardBot custardBot;
        public FunPlayerMonster playerMonster;
        public bool IsBoss => bossBot != null && bot == null;
    }

    [RegisterTypeInIl2Cpp]
    public class CustomNPC : MonoBehaviour
    {
        public CustomNPC(IntPtr ptr) : base(ptr) { }
        public void OnEnable()
        {
            //  NPCManager.npcPool.Add(gameObject);
        }
        public void OnDisable()
        {
            // NPCManager.npcPool.Remove(gameObject);
        }
    }
    public enum NPCType
    {
        BossBot,
        Bot,
        CustardBot,
        PlayerMonster
    }

    public static class NPCManager
    {
        public static string NPCInfoDirectory = Config.mainPath + @"\CustomNPCs";
        public static List<FunNPCInfo> NPCInfos = new List<FunNPCInfo>();
        //public static List<GameObject> npcPool = new List<GameObject>();
        public static Dictionary<string, GameObject[]> loadedBundles = new Dictionary<string, GameObject[]>();
        static void SetLayerAllChildren(Transform root, int layer)
        {
            var children = root.GetComponentsInChildren<Transform>(includeInactive: true);
            foreach (var child in children)
            {
                child.gameObject.layer = layer;
            }
        }
        public static bool Instantiate(string prefabName, Vector3 position, Quaternion rotation)
        {
            var name = prefabName.Split('/');
            if (name.Length < 2) return false;
            var npc = NPCManager.CheckNPCInfos(name[1]);
            if (npc != null)
            {
                CuteLogger.Meow("Instantiating custom NPC: " + prefabName);
                if (name[0] == "SUR")
                {
                    PhotonNetwork.NOOU("SUR/" + (npc.IsBoss ? npc.bossBot.dummyNPC : npc.bot.dummyNPC), position, rotation, 0, new Il2CppReferenceArray<Il2CppSystem.Object>(new Il2CppSystem.Object[] { "CustomNPC", npc.name }));
                    return true;
                }
                else if (name[0] == "NPCTeamA")
                {
                    var go = PhotonNetwork.NOOU("SUR/" + (npc.IsBoss ? npc.bossBot.dummyNPC : npc.bot.dummyNPC), position, rotation, 0, new Il2CppReferenceArray<Il2CppSystem.Object>(new Il2CppSystem.Object[] { "CustomNPC", npc.name }));
                    if (npc.IsBoss) go.GetComponent<BossBot>().BLPBCBFEMNA = 1;
                    else go.GetComponent<Bot>().BLPBCBFEMNA = 1;
                    go.transform.Find("TeamTag").gameObject.tag = $"team2";
                    return true;
                }
                else if (name[0] == "NPCTeamB")
                {
                    var go = PhotonNetwork.NOOU("SUR/" + (npc.IsBoss ? npc.bossBot.dummyNPC : npc.bot.dummyNPC), position, rotation, 0, new Il2CppReferenceArray<Il2CppSystem.Object>(new Il2CppSystem.Object[] { "CustomNPC", npc.name }));
                    if (npc.IsBoss) go.GetComponent<BossBot>().BLPBCBFEMNA = 0;
                    else go.GetComponent<Bot>().BLPBCBFEMNA = 0;
                    go.transform.Find("TeamTag").gameObject.tag = $"team1";
                    return true;
                }
                else if (name[0] == "PlayerTeamA")
                {
                    var go = PhotonNetwork.NOOU("SUR/" + (npc.IsBoss ? npc.bossBot.dummyNPC : npc.bot.dummyNPC), position, rotation, 0, new Il2CppReferenceArray<Il2CppSystem.Object>(new Il2CppSystem.Object[] { "CustomNPC", npc.name }));
                    go.transform.Find("TeamTag").gameObject.tag = $"team2";
                    if (npc.IsBoss)
                    {
                        var b = go.GetComponent<BossBot>();
                        Transform cam = UnityEngine.Object.Instantiate(Helper.Console.BKBGMHGJODL, position, rotation);
                        cam.GetComponent<TPSCamera>().PFDKNPELPNC = b.transform;
                        cam.GetComponent<TPSCamera>().HELMMHBLCLH = b.transform.Find("TeamTag");
                        cam.GetComponent<BossCam>().OMIOOOFAJNP = b;
                        b.KHKDJLKGHDM = true;
                        b.BLPBCBFEMNA = 1;
                        b.IDLHJOOAMIA.enabled = false;
                    }
                    else
                    {
                        var b = go.GetComponent<Bot>();
                        Transform cam = UnityEngine.Object.Instantiate(Helper.Console.BKBGMHGJODL, position, rotation);
                        cam.GetComponent<TPSCamera>().PFDKNPELPNC = b.transform;
                        cam.GetComponent<TPSCamera>().HELMMHBLCLH = b.transform.Find("TeamTag");
                        cam.GetComponent<BossCam>().INHFFFAKNNG = b;
                        b.KHKDJLKGHDM = true;
                        b.BLPBCBFEMNA = 1;
                        b.IDLHJOOAMIA.enabled = false;
                    }
                    Helper.RoomMultiplayerMenu.CNLHJAICIBH = go;
                    return true;
                }
                else if (name[0] == "PlayerTeamB")
                {
                    var go = PhotonNetwork.NOOU("SUR/" + (npc.IsBoss ? npc.bossBot.dummyNPC : npc.bot.dummyNPC), position, rotation, 0, new Il2CppReferenceArray<Il2CppSystem.Object>(new Il2CppSystem.Object[] { "CustomNPC", npc.name }));
                    go.transform.Find("TeamTag").gameObject.tag = $"team1";
                    if (npc.IsBoss)
                    {
                        var b = go.GetComponent<BossBot>();
                        Transform cam = UnityEngine.Object.Instantiate(Helper.Console.BKBGMHGJODL, position, rotation);
                        cam.GetComponent<TPSCamera>().PFDKNPELPNC = b.transform;
                        cam.GetComponent<TPSCamera>().HELMMHBLCLH = b.transform.Find("TeamTag");
                        cam.GetComponent<BossCam>().OMIOOOFAJNP = b;
                        b.KHKDJLKGHDM = true;
                        b.BLPBCBFEMNA = 0;
                        b.IDLHJOOAMIA.enabled = false;
                    }
                    else
                    {
                        var b = go.GetComponent<Bot>();
                        Transform cam = UnityEngine.Object.Instantiate(Helper.Console.BKBGMHGJODL, position, rotation);
                        cam.GetComponent<TPSCamera>().PFDKNPELPNC = b.transform;
                        cam.GetComponent<TPSCamera>().HELMMHBLCLH = b.transform.Find("TeamTag");
                        cam.GetComponent<BossCam>().INHFFFAKNNG = b;
                        b.KHKDJLKGHDM = true;
                        b.BLPBCBFEMNA = 0;
                        b.IDLHJOOAMIA.enabled = false;
                    }
                    Helper.RoomMultiplayerMenu.CNLHJAICIBH = go;
                    return true;
                }
                else if (name[0] == "VS") PhotonNetwork.NOOU2("VS/PlayerImposter", position, rotation, 0, new Il2CppReferenceArray<Il2CppSystem.Object>(new Il2CppSystem.Object[] { "CustomNPC", npc.name }));
                else if (name[0] == "COOP") PhotonNetwork.NOOU2("COOP/Imposter", position, rotation, 0, new Il2CppReferenceArray<Il2CppSystem.Object>(new Il2CppSystem.Object[] { "CustomNPC", npc.name }));
            }
            return false;
        }
        public static void SetUpCustomNPC(string npcName, GameObject dummy, NPCType type)
        {
            CuteLogger.Meow("Loading custom NPC: " + npcName);
            FunNPCInfo npcInfo = CheckNPCInfos(npcName);
            if (npcInfo == null) return;
            //GameObject go = GameObject.Instantiate(Resources.Load("SUR/BossShadow").Cast<GameObject>());
            dummy.transform.Find("Model").gameObject.SetActive(false);
            GameObject newModel = GameObject.Instantiate(loadedBundles[npcInfo.name][0]);
            newModel.transform.SetParent(dummy.transform);
            dummy.transform.localScale = newModel.transform.localScale;
            newModel.transform.localScale = new Vector3(1f, 1f, 1f);
            newModel.transform.localPosition = new Vector3(0f, 0f, 0f);
            newModel.transform.localEulerAngles = Vector3.zero;
            SetLayerAllChildren(newModel.transform, 8);
            var mc = newModel.AddComponent<MecanimControl>();

            if (type == NPCType.BossBot)
            {
                FunBossBot npc = npcInfo.bossBot;
                var b = dummy.GetComponent<BossBot>();
                var na = newModel.GetComponent<NavMeshAgent>();
                if (na != null)
                {
                    dummy.GetComponent<NavMeshAgent>().radius = 0.1f;
                    dummy.GetComponent<NavMeshAgent>().baseOffset = na.baseOffset;
                    na.enabled = false;
                }
                bool customHitboxes = false;
                foreach (Collider collider in newModel.transform.GetComponentsInChildren<Collider>())
                {
                    if (collider.isTrigger && collider.name.Contains("Hitbox"))
                    {
                        if (!customHitboxes)
                        {
                            foreach (HitboxBoss hitbox in dummy.GetComponentsInChildren<HitboxBoss>())
                            {
                                GameObject.Destroy(hitbox.gameObject);
                            }
                            customHitboxes = true;
                        }
                        float mlp = Convert.ToSingle(collider.name.Split(' ')[1]);
                        var h = collider.gameObject.AddComponent<HitboxBoss>();
                        h.DHGDMKPLBJH = 30 * mlp;
                        h.KOKDGLGNJEN = b;
                    }
                }
                b.NNIBHOMNJHM = npc.hp;
                b.DHHNMAIFIFB = npc.speed;
                b.CJKBHAHOLMJ = npc.damage;
                b.GKFPAHIPDOM = npc.runSpeed;
                b.IPOFBAGEDJK = npc.attackRange;
                b.LIBBFHCIHID = npc.attackSpeed;
                b.ELPJDJFCCNJ = mc;
            }
            if (type == NPCType.Bot)
            {
                //Bot.IPOFBAGEDJK range
                //Bot.LIBBFHCIHID attack time
                FunBot npc = npcInfo.bot;
                var b = dummy.GetComponent<Bot>();
                var na = newModel.GetComponent<NavMeshAgent>();
                if (na != null)
                {
                    dummy.GetComponent<NavMeshAgent>().radius = 0.1f;
                    dummy.GetComponent<NavMeshAgent>().baseOffset = na.baseOffset;
                    na.enabled = false;
                }
                GameObject deadPrefab = null;
                bool customHitboxes = false;
                foreach (Collider collider in newModel.transform.GetComponentsInChildren<Collider>())
                {
                    if (collider.isTrigger && collider.name.Contains("Hitbox"))
                    {
                        if (!customHitboxes)
                        {
                            foreach (HitBoxBot hitbox in dummy.GetComponentsInChildren<HitBoxBot>())
                            {
                                GameObject.Destroy(hitbox.gameObject);
                            }
                            customHitboxes = true;
                        }
                        float mlp = Convert.ToSingle(collider.name.Split(' ')[1]);
                        var h = collider.gameObject.AddComponent<HitBoxBot>();
                        h.DHGDMKPLBJH = 30 * mlp;
                        h.KOKDGLGNJEN = b;
                        h.gameObject.transform.SetParent(dummy.transform, true);
                    }
                }
                if (!npc.useRagdoll)
                {
                    deadPrefab = loadedBundles[npcInfo.name][1];
                    b.OGJJNCKKILE = deadPrefab.transform;
                    if (b.OGJJNCKKILE.gameObject.GetComponent<DeadBot>() == null) b.OGJJNCKKILE.gameObject.AddComponent<DeadBot>().CPBLCBMLFAJ = 15f;
                }
                else
                {
                    deadPrefab = loadedBundles[npcInfo.name][1];
                    b.OGJJNCKKILE = deadPrefab.transform;
                    if (b.OGJJNCKKILE.gameObject.GetComponent<WaitForDestroy>() == null) b.OGJJNCKKILE.gameObject.AddComponent<WaitForDestroy>().PHADIEJAKDA = 15f;
                }
                if (npc.useRage)
                {
                    var bm = dummy.GetComponent<BotMorpher>();
                    if (bm != null)
                    {
                        bm.DHHNMAIFIFB = npc.rageRunSpeed;
                        bm.HBPNKDOBIFO = npc.rageRequiredHp;
                        bm.LIBBFHCIHID = npc.rageAttackTime;
                        bm.AMEBGBAKKAL = "RageRun";
                        bm.HNJEODLDKHF = "RageAttack";
                    }
                    else
                    {
                        bm = dummy.AddComponent<BotMorpher>();
                        bm.DHHNMAIFIFB = npc.rageRunSpeed;
                        bm.AMEBGBAKKAL = "RageRun";
                        bm.HNJEODLDKHF = "RageAttack";
                        bm.INHFFFAKNNG = b;
                        bm.HBPNKDOBIFO = npc.rageRequiredHp;
                        bm.LIBBFHCIHID = npc.rageAttackTime;
                    }
                }
                b.CJKBHAHOLMJ = npc.damage;
                b.DHHNMAIFIFB = npc.speed;
                b.NNIBHOMNJHM = npc.hp;
                b.IPOFBAGEDJK = npc.attackRange;
                b.LIBBFHCIHID = npc.attackSpeed;
                b.ELPJDJFCCNJ = mc;
            }
            if (type == NPCType.CustardBot)
            {
                FunCustardBot npc = npcInfo.custardBot;
                var cb = dummy.GetComponent<CustardBot>();
                var na = newModel.GetComponent<NavMeshAgent>();
                if (na != null)
                {
                    dummy.GetComponent<NavMeshAgent>().radius = 0.1f;
                    dummy.GetComponent<NavMeshAgent>().baseOffset = na.baseOffset;
                    na.enabled = false;
                }
                cb.CJKBHAHOLMJ = npc.damage;
                cb.DHHNMAIFIFB = npc.speed;
                cb.GKFPAHIPDOM = npc.runSpeed;
                cb.ELPJDJFCCNJ = mc;
                cb.KAMFBFKCKLI = newModel.transform.GetComponentInChildren<AudioSource>();
                cb.FIKNJKJMAHO = cb.KAMFBFKCKLI.clip;
            }
            if (type == NPCType.PlayerMonster)
            {
                FunPlayerMonster npc = npcInfo.playerMonster;
                var pm = dummy.GetComponent<PlayerMonster>();
                pm.OMFJFIPPGPE = Convert.ToInt32(npc.damage);
                pm.GFHPOIPBLGE.GKJOHCHABPI.HKCDMBALAAK.WalkSpeed = npc.speed;
                pm.GFHPOIPBLGE.GKJOHCHABPI.HKCDMBALAAK.RunSpeed = npc.runSpeed;
                pm.GFHPOIPBLGE.GKJOHCHABPI.AALHECCKHFD.baseHeight = npc.jumpSpeed;
                pm.ELPJDJFCCNJ = mc;
            }
            var a = newModel.GetComponent<Animator>();
            if (a != null)
            {
                foreach (AnimationClip clip in newModel.GetComponent<Animator>().runtimeAnimatorController.animationClips)
                {
                    if (clip.name == "Idle") mc.AMMLJEHHILA(clip, clip.name, 1f, clip.isLooping ? WrapMode.Loop : WrapMode.Clamp);
                }
                foreach (AnimationClip clip in newModel.GetComponent<Animator>().runtimeAnimatorController.animationClips)
                {
                    if (clip.name != "Idle") mc.AMMLJEHHILA(clip, clip.name, 1f, clip.isLooping ? WrapMode.Loop : WrapMode.Clamp);
                }
            }
            CuteLogger.Meow("7");
            var comp = dummy.GetComponent<NavMeshAgent>();
            if (comp != null) { comp.enabled = comp.gameObject.GetComponent<PhotonView>().isMine; }
            dummy.AddComponent<CustomNPC>();
            dummy.name = npcInfo.name;
            CuteLogger.Meow("Loaded.");
        }
        public static IEnumerator LoadAllBundles()
        {
            foreach (FunNPCInfo npcInfo in NPCInfos)
            {
                if (!loadedBundles.ContainsKey(npcInfo.name))
                {
                    var assetBundleCreateRequest = Il2CppAssetBundleManager.LoadFromFile(npcInfo.path + @"\bundle");
                    if (npcInfo.bot != null)
                    {
                        if (npcInfo.bot.useRagdoll)
                        {
                            loadedBundles.Add(npcInfo.name, new GameObject[]
                            {
                                assetBundleCreateRequest.Load<GameObject>(npcInfo.name),
                                assetBundleCreateRequest.Load<GameObject>(npcInfo.name + "(Ragdoll)"),
                            });
                        }
                        else
                        {
                            loadedBundles.Add(npcInfo.name, new GameObject[]
                            {
                                assetBundleCreateRequest.Load<GameObject>(npcInfo.name),
                                assetBundleCreateRequest.Load<GameObject>(npcInfo.name + "(Dead)"),
                            });
                        }
                    }
                    if (npcInfo.bossBot != null)
                    {
                        loadedBundles.Add(npcInfo.name, new GameObject[]
                        {
                                assetBundleCreateRequest.Load<GameObject>(npcInfo.name),
                                assetBundleCreateRequest.Load<GameObject>(npcInfo.name + "(Dead)"),
                        });
                    }
                    assetBundleCreateRequest.Unload(false);
                }
                else if (loadedBundles.TryGetValue(npcInfo.name, out var bundle))
                {
                    if (bundle == null)
                    {
                        loadedBundles.Remove(npcInfo.name);
                        var assetBundleCreateRequest = Il2CppAssetBundleManager.LoadFromFile(npcInfo.path + @"\bundle");
                        yield return assetBundleCreateRequest;
                        if (npcInfo.bot != null)
                        {
                            if (npcInfo.bot.useRagdoll)
                            {
                                loadedBundles.Add(npcInfo.name, new GameObject[]
                                {
                                assetBundleCreateRequest.Load<GameObject>(npcInfo.name),
                                assetBundleCreateRequest.Load<GameObject>(npcInfo.name + "(Ragdoll)"),
                                });
                            }
                            else
                            {
                                loadedBundles.Add(npcInfo.name, new GameObject[]
                                {
                                assetBundleCreateRequest.Load<GameObject>(npcInfo.name),
                                assetBundleCreateRequest.Load<GameObject>(npcInfo.name + "(Dead)"),
                                });
                            }
                        }
                        if (npcInfo.bossBot != null)
                        {
                            loadedBundles.Add(npcInfo.name, new GameObject[]
                            {
                                assetBundleCreateRequest.Load<GameObject>(npcInfo.name),
                                assetBundleCreateRequest.Load<GameObject>(npcInfo.name + "(Dead)"),
                            });
                        }
                        assetBundleCreateRequest.Unload(false);
                    }
                }
            }
        }
        public static void LoadBundles()
        {
            loadedBundles.Clear();
            CustomWeapons.loadedBundles.Clear();
            MelonCoroutines.Start(LoadAllBundles());
            MelonCoroutines.Start(CustomWeapons.LoadBundles());
        }
        public static IEnumerator AddNPCInfos(Volume sandboxConsole)
        {            
            yield return MelonCoroutines.Start(LoadNPCIcons());
            string customNPCs = PhotonNetwork.room.customProperties["customNPCs"] != null ? PhotonNetwork.room.customProperties["customNPCs"].ToString() : "";
            if (customNPCs != null && customNPCs != "")
            {
                foreach (string npcName in customNPCs.Split('|'))
                {
                    if (npcName != null && npcName != "")
                    {
                        var npc = CheckNPCInfos(npcName);
                        if (npc != null)
                        {
                            sandboxConsole.PDKPIOHFCCK[0].options.Add(new Volume.option() { image = npc.iconSprite, optionName = npc.name, resourcePath = "NPCTeamA/" + npc.name });
                            sandboxConsole.PDKPIOHFCCK[1].options.Add(new Volume.option() { image = npc.iconSprite, optionName = npc.name, resourcePath = "NPCTeamB/" + npc.name });
                            sandboxConsole.PDKPIOHFCCK[2].options.Add(new Volume.option() { image = npc.iconSprite, optionName = npc.name, resourcePath = "PlayerTeamA/" + npc.name });
                            sandboxConsole.PDKPIOHFCCK[3].options.Add(new Volume.option() { image = npc.iconSprite, optionName = npc.name, resourcePath = "PlayerTeamB/" + npc.name });
                        }
                    }
                }
            }
            if (customNPCs == "")
            {
                foreach (FunNPCInfo npc in NPCInfos)
                {
                    sandboxConsole.PDKPIOHFCCK[0].options.Add(new Volume.option() { image = npc.iconSprite, optionName = npc.name, resourcePath = "NPCTeamA/" + npc.name });
                    sandboxConsole.PDKPIOHFCCK[1].options.Add(new Volume.option() { image = npc.iconSprite, optionName = npc.name, resourcePath = "NPCTeamB/" + npc.name });
                    sandboxConsole.PDKPIOHFCCK[2].options.Add(new Volume.option() { image = npc.iconSprite, optionName = npc.name, resourcePath = "PlayerTeamA/" + npc.name });
                    sandboxConsole.PDKPIOHFCCK[3].options.Add(new Volume.option() { image = npc.iconSprite, optionName = npc.name, resourcePath = "PlayerTeamB/" + npc.name });
                }
            }
            sandboxConsole.ResetOptions();
            yield return null;
        }
        public static FunNPCInfo CheckNPCInfos(string npcName)
        {
            foreach (FunNPCInfo npc in NPCInfos)
            {
                if (npc.name == npcName) { return npc; }
            }
            return null;
        }
        public static IEnumerator LoadNPCIcons()
        {
            foreach (FunNPCInfo npc in NPCInfos)
            {
                string iconPath = npc.path + @"\icon.png";
                if (File.Exists(iconPath))
                {
                    WWW www = new WWW(@"file://" + iconPath);
                    yield return www;
                    npc.iconSprite = Loader.ConvertTextureToSprite(www.texture, new Vector2(.5f, .5f));
                }
            }
        }
        public static void Init()
        {
            Directory.CreateDirectory(NPCInfoDirectory);
            var npcFiles = Helper.GetAllDirectories(NPCInfoDirectory);
            for (int i = 0; i < npcFiles.Count; i++)
            {
                string bundlePath = npcFiles[i].FullName + @"\bundle";
                if (!File.Exists(bundlePath))
                {
                    return;
                }
                var json = File.ReadAllText(npcFiles[i].FullName + @"\npc.json");
                var npc = JSON.Load(json).Make<FunNPCInfo>();
                npc.path = npcFiles[i].FullName;
                NPCInfos.Add(npc);
            }
        }
    }

    public class CustomBot
    {
    }
}
