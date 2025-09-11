using System.Collections;
using UnityEngine;

namespace _Project._Scripts.Bosses.AttackPatterns
{
    public class IcicleFall : AttackPattern
    {
        [Header("Pattern Settings")]
        [Tooltip("Prefab của viên đạn sẽ được bắn ra.")]
        public GameObject bulletPrefab;
        [Tooltip("Tần suất rơi của đạn (viên/giây).")]
        public float spawnRate = 0.1f;
        [Tooltip("Tổng thời gian pattern này sẽ diễn ra (giây).")]
        public float duration = 5f; // BIẾN MỚI: Thêm thời gian tồn tại cho pattern

        // Ghi đè phương thức Execute từ lớp cha
        public override IEnumerator Execute()
        {
            Debug.Log("Bắt đầu pattern IcicleFall.");
            
            float elapsedTime = 0f;
            
            // Lấy biên của màn hình một lần để tối ưu
            float screenTop = Camera.main.orthographicSize;
            float screenWidth = screenTop * Camera.main.aspect;

            // SỬA ĐỔI: Chạy vòng lặp cho đến khi hết thời gian `duration`
            while (elapsedTime < duration)
            {
                // Tạo một vị trí ngẫu nhiên ở phía trên màn hình
                Vector2 spawnPosition = new Vector2(Random.Range(-screenWidth, screenWidth), screenTop + 1);
            
                // Lấy đạn từ pool (Giả sử bạn có một ObjectPooler)
                // GameObject bullet = ObjectPooler.Instance.GetPooledObject(bulletPrefab);
                GameObject bullet = Instantiate(bulletPrefab, spawnPosition, Quaternion.Euler(0, 0, 180)); // Tạm dùng Instantiate để test
                
                // Chờ một chút rồi tạo viên tiếp theo
                yield return new WaitForSeconds(spawnRate);
                
                // Cập nhật thời gian đã trôi qua
                elapsedTime += spawnRate;
            }
            
            Debug.Log("Kết thúc pattern IcicleFall.");
            // Khi coroutine kết thúc ở đây, BossShooting sẽ tự động chuyển sang pattern tiếp theo.
        }
    }
}