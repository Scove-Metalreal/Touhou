// 8/30/2025 AI-Tag
// This was created with the help of Assistant, a Unity Artificial Intelligence product.

using _Project._Scripts.Core;
using _Project._Scripts.Gameplay.Projectiles;
using UnityEngine;

namespace _Project._Scripts.Bosses
{
    public class BossShooting : MonoBehaviour
    {
        [Header("Shooting Settings")]
        [Tooltip("Tần suất bắn (giây/viên). (Time between shots.)")]
        public float fireRate = 1.0f;

        [Tooltip("Tốc độ của đạn. (Speed of the bullets.)")]
        public float bulletSpeed = 5.0f;

        private float fireTimer = 0f;

        void Update()
        {
            fireTimer += Time.deltaTime;

            if (fireTimer >= fireRate)
            {
                FireBullet();
                fireTimer = 0f;
            }
        }

        private void FireBullet()
        {
            // Lấy đạn từ Object Pooler
            GameObject bullet = ObjectPooler.Instance.GetPooledObject("BossBullet");
            if (bullet != null)
            {
                bullet.transform.position = transform.position;
                bullet.transform.rotation = Quaternion.identity;
                bullet.SetActive(true);

                // Thiết lập hướng và tốc độ cho đạn
                Bullet bulletScript = bullet.GetComponent<Bullet>();
                if (bulletScript != null)
                {
                    // bulletScript.SetDirection(Vector2.down); // Ví dụ: Bắn xuống dưới
                }
            }
        }
    }
}