using UnityEngine;

namespace _Project._Scripts.Core
{
    public class DestroyAfterAnimation : MonoBehaviour
    {
        [Tooltip("Thời gian tồn tại của đối tượng này (tính bằng giây). Nên đặt bằng độ dài của animation.")]
        public float lifetime = 1f;

        void Start()
        {
            // Hẹn giờ để tự hủy GameObject này sau một khoảng thời gian 'lifetime'.
            Destroy(gameObject, lifetime);
        }
    }
}