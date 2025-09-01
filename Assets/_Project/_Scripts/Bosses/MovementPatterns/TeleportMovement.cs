using System.Collections;
using UnityEngine;

namespace _Project._Scripts.Bosses.MovementPatterns
{
    public class TeleportMovement : BossMovementPattern
    {
        [Header("Teleport Settings")]
        [Tooltip("Thời gian giữa mỗi lần dịch chuyển (giây).")]
        [SerializeField] private float teleportInterval = 3f;
        [Tooltip("Góc dưới-trái của vùng có thể dịch chuyển tới.")]
        [SerializeField] private Vector2 teleportAreaMin = new Vector2(-6, 2);
        [Tooltip("Góc trên-phải của vùng có thể dịch chuyển tới.")]
        [SerializeField] private Vector2 teleportAreaMax = new Vector2(6, 4);

        private Coroutine teleportCoroutine;

        // Ghi đè StartMoving() để BẮT ĐẦU chu kỳ dịch chuyển
        public override void StartMoving()
        {
            base.StartMoving(); // Gọi hàm của lớp cha để đặt canMove = true
            
            // Dừng coroutine cũ (nếu có) để tránh chạy nhiều coroutine cùng lúc
            if (teleportCoroutine != null)
            {
                StopCoroutine(teleportCoroutine);
            }
            
            // Bắt đầu một coroutine mới nếu được phép di chuyển
            if (canMove)
            {
                teleportCoroutine = StartCoroutine(TeleportRoutine());
            }
        }

        // Ghi đè StopMoving() để DỪNG chu kỳ dịch chuyển
        public override void StopMoving()
        {
            base.StopMoving(); // Gọi hàm của lớp cha để đặt canMove = false
            
            // Dừng coroutine đang chạy
            if (teleportCoroutine != null)
            {
                StopCoroutine(teleportCoroutine);
                teleportCoroutine = null;
            }
        }

        // Hàm Move() để trống vì di chuyển không diễn ra liên tục mỗi frame.
        // Toàn bộ logic nằm trong coroutine.
        public override void Move() { }

        private IEnumerator TeleportRoutine()
        {
            // Vòng lặp sẽ chạy chừng nào cờ canMove (từ lớp cha) còn true
            while (canMove)
            {
                // Chờ khoảng thời gian định sẵn
                yield return new WaitForSeconds(teleportInterval);

                // Nếu sau khi chờ, boss vẫn còn được phép di chuyển
                if (canMove && bossTransform != null)
                {
                    // Tính toán vị trí mới ngẫu nhiên
                    float randomX = Random.Range(teleportAreaMin.x, teleportAreaMax.x);
                    float randomY = Random.Range(teleportAreaMin.y, teleportAreaMax.y);
                    Vector2 newPosition = new Vector2(randomX, randomY);

                    // (Tùy chọn) Thêm hiệu ứng "chuẩn bị dịch chuyển" ở đây
                    // yield return new WaitForSeconds(0.2f);

                    // Dịch chuyển boss đến vị trí mới bằng cách thay đổi transform
                    bossTransform.position = newPosition;
                
                    // (Tùy chọn) Thêm hiệu ứng "xuất hiện" ở đây
                }
            }
        }
    }
}