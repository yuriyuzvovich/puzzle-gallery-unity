using Cysharp.Threading.Tasks;

namespace PuzzleGallery.Services.Save.Runtime
{
    public sealed class LocalStorageService : ILocalStorageService
    {
        private readonly ISaveStrategy _strategy;

        public LocalStorageService(ISaveStrategy strategy)
        {
            _strategy = strategy;
        }

        public void Initialize() { }

        public void Dispose() { }

        public async UniTask SaveAsync<T>(string key, T data)
        {
            await _strategy.SaveAsync(key, data);
        }

        public async UniTask<T> LoadAsync<T>(string key, T defaultValue = default)
        {
            return await _strategy.LoadAsync(key, defaultValue);
        }

        public async UniTask DeleteAsync(string key)
        {
            await _strategy.DeleteAsync(key);
        }

        public bool HasKey(string key)
        {
            return _strategy.HasKey(key);
        }
    }
}
