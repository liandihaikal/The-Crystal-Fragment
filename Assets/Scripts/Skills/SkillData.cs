using UnityEngine;

namespace JRPGBattle
{
    /// <summary>
    /// Data statis untuk sebuah skill/jurus. Dibuat sebagai asset lewat
    /// menu: Assets > Create > Battle > Skill Data
    /// </summary>
    [CreateAssetMenu(fileName = "NewSkill", menuName = "Battle/Skill Data", order = 2)]
    public class SkillData : ScriptableObject
    {
        [Header("Info Dasar")]
        public string skillName = "New Skill";
        [TextArea] public string description;
        public Sprite icon;

        [Header("Cost")]
        public int mpCost = 0;

        [Header("Target & Efek")]
        public TargetType targetType = TargetType.SingleEnemy;
        public SkillEffectType effectType = SkillEffectType.Damage;

        [Tooltip("Basis kekuatan skill, dipakai dalam formula damage/heal")]
        public int power = 10;

        [Tooltip("Multiplier tambahan dari status caster (mis. 1.0 = 100% dari attack)")]
        public float scaling = 1.0f;

        [Header("Peluang & Akurasi")]
        [Range(0f, 1f)] public float accuracy = 1.0f;
        [Range(0f, 1f)] public float critChance = 0.05f;

        [Header("Animasi (opsional)")]
        public string animationTrigger;
        public GameObject vfxPrefab;
    }
}
