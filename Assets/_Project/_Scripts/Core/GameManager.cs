// FILE: _Project/_Scripts/Core/GameManager.cs (PHIÊN BẢN HOÀN THIỆN)

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
        private PlayerState playerState; // Tham chiếu đến PlayerState để reset máu

        private bool isVictorySequenceRunning = false;
        private bool isPaused = false;

        private SceneData currentSceneData;
        private GameObject currentBossObject;
        private BossController currentBossController;
        private SceneSetup currentSceneSetup;
        private PlayerSkillManager playerSkillManager;

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
            Time.timeScale = 1f;
        }

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                TogglePause();
            }
        }

        #endregion

        #region Scene Loading & Initialization

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (UIManager.Instance == null)
            {
                Debug.LogWarning($"[GameManager.OnSceneLoaded] No UIManager found in scene '{scene.name}'. UI will not function until one is loaded. This is normal for scenes without UI.");
            }

            currentSceneData = GetSceneDataByBuildIndex(scene.buildIndex);
            if (currentSceneData == null)
            {
                Debug.LogError($"[GameManager.OnSceneLoaded] No SceneData found for scene '{scene.name}'.", this);
                GameOver();
                return;
            }

            currentSceneSetup = FindObjectOfType<SceneSetup>();
            if (currentSceneSetup == null)
            {
                Debug.LogError($"[GameManager.OnSceneLoaded] No SceneSetup GameObject found in scene '{scene.name}'.", this);
                GameOver();
                return;
            }

            if (UIManager.Instance != null)
            {
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
                Debug.LogError($"[GameManager.SetupPlayer] Player spawn points not assigned in SceneSetup for scene '{currentSceneData.sceneName}'.", this);
                return;
            }

            if (playerObject == null)
            {
                if (playerPrefab == null)
                {
                    Debug.LogError("[GameManager.SetupPlayer] Player Prefab is not assigned in GameManager.", this);
                    return;
                }
                playerObject = Instantiate(playerPrefab, currentSceneSetup.playerInitialSpawnPoint.position, Quaternion.identity);
                playerController = playerObject.GetComponent<PlayerController>();
                playerState = playerObject.GetComponent<PlayerState>();
                playerSkillManager = playerObject.GetComponent<PlayerSkillManager>();
                DontDestroyOnLoad(playerObject);
            }
            else
            {
                playerObject.transform.position = currentSceneSetup.playerInitialSpawnPoint.position;
                
                // Nếu Player đã tồn tại, chúng ta cần chủ động yêu cầu UIManager mới cập nhật lại.
                if (UIManager.Instance != null)
                {
                    UIManager.Instance.UpdateAllPlayerUI();
                }
            }

            if (playerController != null)
            {
                playerController.SetPlayerControl(false);
                playerObject.SetActive(true);
                // Reset vật lý
                Rigidbody2D playerRb = playerObject.GetComponent<Rigidbody2D>();
                if (playerRb != null)
                {
                    // Đảm bảo Rigidbody không còn là Kinematic nữa
                    playerRb.isKinematic = false; 
                    playerRb.linearVelocity = Vector2.zero;
                }
        
                Collider2D playerCollider = playerObject.GetComponent<Collider2D>();
                if (playerCollider != null) playerCollider.enabled = true;
            }
        }

        private IEnumerator StartLevelSequence()
        {
            if (currentBossObject != null)
            {
                Destroy(currentBossObject);
            }

            if (currentSceneData.bossPrefab != null && currentSceneSetup.bossInitialSpawnPoint != null)
            {
                currentBossObject = Instantiate(currentSceneData.bossPrefab, currentSceneSetup.bossInitialSpawnPoint.position, Quaternion.identity);
                currentBossController = currentBossObject.GetComponent<BossController>();
                if (currentBossController == null)
                {
                    Debug.LogError($"[GameManager.StartLevel] Boss Prefab '{currentSceneData.bossPrefab.name}' is missing BossController component.", currentSceneData.bossPrefab);
                    yield break;
                }
                
                if (UIManager.Instance != null) UIManager.Instance.ShowBossUI();
            }
            else
            {
                if (UIManager.Instance != null) UIManager.Instance.HideBossUI();
                Debug.LogWarning($"[GameManager.StartLevel] No boss configured for scene '{currentSceneData.sceneName}'.", this);
                yield break;
            }

            if (currentSceneSetup.playerSpawnPoint != null && currentSceneSetup.bossSpawnPoint != null)
            {
                yield return StartCoroutine(MoveToSpawnPoints(playerObject.transform, currentBossObject.transform));
            }
            else
            {
                Debug.LogError($"[GameManager.StartLevel] Player or Boss spawn points not assigned in SceneSetup.", this);
                yield break;
            }

            currentBossController.Initialize();

            if (AudioManager.Instance != null)
            {
                if (!string.IsNullOrEmpty(currentSceneData.musicTrackName))
                    AudioManager.Instance.PlayMusic(currentSceneData.musicTrackName);
                else
                    AudioManager.Instance.PlayMusic("01. Night of Knights");
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
            
            if (playerController != null)
            {
                playerController.SetPlayerControl(true);
            }

            if (elapsedTime >= maxMoveToSpawnTime)
            {
                Debug.LogWarning("[GameManager.MoveToSpawnPoints] Movement timed out.");
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
            
            if (playerController != null)
            {
                playerController.SetPlayerControl(false);
            }

            yield return new WaitForSeconds(1f);

            if (Camera.main != null)
            {
                yield return StartCoroutine(ShakeCamera());
            }

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
                if (UIManager.Instance != null)
                    yield return StartCoroutine(UIManager.Instance.ShowTransition());
                
                SceneManager.LoadScene(currentSceneData.nextSceneName);
            }
            else
            {
                Debug.Log("Final boss defeated. Showing Win Screen.");
                if (UIManager.Instance != null)
                {
                    UIManager.Instance.ShowWinScreen();
                }
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
             Debug.Log("GAME OVER signal received from Player. Showing Game Over screen.");
             if (playerObject != null)
             {
                 playerObject.SetActive(false); 
             }
     
             // Hiển thị màn hình Game Over trước
             if (UIManager.Instance != null)
             {
                 UIManager.Instance.ShowGameOverScreen();
             }

             Time.timeScale = 0f;
        }

        public void GameOver()
        {
            Debug.LogError("FATAL ERROR: Resetting game to first scene.");
            Time.timeScale = 1f;
            
            if (playerObject != null) Destroy(playerObject);
            if (ObjectPooler.Instance != null) Destroy(ObjectPooler.Instance.gameObject);
            
            Destroy(gameObject); 
            SceneManager.LoadScene(0);
        }

        public void RestartGameFromBeginning()
        {
            Debug.Log("Restarting game from the beginning.");
            Time.timeScale = 1f;
            
            if (playerObject != null)
            {
                Destroy(playerObject);
            }
            
            if (UIManager.Instance != null) UIManager.Instance.HideAllScreens();
            
            SceneManager.LoadScene(0);
        }
        
        public void RestartLevel()
        {
            Debug.Log($"Restarting current level: {SceneManager.GetActiveScene().name}");
            Time.timeScale = 1f;
            
            if(playerState != null)
            {
                playerState.RestoreFullHealth(); // Giả định có hàm này trong PlayerState
            }

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
            UIManager.Instance.ShowPauseMenu();
        }

        public void ResumeGame()
        {
            isPaused = false;
            Time.timeScale = 1f;
            UIManager.Instance.HidePauseMenu();
        }

        public void QuitGame()
        {
            Application.Quit();
        }

        #endregion
    }
}