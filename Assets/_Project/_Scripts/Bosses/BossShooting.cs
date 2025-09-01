using System.Collections.Generic;
using _Project._Scripts.Bosses.AttackPatterns;
using UnityEngine;

namespace _Project._Scripts.Bosses
{
    public class BossShooting : MonoBehaviour
    {
        private List<AttackPattern> currentAttackPatterns;

        /// <summary>
        /// Nhận danh sách các pattern từ BossController.
        /// </summary>
        public void SetAttackPatterns(List<AttackPattern> newPatterns)
        {
            currentAttackPatterns = newPatterns;
        }

        /// <summary>
        /// Ra lệnh cho TẤT CẢ các pattern được gán bắt đầu bắn CÙNG LÚC.
        /// </summary>
        public void StartShooting()
        {
            if (currentAttackPatterns == null || currentAttackPatterns.Count == 0)
            {
                Debug.LogWarning("Không có Attack Pattern nào được gán cho BossShooting.", this);
                return;
            }

            // Duyệt qua toàn bộ danh sách và gọi StartFiring() trên mỗi pattern
            foreach (var pattern in currentAttackPatterns)
            {
                if (pattern != null)
                {
                    pattern.StartFiring();
                }
            }
        }

        /// <summary>
        /// Ra lệnh cho TẤT CẢ các pattern đang hoạt động ngừng bắn.
        /// </summary>
        public void StopShooting()
        {
            if (currentAttackPatterns == null) return;

            // Duyệt qua toàn bộ danh sách và gọi StopFiring() trên mỗi pattern
            foreach (var pattern in currentAttackPatterns)
            {
                if (pattern != null)
                {
                    pattern.StopFiring();
                }
            }
        }
    }
}