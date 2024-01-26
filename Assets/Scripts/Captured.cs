using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class Captured : NetworkBehaviour
{
    [SerializeField] private Vector3 TopOffset;

    private void OnTriggerEnter(Collider other)
    {
        if(other != null && other.CompareTag("Player"))
        {
            /*gameObject.transform.SetParent(other.gameObject.transform);
            gameObject.transform.localPosition = Vector3.zero + TopOffset;
            gameObject.transform.localRotation = Quaternion.identity;  */ 
            other.GetComponent<SafeSide>().hasFlag = true;
            other.GetComponent<PassFlag>().ActivateFlag();
            //also change player data
            GameMultiplayer.Instance.ChangePlayerHasFlag(true);
            GameManager.Instance.StartCapturedFlagTimer();
            Destroy(gameObject);
        }
    }
}
