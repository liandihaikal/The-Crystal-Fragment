using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace JRPGBattle
{
    /// <summary>
    /// Contoh listener untuk menampilkan HP/MP bar dan log teks battle.
    /// Hubungkan referensi di Inspector sesuai hierarchy UI kamu.
    /// </summary>
    public class BattleUI : MonoBehaviour
    {
        [Header("Referensi")]
        public BattleManager battleManager;

        [Header("Log")]
        public Text logText; // ganti ke TMP_Text kalau pakai TextMeshPro
        public int maxLogLines = 6;
        private readonly List<string> logLines = new List<string>();

        [Header("HP/MP Bar per Unit (urutkan sesuai playerUnits/enemyUnits)")]
        public List<UnitStatusBar> playerBars = new List<UnitStatusBar>();
        public List<UnitStatusBar> enemyBars = new List<UnitStatusBar>();

        [System.Serializable]
        public class UnitStatusBar
        {
            public Slider hpSlider;
            public Slider mpSlider;
            public Text nameText;
        }

        private void OnEnable()
        {
            battleManager.OnTurnStart += HandleTurnStart;
            battleManager.OnActionResolved += HandleActionResolved;
            battleManager.OnUnitDied += HandleUnitDied;
            battleManager.OnBattleEnded += HandleBattleEnded;
        }

        private void OnDisable()
        {
            battleManager.OnTurnStart -= HandleTurnStart;
            battleManager.OnActionResolved -= HandleActionResolved;
            battleManager.OnUnitDied -= HandleUnitDied;
            battleManager.OnBattleEnded -= HandleBattleEnded;
        }

        private void Update()
        {
            // Cara paling simpel: refresh semua bar tiap frame.
            // Untuk battle kecil ini cukup ringan; optimalkan lewat event kalau perlu.
            RefreshBars(battleManager.PlayerUnits, playerBars);
            RefreshBars(battleManager.EnemyUnits, enemyBars);
        }

        private void RefreshBars(IReadOnlyList<BattleUnit> units, List<UnitStatusBar> bars)
        {
            for (int i = 0; i < units.Count && i < bars.Count; i++)
            {
                var unit = units[i];
                var bar = bars[i];
                if (unit == null || bar == null || unit.data == null) continue;

                if (bar.hpSlider != null)
                {
                    bar.hpSlider.maxValue = unit.data.maxHP;
                    bar.hpSlider.value = unit.currentHP;
                }
                if (bar.mpSlider != null)
                {
                    bar.mpSlider.maxValue = unit.data.maxMP;
                    bar.mpSlider.value = unit.currentMP;
                }
                if (bar.nameText != null)
                {
                    bar.nameText.text = unit.data.characterName;
                }
            }
        }

        private void HandleTurnStart(BattleUnit unit)
        {
            AddLog($"Giliran {unit.data.characterName}.");
        }

        private void HandleActionResolved(BattleUnit caster, ActionResult result)
        {
            if (result.target == null) return;

            if (result.isMiss)
            {
                AddLog($"{caster.data.characterName} menyerang {result.target.data.characterName} tapi meleset!");
                return;
            }

            switch (result.effectType)
            {
                case SkillEffectType.Damage:
                    string critText = result.isCrit ? " (KRITIKAL!)" : "";
                    AddLog($"{caster.data.characterName} menyerang {result.target.data.characterName} sebesar {result.amount} damage{critText}.");
                    break;
                case SkillEffectType.Heal:
                    AddLog($"{caster.data.characterName} menyembuhkan {result.target.data.characterName} sebesar {result.amount} HP.");
                    break;
                default:
                    AddLog($"{caster.data.characterName} menggunakan efek pada {result.target.data.characterName}.");
                    break;
            }
        }

        private void HandleUnitDied(BattleUnit unit)
        {
            AddLog($"{unit.data.characterName} tumbang!");
        }

        private void HandleBattleEnded(BattleResult result)
        {
            switch (result)
            {
                case BattleResult.Victory:
                    AddLog("Kemenangan! Semua musuh berhasil dikalahkan.");
                    break;
                case BattleResult.Defeat:
                    AddLog("Kekalahan... seluruh party tumbang.");
                    break;
                case BattleResult.Ran:
                    AddLog("Berhasil kabur dari pertarungan.");
                    break;
            }
        }

        private void AddLog(string line)
        {
            logLines.Add(line);
            if (logLines.Count > maxLogLines)
                logLines.RemoveAt(0);

            if (logText != null)
                logText.text = string.Join("\n", logLines);

            Debug.Log("[Battle] " + line);
        }
    }
}
