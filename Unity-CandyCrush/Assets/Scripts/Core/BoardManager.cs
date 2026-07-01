using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CandyCrush.Core;
using CandyCrush.Data;

namespace CandyCrush.Core
{
    public class BoardManager : MonoBehaviour
    {
        [SerializeField] GameConfig config;
        [SerializeField] CandyPalette palette;
        [SerializeField] Candy candyPrefab;
        [SerializeField] Transform boardRoot;
        [SerializeField] ParticleSystem matchParticles;

        Candy[,] grid;
        CandyType?[,] logicBoard;
        SpecialCandyType[,] specialGrid;
        int boardSize;
        int candyTypeCount;
        bool isProcessing;

        public bool IsProcessing => isProcessing;
        public event Action<int, int> OnScoreChanged;
        public event Action<int> OnComboChanged;
        public event Action OnBoardSettled;

        int comboCount;
        int totalScore;

        public void Initialize(DifficultySettings settings)
        {
            boardSize = settings.boardSize;
            candyTypeCount = settings.candyTypeCount;
            comboCount = 0;
            totalScore = 0;

            ClearBoard();
            grid = new Candy[boardSize, boardSize];
            logicBoard = new CandyType?[boardSize, boardSize];
            specialGrid = new SpecialCandyType[boardSize, boardSize];

            FillBoardAvoidingMatches();
            EnsureValidBoard();
        }

        void ClearBoard()
        {
            if (boardRoot == null)
                return;

            for (int i = boardRoot.childCount - 1; i >= 0; i--)
                Destroy(boardRoot.GetChild(i).gameObject);
        }

        void FillBoardAvoidingMatches()
        {
            for (int row = 0; row < boardSize; row++)
            {
                for (int col = 0; col < boardSize; col++)
                {
                    CandyType type;
                    do
                    {
                        type = RandomCandyType();
                    } while (WouldCreateImmediateMatch(row, col, type));

                    SpawnCandy(row, col, type, SpecialCandyType.None);
                }
            }
        }

        bool WouldCreateImmediateMatch(int row, int col, CandyType type)
        {
            if (col >= 2 && logicBoard[row, col - 1] == type && logicBoard[row, col - 2] == type)
                return true;
            if (row >= 2 && logicBoard[row - 1, col] == type && logicBoard[row - 2, col] == type)
                return true;
            return false;
        }

        CandyType RandomCandyType()
        {
            return (CandyType)UnityEngine.Random.Range(0, candyTypeCount);
        }

        void SpawnCandy(int row, int col, CandyType type, SpecialCandyType special)
        {
            var pos = new Vector2Int(row, col);
            var candy = Instantiate(candyPrefab, boardRoot);
            candy.Initialize(type, special, pos, palette, config.cellSize, config.cellSpacing, boardSize);
            grid[row, col] = candy;
            logicBoard[row, col] = type;
            specialGrid[row, col] = special;
        }

        void EnsureValidBoard()
        {
            int attempts = 0;
            while (!MatchDetector.HasPossibleMoves(logicBoard) && attempts < 50)
            {
                ShuffleBoard();
                attempts++;
            }
        }

        void ShuffleBoard()
        {
            var types = new List<CandyType>();
            for (int row = 0; row < boardSize; row++)
            {
                for (int col = 0; col < boardSize; col++)
                {
                    if (logicBoard[row, col].HasValue)
                        types.Add(logicBoard[row, col].Value);
                }
            }

            for (int i = types.Count - 1; i > 0; i--)
            {
                int j = UnityEngine.Random.Range(0, i + 1);
                (types[i], types[j]) = (types[j], types[i]);
            }

            int index = 0;
            for (int row = 0; row < boardSize; row++)
            {
                for (int col = 0; col < boardSize; col++)
                {
                    var type = types[index++];
                    logicBoard[row, col] = type;
                    specialGrid[row, col] = SpecialCandyType.None;
                    grid[row, col].SetType(type, SpecialCandyType.None, config.cellSize, config.cellSpacing);
                }
            }
        }

        public void TrySwap(Vector2Int a, Vector2Int b, Action<bool> onComplete)
        {
            if (isProcessing)
            {
                onComplete?.Invoke(false);
                return;
            }

            if (!IsAdjacent(a, b) || !IsInBounds(a) || !IsInBounds(b))
            {
                onComplete?.Invoke(false);
                return;
            }

            StartCoroutine(SwapCoroutine(a, b, onComplete));
        }

        bool IsAdjacent(Vector2Int a, Vector2Int b)
        {
            return (Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y)) == 1;
        }

        bool IsInBounds(Vector2Int pos)
        {
            return pos.x >= 0 && pos.x < boardSize && pos.y >= 0 && pos.y < boardSize;
        }

        IEnumerator SwapCoroutine(Vector2Int a, Vector2Int b, Action<bool> onComplete)
        {
            isProcessing = true;

            SwapLogic(a, b);
            yield return AnimateSwap(a, b);

            var match = MatchDetector.FindMatches(logicBoard, config);
            if (!match.hasMatch)
            {
                SwapLogic(a, b);
                yield return AnimateSwap(a, b);
                isProcessing = false;
                onComplete?.Invoke(false);
                yield break;
            }

            comboCount = 0;
            onComplete?.Invoke(true);
            yield return ProcessMatchesLoop();
            isProcessing = false;
            OnBoardSettled?.Invoke();
        }

        void SwapLogic(Vector2Int a, Vector2Int b)
        {
            (logicBoard[a.x, a.y], logicBoard[b.x, b.y]) = (logicBoard[b.x, b.y], logicBoard[a.x, a.y]);
            (specialGrid[a.x, a.y], specialGrid[b.x, b.y]) = (specialGrid[b.x, b.y], specialGrid[a.x, a.y]);
            (grid[a.x, a.y], grid[b.x, b.y]) = (grid[b.x, b.y], grid[a.x, a.y]);

            grid[a.x, a.y].SetGridPosition(a, config.cellSize, config.cellSpacing);
            grid[b.x, b.y].SetGridPosition(b, config.cellSize, config.cellSpacing);
        }

        IEnumerator AnimateSwap(Vector2Int a, Vector2Int b)
        {
            var candyA = grid[a.x, a.y];
            var candyB = grid[b.x, b.y];
            var posA = candyA.transform.position;
            var posB = candyB.transform.position;
            float elapsed = 0f;

            while (elapsed < config.swapDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.SmoothStep(0f, 1f, elapsed / config.swapDuration);
                candyA.transform.position = Vector3.Lerp(posA, posB, t);
                candyB.transform.position = Vector3.Lerp(posB, posA, t);
                yield return null;
            }

            candyA.UpdateWorldPosition(config.cellSize, config.cellSpacing);
            candyB.UpdateWorldPosition(config.cellSize, config.cellSpacing);
        }

        IEnumerator ProcessMatchesLoop()
        {
            while (true)
            {
                var match = MatchDetector.FindMatches(logicBoard, config);
                if (!match.hasMatch)
                    break;

                comboCount++;
                int score = match.baseScore * (1 + (comboCount - 1) * config.comboMultiplierStep);
                totalScore += score;
                OnScoreChanged?.Invoke(totalScore, score);
                OnComboChanged?.Invoke(comboCount);

                var cellsToClear = ExpandSpecialEffects(match);
                var specialCenters = new Dictionary<Vector2Int, (CandyType type, SpecialCandyType special)>();

                foreach (var kvp in match.specialCreations)
                {
                    if (logicBoard[kvp.Key.x, kvp.Key.y].HasValue)
                    {
                        specialCenters[kvp.Key] = (logicBoard[kvp.Key.x, kvp.Key.y].Value, kvp.Value);
                        cellsToClear.Remove(kvp.Key);
                    }
                }

                foreach (var cell in cellsToClear)
                {
                    if (grid[cell.x, cell.y] != null)
                    {
                        if (matchParticles != null)
                        {
                            var ps = Instantiate(matchParticles, grid[cell.x, cell.y].transform.position, Quaternion.identity);
                            var main = ps.main;
                            main.startColor = palette.GetColor(logicBoard[cell.x, cell.y] ?? CandyType.Red);
                            Destroy(ps.gameObject, 2f);
                        }

                        Destroy(grid[cell.x, cell.y].gameObject);
                        grid[cell.x, cell.y] = null;
                        logicBoard[cell.x, cell.y] = null;
                        specialGrid[cell.x, cell.y] = SpecialCandyType.None;
                    }
                }

                foreach (var kvp in specialCenters)
                {
                    specialGrid[kvp.Key.x, kvp.Key.y] = kvp.Value.special;
                    if (grid[kvp.Key.x, kvp.Key.y] != null)
                        grid[kvp.Key.x, kvp.Key.y].SetType(kvp.Value.type, kvp.Value.special, config.cellSize, config.cellSpacing);
                }

                yield return new WaitForSeconds(config.destroyDuration);

                yield return ApplyGravity();
                yield return RefillBoard();
                yield return new WaitForSeconds(config.cascadeDelay);
            }

            comboCount = 0;
            OnComboChanged?.Invoke(0);

            if (!MatchDetector.HasPossibleMoves(logicBoard))
                ShuffleBoard();
        }

        HashSet<Vector2Int> ExpandSpecialEffects(MatchResult match)
        {
            var cells = new HashSet<Vector2Int>(match.matchedCells);

            foreach (var cell in new List<Vector2Int>(cells))
            {
                var special = specialGrid[cell.x, cell.y];
                switch (special)
                {
                    case SpecialCandyType.StripedHorizontal:
                        for (int c = 0; c < boardSize; c++)
                            cells.Add(new Vector2Int(cell.x, c));
                        break;
                    case SpecialCandyType.StripedVertical:
                        for (int r = 0; r < boardSize; r++)
                            cells.Add(new Vector2Int(r, cell.y));
                        break;
                    case SpecialCandyType.Wrapped:
                        for (int dr = -1; dr <= 1; dr++)
                        {
                            for (int dc = -1; dc <= 1; dc++)
                            {
                                int r = cell.x + dr;
                                int c = cell.y + dc;
                                if (IsInBounds(new Vector2Int(r, c)))
                                    cells.Add(new Vector2Int(r, c));
                            }
                        }
                        break;
                    case SpecialCandyType.ColorBomb:
                        var targetType = logicBoard[cell.x, cell.y];
                        if (targetType.HasValue)
                        {
                            for (int r = 0; r < boardSize; r++)
                            {
                                for (int c = 0; c < boardSize; c++)
                                {
                                    if (logicBoard[r, c] == targetType.Value)
                                        cells.Add(new Vector2Int(r, c));
                                }
                            }
                        }
                        break;
                }
            }

            return cells;
        }

        IEnumerator ApplyGravity()
        {
            bool moved = true;
            while (moved)
            {
                moved = false;
                for (int col = 0; col < boardSize; col++)
                {
                    for (int row = boardSize - 1; row >= 0; row--)
                    {
                        if (logicBoard[row, col] == null)
                        {
                            for (int above = row - 1; above >= 0; above--)
                            {
                                if (logicBoard[above, col].HasValue)
                                {
                                    logicBoard[row, col] = logicBoard[above, col];
                                    specialGrid[row, col] = specialGrid[above, col];
                                    logicBoard[above, col] = null;
                                    specialGrid[above, col] = SpecialCandyType.None;

                                    grid[row, col] = grid[above, col];
                                    grid[above, col] = null;
                                    grid[row, col].SetGridPosition(new Vector2Int(row, col), config.cellSize, config.cellSpacing);
                                    moved = true;
                                    break;
                                }
                            }
                        }
                    }
                }

                if (moved)
                    yield return AnimateFalling();
            }
        }

        IEnumerator AnimateFalling()
        {
            var moving = new List<Candy>();
            for (int row = 0; row < boardSize; row++)
            {
                for (int col = 0; col < boardSize; col++)
                {
                    if (grid[row, col] != null)
                        moving.Add(grid[row, col]);
                }
            }

            float elapsed = 0f;
            var startPositions = new Dictionary<Candy, Vector3>();
            foreach (var candy in moving)
                startPositions[candy] = candy.transform.position;

            foreach (var candy in moving)
                candy.UpdateWorldPosition(config.cellSize, config.cellSpacing);

            var targetPositions = new Dictionary<Candy, Vector3>();
            foreach (var candy in moving)
                targetPositions[candy] = candy.transform.position;

            foreach (var kvp in startPositions)
                kvp.Key.transform.position = kvp.Value;

            while (elapsed < config.fallDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.SmoothStep(0f, 1f, elapsed / config.fallDuration);
                foreach (var candy in moving)
                {
                    candy.transform.position = Vector3.Lerp(
                        startPositions[candy],
                        targetPositions[candy],
                        t
                    );
                }
                yield return null;
            }
        }

        IEnumerator RefillBoard()
        {
            for (int col = 0; col < boardSize; col++)
            {
                for (int row = 0; row < boardSize; row++)
                {
                    if (logicBoard[row, col] == null)
                    {
                        var type = RandomCandyType();
                        SpawnCandy(row, col, type, SpecialCandyType.None);

                        var candy = grid[row, col];
                        var target = candy.transform.position;
                        candy.transform.position = target + Vector3.up * 3f;

                        float elapsed = 0f;
                        var start = candy.transform.position;
                        while (elapsed < config.fallDuration)
                        {
                            elapsed += Time.deltaTime;
                            float t = Mathf.SmoothStep(0f, 1f, elapsed / config.fallDuration);
                            candy.transform.position = Vector3.Lerp(start, target, t);
                            yield return null;
                        }
                    }
                }
            }
        }

        public Candy GetCandyAt(Vector2Int pos)
        {
            if (!IsInBounds(pos))
                return null;
            return grid[pos.x, pos.y];
        }

        public int GetTotalScore() => totalScore;
    }
}
