// FILE: _Project/_Scripts/Core/SceneData.cs (SAU KHI LOẠI BỎ CÁC BIẾN TRANSFORM)

using System.Collections.Generic;
using UnityEngine;

namespace _Project._Scripts.Core
{
    [CreateAssetMenu(fileName = "New Scene Data", menuName = "Touhou/Scene Data")]
    public class SceneData : ScriptableObject
    {
        [Header("Scene Information")]
        [Tooltip("Tên của scene này (ví dụ: Level1, Level2). Phải khớp với tên trong Build Settings.")]
        public string sceneName;
        [Tooltip("Thứ tự của scene này trong Build Settings. Phải khớp.")]
        public int sceneBuildIndex;

        [Header("Boss Prefab for this Scene")]
        [Tooltip("Prefab của Boss sẽ spawn trong scene này. Để trống nếu scene không có boss.")]
        public GameObject bossPrefab;

        [Header("Object Pooler Configuration for this Scene")]
        [Tooltip("Danh sách các Pool cho ObjectPooler của scene này.")]
        public List<ObjectPooler.Pool> sceneObjectPools = new List<ObjectPooler.Pool>();

        [Header("Audio Configuration")]
        [Tooltip("Tên track nhạc sẽ được phát trong scene này. Để trống nếu muốn dùng nhạc mặc định của GameManager.")]
        public string musicTrackName;

        [Header("Next Scene")]
        [Tooltip("Tên của scene tiếp theo sẽ được tải khi hoàn thành scene này. Để trống nếu đây là scene cuối cùng.")]
        public string nextSceneName;
    }
}