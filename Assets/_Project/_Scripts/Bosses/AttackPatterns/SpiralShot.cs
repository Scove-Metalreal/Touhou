// FILE: _Project/Scripts/Bosses/AttackPatterns/SpiralShot.cs

using System.Collections;
using UnityEngine;

namespace _Project._Scripts.Bosses.AttackPatterns
{
    public class SpiralShot : AttackPattern
    {
        public GameObject bulletPrefab;
        public int numberOfArms = 4;     // Số nhánh xoắn ốc
        public float angleIncrement = 10f; // Góc xoay mỗi lần bắn
        public float fireRate = 0.05f;

        public override IEnumerator Execute()
        {
            float[] currentAngles = new float[numberOfArms];
            // Khởi tạo góc ban đầu cho các nhánh
            for (int i = 0; i < numberOfArms; i++)
            {
                currentAngles[i] = (360f / numberOfArms) * i;
            }

            while (true)
            {
                for (int i = 0; i < numberOfArms; i++)
                {
                    GameObject bullet = ObjectPooler.Instance.GetPooledObject(bulletPrefab);
                    if (bullet != null)
                    {
                        bullet.transform.position = transform.parent.position;
                        // Bắn đạn theo góc hiện tại của nhánh
                        bullet.transform.rotation = Quaternion.Euler(0, 0, currentAngles[i]);
                        bullet.SetActive(true);
                    }
                    // Tăng góc cho lần bắn tiếp theo
                    currentAngles[i] += angleIncrement;
                }
                yield return new WaitForSeconds(fireRate);
            }
        }
    }
}