using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PassFlag : MonoBehaviour
{
    public bool isPassable = true;
    public GameObject Flag;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnCollisionEnter(Collision collision)
    {
        if(collision != null && collision.gameObject.CompareTag("Player") && isPassable)
        {
            if(isPassable)
            {
                collision.gameObject.GetComponent<PassFlag>().Flag.SetActive(true);
                collision.gameObject.GetComponent<PassFlag>().isPassable = true;
                Flag.SetActive(false);
                isPassable = false;
            }
            /*Transform flag = collision.transform.GetChild(0);
            flag.SetParent(gameObject.transform);
            flag.localPosition = Vector3.zero + new Vector3(0,3,0);*/
            isPassable = false;
        }
    }
}
