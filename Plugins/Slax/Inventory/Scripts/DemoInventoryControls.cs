using UnityEngine;
using Slax.InventorySystem.Runtime.Core;

namespace Slax.InventorySystem.Runtime.Demo
{
    public class DemoInventoryControls : MonoBehaviour
    {
        RuntimeInventory _inventory;
        [SerializeField] ItemSO[] _items = new ItemSO[4];

        public void Init(RuntimeInventory inventory)
        {
            _inventory = inventory;
        }

        public void AddFirstItem()
        {
            AddItem(0);
        }

        public void AddSecondItem()
        {
            AddItem(1);
        }

        public void AddThirdItem()
        {
            AddItem(2);
        }

        public void AddFourthItem()
        {
            AddItem(3);
        }

        public void RemoveFirstItem()
        {
            RemoveItem(0);
        }

        public void RemoveSecondItem()
        {
            RemoveItem(1);
        }

        public void RemoveThirdItem()
        {
            RemoveItem(2);
        }

        public void RemoveFourthItem()
        {
            RemoveItem(3);
        }

        void AddItem(int index)
        {
            if (_inventory == null) return;
            if (_items[index] == null) return;
            _inventory.AddItem(_items[index]);
        }

        void RemoveItem(int index)
        {
            if (_inventory == null) return;
            if (_items[index] == null) return;
            _inventory.RemoveItem(_items[index]);
        }
    }
}