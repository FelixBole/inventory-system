using UnityEngine;
using UnityEngine.Events;

namespace Slax.InventorySystem.Runtime.Core
{
    public class Item : MonoBehaviour
    {
        [SerializeField] protected ItemSO _item;

        public event UnityAction<ItemSO> OnPickUp = delegate { };

        public void PickUp()
        {
            OnPickUp.Invoke(_item);
        }
    }
}