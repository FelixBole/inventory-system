using UnityEngine;

namespace Slax.Inventory
{
    /// <summary>
    ///  A tab type that can be used to categorize items in the inventory.
    /// </summary>
    [CreateAssetMenu(menuName = "Slax/Inventory/ItemTabType")]
    public class ItemTabTypeSO : ScriptableObject
    {
        [SerializeField] protected string _name = "New Tab";
        public string Name => _name;
        [SerializeField] protected string _description = "Description";
        public string Description => _description;

        [SerializeField] protected Sprite _icon;
        public Sprite Icon => _icon;

        [SerializeField] protected Color _color = Color.white;
        public Color Color => _color;

        [SerializeField] protected Color _backgroundColor = Color.black;
        public Color BackgroundColor => _backgroundColor;
    }
}
