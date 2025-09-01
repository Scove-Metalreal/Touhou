using System.Collections;
using UnityEngine;

// Đây là một lớp trừu tượng, không thể kéo thả trực tiếp vào GameObject
namespace _Project._Scripts.Bosses.AttackPatterns
{
    public abstract class AttackPattern : MonoBehaviour
    {
        // Biến này sẽ được BossController gọi để bắt đầu tấn công
        public abstract IEnumerator Execute();
    }
}