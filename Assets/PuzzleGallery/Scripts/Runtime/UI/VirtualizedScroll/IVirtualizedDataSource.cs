namespace PuzzleGallery.Scripts.UI
{
    public interface IVirtualizedDataSource<T>
    {
        int ItemCount { get; }

        T GetItem(int index);
    }
}
