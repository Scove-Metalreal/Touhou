using System.Collections.Generic;
using UnityEngine;

namespace _Project._Scripts.Player
{
    [CreateAssetMenu(fileName = "PlayerUpgradePath", menuName = "Scriptable Objects/PlayerUpgradePath")]
    public class PlayerUpgradePath : ScriptableObject
    {
        [Tooltip("Kéo tất cả các file ScriptableObject 'UpgradeData' của người chơi vào đây, theo đúng thứ tự từ cấp 1 đến cấp tối đa.")]
        public List<UpgradeData> upgradeLevels;
    }
}

