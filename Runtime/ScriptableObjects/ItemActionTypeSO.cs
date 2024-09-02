using UnityEngine;

namespace Slax.Inventory
{
    /// <summary>
    /// An item action type that can be used to categorize item actions in the inventory.
    /// </summary>
    [CreateAssetMenu(menuName = "Slax/Inventory/ItemActionType")]
    public class ItemActionTypeSO : ScriptableObject
    {
        [SerializeField] protected string _name;
        public string Name => _name;
    }
}
