using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Slax.Inventory
{

    public class UIInventoryTabManager : MonoBehaviour
    {
        [SerializeField] protected List<UIInventoryTab> _tabs = new List<UIInventoryTab>();

        [Header("Settings")]
        [SerializeField] protected Color _inactive;
        [SerializeField] protected Color _active;

        void OnEnable()
        {
            if (_tabs.Count == 0) return;

            foreach (UIInventoryTab t in _tabs)
            {
                t.Tab.image.color = _inactive;
                t.gameObject.SetActive(false);
            }

            _tabs[0].gameObject.SetActive(true);
            _tabs[0].Tab.image.color = _active;
        }

        public void DrawTab()
        {
            
        }
    }

}