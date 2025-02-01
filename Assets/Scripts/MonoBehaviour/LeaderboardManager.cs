using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class LeaderboardManager : MonoBehaviour
{
    public TMP_InputField playerName;
    public TMP_Text maxKills;

    public Transform statHolder;
    public GameObject prefab;

    public List<LeaderboardStat> stats = new List<LeaderboardStat>();

    public void SetNewStat()
    {
        string name = playerName.text;
        int kills = PlayerPrefs.GetInt("Kills", 0);

        SpawnStat(name, kills);
    }

    public void SaveStats()
    {
        PlayerPrefs.SetInt("length", stats.Count);
        for (int i = 0; i < stats.Count; i++)
        {
            PlayerPrefs.SetString($"PlayerName{i}", stats[i].GetName());
            PlayerPrefs.SetInt($"PlayerKills{i}", stats[i].GetKills());
        }

        foreach (var stat in stats)
        {
            Destroy(stat.gameObject);
        }
    }

    public void LoadStats()
    {
        int length = PlayerPrefs.GetInt("length", 0);
        maxKills.text = "Kills: " + PlayerPrefs.GetInt("Kills", 0);
        for (int i = 0;i < length; i++)
        {
            string playerName = PlayerPrefs.GetString($"PlayerName{i}", "");
            int playerKills = PlayerPrefs.GetInt($"PlayerKills{i}", 0);

            SpawnStat(playerName, playerKills);
        }
    }

    public void SpawnStat(string name, int kills)
    {
        LeaderboardStat stat = Instantiate(prefab, statHolder).GetComponent<LeaderboardStat>();
        stat.SetStats(name, kills);

        stats.Add(stat);
    }
}
