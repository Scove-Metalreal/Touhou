// FILE: _Project/_Scripts/Gameplay/Items/Item.cs (VERSION 4.0 - AUTO LEVEL UP)

using _Project._Scripts.Player;
using UnityEngine;

namespace _Project._Scripts.Gameplay.Items
{
    public enum ItemType { Health, Power, Point, Bomb, Upgrade }

    public class Item : MonoBehaviour
    {
        [Header("⚙️ Thiết lập Vật phẩm")]
        [Tooltip("Chọn loại cho vật phẩm này. Loại sẽ quyết định hành động khi người chơi nhặt.")]
        public ItemType itemType;
        
        [Tooltip("Giá trị số của vật phẩm (ví dụ: lượng máu hồi, điểm số, sức mạnh).")]
        public int value;

        // GHI CHÚ: Biến 'upgradeData' đã được xóa bỏ vì không còn cần thiết.

        [Space(10)]
        [Header("🚀 Hành vi Di chuyển")]
        [Tooltip("Tốc độ vật phẩm bị hút về phía người chơi khi ở trong tầm thu thập.")]
        public float homingSpeed = 8f;
        
        private Transform playerTarget;
        private bool isHoming = false;

        void Update()
        {
            if (isHoming && playerTarget != null)
            {
                transform.position = Vector2.MoveTowards(transform.position, playerTarget.position, homingSpeed * Time.deltaTime);
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
                    // CẬP NHẬT QUAN TRỌNG:
                    // Thay vì truyền một data cụ thể, chúng ta chỉ cần ra lệnh cho PlayerState tự lên cấp.
                    player.LevelUp();
                    break;
            }
        }
    }
}
