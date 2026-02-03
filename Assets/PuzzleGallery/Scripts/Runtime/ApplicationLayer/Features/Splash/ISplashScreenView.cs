using System;
using Cysharp.Threading.Tasks;
using PuzzleGallery.Core.MVP;

namespace PuzzleGallery.Features.Splash
{
    public interface ISplashScreenView : IView
    {
        void SetProgress(float progress);

        UniTask FadeOutAsync(float duration);
    }
}
