// FILE: _Project/_Scripts/UI/FocusTrigger.cs (SCRIPT MỚI)

using UnityEngine;
using UnityEngine.EventSystems;

namespace _Project._Scripts.UI
{
    /// <summary>
    /// Gắn script này vào một GameObject UI vô hình (FocusArea) để bắt sự kiện nhấn giữ.
    /// Nó sẽ báo cho InputManager biết khi nào người chơi muốn vào/thoát chế độ Focus.
    /// </summary>
    public class FocusTrigger : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        // Báo cho InputManager biết khi ngón tay nhấn xuống vùng này
        public void OnPointerDown(PointerEventData eventData)
        {
            if (InputManager.Instance != null)
            {
                InputManager.Instance.SetFocus(true);
            }
        }

        // Báo cho InputManager biết khi ngón tay nhấc lên
        public void OnPointerUp(PointerEventData eventData)
        {
            if (InputManager.Instance != null)
            {
                InputManager.Instance.SetFocus(false);
            }
        }
    }
}