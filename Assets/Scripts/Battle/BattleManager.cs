using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace JRPGBattle
{
    /// <summary>
    /// Inti sistem battle turn-based ala JRPG.
    /// Cara pakai singkat:
    ///  1. Taruh script ini di sebuah GameObject kosong di scene battle.
    ///  2. Isi playerUnits & enemyUnits (BattleUnit) yang sudah ada di scene,
    ///     ATAU isi playerData/enemyData + spawnPoints untuk di-spawn otomatis.
    ///  3. Panggil StartBattle() dari script lain (mis. trigger encounter).
    ///  4. Hubungkan UI ke method publik: SelectCommand, SelectSkill, SelectTarget.
    ///  5. Dengarkan event (OnPhaseChanged, OnTurnStart, OnActionResolved, OnBattleEnded)
    ///     untuk update tampilan/animasi.
    /// </summary>
    public class BattleManager : MonoBehaviour
    {
        [Header("Setup Manual (kalau unit sudah ada di scene)")]
        public List<BattleUnit> playerUnits = new List<BattleUnit>();
        public List<BattleUnit> enemyUnits = new List<BattleUnit>();

        [Header("Setup Otomatis (opsional, kalau mau di-spawn dari data)")]
        public GameObject battleUnitPrefab; // prefab kosong yang punya component BattleUnit
        public List<CharacterData> playerData = new List<CharacterData>();
        public List<CharacterData> enemyData = new List<CharacterData>();
        public List<Transform> playerSpawnPoints = new List<Transform>();
        public List<Transform> enemySpawnPoints = new List<Transform>();

        [Header("Pengaturan")]
        [Tooltip("Delay antar step biar ada waktu animasi/VFX main (detik)")]
        public float stepDelay = 0.6f;

        // ---- State internal ----
        private TurnManager turnManager = new TurnManager();
        private BattleUnit currentUnit;
        private BattlePhase currentPhase;

        // Pilihan player yang sedang menunggu diisi lewat UI
        private BattleCommand? selectedCommand;
        private SkillData selectedSkill;
        private BattleUnit selectedTarget;
        private List<BattleUnit> selectedTargets; // untuk AoE

        // ---- Events untuk UI ----
        public event Action<BattlePhase> OnPhaseChanged;
        public event Action<BattleUnit> OnTurnStart;
        public event Action<BattleUnit, ActionResult> OnActionResolved;
        public event Action<BattleResult> OnBattleEnded;
        public event Action<BattleUnit> OnUnitDied;

        public BattlePhase CurrentPhase => currentPhase;
        public BattleUnit CurrentUnit => currentUnit;
        public IReadOnlyList<BattleUnit> PlayerUnits => playerUnits;
        public IReadOnlyList<BattleUnit> EnemyUnits => enemyUnits;

        private bool battleActive;

        // =========================================================
        //  ENTRY POINT
        // =========================================================

        public void StartBattle()
        {
            if (battleUnitPrefab != null && (playerData.Count > 0 || enemyData.Count > 0))
            {
                SpawnUnitsFromData();
            }

            foreach (var u in playerUnits) u.ResetModifiers();
            foreach (var u in enemyUnits) u.ResetModifiers();

            battleActive = true;
            StartCoroutine(RunBattleLoop());
        }

        private void SpawnUnitsFromData()
        {
            playerUnits.Clear();
            for (int i = 0; i < playerData.Count; i++)
            {
                var spawnPos = i < playerSpawnPoints.Count ? playerSpawnPoints[i] : transform;
                var go = Instantiate(battleUnitPrefab, spawnPos.position, spawnPos.rotation);
                var unit = go.GetComponent<BattleUnit>();
                unit.Initialize(playerData[i], true);
                playerUnits.Add(unit);
            }

            enemyUnits.Clear();
            for (int i = 0; i < enemyData.Count; i++)
            {
                var spawnPos = i < enemySpawnPoints.Count ? enemySpawnPoints[i] : transform;
                var go = Instantiate(battleUnitPrefab, spawnPos.position, spawnPos.rotation);
                var unit = go.GetComponent<BattleUnit>();
                unit.Initialize(enemyData[i], false);
                enemyUnits.Add(unit);
            }
        }

        // =========================================================
        //  MAIN LOOP
        // =========================================================

        private IEnumerator RunBattleLoop()
        {
            SetPhase(BattlePhase.Intro);
            yield return new WaitForSeconds(stepDelay);

            while (battleActive)
            {
                SetPhase(BattlePhase.StartRound);
                turnManager.BuildTurnOrder(playerUnits.Concat(enemyUnits));
                yield return new WaitForSeconds(0.1f);

                while (turnManager.HasNext)
                {
                    currentUnit = turnManager.GetNext();
                    if (currentUnit == null) break;

                    currentUnit.ResetTurnState();
                    OnTurnStart?.Invoke(currentUnit);
                    yield return new WaitForSeconds(0.2f);

                    if (currentUnit.isPlayerControlled)
                        yield return StartCoroutine(HandlePlayerTurn(currentUnit));
                    else
                        yield return StartCoroutine(HandleEnemyTurn(currentUnit));

                    SetPhase(BattlePhase.CheckEnd);
                    var result = EvaluateBattleEnd();
                    if (result != BattleResult.None)
                    {
                        EndBattle(result);
                        yield break;
                    }
                }
            }
        }

        // =========================================================
        //  PLAYER TURN
        // =========================================================

        private IEnumerator HandlePlayerTurn(BattleUnit unit)
        {
            selectedCommand = null;
            selectedSkill = null;
            selectedTarget = null;
            selectedTargets = null;

            SetPhase(BattlePhase.PlayerCommand);
            // Tunggu UI memanggil SelectCommand(...)
            yield return new WaitUntil(() => selectedCommand.HasValue);

            switch (selectedCommand.Value)
            {
                case BattleCommand.Attack:
                    yield return StartCoroutine(WaitForTargetThenExecute(unit, null, TargetType.SingleEnemy));
                    break;

                case BattleCommand.Skill:
                    // UI harus panggil SelectSkill(skill) dulu, baru SelectTarget(...)
                    yield return new WaitUntil(() => selectedSkill != null);
                    yield return StartCoroutine(WaitForTargetThenExecute(unit, selectedSkill, selectedSkill.targetType));
                    break;

                case BattleCommand.Defend:
                    unit.isDefending = true;
                    yield return new WaitForSeconds(stepDelay);
                    break;

                case BattleCommand.Item:
                    // Placeholder: kembangkan sesuai sistem inventory kamu sendiri.
                    yield return new WaitForSeconds(stepDelay);
                    break;

                case BattleCommand.Run:
                    EndBattle(BattleResult.Ran);
                    yield break;
            }
        }

        private IEnumerator WaitForTargetThenExecute(BattleUnit caster, SkillData skill, TargetType targetType)
        {
            if (targetType == TargetType.Self)
            {
                ExecuteAction(caster, skill, new List<BattleUnit> { caster });
                yield break;
            }

            if (targetType == TargetType.AllEnemies || targetType == TargetType.AllAllies)
            {
                var targets = targetType == TargetType.AllEnemies
                    ? enemyUnits.Where(u => u.IsAlive).ToList()
                    : playerUnits.Where(u => u.IsAlive).ToList();
                ExecuteAction(caster, skill, targets);
                yield break;
            }

            SetPhase(BattlePhase.PlayerTargeting);
            yield return new WaitUntil(() => selectedTarget != null);
            ExecuteAction(caster, skill, new List<BattleUnit> { selectedTarget });
        }

        // Dipanggil UI ketika player memilih command
        public void SelectCommand(BattleCommand command)
        {
            if (currentPhase != BattlePhase.PlayerCommand) return;
            selectedCommand = command;
        }

        // Dipanggil UI ketika player memilih skill dari skill menu
        public void SelectSkill(SkillData skill)
        {
            if (currentUnit == null || !currentUnit.SpendMP(skill.mpCost))
            {
                Debug.LogWarning($"MP tidak cukup untuk skill {skill.skillName}");
                return;
            }
            selectedSkill = skill;
        }

        // Dipanggil UI ketika player memilih target
        public void SelectTarget(BattleUnit target)
        {
            if (currentPhase != BattlePhase.PlayerTargeting) return;
            if (target == null || !target.IsAlive) return;
            selectedTarget = target;
        }

        // =========================================================
        //  ENEMY TURN
        // =========================================================

        private IEnumerator HandleEnemyTurn(BattleUnit unit)
        {
            SetPhase(BattlePhase.EnemyThinking);
            yield return new WaitForSeconds(stepDelay * 0.5f);

            var decision = EnemyAI.ChooseAction(unit, enemyUnits, playerUnits);

            if (decision.target == null)
            {
                yield break; // tidak ada target valid, skip giliran
            }

            if (decision.skill != null)
            {
                unit.SpendMP(decision.skill.mpCost);
                var targets = ResolveTargetsForType(decision.skill.targetType, decision.target, unit);
                ExecuteAction(unit, decision.skill, targets);
            }
            else
            {
                ExecuteAction(unit, null, new List<BattleUnit> { decision.target });
            }

            yield return new WaitForSeconds(stepDelay);
        }

        private List<BattleUnit> ResolveTargetsForType(TargetType type, BattleUnit primaryTarget, BattleUnit caster)
        {
            switch (type)
            {
                case TargetType.AllEnemies:
                    return playerUnits.Where(u => u.IsAlive).ToList();
                case TargetType.AllAllies:
                    return enemyUnits.Where(u => u.IsAlive).ToList();
                case TargetType.Self:
                    return new List<BattleUnit> { caster };
                default:
                    return new List<BattleUnit> { primaryTarget };
            }
        }

        // =========================================================
        //  EKSEKUSI AKSI (dipakai player maupun enemy)
        // =========================================================

        private void ExecuteAction(BattleUnit caster, SkillData skill, List<BattleUnit> targets)
        {
            SetPhase(BattlePhase.ExecutingAction);

            foreach (var target in targets)
            {
                if (target == null || !target.IsAlive) continue;

                ActionResult result = skill != null
                    ? BattleFormulas.ExecuteSkillOnTarget(caster, target, skill)
                    : BattleFormulas.ExecuteBasicAttack(caster, target);

                OnActionResolved?.Invoke(caster, result);

                if (!target.IsAlive)
                {
                    OnUnitDied?.Invoke(target);
                }
            }
        }

        // =========================================================
        //  CEK KEMENANGAN / KEKALAHAN
        // =========================================================

        private BattleResult EvaluateBattleEnd()
        {
            bool allPlayersDead = playerUnits.All(u => !u.IsAlive);
            bool allEnemiesDead = enemyUnits.All(u => !u.IsAlive);

            if (allPlayersDead) return BattleResult.Defeat;
            if (allEnemiesDead) return BattleResult.Victory;
            return BattleResult.None;
        }

        private void EndBattle(BattleResult result)
        {
            battleActive = false;
            turnManager.Clear();

            switch (result)
            {
                case BattleResult.Victory:
                    SetPhase(BattlePhase.Victory);
                    break;
                case BattleResult.Defeat:
                    SetPhase(BattlePhase.Defeat);
                    break;
                case BattleResult.Ran:
                    SetPhase(BattlePhase.Ran);
                    break;
            }

            OnBattleEnded?.Invoke(result);
        }

        private void SetPhase(BattlePhase phase)
        {
            currentPhase = phase;
            OnPhaseChanged?.Invoke(phase);
        }
    }
}
