using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GamePlayingClockUI : MonoBehaviour {


    [SerializeField] private Image timerImage;
    [SerializeField] private TMP_Text timer;


    private void Update() {
        Debug.Log(GameManager.Instance.GetGamePlayingTimer());
        timerImage.fillAmount = GameManager.Instance.GetGamePlayingTimerNormalized();
        timer.SetText(Mathf.CeilToInt(GameManager.Instance.GetGamePlayingTimer()).ToString());
    }
}