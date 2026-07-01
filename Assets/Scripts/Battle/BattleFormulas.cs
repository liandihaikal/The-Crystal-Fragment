using UnityEngine;

namespace JRPGBattle
{
    /// <summary>
    /// Hasil dari satu aksi (dipakai UI untuk menampilkan angka damage/heal, crit, dsb).
    /// </summary>
    public struct ActionResult
    {
        public BattleUnit target;
        public int amount;
        public bool isCrit;
        public bool isMiss;
        public SkillEffectType effectType;
    }

    /// <summary>
    /// Kumpulan formula battle. Static agar gampang dipanggil dari mana saja
    /// dan gampang di-tweak/balance tanpa menyentuh logika state machine.
    /// </summary>
    public static class BattleFormulas
    {
        /// <summary>Formula damage dasar: attack skill + attack unit dikurangi defense target.</summary>
        public static int CalculatePhysicalDamage(BattleUnit attacker, BattleUnit target, SkillData skill)
        {
            float baseDamage = (attacker.EffectiveAttack * skill.scaling) + skill.power;
            float mitigated = baseDamage - (target.EffectiveDefense * 0.5f);
            int finalDamage = Mathf.RoundToInt(Mathf.Max(1, mitigated));
            return finalDamage;
        }

        public static int CalculateHeal(BattleUnit caster, SkillData skill)
        {
            float baseHeal = (caster.data.magic * skill.scaling) + skill.power;
            return Mathf.RoundToInt(Mathf.Max(1, baseHeal));
        }

        public static bool RollAccuracy(SkillData skill)
        {
            return Random.value <= skill.accuracy;
        }

        public static bool RollCrit(SkillData skill)
        {
            return Random.value <= skill.critChance;
        }

        /// <summary>
        /// Menjalankan satu skill dari caster ke satu target dan mengembalikan hasilnya.
        /// Untuk skill AoE, panggil ini berulang per target di BattleManager.
        /// </summary>
        public static ActionResult ExecuteSkillOnTarget(BattleUnit caster, BattleUnit target, SkillData skill)
        {
            var result = new ActionResult
            {
                target = target,
                effectType = skill.effectType
            };

            if (!RollAccuracy(skill))
            {
                result.isMiss = true;
                return result;
            }

            switch (skill.effectType)
            {
                case SkillEffectType.Damage:
                    int dmg = CalculatePhysicalDamage(caster, target, skill);
                    bool crit = RollCrit(skill);
                    if (crit) dmg = Mathf.RoundToInt(dmg * 1.5f);
                    target.TakeDamage(dmg);
                    result.amount = dmg;
                    result.isCrit = crit;
                    break;

                case SkillEffectType.Heal:
                case SkillEffectType.Cure:
                    int heal = CalculateHeal(caster, skill);
                    target.Heal(heal);
                    result.amount = heal;
                    break;

                case SkillEffectType.BuffAttack:
                    target.attackModifier += 0.3f;
                    break;

                case SkillEffectType.BuffDefense:
                    target.defenseModifier += 0.3f;
                    break;

                case SkillEffectType.DebuffAttack:
                    target.attackModifier = Mathf.Max(0.1f, target.attackModifier - 0.3f);
                    break;

                case SkillEffectType.DebuffDefense:
                    target.defenseModifier = Mathf.Max(0.1f, target.defenseModifier - 0.3f);
                    break;
            }

            return result;
        }

        private static SkillData _basicAttackSkill;

        /// <summary>Serangan dasar (command "Attack"), tidak menggunakan MP, power tetap kecil & scaling 1.0.</summary>
        public static ActionResult ExecuteBasicAttack(BattleUnit attacker, BattleUnit target)
        {
            if (_basicAttackSkill == null)
            {
                _basicAttackSkill = ScriptableObject.CreateInstance<SkillData>();
                _basicAttackSkill.power = 0;
                _basicAttackSkill.scaling = 1.0f;
                _basicAttackSkill.accuracy = 0.95f;
                _basicAttackSkill.critChance = 0.05f;
                _basicAttackSkill.effectType = SkillEffectType.Damage;
            }

            return ExecuteSkillOnTarget(attacker, target, _basicAttackSkill);
        }
    }
}
