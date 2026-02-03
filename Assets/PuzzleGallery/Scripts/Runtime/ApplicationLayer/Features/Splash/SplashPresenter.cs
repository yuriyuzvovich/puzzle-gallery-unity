using System;
using Cysharp.Threading.Tasks;
using PuzzleGallery.Core.MVP;
using PuzzleGallery.Services;
using PuzzleGallery.Services.Asset;

namespace PuzzleGallery.Features.Splash
{
    public sealed class SplashPresenter : IPresenter
    {
        private readonly ISplashScreenView _screenView;
        private readonly IAssetService _assetService;

        public event Action OnLoadingComplete;

        public SplashPresenter(ISplashScreenView screenView, IAssetService assetService)
        {
            _screenView = screenView;
            _assetService = assetService;
        }

        public void Initialize()
        {
            _screenView.Show();
            _screenView.SetProgress(0f);
        }

        public void Dispose()
        {
            _screenView.Hide();
        }

        public async UniTask LoadAsync(string[] assetsToPreload = null)
        {
            _screenView.SetProgress(0f);

            if (assetsToPreload != null && assetsToPreload.Length > 0)
            {
                float progressStep = 1f / assetsToPreload.Length;
                float currentProgress = 0f;

                foreach (var asset in assetsToPreload)
                {
                    try
                    {
                        await _assetService.LoadAssetAsync<UnityEngine.Object>(asset);
                    }
                    catch (Exception)
                    {
                    }

                    currentProgress += progressStep;
                    _screenView.SetProgress(currentProgress);
                }
            }
            else
            {
                for (float t = 0; t < 1f; t += 0.1f)
                {
                    _screenView.SetProgress(t);
                    await UniTask.Delay(100);
                }
            }

            _screenView.SetProgress(1f);
            OnLoadingComplete?.Invoke();
        }

        public async UniTask FadeOutAsync(float duration = 0.5f)
        {
            await _screenView.FadeOutAsync(duration);
        }
    }
}
