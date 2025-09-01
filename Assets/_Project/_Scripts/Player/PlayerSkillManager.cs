// FILE: _Project/Scripts/Player/PlayerSkillManager.cs

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
// Cần thiết để điều khiển UI

namespace _Project._Scripts.Player
{
    public class PlayerSkillManager : MonoBehaviour
    {
        // Enum để định danh các kỹ năng
        public enum SkillType { BulletClear, Invincibility }
    
        // Lớp nội bộ để lưu trữ thông tin của mỗi kỹ năng
        private class Skill
        {
            public SkillType Type;
            public float Cooldown;
            public float CooldownTimer;
            public KeyCode Key;
            public Image CooldownImage; // UI Image cho hiệu ứng cooldown
            public TMPro.TextMeshProUGUI CooldownText; // UI Text để hiện số giây
        }

        // --- Biến public để thiết lập trong Inspector ---
        [Header("Thiết lập Skill Xóa Đạn")]
        [SerializeField] private float bulletClearCooldown = 15f;
        [SerializeField] private Image bulletClear_CooldownImage;
        [SerializeField] private TMPro.TextMeshProUGUI bulletClear_CooldownText;

        [Header("Thiết lập Skill Bất Tử")]
        [SerializeField] private float invincibilityCooldown = 30f;
        [SerializeField] private Image invincibility_CooldownImage;
        [SerializeField] private TMPro.TextMeshProUGUI invincibility_CooldownText;
    
        // --- Biến nội bộ ---
        private Dictionary<SkillType, Skill> skills = new Dictionary<SkillType, Skill>();

        void Start()
        {
            // Khởi tạo thông tin cho các kỹ năng
            InitializeSkill(SkillType.BulletClear, bulletClearCooldown, KeyCode.Mouse0, bulletClear_CooldownImage, bulletClear_CooldownText);
            InitializeSkill(SkillType.Invincibility, invincibilityCooldown, KeyCode.E, invincibility_CooldownImage, invincibility_CooldownText);
        }

        void Update()
        {
            // Cập nhật timer và UI cho tất cả các kỹ năng
            foreach (var skill in skills.Values)
            {
                if (skill.CooldownTimer > 0)
                {
                    skill.CooldownTimer -= Time.deltaTime;
                    UpdateCooldownUI(skill);
                }
            }
        }
    
        // --- Các hàm Public để các script khác gọi ---

        /// <summary>
        /// Kiểm tra xem một kỹ năng có sẵn sàng để sử dụng không.
        /// </summary>
        public bool IsSkillReady(SkillType type)
        {
            return skills.ContainsKey(type) && skills[type].CooldownTimer <= 0;
        }

        /// <summary>
        /// Kích hoạt thời gian hồi chiêu cho một kỹ năng.
        /// </summary>
        public void TriggerCooldown(SkillType type)
        {
            if (skills.ContainsKey(type))
            {
                skills[type].CooldownTimer = skills[type].Cooldown;
            }
        }

        // --- Các hàm nội bộ ---

        private void InitializeSkill(SkillType type, float cooldown, KeyCode key, Image image, TMPro.TextMeshProUGUI text)
        {
            skills[type] = new Skill
            {
                Type = type,
                Cooldown = cooldown,
                CooldownTimer = 0f,
                Key = key,
                CooldownImage = image,
                CooldownText = text
            };
            // Ban đầu, ẩn hết UI cooldown
            if (image != null) image.fillAmount = 0;
            if (text != null) text.enabled = false;
        }
    
        private void UpdateCooldownUI(Skill skill)
        {
            if (skill.CooldownImage != null)
            {
                skill.CooldownImage.fillAmount = skill.CooldownTimer / skill.Cooldown;
            }
            if (skill.CooldownText != null)
            {
                if (skill.CooldownTimer > 0)
                {
                    skill.CooldownText.enabled = true;
                    skill.CooldownText.text = Mathf.Ceil(skill.CooldownTimer).ToString();
                }
                else
                {
                    skill.CooldownText.enabled = false;
                }
            }
        }
    }
}