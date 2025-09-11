// FILE: _Project/_Scripts/Core/GameManager.cs (PHIÊN BẢN CUỐI CÙNG - FULL)

using _Project._Scripts.Bosses;
using System.Collections;
using System.Collections.Generic;
using _Project._Scripts.Gameplay.Items;
using _Project._Scripts.Player;
using _Project._Scripts.UI;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace _Project._Scripts.Core
{
    public class GameManager : MonoBehaviour
    {
        [Header("Scene Management")]
        [Tooltip("Danh sách dữ liệu cho từng Scene. Phải được gán theo đúng thứ tự Build Index.")]
        public List<SceneData> sceneDatas = new List<SceneData>();

        [Header("Global Player Configuration")]
        [SerializeField] private GameObject playerPrefab;

        [Header("Movement Configuration")]
        [SerializeField] private float movementSpeed = 2f;
        [Tooltip("Thời gian tối đa cho phép di chuyển Player/Boss đến điểm spawn để tránh vòng lặp vô hạn.")]
        [SerializeField] private float maxMoveToSpawnTime = 10f;

        [Header("Victory Sequence Configuration")]
        [SerializeField] private float playerExitSpeed = 3f;

        [Header("Camera Shake Configuration")]
        [SerializeField] private float shakeDuration = 0.5f;
        [SerializeField] private float shakeMagnitude = 0.1f;

        // --- Biến nội bộ ---
        private GameObject playerObject;
        private PlayerController playerController;
        private PlayerState playerState;
        private PlayerSkillManager playerSkillManager;

        private bool isGameStarted = false;
        private bool isVictorySequenceRunning = false;
        private bool isPaused = false;

        private SceneData currentSceneData;
        private GameObject currentBossObject;
        private BossController currentBossController;
        private SceneSetup currentSceneSetup;

        // --- Singleton Instance ---
        public static GameManager Instance { get; private set; }

        // --- Public Getters ---
        public GameObject PlayerObject => playerObject;

        #region Unity Lifecycle & Singleton Setup

        void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else if (Instance != this)
            {
                Destroy(gameObject);
                return;
            }
        }

        void OnEnable()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        void OnDisable()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        void Start()
        {
            // Đảm bảo Time Scale luôn được reset khi game khởi động
            Time.timeScale = 1f;
        }

        void Update()
        {
            // Chỉ cho phép pause nếu game đã bắt đầu và không trong cinematic chiến thắng
            if (isGameStarted && !isVictorySequenceRunning && Input.GetKeyDown(KeyCode.Escape))
            {
                TogglePause();
            }
        }

        #endregion

        #region Scene Loading & Initialization

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            // Dừng tất cả các coroutine đang chạy từ scene trước để tránh xung đột.
            StopAllCoroutines();
            // Việc khởi tạo sẽ được kích hoạt bởi UIManager thông qua NotifyUIManagerReady.
        }
        
        /// <summary>
        /// Được UIManager gọi khi nó đã sẵn sàng. Đây là điểm khởi đầu logic cho mỗi scene.
        /// </summary>
        public void NotifyUIManagerReady()
        {
            if (!isGameStarted)
            {
                // Nếu game chưa bắt đầu, đây là lần đầu vào game -> hiển thị Main Menu.
                if (UIManager.Instance != null)
                {
                    UIManager.Instance.ShowMainMenu();
                }
            }
            else
            {
                // Nếu game đã bắt đầu (đang chuyển màn), tiến hành khởi tạo màn chơi mới.
                InitializeLevel();
            }
        }
        
        /// <summary>
        /// Bắt đầu game, được gọi từ nút "Bắt Đầu" trên UI.
        /// </summary>
        public void StartGame()
        {
            if (isGameStarted) return;
            
            isGameStarted = true;
            Debug.Log("[GameManager] Bắt đầu game. Khởi tạo màn chơi đầu tiên.");
            
            if (UIManager.Instance != null)
            {
                UIManager.Instance.HidePauseMenu(); // Ẩn main menu
            }
            
            // Khởi tạo màn chơi hiện tại (màn đầu tiên).
            InitializeLevel();
        }
        
        /// <summary>
        /// Khởi tạo tài nguyên và bắt đầu chuỗi sự kiện cho một màn chơi.
        /// </summary>
        private void InitializeLevel()
        {
            currentSceneData = GetSceneDataByBuildIndex(SceneManager.GetActiveScene().buildIndex);
            if (currentSceneData == null) { Debug.LogError("Không tìm thấy SceneData cho màn chơi hiện tại."); GameOver(); return; }

            currentSceneSetup = FindObjectOfType<SceneSetup>();
            if (currentSceneSetup == null) { Debug.LogError("Không tìm thấy SceneSetup trong màn chơi hiện tại."); GameOver(); return; }

            if (UIManager.Instance != null)
            {
                UIManager.Instance.ShowGameplayUI();
                StartCoroutine(UIManager.Instance.HideTransition());
            }

            SetupPlayerForNewScene();
            StopAllCoroutines();
            StartCoroutine(StartLevelSequence());
        }

        private void SetupPlayerForNewScene()
        {
            if (currentSceneSetup.playerInitialSpawnPoint == null || currentSceneSetup.playerSpawnPoint == null)
            {
                Debug.LogError($"Player spawn points chưa được gán trong SceneSetup.");
                return;
            }

            if (playerObject == null)
            {
                if (playerPrefab == null) { Debug.LogError("Player Prefab chưa được gán trong GameManager."); return; }
                playerObject = Instantiate(playerPrefab, currentSceneSetup.playerInitialSpawnPoint.position, Quaternion.identity);
                playerController = playerObject.GetComponent<PlayerController>();
                playerState = playerObject.GetComponent<PlayerState>();
                playerSkillManager = playerObject.GetComponent<PlayerSkillManager>();
                DontDestroyOnLoad(playerObject);
            }
            else
            {
                playerObject.transform.position = currentSceneSetup.playerInitialSpawnPoint.position;
            }

            // Kích hoạt lại Player và reset trạng thái vật lý
            playerObject.SetActive(true);
            playerController?.SetPlayerControl(false);
            
            Rigidbody2D playerRb = playerObject.GetComponent<Rigidbody2D>();
            if (playerRb != null) { playerRb.isKinematic = false; playerRb.linearVelocity = Vector2.zero; }
            Collider2D playerCollider = playerObject.GetComponent<Collider2D>();
            if (playerCollider != null) playerCollider.enabled = true;

            // Yêu cầu UIManager của scene mới cập nhật toàn bộ thông tin của Player
            UIManager.Instance?.UpdateAllPlayerUI();
        }

        private IEnumerator StartLevelSequence()
        {
            if (currentBossObject != null)
            {
                Destroy(currentBossObject);
            }

            if (currentSceneData.bossPrefab != null && currentSceneSetup.bossInitialSpawnPoint != null && currentSceneSetup.bossSpawnPoint != null)
            {
                currentBossObject = Instantiate(currentSceneData.bossPrefab, currentSceneSetup.bossInitialSpawnPoint.position, Quaternion.identity);
                currentBossController = currentBossObject.GetComponent<BossController>();
                if (currentBossController == null) yield break;
                
                UIManager.Instance?.ShowBossUI();
                
                // Di chuyển Player và Boss vào vị trí chiến đấu
                yield return StartCoroutine(MoveToSpawnPoints(playerObject.transform, currentBossObject.transform));
            }
            else
            {
                Debug.LogWarning($"Không có boss hoặc spawn point cho màn chơi '{currentSceneData.sceneName}'.");
                playerController?.SetPlayerControl(true); // Cho phép người chơi di chuyển nếu không có boss
                UIManager.Instance?.HideBossUI();
                yield break;
            }

            // Kích hoạt boss và bật nhạc nền
            currentBossController.Initialize();
            if (AudioManager.Instance != null)
            {
                if (!string.IsNullOrEmpty(currentSceneData.musicTrackName))
                {
                    AudioManager.Instance.PlayMusic(currentSceneData.musicTrackName);
                }
                else
                {
                    // Nếu không có nhạc cụ thể cho scene, có thể dừng nhạc hoặc phát một bài mặc định
                    Debug.LogWarning($"[GameManager] No music track specified in SceneData for '{currentSceneData.sceneName}'.");
                    AudioManager.Instance.StopMusic(); // Ví dụ: dừng nhạc nếu không có bài nào được chọn
                }
            }
        }

        private IEnumerator MoveToSpawnPoints(Transform playerTransform, Transform bossTransform)
        {
            float journeyLengthPlayer = Vector3.Distance(playerTransform.position, currentSceneSetup.playerSpawnPoint.position);
            float journeyLengthBoss = Vector3.Distance(bossTransform.position, currentSceneSetup.bossSpawnPoint.position);
            float startTime = Time.time;
            float elapsedTime = 0f;

            while ((Vector3.Distance(playerTransform.position, currentSceneSetup.playerSpawnPoint.position) > 0.01f ||
                   Vector3.Distance(bossTransform.position, currentSceneSetup.bossSpawnPoint.position) > 0.01f) && elapsedTime < maxMoveToSpawnTime)
            {
                float distCovered = (Time.time - startTime) * movementSpeed;

                if (journeyLengthPlayer > 0.001f)
                    playerTransform.position = Vector3.Lerp(currentSceneSetup.playerInitialSpawnPoint.position, currentSceneSetup.playerSpawnPoint.position, distCovered / journeyLengthPlayer);

                if (journeyLengthBoss > 0.001f)
                    bossTransform.position = Vector3.Lerp(currentSceneSetup.bossInitialSpawnPoint.position, currentSceneSetup.bossSpawnPoint.position, distCovered / journeyLengthBoss);

                elapsedTime += Time.deltaTime;
                yield return null;
            }

            playerTransform.position = currentSceneSetup.playerSpawnPoint.position;
            bossTransform.position = currentSceneSetup.bossSpawnPoint.position;
            
            playerController?.SetPlayerControl(true);

            if (elapsedTime >= maxMoveToSpawnTime)
            {
                Debug.LogWarning("Di chuyển đến điểm spawn bị quá thời gian.");
            }
        }

        #endregion

        #region Victory, Defeat & Restart Logic

        public void OnBossDefeated()
        {
            if (isVictorySequenceRunning) return;
            StartCoroutine(BossDefeatedSequence());
        }

        private IEnumerator BossDefeatedSequence()
        {
            isVictorySequenceRunning = true;
            playerController?.SetPlayerControl(false);

            yield return new WaitForSeconds(1f);

            if (Camera.main != null) yield return StartCoroutine(ShakeCamera());

            if (currentBossObject != null)
            {
                Vector3 bossPosition = currentBossObject.transform.position;
                BossData defeatedBossData = currentBossController.bossData;
                
                if (defeatedBossData != null && defeatedBossData.defeatExplosionVFX != null)
                {
                    Instantiate(defeatedBossData.defeatExplosionVFX, bossPosition, Quaternion.identity);
                    DestroyAfterAnimation vfxDestroyScript = defeatedBossData.defeatExplosionVFX.GetComponent<DestroyAfterAnimation>();
                    float explosionDuration = vfxDestroyScript != null ? vfxDestroyScript.lifetime : 1f;
                    
                    if (currentBossObject.GetComponent<SpriteRenderer>() != null)
                        currentBossObject.GetComponent<SpriteRenderer>().enabled = false;
                    
                    yield return new WaitForSeconds(explosionDuration);
                }
                
                Destroy(currentBossObject);
            }

            yield return StartCoroutine(WaitForLootCollection());
            yield return StartCoroutine(MovePlayerToExit());

            if (!string.IsNullOrEmpty(currentSceneData.nextSceneName))
            {
                if (UIManager.Instance != null) yield return StartCoroutine(UIManager.Instance.ShowTransition());
                SceneManager.LoadScene(currentSceneData.nextSceneName);
            }
            else
            {
                Debug.Log("Đã hạ boss cuối. Hiển thị màn hình chiến thắng.");
                UIManager.Instance?.ShowWinScreen();
            }

            isVictorySequenceRunning = false;
        }

        private IEnumerator MovePlayerToExit()
        {
            if (playerObject == null || currentSceneSetup.playerExitPoint == null) yield break;

            if (playerController != null)
            {
                if (playerController.GetComponent<Collider2D>() != null) playerController.GetComponent<Collider2D>().enabled = false;
                if (playerController.GetComponent<Rigidbody2D>() != null) playerController.GetComponent<Rigidbody2D>().isKinematic = true;
            }
            
            Transform playerTransform = playerObject.transform;
            float exitMoveTime = 0f;
            while (playerTransform != null && Vector3.Distance(playerTransform.position, currentSceneSetup.playerExitPoint.position) > 0.01f && exitMoveTime < 15f)
            {
                playerTransform.position = Vector3.MoveTowards(playerTransform.position, currentSceneSetup.playerExitPoint.position, playerExitSpeed * Time.deltaTime);
                exitMoveTime += Time.deltaTime;
                yield return null;
            }
        }
        
        public void PlayerDied()
        {
             Debug.Log("Player đã chết. Hiển thị màn hình Game Over.");
             if (playerObject != null)
             {
                 playerObject.SetActive(false); 
             }
     
             UIManager.Instance?.ShowGameOverScreen();
             Time.timeScale = 0f;
        }

        public void GameOver()
        {
            Debug.LogError("Lỗi nghiêm trọng! Đang reset game về màn hình đầu tiên.");
            Time.timeScale = 1f;
            
            if (playerObject != null) Destroy(playerObject);
            if (ObjectPooler.Instance != null) Destroy(ObjectPooler.Instance.gameObject);
            
            Destroy(gameObject); 
            SceneManager.LoadScene(0);
        }

        public void RestartGameFromBeginning()
        {
            Debug.Log("Chơi lại từ đầu.");
            Time.timeScale = 1f;
            isGameStarted = false;
            
            if (playerObject != null)
            {
                Destroy(playerObject);
                playerObject = null;
            }
            
            // Dừng nhạc khi quay về Main Menu
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.StopMusic();
            }
            
            SceneManager.LoadScene(0);
        }
        
        public void RestartLevel()
        {
            Debug.Log($"Chơi lại màn: {SceneManager.GetActiveScene().name}");
            Time.timeScale = 1f;
            
            playerState?.RestoreFullHealth();

            // Tải lại scene sẽ tự động kích hoạt luồng OnSceneLoaded -> NotifyUIManagerReady -> InitializeLevel
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        #endregion

        #region Utility & Helpers

        private IEnumerator WaitForLootCollection()
        {
            while (FindGuaranteedLoot() != null)
            {
                yield return new WaitForSeconds(0.5f);
            }
        }

        private Item FindGuaranteedLoot()
        {
            Item[] allItems = FindObjectsOfType<Item>();
            foreach (Item item in allItems)
            {
                if (item != null && item.isGuaranteedLoot)
                {
                    return item;
                }
            }
            return null;
        }

        private IEnumerator ShakeCamera()
        {
            if (Camera.main == null) yield break;
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

        private SceneData GetSceneDataByBuildIndex(int buildIndex)
        {
            if (sceneDatas == null || sceneDatas.Count == 0) return null;
            foreach (var data in sceneDatas)
            {
                if (data != null && data.sceneBuildIndex == buildIndex) return data;
            }
            return null;
        }

        #endregion

        #region Pause Menu Logic

        public void TogglePause()
        {
            isPaused = !isPaused;
            if (isPaused) PauseGame();
            else ResumeGame();
        }

        public void PauseGame()
        {
            isPaused = true;
            Time.timeScale = 0f;
            
            if (AudioManager.Instance != null) AudioManager.Instance.PauseMusic();
            
            if (UIManager.Instance != null) UIManager.Instance.ShowPauseMenu();
        }

        public void ResumeGame()
        {
            isPaused = false;
            Time.timeScale = 1f;
            
            if (AudioManager.Instance != null) AudioManager.Instance.ResumeMusic();
            
            if (UIManager.Instance != null) UIManager.Instance.HidePauseMenu();
        }

        public void QuitGame()
        {
            Debug.Log("Thoát game...");
            Application.Quit();
        }

        #endregion
    }
}