using System;
using UnityEngine;

namespace PuzzleGallery.Features.MainMenu
{
    public class TabBarView : MonoBehaviour, ITabBarView
    {
        [Header("Tab Buttons")]
        [SerializeField] private TabButton[] _tabButtons;

        public bool IsVisible => gameObject.activeSelf;

        public event Action<FilterType> OnTabSelected;
        public event Action<int> OnTabIndexChanged;

        private FilterType _currentFilter = FilterType.All;

        private void Awake()
        {
            foreach (var tabButton in _tabButtons)
            {
                if (tabButton != null && tabButton.Button != null)
                {
                    var filterType = tabButton.FilterType;
                    tabButton.Button.onClick.AddListener(() => HandleTabClicked(filterType));
                }
            }
        }

        private void Start()
        {
            for (int i = 0; i < _tabButtons.Length; i++)
            {
                var tabButton = _tabButtons[i];
                if (tabButton != null)
                {
                    bool isSelected = tabButton.FilterType == _currentFilter;
                    tabButton.SetSelected(isSelected, animated: false);

                    if (isSelected)
                    {
                        OnTabIndexChanged?.Invoke(i);
                    }
                }
            }
        }

        private void OnDestroy()
        {
            foreach (var tabButton in _tabButtons)
            {
                if (tabButton != null && tabButton.Button != null)
                {
                    tabButton.Button.onClick.RemoveAllListeners();
                }
            }
        }

        public void SelectTab(FilterType filterType)
        {
            if (_currentFilter == filterType)
            {
                return;
            }

            _currentFilter = filterType;

            for (int i = 0; i < _tabButtons.Length; i++)
            {
                var tabButton = _tabButtons[i];
                if (tabButton != null)
                {
                    bool isSelected = tabButton.FilterType == filterType;
                    tabButton.SetSelected(isSelected, animated: true);

                    if (isSelected)
                    {
                        OnTabIndexChanged?.Invoke(i);
                    }
                }
            }
        }

        public void Show()
        {
            gameObject.SetActive(true);
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }

        private void HandleTabClicked(FilterType filterType)
        {
            SelectTab(filterType);
            OnTabSelected?.Invoke(filterType);
        }
    }
}
