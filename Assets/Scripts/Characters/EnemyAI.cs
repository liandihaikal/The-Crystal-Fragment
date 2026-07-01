using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace JRPGBattle
{
    /// <summary>
    /// Hasil keputusan AI: skill apa (null = basic attack) dan target siapa.
    /// </summary>
    public struct AIDecision
    {
        public SkillData skill; // null berarti serangan dasar
        public BattleUnit target;
    }

    /// <summary>
    /// AI musuh yang sederhana namun bisa dikembangkan lebih lanjut.
    /// Logika saat ini: kadang pakai skill (kalau MP cukup), kadang basic attack,
    /// target dipilih acak dari pihak lawan yang masih hidup.
    /// Prioritaskan heal ke ally dengan HP rendah kalau enemy punya skill Heal.
    /// </summary>
    public static class EnemyAI
    {
        public static AIDecision ChooseAction(BattleUnit self, List<BattleUnit> enemyAllies, List<BattleUnit> playerParty)
        {
            var aliveAllies = enemyAllies.Where(u => u.IsAlive).ToList();
            var alivePlayers = playerParty.Where(u => u.IsAlive).ToList();

            // Prioritas 1: kalau ada ally (termasuk diri sendiri) HP kritis dan punya skill heal, pakai itu
            var lowHpAlly = aliveAllies.OrderBy(u => (float)u.currentHP / u.data.maxHP).FirstOrDefault();
            if (lowHpAlly != null && (float)lowHpAlly.currentHP / lowHpAlly.data.maxHP < 0.35f)
            {
                var healSkill = self.data.skills.FirstOrDefault(s =>
                    s.effectType == SkillEffectType.Heal && s.mpCost <= self.currentMP);

                if (healSkill != null)
                {
                    return new AIDecision { skill = healSkill, target = lowHpAlly };
                }
            }

            // Prioritas 2: kadang pakai skill offensive sesuai skillUsageChance
            bool wantsToUseSkill = Random.value <= self.data.skillUsageChance;
            var offensiveSkills = self.data.skills
                .Where(s => s.effectType == SkillEffectType.Damage && s.mpCost <= self.currentMP)
                .ToList();

            if (wantsToUseSkill && offensiveSkills.Count > 0 && alivePlayers.Count > 0)
            {
                var skill = offensiveSkills[Random.Range(0, offensiveSkills.Count)];
                var target = PickTarget(skill.targetType, alivePlayers, aliveAllies, self);
                return new AIDecision { skill = skill, target = target };
            }

            // Fallback: basic attack ke target acak
            if (alivePlayers.Count > 0)
            {
                var target = alivePlayers[Random.Range(0, alivePlayers.Count)];
                return new AIDecision { skill = null, target = target };
            }

            return new AIDecision { skill = null, target = null };
        }

        private static BattleUnit PickTarget(TargetType type, List<BattleUnit> enemiesOfSelf, List<BattleUnit> alliesOfSelf, BattleUnit self)
        {
            switch (type)
            {
                case TargetType.Self:
                    return self;
                case TargetType.SingleAlly:
                    return alliesOfSelf.Count > 0 ? alliesOfSelf[Random.Range(0, alliesOfSelf.Count)] : self;
                default:
                    return enemiesOfSelf.Count > 0 ? enemiesOfSelf[Random.Range(0, enemiesOfSelf.Count)] : null;
            }
        }
    }
}
