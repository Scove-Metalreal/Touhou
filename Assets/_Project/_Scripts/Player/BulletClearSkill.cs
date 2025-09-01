// FILE: _Project/Scripts/Player/BulletClearSkill.cs

using UnityEngine;

namespace _Project._Scripts.Player
{
    public class BulletClearSkill : MonoBehaviour
    {
        [Header("Thiết lập Skill")]
        [Tooltip("Kích thước tối đa (pixel) của vùng chọn. Vector2(width, height).")]
        [SerializeField] private Vector2 maxSelectionSize = new Vector2(400, 300);

        [Header("Tham chiếu")]
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
            // Điều kiện để dùng skill: đã mở khóa VÀ skill đã hồi chiêu xong
            if (playerState.CurrentUpgrade == null || !playerState.CurrentUpgrade.hasBulletClearSkill)
                return;
        
            HandleSelection();
        }

        private void HandleSelection()
        {
            // 1. Nhấn chuột xuống để bắt đầu chọn
            if (Input.GetMouseButtonDown(1) && skillManager.IsSkillReady(PlayerSkillManager.SkillType.BulletClear))
            {
                isSelecting = true;
                startMousePosition = Input.mousePosition;
                if (selectionBox != null)
                {
                    selectionBox.gameObject.SetActive(true);
                }
            }

            // 2. Đang giữ chuột để vẽ vùng
            if (isSelecting && Input.GetMouseButton(1))
            {
                if (selectionBox != null)
                {
                    Vector2 currentMousePosition = Input.mousePosition;
                
                    // Tính toán kích thước
                    Vector2 boxSize = new Vector2(
                        Mathf.Abs(startMousePosition.x - currentMousePosition.x),
                        Mathf.Abs(startMousePosition.y - currentMousePosition.y)
                    );

                    // --- GIỚI HẠN KÍCH THƯỚC ---
                    boxSize.x = Mathf.Min(boxSize.x, maxSelectionSize.x);
                    boxSize.y = Mathf.Min(boxSize.y, maxSelectionSize.y);
                
                    // Điều chỉnh lại vị trí để hộp không vượt quá giới hạn khi kéo lùi
                    Vector2 endPosLimited = startMousePosition + new Vector2(
                        Mathf.Sign(currentMousePosition.x - startMousePosition.x) * boxSize.x,
                        Mathf.Sign(currentMousePosition.y - startMousePosition.y) * boxSize.y);

                    // Cập nhật UI
                    Vector2 boxStart = new Vector2(
                        Mathf.Min(startMousePosition.x, endPosLimited.x),
                        Mathf.Min(startMousePosition.y, endPosLimited.y)
                    );
                
                    selectionBox.position = boxStart;
                    selectionBox.sizeDelta = boxSize;
                }
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
                ClearBulletsInArea(selectionBox.position, (Vector2)selectionBox.position + selectionBox.sizeDelta);
                skillManager.TriggerCooldown(PlayerSkillManager.SkillType.BulletClear);
            }
        }

        void ClearBulletsInArea(Vector2 screenStartPos, Vector2 screenEndPos)
        {
            // ... (Logic xóa đạn giữ nguyên như cũ)
            Vector2 worldStartPos = Camera.main.ScreenToWorldPoint(screenStartPos);
            Vector2 worldEndPos = Camera.main.ScreenToWorldPoint(screenEndPos);
            Collider2D[] colliders = Physics2D.OverlapAreaAll(worldStartPos, worldEndPos);
            int bulletsCleared = 0;
            foreach(var col in colliders)
            {
                if(col.CompareTag("EnemyBullet"))
                {
                    col.gameObject.SetActive(false);
                    bulletsCleared++;
                }
            }
            if(bulletsCleared > 0) Debug.Log($"Cleared {bulletsCleared} enemy bullets!");
        }
    }
}