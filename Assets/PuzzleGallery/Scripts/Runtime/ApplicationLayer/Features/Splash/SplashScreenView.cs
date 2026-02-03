using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace PuzzleGallery.Features.Splash
{
    public class SplashScreenView : MonoBehaviour, ISplashScreenView
    {
        [Header("References")]
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private Slider _progressBar;
        [SerializeField] private Image _progressFill;

        public bool IsVisible => gameObject.activeSelf;

        public void Show()
        {
            gameObject.SetActive(true);
            if (_canvasGroup != null)
            {
                _canvasGroup.alpha = 1f;
            }
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }

        public void SetProgress(float progress)
        {
            if (_progressBar != null)
            {
                _progressBar.value = Mathf.Clamp01(progress);
            }

            if (_progressFill != null)
            {
                _progressFill.fillAmount = Mathf.Clamp01(progress);
            }
        }

        public async UniTask FadeOutAsync(float duration)
        {
            if (_canvasGroup != null)
            {
                var tween = _canvasGroup.DOFade(0f, duration);
                while (tween.active && !tween.IsComplete())
                {
                    await UniTask.Yield();
                }
            }
            else
            {
                await UniTask.Delay((int)(duration * 1000));
            }

            Hide();
        }
    }
}
