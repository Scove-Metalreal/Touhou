using UnityEngine;

public class BackgroundScroller : MonoBehaviour
{
    // --- BIẾN ĐỂ THIẾT LẬP TRONG INSPECTOR ---

    [Header("Thiết lập Cuộn Nền")]
    [Tooltip("Tốc độ trôi của ảnh nền. Giá trị dương sẽ làm nền trôi xuống.")]
    public float scrollSpeed = 2f; // Tốc độ trôi của ảnh nền

    [Tooltip("Kéo 2 tấm ảnh nền của bạn vào đây.")]
    public Transform[] backgrounds; // Mảng để chứa 2 tấm ảnh nền của bạn

    // --- BIẾN NỘI BỘ ---

    // Biến này sẽ lưu trữ chiều cao của một tấm ảnh nền
    private float backgroundHeight;

    // Điểm Y mà khi tâm của ảnh đi qua, nó sẽ được coi là "ngoài màn hình"
    private float offScreenY;

    // Hàm này được gọi một lần ngay khi game bắt đầu
    void Start()
    {
        // Lấy chiều cao của một tấm ảnh nền từ SpriteRenderer của nó.
        // Cách này tự động và chính xác hơn là nhập tay.
        // Chúng ta giả định cả 2 ảnh có cùng kích thước.
        backgroundHeight = backgrounds[0].GetComponent<SpriteRenderer>().bounds.size.y;
        
        // Thiết lập điểm reset. Khi tâm của ảnh đi đến vị trí Y bằng -chiều_cao,
        // có nghĩa là toàn bộ ảnh đã đi ra khỏi màn hình (nếu camera ở 0).
        offScreenY = -backgroundHeight;
    }

    // Hàm Update được gọi mỗi frame
    void Update()
    {
        // Duyệt qua từng tấm ảnh nền trong mảng
        foreach (Transform bg in backgrounds)
        {
            // --- PHẦN 1: DI CHUYỂN ---
            // Di chuyển tấm ảnh nền đi xuống với tốc độ scrollSpeed.
            // Time.deltaTime giúp chuyển động mượt mà và không phụ thuộc vào cấu hình máy.
            bg.Translate(Vector2.down * scrollSpeed * Time.deltaTime);

            // --- PHẦN 2: KIỂM TRA VÀ RESET VỊ TRÍ ---
            // Kiểm tra xem tâm của tấm ảnh nền đã đi qua điểm off-screen chưa
            if (bg.position.y <= offScreenY)
            {
                // Nếu đã đi qua, chúng ta sẽ dịch chuyển nó lên trên.
                // Lượng dịch chuyển chính xác bằng 2 lần chiều cao của ảnh.
                // Ví dụ: Nếu có 2 ảnh cao 10 đơn vị, dịch chuyển lên 20 đơn vị sẽ
                // đặt tấm ảnh dưới cùng lên ngay phía trên tấm ảnh còn lại.
                Vector3 resetPosition = new Vector3(bg.position.x, 
                                                    bg.position.y + backgroundHeight * 2f,
                                                    bg.position.z);
                
                // Gán vị trí mới cho tấm ảnh
                bg.position = resetPosition;
            }
        }
    }
}