// FILE: _Project/_Scripts/Gameplay/Projectiles/BouncingBullet.cs

using UnityEngine;

namespace _Project._Scripts.Gameplay.Projectiles
{
    // Kế thừa từ lớp Bullet
    public class BouncingBullet : Bullet
    {
        [Header("Bouncing Bullet Settings")]
        [Tooltip("Số lần đạn có thể nảy bật trước khi biến mất.")]
        [SerializeField] private int maxBounces = 3;
        private int currentBounces;

        // Override OnEnable để reset trạng thái nảy bật và thay đổi setup Rigidbody/Collider
        protected new void OnEnable() // Dùng 'new' để ẩn OnEnable của lớp cha
        {
            currentBounces = maxBounces;
            // Đảm bảo behavior được đặt là Straight để di chuyển ban đầu
            // (hoặc bạn có thể override toàn bộ logic di chuyển nếu muốn)
            // base.behavior = BulletBehavior.Straight; // Không cần thiết nếu bạn đã cấu hình trong Inspector
            
            // Kích hoạt lại Rigidbody và Collider
            Rigidbody2D rb = GetComponent<Rigidbody2D>();
            Collider2D col = GetComponent<Collider2D>();

            if (rb != null)
            {
                rb.gravityScale = 0;
                rb.isKinematic = false; // Phải là non-kinematic để va chạm vật lý
                rb.constraints = RigidbodyConstraints2D.FreezeRotation; // Đạn không nên tự xoay
                // Đặt vận tốc ban đầu (transform.up là hướng bay của bullet)
                rb.linearVelocity = transform.up * GetComponent<Bullet>().Damage; // Sử dụng speed từ Bullet.cs
            }
            if (col != null)
            {
                col.isTrigger = false; // PHẢI LÀ FALSE để kích hoạt OnCollisionEnter2D
                // Đảm bảo có vật liệu vật lý để kiểm soát độ nảy
                if (col.sharedMaterial == null)
                {
                    Debug.LogWarning("BouncingBullet: Collider on " + gameObject.name + " has no Physics Material 2D. Bouncing might not work as expected.", this);
                }
            }
            
            // Gọi OnEnable của lớp cha (Bullet) để xử lý lifetime, v.v.
            // Lưu ý: Bullet.OnEnable sẽ đặt lại linearVelocity, vì vậy chúng ta sẽ đặt lại sau.
            base.OnEnable(); 
            // Sau khi base.OnEnable đặt velocity, chúng ta cần ghi đè lại nếu muốn
            if (rb != null)
            {
                 rb.linearVelocity = transform.up * GetComponent<Bullet>().Speed; // Sửa lại thành GetComponent<Bullet>().speed
            }
        }

        // Override OnTriggerEnter2D của lớp cha để chỉ xử lý va chạm với người chơi/boss
        protected new void OnTriggerEnter2D(Collider2D other) // Dùng 'new' để ẩn OnTriggerEnter2D của lớp cha
        {
            // Logic va chạm với Player/Boss vẫn như cũ, không thay đổi
            base.OnTriggerEnter2D(other);
        }

        // Xử lý va chạm vật lý với môi trường
        void OnCollisionEnter2D(Collision2D collision)
        {
            // Kiểm tra xem va chạm có phải với Player/Boss không
            // Nếu là Player/Boss, nó sẽ được xử lý bởi OnTriggerEnter2D (nhờ Collider2D.isTrigger = true trên Player/Boss)
            // và đạn sẽ biến mất. Logic này chỉ xử lý va chạm với môi trường (tường).
            
            // Nếu va chạm với bất kỳ thứ gì không phải Player hoặc Boss (tức là môi trường)
            if (!collision.gameObject.CompareTag("Player") && !collision.gameObject.CompareTag("Boss"))
            {
                currentBounces--;
                if (currentBounces <= 0)
                {
                    // Hết số lần nảy, vô hiệu hóa đạn
                    gameObject.SetActive(false);
                }
                else
                {
                    // Phát âm thanh nảy (nếu có)
                    // AudioManager.Instance?.PlaySFX("BulletBounce");
                }
            }
        }
        
        // Override CheckIfOffScreen để ngăn đạn bị vô hiệu hóa khi bay ra khỏi màn hình
        // Bouncing bullet không nên bị tắt khi ra khỏi màn hình, nó sẽ va chạm và nảy.
        // protected new void CheckIfOffScreen() { /* Do nothing */ }
        // Tốt hơn là chúng ta không nên gọi hàm này trong Update của BouncingBullet.
        // Vì nó là một phương thức private của Bullet, chúng ta không thể override nó trực tiếp.
        // Thay vào đó, chúng ta sẽ không cho nó chạy logic đó trong Update của Bullet bằng cách 
        // bỏ behavior Bouncing khỏi CheckIfOffScreen trong Bullet.cs, nhưng đó sẽ làm phức tạp Bullet.cs.
        // Cách tốt nhất là thiết kế lại một chút: Bullet.cs sẽ có một cờ 'disableOnScreenExit'.
        // HOẶC, chúng ta chấp nhận việc BouncingBullet vẫn gọi base.Update(), 
        // nhưng các bức tường của bạn sẽ cần phải đủ dày hoặc ở ngoài tầm nhìn camera để đạn không bị tắt quá sớm.
        // Trong trường hợp này, chúng ta sẽ tạm thời bỏ qua việc override CheckIfOffScreen
        // và giả định các bức tường của bạn sẽ ngăn đạn ra khỏi màn hình.

        // Tuy nhiên, nếu bạn muốn hoàn toàn vô hiệu hóa check off-screen cho đạn nảy,
        // bạn có thể thay đổi Bullet.cs một chút để cho phép các lớp con kiểm soát.
        // Hiện tại, chúng ta sẽ để Bullet.cs tự động tắt nếu nó ra ngoài, 
        // điều này có nghĩa là bạn cần đảm bảo các collider "tường" của bạn
        // bao phủ toàn bộ vùng chơi.
    }
}