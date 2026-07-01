using UnityEngine;
using UnityEngine.EventSystems;
using CandyCrush.Data;

namespace CandyCrush.Core
{
    public class InputHandler : MonoBehaviour
    {
        [SerializeField] Camera mainCamera;
        [SerializeField] BoardManager boardManager;
        [SerializeField] GameConfig config;
        [SerializeField] LayerMask candyLayer;

        Candy selectedCandy;
        Vector2Int? selectedGridPos;

        void Update()
        {
            if (boardManager == null || boardManager.IsProcessing)
                return;

            if (Input.GetMouseButtonDown(0) && !EventSystem.current.IsPointerOverGameObject())
                HandleClick();
        }

        void HandleClick()
        {
            var worldPos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
            var hit = Physics2D.Raycast(worldPos, Vector2.zero, 0f, candyLayer);

            if (!hit.collider)
            {
                Deselect();
                return;
            }

            var candy = hit.collider.GetComponent<Candy>();
            if (candy == null)
                return;

            if (selectedCandy == null)
            {
                Select(candy);
                return;
            }

            if (selectedCandy == candy)
            {
                Deselect();
                return;
            }

            var targetPos = candy.GridPosition;
            var sourcePos = selectedCandy.GridPosition;

            boardManager.TrySwap(sourcePos, targetPos, success =>
            {
                if (success)
                {
                    Systems.AudioManager.Instance?.PlaySwap();
                    GameManager.Instance?.RegisterMove(true);
                }
                else
                {
                    Systems.AudioManager.Instance?.PlayInvalidSwap();
                }
            });

            Deselect();
            FindObjectOfType<HintSystem>()?.ResetIdleTimer();
        }

        void Select(Candy candy)
        {
            selectedCandy = candy;
            selectedGridPos = candy.GridPosition;
            float scale = (config != null ? config.cellSize : 1f) * 0.85f * 1.15f;
            candy.transform.localScale = new Vector3(scale, scale, 1f);
        }

        void Deselect()
        {
            if (selectedCandy != null)
            {
                float cellSize = config != null ? config.cellSize : 1f;
                float spacing = config != null ? config.cellSpacing : 0.1f;
                selectedCandy.SetType(selectedCandy.Type, selectedCandy.SpecialType, cellSize, spacing);
            }

            selectedCandy = null;
            selectedGridPos = null;
        }
    }
}
