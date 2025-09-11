using UnityEngine;

public class ScreenBounds : MonoBehaviour
{
    public static ScreenBounds Instance { get; private set; }

    public float Top { get; private set; }
    public float Bottom { get; private set; }
    public float Left { get; private set; }
    public float Right { get; private set; }
    public Vector2 TopLeft { get; private set; }
    public Vector2 TopRight { get; private set; }
    public Vector2 BottomLeft { get; private set; }
    public Vector2 BottomRight { get; private set; }

    private void Awake()
    {
        // Cài đặt Singleton
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        CalculateBounds();
    }

    private void CalculateBounds()
    {
        Camera mainCamera = Camera.main;
        if (mainCamera == null)
        {
            Debug.LogError("ScreenBounds: Không tìm thấy Camera có tag 'MainCamera'!");
            return;
        }

        // Chuyển đổi từ tọa độ viewport (0,0 -> 1,1) sang tọa độ thế giới
        BottomLeft = mainCamera.ViewportToWorldPoint(new Vector3(0, 0, mainCamera.nearClipPlane));
        TopRight = mainCamera.ViewportToWorldPoint(new Vector3(1, 1, mainCamera.nearClipPlane));
        TopLeft = new Vector2(BottomLeft.x, TopRight.y);
        BottomRight = new Vector2(TopRight.x, BottomLeft.y);

        // Lưu các giá trị cạnh
        Top = TopRight.y;
        Bottom = BottomLeft.y;
        Left = BottomLeft.x;
        Right = TopRight.x;
    }
}

