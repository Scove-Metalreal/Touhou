// FILE: _Project/Scripts/Bosses/AttackPatterns/BouncingShotPattern.cs

using System.Collections;
using UnityEngine;

namespace _Project._Scripts.Bosses.AttackPatterns
{
    public class BouncingShotPattern : AttackPattern
    {
        [Header("Bouncing Shot Settings")]
        public GameObject bouncingBulletPrefab;
        [Tooltip("Số lượng viên đạn mỗi lần bắn.")]
        public int bulletCount = 1;
        [Tooltip("Góc giữa các viên đạn nếu bulletCount > 1.")]
        public float angleSpread = 30f; 
        [Tooltip("Tốc độ giữa các lần bắn.")]
        public float fireRate = 2.0f;
        [Tooltip("Đánh dấu nếu đạn nên hướng tới người chơi.")]
        public bool aimAtPlayer = true;

        public override IEnumerator Execute()
        {
            while (true)
            {
                Vector3 baseDirection = transform.up; 
                if (aimAtPlayer)
                {
                    if (bossController.playerTransform != null)
                    {
                        baseDirection = (bossController.playerTransform.position - transform.parent.position).normalized;
                    }
                }

                float startAngle = 0;
                if (bulletCount > 1)
                {
                    startAngle = -angleSpread / 2f;
                }

                for (int i = 0; i < bulletCount; i++)
                {
                    float currentAngle = startAngle + (bulletCount > 1 ? i * (angleSpread / (bulletCount - 1)) : 0);
                    Quaternion rotation = Quaternion.LookRotation(Vector3.forward, baseDirection) * Quaternion.Euler(0, 0, currentAngle);

                    GameObject bullet = ObjectPooler.Instance.GetPooledObject(bouncingBulletPrefab);
                    if (bullet != null)
                    {
                        bullet.transform.position = transform.parent.position;
                        bullet.transform.rotation = rotation;
                        bullet.SetActive(true);
                        // Set velocity ở đây nếu bạn muốn, nhưng BouncingBullet.OnEnable đã làm rồi.
                        // bullet.GetComponent<Rigidbody2D>().velocity = bullet.transform.up * bullet.GetComponent<Bullet>().speed;
                    }
                }

                yield return new WaitForSeconds(fireRate);
            }
        }
    }
}