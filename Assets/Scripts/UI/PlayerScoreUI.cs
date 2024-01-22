using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PlayerScoreUI : MonoBehaviour
{
    public ulong playerId;
    public TMP_Text playerName;
    public TMP_Text playerScore;

    public void UpdatePlayerScoreUI()
    {
        PlayerData playerData = GameMultiplayer.Instance.GetPlayerDataFromClientId(playerId);
        playerScore.SetText(Mathf.RoundToInt(playerData.score).ToString());
    }

    // Start is called before the first frame update
    void Start()
    {
        PlayerData playerData = GameMultiplayer.Instance.GetPlayerDataFromClientId(playerId);
        playerName.SetText(playerData.playerName.ToString());
    }

    // Update is called once per frame
    void Update()
    {
        UpdatePlayerScoreUI();
    }
}
