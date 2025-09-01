using System.Collections;
using UnityEngine;

namespace _Project._Scripts.Bosses.AttackPatterns
{
    public abstract class AttackPattern : MonoBehaviour
    {
        protected BossController bossController;
        protected Coroutine firingCoroutine; // Biến để lưu trữ coroutine đang chạy

        public virtual void Initialize(BossController controller)
        {
            this.bossController = controller;
        }

        /// <summary>
        /// Bắt đầu thực thi logic tấn công.
        /// </summary>
        public virtual void StartFiring()
        {
            // Dừng coroutine cũ nếu có và bắt đầu một cái mới
            if (firingCoroutine != null)
            {
                StopCoroutine(firingCoroutine);
            }
            firingCoroutine = StartCoroutine(Execute());
        }

        /// <summary>
        /// Dừng logic tấn công đang chạy.
        /// </summary>
        public virtual void StopFiring()
        {
            if (firingCoroutine != null)
            {
                StopCoroutine(firingCoroutine);
                firingCoroutine = null;
            }
        }

        /// <summary>
        /// Coroutine chứa logic tấn công chính.
        /// </summary>
        public abstract IEnumerator Execute();
    }
}