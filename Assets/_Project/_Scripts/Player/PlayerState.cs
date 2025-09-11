// FILE: _Project/_Scripts/Player/PlayerState.cs (VERSION 6.0 - AUTO LEVEL UP)

using UnityEngine;
using System;
using System.Collections.Generic;
using _Project._Scripts.Core;
using _Project._Scripts.UI;

namespace _Project._Scripts.Player
{
    [RequireComponent(typeof(PlayerShooting), typeof(PlayerSkillManager), typeof(PlayerController))]
    public class PlayerState : MonoBehaviour
    {
        [Header("📈 Hệ thống Nâng cấp (Upgrade System)")]
        [Tooltip("Kéo ScriptableObject 'PlayerUpgradePath' chứa toàn bộ chuỗi nâng cấp vào đây.")]
        [SerializeField] private PlayerUpgradePath upgradePath;
        
        // Biến nội bộ để theo dõi cấp độ hiện tại
        private int currentUpgradeLevel = -1; // Bắt đầu từ -1, để cấp độ đầu tiên là 0

        [Header("❤️ Hệ thống Máu (Health System)")]
        [Tooltip("Lượng máu tối đa của người chơi.")]
        [SerializeField] private int maxHealth = 100;
        private int currentHealth;

        [Space(10)]
        [Header("✨ Trạng thái Bất tử (Invincibility)")]
        [Tooltip("Thời gian bất tử (giây) sau khi nhận sát thương.")]
        [SerializeField] private float invincibilityDuration = 2f;
        public bool IsInvincible { get; private set; }
        private float invincibilityTimer;

        [Space(10)]
        [Header("⭐ Chỉ số Gameplay (Gameplay Stats)")]
        [Tooltip("Số bom khởi điểm của người chơi.")]
        [SerializeField] private int initialBombs = 3;
        [Tooltip("Sức mạnh khởi điểm của người chơi.")]
        [SerializeField] private float initialPower = 1.0f;
        
        [HideInInspector] public int bombs;
        [HideInInspector] public float power;
        [HideInInspector] public long score;

        [Space(15)]
        [Header("🛠️ Chức năng Gỡ lỗi (Debug)")]
        [Tooltip("Bật chế độ này để sử dụng các công cụ gỡ lỗi bên dưới KHI GAME ĐANG CHẠY.")]
        [SerializeField] private bool debugMode = false;
        
        [Tooltip("Khi game đang chạy, kéo một ScriptableObject UpgradeData vào đây để áp dụng ngay. Set về 'None' để quay về trạng thái cơ bản.")]
        [SerializeField] private UpgradeData debugApplyUpgrade;
        private UpgradeData lastAppliedDebugUpgrade;

        // --- Tham chiếu và Biến nội bộ ---
        private PlayerShooting playerShooting;
        private PlayerSkillManager playerSkillManager;
        private PlayerController playerController;
        private List<UpgradeData> collectedUpgrades = new List<UpgradeData>();
        
        public static event Action OnPlayerDied;

        #region Unity Lifecycle & Setup

        void Awake()
        {
            playerShooting = GetComponent<PlayerShooting>();
            playerSkillManager = GetComponent<PlayerSkillManager>();
            playerController = GetComponent<PlayerController>();
        }

        void Start()
        {
            currentHealth = maxHealth;
            bombs = initialBombs;
            power = initialPower;
            score = 0;
            collectedUpgrades = new List<UpgradeData>();
            ApplyAllUpgrades();
            UpdateAllUI();
        }

        void Update()
        {
            HandleInvincibilityTimer();
            HandleDebugUpgrade();
        }

        #endregion

        #region Skill & Upgrade Logic

        /// <summary>
        /// Nâng cấp người chơi lên cấp độ tiếp theo dựa trên Upgrade Path.
        /// Được gọi bởi Item.cs khi người chơi nhặt vật phẩm "Upgrade".
        /// </summary>
        public void LevelUp()
        {
            if (upgradePath == null || upgradePath.upgradeLevels.Count == 0)
            {
                Debug.LogError("Chưa thiết lập 'Upgrade Path' cho PlayerState!", this);
                return;
            }

            currentUpgradeLevel++;

            if (currentUpgradeLevel >= upgradePath.upgradeLevels.Count)
            {
                currentUpgradeLevel = upgradePath.upgradeLevels.Count - 1;
                Debug.Log("Player đã đạt cấp độ tối đa!");
                return;
            }

            UpgradeData nextUpgrade = upgradePath.upgradeLevels[currentUpgradeLevel];
        
            if (nextUpgrade != null)
            {
            
                // Chỉ thêm nâng cấp mới. ApplyAllUpgrades() sẽ tính toán lại tất cả.
                AddUpgrade(nextUpgrade); 
                Debug.Log($"Player đã lên cấp {currentUpgradeLevel}: Áp dụng nâng cấp '{nextUpgrade.name}'");
            }
        }

        public void UseInvincibilitySkill()
        {
            if (!HasInvincibilitySkill() || !playerSkillManager.IsSkillReady(PlayerSkillManager.SkillType.Invincibility)) return;

            float duration = 0f;
            foreach (var upgrade in collectedUpgrades)
            {
                if (upgrade.hasInvincibilitySkill && upgrade.invincibilitySkillDuration > duration)
                {
                    duration = upgrade.invincibilitySkillDuration;
                }
            }
            
            SetTemporaryInvincibility(duration);
            playerSkillManager.TriggerCooldown(PlayerSkillManager.SkillType.Invincibility);
            Debug.Log($"Đã kích hoạt Bất Tử trong {duration} giây!");
        }

        public void AddUpgrade(UpgradeData upgrade)
        {
            if (upgrade == null || collectedUpgrades.Contains(upgrade)) return;
        
            collectedUpgrades.Add(upgrade);
            ApplyAllUpgrades(); // Gọi áp dụng lại sau khi thêm
        }
        
        public void ClearAllUpgrades()
        {
            collectedUpgrades.Clear();
            ApplyAllUpgrades(); // Gọi áp dụng lại sau khi xóa
        }

        
        public bool HasDashAbility()
        {
            foreach (var upgrade in collectedUpgrades)
                if (upgrade.unlocksDash) return true;
            return false;
        }
        
        public bool HasBulletClearSkill()
        {
            foreach (var upgrade in collectedUpgrades)
                if (upgrade.hasBulletClearSkill) return true;
            return false;
        }

        public bool HasInvincibilitySkill()
        {
            foreach (var upgrade in collectedUpgrades)
                if (upgrade.hasInvincibilitySkill) return true;
            return false;
        }

        public void SetTemporaryInvincibility(float duration)
        {
            IsInvincible = true;
            if (duration > invincibilityTimer)
                invincibilityTimer = duration;
        }
        
        private void ApplyAllUpgrades()
        {
            float totalSpeedMultiplier = 1.0f;
        
            // Reset trạng thái trước khi áp dụng lại
            bool newDashState = false; 
        
            // Tính toán lại tất cả các hiệu ứng từ đầu
            foreach (var upgrade in collectedUpgrades)
            {
                totalSpeedMultiplier *= upgrade.moveSpeedMultiplier;
                if (upgrade.unlocksDash)
                {
                    newDashState = true;
                }
            }
        
            // Cập nhật các component khác
            playerController?.SetSpeedMultiplier(totalSpeedMultiplier);
            playerShooting?.ApplyUpgrades(collectedUpgrades);
        }
        
        private void HandleDebugUpgrade()
        {
            if (!debugMode) return;
            if (debugApplyUpgrade != lastAppliedDebugUpgrade)
            {
                ClearAllUpgrades();
                if (debugApplyUpgrade != null)
                {
                    AddUpgrade(debugApplyUpgrade);
                }
                lastAppliedDebugUpgrade = debugApplyUpgrade;
            }
        }
        #endregion

        #region Gameplay Actions & Handlers
        public void TakeDamage(int damage)
        {
            if (IsInvincible) return;
            currentHealth -= damage;
            if (currentHealth < 0) currentHealth = 0;
            UpdateHealthUI();
            if (currentHealth <= 0)
            {
                GameManager.Instance.PlayerDied();
                Die();
            }
            else StartInvincibility();
        }
        
        public void RestoreFullHealth()
        {
            currentHealth = maxHealth;
            // Cập nhật UI máu
        }
        
        public void UseBomb()
        {
            if (bombs > 0 && playerSkillManager != null)
            {
                bombs--;
                playerSkillManager.ActivateBomb();
                UIManager.Instance?.UpdateBombsText(bombs);
            }
        }
        
        private void HandleInvincibilityTimer()
        {
            if (IsInvincible)
            {
                invincibilityTimer -= Time.deltaTime;
                if (invincibilityTimer <= 0) IsInvincible = false;
            }
        }
        
        private void Die()
        {
            OnPlayerDied?.Invoke();
            gameObject.SetActive(false);
        }

        private void StartInvincibility()
        {
            SetTemporaryInvincibility(invincibilityDuration);
        }
        #endregion
        
        #region UI & Data Getters
        private void UpdateHealthUI()
        {
            float healthPercentage = (float)currentHealth / maxHealth;
            UIManager.Instance?.UpdatePlayerHealthBar(healthPercentage);
        }
        
        public void UpdateAllUI()
        {
            UpdateHealthUI();
            UIManager.Instance?.UpdateBombsText(bombs);
            UIManager.Instance?.UpdatePowerText(power);
            UIManager.Instance?.UpdateScoreText(score);
        }
        public void Heal(int amount)
        {
            currentHealth += amount;
            if (currentHealth > maxHealth) currentHealth = maxHealth;
            UpdateHealthUI();
        }
        public void AddScore(long amount)
        {
            score += amount;
            UIManager.Instance?.UpdateScoreText(score);
        }
        public void AddPower(float amount)
        {
            power += amount;
            UIManager.Instance?.UpdatePowerText(power);
            // Không cần gọi ApplyAllUpgrades ở đây vì Power không trực tiếp thay đổi kiểu bắn
        }
        public void AddBomb(int amount)
        {
            bombs += amount;
            UIManager.Instance?.UpdateBombsText(bombs);
        }
        #endregion
    }
}

