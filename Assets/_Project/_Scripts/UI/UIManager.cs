// FILE: _Project/_Scripts/UI/UIManager.cs (VERSION 4.3 - EXPLICIT HIDE)

using UnityEngine;
using TMPro;
using MagicPigGames;
using System.Collections;
using _Project._Scripts.Core;
using UnityEngine.UIElements;
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
        [Header("👹 Giao diện Boss")]
        [SerializeField] private ProgressBar bossHealthBar;
        [SerializeField] private GameObject spellCardDeclarationGroup;
        [SerializeField] private Animator spellCardAnimator;
        [SerializeField] private float spellCardDisplayTime = 3.5f;
        // Có thể thêm các thành phần khác như tên, chân dung boss...
        
        [Space(15)]
        [Header("Spell Card UI")]
        [SerializeField] private GameObject spellCardPanel;
        [SerializeField] private TextMeshProUGUI spellCardNameText;
        [SerializeField] private TextMeshProUGUI spellCardTimerText;
        
        [Space(15)]
        [Header("⏸️ Giao diện Tạm dừng")]
        [SerializeField] private GameObject pauseMenuPanel;

        private Coroutine spellCardDisplayCoroutine;

        #region Unity Lifecycle

        void Awake()
        {
            if (Instance != null && Instance != this)
                Destroy(gameObject);
            else
                Instance = this;
        }

        void Start()
        {
            if (playerHealthBar != null)
                playerHealthBar.gameObject.SetActive(true);
            
            if(pauseMenuPanel != null)
                pauseMenuPanel.SetActive(false);
        }

        #endregion
        
        #region Pause Menu UI Methods
        public void ShowPauseMenu()
        {
            if (pauseMenuPanel != null)
                pauseMenuPanel.SetActive(true);
        }

        public void HidePauseMenu()
        {
            if (pauseMenuPanel != null)
                pauseMenuPanel.SetActive(false);
        }

        // Các hàm này sẽ được gọi bởi các nút trên UI
        public void OnResumeButtonPressed()
        {
            GameManager.Instance.ResumeGame();
        }

        public void OnRestartButtonPressed()
        {
            GameManager.Instance.RestartLevel();
        }

        public void OnQuitButtonPressed()
        {
            GameManager.Instance.QuitGame();
        }
        #endregion
        
        #region Boss UI Methods

        public void ShowBossUI()
        {
            if (bossHealthBar != null)
                bossHealthBar.gameObject.SetActive(true);
        }

        public void HideBossUI()
        {
            if (bossHealthBar != null)
                bossHealthBar.gameObject.SetActive(false);
            
            ClearSpellCardDeclaration();
        }
        
        // Hàm này sẽ được gọi từ BossController
        public void HideSpellCardUI()
        {
            // Dừng coroutine timer đang chạy để tránh lãng phí tài nguyên
            if (spellCardDisplayCoroutine != null)
            {
                StopCoroutine(spellCardDisplayCoroutine);
                spellCardDisplayCoroutine = null; // Reset tham chiếu
            }

            // Ẩn panel chính của Spell Card UI
            if (spellCardPanel != null)
            {
                spellCardPanel.SetActive(false);
            }
        }
        
        private IEnumerator UpdateSpellCardTimer(float timeLimit)
        {
            float timer = timeLimit;
            while (timer > 0)
            {
                timer -= Time.deltaTime;
                if (spellCardTimerText != null)
                {
                    // Cập nhật text, làm tròn đến 2 chữ số thập phân
                    spellCardTimerText.text = timer.ToString("F2");
                }
                yield return null;
            }

            // Đảm bảo timer hiển thị 0.00 khi hết giờ
            if (spellCardTimerText != null)
            {
                spellCardTimerText.text = "0.00";
            }
        }
        
        public void UpdateBossHealthBar(float fillAmount)
        {
            if (bossHealthBar != null && bossHealthBar.gameObject.activeInHierarchy)
                bossHealthBar.SetProgress(fillAmount);
        }

        public void DeclareSpellCard(string name, float time)
        {
            if (spellCardDisplayCoroutine != null)
                StopCoroutine(spellCardDisplayCoroutine);
            
            spellCardDisplayCoroutine = StartCoroutine(ShowAndHideSpellCardRoutine(name));
        }

        private IEnumerator ShowAndHideSpellCardRoutine(string name)
        {
            // --- Hiển thị ---
            // Bật cả panel nền và text lên
            if (spellCardDeclarationGroup != null)
                spellCardDeclarationGroup.SetActive(true);
            if (spellCardNameText != null)
            {
                spellCardNameText.gameObject.SetActive(true);
                spellCardNameText.text = name;
            }

            if (spellCardAnimator != null)
                spellCardAnimator.SetTrigger("Declare");
            
            // --- Chờ ---
            yield return new WaitForSeconds(spellCardDisplayTime);
            
            // --- Ẩn ---
            // SỬA LỖI QUAN TRỌNG: Ẩn cả hai đối tượng để đảm bảo chúng biến mất
            if (spellCardDeclarationGroup != null)
                spellCardDeclarationGroup.SetActive(false);
            if (spellCardNameText != null)
                spellCardNameText.gameObject.SetActive(false);
        }

        public void ClearSpellCardDeclaration()
        {
            if (spellCardDisplayCoroutine != null)
            {
                StopCoroutine(spellCardDisplayCoroutine);
                spellCardDisplayCoroutine = null;
            }
            
            // SỬA LỖI QUAN TRỌNG: Ẩn cả hai đối tượng
            if (spellCardDeclarationGroup != null)
                spellCardDeclarationGroup.SetActive(false);
            if (spellCardNameText != null)
                spellCardNameText.gameObject.SetActive(false);
        }

        #endregion

        #region Player UI Methods
        public void UpdatePlayerHealthBar(float fillAmount)
        {
            if (playerHealthBar != null)
            {
                playerHealthBar.SetProgress(fillAmount);
            }
        }
        public void UpdateBombsText(int bombs)
        {
            if (bombsText != null)
            {
                bombsText.text = $"Bom: {bombs}";
            }
        }
        public void UpdatePowerText(float power)
        {
            if (powerText != null)
            {
                powerText.text = $"Sức mạnh: {power:F2}";
            }
        }
        public void UpdateScoreText(long score)
        {
            if (scoreText != null)
            {
                scoreText.text = $"Điểm: {score:N0}";
            }
        }
        #endregion
    }
}

