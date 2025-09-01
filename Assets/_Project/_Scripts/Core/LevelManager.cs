// FILE: _Project/Scripts/Core/LevelManager.cs

using UnityEngine;
using UnityEngine.SceneManagement;

namespace _Project._Scripts.Core
{
    public class LevelManager : MonoBehaviour
    {
        public static LevelManager Instance;
    
        [Tooltip("Tên của màn chơi tiếp theo (đặt trong Build Settings).")]
        public string nextSceneName;

        void Awake()
        {
            Instance = this;
        }

        public void GoToNextLevel()
        {
            // Tạm thời chỉ load scene, sau có thể thêm hiệu ứng chuyển cảnh
            if (!string.IsNullOrEmpty(nextSceneName))
            {
                SceneManager.LoadScene(nextSceneName);
            }
            else
            {
                Debug.Log("YOU WIN! (No next scene specified)");
                // Hiển thị màn hình chiến thắng
            }
        }
    }
}