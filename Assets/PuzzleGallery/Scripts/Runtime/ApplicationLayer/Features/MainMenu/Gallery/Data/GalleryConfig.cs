using UnityEngine;

namespace PuzzleGallery.Features.MainMenu
{
    [CreateAssetMenu(fileName = "GalleryConfig", menuName = "PuzzleGallery/Gallery Config")]
    public sealed class GalleryConfig : ScriptableObject
    {
        [Header("Image Source")]
        [SerializeField] private string _baseUrl = "http://data.ikppbb.com/test-task-unity-data/pics/";
        [SerializeField] private int _totalImages = 66;

        [Header("Premium Settings")]
        [SerializeField] private int _premiumInterval = 4;

        [Header("Connection Settings")]
        [SerializeField] private ProtocolStrategy _protocolStrategy = ProtocolStrategy.HttpsWithFallback;
        [SerializeField] private bool _allowInsecureConnections = true;
        [SerializeField] private bool _bypassCertificateValidation = false;

        [Header("UI Settings")]
        [SerializeField] private float _cellHeight = 200f;
        [SerializeField] private float _cellSpacing = 10f;
        [SerializeField] private float _padding = 10f;

        public string BaseUrl => _baseUrl;
        public int TotalImages => _totalImages;
        public int PremiumInterval => _premiumInterval;
        public ProtocolStrategy Strategy => _protocolStrategy;
        public bool AllowInsecureConnections => _allowInsecureConnections;
        public bool BypassCertificateValidation => _bypassCertificateValidation;
        public float CellHeight => _cellHeight;
        public float CellSpacing => _cellSpacing;
        public float Padding => _padding;

        public enum ProtocolStrategy
        {
            HttpsOnly,
            HttpOnly,
            HttpsWithFallback,
            Auto
        }
    }
}
