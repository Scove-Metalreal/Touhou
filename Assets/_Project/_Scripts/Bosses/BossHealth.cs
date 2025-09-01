using System;
using _Project._Scripts.UI;
using UnityEngine;

namespace _Project._Scripts.Bosses
{
    public class BossHealth : MonoBehaviour
    {
        public event Action OnStageHealthDepleted; // Sự kiện báo cho BossController biết đã hết máu

        private int currentHealth;
        private int maxHealth;
        private UIManager uiManager;
        private BossController bossController;
        private int damageAccumulator = 0; // Biến đếm sát thương

        void Awake() // Đổi từ Start sang Awake
        {
            bossController = GetComponent<BossController>();
        }
        
        void Start()
        {
            // Lấy UIManager để cập nhật thanh máu
            uiManager = UIManager.Instance;
        }

        // Hàm này được gọi bởi BossController khi bắt đầu một stage mới
        public void SetNewStage(int newMaxHealth)
        {
            maxHealth = newMaxHealth;
            currentHealth = maxHealth;
        }

        // Hàm này được gọi bởi viên đạn của Player
        public void TakeDamage(int damage)
        {
            currentHealth -= damage;
            damageAccumulator += damage;
            
            // Kiểm tra xem có đủ sát thương để thử rơi đồ không
            if (bossController.bossData.randomLootTable != null && 
                damageAccumulator >= bossController.bossData.damageThresholdForLoot)
            {
                damageAccumulator = 0; // Reset bộ đếm
                GameObject itemToDrop = bossController.bossData.randomLootTable.GetRandomDrop();
                if (itemToDrop != null)
                {
                    Instantiate(itemToDrop, transform.position, Quaternion.identity);
                }
            }
            
            // Cập nhật thanh máu trên UI
            if (uiManager != null)
            {
                uiManager.UpdateBossHealthBar((float)currentHealth / maxHealth);
            }

            // Nếu hết máu, kích hoạt sự kiện
            if (currentHealth <= 0)
            {
                OnStageHealthDepleted?.Invoke();
            }
        }
    }
}