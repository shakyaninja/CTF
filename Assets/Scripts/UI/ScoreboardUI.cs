using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.Netcode;
using UnityEngine;

public class ScoreboardUI : NetworkBehaviour
{
    [SerializeField]private GameObject[] playerScoreUIs;
    [SerializeField] private GameObject individualPlayerScoreUI;

    // Start is called before the first frame update
    void Start()
    {
        InitializeScoreboard();
        //UpdatePlayerScoreboardUI();
    }

    private void InitializeScoreboard()
    {
        foreach (ulong playerId in NetworkManager.Singleton.ConnectedClientsIds)
        {
            GameObject playerScoreUIGO = Instantiate(individualPlayerScoreUI);
            playerScoreUIGO.GetComponent<PlayerScoreUI>().playerId = playerId;
            playerScoreUIGO.transform.SetParent(transform);
            playerScoreUIs.Append(playerScoreUIGO);
        }
    }

   /* private void UpdatePlayerScoreboardUI()
    {
        foreach (GameObject playerScore in playerScoreUIs)
        {
        }
    }*/

    // Update is called once per frame
    void Update()
    {
    }
}
