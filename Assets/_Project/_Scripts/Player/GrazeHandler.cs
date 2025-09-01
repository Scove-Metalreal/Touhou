// FILE: _Project/Scripts/Player/GrazeHandler.cs
using UnityEngine;
using System.Collections.Generic;

public class GrazeHandler : MonoBehaviour
{
    // Dùng List để theo dõi những viên đạn đã được tính graze trong frame này
    // để tránh việc một viên đạn cộng điểm nhiều lần.
    private HashSet<GameObject> grazedBulletsThisFrame = new HashSet<GameObject>();

    void LateUpdate()
    {
        // Xóa danh sách vào cuối mỗi frame
        grazedBulletsThisFrame.Clear();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // Chỉ tương tác với đạn của địch
        if (other.CompareTag("EnemyBullet"))
        {
            // Kiểm tra xem viên đạn này đã được tính graze hay chưa
            if (!grazedBulletsThisFrame.Contains(other.gameObject))
            {
                // Thêm vào danh sách để không tính lại
                grazedBulletsThisFrame.Add(other.gameObject);

                // Logic cộng điểm Graze
                // GameManager.Instance.AddGraze(1); // Giả sử GameManager có hàm này
                Debug.Log("Graze! +1");
                
                // (Tùy chọn) Tạo hiệu ứng hình ảnh/âm thanh cho graze
                // EffectManager.Instance.CreateGrazeEffect(transform.position);
                // AudioManager.Instance.Play("GrazeSFX");
            }
        }
    }
}