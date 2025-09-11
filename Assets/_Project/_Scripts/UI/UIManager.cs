// FILE: _Project/_Scripts/UI/UIManager.cs (PHIÊN BẢN CUỐI CÙNG - FULL)

using UnityEngine;
using TMPro;
using System.Collections;
using _Project._Scripts.Bosses;
using _Project._Scripts.Core;
using _Project._Scripts.Player;
using UnityEngine.UI;
using ProgressBar = ThirdParty.InfinityPBR___Magic_Pig_Games.Progress_Bar.Scripts.ProgressBar;

namespace _Project._Scripts.UI
{
    public class UIManager : MonoBehaviour
    {
        public static UIManager Instance { get; private set; }

        [Header("📊 Giao diện Gameplay")]
        [Tooltip("Kéo GameObject 'Player Visual UI' từ Hierarchy vào đây.")]
        [SerializeField] private GameObject playerHUD;
        [SerializeField] private ProgressBar playerHealthBar;
        [SerializeField] private TextMeshProUGUI bombsText;
        [SerializeField] private TextMeshProUGUI powerText;
        [SerializeField] private TextMeshProUGUI scoreText;
        
        [Space(10)]
        [Tooltip("Kéo GameObject 'Boss Visual UI' từ Hierarchy vào đây.")]
        [SerializeField] private GameObject bossUIGroup;
        [SerializeField] private ProgressBar bossHealthBar;
        
        [Space(15)]
        [Header("✨ Giao diện Kỹ năng Player")]
        [SerializeField] private Image bulletClear_CooldownImage;
        [SerializeField] private TextMeshProUGUI bulletClear_CooldownText;
        [SerializeField] private Image invincibility_CooldownImage;
        [SerializeField] private TextMeshProUGUI invincibility_CooldownText;

        [Space(15)]
        [Header("Spell Card UI")]
        [SerializeField] private GameObject spellCardDeclarationGroup;
        [SerializeField] private TextMeshProUGUI spellCardDeclarationNameText;
        [SerializeField] private Animator spellCardAnimator;
        [SerializeField] private float spellCardDisplayTime = 3.5f;
        [SerializeField] private GameObject spellCardPanel;
        [SerializeField] private TextMeshProUGUI spellCardNameText;
        [SerializeField] private TextMeshProUGUI spellCardTimerText;
        
        [Space(15)]
        [Header("PANELS & SCREENS")]
        [Tooltip("Kéo GameObject 'Menu' từ Hierarchy vào đây.")]
        [SerializeField] private GameObject menuPanel;
        [Tooltip("Kéo GameObject 'Win' từ Hierarchy vào đây.")]
        [SerializeField] private GameObject winScreen;
        [Tooltip("Kéo GameObject 'GameOver' từ Hierarchy vào đây.")]
        [SerializeField] private GameObject gameOverScreen;

        [Header("MENU BUTTONS")]
        [Tooltip("Tham chiếu đến nút 'Resume/Start' bên trong Panel 'Menu'.")]
        [SerializeField] private Button resumeStartButton;
        [Tooltip("Tham chiếu đến Text của nút 'Resume/Start'.")]
        [SerializeField] private TextMeshProUGUI resumeStartButtonText;
        
        [Header("🎬 Hiệu ứng Cinematic")]
        [SerializeField] private Image transitionScreen;
        [SerializeField] private float transitionDuration = 1.0f;
        [SerializeField] private Image comboBurstImage;
        [SerializeField] private float comboBurstDisplayTime = 1.5f;

        private Coroutine spellCardDisplayCoroutine;
        private Coroutine spellCardTimerCoroutine;

        #region Unity Lifecycle & Event Subscription

        void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            // UIManager này là một phần của scene, không dùng DontDestroyOnLoad
        }

        void OnEnable()
        {
            BossHealth.OnComboBurstTriggered += ShowComboBurst;
        }

        void OnDisable()
        {
            BossHealth.OnComboBurstTriggered -= ShowComboBurst;
        }

        void Start()
        {
            // Ẩn tất cả các panel có thể ẩn khi bắt đầu
            HideAllScreens();
            
            // Ẩn các UI của gameplay ban đầu
            HideGameplayUI();
            
            // Báo cho GameManager biết rằng UIManager đã sẵn sàng và chờ lệnh
            if (GameManager.Instance != null)
            {
                GameManager.Instance.NotifyUIManagerReady();
            }
        }

        #endregion
        
        #region Main Menu, Pause & Game State UI

        /// <summary>
        /// Hiển thị menu chính khi mới vào game. Được GameManager gọi.
        /// </summary>
        public void ShowMainMenu()
        {
            // Chỉ hiện menu, không ẩn các màn hình khác nữa vì Start đã làm
            if (menuPanel != null) menuPanel.SetActive(true);
            
            if (resumeStartButtonText != null) resumeStartButtonText.text = "Bắt Đầu";
            
            if (resumeStartButton != null)
            {
                resumeStartButton.onClick.RemoveAllListeners();
                resumeStartButton.onClick.AddListener(GameManager.Instance.StartGame);
            }
        }

        /// <summary>
        /// Hiển thị menu pause trong khi chơi. Được GameManager gọi.
        /// </summary>
        public void ShowPauseMenu()
        {
            if (menuPanel != null) menuPanel.SetActive(true);
            
            if (resumeStartButtonText != null) resumeStartButtonText.text = "Tiếp Tục";
            
            if (resumeStartButton != null)
            {
                resumeStartButton.onClick.RemoveAllListeners();
                resumeStartButton.onClick.AddListener(GameManager.Instance.ResumeGame);
            }
        }

        public void HidePauseMenu()
        {
            if (menuPanel != null) menuPanel.SetActive(false);
        }
        
        public void ShowWinScreen()
        {
            HideGameplayUI();
            if (winScreen != null) winScreen.SetActive(true);
        }

        public void ShowGameOverScreen()
        {
            HideGameplayUI();
            if (gameOverScreen != null) gameOverScreen.SetActive(true);
        }
        
        public void HideAllScreens()
        {
            if (menuPanel != null) menuPanel.SetActive(false);
            if (winScreen != null) winScreen.SetActive(false);
            if (gameOverScreen != null) gameOverScreen.SetActive(false);
        }

        public void ShowGameplayUI()
        {
            if (playerHUD != null) playerHUD.SetActive(true);
        }
        
        public void HideGameplayUI()
        {
            if (playerHUD != null) playerHUD.SetActive(false);
            HideBossUI();
        }
        
        // --- CÁC HÀM GỌI TỪ NÚT TRÊN UI ---
        public void OnResumeButtonPressed() { GameManager.Instance.ResumeGame(); }
        public void OnRestartLevelButtonPressed() { GameManager.Instance.RestartLevel(); }
        public void OnRestartFromBeginningButtonClicked() { GameManager.Instance.RestartGameFromBeginning(); }
        public void OnQuitButtonPressed() { GameManager.Instance.QuitGame(); }
        
        #endregion
        
        #region Boss UI Methods

        public void ShowBossUI()
        {
            if (bossUIGroup != null) bossUIGroup.SetActive(true);
        }
        
        public void HideBossUI()
        {
            if (bossUIGroup != null) bossUIGroup.SetActive(false);
            HideSpellCardUI(); 
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
        
        public void UpdateAllPlayerUI()
        {
            if (GameManager.Instance == null || GameManager.Instance.PlayerObject == null) return;
            
            PlayerState playerState = GameManager.Instance.PlayerObject.GetComponent<PlayerState>();
            PlayerSkillManager playerSkillManager = GameManager.Instance.PlayerObject.GetComponent<PlayerSkillManager>();

            if (playerState != null) 
            {
                playerState.UpdateAllUI();
            }
            if (playerSkillManager != null) 
            {
                playerSkillManager.InitializeUI();
            }
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
        
        public Image GetSkillCooldownImage(PlayerSkillManager.SkillType skillType)
        {
            switch (skillType)
            {
                case PlayerSkillManager.SkillType.BulletClear: return bulletClear_CooldownImage;
                case PlayerSkillManager.SkillType.Invincibility: return invincibility_CooldownImage;
                default:
                    Debug.LogWarning($"UIManager: Yêu cầu hình ảnh cooldown cho skill không xác định: {skillType}");
                    return null;
            }
        }
        
        public TextMeshProUGUI GetSkillCooldownText(PlayerSkillManager.SkillType skillType)
        {
            switch (skillType)
            {
                case PlayerSkillManager.SkillType.BulletClear: return bulletClear_CooldownText;
                case PlayerSkillManager.SkillType.Invincibility: return invincibility_CooldownText;
                default:
                    Debug.LogWarning($"UIManager: Yêu cầu text cooldown cho skill không xác định: {skillType}");
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
                // Sử dụng Time.unscaledDeltaTime vì hiệu ứng chuyển cảnh có thể xảy ra khi game bị pause
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
            
            if(gameObject.activeInHierarchy)
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