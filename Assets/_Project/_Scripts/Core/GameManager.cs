using _Project._Scripts.Bosses;
using Unity.Mathematics;
using UnityEngine;

namespace _Project._Scripts.Core
{
    public class GameManager : MonoBehaviour
    {
        [Header("Boss Configuration")] [Tooltip("Kéo Prefab của con trùm cho màn chơi này vào đây.")] [SerializeField]
        private GameObject bossPrefab;
    
        [Tooltip("Vị trí mà con trùm sẽ xuất hiện.")]
        [SerializeField] private Transform bossSpawnPoint;

        // Singleton Pattern để các script khác có thể truy cập dễ dàng
        public static GameManager Instance { get; private set; }

        void Awake()
        {
            // Thiết lập Singleton
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
            }
            else
            {
                Instance = this;
            }
        }

        void Start()
        {
            // Kiểm tra xem đã gán prefab và vị trí chưa
            if (bossPrefab != null && bossSpawnPoint != null)
            {
                // 1. Tạo ra boss
                GameObject bossObject = Instantiate(bossPrefab, bossSpawnPoint.position, Quaternion.identity);
            
                // 2. Lấy component BossController từ boss vừa tạo
                BossController bossController = bossObject.GetComponent<BossController>();

                // 3. Khởi động trận đấu một cách an toàn
                // 3. Khởi động trận đấu một cách an toàn
                if (bossController != null)
                {
                    bossController.Initialize();
                }
                else
                {
                    Debug.LogError("Boss Prefab không chứa script BossController!", bossPrefab);
                }
            }
            else
            {
                Debug.LogError("GameManager: Vui lòng gán Boss Prefab và Boss Spawn Point trong Inspector!");
            }
        }

        // Hàm này được gọi bởi PlayerState khi hết mạng
        public void GameOver()
        {
            Debug.Log("GAME OVER - Implement logic to show game over screen and restart level.");
            // Ví dụ: UnityEngine.SceneManagement.SceneManager.LoadScene(0);
        }
    }
}