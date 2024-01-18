using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraActive : MonoBehaviour
{
    [SerializeField]private GameObject m_Camera;
   

    public void ActivateCamera()
    {
        m_Camera.SetActive(true);
    }

    public void DeactivateCamera()
    {
        m_Camera.SetActive(false);  
    }
}
