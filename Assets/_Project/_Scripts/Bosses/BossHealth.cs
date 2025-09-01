using System;
using _Project._Scripts.UI;
using UnityEngine;

namespace _Project._Scripts.Bosses
{
    public class BossHealth : MonoBehaviour
    {
        public event Action OnStageHealthDepleted;

        private int currentHealth;
        private int maxHealth;
        private UIManager uiManager;
        private BossController bossController;
        private int damageAccumulator = 0;
        private bool isStageDepleted = false;

        void Awake()
        {
            bossController = GetComponent<BossController>();
        }
        
        void Start()
        {
            uiManager = UIManager.Instance;
        }

        public void SetNewStage(int newMaxHealth)
        {
            maxHealth = newMaxHealth;
            currentHealth = maxHealth;
            isStageDepleted = false;
        }

        public void TakeDamage(int damage)
        {
            if (isStageDepleted) return;

            currentHealth -= damage;
            damageAccumulator += damage;
            
            if (currentHealth < 0)
            {
                currentHealth = 0;
            }
            
            if (bossController.bossData.randomLootTable != null && 
                damageAccumulator >= bossController.bossData.damageThresholdForLoot)
            {
                damageAccumulator = 0;
                GameObject itemToDrop = bossController.bossData.randomLootTable.GetRandomDrop();
                if (itemToDrop != null)
                {
                    Instantiate(itemToDrop, transform.position, Quaternion.identity);
                }
            }
            
            if (uiManager != null && maxHealth > 0)
            {
                uiManager.UpdateBossHealthBar((float)currentHealth / maxHealth);
            }

            if (currentHealth <= 0)
            {
                StageDepleted();
            }
        }

        private void StageDepleted()
        {
            if (isStageDepleted) return;
            isStageDepleted = true;

            // Kích hoạt sự kiện để BossController biết giai đoạn hiện tại đã kết thúc
            OnStageHealthDepleted?.Invoke();
        }
    }
}