using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Slax.Inventory
{
    [CreateAssetMenu(menuName = "Slax/Inventory/Item")]
    public class ItemSO : ScriptableObject
    {
        [SerializeField] protected string _id;
        public string ID => _id;

        [SerializeField] protected string _name;
        public string Name => _name;

        [SerializeField, TextArea] protected string _description;
        public string Description => _description;

        [SerializeField] protected Sprite _previewSprite;
        public Sprite PreviewSprite => _previewSprite;

        [SerializeField] protected List<ItemActionTypeSO> _actionTypes = new List<ItemActionTypeSO>();
        public List<ItemActionTypeSO> ActionTypes => _actionTypes;

        [SerializeField] protected List<InventoryTabConfigSO> _tabConfigs = new List<InventoryTabConfigSO>();
        public List<InventoryTabConfigSO> TabConfigs => _tabConfigs;

        [SerializeField] protected GameObject _prefab;
        public GameObject Prefab => _prefab;

        [SerializeField, Tooltip("If set to true, allows items of this kind to be stacked on top of each other in a single slot.")] protected bool _isStackable = true;
        public bool IsStackable => _isStackable;

        [SerializeField, Tooltip("The maximum number of items that can be stacked together for this item. Setting to a value below 0 makes it have no limitations.")] protected int _stackLimit = -1;
        public int StackLimit => _stackLimit;

        [SerializeField, Tooltip("If toggled, it will prevent the item stack to go over 1 and will override the inventory's UseSameItemInMultipleSlots flag and will prevent the item to be added to a new slot.")] protected bool _isUnique = false;
        public bool IsUnique => _isUnique;

        [SerializeField] protected int _minDrops = 1;
        [SerializeField] protected int _maxDrops = 10;

        [SerializeField] protected float _weight = 0f;
        public float Weight => _weight;

        public bool HasActionType(ItemActionTypeSO actionType) => _actionTypes.Contains(actionType);
        public bool HasTabConfig(InventoryTabConfigSO tabConfig) => _tabConfigs.Contains(tabConfig);
        public int GetRandomDrops() => Random.Range(_minDrops, _maxDrops);
        public int GetRandomDrops(float multiplier) => Random.Range(_minDrops, Mathf.FloorToInt(_maxDrops * multiplier));
        public int GetRandomDrops(int overrideMinDrops) => Random.Range(overrideMinDrops, _maxDrops);
        public int GetRandomDrops(int overrideMinDrops, float multiplier) => Random.Range(overrideMinDrops, Mathf.FloorToInt(_maxDrops * multiplier));
        public int GetRandomDrops(int overrideMinDrops, int overrideMaxDrops) => Random.Range(overrideMinDrops, overrideMaxDrops);

        // Serialization
        [SerializeField, HideInInspector] protected string _guid;
        public string Guid => _guid;

#if UNITY_EDITOR
        void OnValidate()
        {
            var path = AssetDatabase.GetAssetPath(this);
            _guid = AssetDatabase.AssetPathToGUID(path);
        }
#endif
    }
}
