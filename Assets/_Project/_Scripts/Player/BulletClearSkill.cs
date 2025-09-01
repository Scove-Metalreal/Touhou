// FILE: _Project/_Scripts/Player/BulletClearSkill.cs (VERSION 2.0 - UPGRADE SYSTEM COMPATIBLE)

using UnityEngine;

namespace _Project._Scripts.Player
{
    /// <summary>
    /// Quản lý kỹ năng "Xóa Đạn" của người chơi.
    /// Cho phép người chơi vẽ một vùng hình chữ nhật để xóa tất cả đạn địch bên trong.
    /// </summary>
    [RequireComponent(typeof(PlayerState), typeof(PlayerSkillManager))]
    public class BulletClearSkill : MonoBehaviour
    {
        [Header("Thiết lập Skill")]
        [Tooltip("Kích thước tối đa (pixel) của vùng chọn. Vector2(width, height).")]
        [SerializeField] private Vector2 maxSelectionSize = new Vector2(400, 300);

        [Header("Tham chiếu UI")]
        [Tooltip("Kéo Panel UI 'SelectionBox_Visual' từ Canvas vào đây.")]
        public RectTransform selectionBox; 

        // --- Biến nội bộ ---
        private PlayerState playerState;
        private PlayerSkillManager skillManager;
        private Vector2 startMousePosition;
        private bool isSelecting = false;

        void Awake() 
        {
            playerState = GetComponent<PlayerState>();
            skillManager = GetComponent<PlayerSkillManager>();
        }

        void Start()
        {
            if (selectionBox != null)
            {
                selectionBox.gameObject.SetActive(false);
            }
        }

        void Update()
        {
            // CẬP NHẬT: Điều kiện để dùng skill được kiểm tra thông qua hàm mới trong PlayerState
            if (!playerState.HasBulletClearSkill())
                return;
        
            HandleSelectionInput();
        }

        /// <summary>
        /// Xử lý toàn bộ logic input liên quan đến việc vẽ và kích hoạt vùng chọn.
        /// </summary>
        private void HandleSelectionInput()
        {
            // 1. Nhấn chuột phải xuống để bắt đầu chọn (chỉ khi skill sẵn sàng)
            if (Input.GetMouseButtonDown(1) && skillManager.IsSkillReady(PlayerSkillManager.SkillType.BulletClear))
            {
                isSelecting = true;
                startMousePosition = Input.mousePosition;
                if (selectionBox != null)
                {
                    selectionBox.gameObject.SetActive(true);
                }
            }

            // 2. Đang giữ chuột để vẽ vùng chọn
            if (isSelecting && Input.GetMouseButton(1))
            {
                UpdateSelectionBoxVisual();
            }

            // 3. Thả chuột ra để kích hoạt skill
            if (isSelecting && Input.GetMouseButtonUp(1))
            {
                isSelecting = false;
                if (selectionBox != null)
                {
                    selectionBox.gameObject.SetActive(false);
                }
            
                // Kích hoạt skill và bắt đầu hồi chiêu
                ClearBulletsInSelectionArea();
                skillManager.TriggerCooldown(PlayerSkillManager.SkillType.BulletClear);
            }
        }

        /// <summary>
        /// Cập nhật vị trí và kích thước của UI vùng chọn dựa trên vị trí chuột.
        /// </summary>
        private void UpdateSelectionBoxVisual()
        {
            if (selectionBox == null) return;
            
            Vector2 currentMousePosition = Input.mousePosition;
                
            // Tính toán kích thước và giới hạn nó
            Vector2 boxSize = new Vector2(
                Mathf.Abs(startMousePosition.x - currentMousePosition.x),
                Mathf.Abs(startMousePosition.y - currentMousePosition.y)
            );
            boxSize.x = Mathf.Min(boxSize.x, maxSelectionSize.x);
            boxSize.y = Mathf.Min(boxSize.y, maxSelectionSize.y);
                
            // Điều chỉnh lại vị trí để hộp không vượt quá giới hạn khi kéo lùi
            Vector2 endPosLimited = startMousePosition + new Vector2(
                Mathf.Sign(currentMousePosition.x - startMousePosition.x) * boxSize.x,
                Mathf.Sign(currentMousePosition.y - startMousePosition.y) * boxSize.y);

            // Cập nhật vị trí và sizeDelta của RectTransform
            Vector2 boxStart = new Vector2(
                Mathf.Min(startMousePosition.x, endPosLimited.x),
                Mathf.Min(startMousePosition.y, endPosLimited.y)
            );
                
            selectionBox.position = boxStart;
            selectionBox.sizeDelta = boxSize;
        }
        
        /// <summary>
        /// Tìm và xóa tất cả đạn địch nằm trong vùng chọn của UI.
        /// </summary>
        private void ClearBulletsInSelectionArea()
        {
            if (selectionBox == null) return;
            
            // Lấy vị trí góc của vùng chọn trên màn hình
            Vector2 screenStartPos = selectionBox.position;
            Vector2 screenEndPos = screenStartPos + selectionBox.sizeDelta;
            
            // Chuyển đổi vị trí từ Screen Space sang World Space
            Vector2 worldStartPos = Camera.main.ScreenToWorldPoint(screenStartPos);
            Vector2 worldEndPos = Camera.main.ScreenToWorldPoint(screenEndPos);
            
            // Tìm tất cả các collider trong vùng hình chữ nhật đó
            Collider2D[] colliders = Physics2D.OverlapAreaAll(worldStartPos, worldEndPos);
            
            int bulletsCleared = 0;
            foreach(var col in colliders)
            {
                // Nếu collider thuộc về một viên đạn địch, vô hiệu hóa nó
                if(col.CompareTag("EnemyBullet"))
                {
                    col.gameObject.SetActive(false);
                    bulletsCleared++;
                }
            }

            if(bulletsCleared > 0) 
            {
                Debug.Log($"Đã xóa {bulletsCleared} viên đạn địch!");
            }
        }
    }
}
