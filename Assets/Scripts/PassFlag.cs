using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PassFlag : MonoBehaviour
{
    public bool isPassable = true;
    public GameObject Flag;
    
    public void ActivateFlag()
    {
        Debug.Log("activated flag ...");
        Flag.SetActive(true);
        isPassable = true;
    }

    public void DeactivateFlag() 
    {
        Debug.Log("deactivated flag ...");
        Flag.SetActive(false);
        isPassable = false;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if(collision != null && collision.gameObject.CompareTag("Player") && isPassable)
        {
            if(isPassable)
            {
                collision.gameObject.GetComponent<PassFlag>().ActivateFlag();
                DeactivateFlag();
            }
            isPassable = false;
        }
    }
}
