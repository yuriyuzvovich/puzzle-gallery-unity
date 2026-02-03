using UnityEngine;

namespace PuzzleGallery.Features.Premium
{
    [CreateAssetMenu(fileName = "PopupConfig", menuName = "PuzzleGallery/Popup Config")]
    public sealed class PremiumPopupConfig : ScriptableObject
    {
        [Header("Addressable Keys")]
        [SerializeField] private string _premiumPurchaseAddress = "Prefabs/Popups/PremiumPurchasePopup";

        public string PremiumPurchaseAddress => _premiumPurchaseAddress;
    }
}
