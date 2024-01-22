using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

public class StatsManager : NetworkBehaviour
{
    private List<PlayerScore> playerScores;
    public float GetPlayerScore()
    {
        PlayerData playerData = GameMultiplayer.Instance.GetPlayerDataFromClientId(OwnerClientId);
        return playerData.score;
    }

    public struct PlayerScore
    {
        public ulong playerId;
        public float score;
    }
    public List<PlayerScore> GetAllPlayersScoresInGame()
    {
        List<PlayerScore> scores = new List<PlayerScore>();
        foreach (ulong id in NetworkManager.Singleton.ConnectedClientsIds)
        {
            PlayerData playerData = GameMultiplayer.Instance.GetPlayerDataFromClientId(id);
            PlayerScore score = new PlayerScore { playerId = playerData.clientId, score = playerData.score};
            scores.Add(score);
        }
        return scores;
    }

    public void ResetAllScores()
    {
       
    }

    private void Start()
    {
        StartCoroutine("SyncScores");
    }

    public IEnumerator SyncScores()
    {
        yield return new WaitForSeconds(1f);
        playerScores = GetAllPlayersScoresInGame();
    }

    /*   [ServerRpc]
       protected void SyncPlayerStatsServerRpc()
       {
           SyncPlayerStatsClientRpc(playerStats);
       }

       [ClientRpc]
       protected void SyncPlayerStatsClientRpc(PlayerStat[] syncPlayerStats)
       {
           playerStats = syncPlayerStats;
       }*/

    void Update()
    {
        
    }
}
