using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MagicSeal_Collect : MonoBehaviour
{
    public string sealName;
    bool activeSeal;
    GameObject playerRef;

    // Start is called before the first frame update
    void Start()
    {
        activeSeal = true;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        if (activeSeal && other.tag == "Player")
        {
            activeSeal = false;

            playerRef = other.gameObject;

            transform.GetChild(0).gameObject.SetActive(false);
            other.transform.GetChild(1).GetChild(0).GetComponent<MeshRenderer>().enabled = true;
            other.transform.GetChild(1).GetComponent<Animator>().SetTrigger("HitGong");
            GameManager.main.currentlyTakingDamage = true;

            SoundsLibrary.PlaySoundEffectDelayed("GongHit", 1.0f);
            Invoke("HideGongSealVisual", 0.9f);

            Invoke("GongHitFinished", 2.5f);
        }
    }

    void GongHitFinished()
    {
        transform.GetChild(0).gameObject.SetActive(true);
        playerRef.transform.GetChild(1).GetChild(0).GetComponent<MeshRenderer>().enabled = false;

        GameObject.FindGameObjectWithTag("GameController").GetComponent<GameManager>().CollectedSeal(sealName);
    }

    void HideGongSealVisual()
    {
        transform.parent.GetChild(3).GetComponent<Animator>().SetTrigger("Burst");
        transform.parent.GetChild(3).GetChild(0).gameObject.SetActive(true);
    }
}
