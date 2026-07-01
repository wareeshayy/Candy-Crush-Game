using System.Collections.Generic;
using UnityEngine;
using CandyCrush.Core;
using CandyCrush.Data;

namespace CandyCrush.Core
{
    public struct MatchResult
    {
        public HashSet<Vector2Int> matchedCells;
        public Dictionary<Vector2Int, SpecialCandyType> specialCreations;
        public int baseScore;
        public bool hasMatch;

        public static MatchResult Empty => new MatchResult
        {
            matchedCells = new HashSet<Vector2Int>(),
            specialCreations = new Dictionary<Vector2Int, SpecialCandyType>(),
            baseScore = 0,
            hasMatch = false
        };
    }

    public static class MatchDetector
    {
        public static MatchResult FindMatches(CandyType?[,] board, GameConfig config)
        {
            var result = MatchResult.Empty;
            int size = board.GetLength(0);
            var processed = new bool[size, size];

            for (int row = 0; row < size; row++)
            {
                for (int col = 0; col < size; col++)
                {
                    if (processed[row, col] || !board[row, col].HasValue)
                        continue;

                    var type = board[row, col].Value;
                    var horizontal = GetLine(board, row, col, 0, 1, type, size);
                    var vertical = GetLine(board, row, col, 1, 0, type, size);

                    if (horizontal.Count >= 3)
                        AddMatchGroup(result, horizontal, horizontal.Count, vertical.Count, processed, config);

                    if (vertical.Count >= 3)
                        AddMatchGroup(result, vertical, horizontal.Count, vertical.Count, processed, config);
                }
            }

            result.hasMatch = result.matchedCells.Count > 0;
            return result;
        }

        static List<Vector2Int> GetLine(CandyType?[,] board, int startRow, int startCol, int dRow, int dCol, CandyType type, int size)
        {
            var cells = new List<Vector2Int> { new Vector2Int(startRow, startCol) };

            for (int step = 1; step < size; step++)
            {
                int r = startRow + dRow * step;
                int c = startCol + dCol * step;
                if (r < 0 || r >= size || c < 0 || c >= size || board[r, c] != type)
                    break;
                cells.Add(new Vector2Int(r, c));
            }

            for (int step = 1; step < size; step++)
            {
                int r = startRow - dRow * step;
                int c = startCol - dCol * step;
                if (r < 0 || r >= size || c < 0 || c >= size || board[r, c] != type)
                    break;
                cells.Insert(0, new Vector2Int(r, c));
            }

            return cells;
        }

        static void AddMatchGroup(MatchResult result, List<Vector2Int> line, int hCount, int vCount, bool[,] processed, GameConfig config)
        {
            int count = line.Count;
            int match3 = config != null ? config.match3Score : 10;
            int match4 = config != null ? config.match4Score : 25;
            int match5 = config != null ? config.match5Score : 50;
            int lShape = config != null ? config.lShapeScore : 35;

            int score = count switch
            {
                >= 5 => match5,
                4 => match4,
                _ => match3
            };

            if (hCount >= 3 && vCount >= 3)
                score = Mathf.Max(score, lShape);

            result.baseScore += score;

            Vector2Int center = line[line.Count / 2];
            SpecialCandyType special = SpecialCandyType.None;

            if (count >= 5)
                special = SpecialCandyType.ColorBomb;
            else if (count == 4)
                special = line[0].x == line[1].x ? SpecialCandyType.StripedVertical : SpecialCandyType.StripedHorizontal;
            else if (hCount >= 3 && vCount >= 3)
                special = SpecialCandyType.Wrapped;

            foreach (var cell in line)
            {
                result.matchedCells.Add(cell);
                processed[cell.x, cell.y] = true;
            }

            if (special != SpecialCandyType.None && !result.specialCreations.ContainsKey(center))
                result.specialCreations[center] = special;
        }

        public static bool WouldCreateMatch(CandyType?[,] board, Vector2Int a, Vector2Int b)
        {
            if (!board[a.x, a.y].HasValue || !board[b.x, b.y].HasValue)
                return false;

            Swap(board, a, b);
            var result = FindMatches(board, null);
            Swap(board, a, b);
            return result.hasMatch;
        }

        static void Swap(CandyType?[,] board, Vector2Int a, Vector2Int b)
        {
            var temp = board[a.x, a.y];
            board[a.x, a.y] = board[b.x, b.y];
            board[b.x, b.y] = temp;
        }

        public static bool HasPossibleMoves(CandyType?[,] board)
        {
            int size = board.GetLength(0);
            for (int row = 0; row < size; row++)
            {
                for (int col = 0; col < size; col++)
                {
                    var current = new Vector2Int(row, col);
                    if (col + 1 < size && WouldCreateMatch(board, current, new Vector2Int(row, col + 1)))
                        return true;
                    if (row + 1 < size && WouldCreateMatch(board, current, new Vector2Int(row + 1, col)))
                        return true;
                }
            }
            return false;
        }
    }
}
