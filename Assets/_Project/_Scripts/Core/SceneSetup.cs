// FILE: _Project/_Scripts/Core/SceneSetup.cs

using UnityEngine;

namespace _Project._Scripts.Core
{
    /// <summary>
    /// Component này được đặt trong mỗi Scene để chứa các tham chiếu Transform
    /// cụ thể của Scene đó (điểm spawn của Player và Boss).
    /// GameManager sẽ tìm đối tượng này khi Scene mới được tải.
    /// </summary>
    public class SceneSetup : MonoBehaviour
    {
        [Header("Scene Specific Spawn Points")]
        public Transform bossSpawnPoint;
        public Transform bossInitialSpawnPoint;

        public Transform playerSpawnPoint;
        public Transform playerInitialSpawnPoint;
        public Transform playerExitPoint;

        void Awake()
        {
            // Đảm bảo chỉ có một SceneSetup trong scene để tránh lỗi
            if (FindObjectsOfType<SceneSetup>().Length > 1)
            {
                Debug.LogWarning("SceneSetup: Multiple SceneSetup objects found in this scene. Destroying this duplicate.", this);
                Destroy(gameObject);
            }
        }
    }
}