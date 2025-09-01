// FILE: _Project/_Scripts/Gameplay/Projectiles/Bullet.cs

using _Project._Scripts.Bosses;
using _Project._Scripts.Player;
using UnityEngine;

namespace _Project._Scripts.Gameplay.Projectiles
{
    public class Bullet : MonoBehaviour
    {
        // Enum để định nghĩa các loại hành vi của đạn
        public enum BulletBehavior { Straight, Homing, Explosive }

        [Header("Thông số Cơ bản")]
        [SerializeField] private float speed = 10f;
        [SerializeField] private int damage = 1;
        [Tooltip("Đánh dấu nếu đây là đạn của địch.")]
        [SerializeField] private bool isEnemyBullet = true;
        
        [Header("Hành vi của Đạn")]
        [Tooltip("Chọn kiểu hành vi cho viên đạn này.")]
        [SerializeField] private BulletBehavior behavior = BulletBehavior.Straight;

        [Header("Thiết lập Homing (Chỉ dùng cho Behavior.Homing)")]
        [SerializeField] private float rotationSpeed = 200f;
        
        [Header("Thiết lập Nổ (Chỉ dùng cho Behavior.Explosive)")]
        [SerializeField] private GameObject explosionVFX_Prefab;
        
        // --- Biến nội bộ ---
        private Rigidbody2D rb;
        private Transform homingTarget;
        private Camera mainCamera;

        #region Unity Lifecycle & Setup

        void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            mainCamera = Camera.main;
        }
        
        // OnEnable được gọi mỗi khi viên đạn được kích hoạt từ pool
        void OnEnable()
        {
            // Thiết lập ban đầu dựa trên hành vi
            switch (behavior)
            {
                case BulletBehavior.Straight:
                case BulletBehavior.Explosive:
                    // Đạn thẳng và đạn nổ ban đầu chỉ bay theo hướng được bắn ra
                    rb.linearVelocity = transform.up * speed;
                    rb.angularVelocity = 0; // Đảm bảo không xoay
                    break;
                case BulletBehavior.Homing:
                    // Đạn Homing cần tìm mục tiêu khi được bắn
                    FindHomingTarget();
                    break;
            }
        }
        
        #endregion

        #region Movement & Update
        
        void Update()
        {
            // Chỉ gọi logic di chuyển phức tạp nếu cần
            if (behavior == BulletBehavior.Homing)
            {
                MoveHoming();
            }
            // Logic di chuyển thẳng/nổ đã được xử lý bởi velocity trong OnEnable

            CheckIfOffScreen();
        }

        private void MoveHoming()
        {
            if (homingTarget == null)
            {
                // Nếu mất mục tiêu, bay thẳng về phía trước
                rb.linearVelocity = transform.up * speed;
                return;
            }

            Vector2 direction = (Vector2)homingTarget.position - rb.position;
            direction.Normalize();
            float rotateAmount = Vector3.Cross(direction, transform.up).z;
            
            rb.angularVelocity = -rotateAmount * rotationSpeed;
            rb.linearVelocity = transform.up * speed;
        }

        private void FindHomingTarget()
        {
            // Đạn homing của player tìm Boss, đạn của boss tìm Player
            string targetTag = isEnemyBullet ? "Player" : "Boss";
            GameObject targetObject = GameObject.FindGameObjectWithTag(targetTag);
            if (targetObject != null)
            {
                homingTarget = targetObject.transform;
            }
            else
            {
                // Nếu không tìm thấy mục tiêu, sẽ bay thẳng (xử lý trong Update)
                homingTarget = null;
            }
        }

        #endregion

        #region Collision & Effects

        void OnTriggerEnter2D(Collider2D other)
        {
            bool hitTarget = false;
            
            if (isEnemyBullet && other.CompareTag("Player"))
            {
                other.GetComponent<PlayerState>()?.TakeDamage();
                hitTarget = true;
            }
            else if (!isEnemyBullet && other.CompareTag("Boss"))
            {
                other.GetComponent<BossHealth>()?.TakeDamage(damage);
                hitTarget = true;
            }

            if (hitTarget)
            {
                // Nếu là đạn nổ, tạo hiệu ứng trước khi trả về kho
                if (behavior == BulletBehavior.Explosive && explosionVFX_Prefab != null)
                {
                    Instantiate(explosionVFX_Prefab, transform.position, Quaternion.identity);
                }
                
                // Trả về kho sau khi va chạm
                gameObject.SetActive(false);
            }
        }

        #endregion

        #region Utility Methods

        private void CheckIfOffScreen()
        {
            if (mainCamera == null) return;
            Vector3 viewportPosition = mainCamera.WorldToViewportPoint(transform.position);
            if (viewportPosition.x < -0.1f || viewportPosition.x > 1.1f ||
                viewportPosition.y < -0.1f || viewportPosition.y > 1.1f)
            {
                gameObject.SetActive(false);
            }
        }
        
        #endregion
    }
}