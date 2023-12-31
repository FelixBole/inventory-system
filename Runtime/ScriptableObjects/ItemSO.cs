using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Slax.Inventory
{
    public enum ItemActionType
    {
        NONE, // Key kept items
        SEED, // Can be planted
        SELL, // Can be sold
        GIVE, // Can be given
        QUEST, // Used in a quest
        CRAFT, // Can be used in a craft
        EQUIP, // Can be equiped
        DROP, // Can be dropped
        CONSUME, // Can be consumed
    }

    public enum ItemTabType
    {
        MIXED,
        QUEST,
        CONSUMABLE,
        RESOURCE,
        EQUIPMENT,
    }

    [CreateAssetMenu(menuName = "Slax/Inventory/Item")]
    public class ItemSO : ScriptableObject
    {
        [SerializeField] private string _id;
        public string ID => _id;

        [SerializeField] private string _name;
        public string Name => _name;

        [SerializeField] private string _description;
        public string Description => _description;

        [SerializeField] private Sprite _previewSprite;
        public Sprite PreviewSprite => _previewSprite;

        [SerializeField] private List<ItemActionType> _actionTypes = new List<ItemActionType>();
        public List<ItemActionType> ActionTypes => _actionTypes;
        [SerializeField] private List<ItemTabType> _tabTypes = new List<ItemTabType>();
        public List<ItemTabType> TabTypes => _tabTypes;

        [SerializeField] private GameObject _prefab;
        public GameObject Prefab => _prefab;

        [SerializeField] private int _price;
        public int Price => _price;

        [Header("Loot settings")]
        [SerializeField] private int _minDrops = 1;
        [SerializeField] private int _maxDrops = 10;

        public bool HasActionType(ItemActionType actionType) => _actionTypes.Contains(actionType);
        public bool HasTabType(ItemTabType tabType) => _tabTypes.Contains(tabType);
        public int GetRandomDrops() => Random.Range(_minDrops, _maxDrops);
        public int GetRandomDrops(float multiplier) => Random.Range(_minDrops, Mathf.FloorToInt(_maxDrops * multiplier));
        public int GetRandomDrops(int overrideMinDrops) => Random.Range(overrideMinDrops, _maxDrops);
        public int GetRandomDrops(int overrideMinDrops, float multiplier) => Random.Range(overrideMinDrops, Mathf.FloorToInt(_maxDrops * multiplier));
        public int GetRandomDrops(int overrideMinDrops, int overrideMaxDrops) => Random.Range(overrideMinDrops, overrideMaxDrops);


        // Serialization
        [SerializeField, HideInInspector] private string _guid;
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