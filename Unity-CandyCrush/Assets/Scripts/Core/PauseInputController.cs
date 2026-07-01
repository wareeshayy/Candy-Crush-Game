using UnityEngine;
using CandyCrush.Core;

namespace CandyCrush.Core
{
    public class PauseInputController : MonoBehaviour
    {
        void Update()
        {
            if (!Input.GetKeyDown(KeyCode.Escape))
                return;

            if (GameManager.Instance == null)
                return;

            switch (GameManager.Instance.State)
            {
                case GameState.Playing:
                    GameManager.Instance.PauseGame();
                    break;
                case GameState.Paused:
                    GameManager.Instance.ResumeGame();
                    break;
            }
        }
    }
}
