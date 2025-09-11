using System.Collections;
using System.Collections.Generic;
using _Project._Scripts.Bosses.AttackPatterns;
using UnityEngine;

namespace _Project._Scripts.Bosses
{
    public class BossShooting : MonoBehaviour
    {
        [Header("Scheduler Settings")]
        [Tooltip("Thời gian nghỉ tối thiểu trước khi thử kích hoạt một đợt tấn công (Cyclic) mới.")]
        [SerializeField] private float minCyclicActivationCooldown = 1.0f;
        [Tooltip("Thời gian nghỉ tối đa trước khi thử kích hoạt một đợt tấn công (Cyclic) mới.")]
        [SerializeField] private float maxCyclicActivationCooldown = 2.5f;

        // Danh sách các pattern được phân loại
        private List<AttackPattern> persistentPatterns = new List<AttackPattern>();
        private List<AttackPattern> cyclicPatterns = new List<AttackPattern>();
        
        // Coroutine chính điều phối các đợt tấn công
        private Coroutine schedulerCoroutine;

        public void SetAttackPatterns(List<AttackPattern> allPatterns)
        {
            persistentPatterns.Clear();
            cyclicPatterns.Clear();

            if (allPatterns == null) return;

            foreach (var pattern in allPatterns)
            {
                if (pattern.isPersistent)
                {
                    persistentPatterns.Add(pattern);
                }
                else
                {
                    cyclicPatterns.Add(pattern);
                }
            }
        }

        public void StartShooting()
        {
            // 1. Bắt đầu tất cả các pattern Persistent. Chúng sẽ chạy liên tục.
            foreach (var pattern in persistentPatterns)
            {
                pattern?.StartFiring();
            }

            // 2. Bắt đầu bộ điều phối cho các pattern Cyclic.
            //    Bộ điều phối này sẽ liên tục kích hoạt lại chúng.
            if (cyclicPatterns.Count > 0)
            {
                if (schedulerCoroutine != null) StopCoroutine(schedulerCoroutine);
                schedulerCoroutine = StartCoroutine(CyclicPatternScheduler());
            }
        }

        public void StopShooting()
        {
            // Dừng bộ điều phối
            if (schedulerCoroutine != null)
            {
                StopCoroutine(schedulerCoroutine);
                schedulerCoroutine = null;
            }

            // Dừng tất cả các pattern đang chạy (cả Persistent và Cyclic)
            foreach (var pattern in persistentPatterns)
            {
                pattern?.StopFiring();
            }
            foreach (var pattern in cyclicPatterns)
            {
                pattern?.StopFiring();
            }
        }

        private IEnumerator CyclicPatternScheduler()
        {
            // Vòng lặp này chạy suốt stage để liên tục kích hoạt các pattern Cyclic
            while (true)
            {
                // Chờ một khoảng thời gian ngẫu nhiên
                float cooldown = Random.Range(minCyclicActivationCooldown, maxCyclicActivationCooldown);
                yield return new WaitForSeconds(cooldown);

                // Sau khi chờ, chọn một pattern Cyclic ngẫu nhiên để thực thi
                if (cyclicPatterns.Count > 0)
                {
                    int randomIndex = Random.Range(0, cyclicPatterns.Count);
                    
                    AttackPattern patternToExecute = cyclicPatterns[randomIndex];

                    if (patternToExecute != null)
                    {
                        Debug.Log($"<color=yellow>Scheduler is re-triggering: {patternToExecute.GetType().Name}</color>");
                        
                        // Khởi động pattern. Hàm StartFiring() trong AttackPattern
                        // sẽ tự động xử lý việc dừng coroutine cũ (nếu có) và bắt đầu cái mới.
                        patternToExecute.StartFiring();
                    }
                }
            }
        }
    }
}