using UnityEngine;
using CandyCrush.Core;
using CandyCrush.Data;

namespace CandyCrush.Core
{
    [RequireComponent(typeof(SpriteRenderer))]
    public class Candy : MonoBehaviour
    {
        public CandyType Type { get; private set; }
        public SpecialCandyType SpecialType { get; private set; }
        public Vector2Int GridPosition { get; private set; }
        public int BoardSize { get; set; }
        public bool IsMoving { get; set; }

        private SpriteRenderer spriteRenderer;
        private CandyPalette palette;

        public void Initialize(CandyType type, SpecialCandyType special, Vector2Int gridPos, CandyPalette paletteRef, float cellSize, float spacing, int boardSize)
        {
            Type = type;
            SpecialType = special;
            GridPosition = gridPos;
            palette = paletteRef;
            BoardSize = boardSize;
            spriteRenderer = GetComponent<SpriteRenderer>();
            UpdateVisual(cellSize, spacing);
            UpdateWorldPosition(cellSize, spacing);
        }

        public void SetGridPosition(Vector2Int pos, float cellSize, float spacing)
        {
            GridPosition = pos;
            UpdateWorldPosition(cellSize, spacing);
        }

        public void UpdateWorldPosition(float cellSize, float spacing)
        {
            float offset = (BoardSize - 1) * (cellSize + spacing) * 0.5f;
            transform.position = new Vector3(
                GridPosition.y * (cellSize + spacing) - offset,
                -GridPosition.x * (cellSize + spacing) + offset,
                0f
            );
        }

        public void SetType(CandyType type, SpecialCandyType special, float cellSize, float spacing)
        {
            Type = type;
            SpecialType = special;
            UpdateVisual(cellSize, spacing);
        }

        void UpdateVisual(float cellSize, float spacing)
        {
            if (palette == null)
                return;

            spriteRenderer.color = palette.GetColor(Type);

            float scale = cellSize * 0.85f;
            transform.localScale = new Vector3(scale, scale, 1f);

            switch (SpecialType)
            {
                case SpecialCandyType.StripedHorizontal:
                    transform.localScale = new Vector3(scale * 1.2f, scale * 0.7f, 1f);
                    break;
                case SpecialCandyType.StripedVertical:
                    transform.localScale = new Vector3(scale * 0.7f, scale * 1.2f, 1f);
                    break;
                case SpecialCandyType.Wrapped:
                    transform.localScale = new Vector3(scale * 1.1f, scale * 1.1f, 1f);
                    break;
                case SpecialCandyType.ColorBomb:
                    transform.localScale = new Vector3(scale * 1.3f, scale * 1.3f, 1f);
                    spriteRenderer.color = Color.white;
                    break;
            }
        }
    }
}
