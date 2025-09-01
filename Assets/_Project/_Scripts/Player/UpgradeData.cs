using UnityEngine;

namespace _Project._Scripts.Player
{
    [CreateAssetMenu(fileName = "UpgradeLevel_", menuName = "Touhou/Player Upgrade Level")]
    public class UpgradeData : ScriptableObject
    {
        [Header("Shooting Pattern")]
        public int straightShots = 0;
        public int diagonalShots = 0;
        public int homingShots = 0;
        public int cannonballShots = 0;

        [Header("Passive Abilities")]
        public bool hasDash = false;
        public float moveSpeedMultiplier = 1.0f;

        [Header("Active Skills")]
        public bool hasBulletClearSkill = false;
        public bool hasInvincibilitySkill = false;
    }
}
