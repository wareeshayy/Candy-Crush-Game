using System.Text;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using CandyCrush.Systems;

namespace CandyCrush.UI
{
    public class ScoreboardPanel : MonoBehaviour
    {
        [SerializeField] GameObject root;
        [SerializeField] TextMeshProUGUI scoreListText;
        [SerializeField] Button closeButton;

        void Start()
        {
            closeButton?.onClick.AddListener(Hide);
            Hide();
        }

        public void Show()
        {
            if (root != null)
                root.SetActive(true);

            RefreshList();
        }

        public void Hide()
        {
            if (root != null)
                root.SetActive(false);
        }

        void RefreshList()
        {
            if (scoreListText == null)
                return;

            var data = SaveManager.LoadScoreboard();
            if (data.entries.Count == 0)
            {
                scoreListText.text = "No scores yet. Play a game!";
                return;
            }

            var sb = new StringBuilder();
            for (int i = 0; i < data.entries.Count; i++)
            {
                var entry = data.entries[i];
                sb.AppendLine($"{i + 1}. {entry.playerName} — {entry.score} ({entry.difficulty})");
                sb.AppendLine($"   {entry.date}");
            }

            scoreListText.text = sb.ToString();
        }
    }
}
