// FILE: _Project/Scripts/Bosses/MovementPatterns/BossMovementPattern.cs

using UnityEngine;

namespace _Project._Scripts.Bosses.MovementPatterns
{
    public abstract class BossMovementPattern : MonoBehaviour
    {
        protected Rigidbody2D rb;
        protected Transform bossTransform;
        protected bool isMoving = false;

        public virtual void Initialize(BossController controller)
        {
            this.rb = controller.GetComponent<Rigidbody2D>();
            if (this.rb == null)
            {
                Debug.LogError("BossMovementPattern yêu cầu một Rigidbody2D trên Boss!", controller.gameObject);
                return;
            }
            this.bossTransform = controller.transform;
        }

        public abstract void PerformMove();

        public void Move()
        {
            if (!isMoving || rb == null) return;
            PerformMove();
        }

        /// <summary>
        /// SỬA ĐỔI: Thêm từ khóa "virtual" để cho phép các lớp con override.
        /// </summary>
        public virtual void StartMoving()
        {
            isMoving = true;
        }

        /// <summary>
        /// SỬA ĐỔI: Thêm từ khóa "virtual" để cho phép các lớp con override.
        /// </summary>
        public virtual void StopMoving()
        {
            isMoving = false;
            if (rb != null)
            {
                rb.linearVelocity = Vector2.zero;
            }
        }
    }
}