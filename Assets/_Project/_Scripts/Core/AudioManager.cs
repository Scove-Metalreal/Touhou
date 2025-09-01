using System;
using UnityEngine;

namespace _Project._Scripts.Core
{
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }

        [Header("Âm thanh")]
        public Sound[] musicSounds; // Danh sách nhạc nền
        public Sound[] sfxSounds;   // Danh sách hiệu ứng âm thanh (SFX)

        [Header("Audio Sources")]
        [SerializeField] private AudioSource musicSource;
        [SerializeField] private AudioSource sfxSource;

        // Khóa để lưu trữ giá trị âm lượng
        private const string MusicVolumeKey = "MusicVolume";
        private const string SfxVolumeKey = "SfxVolume";

        void Awake()
        {
            // --- Singleton Pattern ---
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject); // Không hủy đối tượng này khi chuyển scene
            }
            else
            {
                Destroy(gameObject); // Nếu đã có 1 AudioManager, hủy cái mới này đi
                return;
            }

            // Tải âm lượng đã lưu
            LoadVolume();
        }

        // private void Start()
        // {
        //     // Ví dụ: Phát nhạc nền cho Main Menu khi game bắt đầu
        //     // Thay "MainMenuTheme" bằng tên nhạc nền của bạn
        //     PlayMusic("MainMenuTheme");
        // }

        // Hàm phát nhạc nền
        public void PlayMusic(string name)
        {
            Sound s = Array.Find(musicSounds, sound => sound.name == name);
            if (s == null)
            {
                Debug.LogWarning("AudioManager: Không tìm thấy nhạc nền tên: " + name);
                return;
            }

            musicSource.clip = s.clip;
            musicSource.loop = true;
            musicSource.Play();
        }

        // Hàm phát hiệu ứng âm thanh
        public void PlaySFX(string name)
        {
            Sound s = Array.Find(sfxSounds, sound => sound.name == name);
            if (s == null)
            {
                Debug.LogWarning("AudioManager: Không tìm thấy SFX tên: " + name);
                return;
            }

            // PlayOneShot cho phép phát nhiều hiệu ứng chồng lên nhau mà không cắt ngang
            sfxSource.PlayOneShot(s.clip, s.volume);
        }

        // Hàm thay đổi âm lượng nhạc nền (gọi từ Slider)
        public void SetMusicVolume(float volume)
        {
            musicSource.volume = volume;
            PlayerPrefs.SetFloat(MusicVolumeKey, volume); // Lưu giá trị
            PlayerPrefs.Save();
        }

        // Hàm thay đổi âm lượng SFX (gọi từ Slider)
        public void SetSfxVolume(float volume)
        {
            sfxSource.volume = volume;
            PlayerPrefs.SetFloat(SfxVolumeKey, volume); // Lưu giá trị
            PlayerPrefs.Save();
        }

        // Tải giá trị âm lượng đã lưu
        private void LoadVolume()
        {
            // Mặc định là 1 (tối đa) nếu chưa có giá trị nào được lưu
            musicSource.volume = PlayerPrefs.GetFloat(MusicVolumeKey, 1f);
            sfxSource.volume = PlayerPrefs.GetFloat(SfxVolumeKey, 1f);
        }

        // Hàm lấy giá trị âm lượng hiện tại (để cập nhật slider khi khởi động)
        public float GetMusicVolume()
        {
            return musicSource.volume;
        }

        public float GetSfxVolume()
        {
            return sfxSource.volume;
        }
    }
}