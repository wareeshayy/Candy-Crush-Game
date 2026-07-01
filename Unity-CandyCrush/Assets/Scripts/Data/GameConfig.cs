using System;
using System.Collections.Generic;
using UnityEngine;
using CandyCrush.Core;

namespace CandyCrush.Data
{
    [CreateAssetMenu(fileName = "GameConfig", menuName = "Candy Crush/Game Config")]
    public class GameConfig : ScriptableObject
    {
        [Header("Easy Mode (matches original C++ 8x8)")]
        public int easyBoardSize = 8;
        public int easyCandyTypes = 5;
        public float easyTimeLimit = 60f;
        public int easyMaxMoves = 15;
        public int easyTargetScore = 500;

        [Header("Hard Mode (matches original C++ 10x10)")]
        public int hardBoardSize = 10;
        public int hardCandyTypes = 7;
        public float hardTimeLimit = 40f;
        public int hardMaxMoves = 15;
        public int hardTargetScore = 800;

        [Header("Scoring (enhanced from C++ base scores)")]
        public int match3Score = 10;
        public int match4Score = 25;
        public int match5Score = 50;
        public int lShapeScore = 35;
        public int tShapeScore = 40;
        public int comboMultiplierStep = 2;

        [Header("Animation")]
        public float swapDuration = 0.25f;
        public float fallDuration = 0.3f;
        public float destroyDuration = 0.2f;
        public float cascadeDelay = 0.15f;

        [Header("Visuals")]
        public float cellSize = 1f;
        public float cellSpacing = 0.1f;

        public DifficultySettings GetSettings(Difficulty difficulty)
        {
            return difficulty == Difficulty.Easy
                ? new DifficultySettings(easyBoardSize, easyCandyTypes, easyTimeLimit, easyMaxMoves, easyTargetScore)
                : new DifficultySettings(hardBoardSize, hardCandyTypes, hardTimeLimit, hardMaxMoves, hardTargetScore);
        }
    }

    [Serializable]
    public struct DifficultySettings
    {
        public int boardSize;
        public int candyTypeCount;
        public float timeLimit;
        public int maxMoves;
        public int targetScore;

        public DifficultySettings(int boardSize, int candyTypeCount, float timeLimit, int maxMoves, int targetScore)
        {
            this.boardSize = boardSize;
            this.candyTypeCount = candyTypeCount;
            this.timeLimit = timeLimit;
            this.maxMoves = maxMoves;
            this.targetScore = targetScore;
        }
    }

    [CreateAssetMenu(fileName = "CandyPalette", menuName = "Candy Crush/Candy Palette")]
    public class CandyPalette : ScriptableObject
    {
        public CandyVisual[] candies;

        public Color GetColor(CandyType type)
        {
            int index = (int)type;
            if (candies != null && index >= 0 && index < candies.Length)
                return candies[index].color;
            return Color.white;
        }

        public string GetLabel(CandyType type)
        {
            int index = (int)type;
            if (candies != null && index >= 0 && index < candies.Length)
                return candies[index].displayName;
            return type.ToString();
        }
    }

    [Serializable]
    public struct CandyVisual
    {
        public CandyType type;
        public string displayName;
        public Color color;
        public Sprite sprite;
    }
}
