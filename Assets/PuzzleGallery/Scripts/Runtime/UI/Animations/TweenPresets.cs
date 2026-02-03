using DG.Tweening;
using UnityEngine;

namespace PuzzleGallery.Scripts.UI
{
    public static class TweenPresets
    {
        public const float FastDuration = 0.15f;
        public const float NormalDuration = 0.3f;
        public const float SlowDuration = 0.5f;

        public const float PressScale = 0.95f;
        public const float BounceScale = 1.1f;

        public const float FadeOutAlpha = 0f;
        public const float FadeInAlpha = 1f;

        public static Tween PressDown(RectTransform target, float duration = FastDuration)
        {
            return target.DOScale(PressScale, duration).SetEase(Ease.OutQuad);
        }

        public static Tween PressUp(RectTransform target, float duration = FastDuration)
        {
            return target.DOScale(1f, duration).SetEase(Ease.OutBack);
        }

        public static Tween BounceIn(RectTransform target, float duration = NormalDuration)
        {
            target.localScale = Vector3.zero;
            return target.DOScale(1f, duration).SetEase(Ease.OutBack);
        }

        public static Tween BounceOut(RectTransform target, float duration = NormalDuration)
        {
            return target.DOScale(0f, duration).SetEase(Ease.InBack);
        }

        public static Tween FadeIn(CanvasGroup target, float duration = NormalDuration)
        {
            target.alpha = 0f;
            return target.DOFade(1f, duration).SetEase(Ease.OutQuad);
        }

        public static Tween FadeOut(CanvasGroup target, float duration = NormalDuration)
        {
            return target.DOFade(0f, duration).SetEase(Ease.InQuad);
        }

        public static Tween SlideInFromBottom(RectTransform target, float distance = 100f, float duration = NormalDuration)
        {
            var startPos = target.anchoredPosition;
            target.anchoredPosition = new Vector2(startPos.x, startPos.y - distance);
            return target.DOAnchorPos(startPos, duration).SetEase(Ease.OutQuad);
        }

        public static Tween SlideOutToBottom(RectTransform target, float distance = 100f, float duration = NormalDuration)
        {
            var endPos = new Vector2(target.anchoredPosition.x, target.anchoredPosition.y - distance);
            return target.DOAnchorPos(endPos, duration).SetEase(Ease.InQuad);
        }

        public static Tween Pulse(RectTransform target, float scale = BounceScale, float duration = NormalDuration)
        {
            return target.DOScale(scale, duration / 2f)
                .SetEase(Ease.OutQuad)
                .SetLoops(2, LoopType.Yoyo);
        }

        public static Tween Shake(RectTransform target, float strength = 10f, float duration = NormalDuration)
        {
            return target.DOShakeAnchorPos(duration, strength, 10, 90f, false, true);
        }
    }
}
