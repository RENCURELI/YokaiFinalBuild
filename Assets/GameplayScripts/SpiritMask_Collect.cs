using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpiritMask_Collect : MonoBehaviour
{
    bool activeMask = true;
    bool focusingMask = false;
    float focusingMaskAnim = 0;
    Vector3 goalPosAfterLiftAnim;

    GameObject playerRef;
    // Start is called before the first frame update
    void Start()
    {
        goalPosAfterLiftAnim = transform.position + Vector3.up * 0.7f;
    }

    // Update is called once per frame
    void Update()
    {
        if (focusingMask)
        {
            focusingMaskAnim += Time.deltaTime * 0.01f;

            //transform.position = Vector3.Lerp(transform.position, playerRef.transform.GetChild(0).GetChild(0).transform.position + playerRef.transform.up * 0.05f, focusingMaskAnim);
            transform.position = Vector3.Lerp(transform.position, goalPosAfterLiftAnim, focusingMaskAnim);
            //transform.rotation = Quaternion.Lerp(transform.rotation, playerRef.transform.GetChild(0).GetChild(0).transform.rotation * Quaternion.Euler(0, 180, 0), focusingMaskAnim);

            if (Vector3.Distance(transform.position, goalPosAfterLiftAnim) <= 0.01f)
            {
                
                Invoke("MaskLiftAnimFinished", 0.5f);
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (activeMask && other.tag == "Player")
        {
            activeMask = false;
            playerRef = other.gameObject;
            Messenger.Broadcast("PlayerStartedCollectingMask");
            PlayerCameraControl.SetPitch(15);
            PlayerCameraControl.LookInDirWithoutRelease(transform.position - playerRef.transform.position, 0.3f);
            focusingMask = true;
        }
    }

    void MaskLiftAnimFinished()
    {
        focusingMask = false;
        transform.localScale = new Vector3(0.8f, 0.8f, 0.8f);
        transform.position = playerRef.transform.GetChild(0).GetChild(0).transform.position + playerRef.transform.up * 0.1f;
        transform.rotation = playerRef.transform.GetChild(0).GetChild(0).transform.rotation * Quaternion.Euler(0, 180, 0);
        playerRef.GetComponent<SpiritMask>().CollectedSpiritMask();
        Messenger.AddListener("ConfirmedMaskHowTo", EndMaskCollect);
    }

    void EndMaskCollect()
    { 
        Destroy(this.gameObject);
    }
}
