using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SealVisuals : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        Messenger.AddListener("InitializeDoorSeals", InitializeAllSeals);

        Messenger.AddListener("ActivateXSealVisual", ActivateXSealVisual);
        Messenger.AddListener("ActivateYSealVisual", ActivateYSealVisual);
        Messenger.AddListener("ActivateZSealVisual", ActivateZSealVisual);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    
    void InitializeAllSeals()
    {

    }

    void ActivateXSealVisual()
    {
        Debug.Log("X Seal Visual activated");
    }

    void ActivateYSealVisual()
    {
        Debug.Log("Y Seal Visual activated");
    }

    void ActivateZSealVisual()
    {
        Debug.Log("Z Seal Visual activated");
    }
}
