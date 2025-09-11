// FILE: _Project/_Scripts/Bosses/MovementPatterns/CircleMovement.cs

using UnityEngine;

namespace _Project._Scripts.Bosses.MovementPatterns
{
    public class CircleMovement : BossMovementPattern
    {
        [Header("Circle Movement Settings")]
        [Tooltip("Tốc độ di chuyển của boss quanh quỹ đạo tròn (độ/giây).")]
        [SerializeField] private float rotationSpeed = 45f;
        
        [Tooltip("Bán kính của quỹ đạo tròn.")]
        [SerializeField] private float radius = 3f;
        
        [Tooltip("Điểm trung tâm của quỹ đạo tròn. Sẽ được tính toán khi bắt đầu di chuyển.")]
        private Vector2 centerPoint;
        private float currentAngle;

        /// <summary>
        /// Ghi đè hàm StartMoving để thiết lập điểm trung tâm và góc ban đầu.
        /// </summary>
        public override void StartMoving()
        {
            base.StartMoving(); // Gọi hàm gốc để đặt canMove = true
            
            // Thiết lập điểm trung tâm của vòng tròn là vị trí hiện tại của boss trừ đi bán kính
            // Điều này làm cho boss bắt đầu di chuyển từ vị trí hiện tại của nó trên vòng tròn.
            centerPoint = new Vector2(bossTransform.position.x, bossTransform.position.y - radius);
            
            // Tính toán góc ban đầu dựa trên vị trí hiện tại so với tâm
            Vector2 initialOffset = (Vector2)bossTransform.position - centerPoint;
            currentAngle = Mathf.Atan2(initialOffset.y, initialOffset.x) * Mathf.Rad2Deg;
        }
        
        /// <summary>
        /// Di chuyển boss theo quỹ đạo tròn.
        /// </summary>
        public override void Move()
        {
            // Chỉ di chuyển nếu được phép
            if (!canMove) return;

            // Cập nhật góc dựa trên tốc độ và thời gian
            currentAngle += rotationSpeed * Time.fixedDeltaTime;

            // Tính toán vị trí mới trên vòng tròn bằng lượng giác
            // currentAngle được chuyển đổi sang radian cho các hàm Sin/Cos
            float x = centerPoint.x + radius * Mathf.Cos(currentAngle * Mathf.Deg2Rad);
            float y = centerPoint.y + radius * Mathf.Sin(currentAngle * Mathf.Deg2Rad);
            
            // Di chuyển boss đến vị trí mới
            bossTransform.position = new Vector3(x, y, bossTransform.position.z);
        }
    }
}