using System;
using System.Collections.Generic;
using UnityEngine;
using CandyCrush.Core;

namespace CandyCrush.Systems
{
    [Serializable]
    public class ScoreEntry
    {
        public string playerName;
        public int score;
        public string difficulty;
        public string date;
    }

    [Serializable]
    public class ScoreboardData
    {
        public List<ScoreEntry> entries = new List<ScoreEntry>();
    }

    public static class SaveManager
    {
        const string ScoreKey = "CandyCrush_Scoreboard";
        const int MaxEntries = 10;

        public static void SaveScore(string playerName, int score, Difficulty difficulty)
        {
            var data = LoadScoreboard();
            data.entries.Add(new ScoreEntry
            {
                playerName = playerName,
                score = score,
                difficulty = difficulty.ToString(),
                date = DateTime.Now.ToString("yyyy-MM-dd HH:mm")
            });

            data.entries.Sort((a, b) => b.score.CompareTo(a.score));
            if (data.entries.Count > MaxEntries)
                data.entries.RemoveRange(MaxEntries, data.entries.Count - MaxEntries);

            PlayerPrefs.SetString(ScoreKey, JsonUtility.ToJson(data));
            PlayerPrefs.Save();
        }

        public static ScoreboardData LoadScoreboard()
        {
            if (!PlayerPrefs.HasKey(ScoreKey))
                return new ScoreboardData();

            try
            {
                return JsonUtility.FromJson<ScoreboardData>(PlayerPrefs.GetString(ScoreKey));
            }
            catch
            {
                return new ScoreboardData();
            }
        }

        public static void ClearScoreboard()
        {
            PlayerPrefs.DeleteKey(ScoreKey);
            PlayerPrefs.Save();
        }
    }
}
