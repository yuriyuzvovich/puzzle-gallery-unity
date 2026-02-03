namespace PuzzleGallery.Core.MVP
{
    public interface IView
    {
        void Show();

        void Hide();

        bool IsVisible { get; }
    }
}
