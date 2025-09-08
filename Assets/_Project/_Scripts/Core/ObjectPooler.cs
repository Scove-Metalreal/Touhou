// FILE: _Project/_Scripts/Core/ObjectPooler.cs (PHIÊN BẢN DUY TRÌ XUYÊN SCENE)

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement; // Cần thiết để đăng ký sự kiện sceneLoaded
using _Project._Scripts.Core; // Cần thiết để truy cập GameManager và SceneData

public class ObjectPooler : MonoBehaviour
{
    // Lớp Pool này sẽ được sử dụng trong SceneData ScriptableObject
    [System.Serializable]
    public class Pool
    {
        public string tag;
        public GameObject prefab;
        public int size;
    }

    public static ObjectPooler Instance;
    // KHÔNG CẦN danh sách pools được serialized ở đây nữa, nó sẽ được GameManager cung cấp thông qua SceneData

    private Dictionary<string, List<GameObject>> poolDictionary;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // GIỮ OBJECT POOLER TỒN TẠI XUYÊN CÁC SCENE
            poolDictionary = new Dictionary<string, List<GameObject>>(); // Khởi tạo dictionary
        }
        else if (Instance != this)
        {
            // Nếu đã có một ObjectPooler tồn tại, hủy bản duplicate này đi
            Debug.LogWarning("ObjectPooler: Duplicate ObjectPooler found. Destroying this one.");
            Destroy(gameObject);
            return;
        }
    }

    void OnEnable()
    {
        // Đăng ký sự kiện sceneLoaded để biết khi nào cần thiết lập lại pool
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        // Hủy đăng ký để tránh memory leak
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    /// <summary>
    /// Được gọi khi một Scene mới được tải, để thiết lập lại các pool.
    /// </summary>
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log($"<color=blue>ObjectPooler: Scene {scene.name} loaded. Re-initializing pools.</color>");
        
        // Đảm bảo GameManager đã được khởi tạo và có SceneData cho scene hiện tại
        if (GameManager.Instance != null)
        {
            SceneData sceneData = GameManager.Instance.sceneDatas[scene.buildIndex];
            if (sceneData != null)
            {
                InitializePoolsForScene(sceneData.sceneObjectPools);
            }
            else
            {
                Debug.LogError($"ObjectPooler: Could not find SceneData for Build Index {scene.buildIndex} from GameManager. Scene pools will not be initialized.", this);
            }
        }
        else
        {
            Debug.LogError("ObjectPooler: GameManager.Instance is null. Pools cannot be initialized.", this);
        }
    }

    /// <summary>
    /// Xóa tất cả các pool hiện có và tạo mới dựa trên danh sách pool cung cấp.
    /// </summary>
    public void InitializePoolsForScene(List<Pool> newPools)
    {
        // Xóa tất cả các đối tượng hiện có từ các pool cũ và dọn dẹp dictionary
        if (poolDictionary != null)
        {
            foreach (var list in poolDictionary.Values)
            {
                foreach (var obj in list)
                {
                    if (obj != null) Destroy(obj); // Hủy các GameObject đã Instantiate
                }
            }
            poolDictionary.Clear();
            Debug.Log("ObjectPooler: Cleared existing pools and destroyed all pooled objects.");
        }
        else
        {
            poolDictionary = new Dictionary<string, List<GameObject>>();
        }


        if (newPools == null || newPools.Count == 0)
        {
            Debug.LogWarning("ObjectPooler: No new pools defined for this scene. Object Pooler will be empty.");
            return;
        }

        foreach (Pool pool in newPools)
        {
            // Kiểm tra nếu pool với tag này đã tồn tại (không nên xảy ra nếu poolDictionary.Clear() hoạt động)
            if (poolDictionary.ContainsKey(pool.tag))
            {
                Debug.LogWarning($"ObjectPooler: Pool with tag '{pool.tag}' already exists in the new pool list. Skipping this duplicate pool definition.");
                continue;
            }

            if (pool.prefab == null)
            {
                Debug.LogWarning($"ObjectPooler: Prefab for pool tag '{pool.tag}' is null. Skipping this pool definition.");
                continue;
            }

            List<GameObject> objectList = new List<GameObject>();
            for (int i = 0; i < pool.size; i++)
            {
                GameObject obj = Instantiate(pool.prefab);
                obj.SetActive(false);
                objectList.Add(obj);
            }
            poolDictionary.Add(pool.tag, objectList);
            Debug.Log($"ObjectPooler: Created pool for '{pool.tag}' with size {pool.size}.");
        }
    }

    /// <summary>
    /// Lấy một đối tượng từ pool dựa trên tag.
    /// </summary>
    /// <param name="tag">Tag của pool cần lấy đối tượng.</param>
    /// <returns>GameObject đã được lấy từ pool, hoặc null nếu không tìm thấy pool.</returns>
    public GameObject GetPooledObject(string tag)
    {
        if (!poolDictionary.ContainsKey(tag))
        {
            Debug.LogWarning("ObjectPooler: Pool with tag '" + tag + "' doesn't exist in the current scene's configuration.");
            return null;
        }

        List<GameObject> objectList = poolDictionary[tag];
        for (int i = 0; i < objectList.Count; i++)
        {
            if (objectList[i] != null && !objectList[i].activeInHierarchy)
            {
                return objectList[i];
            }
        }

        // Logic Mở rộng Kho (nếu tất cả đối tượng trong pool đang được sử dụng)
        // Lấy thông tin prefab từ SceneData của scene hiện tại để tạo thêm
        if (GameManager.Instance != null && SceneManager.GetActiveScene().buildIndex < GameManager.Instance.sceneDatas.Count)
        {
            SceneData currentSceneData = GameManager.Instance.sceneDatas[SceneManager.GetActiveScene().buildIndex];
            foreach (Pool p in currentSceneData.sceneObjectPools)
            {
                if (p.tag == tag && p.prefab != null)
                {
                    Debug.LogWarning("ObjectPooler: Object Pool with tag '" + tag + "' was empty. Expanding pool size.");
                    GameObject obj = Instantiate(p.prefab);
                    objectList.Add(obj);
                    return obj;
                }
            }
        }
        
        Debug.LogError("ObjectPooler: Could not expand pool for tag '" + tag + "'. Prefab definition missing in SceneData or GameManager instance is null.");
        return null;
    }

    /// <summary>
    /// Hàm tiện ích để lấy đối tượng từ pool bằng tên prefab (dùng prefab.name làm tag).
    /// </summary>
    public GameObject GetPooledObject(GameObject prefab)
    {
        if (prefab == null)
        {
            Debug.LogError("ObjectPooler: Attempted to get pooled object with a null prefab reference.");
            return null;
        }
        return GetPooledObject(prefab.name);
    }
}