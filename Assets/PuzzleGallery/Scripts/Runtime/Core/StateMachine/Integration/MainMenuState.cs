using Cysharp.Threading.Tasks;
using PuzzleGallery.Core.StateMachine.Runtime;
using PuzzleGallery.Features.MainMenu;

namespace PuzzleGallery.Core.StateMachine.Integration
{
    public sealed class MainMenuState : IState
    {
        private readonly MainMenuScreenPresenter _presenter;

        public MainMenuState(MainMenuScreenPresenter presenter)
        {
            _presenter = presenter;
        }

        public async UniTask EnterAsync()
        {
            await _presenter.ShowAsync();
        }

        public async UniTask ExitAsync()
        {
            await _presenter.HideAsync();
        }

        public void Update()
        {
        }
    }
}
