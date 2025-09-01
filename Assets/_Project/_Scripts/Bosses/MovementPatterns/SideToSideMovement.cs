using UnityEngine;

namespace _Project._Scripts.Bosses.MovementPatterns
{
    public class SideToSideMovement : BossMovementPattern
    {
        [Header("Movement Settings")]
        [Tooltip("Tốc độ di chuyển của boss.")]
        [SerializeField] private float speed = 3f;
        [Tooltip("Biên di chuyển bên trái.")]
        [SerializeField] private float leftBound = -6f;
        [Tooltip("Biên di chuyển bên phải.")]
        [SerializeField] private float rightBound = 6f;
    
        private int direction = 1; // 1 = sang phải, -1 = sang trái

        /// <summary>
        /// Ghi đè (override) hàm Move() từ lớp cha.
        /// Hàm này chứa logic di chuyển chính và được BossController gọi liên tục.
        /// </summary>
        public override void Move()
        {
            // Chỉ di chuyển khi cờ canMove (từ lớp cha) là true
            if (!canMove || bossTransform == null) return;

            // Tính toán vị trí mới dựa trên tốc độ và hướng di chuyển
            float horizontalMovement = direction * speed * Time.fixedDeltaTime;
            bossTransform.position += new Vector3(horizontalMovement, 0, 0);

            // Đảo chiều khi chạm biên
            if (direction == 1 && bossTransform.position.x >= rightBound)
            {
                direction = -1;
            }
            else if (direction == -1 && bossTransform.position.x <= leftBound)
            {
                direction = 1;
            }
        }
    }
}