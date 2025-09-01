// FILE: _Project/_Scripts/UI/UIManager.cs (VERSION 4.3 - EXPLICIT HIDE)

using UnityEngine;
using TMPro;
using MagicPigGames;
using System.Collections;
using ThirdParty.InfinityPBR___Magic_Pig_Games.Progress_Bar.Scripts;

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
        [SerializeField] private TextMeshProUGUI spellCardNameText;
        [SerializeField] private GameObject spellCardDeclarationGroup;
        [SerializeField] private Animator spellCardAnimator;
        [SerializeField] private float spellCardDisplayTime = 3.5f;

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

