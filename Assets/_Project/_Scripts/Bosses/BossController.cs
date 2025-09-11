// FILE: _Project/_Scripts/Bosses/BossController.cs (PHIÊN BẢN PERSISTENT-AWARE)

using System;
using System.Collections;
using System.Collections.Generic;
using _Project._Scripts.Bosses.AttackPatterns;
using _Project._Scripts.Bosses.MovementPatterns;
using _Project._Scripts.Core; // Cần namespace này để truy cập GameManager
using _Project._Scripts.Gameplay.LootSystem;
using _Project._Scripts.UI;
using UnityEngine;

namespace _Project._Scripts.Bosses
{
    [RequireComponent(typeof(BossHealth))]
    [RequireComponent(typeof(BossShooting))]
    [RequireComponent(typeof(Rigidbody2D))] // Đảm bảo Boss có Rigidbody2D để di chuyển
    public class BossController : MonoBehaviour
    {
        [Header("Configuration")]
        [Tooltip("ScriptableObject chứa dữ liệu cụ thể cho boss này (máu, pattern, v.v.).")]
        public BossData bossData;
        [SerializeField] private float transitionDelay = 2.0f; // Thời gian chờ giữa các stage

        // --- References ---
        private Rigidbody2D rb;
        private BossHealth bossHealth;
        private BossShooting bossShooting;
        private UIManager uiManager;

        // --- Tham chiếu Player (lấy từ GameManager) ---
        [HideInInspector] // Ẩn khỏi Inspector vì nó sẽ được gán bằng code
        public Transform playerTransform; // Để các AttackPattern/MovementPattern có thể tìm Player

        // --- State ---
        private int currentStageIndex = -1; // -1 ban đầu, 0 khi bắt đầu stage đầu tiên
        private List<AttackPattern> activeAttackPatterns = new List<AttackPattern>();
        private BossMovementPattern activeMovementPattern;

        // Cờ trạng thái để quản lý luồng hoạt động
        private bool isFighting = false;
        private bool isTransitioning = false;
        private bool isDefeated = false;

        #region Unity Lifecycle

        private void Awake()
        {
            // Lấy các component cần thiết một lần duy nhất
            rb = GetComponent<Rigidbody2D>();
            bossHealth = GetComponent<BossHealth>();
            bossShooting = GetComponent<BossShooting>();

            // Cấu hình Rigidbody: Boss di chuyển qua transform (Kinematic)
            rb.gravityScale = 0; // Không bị ảnh hưởng bởi trọng lực
            rb.isKinematic = true; // Di chuyển thủ công qua transform
            rb.constraints = RigidbodyConstraints2D.FreezeRotation; // Không tự xoay
        }

        private void Start()
        {
            // Lấy tham chiếu đến UIManager (Singleton)
            uiManager = UIManager.Instance;

            // Tìm và gán playerTransform từ GameManager
            // Đây là bước quan trọng để các pattern có thể nhắm mục tiêu vào Player
            if (GameManager.Instance != null && GameManager.Instance.PlayerObject != null)
            {
                playerTransform = GameManager.Instance.PlayerObject.transform;
            }
            else
            {
                Debug.LogWarning("BossController: Player object not found from GameManager. Boss patterns might not aim correctly!", this);
                // Có thể tắt boss hoặc có logic khác nếu không tìm thấy Player
            }
        }

        /// <summary>
        /// FixedUpdate được gọi theo một khoảng thời gian cố định, lý tưởng cho vật lý và di chuyển Boss.
        /// </summary>
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
            // Đăng ký sự kiện khi máu của stage hiện tại của boss cạn kiệt
            bossHealth.OnStageHealthDepleted += HandleStageHealthDepleted;
        }

        private void OnDisable()
        {
            // Hủy đăng ký sự kiện để tránh lỗi và memory leak khi boss bị hủy
            bossHealth.OnStageHealthDepleted -= HandleStageHealthDepleted;
        }

        #endregion

        #region Core Fight Loop

        /// <summary>
        /// Được GameManager gọi để khởi tạo boss khi vào một scene mới.
        /// </summary>
        public void Initialize()
        {
            if (bossData == null || bossData.stages == null || bossData.stages.Count == 0)
            {
                Debug.LogError("BossData is not assigned or has no stages! Boss cannot initialize.", this);
                gameObject.SetActive(false); // Vô hiệu hóa boss nếu không có dữ liệu
                return;
            }

            // Đảm bảo playerTransform được cập nhật trước khi boss bắt đầu tấn công
            if (GameManager.Instance != null && GameManager.Instance.PlayerObject != null)
            {
                playerTransform = GameManager.Instance.PlayerObject.transform;
            }
            else
            {
                Debug.LogError("BossController: Cannot initialize. Player object not found from GameManager! Ensure Player is spawned.", this);
                gameObject.SetActive(false);
                return;
            }

            isFighting = true;
            isDefeated = false; // Reset trạng thái nếu boss được tái sử dụng
            currentStageIndex = 0; // Bắt đầu từ stage đầu tiên
            uiManager?.ShowBossUI(); // Hiển thị UI Boss Bar

            // Bắt đầu trình tự chuyển sang stage đầu tiên
            StartCoroutine(TransitionToStage(currentStageIndex));
            Debug.Log($"BossController: Boss {bossData.bossName} Initialized.");
        }

        /// <summary>
        /// Được gọi bởi event OnStageHealthDepleted từ BossHealth khi máu của một stage cạn.
        /// </summary>
        private void HandleStageHealthDepleted()
        {
            if (!isFighting || isDefeated) return; // Chỉ xử lý khi đang chiến đấu và chưa bị đánh bại

            Debug.Log($"BossController: Stage {currentStageIndex + 1} health depleted.");
            currentStageIndex++; // Chuyển sang stage tiếp theo

            if (currentStageIndex < bossData.stages.Count)
            {
                // Nếu vẫn còn stage, chuyển sang stage đó
                StartCoroutine(TransitionToStage(currentStageIndex));
            }
            else
            {
                // Hết tất cả các stage, boss bị đánh bại
                StartCoroutine(DefeatSequence());
            }
        }

        /// <summary>
        /// Coroutine xử lý việc chuyển đổi giữa các stage của boss.
        /// </summary>
        private IEnumerator TransitionToStage(int stageIndex)
        {
            isTransitioning = true; // Bật cờ trạng thái chuyển cảnh

            StopAndClearAllPatterns(); // Dừng tất cả các pattern đang chạy của stage trước

            if (stageIndex > 0) // Chỉ chờ nếu không phải là stage đầu tiên
            {
                Debug.Log($"BossController: Transitioning to Stage {stageIndex + 1}...");
                yield return new WaitForSeconds(transitionDelay);
            }

            SetupStage(bossData.stages[stageIndex]); // Thiết lập stage mới

            isTransitioning = false; // Tắt cờ trạng thái chuyển cảnh
            Debug.Log($"BossController: Transition to Stage {stageIndex + 1} complete.");
        }

        /// <summary>
        /// Thiết lập các thành phần cho một stage cụ thể của boss (máu, pattern, di chuyển).
        /// </summary>
        private void SetupStage(BossStage stage)
        {
            if (stage == null)
            {
                Debug.LogError("BossController: Attempted to setup a null BossStage!", this);
                return;
            }

            Debug.Log($"Setting up Stage {currentStageIndex + 1}: {(stage.isSpellCard ? "Spell Card - " + stage.spellCardName : "Normal Stage")}");

            // Cập nhật UI cho Spell Card nếu có
            if (stage.isSpellCard)
            {
                uiManager?.DeclareSpellCard(stage.spellCardName, stage.timeLimit);
            }
            else
            {
                uiManager?.HideSpellCardUI(); // Ẩn UI Spell Card nếu không phải
            }

            bossHealth.SetNewStage(stage.health); // Đặt lại máu cho stage mới
            SetupMovementPattern(stage); // Thiết lập kiểu di chuyển
            SetupAttackPatterns(stage); // Thiết lập các kiểu tấn công

            // Bắt đầu các hành động của stage
            bossShooting.StartShooting(); // Bắt đầu bắn đạn
            activeMovementPattern?.StartMoving(); // Kích hoạt di chuyển cho pattern mới
        }

        /// <summary>
        /// Coroutine xử lý khi boss bị đánh bại hoàn toàn.
        /// </summary>
        private IEnumerator DefeatSequence()
        {
            if (isDefeated) yield break; // Ngăn không cho chạy lại
            isDefeated = true;
            isFighting = false;

            Debug.Log($"Boss {bossData.bossName} has been completely defeated!");

            StopAndClearAllPatterns(); // Dừng và dọn dẹp tất cả các pattern

            // Vô hiệu hóa Collider của boss để tránh va chạm thêm
            if (GetComponent<Collider2D>() != null) GetComponent<Collider2D>().enabled = false;

            uiManager?.HideBossUI(); // Ẩn UI Boss Bar
            uiManager?.HideSpellCardUI(); // Ẩn UI Spell Card

            yield return new WaitForSeconds(1f); // Chờ một chút

            DropGuaranteedLoot(); // Rớt vật phẩm chắc chắn

            GameManager.Instance?.OnBossDefeated(); // Thông báo cho GameManager rằng boss đã bị đánh bại

            // Có thể thêm hiệu ứng nổ lớn hoặc animation chết ở đây
            // Destroy(gameObject, 2f); // Boss GameObject sẽ bị GameManager hủy khi chuyển scene
        }

        #endregion

        #region Pattern & Loot Management

        /// <summary>
        /// Thiết lập các attack pattern cho stage hiện tại.
        /// </summary>
        private void SetupAttackPatterns(BossStage stage)
        {
            // Các pattern được Instantiate làm con của BossController để dễ quản lý lifecycle
            foreach (var patternPrefab in stage.attackPatterns)
            {
                if (patternPrefab != null)
                {
                    // Tạo một bản sao của AttackPattern prefab và gán làm con của boss
                    AttackPattern newInstance = Instantiate(patternPrefab, transform);
                    newInstance.Initialize(this); // Khởi tạo pattern với tham chiếu BossController này
                    activeAttackPatterns.Add(newInstance); // Thêm vào danh sách đang hoạt động
                }
                else
                {
                    Debug.LogWarning($"BossController: Null AttackPattern prefab found in BossData for stage {currentStageIndex + 1}.", this);
                }
            }
            bossShooting.SetAttackPatterns(activeAttackPatterns); // Gán các pattern cho BossShooting
        }

        /// <summary>
        /// Thiết lập movement pattern cho stage hiện tại.
        /// </summary>
        private void SetupMovementPattern(BossStage stage)
        {
            if (stage.movementPattern != null)
            {
                // Tạo một bản sao của BossMovementPattern prefab và gán làm con của boss
                activeMovementPattern = Instantiate(stage.movementPattern, transform);
                activeMovementPattern.Initialize(this); // Khởi tạo pattern với tham chiếu BossController này
            }
            else
            {
                Debug.LogWarning($"BossController: No MovementPattern assigned for stage {currentStageIndex + 1}.", this);
            }
        }

        /// <summary>
        /// Hàm dọn dẹp TẤT CẢ các pattern đang hoạt động (cả tấn công và di chuyển).
        /// </summary>
        private void StopAndClearAllPatterns()
        {
            bossShooting.StopShooting(); // Dừng tất cả các pattern tấn công

            activeMovementPattern?.StopMoving(); // Yêu cầu pattern di chuyển ngừng hoạt động

            // Hủy tất cả các AttackPattern đã được Instantiate
            foreach (var pattern in activeAttackPatterns)
            {
                if (pattern != null)
                {
                    Destroy(pattern.gameObject);
                }
            }
            activeAttackPatterns.Clear(); // Xóa danh sách

            // Hủy MovementPattern đã được Instantiate
            if (activeMovementPattern != null)
            {
                Destroy(activeMovementPattern.gameObject);
                activeMovementPattern = null;
            }
            Debug.Log("BossController: All patterns stopped and cleared.");
        }

        /// <summary>
        /// Rớt các vật phẩm chắc chắn khi boss bị đánh bại hoàn toàn.
        /// </summary>
        private void DropGuaranteedLoot()
        {
            if (bossData.guaranteedUpgradeDrop != null)
            {
                Instantiate(bossData.guaranteedUpgradeDrop, transform.position, Quaternion.identity);
                Debug.Log("BossController: Dropped guaranteed upgrade item.");
            }
            if (bossData.rareHealthDrop != null)
            {
                Instantiate(bossData.rareHealthDrop, transform.position, Quaternion.identity);
                Debug.Log("BossController: Dropped rare health item.");
            }
        }

        #endregion
    }
}