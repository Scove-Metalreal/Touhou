using System.Collections;
using UnityEngine;

namespace _Project._Scripts.Bosses.AttackPatterns
{
    public class IcicleFall : AttackPattern
    {
        public GameObject bulletPrefab;
        public float spawnRate = 0.1f; // Tốc độ rơi của đạn

        public override IEnumerator Execute()
        {
            // Lấy biên của màn hình
            float screenTop = Camera.main.orthographicSize;
            float screenWidth = screenTop * Camera.main.aspect;

            while (true)
            {
                // Tạo một vị trí ngẫu nhiên ở phía trên màn hình
                Vector2 spawnPosition = new Vector2(Random.Range(-screenWidth, screenWidth), screenTop + 1);
            
                // Lấy đạn từ pool
                GameObject bullet = ObjectPooler.Instance.GetPooledObject(bulletPrefab);
                if (bullet != null)
                {
                    bullet.transform.position = spawnPosition;
                    bullet.transform.rotation = Quaternion.Euler(0, 0, 180); // Hướng đạn bay xuống
                    bullet.SetActive(true);
                }
            
                // Chờ một chút rồi tạo viên tiếp theo
                yield return new WaitForSeconds(spawnRate);
            }
        }
    }
}