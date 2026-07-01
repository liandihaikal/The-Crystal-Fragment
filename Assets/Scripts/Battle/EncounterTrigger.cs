using UnityEngine;

namespace JRPGBattle
{
    /// <summary>
    /// Contoh trigger sederhana: kalau player masuk collider ini, battle dimulai.
    /// Tempel di sebuah GameObject dengan Collider2D/Collider (Is Trigger = true).
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class EncounterTrigger : MonoBehaviour
    {
        public BattleManager battleManager;
        public string playerTag = "Player";

        [Tooltip("Kalau true, trigger ini hanya bisa memicu battle sekali")]
        public bool oneShot = true;
        private bool triggered = false;

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (triggered && oneShot) return;
            if (!other.CompareTag(playerTag)) return;

            triggered = true;
            battleManager.StartBattle();

            // TODO: sesuaikan dengan sistem scene transition kamu,
            // misal: load additive scene battle, disable movement player, dsb.
        }
    }
}
