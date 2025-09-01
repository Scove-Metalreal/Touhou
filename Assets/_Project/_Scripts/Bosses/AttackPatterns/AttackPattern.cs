using System.Collections;
using UnityEngine;

namespace _Project._Scripts.Bosses.AttackPatterns
{
    public abstract class AttackPattern : MonoBehaviour
    {
        protected BossController bossController;
        protected Transform bossTransform;

        public virtual void Initialize(BossController controller)
        {
            this.bossController = controller;
            this.bossTransform = controller.transform;
        }

        /// <summary>
        /// Coroutine chứa logic tấn công chính. 
        /// Quan trọng: Coroutine này PHẢI có điểm kết thúc.
        /// </summary>
        public abstract IEnumerator Execute();
    }
}