using System;
using System.Collections;
using System.Collections.Generic;
using _Project._Scripts.Bosses.AttackPatterns;
using _Project._Scripts.Bosses.MovementPatterns;
using _Project._Scripts.Core;
using _Project._Scripts.UI;
using UnityEngine;

// Các component này là bắt buộc để BossController hoạt động
namespace _Project._Scripts.Bosses
{
    [RequireComponent(typeof(BossHealth))]
    [RequireComponent(typeof(BossShooting))]
    [RequireComponent(typeof(Rigidbody2D))] // THÊM MỚI: Yêu cầu có Rigidbody2D
    public class BossController : MonoBehaviour
    {
        [Header("Boss Data")]
        public BossData bossData;

        // Tham chiếu đến các component khác
        private Rigidbody2D rb; // THÊM MỚI: Tham chiếu đến Rigidbody2D
        private BossHealth bossHealth;
        private BossShooting bossShooting;

        // Biến trạng thái
        private int currentStageIndex = 0;
        private bool isTransitioning = false;
        private UIManager uiManager;

        // Danh sách chứa các pattern đã được tạo ra (instance)
        private List<AttackPattern> activeAttackPatterns = new List<AttackPattern>();
        private BossMovementPattern activeMovementPattern;

        #region Unity Lifecycle

        void Awake()
        {
            // Lấy các component cần thiết
            rb = GetComponent<Rigidbody2D>();
            bossHealth = GetComponent<BossHealth>();
            bossShooting = GetComponent<BossShooting>();
        
            // Cài đặt Rigidbody2D cho game 2D (không trọng lực, không xoay)
            rb.gravityScale = 0;
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        }

        private void Start()
        {
            uiManager = UIManager.Instance;
        }

        // THÊM MỚI: Hàm FixedUpdate để xử lý di chuyển vật lý
        void FixedUpdate()
        {
            // Nếu có một movement pattern đang hoạt động, gọi hàm Move() của nó
            if (activeMovementPattern != null && !isTransitioning)
            {
                activeMovementPattern.Move();
            }
        }

        private void OnDestroy()
        {
            if (bossHealth != null)
            {
                bossHealth.OnStageHealthDepleted -= HandleStageEnd;
            }
            ClearActivePatterns();
        }

        #endregion

        #region Stage Management

        public void Initialize()
        {
            if (bossData == null || bossData.stages.Count == 0)
            {
                Debug.LogError("BossData chưa được gán hoặc không có stage nào!", this);
                return;
            }
            
            if (uiManager != null)
            {
                uiManager.ShowBossUI();
            }

            bossHealth.OnStageHealthDepleted += HandleStageEnd;
            StartNextStage();
        }

        private void HandleStageEnd()
        {
            if (isTransitioning) return;

            Debug.Log($"Stage {currentStageIndex + 1} đã kết thúc.");
            currentStageIndex++;

            if (currentStageIndex < bossData.stages.Count)
            {
                StartCoroutine(TransitionToNextStage());
            }
            else
            {
                DefeatBoss();
            }
        }

        private void StartNextStage()
        {
            Debug.Log($"Bắt đầu Stage {currentStageIndex + 1}");
            var currentStage = bossData.stages[currentStageIndex];
            
            if (uiManager != null && currentStage.isSpellCard)
            {
                uiManager.DeclareSpellCard(currentStage.spellCardName, currentStage.timeLimit);
            }

            bossHealth.SetNewStage(currentStage.health);
            SetupAttackPatternsForStage(currentStage);
            SetupMovementPatternForStage(currentStage); // Sửa đổi để tương thích

            // Bắt đầu các hành động
            bossShooting.StartShooting();
            if (activeMovementPattern != null) activeMovementPattern.StartMoving();
        }

        private IEnumerator TransitionToNextStage()
        {
            isTransitioning = true;
            bossShooting.StopShooting();
            if (activeMovementPattern != null) activeMovementPattern.StopMoving();

            Debug.Log("Đang chuyển sang stage tiếp theo...");
            yield return new WaitForSeconds(2f);

            isTransitioning = false;
            StartNextStage();
        }

        private void DefeatBoss()
        {
            Debug.Log("Boss đã bị đánh bại!");
            bossHealth.OnStageHealthDepleted -= HandleStageEnd;
            StopAllCoroutines();
            bossShooting.StopShooting();
            if (activeMovementPattern != null) activeMovementPattern.StopMoving();
            if (GetComponent<Collider2D>() != null) GetComponent<Collider2D>().enabled = false;

            if (uiManager != null)
            {
                uiManager.HideBossUI();
            }
            
            if (GameManager.Instance != null) GameManager.Instance.OnBossDefeated();
        
            Destroy(gameObject, 3f);
        }

        #endregion

        #region Pattern Management

        private void SetupAttackPatternsForStage(BossStage stage)
        {
            // Dọn dẹp Attack Pattern cũ
            foreach (var pattern in activeAttackPatterns)
            {
                if (pattern != null) Destroy(pattern.gameObject);
            }
            activeAttackPatterns.Clear();

            // Tạo và khởi tạo Attack Pattern mới
            foreach (AttackPattern patternPrefab in stage.attackPatterns)
            {
                if (patternPrefab != null)
                {
                    AttackPattern newInstance = Instantiate(patternPrefab, transform);
                    newInstance.Initialize(this); // Khởi tạo với BossController
                    activeAttackPatterns.Add(newInstance);
                }
            }
            bossShooting.SetAttackPatterns(activeAttackPatterns);
        }

        private void SetupMovementPatternForStage(BossStage stage)
        {
            // Dọn dẹp Movement Pattern cũ
            if (activeMovementPattern != null)
            {
                Destroy(activeMovementPattern.gameObject);
                activeMovementPattern = null;
            }

            // Tạo và khởi tạo Movement Pattern mới
            if (stage.movementPattern != null)
            {
                activeMovementPattern = Instantiate(stage.movementPattern, transform);
                activeMovementPattern.Initialize(this); // SỬA ĐỔI: Khởi tạo với BossController
            }
        }
    
        private void ClearActivePatterns()
        {
            SetupAttackPatternsForStage(new BossStage()); // Gọi với stage rỗng để dọn dẹp
            SetupMovementPatternForStage(new BossStage()); // Gọi với stage rỗng để dọn dẹp
        }

        #endregion
    }
}