using System;
using System.Collections.Generic;
using PuzzleGallery.Services.Logging;
using UnityEngine;
using UnityEngine.UI;

namespace PuzzleGallery.Scripts.UI
{
    [RequireComponent(typeof(ScrollRect))]
    public class VirtualizedScrollRect : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private ScrollRect _scrollRect;
        [SerializeField] private RectTransform _viewport;
        [SerializeField] private RectTransform _content;
        [SerializeField] private GameObject _cellPrefab;

        [Header("Layout Settings")]
        [SerializeField] private float _horizontalSpacing = 10f;
        [SerializeField] private float _verticalSpacing = 10f;
        [SerializeField] private float _horizontalPadding = 10f;
        [SerializeField] private float _verticalPadding = 10f;
        [SerializeField] private int _bufferRows = 2;

        private int _columnCount = 2;
        private int _itemCount;
        private RecyclePool<VirtualizedCell> _pool;
        private readonly Dictionary<int, VirtualizedCell> _activeCells = new Dictionary<int, VirtualizedCell>();

        private int _firstVisibleRow = -1;
        private int _lastVisibleRow = -1;
        private float _cellWidth;
        private float _cellHeight;

        private Func<int, object> _dataProvider;

        public event Action<int> OnCellClicked;

        public event Action<VirtualizedCell, int> OnConfigureCell;

        private void Awake()
        {
            if (!_scrollRect) throw new Exception("ScrollRect reference is missing in VirtualizedScrollRect.");
            if (!_viewport) throw new Exception("Viewport reference is missing in VirtualizedScrollRect.");
            if (!_content) throw new Exception("Content reference is missing in VirtualizedScrollRect.");
        }

        private void Start()
        {
            _scrollRect.onValueChanged.AddListener(OnScroll);
        }

        private void OnDestroy()
        {
            _scrollRect.onValueChanged.RemoveListener(OnScroll);
            _pool?.Clear();
        }

        public void Initialize(int itemCount, Func<int, object> dataProvider = null)
        {
            _itemCount = itemCount;
            _dataProvider = dataProvider;

            CalculateCellSize();
            InitializePool();
            UpdateContentSize();
            UpdateVisibleCells();
        }

        private void CalculateCellSize()
        {
            float availableWidth = _viewport.rect.width - _horizontalPadding * 2 - _horizontalSpacing * (_columnCount - 1);
            _cellWidth = availableWidth / _columnCount;
            _cellHeight = _cellWidth;
        }

        public void SetColumnCount(int columns)
        {
            if (_columnCount == columns)
            {
                return;
            }

            _columnCount = columns;
            RecycleAllCells();
            UpdateContentSize();
            UpdateVisibleCells();
        }

        public void RefreshData()
        {
            foreach (var kvp in _activeCells)
            {
                OnConfigureCell?.Invoke(kvp.Value, kvp.Key);
            }
        }

        public void RefreshWithNewCount(int itemCount, bool forceRecycle = false, bool scrollToTop = false)
        {
            if (_itemCount == itemCount && !forceRecycle)
            {
                RefreshData();
                return;
            }

            _itemCount = itemCount;
            RecycleAllCells();
            UpdateContentSize();

            if (scrollToTop)
            {
                ScrollToTop();
            }

            UpdateVisibleCells();
        }

        public void ScrollToTop()
        {
            _scrollRect.verticalNormalizedPosition = 1f;
        }

        private void InitializePool()
        {
            if (_pool != null)
            {
                _pool.Clear();
            }

            int visibleRows = Mathf.CeilToInt(_viewport.rect.height / (_cellHeight + _verticalSpacing)) + 1;
            int poolSize = (visibleRows + _bufferRows * 2) * _columnCount;

            _pool = new RecyclePool<VirtualizedCell>(CreateCell, _content, poolSize);
        }

        private VirtualizedCell CreateCell()
        {
            var go = Instantiate(_cellPrefab, _content);
            var cell = go.GetComponent<VirtualizedCell>();

            if (cell == null)
            {
                Logs.Error("Cell prefab must have a VirtualizedCell component!");
                return null;
            }

            cell.OnCellClicked += index => OnCellClicked?.Invoke(index);
            return cell;
        }

        private void UpdateContentSize()
        {
            CalculateCellSize();

            int rowCount = Mathf.CeilToInt((float) _itemCount / _columnCount);

            float height;
            if (rowCount == 0)
            {
                height = _verticalPadding * 2;
            }
            else
            {
                height = rowCount * (_cellHeight + _verticalSpacing) - _verticalSpacing + _verticalPadding * 2;
            }

            height = Mathf.Max(0, height);

            if (Logs.IsEnabled(LogLevel.Debug))
            {
                Logs.Debug($"[VirtualizedScrollRect] UpdateContentSize: itemCount={_itemCount}, columnCount={_columnCount}, rowCount={rowCount}, cellHeight={_cellHeight:F2}, vSpacing={_verticalSpacing}, vPadding={_verticalPadding}, calculatedHeight={height:F2}, viewportHeight={_viewport.rect.height:F2}");
            }

            _content.sizeDelta = new Vector2(_content.sizeDelta.x, height);
        }

        private void OnScroll(Vector2 position)
        {
            UpdateVisibleCells();
        }

        private void UpdateVisibleCells()
        {
            if (_itemCount == 0)
            {
                RecycleAllCells();
                return;
            }

            float scrollY = _content.anchoredPosition.y;
            float viewportHeight = _viewport.rect.height;

            int newFirstRow = Mathf.Max(0, Mathf.FloorToInt((scrollY - _verticalPadding) / (_cellHeight + _verticalSpacing)) - _bufferRows);
            int totalRows = Mathf.CeilToInt((float) _itemCount / _columnCount);
            int newLastRow = Mathf.Min(
                Mathf.CeilToInt((scrollY + viewportHeight - _verticalPadding) / (_cellHeight + _verticalSpacing)) + _bufferRows,
                totalRows - 1
            );

            if (newFirstRow == _firstVisibleRow && newLastRow == _lastVisibleRow)
            {
                return;
            }

            RecycleCellsOutOfRange(newFirstRow, newLastRow);

            for (int row = newFirstRow; row <= newLastRow; row++)
            {
                for (int col = 0; col < _columnCount; col++)
                {
                    int index = row * _columnCount + col;
                    if (index >= _itemCount)
                    {
                        break;
                    }

                    if (!_activeCells.ContainsKey(index))
                    {
                        var cell = _pool.Get();
                        cell.SetIndex(index);
                        PositionCell(cell, row, col);

                        OnConfigureCell?.Invoke(cell, index);
                        _activeCells[index] = cell;
                    }
                }
            }

            _firstVisibleRow = newFirstRow;
            _lastVisibleRow = newLastRow;
        }

        private void RecycleCellsOutOfRange(int newFirstRow, int newLastRow)
        {
            var toRecycle = new List<int>();

            foreach (var kvp in _activeCells)
            {
                int row = kvp.Key / _columnCount;
                if (row < newFirstRow || row > newLastRow)
                {
                    toRecycle.Add(kvp.Key);
                }
            }

            foreach (var index in toRecycle)
            {
                var cell = _activeCells[index];
                cell.OnRecycle();
                _pool.Return(cell);
                _activeCells.Remove(index);
            }
        }

        private void RecycleAllCells()
        {
            foreach (var cell in _activeCells.Values)
            {
                cell.OnRecycle();
                _pool.Return(cell);
            }
            _activeCells.Clear();
            _firstVisibleRow = -1;
            _lastVisibleRow = -1;
        }

        private void PositionCell(VirtualizedCell cell, int row, int col)
        {
            var rt = cell.GetComponent<RectTransform>();

            rt.anchorMin = new Vector2(0, 1);
            rt.anchorMax = new Vector2(0, 1);
            rt.pivot = new Vector2(0, 1);

            float x = _horizontalPadding + col * (_cellWidth + _horizontalSpacing);
            float y = -_verticalPadding - row * (_cellHeight + _verticalSpacing);

            rt.anchoredPosition = new Vector2(x, y);
            rt.sizeDelta = new Vector2(_cellWidth, _cellHeight);
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (_scrollRect == null)
            {
                _scrollRect = GetComponent<ScrollRect>();
            }
        }
#endif
    }
}