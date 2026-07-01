using System;
using UnityEngine;
using CandyCrush.Core;
using CandyCrush.Data;
using CandyCrush.Systems;
using CandyCrush.UI;

namespace CandyCrush.Core
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [SerializeField] GameConfig config;
        [SerializeField] BoardManager boardManager;
        [SerializeField] GameHUD gameHUD;
        [SerializeField] GameOverPanel gameOverPanel;
        [SerializeField] PauseMenuPanel pauseMenuPanel;

        public GameState State { get; private set; } = GameState.Menu;
        public Difficulty CurrentDifficulty { get; private set; }
        public string PlayerName { get; private set; } = "Player";

        DifficultySettings settings;
        float timeRemaining;
        int movesRemaining;
        int lastScoreGain;

        public event Action<GameState> OnStateChanged;

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
        }

        void Start()
        {
            AudioManager.Instance?.PlayMenuMusic();
        }

        void Update()
        {
            if (State != GameState.Playing)
                return;

            timeRemaining -= Time.deltaTime;
            gameHUD?.UpdateTimer(timeRemaining);

            if (timeRemaining <= 0f)
                EndGame(false);
        }

        public void StartGame(string playerName, Difficulty difficulty)
        {
            PlayerName = string.IsNullOrWhiteSpace(playerName) ? "Player" : playerName.Trim();
            CurrentDifficulty = difficulty;
            settings = config.GetSettings(difficulty);

            timeRemaining = settings.timeLimit;
            movesRemaining = settings.maxMoves;
            lastScoreGain = 0;

            SetState(GameState.Playing);
            boardManager.OnScoreChanged += HandleScoreChanged;
            boardManager.OnComboChanged += HandleComboChanged;
            boardManager.Initialize(settings);

            gameHUD?.Show(settings, PlayerName);
            gameOverPanel?.Hide();
            pauseMenuPanel?.Hide();

            AudioManager.Instance?.PlayGameMusic();
        }

        void HandleScoreChanged(int totalScore, int gain)
        {
            lastScoreGain = gain;
            gameHUD?.UpdateScore(totalScore, gain);
            AudioManager.Instance?.PlayMatch();

            if (totalScore >= settings.targetScore)
                EndGame(true);
        }

        void HandleComboChanged(int combo)
        {
            gameHUD?.UpdateCombo(combo);
            if (combo > 1)
                AudioManager.Instance?.PlayCombo();
        }

        public void RegisterMove(bool consumed)
        {
            if (!consumed || State != GameState.Playing)
                return;

            movesRemaining--;
            gameHUD?.UpdateMoves(movesRemaining);

            if (movesRemaining <= 0)
                EndGame(boardManager.GetTotalScore() >= settings.targetScore);
        }

        void EndGame(bool victory)
        {
            if (State == GameState.GameOver || State == GameState.Victory)
                return;

            SetState(victory ? GameState.Victory : GameState.GameOver);

            boardManager.OnScoreChanged -= HandleScoreChanged;
            boardManager.OnComboChanged -= HandleComboChanged;

            int finalScore = boardManager.GetTotalScore();
            SaveManager.SaveScore(PlayerName, finalScore, CurrentDifficulty);

            if (victory)
                AudioManager.Instance?.PlayVictory();
            else
                AudioManager.Instance?.PlayGameOver();

            gameHUD?.Hide();
            gameOverPanel?.Show(PlayerName, finalScore, victory, settings.targetScore);
        }

        public void PauseGame()
        {
            if (State != GameState.Playing)
                return;

            Time.timeScale = 0f;
            SetState(GameState.Paused);
            pauseMenuPanel?.Show();
        }

        public void ResumeGame()
        {
            if (State != GameState.Paused)
                return;

            Time.timeScale = 1f;
            SetState(GameState.Playing);
            pauseMenuPanel?.Hide();
        }

        public void ReturnToMenu()
        {
            Time.timeScale = 1f;
            SetState(GameState.Menu);
            gameHUD?.Hide();
            gameOverPanel?.Hide();
            pauseMenuPanel?.Hide();
            AudioManager.Instance?.PlayMenuMusic();
        }

        void SetState(GameState newState)
        {
            State = newState;
            OnStateChanged?.Invoke(newState);
        }
    }
}
