using System.Collections;
using UnityEngine;

namespace _Project._Scripts.Bosses.AttackPatterns
{
    public abstract class AttackPattern : MonoBehaviour
    {
        [Header("Pattern Behavior")]
        [Tooltip("Đánh dấu nếu pattern này nên chạy liên tục trong suốt stage (ví dụ: các vòng đạn cơ bản). Bỏ đánh dấu nếu nó chỉ chạy trong một khoảng thời gian rồi kết thúc (ví dụ: một đợt mưa đạn).")]
        public bool isPersistent = true;

        protected BossController bossController;
        protected Coroutine firingCoroutine;

        public virtual void Initialize(BossController controller)
        {
            this.bossController = controller;
        }

        public virtual void StartFiring()
        {
            if (firingCoroutine != null) StopCoroutine(firingCoroutine);
            firingCoroutine = StartCoroutine(Execute());
        }

        public virtual void StopFiring()
        {
            if (firingCoroutine != null)
            {
                StopCoroutine(firingCoroutine);
                firingCoroutine = null;
            }
        }

        public abstract IEnumerator Execute();
    }
}