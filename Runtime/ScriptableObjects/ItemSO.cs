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

        [SerializeField] protected string _description;
        public string Description => _description;

        [SerializeField] protected Sprite _previewSprite;
        public Sprite PreviewSprite => _previewSprite;

        [SerializeField] protected List<ItemActionTypeSO> _actionTypes = new List<ItemActionTypeSO>();
        public List<ItemActionTypeSO> ActionTypes => _actionTypes;

        [SerializeField] protected List<ItemTabTypeSO> _tabTypes = new List<ItemTabTypeSO>();
        public List<ItemTabTypeSO> TabTypes => _tabTypes;

        [SerializeField] protected GameObject _prefab;
        public GameObject Prefab => _prefab;

        [Header("Loot settings")]
        [SerializeField] protected int _minDrops = 1;
        [SerializeField] protected int _maxDrops = 10;

        [Header("Extensions")]
        [SerializeField] protected float _weight = 0f;
        public float Weight => _weight;

        public bool HasActionType(ItemActionTypeSO actionType) => _actionTypes.Contains(actionType);
        public bool HasTabType(ItemTabTypeSO tabType) => _tabTypes.Contains(tabType);
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
