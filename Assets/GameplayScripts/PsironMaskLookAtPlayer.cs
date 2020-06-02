using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PsironMaskLookAtPlayer : MonoBehaviour
{
    GameObject maskRef;
    GameObject playerRef;
    bool playerInRange = false;
    float detectionHeightLimit = 4;
    float detectionAngle = 65;
    Quaternion maskInitialRot;

    // Start is called before the first frame update
    void Start()
    {
        maskRef = transform.GetChild(2).gameObject;
        maskInitialRot = maskRef.transform.rotation;
    }

    // Update is called once per frame
    void Update()
    {
        if (playerInRange)
        {
            if (maskRef.activeSelf)
            {
                Vector3 toPlayer = playerRef.transform.position - transform.position;
                float heightDiff = Mathf.Abs(toPlayer.y);
                toPlayer.y = 0;
                float angle = Vector3.Angle(transform.forward, toPlayer.normalized);
                if (heightDiff <= detectionHeightLimit && angle <= detectionAngle)
                {
                    Debug.Log("dfdsfds");
                    maskRef.transform.rotation = Quaternion.Lerp(maskRef.transform.rotation, Quaternion.LookRotation(toPlayer, maskRef.transform.up), Time.deltaTime);
                } else
                {
                    maskRef.transform.rotation = Quaternion.Lerp(maskRef.transform.rotation, maskInitialRot, Time.deltaTime * 0.5f);
                }
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player")
        {
            playerRef = other.gameObject;
            playerInRange = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.tag == "Player")
        {
            playerInRange = false;
        }
    }
}
