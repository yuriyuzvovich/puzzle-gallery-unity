using Cysharp.Threading.Tasks;
using PuzzleGallery.Core;
using PuzzleGallery.Core.EventBus;
using PuzzleGallery.Core.ServiceLocator;
using PuzzleGallery.Services.Popup;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace PuzzleGallery.Features.Premium
{
    public class PremiumPurchasePopup : PopupBase
    {
        [Header("Premium Popup References")]
        [SerializeField] private TextMeshProUGUI _titleText;
        [SerializeField] private TextMeshProUGUI _descriptionText;
        [SerializeField] private Button _purchaseButton;
        [SerializeField] private TextMeshProUGUI _purchaseButtonText;

        [Header("Subscription Selector")]
        [SerializeField] private SubscriptionSelectorView _subscriptionSelector;

        private GlobalEventBus _eventBus;
        private SubscriptionType _currentSubscription = SubscriptionType.Month;
        private bool _subscriptionSelectorInitialized;

        protected override void Awake()
        {
            base.Awake();

            if (ServiceLocator.Instance.TryGet<GlobalEventBus>(out var eventBus))
            {
                _eventBus = eventBus;
            }

            if (_purchaseButton)
            {
                _purchaseButton.onClick.AddListener(OnPurchaseClicked);
            }

            if (_subscriptionSelector)
            {
                _subscriptionSelector.OnSubscriptionChanged += OnSubscriptionChanged;
            }
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            if (_purchaseButton)
            {
                _purchaseButton.onClick.RemoveListener(OnPurchaseClicked);
            }

            if (_subscriptionSelector)
            {
                _subscriptionSelector.OnSubscriptionChanged -= OnSubscriptionChanged;
            }
        }

        public override void SetData(PopupData data)
        {
            base.SetData(data);

            if (data is PremiumPurchasePopupData premiumData)
            {
                InitializeSubscriptionSelector();
            }
            else
            {
                UpdateDefaultContent();
            }
        }

        private void UpdateDefaultContent()
        {
            InitializeSubscriptionSelector();
        }

        private void InitializeSubscriptionSelector()
        {
            if (_subscriptionSelector == null || _subscriptionSelectorInitialized)
            {
                return;
            }

            var subscriptionOptions = new SubscriptionOption[] {
                new SubscriptionOption {
                    Type = SubscriptionType.Week,
                },
                new SubscriptionOption {
                    Type = SubscriptionType.Month,
                },
                new SubscriptionOption {
                    Type = SubscriptionType.Year,
                }
            };

            _subscriptionSelector.Initialize(subscriptionOptions, SubscriptionType.Month);
            _currentSubscription = SubscriptionType.Month;
            _subscriptionSelectorInitialized = true;
        }

        private void OnSubscriptionChanged(SubscriptionType newType)
        {
            _currentSubscription = newType;
        }

        private void OnPurchaseClicked()
        {
            _eventBus?.Publish(new SubscriptionSelectedEvent(_currentSubscription));
            HideAsync().Forget();
        }

        protected override void OnShown()
        {
            base.OnShown();
        }

        protected override void OnHidden()
        {
            base.OnHidden();
        }
    }
}