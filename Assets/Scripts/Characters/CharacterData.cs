using System.Collections.Generic;
using UnityEngine;

namespace JRPGBattle
{
    /// <summary>
    /// Data statis karakter (player maupun enemy). Dibuat sebagai asset lewat
    /// menu: Assets > Create > Battle > Character Data
    /// </summary>
    [CreateAssetMenu(fileName = "NewCharacter", menuName = "Battle/Character Data", order = 1)]
    public class CharacterData : ScriptableObject
    {
        [Header("Info Dasar")]
        public string characterName = "New Character";
        public Sprite battleSprite;
        public Sprite portrait;

        [Header("Base Stats")]
        public int maxHP = 100;
        public int maxMP = 30;
        public int attack = 10;
        public int defense = 10;
        public int magic = 10;
        public int speed = 10;

        [Header("Skill yang dikuasai")]
        public List<SkillData> skills = new List<SkillData>();

        [Header("Reward jika ini enemy")]
        public int expReward = 0;
        public int goldReward = 0;

        [Header("AI (khusus enemy)")]
        [Tooltip("Semakin tinggi, semakin sering pilih skill dibanding attack biasa")]
        [Range(0f, 1f)] public float skillUsageChance = 0.4f;
    }
}
