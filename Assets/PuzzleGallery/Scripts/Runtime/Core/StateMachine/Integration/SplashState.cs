using Cysharp.Threading.Tasks;
using PuzzleGallery.Core.StateMachine.Runtime;

namespace PuzzleGallery.Core.StateMachine.Integration
{
    public sealed class SplashState : IState
    {
        private readonly IStateMachine _stateMachine;

        public SplashState(IStateMachine stateMachine)
        {
            _stateMachine = stateMachine;
        }

        public async UniTask EnterAsync()
        {
            await UniTask.Delay(1000);

            await _stateMachine.TransitionToAsync<MainMenuState>();
        }

        public async UniTask ExitAsync()
        {
            await UniTask.Yield();
        }

        public void Update()
        {
        }
    }
}
