using System.Threading;
using Cysharp.Threading.Tasks;
using PuzzleGallery.Core.ServiceLocator;
using UnityEngine;

namespace PuzzleGallery.Services.RemoteImage
{
    public interface IRemoteImageService : IService
    {
        UniTask<Sprite> LoadImageAsync(string url, CancellationToken cancellationToken = default);

        void ClearCache();

        bool IsImageCached(string url);
    }
}
