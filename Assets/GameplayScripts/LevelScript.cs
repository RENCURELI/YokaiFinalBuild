using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class LevelScript : MonoBehaviour
{
    GameManager gameManagerRef;
    // Start is called before the first frame update
    void Start()
    {
        gameManagerRef = GetComponent<GameManager>();

        Messenger.AddListener("PlayerEnteredTemple", PlayerEnteredForTheFirstTime);
        Messenger.AddListener("PlayerStartedCollectingMask", PlayerStartedCollectingMask);
        Messenger.AddListener("PlayerCollectedSpiritMask", PlayerCollectedMask);

        Messenger.AddListener("SealXCollected", PlayerCollectedSealX);
        Messenger.AddListener("SealYCollected", PlayerCollectedSealY);
        Messenger.AddListener("SealZCollected", PlayerCollectedSealZ);
        Messenger.AddListener("AllSealsCollected", PlayerCollectedAllSeals);

        Messenger.AddListener("PlayerEscapedTemple", End);

        Messenger.AddListener("NoTimeLeft", TimerReachedZero);

    }

    
    //// PLAYER ENTERED TEMPLE ////
    ///////////////////////////////

    void PlayerEnteredForTheFirstTime()
    {
        Messenger.Broadcast("MainDoors_Open");
        SoundsLibrary.PlaySoundEffect("MainDoorOpen");
    }

    //// PLAYER COLLECTED MASK ////
    ///////////////////////////////
    
    void PlayerStartedCollectingMask()
    {
        Messenger.RemoveListener("PlayerStartedCollectingMask", PlayerStartedCollectingMask);
        SoundsLibrary.PlaySoundEffectDelayed("MaskRaise", 2.5f);
        gameManagerRef.GetPlayerRef().GetComponent<CharacterController>().enabled = false;
        gameManagerRef.GetPlayerRef().GetComponent<UnityStandardAssets.Characters.FirstPerson.FirstPersonController>().enabled = false;
    }

    void PlayerCollectedMask()
    {
        Messenger.RemoveListener("PlayerCollectedSpiritMask", PlayerCollectedMask);
        Messenger.Broadcast("EnableEscapeTrigger");
        gameManagerRef.GetPlayerRef().GetComponent<SpiritMask>().enabled = false;

        UI_Manager.main.ActivateMaskConfirm();
        Messenger.AddListener("ConfirmedMaskHowTo", PlayerConfirmedMaskHowTo);
    }

    void PlayerConfirmedMaskHowTo()
    {
        SoundsLibrary.PlaySoundEffect("OnCollectMask");
        StartCoroutine(CollectedMaskSequence());
    }

    IEnumerator CollectedMaskSequence()
    {
        gameManagerRef.transform.GetChild(1).gameObject.SetActive(true);
        gameManagerRef.GetPlayerRef().transform.GetChild(0).gameObject.SetActive(false);
        gameManagerRef.transform.GetChild(1).GetComponent<Animator>().enabled = true;

        yield return new WaitForSeconds(0.65f);
        //GameManager.main.timerActive = true;
        Messenger.Broadcast("MainDoors_Close");
        Messenger.Broadcast("PlayMusic");
        //Messenger.Broadcast("StartEncens");
        //gameManagerRef.StartGameTimer();
        SoundsLibrary.PlaySoundEffect("MainDoorClose");

        yield return new WaitForSeconds(6f);
        UI_Manager.main.fadeOutFast();
        yield return new WaitForSeconds(2f);

        yield return new WaitForSeconds(0.5f);
        GameObject closestEncens = gameManagerRef.GetClosestIncenseToPlayer();
        closestEncens.transform.GetChild(2).GetComponent<Camera>().enabled = true;
        gameManagerRef.transform.GetChild(1).gameObject.SetActive(false);
        UI_Manager.main.QuitFadeOut();

        yield return new WaitForSeconds(1f);
        GameManager.main.timerActive = true;
        gameManagerRef.StartGameTimer();
        SoundsLibrary.PlaySoundEffectDelayed("Hurry", 0.5f);
        yield return new WaitForSeconds(4f);
        UI_Manager.main.fadeOutFast();
        yield return new WaitForSeconds(2f);

        yield return new WaitForSeconds(0.5f);
        closestEncens.transform.GetChild(2).GetComponent<Camera>().enabled = false;
        gameManagerRef.GetPlayerRef().transform.GetChild(0).gameObject.SetActive(true);
        UI_Manager.main.QuitFadeOut();
        //gameManagerRef.transform.GetChild(1).gameObject.SetActive(false);
        gameManagerRef.GetPlayerRef().GetComponent<CharacterController>().enabled = true;
        gameManagerRef.GetPlayerRef().GetComponent<UnityStandardAssets.Characters.FirstPerson.FirstPersonController>().enabled = true;
        gameManagerRef.GetPlayerRef().GetComponent<SpiritMask>().enabled = true;

        UI_Manager.main.IfUseMaskCancelTutorial();
        yield return new WaitForSeconds(1.5f);
        UI_Manager.main.ActivateMaskHowTo();

        yield return new WaitForSeconds(40f);
        MusicManager.StopBGM();
    }



    //// SEALS COLLECTED ////
    /////////////////////////

    void PlayerCollectedSealX()
    {
        gameManagerRef.currentlyTakingDamage = true;
        gameManagerRef.currentlyHittingGong = true;

        if (gameManagerRef.gameOverNemesisRef != null)
        {
            //gameManagerRef.gameOverNemesisRef.GetComponent<EnemyBehaviour>().ForceNemesisMoveToTarget(new Vector3(-1.63f, 1.93f, 0.238f));
        }

        UI_Manager.main.fadeOut();

        // Disable player movement
        GameObject.FindGameObjectWithTag("Player").GetComponent<CharacterController>().enabled = false;
        gameManagerRef.GetPlayerRef().GetComponent<UnityStandardAssets.Characters.FirstPerson.FirstPersonController>().enabled = false;

        StartCoroutine(SealXCollected());
    }

    IEnumerator SealXCollected()
    {
        gameManagerRef.PauseTimer(true);

        Vector3 playerBeginPos = gameManagerRef.GetPlayerRef().transform.position;
        Quaternion playerBeginRot = gameManagerRef.GetPlayerRef().transform.rotation;
        
        yield return new WaitForSeconds(3);
        // TEMP TP Player TO CUTSCENE CAMERA TO AVOID OCCLUSION CULLING
        gameManagerRef.GetPlayerRef().transform.position = transform.GetChild(0).position;
        gameManagerRef.GetPlayerRef().transform.rotation = transform.GetChild(0).rotation;
        gameManagerRef.GetPlayerRef().GetComponent<UnityStandardAssets.Characters.FirstPerson.FirstPersonController>().GetMouseLook().Init(gameManagerRef.GetPlayerRef().transform, gameManagerRef.GetPlayerRef().transform.GetChild(0));

        yield return new WaitForSeconds(.5f);
        // Switch cameras
        GameObject.FindGameObjectWithTag("Player").transform.GetChild(0).gameObject.SetActive(false);
        gameManagerRef.SwitchToMainDoorsCamera();

        // Show seal appearing
        UI_Manager.main.QuitFadeOut();
        yield return new WaitForSeconds(1);
        Messenger.Broadcast("ActivateXSealVisual");

        yield return new WaitForSeconds(4);

        if (gameManagerRef.GetCurrentSealCount() >= 3)
        {
            Messenger.Broadcast("AllSealsCollected");
            yield return new WaitForSeconds(3);
        }


        UI_Manager.main.fadeOutFast();
        yield return new WaitForSeconds(2);

        // Deactivate enemies
        Messenger.Broadcast("DeactivateZoneXEnemies");


        // Teleport player to entrance and activate camera.
        yield return new WaitForSeconds(2);
        if (!(gameManagerRef.GetCurrentSealCount() >= 3))
        {
            Messenger.Broadcast("TeleportPlayerToXEntrance");
        } else
        {
            //PUT PLAYER BACK AT INITIAL POS BEFORE CUTSCENE
            gameManagerRef.GetPlayerRef().transform.position = playerBeginPos;
            gameManagerRef.GetPlayerRef().transform.rotation = playerBeginRot;
            gameManagerRef.GetPlayerRef().GetComponent<UnityStandardAssets.Characters.FirstPerson.FirstPersonController>().GetMouseLook().Init(gameManagerRef.GetPlayerRef().transform, gameManagerRef.GetPlayerRef().transform.GetChild(0));
        }
        gameManagerRef.GetPlayerRef().transform.GetChild(0).gameObject.SetActive(true);
        yield return new WaitForSeconds(0.5f);
        gameManagerRef.SwitchToPlayerCamera();
        UI_Manager.main.QuitFadeOut();
        gameManagerRef.PauseTimer(false);

        // Activate lights
        yield return new WaitForSeconds(1.25f);
        Messenger.Broadcast<bool>("ActivateZoneXStatueLights", true);

        // Activate player controls
        yield return new WaitForSeconds(1.5f);


        if (gameManagerRef.getCurrentTimeLeft() <= 15 && !gameManagerRef.showedNoTimeLeft)
        {
            yield return new WaitForSeconds(2);
            StartCoroutine(gameManagerRef.ShowNoTimeLeft());
            gameManagerRef.currentlyHittingGong = false;
            gameManagerRef.currentlyTakingDamage = false;
        } else
        {
            gameManagerRef.GetPlayerRef().GetComponent<UnityStandardAssets.Characters.FirstPerson.FirstPersonController>().GetMouseLook().Init(gameManagerRef.GetPlayerRef().transform, gameManagerRef.GetPlayerRef().transform.GetChild(0));
            gameManagerRef.GetPlayerRef().GetComponent<CharacterController>().enabled = true;
            gameManagerRef.GetPlayerRef().GetComponent<UnityStandardAssets.Characters.FirstPerson.FirstPersonController>().enabled = true;

            gameManagerRef.currentlyHittingGong = false;
            gameManagerRef.currentlyTakingDamage = false;
        }

        if (gameManagerRef.gameOverNemesisRef != null)
        {
            //gameManagerRef.gameOverNemesisRef.GetComponent<EnemyBehaviour>().AwakeNemesis();
        }
    }

    void PlayerCollectedSealY()
    {
        //to avoid bugs
        gameManagerRef.currentlyTakingDamage = true;
        gameManagerRef.currentlyHittingGong = true;


        if (gameManagerRef.gameOverNemesisRef != null)
        {
            //gameManagerRef.gameOverNemesisRef.GetComponent<EnemyBehaviour>().ForceNemesisMoveToTarget(new Vector3(-1.63f, 1.93f, 0.238f));
        }

        UI_Manager.main.fadeOut();

        // Disable player movement
        gameManagerRef.GetPlayerRef().GetComponent<CharacterController>().enabled = false;
        gameManagerRef.GetPlayerRef().GetComponent<UnityStandardAssets.Characters.FirstPerson.FirstPersonController>().enabled = false;


        StartCoroutine(SealYCollected());
    }

    IEnumerator SealYCollected()
    {
        gameManagerRef.PauseTimer(true);

        Vector3 playerBeginPos = gameManagerRef.GetPlayerRef().transform.position;
        Quaternion playerBeginRot = gameManagerRef.GetPlayerRef().transform.rotation;

        yield return new WaitForSeconds(3);
        // TEMP TP Player TO CUTSCENE CAMERA TO AVOID OCCLUSION CULLING
        gameManagerRef.GetPlayerRef().transform.position = transform.GetChild(0).position;
        gameManagerRef.GetPlayerRef().transform.rotation = transform.GetChild(0).rotation;
        gameManagerRef.GetPlayerRef().GetComponent<UnityStandardAssets.Characters.FirstPerson.FirstPersonController>().GetMouseLook().Init(gameManagerRef.GetPlayerRef().transform, gameManagerRef.GetPlayerRef().transform.GetChild(0));

        yield return new WaitForSeconds(.5f);
        // Switch cameras
        GameObject.FindGameObjectWithTag("Player").transform.GetChild(0).gameObject.SetActive(false);
        gameManagerRef.SwitchToMainDoorsCamera();

        // Show seal appearing
        UI_Manager.main.QuitFadeOut();
        yield return new WaitForSeconds(1);
        Messenger.Broadcast("ActivateYSealVisual");

        yield return new WaitForSeconds(4);

        if (gameManagerRef.GetCurrentSealCount() >= 3)
        {
            Messenger.Broadcast("AllSealsCollected");
            yield return new WaitForSeconds(3);
        }


        UI_Manager.main.fadeOutFast();
        yield return new WaitForSeconds(2);

        // Deactivate enemies
        Messenger.Broadcast("DeactivateZoneYEnemies");
        

        // Teleport player to entrance and activate camera.
        yield return new WaitForSeconds(2);
        if (!(gameManagerRef.GetCurrentSealCount() >= 3))
        {
            Messenger.Broadcast("TeleportPlayerToYEntrance");
        } else
        {
            //PUT PLAYER BACK AT INITIAL POS BEFORE CUTSCENE
            gameManagerRef.GetPlayerRef().transform.position = playerBeginPos;
            gameManagerRef.GetPlayerRef().transform.rotation = playerBeginRot;
            gameManagerRef.GetPlayerRef().GetComponent<UnityStandardAssets.Characters.FirstPerson.FirstPersonController>().GetMouseLook().Init(gameManagerRef.GetPlayerRef().transform, gameManagerRef.GetPlayerRef().transform.GetChild(0));
        }
        gameManagerRef.GetPlayerRef().transform.GetChild(0).gameObject.SetActive(true);
        yield return new WaitForSeconds(0.5f);
        gameManagerRef.SwitchToPlayerCamera();
        UI_Manager.main.QuitFadeOut();
        gameManagerRef.PauseTimer(false);

        // Activate lights
        yield return new WaitForSeconds(1.25f);
        Messenger.Broadcast<bool>("ActivateZoneYStatueLights", true);

        // Activate player controls
        yield return new WaitForSeconds(1.5f);


        if (gameManagerRef.getCurrentTimeLeft() <= 15 && !gameManagerRef.showedNoTimeLeft)
        {
            yield return new WaitForSeconds(2);
            StartCoroutine(gameManagerRef.ShowNoTimeLeft());
            gameManagerRef.currentlyHittingGong = false;
            gameManagerRef.currentlyTakingDamage = false;
        } else
        {
            gameManagerRef.GetPlayerRef().GetComponent<UnityStandardAssets.Characters.FirstPerson.FirstPersonController>().GetMouseLook().Init(gameManagerRef.GetPlayerRef().transform, gameManagerRef.GetPlayerRef().transform.GetChild(0));
            gameManagerRef.GetPlayerRef().GetComponent<CharacterController>().enabled = true;
            gameManagerRef.GetPlayerRef().GetComponent<UnityStandardAssets.Characters.FirstPerson.FirstPersonController>().enabled = true;

            gameManagerRef.currentlyHittingGong = false;
            gameManagerRef.currentlyTakingDamage = false;
        }

        if (gameManagerRef.gameOverNemesisRef != null)
        {
            //gameManagerRef.gameOverNemesisRef.GetComponent<EnemyBehaviour>().AwakeNemesis();
        }
    }

    void PlayerCollectedSealZ()
    {
        gameManagerRef.currentlyTakingDamage = true;
        gameManagerRef.currentlyHittingGong = true;


        if (gameManagerRef.gameOverNemesisRef != null)
        {
            //gameManagerRef.gameOverNemesisRef.GetComponent<EnemyBehaviour>().ForceNemesisMoveToTarget(new Vector3(-1.63f, 1.93f, 0.238f));
        }

        UI_Manager.main.fadeOut();

        // Disable player movement
        GameObject.FindGameObjectWithTag("Player").GetComponent<CharacterController>().enabled = false;
        gameManagerRef.GetPlayerRef().GetComponent<UnityStandardAssets.Characters.FirstPerson.FirstPersonController>().enabled = false;

        StartCoroutine(SealZCollected());
    }

    IEnumerator SealZCollected()
    {
        gameManagerRef.PauseTimer(true);

        Vector3 playerBeginPos = gameManagerRef.GetPlayerRef().transform.position;
        Quaternion playerBeginRot = gameManagerRef.GetPlayerRef().transform.rotation;

        yield return new WaitForSeconds(3);
        // TEMP TP Player TO CUTSCENE CAMERA TO AVOID OCCLUSION CULLING
        gameManagerRef.GetPlayerRef().transform.position = transform.GetChild(0).position;
        gameManagerRef.GetPlayerRef().transform.rotation = transform.GetChild(0).rotation;
        gameManagerRef.GetPlayerRef().GetComponent<UnityStandardAssets.Characters.FirstPerson.FirstPersonController>().GetMouseLook().Init(gameManagerRef.GetPlayerRef().transform, gameManagerRef.GetPlayerRef().transform.GetChild(0));

        yield return new WaitForSeconds(.5f);
        // Switch cameras
        GameObject.FindGameObjectWithTag("Player").transform.GetChild(0).gameObject.SetActive(false);
        gameManagerRef.SwitchToMainDoorsCamera();

        // Show seal appearing
        UI_Manager.main.QuitFadeOut();
        yield return new WaitForSeconds(1);
        Messenger.Broadcast("ActivateZSealVisual");

        yield return new WaitForSeconds(4);

        if (gameManagerRef.GetCurrentSealCount() >= 3)
        {
            Messenger.Broadcast("AllSealsCollected");
            yield return new WaitForSeconds(3);
        }

        UI_Manager.main.fadeOutFast();
        yield return new WaitForSeconds(2);

        // Deactivate enemies
        Messenger.Broadcast("DeactivateZoneZEnemies");


        // Teleport player to entrance and activate camera.
        yield return new WaitForSeconds(2);
        if (!(gameManagerRef.GetCurrentSealCount() >= 3))
        {
            Messenger.Broadcast("TeleportPlayerToZEntrance");
        } else
        {
            //PUT PLAYER BACK AT INITIAL POS BEFORE CUTSCENE
            gameManagerRef.GetPlayerRef().transform.position = playerBeginPos;
            gameManagerRef.GetPlayerRef().transform.rotation = playerBeginRot;
            gameManagerRef.GetPlayerRef().GetComponent<UnityStandardAssets.Characters.FirstPerson.FirstPersonController>().GetMouseLook().Init(gameManagerRef.GetPlayerRef().transform, gameManagerRef.GetPlayerRef().transform.GetChild(0));
        }
        gameManagerRef.GetPlayerRef().transform.GetChild(0).gameObject.SetActive(true);
        yield return new WaitForSeconds(0.5f);
        gameManagerRef.SwitchToPlayerCamera();
        UI_Manager.main.QuitFadeOut();
        gameManagerRef.PauseTimer(false);

        // Activate lights
        yield return new WaitForSeconds(1.25f);
        Messenger.Broadcast<bool>("ActivateZoneZStatueLights", true);

        // Activate player controls if still time left. Else, go directly into no time left cinematic
        yield return new WaitForSeconds(1.5f);

        if (gameManagerRef.getCurrentTimeLeft() <= 15 && !gameManagerRef.showedNoTimeLeft)
        {
            yield return new WaitForSeconds(2);
            StartCoroutine(gameManagerRef.ShowNoTimeLeft());
            gameManagerRef.currentlyHittingGong = false;
            gameManagerRef.currentlyTakingDamage = false;
        } else
        {
            gameManagerRef.GetPlayerRef().GetComponent<UnityStandardAssets.Characters.FirstPerson.FirstPersonController>().GetMouseLook().Init(gameManagerRef.GetPlayerRef().transform, gameManagerRef.GetPlayerRef().transform.GetChild(0));
            gameManagerRef.GetPlayerRef().GetComponent<CharacterController>().enabled = true;
            gameManagerRef.GetPlayerRef().GetComponent<UnityStandardAssets.Characters.FirstPerson.FirstPersonController>().enabled = true;

            gameManagerRef.currentlyHittingGong = false;
            gameManagerRef.currentlyTakingDamage = false;
        }

        if (gameManagerRef.gameOverNemesisRef != null)
        {
            //gameManagerRef.gameOverNemesisRef.GetComponent<EnemyBehaviour>().AwakeNemesis();
        }
    }



    //// PLAYER ACHIEVED FINAL OBJECTIVE /////
    //////////////////////////////////////////

    void PlayerCollectedAllSeals()
    {
        Messenger.Broadcast("MainDoors_Open");
    }

    //// TIMER REACHED ZERO /////
    /////////////////////////////
    
    void TimerReachedZero()
    {
        FindObjectOfType<OnWorldChangeEnableCollision>().gameObject.layer = 8;
        gameManagerRef.GetComponent<NavMeshSurface>().BuildNavMesh();

        StartCoroutine(NoTimeLeftSequence());
    }

    IEnumerator NoTimeLeftSequence()
    {
        Vector3 playerBeginPos = gameManagerRef.GetPlayerRef().transform.position;
        Quaternion playerBeginRot = gameManagerRef.GetPlayerRef().transform.rotation;

        yield return new WaitForSeconds(2);
        UI_Manager.main.fadeOutFast();

        yield return new WaitForSeconds(2);

        //DISABLE PLAYER
        GameObject.FindGameObjectWithTag("Player").GetComponent<CharacterController>().enabled = false;
        gameManagerRef.GetPlayerRef().GetComponent<UnityStandardAssets.Characters.FirstPerson.FirstPersonController>().enabled = false;
        gameManagerRef.GetPlayerRef().transform.GetChild(0).gameObject.SetActive(false);

        // TEMP TP Player TO CUTSCENE CAMERA TO AVOID OCCLUSION CULLING
        gameManagerRef.GetPlayerRef().transform.position = gameManagerRef.transform.GetChild(3).position;
        gameManagerRef.GetPlayerRef().transform.rotation = gameManagerRef.transform.GetChild(3).rotation;
        gameManagerRef.GetPlayerRef().GetComponent<UnityStandardAssets.Characters.FirstPerson.FirstPersonController>().GetMouseLook().Init(gameManagerRef.GetPlayerRef().transform, gameManagerRef.GetPlayerRef().transform.GetChild(0));

        gameManagerRef.transform.GetChild(3).gameObject.SetActive(true);

        // SPAWN NEMESIS
        gameManagerRef.SpawnGameOverNemesis();
        Messenger.Broadcast("DeactivateZoneXEnemies");
        Messenger.Broadcast("DeactivateZoneYEnemies");
        Messenger.Broadcast("DeactivateZoneZEnemies");

        yield return new WaitForSeconds(4f);
        UI_Manager.main.QuitFadeOut();

        yield return new WaitForSeconds(2.5f);


        yield return new WaitForSeconds(6f);
        UI_Manager.main.fadeOutInstantly();
        yield return new WaitForSeconds(.5f);
        gameManagerRef.transform.GetChild(3).gameObject.SetActive(false);

        //PUT PLAYER BACK AT INITIAL POS BEFORE CUTSCENE
        gameManagerRef.GetPlayerRef().transform.position = playerBeginPos;
        gameManagerRef.GetPlayerRef().transform.rotation = playerBeginRot;
        gameManagerRef.GetPlayerRef().GetComponent<UnityStandardAssets.Characters.FirstPerson.FirstPersonController>().GetMouseLook().Init(gameManagerRef.GetPlayerRef().transform, gameManagerRef.GetPlayerRef().transform.GetChild(0));

        //ENABLE PLAYER
        gameManagerRef.GetPlayerRef().transform.GetChild(0).gameObject.SetActive(true);
        yield return new WaitForSeconds(0.5f);
        UI_Manager.main.QuitFadeOut();
        GameObject.FindGameObjectWithTag("Player").GetComponent<CharacterController>().enabled = true;
        gameManagerRef.GetPlayerRef().GetComponent<UnityStandardAssets.Characters.FirstPerson.FirstPersonController>().enabled = true;
        gameManagerRef.GetPlayerRef().GetComponent<SpiritMask>().forceCanUseMask(true);

        gameManagerRef.currentlyTakingDamage = false;

    }

    //// PLAYER ESCAPED TEMPLE /////
    ////////////////////////////////
    
    void End()
    {
        Debug.Log("You won");
    }
}
