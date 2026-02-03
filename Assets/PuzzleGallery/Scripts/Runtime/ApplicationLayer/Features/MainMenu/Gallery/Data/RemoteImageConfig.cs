using UnityEngine;

namespace PuzzleGallery.Features.MainMenu
{
    [CreateAssetMenu(fileName = "RemoteImageConfig", menuName = "PuzzleGallery/Remote Image Config")]
    public sealed class RemoteImageConfig : ScriptableObject
    {
        [Header("Cache Settings")]
        [SerializeField] private int _maxCacheSize = 100;

        [Header("Loading Settings")]
        [SerializeField] private int _maxConcurrentDownloads = 4;
        [SerializeField] private float _downloadTimeout = 30f;

        [Header("Retry Settings")]
        [SerializeField] private int _maxRetries = 3;
        [SerializeField] private float _retryDelay = 1f;

        public int MaxCacheSize => _maxCacheSize;
        public int MaxConcurrentDownloads => _maxConcurrentDownloads;
        public float DownloadTimeout => _downloadTimeout;
        public int MaxRetries => _maxRetries;
        public float RetryDelay => _retryDelay;
    }
}
