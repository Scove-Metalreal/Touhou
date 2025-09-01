using UnityEngine;

namespace _Project._Scripts.Bosses.MovementPatterns
{
    public abstract class BossMovementPattern : MonoBehaviour
    {
        protected Rigidbody2D rb;
        protected Transform bossTransform;

        /// <summary>
        /// Hàm này được gọi một lần duy nhất khi pattern được kích hoạt.
        /// Dùng để thiết lập các tham chiếu cần thiết.
        /// </summary>
        public virtual void Initialize(Rigidbody2D bossRigidbody2D)
        {
            this.rb = bossRigidbody2D;
            this.bossTransform = bossRigidbody2D.transform;
        }
        
        /// <summary>
        /// Hàm này được gọi trong FixedUpdate của BossController.
        /// Nơi để thực hiện logic di chuyển dựa trên vật lý.
        /// </summary>
        public abstract void Move();
    }
}
