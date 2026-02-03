using UnityEngine;

namespace PuzzleGallery.Features.MainMenu
{
    [CreateAssetMenu(fileName = "CarouselConfig", menuName = "PuzzleGallery/Carousel Config")]
    public sealed class CarouselConfig : ScriptableObject
    {
        [Header("State")]
        [SerializeField] private bool _enabled = true;

        [Header("Content")]
        [SerializeField] private BannerData[] _banners = new BannerData[3];

        [Header("Auto Scroll")]
        [SerializeField] private float _autoScrollInterval = 5f;
        [SerializeField] private bool _autoScrollEnabled = true;

        [Header("Animation")]
        [SerializeField] private float _scrollDuration = 0.5f;
        [SerializeField] private AnimationCurve _scrollCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

        public bool Enabled => _enabled;
        public BannerData[] Banners => _banners;
        public float AutoScrollInterval => _autoScrollInterval;
        public bool AutoScrollEnabled => _autoScrollEnabled;
        public float ScrollDuration => _scrollDuration;
        public AnimationCurve ScrollCurve => _scrollCurve;
    }
}
