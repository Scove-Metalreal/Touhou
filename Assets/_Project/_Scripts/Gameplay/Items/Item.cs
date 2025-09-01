using _Project._Scripts.Player;
using UnityEngine;

namespace _Project._Scripts.Gameplay.Items
{
    public enum ItemType { Health, Power, Point, Bomb, Upgrade }

    public class Item : MonoBehaviour
    {
        [Header("⚙️ Thiết lập Vật phẩm")]
        [Tooltip("Chọn loại cho vật phẩm này.")]
        public ItemType itemType;
        
        [Tooltip("Giá trị số của vật phẩm.")]
        public int value;

        [Tooltip("Đánh dấu nếu đây là vật phẩm quan trọng rơi ra từ Boss.")]
        public bool isGuaranteedLoot = false; // <-- THÊM BIẾN NÀY

        [Space(10)]
        [Header("🚀 Hành vi Di chuyển")]
        [Tooltip("Tốc độ vật phẩm bị hút về phía người chơi.")]
        public float homingSpeed = 8f;

        [Space(10)]
        [Header("🗑️ Tự động dọn dẹp")]
        [Tooltip("Thời gian tồn tại của vật phẩm (giây). Sau thời gian này vật phẩm sẽ tự hủy.")]
        [SerializeField] private float lifetime = 15f;
        
        private Transform playerTarget;
        private bool isHoming = false;

        // --- THÊM HÀM START ---
        void Start()
        {
            // Tự động hủy vật phẩm sau một khoảng thời gian
            // Chỉ áp dụng cho các vật phẩm không phải là loot quan trọng
            if (!isGuaranteedLoot)
            {
                Destroy(gameObject, lifetime);
            }
        }

        void Update()
        {
            if (isHoming && playerTarget != null)
            {
                transform.position = Vector2.MoveTowards(transform.position, playerTarget.position, homingSpeed * Time.deltaTime);
            }
        }

        // --- THÊM HÀM ONBECAMEINVISIBLE ---
        // Hàm này được Unity tự động gọi khi vật thể không còn được camera nhìn thấy
        void OnBecameInvisible()
        {
            // Tự hủy khi ra khỏi màn hình
            // Chỉ áp dụng cho các vật phẩm không phải là loot quan trọng
            if (!isGuaranteedLoot)
            {
                Destroy(gameObject);
            }
        }

        public void StartHoming(Transform target)
        {
            isHoming = true;
            playerTarget = target;
        }

        void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Player"))
            {
                PlayerState playerState = other.GetComponent<PlayerState>();
                if (playerState != null)
                {
                    Collect(playerState);
                    Destroy(gameObject); 
                }
            }
        }
        
        private void Collect(PlayerState player)
        {
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
        }
    }
}