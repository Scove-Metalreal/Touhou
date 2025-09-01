using System.Collections;
using UnityEngine;

namespace _Project._Scripts.Bosses.AttackPatterns
{
    public class SimpleCircleShot : AttackPattern
    {
        public GameObject bulletPrefab; // Đạn sẽ bắn ra
        public int bulletCount = 8;    // Số lượng đạn trong 1 vòng
        public float fireRate = 1f;    // 1 giây bắn 1 lần

        public override IEnumerator Execute()
        {
            // Thay 'true' bằng 'this.enabled'.
            // Vòng lặp sẽ tự động dừng khi component này bị disable hoặc GameObject bị phá hủy.
            while (this.enabled)
            {
                // Bắn ra 8 viên đạn theo hình tròn
                for (int i = 0; i < bulletCount; i++)
                {
                    float angle = i * (360f / bulletCount);
                    GameObject bullet = ObjectPooler.Instance.GetPooledObject(bulletPrefab);
                    if (bullet != null)
                    {
                        bullet.transform.position = transform.parent.position; 
                        bullet.transform.rotation = Quaternion.Euler(0, 0, angle);
                        bullet.SetActive(true);
                    }
                }
                // Chờ 1 giây rồi lặp lại
                yield return new WaitForSeconds(fireRate);
            }
        }
    }
}