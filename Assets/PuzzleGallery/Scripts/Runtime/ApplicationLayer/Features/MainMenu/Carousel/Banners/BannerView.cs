using UnityEngine;

namespace PuzzleGallery.Features.MainMenu
{
    public abstract class BannerView : MonoBehaviour, IBannerView
    {
        private BannerPresenter _presenter;

        public bool IsVisible => gameObject.activeSelf;

        protected abstract BannerPresenter CreatePresenter();

        protected virtual void Awake()
        {
            _presenter = CreatePresenter();
            _presenter?.Initialize();
        }

        protected virtual void OnDestroy()
        {
            _presenter?.Dispose();
            _presenter = null;
        }

        public void Show()
        {
            gameObject.SetActive(true);
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }
    }
}
