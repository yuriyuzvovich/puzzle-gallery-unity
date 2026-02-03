using Cysharp.Threading.Tasks;
using PuzzleGallery.Core.ServiceLocator;

namespace PuzzleGallery.Services.Popup
{
    public interface IPopupService : IService
    {
        UniTask<T> ShowAsync<T>(PopupData data = null) where T : class, IPopup;

        UniTask HideAsync<T>() where T : class, IPopup;

        UniTask HideAllAsync();

        bool IsPopupShown<T>() where T : class, IPopup;

        IPopup CurrentPopup { get; }
    }
}
