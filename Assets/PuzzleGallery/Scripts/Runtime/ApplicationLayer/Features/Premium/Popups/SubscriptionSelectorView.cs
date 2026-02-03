using System;
using UnityEngine;

namespace PuzzleGallery.Features.Premium
{
    public sealed class SubscriptionSelectorView : MonoBehaviour
    {
        [Header("Button Configuration")]
        [SerializeField] private Transform _buttonsContainer;
        [SerializeField] private SubscriptionOptionButton[] _buttons;

        public event Action<SubscriptionType> OnSubscriptionChanged;

        private SubscriptionOption[] _options;
        private SubscriptionType _currentSelection = SubscriptionType.Year;

        public void Initialize(SubscriptionOption[] options, SubscriptionType defaultSelection)
        {
            if (options == null || options.Length == 0)
            {
                Debug.LogWarning("SubscriptionSelectorView: No subscription options provided.");
                return;
            }

            _options = options;
            _currentSelection = defaultSelection;

            CreateButtons();
            SelectSubscription(defaultSelection, animated : false);
        }

        public void SelectSubscription(SubscriptionType type, bool animated = true)
        {
            _currentSelection = type;

            UpdateButtons(animated);

            OnSubscriptionChanged?.Invoke(type);
        }

        private void CreateButtons()
        {
            if (!_buttonsContainer || _buttons == null)
            {
                Debug.LogWarning("SubscriptionSelectorView: ButtonsContainer or SubscriptionButtonPrefab is not assigned.");
                return;
            }

            for (int i = 0; i < _options.Length; i++)
            {
                var option = _options[i];
                var button = _buttons[i];

                button.SetData(option);

                var subscriptionType = option.Type;
                button.Button.onClick.AddListener(() => SelectSubscription(subscriptionType));
            }
        }

        private void UpdateButtons(bool animated)
        {
            if (_buttons == null)
            {
                return;
            }

            for (int i = 0; i < _buttons.Length; i++)
            {
                if (_buttons[i] == null)
                {
                    continue;
                }

                bool isSelected = _options[i].Type == _currentSelection;
                _buttons[i].SetSelected(isSelected, animated);
            }
        }

        private void OnDestroy()
        {
            if (_buttons != null)
            {
                foreach (var button in _buttons)
                {
                    if (button != null && button.Button != null)
                    {
                        button.Button.onClick.RemoveAllListeners();
                    }
                }
            }
        }
    }
}
