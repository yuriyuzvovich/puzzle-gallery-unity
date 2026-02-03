using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using PuzzleGallery.Features.MainMenu;
using PuzzleGallery.Services.Logging;
using UnityEngine;
using UnityEngine.Networking;

namespace PuzzleGallery.Services.RemoteImage
{
    public sealed class RemoteImageService : IRemoteImageService
    {
        private readonly RemoteImageConfig _config;
        private readonly Dictionary<string, Sprite> _memoryCache = new Dictionary<string, Sprite>();
        private readonly Dictionary<string, UniTask<Sprite>> _loadingTasks = new Dictionary<string, UniTask<Sprite>>();
        private readonly Queue<string> _cacheOrder = new Queue<string>();
        private int _activeDownloads = 0;

        public RemoteImageService(RemoteImageConfig config)
        {
            _config = config;
        }

        public void Initialize() { }

        public void Dispose()
        {
            ClearCache();
        }

        public async UniTask<Sprite> LoadImageAsync(string url, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(url))
            {
                Logs.Error("[RemoteImageService] Cannot load image: URL is null or empty");
                throw new ArgumentException("URL cannot be null or empty", nameof(url));
            }

            if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
            {
                Logs.Error($"[RemoteImageService] Invalid URL format: {url}");
                throw new ArgumentException($"Invalid URL format: {url}", nameof(url));
            }

            if (_memoryCache.TryGetValue(url, out var cached))
            {
                if (Logs.IsEnabled(LogLevel.Debug))
                {
                    Logs.Debug($"[RemoteImageService] Cache hit: {url}");
                }
                return cached;
            }

            if (_loadingTasks.ContainsKey(url))
            {
                while (_loadingTasks.ContainsKey(url))
                {
                    await UniTask.Yield(cancellationToken);

                    if (_memoryCache.TryGetValue(url, out var cachedResult))
                    {
                        return cachedResult;
                    }
                }

                if (_memoryCache.TryGetValue(url, out var finalResult))
                {
                    return finalResult;
                }
            }

            while (_activeDownloads >= _config.MaxConcurrentDownloads)
            {
                await UniTask.Yield(cancellationToken);
            }

            _activeDownloads++;
            var task = DownloadImageAsync(url, cancellationToken);
            _loadingTasks[url] = task;

            try
            {
                var sprite = await task;
                AddToCache(url, sprite);
                return sprite;
            }
            finally
            {
                _loadingTasks.Remove(url);
                _activeDownloads--;
            }
        }

        private async UniTask<Sprite> DownloadImageAsync(string url, CancellationToken cancellationToken)
        {
            int attempt = 0;
            Exception lastException = null;

            while (attempt <= _config.MaxRetries)
            {
                try
                {
                    using var request = UnityWebRequestTexture.GetTexture(url);
                    request.timeout = (int)_config.DownloadTimeout;

                    await request.SendWebRequest().WithCancellation(cancellationToken);

                    if (request.result != UnityWebRequest.Result.Success)
                    {
                        throw new Exception($"Failed to download image from {url}: {request.error}");
                    }

                    var texture = DownloadHandlerTexture.GetContent(request);
                    var sprite = Sprite.Create(
                        texture,
                        new Rect(0, 0, texture.width, texture.height),
                        new Vector2(0.5f, 0.5f));

                    if (attempt > 0)
                    {
                        Logs.Info($"[RemoteImageService] Successfully downloaded {url} after {attempt} retries");
                    }
                    else if (Logs.IsEnabled(LogLevel.Debug))
                    {
                        Logs.Debug($"[RemoteImageService] Successfully downloaded {url}");
                    }

                    return sprite;
                }
                catch (OperationCanceledException)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    lastException = ex;
                    attempt++;

                    if (attempt <= _config.MaxRetries)
                    {
                        float delay = _config.RetryDelay * Mathf.Pow(2, attempt - 1);
                        Logs.Warning($"[RemoteImageService] Failed to download {url} (attempt {attempt}/{_config.MaxRetries}). Retrying in {delay:F1}s... Error: {ex.Message}");

                        await UniTask.Delay(TimeSpan.FromSeconds(delay), cancellationToken: cancellationToken);
                    }
                    else
                    {
                        Logs.Exception(ex, $"[RemoteImageService] Failed to download {url} after {_config.MaxRetries} retries.");
                    }
                }
            }

            throw lastException ?? new Exception($"Failed to download image from {url} after {_config.MaxRetries} retries");
        }

        private void AddToCache(string url, Sprite sprite)
        {
            if (_memoryCache.Count >= _config.MaxCacheSize)
            {
                var oldest = _cacheOrder.Dequeue();
                if (_memoryCache.TryGetValue(oldest, out var oldSprite))
                {
                    if (oldSprite != null && oldSprite.texture != null)
                    {
                        UnityEngine.Object.Destroy(oldSprite.texture);
                        UnityEngine.Object.Destroy(oldSprite);
                    }
                    _memoryCache.Remove(oldest);
                }
            }

            _memoryCache[url] = sprite;
            _cacheOrder.Enqueue(url);
        }

        public void ClearCache()
        {
            foreach (var sprite in _memoryCache.Values)
            {
                if (sprite != null)
                {
                    if (sprite.texture != null)
                    {
                        UnityEngine.Object.Destroy(sprite.texture);
                    }
                    UnityEngine.Object.Destroy(sprite);
                }
            }

            _memoryCache.Clear();
            _cacheOrder.Clear();
            _loadingTasks.Clear();
        }

        public bool IsImageCached(string url)
        {
            return _memoryCache.ContainsKey(url);
        }
    }
}
