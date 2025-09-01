// FILE: _Project/Scripts/Bosses/AttackPatterns/SimpleCircleShot.cs (UPDATED)

using System.Collections;
using UnityEngine;

namespace _Project._Scripts.Bosses.AttackPatterns
{
    public class SimpleCircleShot : AttackPattern
    {
        public GameObject bulletPrefab;
        public int bulletCount = 8;
        public float fireRate = 1f;

        // Implement logic tấn công bên trong hàm này
        public override IEnumerator Execute()
        {
            while (true) // Vòng lặp vẫn an toàn vì sẽ được quản lý bởi StopPattern()
            {
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
                yield return new WaitForSeconds(fireRate);
            }
        }
    }
}