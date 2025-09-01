// FILE: _Project/_Scripts/Player/PlayerSkillManager.cs (VERSION 3.0 - INVINCIBILITY ADDED)

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace _Project._Scripts.Player
{
    [RequireComponent(typeof(PlayerState))]
    public class PlayerSkillManager : MonoBehaviour
    {
        public enum SkillType { BulletClear, Invincibility }
    
        private class Skill
        {
            public SkillType Type;
            public float Cooldown;
            public float CooldownTimer;
            public Image CooldownImage;
            public TextMeshProUGUI CooldownText;
        }

        [Header("💣 Thiết lập Bom")]
        [Tooltip("Prefab của đối tượng Bom sẽ được tạo ra.")]
        [SerializeField] private GameObject bombPrefab;

        [Space(10)]
        [Header("✨ Thiết lập Skill Xóa Đạn")]
        [Tooltip("Thời gian hồi chiêu (giây) của kỹ năng Xóa Đạn.")]
        [SerializeField] private float bulletClearCooldown = 15f;
        [Tooltip("UI Image để hiển thị hiệu ứng hồi chiêu của kỹ năng Xóa Đạn.")]
        [SerializeField] private Image bulletClear_CooldownImage;
        [Tooltip("UI Text để hiển thị số giây hồi chiêu còn lại.")]
        [SerializeField] private TextMeshProUGUI bulletClear_CooldownText;

        // --- THÊM MỚI: Thiết lập cho kỹ năng bất tử ---
        [Space(10)]
        [Header("🛡️ Thiết lập Skill Bất Tử")]
        [Tooltip("Thời gian hồi chiêu (giây) của kỹ năng Bất Tử.")]
        [SerializeField] private float invincibilityCooldown = 45f;
        [Tooltip("UI Image để hiển thị hiệu ứng hồi chiêu của kỹ năng Bất Tử.")]
        [SerializeField] private Image invincibility_CooldownImage;
        [Tooltip("UI Text để hiển thị số giây hồi chiêu còn lại.")]
        [SerializeField] private TextMeshProUGUI invincibility_CooldownText;

        private Dictionary<SkillType, Skill> skills = new Dictionary<SkillType, Skill>();
        private PlayerState playerState;

        void Awake()
        {
            playerState = GetComponent<PlayerState>();
        }

        void Start()
        {
            // Khởi tạo thông tin cho các kỹ năng có cooldown
            InitializeSkill(SkillType.BulletClear, bulletClearCooldown, bulletClear_CooldownImage, bulletClear_CooldownText);
            // --- THÊM MỚI: Khởi tạo kỹ năng bất tử ---
            InitializeSkill(SkillType.Invincibility, invincibilityCooldown, invincibility_CooldownImage, invincibility_CooldownText);
        }

        void Update()
        {
            foreach (var skill in skills.Values)
            {
                if (skill.CooldownTimer > 0)
                {
                    skill.CooldownTimer -= Time.deltaTime;
                    UpdateCooldownUI(skill);
                }
            }
        }
    
        public void ActivateBomb()
        {
            if (bombPrefab != null)
            {
                Instantiate(bombPrefab, transform.position, Quaternion.identity);
                Debug.Log("Bom đã được kích hoạt!");
            }
        }

        public bool IsSkillReady(SkillType type)
        {
            return skills.ContainsKey(type) && skills[type].CooldownTimer <= 0;
        }

        public void TriggerCooldown(SkillType type)
        {
            if (skills.ContainsKey(type))
            {
                skills[type].CooldownTimer = skills[type].Cooldown;
            }
        }
        
        private void InitializeSkill(SkillType type, float cooldown, Image image, TextMeshProUGUI text)
        {
            skills[type] = new Skill
            {
                Type = type,
                Cooldown = cooldown,
                CooldownTimer = 0f,
                CooldownImage = image,
                CooldownText = text
            };
            
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
