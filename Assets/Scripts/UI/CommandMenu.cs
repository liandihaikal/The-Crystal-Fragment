using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace JRPGBattle
{
    /// <summary>
    /// Contoh UI menu command (Attack/Skill/Item/Defend/Run) + submenu skill & target.
    /// Sesuaikan referensi Button/Transform di Inspector dengan hierarchy UI kamu sendiri.
    /// Ini hanya contoh wiring — tampilan/animasi silakan dikembangkan sendiri.
    /// </summary>
    public class CommandMenu : MonoBehaviour
    {
        [Header("Referensi")]
        public BattleManager battleManager;

        [Header("Panel Command Utama")]
        public GameObject commandPanel;
        public Button attackButton;
        public Button skillButton;
        public Button itemButton;
        public Button defendButton;
        public Button runButton;

        [Header("Panel Skill")]
        public GameObject skillPanel;
        public Transform skillButtonContainer;
        public Button skillButtonPrefab; // prefab tombol sederhana dengan komponen Text/TMP_Text di child

        [Header("Panel Target")]
        public GameObject targetPanel; // opsional, bisa juga target dipilih langsung klik sprite unit di scene

        private void Awake()
        {
            attackButton.onClick.AddListener(OnAttackClicked);
            skillButton.onClick.AddListener(OnSkillMenuClicked);
            itemButton.onClick.AddListener(OnItemClicked);
            defendButton.onClick.AddListener(OnDefendClicked);
            runButton.onClick.AddListener(OnRunClicked);
        }

        private void OnEnable()
        {
            if (battleManager != null)
                battleManager.OnPhaseChanged += HandlePhaseChanged;
        }

        private void OnDisable()
        {
            if (battleManager != null)
                battleManager.OnPhaseChanged -= HandlePhaseChanged;
        }

        private void HandlePhaseChanged(BattlePhase phase)
        {
            commandPanel.SetActive(phase == BattlePhase.PlayerCommand);
            skillPanel.SetActive(false);

            if (targetPanel != null)
                targetPanel.SetActive(phase == BattlePhase.PlayerTargeting);
        }

        private void OnAttackClicked()
        {
            battleManager.SelectCommand(BattleCommand.Attack);
        }

        private void OnSkillMenuClicked()
        {
            skillPanel.SetActive(true);
            PopulateSkillButtons();
        }

        private void PopulateSkillButtons()
        {
            foreach (Transform child in skillButtonContainer)
                Destroy(child.gameObject);

            var unit = battleManager.CurrentUnit;
            if (unit == null || unit.data == null) return;

            foreach (var skill in unit.data.skills)
            {
                var btn = Instantiate(skillButtonPrefab, skillButtonContainer);
                var label = btn.GetComponentInChildren<Text>();
                if (label != null)
                    label.text = $"{skill.skillName} (MP {skill.mpCost})";

                btn.interactable = skill.mpCost <= unit.currentMP;

                var capturedSkill = skill; // hindari closure bug di loop
                btn.onClick.AddListener(() =>
                {
                    battleManager.SelectCommand(BattleCommand.Skill);
                    battleManager.SelectSkill(capturedSkill);
                    skillPanel.SetActive(false);
                });
            }
        }

        private void OnItemClicked()
        {
            battleManager.SelectCommand(BattleCommand.Item);
        }

        private void OnDefendClicked()
        {
            battleManager.SelectCommand(BattleCommand.Defend);
        }

        private void OnRunClicked()
        {
            battleManager.SelectCommand(BattleCommand.Run);
        }

        /// <summary>Panggil ini dari script klik-sprite-unit musuh/ally untuk memilih target.</summary>
        public void OnUnitClickedAsTarget(BattleUnit unit)
        {
            battleManager.SelectTarget(unit);
        }
    }
}
