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

namespace FunPlusEssentials.CustomContent
{
    public static class ListExtensions
    {
        public static void MoveItemAtIndexToFront(UnhollowerBaseLib.Il2CppReferenceArray<Il2CppSystem.Object> list, int index)
        {
            var item = list[index];
            for (int i = index; i > 0; i--)
                list[i] = list[i - 1];
            list[0] = item;
        }
    }

    public class NPCInfo
    {
        public string name;
        public string version;
        public Sprite iconSprite;
        public string path;
        public Bot bot;
        public BossBot bossBot;
        public CustardBot custardBot;
        public PlayerMonster playerMonster;
        public bool IsBoss => bossBot != null && bot == null;


        public class NPC
        {
            public float hp;
            public float damage;
            public float speed;
        }

        public class Bot : NPC
        {
            public bool useRagdoll;
            public string dummyNPC;
        }
        public class BossBot : NPC
        {
            public float runSpeed;
            public string dummyNPC;
        }
        public class CustardBot : NPC
        {
            public float runSpeed;
            public float jumpSpeed;
        }
        public class PlayerMonster : CustardBot
        {
        }
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
        public static List<NPCInfo> NPCInfos = new List<NPCInfo>();
        //public static List<GameObject> npcPool = new List<GameObject>();
        public static Dictionary<string, Il2CppAssetBundle> loadedBundles = new Dictionary<string, Il2CppAssetBundle>();

        public static void SpawnCustomNPC(string npcName, GameObject dummy, NPCType type)
        {
            NPCInfo npcInfo = CheckNPCInfos(npcName);
            if (npcInfo == null) return;
            //GameObject go = GameObject.Instantiate(Resources.Load("SUR/BossShadow").Cast<GameObject>());
            dummy.transform.Find("Model").gameObject.SetActive(false);
            GameObject newModel = GameObject.Instantiate(loadedBundles[npcInfo.name].LoadAsset<GameObject>(npcInfo.name));
            newModel.transform.SetParent(dummy.transform);
            dummy.transform.localScale = newModel.transform.localScale;
            newModel.transform.localScale = new Vector3(1f, 1f, 1f);
            newModel.transform.localPosition = new Vector3(0f, 0f, 0f);
            newModel.transform.localEulerAngles = Vector3.zero;
            var na = newModel.GetComponent<NavMeshAgent>();
            if (na != null) dummy.GetComponent<NavMeshAgent>().baseOffset = na.baseOffset;
            var mc = newModel.AddComponent<MecanimControl>();

            if (type == NPCType.BossBot)
            {
                NPCInfo.BossBot npc = npcInfo.bossBot;
                var b = dummy.GetComponent<BossBot>();
                bool customHitboxes = false;
                foreach (Collider collider in newModel.transform.GetComponentsInChildren<Collider>())
                {
                    if (collider.isTrigger && collider.name.Contains("Hitbox"))
                    {
                        if (customHitboxes && !customHitboxes)
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
                b.ELPJDJFCCNJ = mc;
            }           
            if (type == NPCType.Bot)
            {
                NPCInfo.Bot npc = npcInfo.bot;
                var b = dummy.GetComponent<Bot>();
                GameObject deadPrefab = null;
                bool customHitboxes = false;
                foreach (Collider collider in newModel.transform.GetComponentsInChildren<Collider>())
                {
                    if (collider.isTrigger && collider.name.Contains("Hitbox"))
                    {
                        if (customHitboxes && !customHitboxes)
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
                    }
                }                
                if (!npc.useRagdoll)
                {
                    deadPrefab = loadedBundles[npcInfo.name].LoadAsset<GameObject>(npcInfo.name + "(Dead)");
                    b.OGJJNCKKILE = deadPrefab.transform;
                    if (b.OGJJNCKKILE.gameObject.GetComponent<DeadBot>() == null) b.OGJJNCKKILE.gameObject.AddComponent<DeadBot>().CPBLCBMLFAJ = 15f;
                }
                else
                {
                    deadPrefab = loadedBundles[npcInfo.name].LoadAsset<GameObject>(npcInfo.name + "(Ragdoll)");
                    b.OGJJNCKKILE = deadPrefab.transform;
                    if (b.OGJJNCKKILE.gameObject.GetComponent<WaitForDestroy>() == null) b.OGJJNCKKILE.gameObject.AddComponent<WaitForDestroy>().PHADIEJAKDA = 15f;
                }
                b.CJKBHAHOLMJ = npc.damage;
                b.DHHNMAIFIFB = npc.speed;
                b.NNIBHOMNJHM = npc.hp;
                b.ELPJDJFCCNJ = mc;
            }
            if (type == NPCType.CustardBot)
            {
                NPCInfo.CustardBot npc = npcInfo.custardBot;
                var cb = dummy.GetComponent<CustardBot>();
                cb.CJKBHAHOLMJ = npc.damage;
                cb.DHHNMAIFIFB = npc.speed;
                cb.GKFPAHIPDOM = npc.runSpeed;
                cb.ELPJDJFCCNJ = mc;
                cb.KAMFBFKCKLI = newModel.transform.GetComponentInChildren<AudioSource>();
                cb.FIKNJKJMAHO = cb.KAMFBFKCKLI.clip;
            }
            if (type == NPCType.PlayerMonster)
            {
                NPCInfo.PlayerMonster npc = npcInfo.playerMonster;
                var pm = dummy.GetComponent<PlayerMonster>();
                pm.OMFJFIPPGPE = Convert.ToInt32(npc.damage);
                pm.GFHPOIPBLGE.GKJOHCHABPI.HKCDMBALAAK.WalkSpeed = npc.speed;
                pm.GFHPOIPBLGE.GKJOHCHABPI.HKCDMBALAAK.RunSpeed = npc.runSpeed;
                pm.GFHPOIPBLGE.GKJOHCHABPI.AALHECCKHFD.baseHeight = npc.jumpSpeed;
                pm.ELPJDJFCCNJ = mc;
            }
            foreach (AnimationClip clip in newModel.GetComponent<Animator>().runtimeAnimatorController.animationClips)
            {
                if (clip.name == "Idle") mc.AMMLJEHHILA(clip, clip.name, 1f, clip.isLooping ? WrapMode.Loop : WrapMode.Clamp);
            }
            foreach (AnimationClip clip in newModel.GetComponent<Animator>().runtimeAnimatorController.animationClips)
            {
                if (clip.name != "Idle") mc.AMMLJEHHILA(clip, clip.name, 1f, clip.isLooping ? WrapMode.Loop : WrapMode.Clamp);
            }

            dummy.AddComponent<CustomNPC>();
            dummy.name = npcInfo.name;
        }
        public static void LoadBundles()
        {
            foreach (NPCInfo npcInfo in NPCInfos)
            {
                if (!loadedBundles.ContainsKey(npcInfo.name))
                {
                    loadedBundles.Add(npcInfo.name, BundleManager.LoadBundle(npcInfo.path + @"\bundle"));
                }
                else if (loadedBundles.TryGetValue(npcInfo.name, out var bundle))
                {
                    if (bundle == null)
                    {
                        loadedBundles.Remove(npcInfo.name);
                        loadedBundles.Add(npcInfo.name, BundleManager.LoadBundle(npcInfo.path + @"\bundle"));
                    }
                }
            }
        }
        public static IEnumerator AddNPCInfos(Volume sandboxConsole)
        {
            yield return new WaitForSeconds(1);
            foreach (NPCInfo npc in NPCInfos)
            {
                GameObject.FindObjectOfType<Volume>().PDKPIOHFCCK.Add(new Volume.catagory() { catagoryName = "Test1" });
                sandboxConsole.PDKPIOHFCCK[0].options.Add(new Volume.option() { image = npc.iconSprite, optionName = npc.name, resourcePath = npc.IsBoss ? npc.bossBot.dummyNPC : npc.bot.dummyNPC });
                sandboxConsole.PDKPIOHFCCK[1].options.Add(new Volume.option() { image = npc.iconSprite, optionName = npc.name, resourcePath = npc.IsBoss ? npc.bossBot.dummyNPC : npc.bot.dummyNPC });
            }
            yield return null;
        }
        public static NPCInfo CheckNPCInfos(string npcName)
        {
            foreach (NPCInfo npc in NPCInfos)
            {
                if (npc.name == npcName) { return npc; }
            }
            return null;
        }
        public static IEnumerator LoadNPCIcons()
        {
            foreach (NPCInfo npc in NPCInfos)
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
                var npc = JSON.Load(json).Make<NPCInfo>();
                npc.path = npcFiles[i].FullName;
                NPCInfos.Add(npc);
            }
        }
    }

    public class CustomBot
    {
    }
}
