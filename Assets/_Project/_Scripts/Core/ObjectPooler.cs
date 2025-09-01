// FILE: _Project/Scripts/Core/ObjectPooler.cs (VERSION 2.0 - ROBUST)

using System.Collections.Generic;
using UnityEngine;

public class ObjectPooler : MonoBehaviour
{
    // Lớp Pool vẫn giữ nguyên để tương thích với Inspector của bạn
    [System.Serializable]
    public class Pool
    {
        public string tag;
        public GameObject prefab;
        public int size;
    }

    public static ObjectPooler Instance;
    public List<Pool> pools;

    // Thay thế Dictionary<string, Queue> bằng Dictionary<string, List>
    // Điều này cho phép chúng ta truy cập và thêm đối tượng dễ dàng hơn.
    private Dictionary<string, List<GameObject>> poolDictionary;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        poolDictionary = new Dictionary<string, List<GameObject>>();

        foreach (Pool pool in pools)
        {
            // Tạo một List để chứa các đối tượng của pool này
            List<GameObject> objectList = new List<GameObject>();

            for (int i = 0; i < pool.size; i++)
            {
                GameObject obj = Instantiate(pool.prefab);
                obj.SetActive(false);
                objectList.Add(obj); // Thêm vào List
            }
            
            // Thêm List của pool này vào Dictionary
            poolDictionary.Add(pool.tag, objectList);
        }
    }

    // Hàm lấy đối tượng từ kho (đã được viết lại hoàn toàn)
    public GameObject GetPooledObject(string tag)
    {
        // Kiểm tra xem có pool với tag này không
        if (!poolDictionary.ContainsKey(tag))
        {
            Debug.LogWarning("Pool with tag " + tag + " doesn't exist.");
            return null;
        }

        // --- Logic mới để tìm đối tượng không hoạt động ---
        // 1. Tìm trong danh sách một đối tượng đang bị tắt (inactive)
        List<GameObject> objectList = poolDictionary[tag];
        for (int i = 0; i < objectList.Count; i++)
        {
            if (!objectList[i].activeInHierarchy)
            {
                return objectList[i]; // Tìm thấy, trả về ngay lập tức
            }
        }

        // --- Logic Mở rộng Kho (đã được sửa lỗi) ---
        // 2. Nếu vòng lặp trên không tìm thấy đối tượng nào, nghĩa là kho đã cạn hàng
        // Lấy thông tin prefab từ danh sách pools ban đầu
        foreach (Pool p in pools)
        {
            if (p.tag == tag)
            {
                // In ra cảnh báo để bạn biết cần tăng Size trong Inspector
                Debug.LogWarning("Object Pool with tag '" + tag + "' was empty. Expanding pool size.");
                
                // Tạo một đối tượng mới
                GameObject obj = Instantiate(p.prefab);
                
                // QUAN TRỌNG: Thêm đối tượng mới này vào danh sách quản lý
                objectList.Add(obj);
                
                // Trả về đối tượng vừa tạo
                return obj;
            }
        }

        // Trường hợp không thể xảy ra nhưng để an toàn
        return null;
    }

    // Hàm tiện ích vẫn giữ nguyên, hoạt động hoàn hảo với logic mới
    public GameObject GetPooledObject(GameObject prefab)
    {
        return GetPooledObject(prefab.name);
    }
}