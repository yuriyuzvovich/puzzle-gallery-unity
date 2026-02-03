using System;
using Cysharp.Threading.Tasks;

namespace PuzzleGallery.Services.Popup
{
    public interface IPopup
    {
        UniTask ShowAsync();

        UniTask HideAsync();

        void SetData(PopupData data);

        event Action OnClosed;

        bool IsVisible { get; }
    }
}
