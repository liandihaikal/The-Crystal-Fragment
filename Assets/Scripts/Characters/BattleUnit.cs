using System;
using UnityEngine;

namespace JRPGBattle
{
    /// <summary>
    /// Representasi runtime dari sebuah unit di battle (player atau enemy).
    /// Berbeda dengan CharacterData (data statis), BattleUnit menyimpan
    /// state yang berubah-ubah selama pertarungan (HP, MP, buff, dsb).
    /// </summary>
    public class BattleUnit : MonoBehaviour
    {
        [Header("Data Sumber")]
        public CharacterData data;
        public bool isPlayerControlled = true;

        [Header("Runtime Stats")]
        public int currentHP;
        public int currentMP;
        public bool isDefending;

        // Modifier sementara (buff/debuff), direset tiap akhir battle
        [HideInInspector] public float attackModifier = 1f;
        [HideInInspector] public float defenseModifier = 1f;

        public bool IsAlive => currentHP > 0;

        // Events agar UI/animasi bisa bereaksi tanpa coupling langsung
        public event Action<int> OnDamaged;   // parameter: jumlah damage
        public event Action<int> OnHealed;    // parameter: jumlah heal
        public event Action OnDeath;

        public int EffectiveAttack => Mathf.RoundToInt(data.attack * attackModifier);
        public int EffectiveDefense => Mathf.RoundToInt(data.defense * defenseModifier * (isDefending ? 1.5f : 1f));

        public void Initialize(CharacterData sourceData, bool playerControlled)
        {
            data = sourceData;
            isPlayerControlled = playerControlled;
            currentHP = data.maxHP;
            currentMP = data.maxMP;
            attackModifier = 1f;
            defenseModifier = 1f;
            isDefending = false;
        }

        public void TakeDamage(int amount)
        {
            amount = Mathf.Max(0, amount);
            currentHP = Mathf.Max(0, currentHP - amount);
            OnDamaged?.Invoke(amount);

            if (currentHP <= 0)
            {
                OnDeath?.Invoke();
            }
        }

        public void Heal(int amount)
        {
            amount = Mathf.Max(0, amount);
            currentHP = Mathf.Min(data.maxHP, currentHP + amount);
            OnHealed?.Invoke(amount);
        }

        public bool SpendMP(int amount)
        {
            if (currentMP < amount) return false;
            currentMP -= amount;
            return true;
        }

        public void RestoreMP(int amount)
        {
            currentMP = Mathf.Min(data.maxMP, currentMP + amount);
        }

        /// <summary>Dipanggil di awal tiap giliran unit ini untuk reset state sesaat seperti Defend.</summary>
        public void ResetTurnState()
        {
            isDefending = false;
        }

        /// <summary>Dipanggil sekali di akhir battle agar modifier tidak terbawa ke battle berikutnya.</summary>
        public void ResetModifiers()
        {
            attackModifier = 1f;
            defenseModifier = 1f;
        }
    }
}
