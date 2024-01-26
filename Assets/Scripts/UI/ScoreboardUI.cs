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
    private void Start()
    {
        InitializeScoreboard();
        GameMultiplayer.Instance.OnPlayerDataNetworkListChanged += ScoreboardUI_PlayerDataNetworkList_OnListChanged;
        //UpdatePlayerScoreboardUI();
    }

    private void ScoreboardUI_PlayerDataNetworkList_OnListChanged(object sender, System.EventArgs e)
    {

    }

    private void InitializeScoreboard()
    {
        GameObject playerScoreUIGO = Instantiate(individualPlayerScoreUI);
        playerScoreUIGO.GetComponent<PlayerScoreUI>().playerId = OwnerClientId;
        playerScoreUIGO.transform.SetParent(transform);
        playerScoreUIGO.GetComponent<NetworkObject>().Spawn();

       /* foreach (ulong playerId in GameMultiplayer.Instance.)
        {
            playerScoreUIs.Append(playerScoreUIGO);
        }*/
    }

    /* private void UpdatePlayerScoreboardUI()
     {
         foreach (GameObject playerScore in playerScoreUIs)
         {
         }
     }*/

    private void OnDestroy()
    {
        GameMultiplayer.Instance.OnPlayerDataNetworkListChanged -= ScoreboardUI_PlayerDataNetworkList_OnListChanged;
    }
}
