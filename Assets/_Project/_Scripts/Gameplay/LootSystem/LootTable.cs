using UnityEngine;

namespace _Project._Scripts.Gameplay.LootSystem
{
    [System.Serializable]
    public class LootDrop{
        public GameObject itemPrefab;
        [Range(0f, 100f)]
        public float dropChance; // Tỉ lệ rơi (phần trăm)
    }

    [CreateAssetMenu(fileName = "New Loot Table", menuName = "Touhou/Loot Table")]
    public class LootTable : ScriptableObject
    {
        public LootDrop[] drops;

        public GameObject GetRandomDrop()
        {
            float randomValue = Random.Range(0f, 100f);
            float cumulativeChance = 0f;
        
            foreach (var drop in drops)
            {
                cumulativeChance += drop.dropChance;
                if (randomValue <= cumulativeChance)
                {
                    return drop.itemPrefab;
                }
            }
            return null; // Không rơi gì cả
        }
    }
}
