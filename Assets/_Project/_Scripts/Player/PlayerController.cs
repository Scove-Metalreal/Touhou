// FILE: _Project/_Scripts/Player/PlayerController.cs (VERSION 6.0 - FINAL & STABLE)

using System.Collections;
using UnityEngine;

namespace _Project._Scripts.Player
{
    /// <summary>
    /// Xử lý toàn bộ input từ người chơi và điều khiển chuyển động, kỹ năng.
    /// Script này là trung tâm điều khiển, nhận input và ra lệnh cho các script khác (PlayerState, PlayerShooting) thực thi hành động.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D), typeof(PlayerState), typeof(PlayerShooting))]
    public class PlayerController : MonoBehaviour
    {
        // Tối ưu hóa: Chuyển string thành hash ID một lần duy nhất để Animator hoạt động hiệu quả hơn
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
        }

        #endregion
        
        /// <summary>
        /// Xử lý toàn bộ input của người chơi bằng hệ thống Input cũ.
        /// </summary>
        private void ProcessInputs()
        {
            // Input di chuyển
            moveInput = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
            
            // Input Focus (giữ nút "Fire3", mặc định là Left Shift)
            IsFocused = Input.GetButton("Fire3");
            
            // Input Bắn (giữ nút "Fire1", mặc định là Z hoặc Left Ctrl)
            if (Input.GetButton("Fire1"))
            {
                playerShooting.TryToShoot();
            }

            // Input Dùng Bom (nhấn 1 lần "Fire2", mặc định là X hoặc Left Alt)
            if (Input.GetButtonDown("Fire2"))
            {
                playerState.UseBomb();
            }
            
            // Input Dash (nhấn 1 lần "Jump", mặc định là Space)
            if (Input.GetButtonDown("Jump") && canDash && playerState.HasDashAbility())
            {
                StartCoroutine(Dash());
            }

            // Input Dùng Skill Bất Tử (nhấn 1 lần phím "E")
            if (Input.GetKeyDown(KeyCode.E))
            {
                playerState.UseInvincibilitySkill();
            }
        }
        
        #region Movement & Abilities

        private void HandleMovement()
        {
            if (isDashing) return;
            float finalSpeed = currentSpeed * speedMultiplier;
            Vector2 newPosition = rb.position + moveInput.normalized * finalSpeed * Time.fixedDeltaTime;
            newPosition.x = Mathf.Clamp(newPosition.x, horizontalBounds.x, horizontalBounds.y);
            newPosition.y = Mathf.Clamp(newPosition.y, verticalBounds.x, verticalBounds.y);
            rb.MovePosition(newPosition);
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
            
            // Kích hoạt bất tử tạm thời trong suốt thời gian lướt
            playerState.SetTemporaryInvincibility(dashDuration);

            rb.linearVelocity = moveInput.normalized * dashSpeed;
            yield return new WaitForSeconds(dashDuration);
            rb.linearVelocity = Vector2.zero;

            isDashing = false;
            
            // Bắt đầu đếm ngược cooldown sau khi dash kết thúc
            yield return new WaitForSeconds(dashCooldown);
            canDash = true;
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

