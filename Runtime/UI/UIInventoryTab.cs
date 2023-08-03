using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Slax.Inventory
{
    public class UIInventoryTab : MonoBehaviour
    {
        public string Name;
        public string Description;
        public Sprite Sprite;
        public ItemTabType Type;
        public Button Tab;
        public Image Image;

        public void Draw(InventorySO inventory, UIInventoryItemStack prefab)
        {
            Clear();
            List<ItemStack> stacks = inventory.Items.FindAll(o => o.Item.TabTypes.Contains(Type));
            
            foreach (var stack in stacks)
            {
                var s = Instantiate(prefab, new Vector3(), Quaternion.identity);
                s.Init(stack.Item, stack.Amount);
                s.transform.SetParent(transform, false);
            }
        }

        public void DrawAll(InventorySO inventory, UIInventoryItemStack prefab)
        {
            Clear();
            foreach (ItemStack stack in inventory.Items)
            {
                var s = Instantiate(prefab, new Vector3(), Quaternion.identity);
                s.Init(stack.Item, stack.Amount);
                s.transform.SetParent(transform, false);
            }
        }

        public void Clear()
        {
            for (int i = transform.childCount - 1; i >= 0; i--)
            {
                Transform child = transform.GetChild(i);
                Destroy(child.gameObject);
            }
        }
    }
}
