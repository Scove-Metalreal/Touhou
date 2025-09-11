// FILE: _Project/_Scripts/Player/PlayerController.cs (CẬP NHẬT ĐỂ TÍCH HỢP VỚI GAMEMANAGER)

using System.Collections;
using UnityEngine;
using _Project._Scripts.Core; // Cần thiết để tham chiếu GameManager (tùy chọn, không bắt buộc)

namespace _Project._Scripts.Player
{
    /// <summary>
    /// Xử lý toàn bộ input từ người chơi và điều khiển chuyển động, kỹ năng.
    /// Script này là trung tâm điều khiển, nhận input và ra lệnh cho các script khác (PlayerState, PlayerShooting) thực thi hành động.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D), typeof(PlayerState), typeof(PlayerShooting))]
    public class PlayerController : MonoBehaviour
    {
        private static readonly int MoveXAnimID = Animator.StringToHash("MoveX");

        [Header("Thiết lập Di chuyển Chính")]
        [SerializeField] private float moveSpeed = 5.0f;
        [SerializeField] private float focusSpeed = 2.5f;

        [Header("Thiết lập Dash (Lướt)")]
        [SerializeField] private float dashSpeed = 15f;
        [SerializeField] private float dashDuration = 0.15f;
        [SerializeField] private float dashCooldown = 1f;

        [Header("Giới hạn Di chuyển")]
        [SerializeField] private Vector2 horizontalBounds = new Vector2(-8f, 8f);
        [SerializeField] private Vector2 verticalBounds = new Vector2(-4.5f, 4.5f);
        
        [Header("Tham chiếu Đối tượng")]
        [SerializeField] private Transform hitboxIndicator;
        [SerializeField] private Transform playerVisuals;
        
        [Header("Thiết lập Hiệu ứng")]
        [SerializeField] private float maxTiltAngle = 15f;
        [SerializeField] private float tiltSpeed = 10f;
        
        [Header("Victory Sequence")]
        [Tooltip("Lực kéo người chơi lên trên sau khi thắng.")]
        [SerializeField] private float victoryPullForce = 2f;
        [Tooltip("Vị trí Y mà từ đó lực kéo bắt đầu có tác dụng.")]
        [SerializeField] private float victoryPullStartY = 0f; // 1/3 màn hình, ví dụ: 0
        
        // --- Các biến trạng thái ---
        private Rigidbody2D rb;
        private PlayerState playerState;
        private PlayerShooting playerShooting;
        private Animator animator;
        private Vector2 moveInput;
        private float currentSpeed;
        private float speedMultiplier = 1.0f;
        private bool canDash = true;
        private bool isDashing = false;
        private bool isVictoryExiting = false;
        
        // --- CÁC CỜ KIỂM SOÁT MỚI ---
        private bool canMove = false;
        private bool canShoot = false;
        
        public bool IsFocused { get; private set; } 

        #region Unity Lifecycle

        void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            playerState = GetComponent<PlayerState>();
            playerShooting = GetComponent<PlayerShooting>();
            
            if (playerVisuals != null)
                animator = playerVisuals.GetComponentInChildren<Animator>();
        }

        void Start()
        {
            if (hitboxIndicator != null)
                hitboxIndicator.gameObject.SetActive(false);
        }

        void Update()
        {
            // Nếu đang trong trạng thái lướt, không nhận thêm bất kỳ input nào
            if (isDashing) return;

            ProcessInputs();
            UpdateFocusState();
            UpdateAnimator();
            UpdateVisualsTilt();
        }

        void FixedUpdate()
        {
            // Xử lý di chuyển trong FixedUpdate để đảm bảo vật lý ổn định
            HandleMovement();
            
            // Nếu đang trong trạng thái thoát, áp dụng lực kéo
            if (isVictoryExiting)
            {
                HandleVictoryPull();
            }
        }

        #endregion
        
        // --- THÊM HÀM MỚI ---
        private void HandleVictoryPull()
        {
            // Chỉ áp dụng lực kéo nếu người chơi ở phía trên của vùng cho phép
            if (transform.position.y > victoryPullStartY)
            {
                // Thêm một lực không đổi hướng lên trên
                rb.AddForce(Vector2.up * victoryPullForce, ForceMode2D.Force);
            }
        }
        
        /// <summary>
        /// Xử lý toàn bộ input của người chơi bằng hệ thống Input cũ.
        /// </summary>
        private void ProcessInputs()
        {
            // Chỉ xử lý input di chuyển nếu được phép
            if (canMove)
            {
                moveInput = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
            }
            else
            {
                // Nếu không được phép di chuyển, đảm bảo moveInput là zero để dừng lại
                moveInput = Vector2.zero;
            }
            
            // Input Focus (giữ nút "Fire3", mặc định là Left Shift)
            IsFocused = canMove && Input.GetButton("Fire3"); // Chỉ có thể focus khi có thể di chuyển
            
            // Input Bắn (giữ nút "Fire1", mặc định là Z hoặc Left Ctrl)
            if (canShoot && Input.GetButton("Fire1"))
            {
                playerShooting.TryToShoot();
            }

            // Input Dùng Bom (nhấn 1 lần "Fire2", mặc định là X hoặc Left Alt)
            if (canMove && Input.GetButtonDown("Fire2")) // Thường thì bạn có thể dùng bom ngay cả khi không thể bắn
            {
                playerState.UseBomb();
            }
            
            // Input Dash (nhấn 1 lần "Jump", mặc định là Space)
            if (canMove && Input.GetButtonDown("Jump") && canDash && playerState.HasDashAbility())
            {
                StartCoroutine(Dash());
            }

            // Input Dùng Skill Bất Tử (nhấn 1 lần phím "E")
            if (canMove && Input.GetKeyDown(KeyCode.E))
            {
                playerState.UseInvincibilitySkill();
            }
        }
        
        #region Movement & Abilities
        
        /// <summary>
        /// Kích hoạt trạng thái thoát sau khi thắng. Player vẫn có thể di chuyển và bắn.
        /// </summary>
        public void StartVictoryExitSequence()
        {
            SetPlayerControl(true); // Đảm bảo người chơi có thể di chuyển
            isVictoryExiting = true;
            Debug.Log("[PlayerController] Victory exit sequence started. Player is being pulled upwards.");
        }

        private void HandleMovement()
        {
            // và không đang trong trạng thái lướt
            if (isDashing)
            {
                return;
            }
            
            // Nếu không được phép di chuyển, dừng người chơi lại.
            if (!canMove)
            {
                rb.linearVelocity = Vector2.zero;
                return;
            }

            
            float finalSpeed = currentSpeed * speedMultiplier;
            Vector2 targetVelocity = moveInput.normalized * finalSpeed;
            // Di chuyển bằng cách set velocity để phản ứng nhanh hơn
            rb.linearVelocity = targetVelocity; 

            // Giới hạn vị trí sau khi di chuyển
            Vector2 newPosition = rb.position;
            newPosition.x = Mathf.Clamp(newPosition.x, horizontalBounds.x, horizontalBounds.y);
            newPosition.y = Mathf.Clamp(newPosition.y, verticalBounds.x, verticalBounds.y);
            rb.position = newPosition; // Cập nhật lại vị trí đã được giới hạn
        }

        private void UpdateFocusState()
        {
            currentSpeed = IsFocused ? focusSpeed : moveSpeed;
            if (hitboxIndicator != null)
            {
                hitboxIndicator.gameObject.SetActive(IsFocused);
            }
        }

        private void UpdateAnimator()
        {
            if (animator != null)
            {
                animator.SetFloat(MoveXAnimID, moveInput.x);
            }
        }
        
        private void UpdateVisualsTilt()
        {
            if (playerVisuals == null) return;
            float targetAngle = -moveInput.x * maxTiltAngle;
            Quaternion targetRotation = Quaternion.Euler(0, 0, targetAngle);
            playerVisuals.rotation = Quaternion.Slerp(playerVisuals.rotation, targetRotation, Time.deltaTime * tiltSpeed);
        }
        
        private IEnumerator Dash()
        {
            canDash = false;
            isDashing = true;
            
            playerState.SetTemporaryInvincibility(dashDuration);

            // Tạm thời bỏ qua giới hạn di chuyển để dash mượt mà
            Vector2 dashDirection = moveInput.normalized;
            if (dashDirection == Vector2.zero)
            {
                // Nếu người chơi đứng yên và dash, dash về phía trước (lên trên)
                dashDirection = Vector2.up; 
            }
            rb.linearVelocity = dashDirection * dashSpeed;
            
            yield return new WaitForSeconds(dashDuration);
            
            rb.linearVelocity = Vector2.zero; // Dừng lại sau khi dash

            isDashing = false;
            
            // Bắt đầu đếm ngược cooldown sau khi dash kết thúc
            yield return new WaitForSeconds(dashCooldown);
            canDash = true;
        }

        #endregion

        #region Public Control Methods
        
        /// <summary>
        /// Hàm này được GameManager gọi để cho phép hoặc vô hiệu hóa khả năng di chuyển và bắn của người chơi.
        /// </summary>
        /// <param name="isEnabled">True để cho phép, False để vô hiệu hóa.</param>
        public void SetPlayerControl(bool isEnabled)
        {
            canMove = isEnabled;
            canShoot = isEnabled;
            Debug.Log($"[PlayerController] Player control set to: {isEnabled}");
            
            // Nếu bị vô hiệu hóa, đảm bảo dừng mọi chuyển động
            if (!isEnabled)
            {
                moveInput = Vector2.zero;
                rb.linearVelocity = Vector2.zero;
            }
        }

        /// <summary>
        /// Hàm này được PlayerState gọi để cập nhật hệ số nhân tốc độ khi có nâng cấp mới.
        /// </summary>
        public void SetSpeedMultiplier(float multiplier)
        {
            speedMultiplier = multiplier;
        }

        public Transform GetVisualsTransform()
        {
            return playerVisuals;
        }

        #endregion
    }
}