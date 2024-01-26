using Cinemachine;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.Services.Authentication;
using UnityEngine;

public class GameManager : NetworkBehaviour {


    public static GameManager Instance { get; private set; }



    public event EventHandler OnStateChanged;
    public event EventHandler OnLocalGamePaused;
    public event EventHandler OnLocalGameUnpaused;
    public event EventHandler OnMultiplayerGamePaused;
    public event EventHandler OnMultiplayerGameUnpaused;
    public event EventHandler OnLocalPlayerReadyChanged;


    private enum State {
        WaitingToStart,
        CountdownToStart,
        GamePlaying,
        GameCapturedFlagPlaying,
        RoundOver,
        GameOver,
    }


    [SerializeField] private Transform playerPrefab;


    private NetworkVariable<State> state = new NetworkVariable<State>(State.WaitingToStart);
    private bool isLocalPlayerReady;
    private NetworkVariable<float> countdownToStartTimer = new NetworkVariable<float>(3f);
    private NetworkVariable<float> gamePlayingTimer = new NetworkVariable<float>(0f);
    private float gamePlayingTimerMax = 10f;
    private bool isLocalGamePaused = false;
    private NetworkVariable<bool> isGamePaused = new NetworkVariable<bool>(false);
    private Dictionary<ulong, bool> playerReadyDictionary;
    private Dictionary<ulong, bool> playerPausedDictionary;
    private bool autoTestGamePausedState;
    [SerializeField]private CinemachineVirtualCamera virtualCamera;
    [SerializeField]private Camera mainCamera;
    private SpawnFlag spawnFlag;
    [SerializeField] private StatsManager _statsManager;


    private void Awake() {
        Instance = this;

        playerReadyDictionary = new Dictionary<ulong, bool>();
        playerPausedDictionary = new Dictionary<ulong, bool>();

    }

    [ServerRpc(RequireOwnership =false)]
    private void RespawnFlagServerRpc()
    {
        spawnFlag.spawnFlag();
    }

    public void RespawnFlag()
    {
        RespawnFlagServerRpc();
    }

    private void Start() {
        spawnFlag = GetComponent<SpawnFlag>();
        _statsManager = GetComponent<StatsManager>();
        GameInput.Instance.OnPauseAction += GameInput_OnPauseAction;
        GameInput.Instance.OnInteractAction += GameInput_OnInteractAction;
        RespawnFlag();
    }

    public void StartCapturedFlagTimer()
    {
        Debug.Log("capture flag timer started !!");
        state.Value = State.GameCapturedFlagPlaying;
    }

    public void ResetCapturedFlagTimer()
    {
        Debug.Log("Timer reset...");
        state.Value = State.GamePlaying;
    }
    public override void OnNetworkSpawn() {
        state.OnValueChanged += State_OnValueChanged;
        isGamePaused.OnValueChanged += IsGamePaused_OnValueChanged;

        if (IsServer) {
            NetworkManager.Singleton.OnClientDisconnectCallback += NetworkManager_OnClientDisconnectCallback;
            NetworkManager.Singleton.SceneManager.OnLoadEventCompleted += SceneManager_OnLoadEventCompleted;
        }
    }

    private void SceneManager_OnLoadEventCompleted(string sceneName, UnityEngine.SceneManagement.LoadSceneMode loadSceneMode, List<ulong> clientsCompleted, List<ulong> clientsTimedOut) {
        Debug.Log("Scene Loading Completed...");
        foreach (ulong clientId in NetworkManager.Singleton.ConnectedClientsIds) {
            Transform playerTransform = Instantiate(playerPrefab);
            playerTransform.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientId, true);
            Debug.Log("clientId: "+clientId);
            Debug.Log("network clientId: "+NetworkManager.Singleton.LocalClientId);
            AssignCameraToFollowPlayerClientRpc();
        }

    }

    [ClientRpc]
    private void AssignCameraToFollowPlayerClientRpc()
    {
        GameObject[] playerPrefabs = GameObject.FindGameObjectsWithTag("Player");
        Debug.Log("players: "+ playerPrefabs.Length);
        foreach (GameObject player in playerPrefabs)
        {
            Debug.Log("network clientId: "+NetworkManager.Singleton.LocalClientId);
            Debug.Log("player clientId : "+player.GetComponent<NetworkObject>().OwnerClientId+", isLocalPlayer : "+player.GetComponent<NetworkObject>().IsLocalPlayer);   
            if(player.GetComponent<NetworkObject>().IsLocalPlayer) { 
                virtualCamera.GetComponent<CinemachineVirtualCamera>().Follow = player.transform;
                virtualCamera.GetComponent<CinemachineVirtualCamera>().LookAt = player.transform;
            }
        }
    }

    private void NetworkManager_OnClientDisconnectCallback(ulong clientId) {
        autoTestGamePausedState = true;
    }

    private void IsGamePaused_OnValueChanged(bool previousValue, bool newValue) {
        if (isGamePaused.Value) {
            Time.timeScale = 0f;

            OnMultiplayerGamePaused?.Invoke(this, EventArgs.Empty);
        } else {
            Time.timeScale = 1f;

            OnMultiplayerGameUnpaused?.Invoke(this, EventArgs.Empty);
        }
    }

    private void State_OnValueChanged(State previousValue, State newValue) {
        OnStateChanged?.Invoke(this, EventArgs.Empty);
    }

    private void GameInput_OnInteractAction(object sender, EventArgs e) {
        if (state.Value == State.WaitingToStart) {
            isLocalPlayerReady = true;
            OnLocalPlayerReadyChanged?.Invoke(this, EventArgs.Empty);

            SetPlayerReadyServerRpc();
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void SetPlayerReadyServerRpc(ServerRpcParams serverRpcParams = default) {
        playerReadyDictionary[serverRpcParams.Receive.SenderClientId] = true;

        bool allClientsReady = true;
        foreach (ulong clientId in NetworkManager.Singleton.ConnectedClientsIds) {
            if (!playerReadyDictionary.ContainsKey(clientId) || !playerReadyDictionary[clientId]) {
                // This player is NOT ready
                allClientsReady = false;
                break;
            }
        }

        if (allClientsReady) {
            state.Value = State.CountdownToStart;
        }
    }

    private void GameInput_OnPauseAction(object sender, EventArgs e) {
        TogglePauseGame();
    }

    private void Update() {
        if (!IsServer) {
            return;
        }

        Debug.Log("state: "+state.Value);

        switch (state.Value) {
            case State.WaitingToStart:
                break;
            case State.CountdownToStart:
                countdownToStartTimer.Value -= Time.deltaTime;
                if (countdownToStartTimer.Value < 0f) {
                    state.Value = State.GamePlaying;
                    gamePlayingTimer.Value = gamePlayingTimerMax;
                }
                break;
            case State.GameCapturedFlagPlaying:
                gamePlayingTimer.Value -= Time.deltaTime;
                Debug.Log("game play captured time :"+gamePlayingTimer.Value);
                if (gamePlayingTimer.Value < 0f) {
                    state.Value = State.RoundOver;
                }
                break;
            case State.GamePlaying:
                gamePlayingTimer.Value = gamePlayingTimerMax;
                break;
            case State.RoundOver:
                //increment score of player whose holding the flag
                IncrementScoreOfPlayerWithFlag();

                //reset flag with all players
                GameMultiplayer.Instance.ResetPlayerHasFlag();
                ResetCapturedFlagTimer();

                //respawn flag
                RespawnFlag();

                state.Value = State.CountdownToStart;
                break;
            case State.GameOver:
                break;
        }
    }

    private void IncrementScoreOfPlayerWithFlag()
    {
        GameMultiplayer.Instance.IncrementPlayerScoreWithFlag();
    }

    private void LateUpdate() {
        if (autoTestGamePausedState) {
            autoTestGamePausedState = false;
            TestGamePausedState();
        }
    }

    public bool IsGamePlaying() {
        return state.Value == State.GamePlaying;
    }

    public bool IsCountdownToStartActive() {
        return state.Value == State.CountdownToStart;
    }

    public float GetCountdownToStartTimer() {
        return countdownToStartTimer.Value;
    }

    public bool IsGameOver() {
        return state.Value == State.GameOver;
    }

    public bool IsWaitingToStart() {
        return state.Value == State.WaitingToStart;
    }

    public bool IsLocalPlayerReady() {
        return isLocalPlayerReady;
    }

    public float GetGamePlayingTimerNormalized() {
        return 1 - (gamePlayingTimer.Value / gamePlayingTimerMax);
    }

    public float GetGamePlayingTimer()
    {
        return gamePlayingTimer.Value;
    }

    public void TogglePauseGame() {
        isLocalGamePaused = !isLocalGamePaused;
        if (isLocalGamePaused) {
            PauseGameServerRpc();

            OnLocalGamePaused?.Invoke(this, EventArgs.Empty);
        } else {
            UnpauseGameServerRpc();

            OnLocalGameUnpaused?.Invoke(this, EventArgs.Empty);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void PauseGameServerRpc(ServerRpcParams serverRpcParams = default) {
        playerPausedDictionary[serverRpcParams.Receive.SenderClientId] = true;

        TestGamePausedState();
    }

    [ServerRpc(RequireOwnership = false)]
    private void UnpauseGameServerRpc(ServerRpcParams serverRpcParams = default) {
        playerPausedDictionary[serverRpcParams.Receive.SenderClientId] = false;

        TestGamePausedState();
    }

    

    private void TestGamePausedState() {
        foreach (ulong clientId in NetworkManager.Singleton.ConnectedClientsIds) {
            if (playerPausedDictionary.ContainsKey(clientId) && playerPausedDictionary[clientId]) {
                // This player is paused
                isGamePaused.Value = true;
                return;
            }
        }

        // All players are unpaused
        isGamePaused.Value = false;
    }

}