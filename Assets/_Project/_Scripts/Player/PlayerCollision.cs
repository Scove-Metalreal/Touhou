using UnityEngine;

namespace _Project._Scripts.Player
{
    /// <summary>
    /// Handles collision events for the player, such as being hit by enemy bullets (death)
    /// or bullets passing very close (graze).
    /// ---
    /// Xử lý các sự kiện va chạm cho người chơi, như bị trúng đạn địch (chết)
    /// hoặc khi đạn bay sượt qua (graze).
    /// </summary>
    public class PlayerCollision : MonoBehaviour
    {
        [Header("Component References")]
        [Tooltip("Kéo chính đối tượng Player (chứa script PlayerState) vào đây. (Reference to the PlayerState script.)")]
        public PlayerState playerState;

        [Header("Effects & Sounds")]
        [Tooltip("Kéo Prefab hiệu ứng nổ khi chết vào đây. (VFX Prefab for player death.)")]
        public GameObject deathVFX;

        [Tooltip("Kéo file âm thanh 'pichuun' vào đây. (SFX for player death.)")]
        public AudioClip deathSFX;

        [Tooltip("Kéo Prefab hiệu ứng khi graze vào đây. (VFX Prefab for grazing a bullet.)")]
        public GameObject grazeVFX;

        [Tooltip("Kéo file âm thanh khi graze vào đây. (SFX for grazing a bullet.)")]
        public AudioClip grazeSFX;
    
        // --- Private Variables ---
        private AudioSource audioSource; // Dùng để phát âm thanh. (Used to play audio clips.)

        /// <summary>
        /// Awake được gọi khi script được tải.
        /// (Awake is called when the script instance is being loaded.)
        /// </summary>
        void Awake()
        {
            // Tự động thêm một AudioSource nếu chưa có.
            // (Automatically add an AudioSource component if one doesn't exist.)
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }
        }
    
        /// <summary>
        /// Được gọi khi một Collider2D khác đi vào trigger của đối tượng này.
        /// (Called when another Collider2D enters this object's trigger.)
        /// </summary>
        /// <param name="other">Collider của đối tượng đã va chạm.</param>
        void OnTriggerEnter2D(Collider2D other)
        {
            // Kiểm tra tag của đối tượng va chạm.
            // (Check the tag of the colliding object.)

            // Nếu va chạm với đạn của địch.
            // (If colliding with an enemy bullet.)
            if (other.CompareTag("EnemyBullet"))
            {
                HandleDeath();
                // Destroy(other.gameObject); // Phá hủy viên đạn. (Destroy the bullet.)
                other.gameObject.SetActive(false); 
            }

            // Nếu va chạm với vật phẩm.
            // (If colliding with an item.)
            if (other.CompareTag("Item"))
            {
                // Thêm logic nhặt item ở đây.
                // (Add item collection logic here.)
                Debug.Log("Item collected!");
                Destroy(other.gameObject); // Phá hủy vật phẩm. (Destroy the item.)
            }
        }
    
        /// <summary>
        /// Xử lý logic khi người chơi chết.
        /// (Handles the player's death logic.)
        /// </summary>
        private void HandleDeath()
        {
            // Kiểm tra xem người chơi có đang bất tử hay không.
            // (Check if the player is currently invincible.)
            if (playerState.IsInvincible) return;

            // Kích hoạt hiệu ứng và âm thanh.
            // (Trigger visual and sound effects.)
            if (deathVFX != null)
            {
                Instantiate(deathVFX, transform.position, Quaternion.identity);
            }
            if (deathSFX != null && audioSource != null)
            {
                audioSource.PlayOneShot(deathSFX);
            }

            // Gọi hàm TakeDamage từ PlayerState.
            // (Call the TakeDamage function from PlayerState.)
            playerState.TakeDamage();
        
            // Có thể ẩn người chơi đi một chút trước khi respawn.
            // (You might want to temporarily hide the player before they respawn.)
            // gameObject.SetActive(false); // Ví dụ
        }

        /// <summary>
        /// (Hàm này dành cho Graze Collider) - Xử lý khi đạn sượt qua.
        /// (This function is for a separate Graze Collider) - Handles bullet grazing.
        /// </summary>
        public void HandleGraze(GameObject bullet)
        {
            // Tạo hiệu ứng và âm thanh graze.
            // (Create graze VFX and play SFX.)
            if (grazeVFX != null)
            {
                // Tạo hiệu ứng tại điểm gần nhất trên collider với viên đạn.
                // (Instantiate the effect at the closest point on the collider to the bullet.)
                Vector3 spawnPosition = bullet.transform.position; 
                Instantiate(grazeVFX, spawnPosition, Quaternion.identity);
            }
            if (grazeSFX != null && audioSource != null)
            {
                audioSource.PlayOneShot(grazeSFX);
            }
        
            // Thêm điểm hoặc power cho người chơi.
            // (Add score or power to the player.)
            Debug.Log("Graze!");
        }
    }
}