// FILE: _Project/_Scripts/Core/GameManager.cs (PHIÊN BẢN CUỐI CÙNG - CÓ THÊM TIMEOUT)

using _Project._Scripts.Bosses;
using System.Collections;
using System.Collections.Generic;
using _Project._Scripts.Gameplay.Items;
using _Project._Scripts.Player;
using _Project._Scripts.UI;
using UnityEngine;
using UnityEngine.SceneManagement; // Giữ nguyên, không dùng UnityEditor.SceneManagement

namespace _Project._Scripts.Core
{
    public class GameManager : MonoBehaviour
    {
        [Header("Scene Management")]
        [Tooltip("Danh sách dữ liệu cho từng Scene. Thứ tự trong danh sách tương ứng với Build Index của Scene.")]
        public List<SceneData> sceneDatas = new List<SceneData>();

        [Header("Global Player Configuration")]
        [SerializeField] private GameObject playerPrefab;

        [Header("Movement Configuration")]
        [SerializeField] private float movementSpeed = 2f;
        [Tooltip("Thời gian tối đa cho phép di chuyển Player/Boss đến điểm spawn. Ngăn chặn vòng lặp vô hạn.")]
        [SerializeField] private float maxMoveToSpawnTime = 10f; // THÊM BIẾN MỚI NÀY

        [Header("Victory Sequence Configuration")]
        [SerializeField] private float playerExitSpeed = 3f;

        [Header("Camera Shake Configuration")]
        [SerializeField] private float shakeDuration = 0.5f;
        [SerializeField] private float shakeMagnitude = 0.1f;

        private GameObject playerObject;
        private PlayerController playerController;

        private bool isVictorySequenceRunning = false;
        private bool isPaused = false;

        private SceneData currentSceneData;
        private GameObject currentBossObject;
        private BossController currentBossController;

        private SceneSetup currentSceneSetup; // Tham chiếu đến SceneSetup của scene hiện tại

        public static GameManager Instance { get; private set; }

        public GameObject PlayerObject => playerObject; // Public Getter cho Player GameObject

        void Awake()
        {
            Debug.Log($"[GameManager.Awake] Start. Current Instance: {(Instance != null ? Instance.name : "None")}, This: {gameObject.name}");
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                Debug.Log($"[GameManager.Awake] Set Instance to {gameObject.name}. DontDestroyOnLoad applied.");
            }
            else if (Instance != this)
            {
                Debug.LogWarning($"[GameManager.Awake] Duplicate GameManager found (Existing: {Instance.name}, This: {gameObject.name}). Destroying this duplicate.");
                Destroy(gameObject);
                return;
            }
            Debug.Log($"[GameManager.Awake] End for {gameObject.name}.");
        }

        void OnEnable()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
            Debug.Log($"[GameManager.OnEnable] SceneManager.sceneLoaded subscribed by {gameObject.name}.");
        }

        void OnDisable()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            Debug.Log($"[GameManager.OnDisable] SceneManager.sceneLoaded unsubscribed by {gameObject.name}.");
        }

        void Start()
        {
            Time.timeScale = 1f;
        }

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                TogglePause();
            }
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            Debug.Log($"<color=cyan>[GameManager.OnSceneLoaded] Scene {scene.name} (Build Index {scene.buildIndex}) loaded. Starting initialization.</color>");

            currentSceneData = GetSceneDataByBuildIndex(scene.buildIndex);
            if (currentSceneData == null)
            {
                Debug.LogError($"[GameManager.OnSceneLoaded] No SceneData found for scene {scene.name} (Build Index {scene.buildIndex})! Calling GameOver.", this);
                GameOver();
                return;
            }

            currentSceneSetup = FindObjectOfType<SceneSetup>();
            if (currentSceneSetup == null)
            {
                Debug.LogError($"[GameManager.OnSceneLoaded] No SceneSetup GameObject found in scene {scene.name}! Please add a SceneSetup GameObject and configure its spawn points. Calling GameOver.", this);
                GameOver();
                return;
            }
            Debug.Log($"[GameManager.OnSceneLoaded] Found SceneSetup in {scene.name}.");

            Debug.Log("[GameManager.OnSceneLoaded] Calling SetupPlayerForNewScene().");
            SetupPlayerForNewScene();
            Debug.Log("[GameManager.OnSceneLoaded] SetupPlayerForNewScene() finished. Stopping all previous coroutines.");
            StopAllCoroutines();
            Debug.Log("[GameManager.OnSceneLoaded] Starting StartLevelSequence() coroutine.");
            StartCoroutine(StartLevelSequence());
        }

        private void SetupPlayerForNewScene()
        {
            if (currentSceneSetup.playerInitialSpawnPoint == null || currentSceneSetup.playerSpawnPoint == null)
            {
                Debug.LogError($"GameManager: Player Initial/Spawn Points for scene {currentSceneData.sceneName} are not assigned in its SceneSetup GameObject! Ensure Transforms are dragged into the SceneSetup component in Hierarchy.", this);
                return;
            }

            if (playerObject == null)
            {
                if (playerPrefab == null)
                {
                    Debug.LogError("GameManager: Player Prefab is not assigned in GameManager's Global Player Configuration! Please assign the Player Prefab.", this);
                    return;
                }
                playerObject = Instantiate(playerPrefab, currentSceneSetup.playerInitialSpawnPoint.position, Quaternion.identity);
                playerController = playerObject.GetComponent<PlayerController>();
                if (playerController == null)
                {
                    Debug.LogError("GameManager: Player Prefab does not have PlayerController component! Please add it to your Player Prefab.", playerPrefab);
                }
                DontDestroyOnLoad(playerObject);
                Debug.Log("GameManager: Instantiated new Player object and set as DontDestroyOnLoad.");
            }
            else
            {
                playerObject.transform.position = currentSceneSetup.playerInitialSpawnPoint.position;
                Debug.Log("GameManager: Player object already exists. Resetting position to initial spawn point.");
            }

            if (playerController != null)
            {
                playerController.enabled = true;
                Rigidbody2D playerRb = playerController.GetComponent<Rigidbody2D>();
                if (playerRb != null)
                {
                    playerRb.isKinematic = false;
                    playerRb.linearVelocity = Vector2.zero;
                    playerRb.angularVelocity = 0;
                }
                Collider2D playerCollider = playerController.GetComponent<Collider2D>();
                if (playerCollider != null)
                {
                    playerCollider.enabled = true;
                }
            }
        }

        private IEnumerator StartLevelSequence()
        {
            Debug.Log($"[GameManager.StartLevelSequence] Starting for scene {SceneManager.GetActiveScene().name}");

            if (currentBossObject != null)
            {
                Destroy(currentBossObject);
                currentBossObject = null;
                currentBossController = null;
                Debug.Log("[GameManager.StartLevelSequence] Destroyed previous Boss object.");
            }

            if (currentSceneData.bossPrefab != null && currentSceneSetup.bossInitialSpawnPoint != null)
            {
                currentBossObject = Instantiate(currentSceneData.bossPrefab, currentSceneSetup.bossInitialSpawnPoint.position, Quaternion.identity);
                currentBossController = currentBossObject.GetComponent<BossController>();
                if (currentBossController == null)
                {
                    Debug.LogError($"[GameManager.StartLevelSequence] Boss Prefab ({currentSceneData.bossPrefab.name}) does not have a BossController component! Please add it to your boss prefab.", currentSceneData.bossPrefab);
                    yield break;
                }
                Debug.Log($"[GameManager.StartLevelSequence] Instantiated new Boss: {currentBossObject.name}");
            }
            else
            {
                Debug.LogWarning($"[GameManager.StartLevelSequence] Boss Prefab (from SceneData) and/or Boss Initial Spawn Point (from SceneSetup) are not assigned for scene {currentSceneData.sceneName}. Assuming no boss for this scene, or configuration error. If this is not an end scene, check SceneData and SceneSetup.", this);
                yield break;
            }

            if (currentSceneSetup.playerSpawnPoint != null && currentSceneSetup.bossSpawnPoint != null)
            {
                yield return StartCoroutine(MoveToSpawnPoints(playerObject.transform, currentBossObject.transform));
            }
            else
            {
                Debug.LogError($"[GameManager.StartLevelSequence] Player Spawn Point and/or Boss Spawn Point (from SceneSetup) are not assigned for scene {currentSceneData.sceneName}! Please assign Transforms in the SceneSetup component.", this);
                yield break;
            }

            currentBossController.Initialize();
            Debug.Log("[GameManager.StartLevelSequence] BossController Initialized.");

            if (AudioManager.Instance != null)
            {
                if (!string.IsNullOrEmpty(currentSceneData.musicTrackName))
                {
                    AudioManager.Instance.PlayMusic(currentSceneData.musicTrackName);
                    Debug.Log($"[GameManager.StartLevelSequence] Playing music track: {currentSceneData.musicTrackName}");
                }
                else
                {
                    AudioManager.Instance.PlayMusic("01. Night of Knights");
                    Debug.Log("[GameManager.StartLevelSequence] No specific music track for scene, playing default.");
                }
            }
        }

        private IEnumerator MoveToSpawnPoints(Transform playerTransform, Transform bossTransform)
        {
            Debug.Log("[GameManager.MoveToSpawnPoints] Starting player and boss movement to spawn points.");
            float journeyLengthPlayer = Vector3.Distance(playerTransform.position, currentSceneSetup.playerSpawnPoint.position);
            float journeyLengthBoss = Vector3.Distance(bossTransform.position, currentSceneSetup.bossSpawnPoint.position);
            float startTime = Time.time;

            float elapsedTime = 0f; // THÊM BIẾN NÀY ĐỂ ĐẾM THỜI GIAN

            while ((Vector3.Distance(playerTransform.position, currentSceneSetup.playerSpawnPoint.position) > 0.01f ||
                   Vector3.Distance(bossTransform.position, currentSceneSetup.bossSpawnPoint.position) > 0.01f) && elapsedTime < maxMoveToSpawnTime) // THÊM ĐIỀU KIỆN TIMEOUT
            {
                float distCovered = (Time.time - startTime) * movementSpeed;

                if (Vector3.Distance(playerTransform.position, currentSceneSetup.playerSpawnPoint.position) > 0.01f)
                {
                    float fractionOfJourney = distCovered / journeyLengthPlayer;
                    playerTransform.position = Vector3.Lerp(currentSceneSetup.playerInitialSpawnPoint.position, currentSceneSetup.playerSpawnPoint.position, fractionOfJourney);
                }

                if (Vector3.Distance(bossTransform.position, currentSceneSetup.bossSpawnPoint.position) > 0.01f)
                {
                    float fractionOfJourney = distCovered / journeyLengthBoss;
                    bossTransform.position = Vector3.Lerp(currentSceneSetup.bossInitialSpawnPoint.position, currentSceneSetup.bossSpawnPoint.position, fractionOfJourney);
                }

                elapsedTime += Time.deltaTime; // CẬP NHẬT THỜI GIAN
                yield return null;
            }

            // Đảm bảo vị trí cuối cùng được đặt chính xác sau vòng lặp
            playerTransform.position = currentSceneSetup.playerSpawnPoint.position;
            bossTransform.position = currentSceneSetup.bossSpawnPoint.position;
            Debug.Log("[GameManager.MoveToSpawnPoints] Player and Boss moved to final spawn points (or timed out).");

            if (elapsedTime >= maxMoveToSpawnTime)
            {
                Debug.LogWarning("[GameManager.MoveToSpawnPoints] Movement to spawn points timed out! Player/Boss might not be at exact positions.");
            }
        }

        public void OnBossDefeated()
        {
            Debug.Log("GameManager received BossDefeated signal. Starting cinematic victory sequence.");

            if (isVictorySequenceRunning) return;

            // Bắt đầu coroutine mới
            StartCoroutine(BossDefeatedSequence());
        }


        private IEnumerator BossDefeatedSequence()
        {
            isVictorySequenceRunning = true;

            // 1. Chờ một chút để người chơi cảm nhận chiến thắng
            yield return new WaitForSeconds(1f);

            // 2. Rung màn hình
            if (Camera.main != null)
            {
                yield return StartCoroutine(ShakeCamera());
            }
            
            
            // 3. Kích hoạt hiệu ứng nổ tung của Boss và ẩn Boss
            if (currentBossObject != null)
            {
                // Lấy vị trí của boss trước khi ẩn/hủy nó
                Vector3 bossPosition = currentBossObject.transform.position;

                // Lấy BossData từ BossController để tìm prefab hiệu ứng nổ
                BossData defeatedBossData = currentBossController.bossData;
                
                if (defeatedBossData != null && defeatedBossData.defeatExplosionVFX != null)
                {
                    Debug.Log($"[GameManager.BossDefeated] Spawning defeat VFX: {defeatedBossData.defeatExplosionVFX.name}");
                    // Tạo hiệu ứng nổ tại vị trí của boss
                    Instantiate(defeatedBossData.defeatExplosionVFX, bossPosition, Quaternion.identity);

                    // Lấy thời gian tồn tại của hiệu ứng nổ từ script DestroyAfterAnimation
                    DestroyAfterAnimation vfxDestroyScript = defeatedBossData.defeatExplosionVFX.GetComponent<DestroyAfterAnimation>();
                    float explosionDuration = 1f; // Mặc định là 1 giây
                    if (vfxDestroyScript != null)
                    {
                        explosionDuration = vfxDestroyScript.lifetime;
                    }
                    
                    // Ẩn Sprite Renderer của boss ngay lập tức để nó "biến mất" trong vụ nổ
                    SpriteRenderer bossSprite = currentBossObject.GetComponent<SpriteRenderer>();
                    if (bossSprite != null)
                    {
                        bossSprite.enabled = false;
                    }
                    
                    // Chờ cho đến khi hiệu ứng nổ kết thúc
                    yield return new WaitForSeconds(explosionDuration);
                }
                
                // 4. Hủy GameObject của boss sau khi hiệu ứng kết thúc
                Debug.Log("[GameManager.BossDefeated] Destroying defeated boss object.");
                Destroy(currentBossObject);
                currentBossObject = null;
                currentBossController = null;
            }
            else
            {
                Debug.LogWarning("[GameManager.BossDefeated] currentBossObject is null. Cannot perform defeat cinematic.", this);
            }


            // 5. Chờ cho đến khi người chơi nhặt hết vật phẩm quan trọng
            Debug.Log("Waiting for player to collect guaranteed loot...");
            yield return StartCoroutine(WaitForLootCollection());

            // 6. Di chuyển người chơi ra khỏi màn hình
            yield return StartCoroutine(MovePlayerToExit());

            // 7. Tải màn chơi tiếp theo
            Debug.Log("Player has exited. Loading next scene...");
            if (!string.IsNullOrEmpty(currentSceneData.nextSceneName))
            {
                SceneManager.LoadScene(currentSceneData.nextSceneName);
            }
            else
            {
                Debug.LogWarning("GameManager: Next scene name is not set in SceneData for the current scene! Transitioning to Game Over.", this);
                GameOver();
            }

            isVictorySequenceRunning = false;
        }

        private IEnumerator WaitForLootCollection()
        {
            while (FindGuaranteedLoot() != null)
            {
                yield return new WaitForSeconds(0.5f);
            }
            Debug.Log("All guaranteed loot collected!");
        }

        private Item FindGuaranteedLoot()
        {
            Item[] allItems = FindObjectsOfType<Item>();
            foreach (Item item in allItems)
            {
                if (item != null && item.isGuaranteedLoot) // KIỂM TRA NULL CHO ITEM
                {
                    return item;
                }
            }
            return null;
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
            if (playerObject == null || currentSceneSetup.playerExitPoint == null)
            {
                Debug.LogError("GameManager: Player object hoặc Player Exit Point (từ SceneSetup) chưa được gán! Đảm bảo Player object và playerExitPoint trong SceneSetup hợp lệ.", this);
                yield break;
            }

            if (playerController != null)
            {
                playerController.enabled = false;
                Collider2D playerCollider = playerController.GetComponent<Collider2D>();
                if (playerCollider != null) playerCollider.enabled = false;
                Rigidbody2D playerRb = playerController.GetComponent<Rigidbody2D>();
                if (playerRb != null)
                {
                    playerRb.linearVelocity = Vector2.zero;
                    playerRb.isKinematic = true;
                }
            }


            Transform playerTransform = playerObject.transform;
            float exitMoveTime = 0f; // Thêm timeout cho MovePlayerToExit
            while (playerTransform != null && Vector3.Distance(playerTransform.position, currentSceneSetup.playerExitPoint.position) > 0.01f && exitMoveTime < maxMoveToSpawnTime + 5f) // Thêm timeout
            {
                playerTransform.position = Vector3.MoveTowards(playerTransform.position, currentSceneSetup.playerExitPoint.position, playerExitSpeed * Time.deltaTime);
                exitMoveTime += Time.deltaTime;
                yield return null;
            }

            if (playerTransform != null)
            {
                playerTransform.position = currentSceneSetup.playerExitPoint.position;
            }
            Debug.Log("GameManager: Player moved to exit point (or timed out).");
            if (exitMoveTime >= maxMoveToSpawnTime + 5f)
            {
                 Debug.LogWarning("[GameManager.MovePlayerToExit] Player exit movement timed out!");
            }
        }

        public void GameOver()
        {
            Debug.Log("GAME OVER. Resetting game to first scene.");
            Time.timeScale = 1f;
            SceneManager.LoadScene(0);
        }

        #region Pause Menu Logic

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
            Time.timeScale = 0f;
            UIManager.Instance.ShowPauseMenu();
        }

        public void ResumeGame()
        {
            isPaused = false;
            Time.timeScale = 1f;
            UIManager.Instance.HidePauseMenu();
        }

        public void RestartLevel()
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        public void QuitGame()
        {
            Debug.Log("Quitting game...");
            Application.Quit();
        }
        #endregion

        private SceneData GetSceneDataByBuildIndex(int buildIndex)
        {
            if (sceneDatas == null || sceneDatas.Count == 0)
            {
                Debug.LogError("GameManager: sceneDatas list is empty! Cannot retrieve SceneData. Please add SceneData assets to the GameManager in the initial scene.", this);
                return null;
            }

            foreach (var data in sceneDatas)
            {
                if (data != null && data.sceneBuildIndex == buildIndex)
                {
                    return data;
                }
            }
            Debug.LogError($"SceneData for Build Index {buildIndex} not found in GameManager.sceneDatas list. Make sure all SceneData assets are added and ordered correctly, and that the Build Index matches.", this);
            return null;
        }
    }
}