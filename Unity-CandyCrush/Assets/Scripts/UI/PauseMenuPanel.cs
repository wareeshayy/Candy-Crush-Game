using UnityEngine;
using UnityEngine.UI;
using CandyCrush.Core;

namespace CandyCrush.UI
{
    public class PauseMenuPanel : MonoBehaviour
    {
        [SerializeField] GameObject root;
        [SerializeField] Button resumeButton;
        [SerializeField] Button menuButton;
        [SerializeField] MainMenuController mainMenu;

        void Start()
        {
            resumeButton?.onClick.AddListener(Resume);
            menuButton?.onClick.AddListener(ReturnToMenu);
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

        void Resume()
        {
            GameManager.Instance?.ResumeGame();
        }

        void ReturnToMenu()
        {
            Hide();
            GameManager.Instance?.ReturnToMenu();
            mainMenu?.ShowMenu();
        }
    }
}
