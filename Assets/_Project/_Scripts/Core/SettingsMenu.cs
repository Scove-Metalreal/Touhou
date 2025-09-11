using UnityEngine;
using UnityEngine.UI;

namespace _Project._Scripts.Core
{
    public class SettingsMenu : MonoBehaviour
    {
        [Header("UI Elements")]
        [SerializeField] private Slider musicSlider;
        [SerializeField] private Slider sfxSlider;

        void Start()
        {
            // Đảm bảo AudioManager đã tồn tại
            if (AudioManager.Instance != null)
            {
                // Cập nhật giá trị ban đầu của slider khớp với âm lượng hiện tại
                musicSlider.value = AudioManager.Instance.GetMusicVolume();
                sfxSlider.value = AudioManager.Instance.GetSfxVolume();

                // Gán sự kiện OnValueChanged cho các slider
                musicSlider.onValueChanged.AddListener(AudioManager.Instance.SetMusicVolume);
                sfxSlider.onValueChanged.AddListener(AudioManager.Instance.SetSfxVolume);
            }
            else
            {
                Debug.LogError("SettingsMenu: Không tìm thấy AudioManager trong Scene!");
            }
        }
    }
}