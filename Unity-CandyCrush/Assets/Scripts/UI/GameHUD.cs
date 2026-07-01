using UnityEngine;
using TMPro;
using CandyCrush.Data;

namespace CandyCrush.UI
{
    public class GameHUD : MonoBehaviour
    {
        [SerializeField] GameObject root;
        [SerializeField] TextMeshProUGUI playerNameText;
        [SerializeField] TextMeshProUGUI scoreText;
        [SerializeField] TextMeshProUGUI targetText;
        [SerializeField] TextMeshProUGUI timerText;
        [SerializeField] TextMeshProUGUI movesText;
        [SerializeField] TextMeshProUGUI comboText;
        [SerializeField] TextMeshProUGUI scorePopupText;

        float popupTimer;

        public void Show(DifficultySettings settings, string playerName)
        {
            if (root != null)
                root.SetActive(true);

            if (playerNameText != null)
                playerNameText.text = playerName;

            if (targetText != null)
                targetText.text = $"Target: {settings.targetScore}";

            UpdateScore(0, 0);
            UpdateTimer(settings.timeLimit);
            UpdateMoves(settings.maxMoves);
            UpdateCombo(0);
        }

        public void Hide()
        {
            if (root != null)
                root.SetActive(false);
        }

        public void UpdateScore(int total, int gain)
        {
            if (scoreText != null)
                scoreText.text = $"Score: {total}";

            if (gain > 0 && scorePopupText != null)
            {
                scorePopupText.text = $"+{gain}";
                scorePopupText.gameObject.SetActive(true);
                popupTimer = 1f;
            }
        }

        public void UpdateTimer(float seconds)
        {
            if (timerText == null)
                return;

            seconds = Mathf.Max(0f, seconds);
            int mins = Mathf.FloorToInt(seconds / 60f);
            int secs = Mathf.FloorToInt(seconds % 60f);
            timerText.text = $"Time: {mins:00}:{secs:00}";
            timerText.color = seconds <= 10f ? Color.red : Color.white;
        }

        public void UpdateMoves(int moves)
        {
            if (movesText != null)
                movesText.text = $"Moves: {moves}";
        }

        public void UpdateCombo(int combo)
        {
            if (comboText == null)
                return;

            comboText.gameObject.SetActive(combo > 1);
            comboText.text = combo > 1 ? $"COMBO x{combo}!" : string.Empty;
        }

        void Update()
        {
            if (popupTimer > 0f)
            {
                popupTimer -= Time.deltaTime;
                if (popupTimer <= 0f && scorePopupText != null)
                    scorePopupText.gameObject.SetActive(false);
            }
        }
    }
}
