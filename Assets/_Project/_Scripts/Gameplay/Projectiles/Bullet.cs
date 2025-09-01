// FILE: _Project/_Scripts/Gameplay/Projectiles/Bullet.cs (VERSION 4.0 - FULLY FEATURED)

using System.Collections;
using _Project._Scripts.Bosses;
using _Project._Scripts.Player;
using UnityEngine;
// Cần thiết để sử dụng Coroutine

namespace _Project._Scripts.Gameplay.Projectiles
{
    /// <summary>
    /// Quản lý mọi hành vi của một đối tượng đạn, từ di chuyển, va chạm, cho đến vòng đời.
    /// Script này được thiết kế để hoạt động hiệu quả với hệ thống Object Pooler.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))] // Đảm bảo đối tượng luôn có 2 component này
    public class Bullet : MonoBehaviour
    {
        // =============================================================================================
        // SECTION: KHAI BÁO BIẾN (VARIABLES & REFERENCES)
        // =============================================================================================

        /// <summary>
        /// Enum định nghĩa các kiểu hành vi di chuyển chính của đạn.
        /// </summary>
        public enum BulletBehavior { Straight, Homing, Explosive }

        [Header("🎯 Thông số Cơ bản")]
        [Tooltip("Tốc độ di chuyển của viên đạn.")]
        [SerializeField] private float speed = 10f;

        [Tooltip("Lượng sát thương viên đạn gây ra khi trúng mục tiêu.")]
        [SerializeField] private int damage = 1;

        [Tooltip("Đánh dấu nếu đây là đạn của địch. Dùng để xác định mục tiêu va chạm.")]
        [SerializeField] private bool isEnemyBullet = true;
    
        /// <summary>
        /// Thuộc tính public để các script khác có thể đọc được lượng sát thương của viên đạn.
        /// Ký hiệu "=>" là một cách viết tắt cho "get { return damage; }".
        /// </summary>
        public int Damage => damage;

        [Space(10)] // Thêm khoảng trống trong Inspector

        [Header("⏳ Vòng đời của Đạn (Lifetime)")]
        [Tooltip("Thời gian tối đa (giây) đạn tồn tại trước khi tự động biến mất. Đặt là 0 hoặc số âm để vô hiệu hóa tính năng này.")]
        [SerializeField] private float lifetime = 5f;

        [Space(10)]

        [Header("⚙️ Hành vi của Đạn")]
        [Tooltip("Chọn kiểu hành vi di chuyển cho viên đạn này.")]
        [SerializeField] private BulletBehavior behavior = BulletBehavior.Straight;

        [Space(10)]

        [Header("🚀 Thiết lập Homing (Tự tìm mục tiêu)")]
        [Tooltip("Tốc độ xoay của đạn khi bám theo mục tiêu. Chỉ dùng cho Behavior.Homing.")]
        [SerializeField] private float rotationSpeed = 200f;

        [Header("💥 Thiết lập Nổ (Explosive)")]
        [Tooltip("Prefab hiệu ứng nổ sẽ được tạo ra khi đạn va chạm. Chỉ dùng cho Behavior.Explosive.")]
        [SerializeField] private GameObject explosionVFX_Prefab;

        // --- Biến nội bộ (private) cho logic game ---
        private Rigidbody2D rb;
        private Transform homingTarget;
        private Camera mainCamera;
        private Coroutine lifetimeCoroutine; // Tham chiếu đến coroutine để có thể dừng nó khi cần


        // =============================================================================================
        // SECTION: VÒNG ĐỜI UNITY & KHỞI TẠO (UNITY LIFECYCLE & SETUP)
        // =============================================================================================

        #region Unity Lifecycle & Setup

        void Awake()
        {
            // Lấy các component cần thiết một lần duy nhất để tối ưu hiệu năng
            rb = GetComponent<Rigidbody2D>();
            mainCamera = Camera.main;
        }

        /// <summary>
        /// OnEnable được gọi mỗi khi viên đạn được "lấy ra" từ Object Pool và kích hoạt.
        /// Đây là nơi lý tưởng để reset trạng thái của viên đạn.
        /// </summary>
        void OnEnable()
        {
            // 1. Thiết lập quỹ đạo bay ban đầu dựa trên hành vi đã chọn
            switch (behavior)
            {
                case BulletBehavior.Straight:
                case BulletBehavior.Explosive:
                    rb.linearVelocity = transform.up * speed;
                    rb.angularVelocity = 0; // Đảm bảo đạn không tự xoay
                    break;
                case BulletBehavior.Homing:
                    FindHomingTarget(); // Đạn homing cần tìm mục tiêu ngay khi được bắn ra
                    break;
            }

            // 2. Bắt đầu Coroutine đếm ngược thời gian sống
            // Chỉ thực hiện nếu 'lifetime' được thiết lập một giá trị dương
            if (lifetime > 0f)
            {
                lifetimeCoroutine = StartCoroutine(LifetimeRoutine());
            }
        }

        /// <summary>
        /// OnDisable được gọi khi viên đạn được "trả về" Object Pool (gameObject.SetActive(false)).
        /// Rất quan trọng để dọn dẹp các tiến trình đang chạy.
        /// </summary>
        void OnDisable()
        {
            // Dừng Coroutine lifetime khi đối tượng bị vô hiệu hóa (do va chạm hoặc bay ra khỏi màn hình).
            // Việc này ngăn Coroutine tiếp tục chạy ngầm và gây ra lỗi không mong muốn.
            if (lifetimeCoroutine != null)
            {
                StopCoroutine(lifetimeCoroutine);
                lifetimeCoroutine = null; // Reset tham chiếu
            }
        }

        #endregion


        // =============================================================================================
        // SECTION: DI CHUYỂN & CẬP NHẬT (MOVEMENT & UPDATE)
        // =============================================================================================

        #region Movement & Update

        void Update()
        {
            // Chỉ xử lý logic di chuyển phức tạp trong Update nếu cần thiết
            if (behavior == BulletBehavior.Homing)
            {
                MoveHoming();
            }

            // Luôn kiểm tra xem đạn có bay ra khỏi màn hình không
            CheckIfOffScreen();
        }

        /// <summary>
        /// Xử lý logic di chuyển cho đạn Homing.
        /// </summary>
        private void MoveHoming()
        {
            // Nếu không có hoặc mất mục tiêu, đạn sẽ bay thẳng về phía trước
            if (homingTarget == null)
            {
                rb.linearVelocity = transform.up * speed;
                rb.angularVelocity = 0; // Dừng xoay
                return;
            }

            // Tính toán hướng đến mục tiêu và điều chỉnh góc bay
            Vector2 direction = (Vector2)homingTarget.position - rb.position;
            direction.Normalize();
            float rotateAmount = Vector3.Cross(direction, transform.up).z;

            rb.angularVelocity = -rotateAmount * rotationSpeed;
            rb.linearVelocity = transform.up * speed;
        }

        /// <summary>
        /// Tìm mục tiêu cho đạn Homing dựa trên việc nó là đạn của Player hay của Địch.
        /// </summary>
        private void FindHomingTarget()
        {
            string targetTag = isEnemyBullet ? "Player" : "Boss";
            GameObject targetObject = GameObject.FindGameObjectWithTag(targetTag);

            if (targetObject != null)
            {
                homingTarget = targetObject.transform;
            }
            else
            {
                // Nếu không tìm thấy mục tiêu, đặt là null để nó bay thẳng
                homingTarget = null;
            }
        }

        #endregion


        // =============================================================================================
        // SECTION: VA CHẠM & HIỆU ỨNG (COLLISION & EFFECTS)
        // =============================================================================================

        #region Collision & Effects

        void OnTriggerEnter2D(Collider2D other)
        {
            bool hitValidTarget = false;

            // Xử lý va chạm nếu là đạn địch trúng Player
            if (isEnemyBullet && other.CompareTag("Player"))
            {
                // Lấy PlayerState và gọi TakeDamage với lượng sát thương của đạn
                other.GetComponent<PlayerState>()?.TakeDamage(damage);
                hitValidTarget = true;
            }
            // Xử lý va chạm nếu là đạn Player trúng Boss
            else if (!isEnemyBullet && other.CompareTag("Boss"))
            {
                other.GetComponent<BossHealth>()?.TakeDamage(damage);
                hitValidTarget = true;
            }

            // Nếu đã va chạm với một mục tiêu hợp lệ
            if (hitValidTarget)
            {
                // Nếu là đạn nổ, tạo hiệu ứng trước khi trả về kho
                if (behavior == BulletBehavior.Explosive && explosionVFX_Prefab != null)
                {
                    Instantiate(explosionVFX_Prefab, transform.position, Quaternion.identity);
                }

                // Trả viên đạn về kho sau khi va chạm
                gameObject.SetActive(false);
            }
        }

        #endregion


        // =============================================================================================
        // SECTION: HÀM TIỆN ÍCH & COROUTINES (UTILITY & COROUTINES)
        // =============================================================================================

        #region Utility Methods & Coroutines

        /// <summary>
        /// Kiểm tra xem viên đạn đã bay ra khỏi màn hình chưa và vô hiệu hóa nếu có.
        /// </summary>
        private void CheckIfOffScreen()
        {
            if (mainCamera == null) return;

            Vector3 viewportPosition = mainCamera.WorldToViewportPoint(transform.position);
            // Thêm một khoảng đệm (0.1f) để đảm bảo đạn biến mất hoàn toàn khỏi tầm nhìn
            if (viewportPosition.x < -0.1f || viewportPosition.x > 1.1f ||
                viewportPosition.y < -0.1f || viewportPosition.y > 1.1f)
            {
                gameObject.SetActive(false);
            }
        }

        /// <summary>
        /// Coroutine này sẽ tự động vô hiệu hóa viên đạn sau một khoảng thời gian `lifetime`.
        /// </summary>
        private IEnumerator LifetimeRoutine()
        {
            // Chờ hết thời gian sống của đạn
            yield return new WaitForSeconds(lifetime);

            // Hết giờ, tự động trả về kho
            gameObject.SetActive(false);
        }

        #endregion
    }
}
