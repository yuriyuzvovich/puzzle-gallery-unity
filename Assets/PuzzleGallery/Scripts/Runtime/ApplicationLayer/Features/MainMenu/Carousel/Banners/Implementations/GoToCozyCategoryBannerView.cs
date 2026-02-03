using System;
using UnityEngine;
using UnityEngine.UI;

namespace PuzzleGallery.Features.MainMenu
{
    public sealed class GoToCozyCategoryBannerView : BannerView
    {
        [SerializeField] private Button _actionButton;

        public event Action OnAction;

        protected override void Awake()
        {
            if (_actionButton != null)
            {
                _actionButton.onClick.AddListener(HandleActionClicked);
            }

            base.Awake();
        }

        protected override void OnDestroy()
        {
            if (_actionButton != null)
            {
                _actionButton.onClick.RemoveListener(HandleActionClicked);
            }

            base.OnDestroy();
        }

        protected override BannerPresenter CreatePresenter()
        {
            return new GoToCozyCategoryBannerPresenter(this);
        }

        private void HandleActionClicked()
        {
            OnAction?.Invoke();
        }
    }
}
