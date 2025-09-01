// FILE: _Project/Scripts/Bosses/MovementPatterns/SideToSideMovement.cs

using UnityEngine;

namespace _Project._Scripts.Bosses.MovementPatterns
{
    public class SideToSideMovement : BossMovementPattern
    {
        [Header("Movement Settings")]
        [Tooltip("Tốc độ di chuyển của boss.")]
        public float speed = 3f;
        [Tooltip("Biên di chuyển bên trái.")]
        public float leftBound = -6f;
        [Tooltip("Biên di chuyển bên phải.")]
        public float rightBound = 6f;
    
        private int direction = 1; // 1 = sang phải, -1 = sang trái

        /// <summary>
        /// SỬA ĐỔI: Đổi tên hàm thành PerformMove để khớp với lớp cha.
        /// Hàm này chứa logic di chuyển chính và được gọi liên tục trong FixedUpdate.
        /// </summary>
        public override void PerformMove()
        {
            // Di chuyển Rigidbody bằng cách thay đổi vận tốc
            if (rb != null)
            {
                rb.linearVelocity = new Vector2(direction * speed, 0);
            }

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