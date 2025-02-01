using TMPro;
using UnityEngine;

public class LeaderboardStat : MonoBehaviour
{
    public TMP_Text playerNameText;
    public TMP_Text killsText;

    private string playerName;
    private int kills;

    public void SetStats(string name, int kills)
    {
        playerNameText.text = name;
        killsText.text = "Kills: " + kills;
        this.kills = kills;
        this.playerName = name;
    }

    public string GetName()
    {
        return playerName;
    }

    public int GetKills()
    {
        return kills;
    }
}
