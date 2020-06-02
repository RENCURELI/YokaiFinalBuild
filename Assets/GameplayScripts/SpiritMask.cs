using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpiritMask : MonoBehaviour
{
    public GameObject smokePrefab;
    GameObject destroyedMaskSmoke;

    bool canUseMask; //Global on/off for the mask.
    bool maskReadyToUse = true; //Used by the cooldown
    public float cdBeforeMaskOff = 0.5f;
    Animator maskAnimController;

    float animTimeRealToSpirit = 3;
    float animTimeSpiritToReal = 2;

    GameManager gameManagerRef;

    // Start is called before the first frame update
    void Start()
    {
        gameManagerRef = GameObject.FindGameObjectWithTag("GameController").GetComponent<GameManager>();
        maskAnimController = transform.GetComponentInChildren<Animator>();

        canUseMask = false; //Doesn't have mask initially.
    }

    // Update is called once per frame
    void Update()
    {
        if (canUseMask)
        {
            if (maskReadyToUse)
            {
                if (Input.GetKeyDown(KeyCode.Mouse0) || Input.GetKeyDown(KeyCode.Mouse1))
                {
                    switch (gameManagerRef.GetCurrentGamePhase())
                    {
                        case GamePhase.Real:
                            // Transition Reel -> Esprit

                            gameManagerRef.SetCurrentGamePhase(GamePhase.Spirit);
                            maskAnimController.SetTrigger("WearMask");

                            maskReadyToUse = false;
                            Invoke("MaskIsReadyAgain", animTimeRealToSpirit + cdBeforeMaskOff);
                            break;

                        case GamePhase.Spirit:
                            // Transition Esprit -> Reel

                            gameManagerRef.SetCurrentGamePhase(GamePhase.Real);
                            maskAnimController.SetTrigger("RemoveMask");

                            maskReadyToUse = false;
                            Invoke("MaskIsReadyAgain", animTimeSpiritToReal);
                            break;
                    }
                }
            }
        }
    }

    void MaskIsReadyAgain()
    {
        maskReadyToUse = true;
    }

    public void CollectedSpiritMask()
    {
        canUseMask = true;
        Messenger.Broadcast("PlayerCollectedSpiritMask");
    }

    public void forceCanUseMask(bool status)
    {
        canUseMask = status;
    }

    public void DestroyMask()
    {
        maskAnimController.SetTrigger("DestroyMask");
        Invoke("SpawnSmokeAfterDestroyingMask", 1.5f);
    }

    void SpawnSmokeAfterDestroyingMask()
    {
        destroyedMaskSmoke = Instantiate(smokePrefab, transform.GetChild(0).GetChild(0).position, transform.GetChild(0).GetChild(0).rotation);
        Invoke("DestroySmoke", 3);
    }

    void DestroySmoke()
    {
        Destroy(destroyedMaskSmoke);
    }

    public Animator GetMaskAnimController()
    {
        return maskAnimController;
    } 


}
