using System.Collections.Generic;
using System.Linq;

namespace JRPGBattle
{
    /// <summary>
    /// Mengelola antrian giliran berdasarkan speed tiap unit.
    /// Dipanggil ulang (rebuild) tiap awal "round" baru.
    /// </summary>
    public class TurnManager
    {
        private Queue<BattleUnit> turnQueue = new Queue<BattleUnit>();

        public int RoundNumber { get; private set; } = 0;

        /// <summary>Membangun ulang antrian giliran dari seluruh unit yang masih hidup, diurutkan dari speed tertinggi.</summary>
        public void BuildTurnOrder(IEnumerable<BattleUnit> allUnits)
        {
            RoundNumber++;
            var sorted = allUnits
                .Where(u => u != null && u.IsAlive)
                .OrderByDescending(u => u.data.speed)
                .ThenBy(u => UnityEngine.Random.value); // tie-breaker acak biar tidak selalu urutan sama
            turnQueue = new Queue<BattleUnit>(sorted);
        }

        public bool HasNext => turnQueue.Count > 0;

        /// <summary>Ambil unit giliran berikutnya. Skip otomatis kalau unit sudah mati di tengah round.</summary>
        public BattleUnit GetNext()
        {
            while (turnQueue.Count > 0)
            {
                var unit = turnQueue.Dequeue();
                if (unit.IsAlive) return unit;
            }
            return null;
        }

        public void Clear()
        {
            turnQueue.Clear();
        }
    }
}
