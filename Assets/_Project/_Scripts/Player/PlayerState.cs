// FILE: _Project/Scripts/Player/PlayerState.cs

using UnityEngine;
using System.Collections;
using _Project._Scripts.Core;
using _Project._Scripts.UI;

// Namespace của bạn có thể khác

// Namespace của bạn có thể khác

namespace _Project._Scripts.Player
{
    public class PlayerState : MonoBehaviour
    {
        [Header("Chỉ số Cơ bản")]
        [Tooltip("Số mạng khởi đầu.")]
        public int initialLives = 3;
        [Tooltip("Số bom khởi đầu.")]
        public int initialBombs = 2;
        [Tooltip("Thời gian bất tử sau khi chết (giây).")]
        public float invincibilityDuration = 3.0f;

        [Header("Hệ thống Nâng cấp")]
        [Tooltip("Kéo 12 file UpgradeData vào đây theo đúng thứ tự từ 0 đến 11.")]
        public UpgradeData[] upgradeLevels;
        
        // --- THAY ĐỔI 1: Thêm biến để test ---
        [Tooltip("Cấp độ khởi đầu để test (từ 0 đến 11). Sẽ ghi đè giá trị mặc định.")]
        [Range(0, 11)] // Giới hạn giá trị trong Inspector cho an toàn
        public int startingUpgradeLevel = 0;
        
        // --- Properties ---
        public int CurrentLives { get; private set; }
        public int CurrentBombs { get; private set; }
        public int CurrentPower { get; private set; }
        public bool IsInvincible { get; private set; }
        
        // --- THAY ĐỔI 2: Cho phép chỉnh sửa public nhưng gán giá trị private ---
        public int CurrentUpgradeLevel { get; private set; }
        
        public UpgradeData CurrentUpgrade => upgradeLevels[CurrentUpgradeLevel];

        // --- Tham chiếu ---
        private float invincibilityTimer = 0f;
        private PlayerController playerController;
        private UIManager uiManager;
        private GameManager gameManager;
        private PlayerSkillManager skillManager;
        
        // --- THAY ĐỔI 3: Thêm hàm OnValidate để cập nhật trực tiếp trong Editor ---
        /// <summary>
        /// Hàm này được gọi tự động trong Editor mỗi khi một giá trị được thay đổi.
        /// Rất hữu ích để test game.
        /// </summary>
        void OnValidate()
        {
            // Đảm bảo CurrentUpgradeLevel luôn được cập nhật theo startingUpgradeLevel
            // khi bạn thay đổi nó trong lúc game chưa chạy.
            if (Application.isPlaying == false)
            {
                 CurrentUpgradeLevel = startingUpgradeLevel;
            }
        }


        #region Unity Lifecycle Methods
        
        void Awake()
        {
            playerController = GetComponent<PlayerController>();
            skillManager = GetComponent<PlayerSkillManager>();
        }

        void Start()
        {
            uiManager = UIManager.Instance;
            gameManager = GameManager.Instance;

            CurrentLives = initialLives;
            CurrentBombs = initialBombs;
            CurrentPower = 0;
            IsInvincible = false;
            
            // --- THAY ĐỔI 4: Sử dụng giá trị test ---
            // Gán cấp độ hiện tại bằng cấp độ khởi đầu bạn đã đặt trong Inspector.
            CurrentUpgradeLevel = startingUpgradeLevel;

            ApplyCurrentUpgrade();
            UpdateAllUI();
        }

        void Update()
        {
            HandleInvincibilityTimer();
            HandleSkillInput();
        }

        #endregion

        // ... (Tất cả các hàm còn lại giữ nguyên không thay đổi) ...
        // (TakeDamage, UseBomb, AddUpgrade, ApplyCurrentUpgrade, HandleSkillInput, etc.)
        
        #region Core Gameplay Logic
        public void TakeDamage()
        {
            if (IsInvincible) return;
            CurrentLives--;
            CurrentBombs = initialBombs;
            CurrentPower /= 2;
            UpdateAllUI();
            if (CurrentLives >= 0)
            {
                StartCoroutine(BecomeInvincibleFor(invincibilityDuration, true));
                Debug.Log("Player hit! Lives left: " + CurrentLives);
            }
            else
            {
                Debug.Log("Game Over!");
                gameManager?.GameOver();
            }
        }

        public void UseBomb()
        {
            if (CurrentBombs > 0)
            {
                CurrentBombs--;
                uiManager?.UpdateBombs(CurrentBombs);
            }
        }
        #endregion

        #region Upgrade System
        public void AddUpgrade()
        {
            if (CurrentUpgradeLevel < upgradeLevels.Length - 1)
            {
                // Thay đổi CurrentUpgradeLevel sẽ tự động cập nhật startingUpgradeLevel
                // để bạn có thể thấy sự thay đổi trong Inspector khi game đang chạy.
                startingUpgradeLevel = ++CurrentUpgradeLevel;
                ApplyCurrentUpgrade();
                Debug.Log($"Player Leveled Up to {CurrentUpgradeLevel + 1}");
            }
        }

        private void ApplyCurrentUpgrade()
        {
             // Đảm bảo index không vượt quá giới hạn mảng
            if (playerController != null && CurrentUpgradeLevel < upgradeLevels.Length)
            {
                playerController.SetSpeedMultiplier(CurrentUpgrade.moveSpeedMultiplier);
            }
        }
        #endregion

        #region Active Skills
        private void HandleSkillInput()
        {
            if (Input.GetKeyDown(KeyCode.E))
            {
                if (CurrentUpgrade != null && CurrentUpgrade.hasInvincibilitySkill && skillManager.IsSkillReady(PlayerSkillManager.SkillType.Invincibility))
                {
                    ActivateInvincibilitySkill();
                }
            }
        }

        public void ActivateInvincibilitySkill()
        {
            Debug.Log("Activating Invincibility Skill!");
            // Bắt đầu hồi chiêu
            skillManager.TriggerCooldown(PlayerSkillManager.SkillType.Invincibility);
            // Kích hoạt hiệu ứng
            StartCoroutine(BecomeInvincibleFor(3.0f, false));
        }

        private IEnumerator BecomeInvincibleFor(float duration, bool isAfterDeath)
        {
            IsInvincible = true;
            yield return new WaitForSeconds(duration);
            IsInvincible = false;
        }
        #endregion

        #region UI Updates
        private void UpdateAllUI()
        {
            if (uiManager != null)
            {
                uiManager.UpdateLives(CurrentLives);
                uiManager.UpdateBombs(CurrentBombs);
                uiManager.UpdatePower(CurrentPower);
            }
        }
        
        private void HandleInvincibilityTimer() { }
        #endregion

    }
}