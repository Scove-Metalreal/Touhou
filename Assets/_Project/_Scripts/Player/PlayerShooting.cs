// FILE: _Project/Scripts/Player/PlayerShooting.cs

using _Project._Scripts.Core;
using UnityEngine;

namespace _Project._Scripts.Player
{
    /// <summary>
    /// Quản lý toàn bộ logic bắn của người chơi.
    /// Script này đọc dữ liệu nâng cấp từ PlayerState để quyết định
    /// loại đạn, số lượng đạn, và cách bắn.
    /// </summary>
    public class PlayerShooting : MonoBehaviour
    {
        [Header("Vị trí các điểm bắn")]
        [Tooltip("Điểm bắn ở giữa.")]
        public Transform firePointHead;
        [Tooltip("Điểm bắn bên trái.")]
        public Transform firePointLeft;
        [Tooltip("Điểm bắn bên phải.")]
        public Transform firePointRight;

        [Header("Prefabs các loại đạn")]
        [Tooltip("Prefab cho đạn bắn thẳng.")]
        public GameObject straightShotPrefab;
        [Tooltip("Prefab cho đạn bắn chéo.")]
        public GameObject diagonalShotPrefab;
        [Tooltip("Prefab cho đạn tự tìm mục tiêu.")]
        public GameObject homingShotPrefab;
        [Tooltip("Prefab cho đạn nổ.")]
        public GameObject cannonballPrefab;
        [Tooltip("Prefab cho quả bom.")]
        public GameObject bombPrefab;

        [Header("Thông số Bắn")]
        [Tooltip("Thời gian giữa mỗi lần bắn (giây). 0.1 = 10 viên/giây.")]
        public float fireRate = 0.1f;
        [Tooltip("Góc bắn của đạn chéo (độ).")]
        [SerializeField] private float diagonalAngle = 15f;


        // --- BIẾN NỘI BỘ ---
        private float fireTimer = 0f;
        private PlayerState playerState;
        private ObjectPooler objectPooler;

        /// <summary>
        /// Awake được gọi khi script được tải.
        /// </summary>
        void Awake()
        {
            // Lấy tham chiếu đến PlayerState trên cùng GameObject.
            playerState = GetComponent<PlayerState>();
            if (playerState == null)
            {
                Debug.LogError("PlayerShooting không tìm thấy PlayerState component!");
            }
        }

        /// <summary>
        /// Start được gọi trước frame đầu tiên.
        /// </summary>
        void Start()
        {
            // Lấy tham chiếu đến ObjectPooler Singleton.
            objectPooler = ObjectPooler.Instance;
            if (objectPooler == null)
            {
                Debug.LogError("PlayerShooting không tìm thấy ObjectPooler trong Scene!");
            }
        }

        /// <summary>
        /// Update được gọi mỗi frame.
        /// </summary>
        void Update()
        {
            fireTimer += Time.deltaTime;

            // Xử lý bắn đạn liên tục khi giữ nút "Fire1" (mặc định là Z hoặc Chuột Trái).
            if (Input.GetButton("Fire1") && fireTimer >= fireRate)
            {
                Shoot();
                fireTimer = 0f; // Reset bộ đếm.
            }
        
            // Xử lý dùng bom khi nhấn nút "Fire2" (mặc định là X hoặc Chuột Phải).
            if (Input.GetButtonDown("Fire2"))
            {
                UseBomb();
            }
        }
    
        /// <summary>
        /// Hàm chính thực hiện việc bắn đạn dựa trên cấp độ hiện tại.
        /// </summary>
        private void Shoot()
        {
            // Guard clause: Không bắn nếu các component cần thiết chưa sẵn sàng.
            if (playerState == null || objectPooler == null) return;

            // Lấy thông tin về cấp độ nâng cấp hiện tại từ PlayerState.
            UpgradeData currentUpgrade = playerState.CurrentUpgrade;
            if (currentUpgrade == null) return;

            
            // Bắn đạn thẳng
            HandleStraightShots(currentUpgrade);

            // Bắn đạn chéo
            HandleDiagonalShots(currentUpgrade);
        
            // Bắn đạn homing
            HandleHomingShots(currentUpgrade);

            // Bắn đạn cannonball
            HandleCannonballShots(currentUpgrade);
        }
    
        // --- CÁC HÀM HỖ TRỢ CHO VIỆC BẮN ---

        private void HandleStraightShots(UpgradeData upgrade)
        {
            if (upgrade.straightShots <= 0 || straightShotPrefab == null) return;

            // Logic phân bổ đạn thẳng vào các điểm bắn
            if (upgrade.straightShots == 1)
            {
                SpawnProjectile(straightShotPrefab, firePointHead);
            }
            else if (upgrade.straightShots == 2)
            {
                SpawnProjectile(straightShotPrefab, firePointLeft);
                SpawnProjectile(straightShotPrefab, firePointRight);
            }
            else // 3 hoặc nhiều hơn
            {
                SpawnProjectile(straightShotPrefab, firePointHead);
                SpawnProjectile(straightShotPrefab, firePointLeft);
                SpawnProjectile(straightShotPrefab, firePointRight);
            }
        }

        private void HandleDiagonalShots(UpgradeData upgrade)
        {
            if (upgrade.diagonalShots <= 0 || diagonalShotPrefab == null) return;
        
            // 2 đạn chéo
            Quaternion leftRotation = firePointLeft.rotation * Quaternion.Euler(0, 0, -diagonalAngle);
            Quaternion rightRotation = firePointRight.rotation * Quaternion.Euler(0, 0, diagonalAngle);
        
            SpawnProjectile(diagonalShotPrefab, firePointLeft, leftRotation);
            SpawnProjectile(diagonalShotPrefab, firePointRight, rightRotation);
        }

        private void HandleHomingShots(UpgradeData upgrade)
        {
            if (upgrade.homingShots <= 0 || homingShotPrefab == null) return;

            // Bắn ra số lượng đạn homing tương ứng từ điểm giữa
            for (int i = 0; i < upgrade.homingShots; i++)
            {
                SpawnProjectile(homingShotPrefab, firePointHead);
            }
        }

        private void HandleCannonballShots(UpgradeData upgrade)
        {
            if (upgrade.cannonballShots <= 0 || cannonballPrefab == null) return;
        
            // Bắn ra số lượng đạn cannonball tương ứng từ điểm giữa
            for (int i = 0; i < upgrade.cannonballShots; i++)
            {
                SpawnProjectile(cannonballPrefab, firePointHead);
            }
        }
    
        /// <summary>
        /// Hàm tiện ích để lấy và kích hoạt một viên đạn từ Object Pooler.
        /// </summary>
        private void SpawnProjectile(GameObject prefab, Transform spawnPoint, Quaternion? rotation = null)
        {
            GameObject projectile = objectPooler.GetPooledObject(prefab);
            if (projectile != null)
            {
                projectile.transform.position = spawnPoint.position;
                // Nếu không cung cấp rotation, dùng rotation mặc định của điểm bắn
                projectile.transform.rotation = rotation ?? spawnPoint.rotation; 
                projectile.SetActive(true);
            }
        }

        /// <summary>
        /// Kích hoạt bom nếu người chơi còn bom.
        /// </summary>
        private void UseBomb()
        {
            if (playerState != null && playerState.CurrentBombs > 0 && bombPrefab != null)
            {
                playerState.UseBomb(); // Trừ một quả bom trong PlayerState
            
                // Tạo đối tượng bom trong scene (bom không cần pool vì dùng rất ít)
                Instantiate(bombPrefab, transform.position, Quaternion.identity);
            }
        }
    }
}