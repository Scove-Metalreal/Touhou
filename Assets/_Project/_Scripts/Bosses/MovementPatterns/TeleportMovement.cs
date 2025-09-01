// FILE: _Project/Scripts/Bosses/MovementPatterns/TeleportMovement.cs

using System.Collections;
using UnityEngine;

namespace _Project._Scripts.Bosses.MovementPatterns
{
    public class TeleportMovement : BossMovementPattern
    {
        public float teleportInterval = 3f; // Thời gian giữa mỗi lần dịch chuyển
        public Vector2 teleportAreaMin = new Vector2(-6, 2); // Góc dưới-trái của vùng dịch chuyển
        public Vector2 teleportAreaMax = new Vector2(6, 4);  // Góc trên-phải của vùng dịch chuyển

        private Coroutine teleportCoroutine;

        // Ghi đè hàm Initialize để bắt đầu Coroutine
        public override void Initialize(Rigidbody2D bossRigidbody)
        {
            base.Initialize(bossRigidbody);
            // Bắt đầu chu kỳ dịch chuyển
            if (teleportCoroutine != null) StopCoroutine(teleportCoroutine);
            teleportCoroutine = StartCoroutine(TeleportRoutine());
        }

        // Hàm Move để trống vì di chuyển không dựa trên vật lý liên tục
        public override void Move() { }

        private IEnumerator TeleportRoutine()
        {
            // Vòng lặp vô tận, sẽ bị dừng khi BossController hủy đối tượng này
            while (true)
            {
                yield return new WaitForSeconds(teleportInterval);

                // Tính toán vị trí mới ngẫu nhiên
                float randomX = Random.Range(teleportAreaMin.x, teleportAreaMax.x);
                float randomY = Random.Range(teleportAreaMin.y, teleportAreaMax.y);
                Vector2 newPosition = new Vector2(randomX, randomY);

                // (Tùy chọn) Thêm hiệu ứng "chuẩn bị dịch chuyển" ở đây
                yield return new WaitForSeconds(0.2f);

                // Dịch chuyển boss đến vị trí mới
                bossTransform.position = newPosition;

                // (Tùy chọn) Thêm hiệu ứng "xuất hiện" ở đây
            }
        }
    }
}