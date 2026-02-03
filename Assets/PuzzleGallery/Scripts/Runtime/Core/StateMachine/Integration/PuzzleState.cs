using Cysharp.Threading.Tasks;
using PuzzleGallery.Core.StateMachine.Runtime;
using PuzzleGallery.Features.MainMenu;
using PuzzleGallery.Features.Puzzle;

namespace PuzzleGallery.Core.StateMachine.Integration
{
    public sealed class PuzzleState : IState
    {
        private readonly IStateMachine _stateMachine;
        private readonly PuzzleScreenPresenter _screenPresenter;
        private GalleryItemData _imageData;

        public PuzzleState(
            IStateMachine stateMachine,
            PuzzleScreenPresenter screenPresenter)
        {
            _stateMachine = stateMachine;
            _screenPresenter = screenPresenter;
        }

        public void SetImageData(GalleryItemData imageData)
        {
            _imageData = imageData;
        }

        public async UniTask EnterAsync()
        {
            _screenPresenter.Initialize();
            _screenPresenter.OnBackRequested += HandleBackRequested;

            if (_imageData != null)
            {
                await _screenPresenter.ShowWithImageAsync(_imageData);
            }
        }

        public async UniTask ExitAsync()
        {
            _screenPresenter.OnBackRequested -= HandleBackRequested;
            await _screenPresenter.HideAsync();
        }

        public void Update()
        {
            if (UnityEngine.Input.GetKeyDown(UnityEngine.KeyCode.Escape))
            {
                _screenPresenter.HandleHardwareBackButton();
            }
        }

        private void HandleBackRequested()
        {
            TransitionToMenuAsync().Forget();
        }

        private async UniTaskVoid TransitionToMenuAsync()
        {
            await _stateMachine.TransitionToAsync<MainMenuState>();
        }
    }
}
