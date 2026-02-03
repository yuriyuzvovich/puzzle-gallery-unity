using System;
using UnityEngine;

namespace PuzzleGallery.Scripts.UI
{
    public abstract class VirtualizedCell : MonoBehaviour
    {
        public int Index { get; private set; } = -1;

        public event Action<int> OnCellClicked;

        public void SetIndex(int index)
        {
            Index = index;
        }

        public virtual void OnRecycle()
        {
            Index = -1;
        }

        protected void InvokeCellClicked()
        {
            OnCellClicked?.Invoke(Index);
        }
    }
}
