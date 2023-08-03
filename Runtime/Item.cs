using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Slax.Inventory
{
    public class Item : MonoBehaviour
    {
        [SerializeField] private ItemSO _item;

        public event UnityAction<ItemSO> OnPickUp = delegate { };

        public void PickUp()
        {
            OnPickUp.Invoke(_item);
        }
    }
}