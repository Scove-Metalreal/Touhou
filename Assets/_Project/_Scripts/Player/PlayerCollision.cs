// FILE: _Project/_Scripts/Player/PlayerCollision.cs (VERSION 3.0 - FULLY COMPATIBLE)

using _Project._Scripts.Gameplay.Items;
using _Project._Scripts.Gameplay.Projectiles;
using UnityEngine;

namespace _Project._Scripts.Player
{
    /// <summary>
    /// Xử lý các sự kiện va chạm vật lý cho người chơi.
    /// Script này chịu trách nhiệm phát hiện va chạm với đạn địch hoặc vật phẩm,
    /// sau đó thông báo cho PlayerState để xử lý logic tương ứng.
    /// Nó cũng quản lý các hiệu ứng hình ảnh và âm thanh liên quan đến va chạm và cái chết.
    /// </summary>
    [RequireComponent(typeof(AudioSource))]
    public class PlayerCollision : MonoBehaviour
    {
        // =============================================================================================
        // SECTION: KHAI BÁO BIẾN & THAM CHIẾU (VARIABLES & REFERENCES)
        // =============================================================================================

        [Header("🧩 Tham chiếu Component (Component References)")]
        [Tooltip("Kéo chính đối tượng Player (GameObject chứa script PlayerState) vào đây.")]
        [SerializeField] private PlayerState playerState;

        [Space(10)]

        [Header("💥 Hiệu ứng & Âm thanh (Effects & Sounds)")]
        [Tooltip("Âm thanh sẽ phát khi người chơi bị trúng đạn.")]
        [SerializeField] private AudioClip hitSFX; // Đổi tên từ deathSFX để rõ nghĩa hơn

        [Tooltip("Prefab hiệu ứng nổ sẽ xuất hiện khi máu người chơi về 0.")]
        [SerializeField] private GameObject deathVFX;

        [Space(10)]
        [Header("✨ Hiệu ứng Graze (Sượt đạn)")]
        [Tooltip("Prefab hiệu ứng sẽ xuất hiện khi đạn địch bay sượt qua người chơi.")]
        [SerializeField] private GameObject grazeVFX;

        [Tooltip("Âm thanh sẽ phát khi người chơi graze thành công.")]
        [SerializeField] private AudioClip grazeSFX;
    
        // --- Biến nội bộ (private) ---
        private AudioSource audioSource; // Dùng để phát âm thanh

        // =============================================================================================
        // SECTION: VÒNG ĐỜI UNITY & SỰ KIỆN (UNITY LIFECYCLE & EVENTS)
        // =============================================================================================

        void Awake()
        {
            // Tự động lấy component AudioSource để phát âm thanh.
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }
        }
        
        // Đăng ký lắng nghe sự kiện khi script được kích hoạt
        void OnEnable()
        {
            PlayerState.OnPlayerDied += HandlePlayerDeath;
        }

        // Hủy đăng ký để tránh lỗi khi đối tượng bị phá hủy
        void OnDisable()
        {
            PlayerState.OnPlayerDied -= HandlePlayerDeath;
        }

        /// <summary>
        /// Được gọi bởi Unity mỗi khi một Collider2D khác đi vào trigger của đối tượng này.
        /// </summary>
        void OnTriggerEnter2D(Collider2D other)
        {
            // --- XỬ LÝ VA CHẠM VỚI ĐẠN ĐỊCH ---
            if (other.CompareTag("EnemyBullet"))
            {
                Bullet bullet = other.GetComponent<Bullet>();
                if (bullet != null)
                {
                    // Gọi hàm xử lý va chạm và truyền vào lượng sát thương của viên đạn
                    HandleHit(bullet.Damage);
                }
                
                // Trả viên đạn về Object Pooler
                other.gameObject.SetActive(false);
            }

            // --- XỬ LÝ VA CHẠM VỚI VẬT PHẨM ---
            if (other.CompareTag("Item"))
            {
                Item item = other.GetComponent<Item>();
                if (item != null)
                {
                    // Ra lệnh cho vật phẩm áp dụng hiệu ứng và tự hủy
                    item.Collect(playerState); 
                }
            }
        }

        // =============================================================================================
        // SECTION: HÀM XỬ LÝ LOGIC (LOGIC HANDLERS)
        // =============================================================================================

        /// <summary>
        /// Xử lý logic khi người chơi bị trúng đạn (nhưng chưa chắc đã chết).
        /// </summary>
        private void HandleHit(int damageAmount)
        {
            if (playerState.IsInvincible) return;

            // Phát âm thanh bị trúng đạn
            if (hitSFX != null && audioSource != null)
            {
                audioSource.PlayOneShot(hitSFX);
            }

            // Ra lệnh cho PlayerState nhận sát thương.
            playerState.TakeDamage(damageAmount);
        }

        /// <summary>
        /// Hàm này được gọi bởi sự kiện OnPlayerDied từ PlayerState khi máu về 0.
        /// Chịu trách nhiệm cho hiệu ứng chết.
        /// </summary>
        private void HandlePlayerDeath()
        {
            // Kích hoạt hiệu ứng nổ khi chết
            if (deathVFX != null)
            {
                Instantiate(deathVFX, transform.position, Quaternion.identity);
            }
            // Logic còn lại (ẩn player, hiện màn hình game over...) đã được xử lý ở PlayerState và GameManager.
        }

        /// <summary>
        /// Xử lý khi đạn địch bay sượt qua (được gọi từ một collider riêng cho graze).
        /// </summary>
        public void HandleGraze(GameObject bullet)
        {
            // Tạo hiệu ứng và âm thanh graze.
            if (grazeVFX != null)
            {
                Instantiate(grazeVFX, bullet.transform.position, Quaternion.identity);
            }
            if (grazeSFX != null && audioSource != null)
            {
                audioSource.PlayOneShot(grazeSFX);
            }
        
            // Thêm điểm hoặc power cho người chơi (logic này có thể nằm trong PlayerState)
            playerState.AddScore(50); // Ví dụ: thêm 50 điểm cho mỗi lần graze
            Debug.Log("Graze!");
        }
    }
}

