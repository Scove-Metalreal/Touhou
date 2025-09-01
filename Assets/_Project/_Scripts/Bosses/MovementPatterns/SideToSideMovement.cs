// FILE: _Project/Scripts/Bosses/MovementPatterns/SideToSideMovement.cs

using UnityEngine;

namespace _Project._Scripts.Bosses.MovementPatterns
{
    public class SideToSideMovement : BossMovementPattern
    {
        public float speed = 3f;
        public float leftBound = -6f; // Biên di chuyển trái
        public float rightBound = 6f; // Biên di chuyển phải
    
        private int direction = 1; // 1 = sang phải, -1 = sang trái

        public override void Move()
        {
            // Di chuyển Rigidbody
            rb.linearVelocity = new Vector2(direction * speed, 0);

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