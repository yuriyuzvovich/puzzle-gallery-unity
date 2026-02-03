using Cysharp.Threading.Tasks;

namespace PuzzleGallery.Core.StateMachine.Runtime
{
    public interface IState
    {
        UniTask EnterAsync();

        UniTask ExitAsync();

        void Update();
    }
}
