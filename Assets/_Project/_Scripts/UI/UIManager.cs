// FILE: _Project/_Scripts/UI/UIManager.cs (PHIÊN BẢN SỬA LỖI VÀ HOÀN THIỆN)

using UnityEngine;
using TMPro;
using System.Collections;
using _Project._Scripts.Bosses;
using _Project._Scripts.Core;
using _Project._Scripts.Player;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using ProgressBar = ThirdParty.InfinityPBR___Magic_Pig_Games.Progress_Bar.Scripts.ProgressBar;

namespace _Project._Scripts.UI
{
    public class UIManager : MonoBehaviour
    {
        public static UIManager Instance { get; private set; }

        [Header("📊 Giao diện Người chơi")]
        [SerializeField] private ProgressBar playerHealthBar;
        [SerializeField] private TextMeshProUGUI bombsText;
        [SerializeField] private TextMeshProUGUI powerText;
        [SerializeField] private TextMeshProUGUI scoreText;
        
        [Space(15)]
        [Header("✨ Giao diện Kỹ năng Player")]
        [SerializeField] private Image bulletClear_CooldownImage;
        [SerializeField] private TextMeshProUGUI bulletClear_CooldownText;
        [SerializeField] private Image invincibility_CooldownImage;
        [SerializeField] private TextMeshProUGUI invincibility_CooldownText;

        [Space(15)]
        [Header("👹 Giao diện Boss")]
        [SerializeField] private GameObject bossUIGroup; // GameObject cha chứa thanh máu boss
        [SerializeField] private ProgressBar bossHealthBar;
        [SerializeField] private GameObject spellCardDeclarationGroup;
        [SerializeField] private TextMeshProUGUI spellCardDeclarationNameText; // Đổi tên để rõ ràng hơn
        [SerializeField] private Animator spellCardAnimator;
        [SerializeField] private float spellCardDisplayTime = 3.5f;
        
        [Space(15)]
        [Header("Spell Card UI")]
        [SerializeField] private GameObject spellCardPanel;
        [SerializeField] private TextMeshProUGUI spellCardNameText;
        [SerializeField] private TextMeshProUGUI spellCardTimerText;
        
        [Space(15)]
        [Header("⏸️ Giao diện Tạm dừng")]
        [SerializeField] private GameObject pauseMenuPanel;
        
        [Header("🎬 Hiệu ứng Cinematic")]
        [SerializeField] private Image transitionScreen;
        [SerializeField] private float transitionDuration = 1.0f;
        [SerializeField] private Image comboBurstImage;
        [SerializeField] private float comboBurstDisplayTime = 1.5f;
        
        [Header("🏆 Giao diện Trạng thái Game")]
        [SerializeField] private GameObject winScreen;
        [SerializeField] private GameObject gameOverScreen;

        private Coroutine spellCardDisplayCoroutine;
        private Coroutine spellCardTimerCoroutine; // Thêm tham chiếu cho timer coroutine
        private Canvas canvas;
        
        #region Unity Lifecycle & Event Subscription

        void Awake()
        {
            // Logic Singleton đơn giản hơn, vì GameManager đảm bảo chỉ có 1 được tạo ra
            if (Instance != null)
            {
                // Nếu có một bản sao được tạo ra do lỗi nào đó, hủy nó đi
                Destroy(gameObject.transform.root.gameObject);
                return;
            }
            
            Instance = this;
            // Đảm bảo cả Canvas gốc tồn tại xuyên suốt các màn chơi
            DontDestroyOnLoad(transform.root.gameObject);

            canvas = GetComponentInParent<Canvas>();
            if (canvas == null)
            {
                Debug.LogError("UIManager must be a child of a Canvas!", this);
            }
        }

        // --- GỘP TẤT CẢ CÁC ĐĂNG KÝ SỰ KIỆN VÀO MỘT HÀM ONENABLE ---
        void OnEnable()
        {
            // Lắng nghe event từ BossHealth
            BossHealth.OnComboBurstTriggered += ShowComboBurst;
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        // --- GỘP TẤT CẢ CÁC HỦY ĐĂNG KÝ SỰ KIỆN VÀO MỘT HÀM ONDISABLE ---
        void OnDisable()
        {
            // Hủy lắng nghe
            BossHealth.OnComboBurstTriggered -= ShowComboBurst;
            SceneManager.sceneLoaded -= OnSceneLoaded; 
        }

        void Start()
        {
            // Ẩn tất cả các màn hình không cần thiết khi bắt đầu
            HideAllScreens();
            
            // Hiện các UI cố định
            if (playerHealthBar != null) playerHealthBar.gameObject.SetActive(true);
            
            // Reset UI
            ResetAllSkillCooldowns();
        }

        #endregion
        
        #region Pause Menu & Game State UI
        
        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {

            
            // Cập nhật lại UI của Player khi vào scene mới
            if(GameManager.Instance != null && GameManager.Instance.PlayerObject != null)
            {
                PlayerState playerState = GameManager.Instance.PlayerObject.GetComponent<PlayerState>();
                if(playerState != null)
                {
                    playerState.UpdateAllUI();
                }
            }
        }
        
        public void ShowPauseMenu()
        {
            if (pauseMenuPanel != null) pauseMenuPanel.SetActive(true);
        }

        public void HidePauseMenu()
        {
            if (pauseMenuPanel != null) pauseMenuPanel.SetActive(false);
        }
        
        public void ShowWinScreen()
        {
            if(winScreen != null) winScreen.SetActive(true);
        }

        public void ShowGameOverScreen()
        {
            if(gameOverScreen != null) gameOverScreen.SetActive(true);
        }
        
        /// <summary>
        /// Ẩn tất cả các màn hình trạng thái game (Pause, Win, Game Over).
        /// </summary>
        public void HideAllScreens()
        {
            if(pauseMenuPanel != null) pauseMenuPanel.SetActive(false);
            if(winScreen != null) winScreen.SetActive(false);
            if(gameOverScreen != null) gameOverScreen.SetActive(false);
        }

        // Các hàm này sẽ được gọi bởi các nút trên UI
        public void OnResumeButtonPressed()
        {
            GameManager.Instance.ResumeGame();
        }

        public void OnRestartLevelButtonPressed() // Đổi tên để rõ ràng
        {
            GameManager.Instance.RestartLevel();
        }

        public void OnRestartFromBeginningButtonClicked()
        {
            GameManager.Instance.RestartGameFromBeginning();
        }

        public void OnQuitButtonPressed()
        {
            GameManager.Instance.QuitGame();
        }
        
        #endregion
        
        #region Boss UI Methods

        public void ShowBossUI()
        {
            if (bossUIGroup != null) bossUIGroup.SetActive(true);
        }

        public void HideBossUI()
        {
            if (bossUIGroup != null) bossUIGroup.SetActive(false);
            HideSpellCardUI(); // Luôn ẩn UI spell card khi boss biến mất
            ClearSpellCardDeclaration();
        }
        
        public void UpdateBossHealthBar(float fillAmount)
        {
            if (bossHealthBar != null && bossHealthBar.gameObject.activeInHierarchy)
                bossHealthBar.SetProgress(fillAmount);
        }

        public void DeclareSpellCard(string name, float timeLimit)
        {
            if (spellCardDisplayCoroutine != null) StopCoroutine(spellCardDisplayCoroutine);
            spellCardDisplayCoroutine = StartCoroutine(ShowAndHideSpellCardDeclarationRoutine(name));
            
            // Hiển thị panel spell card và bắt đầu đếm ngược
            if (spellCardPanel != null) spellCardPanel.SetActive(true);
            if (spellCardNameText != null) spellCardNameText.text = name;
            
            if (spellCardTimerCoroutine != null) StopCoroutine(spellCardTimerCoroutine);
            spellCardTimerCoroutine = StartCoroutine(UpdateSpellCardTimer(timeLimit));
        }

        private IEnumerator ShowAndHideSpellCardDeclarationRoutine(string name)
        {
            if (spellCardDeclarationGroup != null) spellCardDeclarationGroup.SetActive(true);
            if (spellCardDeclarationNameText != null)
            {
                spellCardDeclarationNameText.text = name;
            }

            if (spellCardAnimator != null)
            {
                // Reset animator về trạng thái đầu để animation có thể chạy lại
                spellCardAnimator.Rebind();
                spellCardAnimator.Update(0f);
                spellCardAnimator.SetTrigger("Declare");
            }
            
            yield return new WaitForSeconds(spellCardDisplayTime);
            
            if (spellCardDeclarationGroup != null) spellCardDeclarationGroup.SetActive(false);
        }
        
        public void HideSpellCardUI()
        {
            if (spellCardTimerCoroutine != null)
            {
                StopCoroutine(spellCardTimerCoroutine);
                spellCardTimerCoroutine = null;
            }
            if (spellCardPanel != null)
            {
                spellCardPanel.SetActive(false);
            }
        }
        
        public void ClearSpellCardDeclaration()
        {
            if (spellCardDisplayCoroutine != null)
            {
                StopCoroutine(spellCardDisplayCoroutine);
                spellCardDisplayCoroutine = null;
            }
            
            if (spellCardDeclarationGroup != null)
                spellCardDeclarationGroup.SetActive(false);
        }
        
        private IEnumerator UpdateSpellCardTimer(float timeLimit)
        {
            float timer = timeLimit;
            while (timer > 0)
            {
                timer -= Time.deltaTime;
                if (spellCardTimerText != null)
                {
                    spellCardTimerText.text = timer.ToString("F2");
                }
                yield return null;
            }
            if (spellCardTimerText != null)
            {
                spellCardTimerText.text = "0.00";
            }
        }
        
        #endregion

        #region Player UI Methods
        
        public void UpdateSkillCooldown(PlayerSkillManager.SkillType skillType, float fillAmount, float remainingTime)
        {
            // ... (Code này giữ nguyên)
        }

        public void UpdatePlayerHealthBar(float fillAmount)
        {
            if (playerHealthBar != null) playerHealthBar.SetProgress(fillAmount);
        }
        public void UpdateBombsText(int bombs)
        {
            if (bombsText != null) bombsText.text = $"Bom: {bombs}";
        }
        public void UpdatePowerText(float power)
        {
            if (powerText != null) powerText.text = $"Sức mạnh: {power:F2}";
        }
        public void UpdateScoreText(long score)
        {
            if (scoreText != null) scoreText.text = $"Điểm: {score:N0}";
        }
        
        private void ResetAllSkillCooldowns()
        {
            if (bulletClear_CooldownImage != null) bulletClear_CooldownImage.fillAmount = 0;
            if (bulletClear_CooldownText != null) bulletClear_CooldownText.enabled = false;
            if (invincibility_CooldownImage != null) invincibility_CooldownImage.fillAmount = 0;
            if (invincibility_CooldownText != null) invincibility_CooldownText.enabled = false;
        }
        
        /// <summary>
        /// Cung cấp tham chiếu đến Image của skill cooldown cho các script khác.
        /// </summary>
        public Image GetSkillCooldownImage(PlayerSkillManager.SkillType skillType)
        {
            switch (skillType)
            {
                case PlayerSkillManager.SkillType.BulletClear: return bulletClear_CooldownImage;
                case PlayerSkillManager.SkillType.Invincibility: return invincibility_CooldownImage;
                default:
                    Debug.LogWarning($"UIManager: Request for unknown skill cooldown image type: {skillType}");
                    return null;
            }
        }

        /// <summary>
        /// Cung cấp tham chiếu đến Text của skill cooldown cho các script khác.
        /// </summary>
        public TextMeshProUGUI GetSkillCooldownText(PlayerSkillManager.SkillType skillType)
        {
            switch (skillType)
            {
                case PlayerSkillManager.SkillType.BulletClear: return bulletClear_CooldownText;
                case PlayerSkillManager.SkillType.Invincibility: return invincibility_CooldownText;
                default:
                    Debug.LogWarning($"UIManager: Request for unknown skill cooldown text type: {skillType}");
                    return null;
            }
        }
        
        #endregion
        
        #region Cinematic Effects
        
        public IEnumerator ShowTransition()
        {
            if (transitionScreen == null) yield break;
            
            float elapsedTime = 0f;
            transitionScreen.gameObject.SetActive(true);
            
            while (elapsedTime < transitionDuration)
            {
                float alpha = Mathf.Clamp01(elapsedTime / transitionDuration);
                transitionScreen.color = new Color(transitionScreen.color.r, transitionScreen.color.g, transitionScreen.color.b, alpha);
                elapsedTime += Time.unscaledDeltaTime;
                yield return null;
            }
            transitionScreen.color = new Color(transitionScreen.color.r, transitionScreen.color.g, transitionScreen.color.b, 1f);
        }
        
        public IEnumerator HideTransition()
        {
            if (transitionScreen == null) yield break;

            float elapsedTime = 0f;
            
            while (elapsedTime < transitionDuration)
            {
                float alpha = 1f - Mathf.Clamp01(elapsedTime / transitionDuration);
                transitionScreen.color = new Color(transitionScreen.color.r, transitionScreen.color.g, transitionScreen.color.b, alpha);
                elapsedTime += Time.unscaledDeltaTime;
                yield return null;
            }
            transitionScreen.color = new Color(transitionScreen.color.r, transitionScreen.color.g, transitionScreen.color.b, 0f);
            transitionScreen.gameObject.SetActive(false);
        }

        private void ShowComboBurst()
        {
            if (comboBurstImage == null) return;
            
            if(gameObject.activeInHierarchy) // Đảm bảo UIManager đang hoạt động
            {
                StopCoroutine("ComboBurstRoutine");
                StartCoroutine(ComboBurstRoutine());
            }
        }

        private IEnumerator ComboBurstRoutine()
        {
            comboBurstImage.gameObject.SetActive(true);
            comboBurstImage.color = new Color(1, 1, 1, 1);
            
            yield return new WaitForSeconds(comboBurstDisplayTime * 0.7f);
            
            float fadeDuration = comboBurstDisplayTime * 0.3f;
            float elapsedTime = 0f;
            
            while(elapsedTime < fadeDuration)
            {
                float alpha = 1f - (elapsedTime / fadeDuration);
                comboBurstImage.color = new Color(1, 1, 1, alpha);
                elapsedTime += Time.deltaTime;
                yield return null;
            }
            
            comboBurstImage.gameObject.SetActive(false);
        }
        
        #endregion
    }
}