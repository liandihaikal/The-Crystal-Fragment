namespace JRPGBattle
{
    /// <summary>
    /// Fase-fase battle. Dipakai BattleManager untuk broadcast state ke UI
    /// lewat event OnPhaseChanged, mirip pola state-machine yang dipakai
    /// di UI cashier (parking system) — cuma di sini fase-nya battle turn.
    /// </summary>
    public enum BattlePhase
    {
        Intro,          // battle baru mulai, animasi masuk
        StartRound,     // membangun urutan giliran baru
        PlayerCommand,  // menunggu player pilih command (Attack/Skill/Item/Defend/Run)
        PlayerTargeting,// menunggu player pilih target
        EnemyThinking,  // AI musuh sedang memilih aksi
        ExecutingAction,// aksi (attack/skill) sedang dijalankan & dianimasikan
        CheckEnd,       // cek apakah battle sudah selesai
        Victory,
        Defeat,
        Ran
    }
}
