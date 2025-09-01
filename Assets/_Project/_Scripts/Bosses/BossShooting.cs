using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using _Project._Scripts.Bosses.AttackPatterns;

namespace _Project._Scripts.Bosses
{
    public class BossShooting : MonoBehaviour
    {
        private List<AttackPattern> currentAttackPatterns;
        private int currentPatternIndex = 0;
        private Coroutine shootingCoroutine;

        public void SetAttackPatterns(List<AttackPattern> newPatterns)
        {
            currentAttackPatterns = newPatterns;
            currentPatternIndex = 0;
        }

        public void StartShooting()
        {
            StopShooting(); 

            if (currentAttackPatterns != null && currentAttackPatterns.Count > 0)
            {
                shootingCoroutine = StartCoroutine(ShootingSequence());
            }
            else
            {
                Debug.LogWarning("Không có Attack Pattern nào được gán cho BossShooting.", this);
            }
        }

        public void StopShooting()
        {
            if (shootingCoroutine != null)
            {
                StopCoroutine(shootingCoroutine);
                shootingCoroutine = null;
            }
            StopAllCoroutines();
        }

        private IEnumerator ShootingSequence()
        {
            while (true) 
            {
                if (currentAttackPatterns == null || currentAttackPatterns.Count == 0)
                {
                    yield return null;
                    continue;
                }

                AttackPattern currentPattern = currentAttackPatterns[currentPatternIndex];
                
                // Khởi tạo pattern với tham chiếu cần thiết (nếu cần)
                // currentPattern.Initialize(GetComponent<BossController>());

                // SỬA ĐỔI: Gọi đúng tên hàm Execute()
                // Dòng này sẽ đợi cho đến khi coroutine của IcicleFall (đã có điểm dừng) thực thi xong.
                yield return StartCoroutine(currentPattern.Execute());

                currentPatternIndex++;

                if (currentPatternIndex >= currentAttackPatterns.Count)
                {
                    currentPatternIndex = 0;
                }
            }
        }
    }
}