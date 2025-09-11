using _Project._Scripts.Bosses;
using UnityEngine;

namespace _Project._Scripts.Bosses.MovementPatterns
{
    /// <summary>
    /// Lớp trừu tượng (base class) cho tất cả các kiểu di chuyển của Boss.
    /// Mỗi kiểu di chuyển cụ thể (ví dụ: SideToSideMovement) sẽ kế thừa từ lớp này
    /// và triển khai logic di chuyển riêng trong hàm Move().
    /// </summary>
    public abstract class BossMovementPattern : MonoBehaviour
    {
        // Tham chiếu đến các component chính để điều khiển
        protected BossController bossController;
        protected Transform bossTransform;
        
        // Cờ trạng thái để kiểm soát việc di chuyển có được phép hay không
        protected bool canMove = false;

        /// <summary>
        /// Được gọi bởi BossController để khởi tạo pattern với các tham chiếu cần thiết.
        /// </summary>
        /// <param name="controller">Tham chiếu đến BossController chính.</param>
        public virtual void Initialize(BossController controller)
        {
            bossController = controller;
            bossTransform = controller.transform;
        }

        /// <summary>
        /// Kích hoạt trạng thái cho phép di chuyển.
        /// </summary>
        public virtual void StartMoving()
        {
            canMove = true;
        }
        
        /// <summary>
        /// Vô hiệu hóa trạng thái cho phép di chuyển.
        /// </summary>
        public virtual void StopMoving()
        {
            canMove = false;
        }

        /// <summary>
        /// Hàm trừu tượng chứa logic di chuyển chính.
        /// Sẽ được gọi liên tục từ FixedUpdate() của BossController.
        /// Các lớp con bắt buộc phải triển khai (override) hàm này.
        /// </summary>
        public abstract void Move(); 
    }
}