using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace CandyCrush.UI
{
    public class InstructionsPanel : MonoBehaviour
    {
        [SerializeField] GameObject root;
        [SerializeField] TextMeshProUGUI bodyText;
        [SerializeField] Button closeButton;

        const string Instructions = @"HOW TO PLAY

• Click a candy, then click an adjacent candy to swap.
• Match 3 or more of the same color to score.
• Invalid swaps are rejected (unlike the C++ version).
• 4 in a row creates a striped candy.
• 5 in a row creates a color bomb.
• L-shapes create wrapped candies.
• Combos multiply your score on cascades!

EASY MODE (from original C++):
• 8×8 board, 5 candy types
• 60 seconds, 15 moves
• Target score: 500

HARD MODE (from original C++):
• 10×10 board, 7 candy types
• 40 seconds, 15 moves
• Target score: 800

Press ESC to pause during gameplay.";

        void Start()
        {
            if (bodyText != null)
                bodyText.text = Instructions;

            closeButton?.onClick.AddListener(Hide);
            Hide();
        }

        public void Show()
        {
            if (root != null)
                root.SetActive(true);
        }

        public void Hide()
        {
            if (root != null)
                root.SetActive(false);
        }
    }
}
