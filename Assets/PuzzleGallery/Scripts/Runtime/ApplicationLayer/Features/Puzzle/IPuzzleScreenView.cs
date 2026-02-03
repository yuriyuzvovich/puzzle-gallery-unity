using System;
using Cysharp.Threading.Tasks;
using PuzzleGallery.Core.MVP;

namespace PuzzleGallery.Features.Puzzle
{
    public interface IPuzzleScreenView : IView
    {
        event Action OnBackClicked;

        void SetImageUrl(string url);
        UniTask ShowAsync();
        UniTask HideAsync();
    }
}