using _Project._Scripts.Bosses.AttackPatterns;
using _Project._Scripts.Bosses.MovementPatterns;
using _Project._Scripts.Gameplay.LootSystem;
using System.Collections.Generic;
using UnityEngine;

namespace _Project._Scripts.Bosses
{
    [CreateAssetMenu(fileName = "New Boss Data", menuName = "Touhou/Boss Data")]
    public class BossData : ScriptableObject
    {
        [Header("Thông tin cơ bản")]
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
        [Tooltip("Danh sách các giai đoạn (stage) của boss, theo thứ tự.")]
        public List<BossStage> stages; 
    }

    [System.Serializable]
    public class BossStage
    {
        [Header("Stage Configuration")]
        [Tooltip("Lượng máu của boss trong giai đoạn này.")]
        public int health;
        
        [Tooltip("Danh sách các prefab Attack Pattern cho giai đoạn này.")]
        public List<AttackPattern> attackPatterns; // SỬA ĐỔI: Dùng trực tiếp AttackPattern thay vì GameObject
        
        [Tooltip("Prefab của kiểu di chuyển cho giai đoạn này.")]
        public BossMovementPattern movementPattern; // SỬA ĐỔI: Dùng trực tiếp BossMovementPattern

        [Header("Spell Card Declaration")]
        [Tooltip("Đánh dấu nếu đây là một Spell Card.")]
        public bool isSpellCard;
        [Tooltip("Tên của Spell Card, sẽ hiển thị trên UI.")]
        public string spellCardName;
        [Tooltip("Thời gian giới hạn để hoàn thành Spell Card (tính bằng giây).")]
        public float timeLimit;
        [Tooltip("Điểm thưởng nếu sống sót qua Spell Card.")]
        public int survivalBonus;
        [Tooltip("Điểm thưởng nếu hoàn thành (capture) Spell Card.")]
        public int captureBonus;
    }
}