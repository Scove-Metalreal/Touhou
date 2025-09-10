// FILE: _Project/_Scripts/Gameplay/Items/Item.cs (PHIÊN BẢN CÓ TỰ HỦY KHI RA KHỎI MÀN HÌNH)

using _Project._Scripts.Player;
using UnityEngine;

namespace _Project._Scripts.Gameplay.Items
{
    public enum ItemType { Health, Power, Point, Bomb, Upgrade }

    [RequireComponent(typeof(SpriteRenderer))] // Đảm bảo có SpriteRenderer để OnBecameInvisible hoạt động
    public class Item : MonoBehaviour
    {
        [Header("⚙️ Thiết lập Vật phẩm")]
        [Tooltip("Chọn loại cho vật phẩm này.")]
        public ItemType itemType;
        
        [Tooltip("Giá trị số của vật phẩm.")]
        public int value;

        [Tooltip("Đánh dấu nếu đây là vật phẩm quan trọng rơi ra từ Boss và không nên tự hủy.")]
        public bool isGuaranteedLoot = false;

        [Space(10)]
        [Header("🚀 Hành vi Di chuyển")]
        [Tooltip("Tốc độ vật phẩm bị hút về phía người chơi.")]
        public float homingSpeed = 8f;
        [Tooltip("Tốc độ rơi ban đầu của vật phẩm.")]
        [SerializeField] private float initialFallSpeed = 2f;

        [Space(10)]
        [Header("🗑️ Tự động dọn dẹp")]
        [Tooltip("Thời gian tồn tại của vật phẩm (giây). Sau thời gian này vật phẩm sẽ tự hủy nếu không phải là 'Guaranteed Loot'.")]
        [SerializeField] private float lifetime = 10f;
        
        private Transform playerTarget;
        private bool isHoming = false;

        void Start()
        {
            // Hẹn giờ hủy theo thời gian
            // Chỉ áp dụng cho các vật phẩm không phải là loot quan trọng.
            if (!isGuaranteedLoot)
            {
                Destroy(gameObject, lifetime);
            }
        }

        void Update()
        {
            if (isHoming && playerTarget != null)
            {
                // Di chuyển về phía mục tiêu khi được hút
                transform.position = Vector2.MoveTowards(transform.position, playerTarget.position, homingSpeed * Time.deltaTime);
            }
            else if (!isHoming)
            {
                // Rơi xuống từ từ nếu chưa bị hút
                transform.Translate(Vector2.down * initialFallSpeed * Time.deltaTime);
            }
        }

        // --- THÊM LẠI HÀM NÀY ---
        /// <summary>
        /// Được Unity tự động gọi khi đối tượng không còn được bất kỳ camera nào nhìn thấy.
        /// </summary>
        void OnBecameInvisible()
        {
            Destroy(gameObject);
        }
        // --- KẾT THÚC PHẦN THÊM MỚI ---

        /// <summary>
        /// Bắt đầu di chuyển về phía mục tiêu (người chơi).
        /// Được gọi bởi ItemCollectionHandler.
        /// </summary>
        public void StartHoming(Transform target)
        {
            if (isHoming) return;
            isHoming = true;
            playerTarget = target;
        }

        /// <summary>
        /// Áp dụng hiệu ứng của vật phẩm lên người chơi và sau đó tự hủy.
        /// Hàm này được PlayerCollision gọi.
        /// </summary>
        public void Collect(PlayerState player)
        {
            if (player == null) return;

            switch (itemType)
            {
                case ItemType.Health:
                    player.Heal(value);
                    break;
                case ItemType.Power:
                    player.AddPower((float)value / 100f); 
                    break;
                case ItemType.Point:
                    player.AddScore(value);
                    break;
                case ItemType.Bomb:
                    player.AddBomb(value); 
                    break;
                case ItemType.Upgrade:
                    player.LevelUp();
                    break;
            }
            
            // Hủy GameObject ngay lập tức sau khi đã áp dụng hiệu ứng.
            Destroy(gameObject);
        }
    }
}