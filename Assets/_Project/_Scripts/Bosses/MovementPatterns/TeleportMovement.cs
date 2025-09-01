// FILE: _Project/Scripts/Bosses/MovementPatterns/TeleportMovement.cs

using System.Collections;
using UnityEngine;

namespace _Project._Scripts.Bosses.MovementPatterns
{
    public class TeleportMovement : BossMovementPattern
    {
        [Header("Teleport Settings")]
        [Tooltip("Thời gian giữa mỗi lần dịch chuyển (giây).")]
        public float teleportInterval = 3f;
        [Tooltip("Góc dưới-trái của vùng có thể dịch chuyển tới.")]
        public Vector2 teleportAreaMin = new Vector2(-6, 2);
        [Tooltip("Góc trên-phải của vùng có thể dịch chuyển tới.")]
        public Vector2 teleportAreaMax = new Vector2(6, 4);

        private Coroutine teleportCoroutine;

        // SỬA ĐỔI: Ghi đè StartMoving() để BẮT ĐẦU chu kỳ dịch chuyển
        public override void StartMoving()
        {
            base.StartMoving(); // Gọi hàm của lớp cha để đặt isMoving = true
            
            // Dừng coroutine cũ (nếu có) và bắt đầu một cái mới
            if (teleportCoroutine != null)
            {
                StopCoroutine(teleportCoroutine);
            }
            teleportCoroutine = StartCoroutine(TeleportRoutine());
        }

        // SỬA ĐỔI: Ghi đè StopMoving() để DỪNG chu kỳ dịch chuyển
        public override void StopMoving()
        {
            base.StopMoving(); // Gọi hàm của lớp cha để đặt isMoving = false và dừng vận tốc
            
            // Dừng coroutine đang chạy
            if (teleportCoroutine != null)
            {
                StopCoroutine(teleportCoroutine);
                teleportCoroutine = null;
            }
        }

        // Hàm PerformMove để trống vì di chuyển không dựa trên vật lý liên tục
        public override void PerformMove() { }

        private IEnumerator TeleportRoutine()
        {
            // Vòng lặp sẽ chạy khi isMoving là true
            while (isMoving)
            {
                // Chờ khoảng thời gian định sẵn
                yield return new WaitForSeconds(teleportInterval);

                // Tính toán vị trí mới ngẫu nhiên
                float randomX = Random.Range(teleportAreaMin.x, teleportAreaMax.x);
                float randomY = Random.Range(teleportAreaMin.y, teleportAreaMax.y);
                Vector2 newPosition = new Vector2(randomX, randomY);

                // (Tùy chọn) Thêm hiệu ứng "chuẩn bị dịch chuyển" ở đây
                // yield return new WaitForSeconds(0.2f);

                // Dịch chuyển boss đến vị trí mới qua Rigidbody để đảm bảo an toàn vật lý
                if (rb != null)
                {
                    rb.position = newPosition;
                }
                
                // (Tùy chọn) Thêm hiệu ứng "xuất hiện" ở đây
            }
        }
    }
}