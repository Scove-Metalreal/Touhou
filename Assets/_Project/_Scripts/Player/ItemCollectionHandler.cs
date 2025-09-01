// FILE: _Project/Scripts/Player/ItemCollectionHandler.cs

using _Project._Scripts.Gameplay.Items;
using UnityEngine;

namespace _Project._Scripts.Player
{
    public class ItemCollectionHandler : MonoBehaviour
    {
        void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Item"))
            {
                 
                Item item = other.GetComponent<Item>();
            
                if (item != null)
                {
                    // Ra lệnh cho vật phẩm bắt đầu bay về phía người chơi
                    item.StartHoming(this.transform.parent); // this.transform.parent là Transform của Player
                }
            }
        }
    }
}