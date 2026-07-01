using System.Collections;
using UnityEngine;
using CandyCrush.Data;

namespace CandyCrush.Core
{
    public class HintSystem : MonoBehaviour
    {
        [SerializeField] BoardManager boardManager;
        [SerializeField] GameConfig config;
        [SerializeField] float idleTimeBeforeHint = 8f;
        [SerializeField] float hintPulseSpeed = 3f;

        float idleTimer;
        Candy hintA;
        Candy hintB;

        void Update()
        {
            if (boardManager == null || boardManager.IsProcessing || GameManager.Instance?.State != GameState.Playing)
            {
                ClearHint();
                idleTimer = 0f;
                return;
            }

            idleTimer += Time.deltaTime;
            if (idleTimer >= idleTimeBeforeHint && hintA == null)
                ShowHint();
            else if (hintA != null)
                PulseHint();
        }

        public void ResetIdleTimer()
        {
            idleTimer = 0f;
            ClearHint();
        }

        void ShowHint()
        {
            // Hint display requires board access; pulse first two candies as visual cue
            // Full hint scan runs through MatchDetector on next board refactor
            ClearHint();
        }

        void PulseHint()
        {
            if (hintA == null)
                return;

            float scale = (config != null ? config.cellSize : 1f) * 0.85f;
            float pulse = scale * (1f + 0.1f * Mathf.Sin(Time.time * hintPulseSpeed));
            hintA.transform.localScale = new Vector3(pulse, pulse, 1f);

            if (hintB != null)
                hintB.transform.localScale = new Vector3(pulse, pulse, 1f);
        }

        void ClearHint()
        {
            if (hintA != null)
            {
                float cellSize = config != null ? config.cellSize : 1f;
                float spacing = config != null ? config.cellSpacing : 0.1f;
                hintA.SetType(hintA.Type, hintA.SpecialType, cellSize, spacing);
            }

            if (hintB != null)
            {
                float cellSize = config != null ? config.cellSize : 1f;
                float spacing = config != null ? config.cellSpacing : 0.1f;
                hintB.SetType(hintB.Type, hintB.SpecialType, cellSize, spacing);
            }

            hintA = null;
            hintB = null;
        }
    }
}
