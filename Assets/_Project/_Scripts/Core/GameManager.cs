using _Project._Scripts.Bosses;
using System.Collections;
using _Project._Scripts.Player;
using _Project._Scripts.UI;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace _Project._Scripts.Core
{
    public class GameManager : MonoBehaviour
    {
        [Header("Boss Configuration")]
        [SerializeField] private GameObject bossPrefab;
        [SerializeField] private Transform bossSpawnPoint;
        [SerializeField] private Transform bossInitialSpawnPoint;

        [Header("Player Configuration")]
        [SerializeField] private GameObject playerPrefab;
        [SerializeField] private Transform playerSpawnPoint;
        [SerializeField] private Transform playerInitialSpawnPoint;

        [Header("Movement Configuration")]
        [SerializeField] private float movementSpeed = 2f;

        [Header("Victory Sequence Configuration")]
        [SerializeField] private string nextSceneName;
        [SerializeField] private Transform playerExitPoint;
        [SerializeField] private float playerExitSpeed = 3f;

        [Header("Camera Shake Configuration")]
        [SerializeField] private float shakeDuration = 0.5f;
        [SerializeField] private float shakeMagnitude = 0.1f;

        private GameObject playerObject;
        private bool isVictorySequenceRunning = false;
        private bool isPaused = false;
        private PlayerController playerController;

        public static GameManager Instance { get; private set; }

        void Awake()
        {
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
            Time.timeScale = 1f;
            StartCoroutine(StartLevelSequence());
            if (AudioManager.Instance != null)
                AudioManager.Instance.PlayMusic("01. Night of Knights");
            
            playerController = FindObjectOfType<PlayerController>();
        }
        
        void Update()
        {
            // Nếu người chơi nhấn phím Escape
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                TogglePause();
            }
        }

        private IEnumerator StartLevelSequence()
        {
            if (playerPrefab != null && playerInitialSpawnPoint != null)
            {
                playerObject = Instantiate(playerPrefab, playerInitialSpawnPoint.position, Quaternion.identity);
            }
            else
            {
                Debug.LogError("GameManager: Player Prefab và Player Initial Spawn Point chưa được gán!");
                yield break;
            }

            GameObject bossObject = null;
            if (bossPrefab != null && bossInitialSpawnPoint != null)
            {
                bossObject = Instantiate(bossPrefab, bossInitialSpawnPoint.position, Quaternion.identity);
            }
            else
            {
                Debug.LogError("GameManager: Boss Prefab và Boss Initial Spawn Point chưa được gán!");
                yield break;
            }

            if (playerSpawnPoint != null && bossSpawnPoint != null)
            {
                yield return StartCoroutine(MoveToSpawnPoints(playerObject.transform, bossObject.transform));
            }
            else
            {
                Debug.LogError("GameManager: Player Spawn Point và Boss Spawn Point chưa được gán!");
                yield break;
            }

            BossController bossController = bossObject.GetComponent<BossController>();
            if (bossController != null)
            {
                bossController.Initialize();
            }
            else
            {
                Debug.LogError("Boss Prefab không chứa script BossController!", bossPrefab);
            }
        }

        private IEnumerator MoveToSpawnPoints(Transform playerTransform, Transform bossTransform)
        {
            float journeyLengthPlayer = Vector3.Distance(playerTransform.position, playerSpawnPoint.position);
            float journeyLengthBoss = Vector3.Distance(bossTransform.position, bossSpawnPoint.position);
            float startTime = Time.time;

            while (Vector3.Distance(playerTransform.position, playerSpawnPoint.position) > 0.01f ||
                   Vector3.Distance(bossTransform.position, bossSpawnPoint.position) > 0.01f)
            {
                float distCovered = (Time.time - startTime) * movementSpeed;

                if (Vector3.Distance(playerTransform.position, playerSpawnPoint.position) > 0.01f)
                {
                    float fractionOfJourney = distCovered / journeyLengthPlayer;
                    playerTransform.position = Vector3.Lerp(playerInitialSpawnPoint.position, playerSpawnPoint.position, fractionOfJourney);
                }

                if (Vector3.Distance(bossTransform.position, bossSpawnPoint.position) > 0.01f)
                {
                    float fractionOfJourney = distCovered / journeyLengthBoss;
                    bossTransform.position = Vector3.Lerp(bossInitialSpawnPoint.position, bossSpawnPoint.position, fractionOfJourney);
                }

                yield return null;
            }

            playerTransform.position = playerSpawnPoint.position;
            bossTransform.position = bossSpawnPoint.position;
        }

        public void OnBossDefeated()
        {
            Debug.Log("GameManager received BossDefeated signal.");
        
            // Ngăn sequence chạy lại nếu đang chạy rồi
            if (isVictorySequenceRunning) return;

            StartCoroutine(BossDefeatedSequence());
        }


        // Coroutine xử lý chuỗi sự kiện khi chiến thắng
        private IEnumerator BossDefeatedSequence()
        {
            isVictorySequenceRunning = true; // Đánh dấu là sequence đang chạy

            // 1. Chờ một chút để người chơi cảm nhận chiến thắng
            yield return new WaitForSeconds(1f);

            // 2. Rung màn hình (hiệu ứng)
            if (Camera.main != null)
            {
                yield return StartCoroutine(ShakeCamera());
            }

            // 3. Di chuyển người chơi ra khỏi màn hình
            // Giả sử bạn đã có coroutine MovePlayerToExit() từ lần trước
            yield return StartCoroutine(MovePlayerToExit());

            // 4. Tải màn chơi tiếp theo
            Debug.Log("Player has exited. Loading next scene...");
            if (!string.IsNullOrEmpty(nextSceneName))
            {
                SceneManager.LoadScene(nextSceneName);
            }
            else
            {
                Debug.LogWarning("GameManager: Tên của scene tiếp theo chưa được thiết lập!");
            }
        
            isVictorySequenceRunning = false; // Reset cờ nếu cần (mặc dù scene đã chuyển)
        }


        private IEnumerator ShakeCamera()
        {
            Vector3 originalPosition = Camera.main.transform.localPosition;
            float elapsedTime = 0f;

            while (elapsedTime < shakeDuration)
            {
                float x = Random.Range(-1f, 1f) * shakeMagnitude;
                float y = Random.Range(-1f, 1f) * shakeMagnitude;
                Camera.main.transform.localPosition = new Vector3(originalPosition.x + x, originalPosition.y + y, originalPosition.z);
                elapsedTime += Time.deltaTime;
                yield return null;
            }

            Camera.main.transform.localPosition = originalPosition;
        }

        private IEnumerator MovePlayerToExit()
        {
            if (playerObject == null || playerExitPoint == null)
            {
                Debug.LogError("GameManager: Player object hoặc Player Exit Point chưa được gán!");
                yield break; // Thoát khỏi coroutine nếu thiếu tham chiếu
            }
            
            // 1. Vô hiệu hóa script điều khiển của người chơi
            playerController.enabled = false;

            // 2. Vô hiệu hóa Collider để tránh va chạm
            Collider2D playerCollider = playerController.GetComponent<Collider2D>();
            if (playerCollider != null)
            {
                playerCollider.enabled = false;
            }
            
            // 3. Vô hiệu hóa Rigidbody nếu có (để không bị ảnh hưởng bởi trọng lực, v.v.)
            Rigidbody2D playerRb = playerController.GetComponent<Rigidbody2D>();
            if (playerRb != null)
            {
                playerRb.linearVelocity = Vector2.zero; // Dừng mọi chuyển động hiện tại
                playerRb.isKinematic = true; // Chuyển sang Kinematic để không bị ảnh hưởng bởi vật lý
            }

            Transform playerTransform = playerObject.transform;
            while (playerTransform != null && Vector3.Distance(playerTransform.position, playerExitPoint.position) > 0.01f)
            {
                playerTransform.position = Vector3.MoveTowards(playerTransform.position, playerExitPoint.position, playerExitSpeed * Time.deltaTime);
                yield return null; // Chờ đến frame tiếp theo
            }
            
            // Đảm bảo player đến đúng vị trí cuối cùng
            if (playerTransform != null)
            {
                 playerTransform.position = playerExitPoint.position;
            }
        }

        public void GameOver()
        {
            Debug.Log("GAME OVER");
        }
        
        #region Pause Menu Logic

        // --- MỚI: Các hàm xử lý logic Tạm dừng ---
        
        public void TogglePause()
        {
            isPaused = !isPaused;
            if (isPaused)
            {
                PauseGame();
            }
            else
            {
                ResumeGame();
            }
        }

        public void PauseGame()
        {
            isPaused = true;
            Time.timeScale = 0f; // Dừng thời gian trong game
            UIManager.Instance.ShowPauseMenu(); // Yêu cầu UIManager hiển thị menu
        }

        public void ResumeGame()
        {
            isPaused = false;
            Time.timeScale = 1f; // Cho thời gian chạy lại bình thường
            UIManager.Instance.HidePauseMenu(); // Yêu cầu UIManager ẩn menu
        }

        public void RestartLevel()
        {
            // Quan trọng: Phải đặt lại Time.timeScale trước khi tải lại scene
            Time.timeScale = 1f;
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        public void QuitGame()
        {
            Debug.Log("Quitting game..."); // Dòng này chỉ hiện trong Editor
            Application.Quit();
        }

        // --- KẾT THÚC PHẦN MỚI ---
        #endregion
    }
}