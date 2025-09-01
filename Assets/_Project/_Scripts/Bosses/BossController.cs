using System;
using System.Collections;
using System.Collections.Generic;
using _Project._Scripts.Bosses.AttackPatterns;
using _Project._Scripts.Bosses.MovementPatterns;
using _Project._Scripts.Core;
using _Project._Scripts.UI;
using UnityEngine;

namespace _Project._Scripts.Bosses
{
    [RequireComponent(typeof(BossHealth))]
    [RequireComponent(typeof(BossShooting))]
    [RequireComponent(typeof(Rigidbody2D))]
    public class BossController : MonoBehaviour
    {
        [Header("Configuration")]
        public BossData bossData;
        [SerializeField] private float transitionDelay = 2.0f; // Thời gian chờ giữa các stage

        // --- References ---
        private Rigidbody2D rb;
        private BossHealth bossHealth;
        private BossShooting bossShooting;
        private UIManager uiManager;

        // --- State ---
        private int currentStageIndex = -1;
        private List<AttackPattern> activeAttackPatterns = new List<AttackPattern>();
        private BossMovementPattern activeMovementPattern;
        
        // Cờ trạng thái để quản lý luồng hoạt động
        private bool isFighting = false;
        private bool isTransitioning = false;
        private bool isDefeated = false;

        #region Unity Lifecycle
        
        private void Awake()
        {
            // Lấy các component cần thiết
            rb = GetComponent<Rigidbody2D>();
            bossHealth = GetComponent<BossHealth>();
            bossShooting = GetComponent<BossShooting>();
            
            // Cấu hình Rigidbody
            rb.gravityScale = 0;
            rb.isKinematic = true; // Di chuyển boss qua transform sẽ an toàn hơn
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        }

        private void Start()
        {
            uiManager = UIManager.Instance;
            // Bắt đầu trận đấu (có thể gọi từ một script khác như LevelManager)
            Initialize(); 
        }

        private void OnEnable()
        {
            // Đăng ký sự kiện khi component được kích hoạt
            bossHealth.OnStageHealthDepleted += HandleStageHealthDepleted;
        }

        private void OnDisable()
        {
            // Hủy đăng ký sự kiện để tránh lỗi và memory leak
            bossHealth.OnStageHealthDepleted -= HandleStageHealthDepleted;
        }

        #endregion

        #region Core Fight Loop

        // Bắt đầu trận đấu với boss
        public void Initialize()
        {
            if (bossData == null || bossData.stages.Count == 0)
            {
                Debug.LogError("BossData is not assigned or has no stages!", this);
                gameObject.SetActive(false);
                return;
            }

            isFighting = true;
            currentStageIndex = 0;
            uiManager?.ShowBossUI();
            StartCoroutine(TransitionToStage(currentStageIndex));
        }

        // Được gọi bởi event OnStageHealthDepleted từ BossHealth
        private void HandleStageHealthDepleted()
        {
            // Chỉ xử lý khi boss đang trong trận và không bị đánh bại
            if (!isFighting || isDefeated) return;

            currentStageIndex++;

            if (currentStageIndex < bossData.stages.Count)
            {
                // Chuyển sang stage tiếp theo
                StartCoroutine(TransitionToStage(currentStageIndex));
            }
            else
            {
                // Boss đã bị đánh bại
                StartCoroutine(DefeatSequence());
            }
        }

        // Coroutine xử lý việc chuyển đổi giữa các stage
        private IEnumerator TransitionToStage(int stageIndex)
        {
            isTransitioning = true;
            
            // Dừng và dọn dẹp các pattern của stage cũ
            StopAndClearAllPatterns();
            
            // Đợi một khoảng thời gian trước khi bắt đầu stage mới
            if (stageIndex > 0) // Không delay ở stage đầu tiên
            {
                Debug.Log("Transitioning to next stage...");
                yield return new WaitForSeconds(transitionDelay);
            }

            // Thiết lập stage mới
            SetupStage(bossData.stages[stageIndex]);

            isTransitioning = false;
        }

        // Thiết lập các thành phần cho một stage cụ thể
        private void SetupStage(BossStage stage)
        {
            if (stage == null) return;
            
            Debug.Log($"Setting up Stage {currentStageIndex + 1}: {(stage.isSpellCard ? stage.spellCardName : "Non-Spell")}");
            
            // Cập nhật UI nếu là Spell Card
            if (stage.isSpellCard)
            {
                uiManager?.DeclareSpellCard(stage.spellCardName, stage.timeLimit);
            }
            else
            {
                 uiManager?.HideSpellCardUI();
            }

            // Thiết lập máu và các pattern
            bossHealth.SetNewStage(stage.health);
            SetupMovementPattern(stage);
            SetupAttackPatterns(stage);
            
            // Bắt đầu các hành động của stage
            bossShooting.StartShooting();
        }

        // Coroutine xử lý khi boss bị đánh bại hoàn toàn
        private IEnumerator DefeatSequence()
        {
            if (isDefeated) yield break; // Ngăn chạy nhiều lần
            isDefeated = true;
            isFighting = false;

            Debug.Log($"Boss {bossData.bossName} has been defeated!");
            
            // Dừng mọi hoạt động
            StopAllCoroutines();
            StopAndClearAllPatterns();

            // Vô hiệu hóa va chạm
            if (GetComponent<Collider2D>() != null) GetComponent<Collider2D>().enabled = false;
            
            // Ẩn UI của boss
            uiManager?.HideBossUI();
            uiManager?.HideSpellCardUI();

            // TODO: Thêm hiệu ứng nổ, âm thanh, v.v.
            // Ví dụ: Instantiate(deathEffectPrefab, transform.position, Quaternion.identity);
            yield return new WaitForSeconds(1f); // Chờ hiệu ứng

            // Rớt đồ
            DropGuaranteedLoot();
            
            // Thông báo cho GameManager
            GameManager.Instance?.OnBossDefeated();

            // Hủy đối tượng boss sau một khoảng trễ
            Destroy(gameObject, 2f);
        }
        
        #endregion

        #region Pattern & Loot Management

        // Thiết lập các attack pattern cho stage
        private void SetupAttackPatterns(BossStage stage)
        {
            var attackPatternPrefabs = stage.attackPatterns;
            if (attackPatternPrefabs == null) return;

            foreach (var patternPrefab in attackPatternPrefabs)
            {
                if (patternPrefab != null)
                {
                    AttackPattern newInstance = Instantiate(patternPrefab, transform);
                    newInstance.Initialize(this);
                    activeAttackPatterns.Add(newInstance);
                }
            }
            bossShooting.SetAttackPatterns(activeAttackPatterns);
        }

        // Thiết lập movement pattern cho stage
        private void SetupMovementPattern(BossStage stage)
        {
            if (stage.movementPattern != null)
            {
                activeMovementPattern = Instantiate(stage.movementPattern, transform);
                activeMovementPattern.Initialize(this);
            }
        }

        // Hàm dọn dẹp TẤT CẢ các pattern đang hoạt động
        private void StopAndClearAllPatterns()
        {
            // Dừng bắn
            bossShooting.StopShooting();

            // Dọn dẹp Attack Patterns
            foreach (var pattern in activeAttackPatterns)
            {
                if (pattern != null)
                {
                    Destroy(pattern.gameObject);
                }
            }
            activeAttackPatterns.Clear();

            // Dọn dẹp Movement Pattern
            if (activeMovementPattern != null)
            {
                Destroy(activeMovementPattern.gameObject);
                activeMovementPattern = null;
            }
        }

        private void DropGuaranteedLoot()
        {
            if (bossData.guaranteedUpgradeDrop != null)
                Instantiate(bossData.guaranteedUpgradeDrop, transform.position, Quaternion.identity);
            
            if (bossData.rareHealthDrop != null)
                Instantiate(bossData.rareHealthDrop, transform.position, Quaternion.identity);
        }

        #endregion
    }
}