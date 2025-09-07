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
        [HideInInspector] public Transform playerTransform;
        
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
            // Dòng Initialize() này đã có sẵn và đúng vị trí.
        }
        
        
        // --- THÊM HÀM NÀY ĐỂ XỬ LÝ DI CHUYỂN ---
        void FixedUpdate()
        {
            // Nếu có một movement pattern đang hoạt động và không trong giai đoạn chuyển cảnh,
            // gọi hàm Move() của nó. Biến canMove được quản lý bên trong pattern.
            if (activeMovementPattern != null && !isTransitioning)
            {
                activeMovementPattern.Move();
            }
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
            
            // Tìm player sau khi player được Instantiate
            var playerObject = GameObject.FindWithTag("Player");
            if (playerObject != null)
            {
                playerTransform = playerObject.transform;
            }
            else
            {
                Debug.LogWarning("BossController: Player object not found with tag 'Player'!");
            }

            isFighting = true;
            currentStageIndex = 0;
            uiManager?.ShowBossUI();
            StartCoroutine(TransitionToStage(currentStageIndex));
        }

        // Được gọi bởi event OnStageHealthDepleted từ BossHealth
        private void HandleStageHealthDepleted()
        {
            if (!isFighting || isDefeated) return;

            currentStageIndex++;

            if (currentStageIndex < bossData.stages.Count)
            {
                StartCoroutine(TransitionToStage(currentStageIndex));
            }
            else
            {
                StartCoroutine(DefeatSequence());
            }
        }

        // Coroutine xử lý việc chuyển đổi giữa các stage
        private IEnumerator TransitionToStage(int stageIndex)
        {
            isTransitioning = true;
            
            StopAndClearAllPatterns();
            
            if (stageIndex > 0)
            {
                Debug.Log("Transitioning to next stage...");
                yield return new WaitForSeconds(transitionDelay);
            }

            SetupStage(bossData.stages[stageIndex]);

            isTransitioning = false;
        }

        // Thiết lập các thành phần cho một stage cụ thể
        private void SetupStage(BossStage stage)
        {
            if (stage == null) return;
            
            Debug.Log($"Setting up Stage {currentStageIndex + 1}: {(stage.isSpellCard ? stage.spellCardName : "Non-Spell")}");
            
            if (stage.isSpellCard)
            {
                uiManager?.DeclareSpellCard(stage.spellCardName, stage.timeLimit);
            }
            else
            {
                 uiManager?.HideSpellCardUI();
            }

            bossHealth.SetNewStage(stage.health);
            SetupMovementPattern(stage);
            SetupAttackPatterns(stage);
            
            // --- CẬP NHẬT TẠI ĐÂY ---
            // Bắt đầu các hành động của stage
            bossShooting.StartShooting();
            activeMovementPattern?.StartMoving(); // Kích hoạt di chuyển cho pattern mới
        }

        // Coroutine xử lý khi boss bị đánh bại hoàn toàn
        private IEnumerator DefeatSequence()
        {
            if (isDefeated) yield break;
            isDefeated = true;
            isFighting = false;

            Debug.Log($"Boss {bossData.bossName} has been defeated!");
            
            StopAndClearAllPatterns();

            if (GetComponent<Collider2D>() != null) GetComponent<Collider2D>().enabled = false;
            
            uiManager?.HideBossUI();
            uiManager?.HideSpellCardUI();

            yield return new WaitForSeconds(1f);

            DropGuaranteedLoot();
            
            GameManager.Instance?.OnBossDefeated();

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
            bossShooting.StopShooting();

            // --- CẬP NHẬT TẠI ĐÂY ---
            activeMovementPattern?.StopMoving(); // Ra lệnh cho pattern hiện tại ngừng di chuyển

            foreach (var pattern in activeAttackPatterns)
            {
                if (pattern != null)
                {
                    Destroy(pattern.gameObject);
                }
            }
            activeAttackPatterns.Clear();

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