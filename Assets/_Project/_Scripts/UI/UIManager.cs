using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Dùng TextMeshPro để chữ đẹp hơn

namespace _Project._Scripts.UI
{
    public class UIManager : MonoBehaviour
    {
        public static UIManager Instance { get; private set; }

        [Header("Player UI")]
        public TextMeshProUGUI livesText;
        public TextMeshProUGUI bombsText;
        public TextMeshProUGUI powerText;

        [Header("Spell Card UI")]
        public GameObject spellCardPanel;
        public TextMeshProUGUI spellCardNameText;
        public TextMeshProUGUI spellCardTimerText;

        [Header("Boss Health UI")]
        public Slider bossHealthSlider;

        void Awake()
        {
            Instance = this;
        }

        public void DeclareSpellCard(string name, float time)
        {
            spellCardPanel.SetActive(true);
            spellCardNameText.text = name;
            // Cập nhật timer liên tục (bạn có thể dùng Coroutine ở đây)
            spellCardTimerText.text = time.ToString("F2"); 
        }

        public void ClearSpellCardDeclaration()
        {
            spellCardPanel.SetActive(false);
        }

        public void UpdateBossHealthBar(float fillAmount) // 0.0 to 1.0
        {
            bossHealthSlider.value = fillAmount;
        }
    
        // CÁC HÀM ĐƯỢC BỔ SUNG:
        public void UpdateLives(int currentLives)
        {
            if (livesText != null)
            {
                livesText.text = "Lives: " + currentLives;
            }
        }

        public void UpdateBombs(int currentBombs)
        {
            if (bombsText != null)
            {
                bombsText.text = "Bombs: " + currentBombs;
            }
        }

        public void UpdatePower(int currentPower)
        {
            if (powerText != null)
            {
                powerText.text = "Power: " + currentPower;
            }
        }
    }
}