// FILE: _Project/Scripts/Bosses/AttackPatterns/SpreadShotPattern.cs

using System.Collections;
using UnityEngine;

namespace _Project._Scripts.Bosses.AttackPatterns
{
    public class SpreadShotPattern : AttackPattern
    {
        [Header("Spread Shot Settings")]
        public GameObject bulletPrefab;
        [Tooltip("Số lượng viên đạn trong một lần bắn.")]
        public int bulletCount = 5;
        [Tooltip("Góc tối đa mà các viên đạn sẽ phân tán ra (ví dụ: 60 độ).")]
        public float spreadAngle = 60f;
        [Tooltip("Tốc độ giữa các lần bắn.")]
        public float fireRate = 1.0f;
        [Tooltip("Độ trễ giữa việc bắn từng viên đạn trong cùng một lần bắn phân tán. Đặt là 0 để bắn ngay lập tức.")]
        public float bulletDelayInSpread = 0.05f; 
        [Tooltip("Đánh dấu nếu đạn nên hướng tới người chơi.")]
        public bool aimAtPlayer = true;

        public override IEnumerator Execute()
        {
            while (true)
            {
                Vector3 baseDirection = transform.up; // Hướng mặc định là hướng lên của boss
                if (aimAtPlayer)
                {
                    // Hướng tới người chơi
                    if (bossController.playerTransform != null)
                    {
                        baseDirection = (bossController.playerTransform.position - transform.parent.position).normalized;
                    }
                }

                // Tính toán góc bắt đầu và kết thúc của vùng phân tán
                float startAngle = -spreadAngle / 2f;
                float angleStep = spreadAngle / (bulletCount - 1); // Góc giữa các viên đạn

                if (bulletCount == 1) // Nếu chỉ có 1 viên, bắn thẳng
                {
                    angleStep = 0;
                    startAngle = 0;
                }

                for (int i = 0; i < bulletCount; i++)
                {
                    // Tính toán góc quay cho viên đạn hiện tại
                    float currentAngle = startAngle + i * angleStep;
                    
                    // Chuyển đổi hướng cơ sở sang Quaternion, sau đó áp dụng góc quay
                    Quaternion rotation = Quaternion.LookRotation(Vector3.forward, baseDirection) * Quaternion.Euler(0, 0, currentAngle);

                    GameObject bullet = ObjectPooler.Instance.GetPooledObject(bulletPrefab);
                    if (bullet != null)
                    {
                        bullet.transform.position = transform.parent.position; // Vị trí xuất phát của boss
                        bullet.transform.rotation = rotation; // Áp dụng rotation đã tính toán
                        bullet.SetActive(true);
                    }

                    if (bulletDelayInSpread > 0)
                    {
                        yield return new WaitForSeconds(bulletDelayInSpread);
                    }
                }

                yield return new WaitForSeconds(fireRate);
            }
        }
    }
}