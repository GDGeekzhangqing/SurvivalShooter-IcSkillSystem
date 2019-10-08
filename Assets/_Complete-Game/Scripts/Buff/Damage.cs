//创建者:Icarus
//手动滑稽,滑稽脸
//ヾ(•ω•`)o
//https://www.ykls.app
//2019年09月28日-14:40
//Assembly-CSharp

using CabinIcarus.IcSkillSystem.Expansion.Runtime.Buffs.Components;
using CabinIcarus.IcSkillSystem.Runtime.Buffs.Entitys;

namespace Scripts.Buff
{
    public class Damage:IDamageBuff
    {
        public float Value { get; set; }
        public IEntity Maker { get; set; }
        public int Type { get; set; }
    }
    
    public struct DamageStruct:IDamageStructBuff
    {
        public float Value { get; set; }
        public IEntity Maker { get; set; }
        public int Type { get; set; }
        public int ID { get; }
    }
}