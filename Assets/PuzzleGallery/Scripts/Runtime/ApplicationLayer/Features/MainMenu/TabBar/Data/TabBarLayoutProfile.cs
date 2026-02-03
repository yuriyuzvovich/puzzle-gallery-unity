using UnityEngine;

namespace PuzzleGallery.Features.MainMenu.Data
{
    [System.Serializable]
    public struct TabBarLayoutProfile
    {
        [Tooltip("Horizontal spacing between tabs (in pixels)")]
        public float TabSpacing;

        [Tooltip("Padding from container edges (in pixels)")]
        public float EdgePadding;

        [Tooltip("Height of the selection indicator as ratio of tab height (0-1)")]
        [Range(0f, 1f)]
        public float IndicatorHeightRatio;

        [Tooltip("Vertical offset of indicator from bottom as ratio of tab height (0-1)")]
        [Range(0f, 1f)]
        public float IndicatorBottomOffsetRatio;

        [Header("Dividers")]
        [Tooltip("Width of vertical dividers between tabs (in pixels)")]
        [Range(1f, 50f)]
        public float DividerWidth;

        [Tooltip("Vertical padding for dividers from top and bottom (in pixels)")]
        [Range(0f, 50f)]
        public float DividerVerticalPadding;
    }
}