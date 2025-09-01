// FILE: _Project/Scripts/Gameplay/Items/ItemV2.cs

using _Project._Scripts.Player;
using UnityEngine;

namespace _Project._Scripts.Gameplay.Items
{
    public enum ItemType { Health, Upgrade, Point }

    public class Item : MonoBehaviour
    {
        public ItemType itemType;
        public int value;

        [Header("Movement")]
        public float homingSpeed = 8f;
        private Transform playerTarget;
        private bool isHoming = false;

        void Update()
        {
            if (isHoming && playerTarget != null)
            {
                // Di chuyển vật phẩm về phía mục tiêu (người chơi)
                transform.position = Vector2.MoveTowards(transform.position, playerTarget.position, homingSpeed * Time.deltaTime);
            }
        }

        /// <summary>
        /// Hàm này được ItemCollectionHandler gọi để kích hoạt chế độ "hút".
        /// </summary>
        public void StartHoming(Transform target)
        {
            isHoming = true;
            playerTarget = target;
        }

        // Logic va chạm trực tiếp khi người chơi bay vào nhặt
        void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("PlayerHitbox")) // Giả sử Hitbox của bạn có tag này
            {
                PlayerState playerState = other.GetComponentInParent<PlayerState>();
                if (playerState != null)
                {
                    if (itemType == ItemType.Upgrade)
                    {
                        playerState.AddUpgrade();
                    }
                    // Thêm các logic khác cho Health, Point...
                }
                // Tạm thời hủy, sau có thể dùng pool
                Destroy(gameObject); 
            }
        }
    }
}