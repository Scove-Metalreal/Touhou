// FILE: _Project/_Scripts/Player/PlayerShooting.cs (VERSION 2.0 - CORRECTED LOGIC)

using System.Collections.Generic;
using _Project._Scripts.Core;
using UnityEngine;

namespace _Project._Scripts.Player
{
    public class PlayerShooting : MonoBehaviour
    {
        [Header("Vị trí các điểm bắn")]
        [SerializeField] private Transform firePointHead;
        [SerializeField] private Transform firePointLeft;
        [SerializeField] private Transform firePointRight;

        [Header("Prefabs các loại đạn")]
        [SerializeField] private GameObject straightShotPrefab;
        [SerializeField] private GameObject diagonalShotPrefab;
        [SerializeField] private GameObject homingShotPrefab;
        [SerializeField] private GameObject cannonballPrefab;

        [Header("Thông số Bắn")]
        [Tooltip("Thời gian giữa mỗi lần bắn (giây).")]
        [SerializeField] private float fireRate = 0.1f;
        [Tooltip("Góc bắn của đạn chéo (độ).")]
        [SerializeField] private float diagonalAngle = 15f;
        
        // --- BIẾN NỘI BỘ ---
        private float fireTimer = 0f;
        private ObjectPooler objectPooler;

        // Các biến lưu trữ sức mạnh bắn hiện tại của người chơi
        private int currentStraightShots;
        private int currentDiagonalShots;
        private int currentHomingShots;
        private int currentCannonballShots;

        void Start()
        {
            objectPooler = ObjectPooler.Instance;
            if (objectPooler == null)
            {
                Debug.LogError("PlayerShooting không tìm thấy ObjectPooler trong Scene!");
            }
        }

        void Update()
        {
            // Tăng bộ đếm thời gian
            fireTimer += Time.deltaTime;
        }

        /// <summary>
        /// Hàm này được PlayerController gọi khi người chơi nhấn nút bắn.
        /// </summary>
        public void TryToShoot()
        {
            if (fireTimer >= fireRate)
            {
                Shoot();
                fireTimer = 0f;
            }
        }

        /// <summary>
        /// Hàm này được PlayerState gọi mỗi khi có sự thay đổi về nâng cấp.
        /// Nó tính toán lại toàn bộ sức mạnh bắn của người chơi.
        /// </summary>
        public void ApplyUpgrades(List<UpgradeData> upgrades)
        {
            // 1. Reset về vũ khí cơ bản
            //    Đây là điểm quan trọng: Player luôn có ít nhất 2 đạn bắn thẳng.
            currentStraightShots = 2; 
            currentDiagonalShots = 0;
            currentHomingShots = 0;
            currentCannonballShots = 0;

            // 2. Cộng dồn hiệu ứng từ tất cả các nâng cấp đã nhặt
            if (upgrades != null)
            {
                foreach (var upgrade in upgrades)
                {
                    currentStraightShots += upgrade.straightShots;
                    currentDiagonalShots += upgrade.diagonalShots;
                    currentHomingShots += upgrade.homingShots;
                    currentCannonballShots += upgrade.cannonballShots;
                }
            }
            Debug.Log($"Đã cập nhật sức mạnh: {currentStraightShots} thẳng, {currentDiagonalShots} chéo.");
        }
        
        /// <summary>
        /// Thực hiện việc bắn đạn dựa trên các thông số đã được tính toán.
        /// </summary>
        private void Shoot()
        {
            if (objectPooler == null) return;
            
            HandleStraightShots();
            HandleDiagonalShots();
            HandleHomingShots();
            HandleCannonballShots();
        }
        
        // --- CÁC HÀM HỖ TRỢ CHO VIỆC BẮN ---

        private void HandleStraightShots()
        {
            if (currentStraightShots <= 0 || straightShotPrefab == null) return;

            // Logic phân bổ đạn thẳng
            if (currentStraightShots == 1)
            {
                SpawnProjectile(straightShotPrefab, firePointHead);
            }
            else // 2 hoặc nhiều hơn, bắn từ 2 bên
            {
                SpawnProjectile(straightShotPrefab, firePointLeft);
                SpawnProjectile(straightShotPrefab, firePointRight);
                
                // Nếu có 3 đạn trở lên, thêm 1 viên ở giữa
                if (currentStraightShots >= 3)
                {
                     SpawnProjectile(straightShotPrefab, firePointHead);
                }
            }
        }

        private void HandleDiagonalShots()
        {
            if (currentDiagonalShots <= 0 || diagonalShotPrefab == null) return;

            // Mỗi 'điểm' diagonalShots sẽ bắn ra 2 viên đạn chéo
            for (int i = 0; i < currentDiagonalShots; i++)
            {
                Quaternion leftRotation = firePointLeft.rotation * Quaternion.Euler(0, 0, -diagonalAngle * (i + 1));
                Quaternion rightRotation = firePointRight.rotation * Quaternion.Euler(0, 0, diagonalAngle * (i + 1));
        
                SpawnProjectile(diagonalShotPrefab, firePointLeft, leftRotation);
                SpawnProjectile(diagonalShotPrefab, firePointRight, rightRotation);
            }
        }

        private void HandleHomingShots()
        {
            if (currentHomingShots <= 0 || homingShotPrefab == null) return;
            for (int i = 0; i < currentHomingShots; i++)
                SpawnProjectile(homingShotPrefab, firePointHead);
        }

        private void HandleCannonballShots()
        {
            if (currentCannonballShots <= 0 || cannonballPrefab == null) return;
            for (int i = 0; i < currentCannonballShots; i++)
                SpawnProjectile(cannonballPrefab, firePointHead);
        }
    
        private void SpawnProjectile(GameObject prefab, Transform spawnPoint, Quaternion? rotation = null)
        {
            GameObject projectile = objectPooler.GetPooledObject(prefab);
            if (projectile != null)
            {
                projectile.transform.position = spawnPoint.position;
                projectile.transform.rotation = rotation ?? spawnPoint.rotation; 
                projectile.SetActive(true);
            }
        }
    }
}
