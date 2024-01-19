using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FunPlusEssentials
{
    public abstract class FunNPC
    {
        public string dummyNPC;
        public float hp;
        public float damage;
        public float speed;
    }

    public class FunBot : FunNPC
    {
        public bool useRagdoll;
        public float attackRange;
        public float attackSpeed;
        public bool useRage;
        public int rageRequiredHp;
        public float rageAttackTime;
        public float rageRunSpeed;
    }
    public class FunBossBot : FunNPC
    {
        public float runSpeed;
        public float attackRange;
        public float attackSpeed;
    }
    public class FunCustardBot : FunNPC
    {
        public float runSpeed;
        public float jumpSpeed;
    }
    public class FunPlayerMonster : FunCustardBot
    {
    }
}
