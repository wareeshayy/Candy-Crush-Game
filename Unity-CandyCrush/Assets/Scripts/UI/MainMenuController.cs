using UnityEngine;
using UnityEngine.UI;
using TMPro;
using CandyCrush.Core;

namespace CandyCrush.UI
{
    public class MainMenuController : MonoBehaviour
    {
        [SerializeField] GameObject menuRoot;
        [SerializeField] TMP_InputField nameInput;
        [SerializeField] Button easyButton;
        [SerializeField] Button hardButton;
        [SerializeField] Button scoresButton;
        [SerializeField] Button instructionsButton;
        [SerializeField] Button quitButton;
        [SerializeField] ScoreboardPanel scoreboardPanel;
        [SerializeField] InstructionsPanel instructionsPanel;

        void Start()
        {
            easyButton?.onClick.AddListener(() => StartGame(Difficulty.Easy));
            hardButton?.onClick.AddListener(() => StartGame(Difficulty.Hard));
            scoresButton?.onClick.AddListener(ShowScores);
            instructionsButton?.onClick.AddListener(ShowInstructions);
            quitButton?.onClick.AddListener(QuitGame);

            ShowMenu();
        }

        void StartGame(Difficulty difficulty)
        {
            string name = nameInput != null ? nameInput.text : "Player";
            HideMenu();
            GameManager.Instance?.StartGame(name, difficulty);
        }

        public void ShowMenu()
        {
            if (menuRoot != null)
                menuRoot.SetActive(true);
        }

        void HideMenu()
        {
            if (menuRoot != null)
                menuRoot.SetActive(false);
        }

        void ShowScores()
        {
            scoreboardPanel?.Show();
        }

        void ShowInstructions()
        {
            instructionsPanel?.Show();
        }

        void QuitGame()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}
