using _Project._Scripts.Gameplay.LootSystem;
using UnityEngine;

namespace _Project._Scripts.Bosses
{
    [CreateAssetMenu(fileName = "New Boss Data", menuName = "Touhou/Boss Data")]
    public class BossData : ScriptableObject
    {
        public string bossName;
        public Sprite bossPortrait;
    
        [Header("Loot Settings")]
        [Tooltip("Bảng vật phẩm sẽ rơi ngẫu nhiên trong trận đấu.")]
        public LootTable randomLootTable;
        [Tooltip("Lượng sát thương cần nhận để có cơ hội rơi vật phẩm.")]
        public int damageThresholdForLoot = 500;
    
        [Header("Final Drops")]
        [Tooltip("Prefab vật phẩm nâng cấp chắc chắn rơi khi chết.")]
        public GameObject guaranteedUpgradeDrop;
        [Tooltip("Prefab vật phẩm hồi máu hiếm chắc chắn rơi khi chết.")]
        public GameObject rareHealthDrop;

        [Header("Stage Sequence")]
        public BossStage[] stageSequence; 
    }



    [System.Serializable]
    public class BossStage
    {
        [Header("Stage Configuration")]
        public int health;
        public GameObject[] attackPatternPrefabs;
        
        [Tooltip("Prefab của kiểu di chuyển cho giai đoạn này.")]
        public GameObject movementPatternPrefab;

        [Header("Spell Card Declaration")]
        public bool isSpellCard;
        public string spellCardName;
        public float timeLimit;
        public int survivalBonus;
        public int captureBonus;
    }
}