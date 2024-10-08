using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using Slax.InventorySystem.Runtime.Core;

namespace Slax.InventorySystem.Runtime.UI
{
    /// <summary>
    /// This class handles the logic for individual tab buttons. It manages 
    /// visual changes when a tab is active or inactive and triggers any 
    /// related events.
    /// </summary>
    public class UIInventoryTabButton : MonoBehaviour
    {
        [SerializeField] private InventoryTabConfigSO _tabConfig;
        [SerializeField] private Button _button;
        [SerializeField] private Image _backgroundImage;
        [SerializeField] private Color _inactiveColor = Color.gray;
        [SerializeField] private Color _activeColor = Color.white;

        public UnityAction<InventoryTabConfigSO> OnTabSelected;

        private void Start()
        {
            if (_button != null)
            {
                _button.onClick.AddListener(OnButtonClicked);
            }
        }

        private void OnButtonClicked()
        {
            OnTabSelected?.Invoke(_tabConfig);
            SetActive(true);
        }

        public void SetActive(bool isActive)
        {
            if (_backgroundImage != null)
            {
                _backgroundImage.color = isActive ? _activeColor : _inactiveColor;
            }
        }
    }
}
