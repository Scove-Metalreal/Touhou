// FILE: _Project/Scripts/Player/PlayerController.cs

using System.Collections;
using UnityEngine;

namespace _Project._Scripts.Player
{
    [RequireComponent(typeof(Rigidbody2D), typeof(PlayerState))]
    public class PlayerController : MonoBehaviour
    {
        // Tối ưu hóa: Chuyển string thành hash ID một lần duy nhất
        private static readonly int MoveXAnimID = Animator.StringToHash("MoveX");

        [Header("Thiết lập Di chuyển Chính")]
        [SerializeField] private float moveSpeed = 5.0f;
        [SerializeField] private float focusSpeed = 2.5f;

        [Header("Thiết lập Dash (Lướt)")]
        [SerializeField] private float dashSpeed = 15f;
        [SerializeField] private float dashDuration = 0.15f;
        [SerializeField] private float dashCooldown = 1f;

        [Header("Giới hạn Di chuyển trên Màn hình")]
        [SerializeField] private Vector2 horizontalBounds = new Vector2(-8f, 8f);
        [SerializeField] private Vector2 verticalBounds = new Vector2(-4.5f, 4.5f);
        
        [Header("Tham chiếu Đối tượng")]
        [Tooltip("Kéo đối tượng HitboxIndicator từ Hierarchy vào đây.")]
        [SerializeField] private Transform hitboxIndicator;
        
        [Tooltip("Kéo đối tượng cha chứa hình ảnh của player vào đây (đối tượng có Animator).")]
        [SerializeField] private Transform playerVisuals;
        
        [Header("Thiết lập Hiệu ứng Hình ảnh")]
        [Tooltip("Góc nghiêng tối đa (độ) khi người chơi di chuyển ngang.")]
        [SerializeField] private float maxTiltAngle = 15f;
        [Tooltip("Tốc độ quay trở lại vị trí thẳng đứng.")]
        [SerializeField] private float tiltSpeed = 10f;
        
        // --- Trạng thái Nội bộ ---
        private Rigidbody2D rb;
        private PlayerState playerState;
        private Animator animator; // Sẽ được lấy từ playerVisuals

        private Vector2 moveInput;
        private float currentSpeed;
        private float speedMultiplier = 1.0f;

        private bool canDash = true;
        private bool isDashing = false;
        
        public bool IsFocused { get; private set; } 

        #region Unity Lifecycle Methods
        
        void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            playerState = GetComponent<PlayerState>();
            
            // Lấy Animator từ playerVisuals để không cần một ô riêng trong Inspector
            if (playerVisuals != null)
            {
                animator = playerVisuals.GetComponentInChildren<Animator>();
            }
            if(animator == null)
            {
                Debug.LogWarning("PlayerController không tìm thấy Animator trên hoặc trong các con của playerVisuals.");
            }
        }

        void Start()
        {
            if (hitboxIndicator != null)
            {
                hitboxIndicator.gameObject.SetActive(false);
            }
        }

        void Update()
        {
            // Không nhận input nếu đang dashing
            if (isDashing) return;

            ProcessInputs();
            UpdateFocusState();
            UpdateAnimator();
            UpdateVisualsTilt();
        }

        void FixedUpdate()
        {
            HandleMovement();
        }

        #endregion

        #region Movement & Input
        
        private void ProcessInputs()
        {
            moveInput = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
            IsFocused = Input.GetButton("Fire3");
            
            if (Input.GetButtonDown("Jump") && canDash && playerState.CurrentUpgrade.hasDash)
            {
                StartCoroutine(Dash());
            }
        }

        private void HandleMovement()
        {
            if (isDashing) return;

            float speed = currentSpeed * speedMultiplier;
            Vector2 newPosition = rb.position + moveInput.normalized * speed * Time.fixedDeltaTime;

            newPosition.x = Mathf.Clamp(newPosition.x, horizontalBounds.x, horizontalBounds.y);
            // Sửa lỗi ở đây: dùng verticalBounds.x cho min và verticalBounds.y cho max
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
        
        /// <summary>
        /// Cập nhật hiệu ứng nghiêng của sprite dựa trên hướng di chuyển ngang.
        /// </summary>
        private void UpdateVisualsTilt()
        {
            if (playerVisuals == null) return;

            // Tính toán góc nghiêng mục tiêu dựa trên input trục X (-1 đến 1)
            // Di chuyển sang phải (input.x > 0) -> góc nghiêng âm (nghiêng sang phải)
            // Di chuyển sang trái (input.x < 0) -> góc nghiêng dương (nghiêng sang trái)
            float targetAngle = -moveInput.x * maxTiltAngle;

            // Tạo một rotatiton mục tiêu
            Quaternion targetRotation = Quaternion.Euler(0, 0, targetAngle);

            // Dùng Slerp để xoay đối tượng visuals một cách mượt mà về phía góc mục tiêu
            playerVisuals.rotation = Quaternion.Slerp(playerVisuals.rotation, targetRotation, Time.deltaTime * tiltSpeed);
        }

        #endregion

        #region Abilities

        private IEnumerator Dash()
        {
            canDash = false;
            isDashing = true;
            
            // Gợi ý: Khi dash, người chơi nên tạm thời bất tử. 
            // Bạn cần tạo hàm SetTemporaryInvincibility(float duration) trong PlayerState
            // Ví dụ: playerState.SetTemporaryInvincibility(dashDuration); 

            rb.linearVelocity = moveInput.normalized * dashSpeed;
            yield return new WaitForSeconds(dashDuration);
            rb.linearVelocity = Vector2.zero;

            isDashing = false;
            
            yield return new WaitForSeconds(dashCooldown);
            canDash = true;
        }

        public void SetSpeedMultiplier(float multiplier)
        {
            speedMultiplier = multiplier;
        }

        #endregion
        
        // --- Hàm Public để truy cập Visuals ---

        /// <summary>
        /// Trả về Transform của đối tượng hình ảnh (Sprite).
        /// Các script khác có thể gọi hàm này để áp dụng hiệu ứng (ví dụ: nhấp nháy).
        /// </summary>
        public Transform GetVisualsTransform()
        {
            return playerVisuals;
        }
    }
}