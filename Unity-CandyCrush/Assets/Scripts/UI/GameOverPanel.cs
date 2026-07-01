using UnityEngine;
using UnityEngine.UI;
using TMPro;
using CandyCrush.Core;

namespace CandyCrush.UI
{
    public class GameOverPanel : MonoBehaviour
    {
        [SerializeField] GameObject root;
        [SerializeField] TextMeshProUGUI titleText;
        [SerializeField] TextMeshProUGUI scoreText;
        [SerializeField] TextMeshProUGUI detailText;
        [SerializeField] Button menuButton;
        [SerializeField] Button playAgainButton;
        [SerializeField] MainMenuController mainMenu;

        bool wasVictory;
        string lastPlayerName;

        void Start()
        {
            menuButton?.onClick.AddListener(ReturnToMenu);
            playAgainButton?.onClick.AddListener(PlayAgain);
            Hide();
        }

        public void Show(string playerName, int score, bool victory, int targetScore)
        {
            lastPlayerName = playerName;
            wasVictory = victory;

            if (root != null)
                root.SetActive(true);

            if (titleText != null)
                titleText.text = victory ? "SWEET VICTORY!" : "Game Over";

            if (scoreText != null)
                scoreText.text = $"Final Score: {score}";

            if (detailText != null)
            {
                detailText.text = victory
                    ? $"You reached the target of {targetScore}!"
                    : $"Target was {targetScore}. Try again!";
            }
        }

        public void Hide()
        {
            if (root != null)
                root.SetActive(false);
        }

        void ReturnToMenu()
        {
            Hide();
            GameManager.Instance?.ReturnToMenu();
            mainMenu?.ShowMenu();
        }

        void PlayAgain()
        {
            Hide();
            var difficulty = GameManager.Instance != null ? GameManager.Instance.CurrentDifficulty : Difficulty.Easy;
            GameManager.Instance?.StartGame(lastPlayerName, difficulty);
        }
    }
}
