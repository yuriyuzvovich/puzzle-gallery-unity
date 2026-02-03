using System.IO;
using Cysharp.Threading.Tasks;
using PuzzleGallery.Services.Logging;
using UnityEngine;

namespace PuzzleGallery.Services.Save.Runtime
{
    public sealed class JsonSaveStrategy : ISaveStrategy
    {
        private readonly string _savePath;

        public JsonSaveStrategy()
        {
            _savePath = Path.Combine(Application.persistentDataPath, "Saves");
            if (!Directory.Exists(_savePath))
            {
                Directory.CreateDirectory(_savePath);
            }
        }

        public async UniTask SaveAsync<T>(string key, T data)
        {
            var filePath = GetFilePath(key);
            var json = JsonUtility.ToJson(data, true);

            await File.WriteAllTextAsync(filePath, json);
            Logs.Info($"Saved data to: {filePath}");
        }

        public async UniTask<T> LoadAsync<T>(string key, T defaultValue = default)
        {
            var filePath = GetFilePath(key);

            if (!File.Exists(filePath))
            {
                return defaultValue;
            }

            try
            {
                var json = await File.ReadAllTextAsync(filePath);
                return JsonUtility.FromJson<T>(json);
            }
            catch (System.Exception ex)
            {
                Logs.Exception(ex, $"Failed to load data from {filePath}.");
                return defaultValue;
            }
        }

        public async UniTask DeleteAsync(string key)
        {
            var filePath = GetFilePath(key);

            if (File.Exists(filePath))
            {
                await UniTask.RunOnThreadPool(() => File.Delete(filePath));
                Logs.Info($"Deleted save file: {filePath}");
            }
        }

        public bool HasKey(string key)
        {
            return File.Exists(GetFilePath(key));
        }

        private string GetFilePath(string key)
        {
            return Path.Combine(_savePath, $"{key}.json");
        }
    }
}
