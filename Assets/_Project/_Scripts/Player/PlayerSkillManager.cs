using System.Collections.Generic;
using _Project._Scripts.UI; // Thêm để gọi UIManager
using UnityEngine;
using UnityEngine.UI; // Thêm để sử dụng Image
using TMPro; // Thêm để sử dụng TextMeshProUGUI

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
        [SerializeField] private float bulletClearCooldown = 15f;

        [Space(10)]
        [Header("🛡️ Thiết lập Skill Bất Tử")]
        [SerializeField] private float invincibilityCooldown = 45f;
        
        private Dictionary<SkillType, Skill> skills = new Dictionary<SkillType, Skill>();
        private PlayerState playerState;

        void Awake()
        {
            playerState = GetComponent<PlayerState>();
        }

        void Start()
        {
            InitializeUI();
            
            // // Lấy các tham chiếu UI từ UIManager và khởi tạo skill
            // if (UIManager.Instance != null)
            // {
            //     InitializeSkill(SkillType.BulletClear, bulletClearCooldown, 
            //         UIManager.Instance.GetSkillCooldownImage(SkillType.BulletClear), 
            //         UIManager.Instance.GetSkillCooldownText(SkillType.BulletClear));
            //         
            //     InitializeSkill(SkillType.Invincibility, invincibilityCooldown,
            //         UIManager.Instance.GetSkillCooldownImage(SkillType.Invincibility),
            //         UIManager.Instance.GetSkillCooldownText(SkillType.Invincibility));
            // }
            // else
            // {
            //     Debug.LogError("Không tìm thấy UIManager.Instance! UI kỹ năng sẽ không hoạt động.", this);
            // }
        }

        /// <summary>
        /// Được GameManager gọi mỗi khi vào một scene mới để lấy lại các tham chiếu UI
        /// từ UIManager của scene đó và cập nhật trạng thái cooldown.
        /// </summary>
        public void InitializeUI()
        {
            if (UIManager.Instance != null)
            {
                Debug.Log("[PlayerSkillManager] Initializing skill UI references from new UIManager.");
                InitializeSkill(SkillType.BulletClear, bulletClearCooldown, 
                    UIManager.Instance.GetSkillCooldownImage(SkillType.BulletClear), 
                    UIManager.Instance.GetSkillCooldownText(SkillType.BulletClear));
                    
                InitializeSkill(SkillType.Invincibility, invincibilityCooldown,
                    UIManager.Instance.GetSkillCooldownImage(SkillType.Invincibility),
                    UIManager.Instance.GetSkillCooldownText(SkillType.Invincibility));
            }
            else
            {
                Debug.LogError("Cannot initialize skill UI because UIManager.Instance is null!", this);
            }
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
        
        // Giữ nguyên hàm InitializeSkill theo yêu cầu của bạn
        private void InitializeSkill(SkillType type, float cooldown, Image image, TextMeshProUGUI text)
        {
            // Kiểm tra null để tránh lỗi nếu UI không được gán trong UIManager
            if (image == null || text == null)
            {
                Debug.LogWarning($"UI cho kỹ năng {type} chưa được gán trong UIManager.", this);
            }
            
            // Nếu skill chưa tồn tại, tạo mới
            if (!skills.ContainsKey(type))
            {
                skills[type] = new Skill
                {
                    Type = type,
                    Cooldown = cooldown,
                    CooldownTimer = 0f, // Giả sử bắt đầu với skill sẵn sàng
                };
            }
            
            // Cập nhật lại các tham chiếu UI
            skills[type].CooldownImage = image;
            skills[type].CooldownText = text;
            
            // Cập nhật lại trạng thái UI hiện tại
            UpdateCooldownUI(skills[type]);
        }
    
        // Giữ lại hàm UpdateCooldownUI vì PlayerSkillManager giờ đã tự quản lý UI
        private void UpdateCooldownUI(Skill skill)
        {
            if (skill.CooldownTimer < 0) skill.CooldownTimer = 0;

            if (skill.CooldownImage != null)
            {
                // Đảm bảo không chia cho 0
                skill.CooldownImage.fillAmount = skill.Cooldown > 0 ? (skill.CooldownTimer / skill.Cooldown) : 0;
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