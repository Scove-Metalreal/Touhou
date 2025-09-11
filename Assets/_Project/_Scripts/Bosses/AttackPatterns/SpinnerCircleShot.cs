// FILE: _Project/Scripts/Bosses/AttackPatterns/SpinnerCircleShot.cs

using System.Collections;
using UnityEngine;
using _Project._Scripts.Gameplay.Projectiles; // Đảm bảo có namespace này

namespace _Project._Scripts.Bosses.AttackPatterns
{
    public class SpinnerCircleShot : AttackPattern
    {
        [Header("Spinner Circle Shot Settings")]
        public GameObject spinnerBulletPrefab; // Thay đổi tên biến
        public int bulletCount = 12; // Số lượng đạn mỗi lần bắn
        public float fireRate = 0.5f; // Tốc độ bắn
        public float spreadAngle = 360f; // Góc trải rộng của vòng đạn (thường là 360 cho vòng tròn)

        // Implement logic tấn công bên trong hàm này
        public override IEnumerator Execute()
        {
            while (true) // Vòng lặp vẫn an toàn vì sẽ được quản lý bởi StopPattern()
            {
                // Tính toán góc khởi đầu để đạn được phân bố đều
                float startAngle = Random.Range(0f, spreadAngle / bulletCount); // Bắn ngẫu nhiên một chút để đa dạng

                for (int i = 0; i < bulletCount; i++)
                {
                    // Tính toán góc cho từng viên đạn
                    float angle = startAngle + i * (spreadAngle / bulletCount);
                    
                    GameObject bullet = ObjectPooler.Instance.GetPooledObject(spinnerBulletPrefab); // Sử dụng prefab mới
                    if (bullet != null)
                    {
                        bullet.transform.position = transform.parent.position;
                        bullet.transform.rotation = Quaternion.Euler(0, 0, angle);
                        bullet.SetActive(true);
                    }
                }
                yield return new WaitForSeconds(fireRate);
            }
        }
    }
}