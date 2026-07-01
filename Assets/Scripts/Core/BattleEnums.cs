using System;

namespace JRPGBattle
{
    /// <summary>
    /// Menentukan siapa saja yang bisa jadi target dari sebuah skill.
    /// </summary>
    public enum TargetType
    {
        SingleEnemy,
        AllEnemies,
        SingleAlly,
        AllAllies,
        Self
    }

    /// <summary>
    /// Jenis efek yang dihasilkan sebuah skill.
    /// </summary>
    public enum SkillEffectType
    {
        Damage,
        Heal,
        BuffAttack,
        BuffDefense,
        DebuffAttack,
        DebuffDefense,
        Cure // menghilangkan status negatif
    }

    /// <summary>
    /// Command dasar yang bisa dipilih pemain tiap giliran.
    /// </summary>
    public enum BattleCommand
    {
        Attack,
        Skill,
        Item,
        Defend,
        Run
    }

    /// <summary>
    /// Status/hasil akhir dari sebuah battle.
    /// </summary>
    public enum BattleResult
    {
        None,
        Victory,
        Defeat,
        Ran
    }
}
