using Game.RenderPipelines;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum GamePhase { Real, Spirit };

public class GameManager : MonoBehaviour
{
    public static GameManager main;

    GameObject playerRef;
    GamePhase currentGamePhase;
    int currentSealCount;
    bool startedTimer = false;
    public bool timerActive = false;
    bool timerFinished = false;
    float totalTimeLeft;
    float totalTime;
    public bool gameEnded = false;
    bool pausedTimer = false;

    public bool currentlyTakingDamage = false;
    public bool currentlyHittingGong = false;
    public bool showedNoTimeLeft = false;

    public GameObject gameOverNemesisRef;
    public GameObject nemesisPrisonRef;
    public GameObject spawnedNemesis;
    bool growingGameOverNemesis = false;
    float growingAnimTime = 0;

    private void Awake()
    {
        main = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        Cursor.visible = false;
        playerRef = GameObject.FindGameObjectWithTag("Player");

        currentGamePhase = GamePhase.Real;

        currentSealCount = 0;

        totalTime = 60 * 5;
        totalTimeLeft = totalTime;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape)) Application.Quit();
        if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.R))
            UnityEngine.SceneManagement.SceneManager.LoadScene(0);


        if (startedTimer && !pausedTimer)
        {
            totalTimeLeft -= Time.deltaTime;
        }

        if (timerActive && !gameEnded)
        {
            if (totalTimeLeft <= 15)
            {
                if (!currentlyTakingDamage)
                {
                    timerActive = false;
                    if (!currentlyHittingGong && !showedNoTimeLeft)
                    {
                        StartCoroutine(ShowNoTimeLeft());
                    }
                }
            }
        }
        if (growingGameOverNemesis)
        {
            growingAnimTime += Time.deltaTime * 0.5f;
            spawnedNemesis.transform.GetChild(1).localScale = new Vector3(Mathf.Lerp(2, 2.75f, growingAnimTime), Mathf.Lerp(2, 2.75f, growingAnimTime), Mathf.Lerp(2, 2.75f, growingAnimTime));
            if (growingAnimTime >= 1)
            {
                growingGameOverNemesis = false;
            }
        }

        //Debug.Log(totalTimeLeft);

        /*
        if (Input.GetKeyDown(KeyCode.K))
        {
            PlayerGotHit(60, null);
        }*/
    }

    //// GAMEPHASES ////
    ////////////////////

    public GamePhase GetCurrentGamePhase()
    {
        return currentGamePhase;
    }

    public void SetCurrentGamePhase(GamePhase newPhase)
    {
        currentGamePhase = newPhase;
        Debug.Log("Player is now in the " + currentGamePhase + " world.");
        if (newPhase == GamePhase.Spirit)
        {
            SpiritVision.main.FadeIn();
            Messenger.Broadcast("EnteredSpiritWorld");
            SoundsLibrary.PlaySoundEffect("OnWearMaskChime");
        } else
        {
            SpiritVision.main.FadeOut();
            Messenger.Broadcast("EnteredRealWorld");
            SoundsLibrary.PlaySoundEffect("OnRemoveMaskChime");
        }
    }

    //// SEALS ////
    ///////////////

    public void CollectedSeal(string sealName)
    {
        currentSealCount++;

        switch (sealName)
        {
            case "X":
                Messenger.Broadcast("SealXCollected");
                break;
            case "Y":
                Messenger.Broadcast("SealYCollected");
                break;
            case "Z":
                Messenger.Broadcast("SealZCollected");
                break;
        }
    }

    public int GetCurrentSealCount()
    {
        return currentSealCount;
    }

    //// TIME ////
    //////////////
    
    async void LostTime(float amount)
    {
        const float deltaTime = 0.02f;
        const float duration = 1.25f;
        const float speed = 1.0f / duration;
        const float waitBefore = 6.0f;

        await System.Threading.Tasks.Task.Delay(System.TimeSpan.FromSeconds(waitBefore));

        float timer = duration;

        while (timer > 0)
        {
            await System.Threading.Tasks.Task.Delay(System.TimeSpan.FromSeconds(deltaTime));
            timer -= deltaTime;
            totalTimeLeft -= amount * speed * deltaTime;
        }
        totalTimeLeft = Mathf.Clamp(totalTimeLeft, 0, totalTime);
    }

    public float getCurrentTimeLeft()
    {
        return totalTimeLeft;
    }

    void noTimeLeft()
    {
        timerFinished = true;
        Messenger.Broadcast("NoTimeLeft");
    }

    public IEnumerator ShowNoTimeLeft()
    {
        showedNoTimeLeft = true;
        GameObject closestEncens = GetClosestIncenseToPlayer();

        //Deactivate enemy damage
        currentlyTakingDamage = true;

        //Disable Player
        playerRef.GetComponent<CharacterController>().enabled = false;
        playerRef.GetComponent<UnityStandardAssets.Characters.FirstPerson.FirstPersonController>().enabled = false;
        playerRef.GetComponent<SpiritMask>().forceCanUseMask(false);
        UI_Manager.main.fadeOutFast();

        //Switch to closest incens camera
        yield return new WaitForSeconds(2.5f);
        closestEncens.transform.GetChild(2).GetComponent<Camera>().enabled = true;
        playerRef.transform.GetChild(0).gameObject.SetActive(false);
        UI_Manager.main.QuitFadeOut();

        yield return new WaitForSeconds(3f);
        SoundsLibrary.PlaySoundEffectDelayed("Nemesis 3", 1.5f);
        closestEncens.GetComponent<Animator>().SetTrigger("NoTimeLeft");

        yield return new WaitForSeconds(2.5f);


        //UI_Manager.main.fadeOutFast();
        //yield return new WaitForSeconds(2f);
        noTimeLeft();
        yield return new WaitForSeconds(4.1f);
        closestEncens.transform.GetChild(2).GetComponent<Camera>().enabled = false;

        /*
        playerRef.transform.GetChild(0).gameObject.SetActive(true);
        yield return new WaitForSeconds(0.5f);
        UI_Manager.main.QuitFadeOut();
        playerRef.GetComponent<CharacterController>().enabled = true;
        playerRef.GetComponent<UnityStandardAssets.Characters.FirstPerson.FirstPersonController>().enabled = true;
        yield return new WaitForSeconds(5f);*/

        
    }
    
    public void SpawnGameOverNemesis()
    {
        nemesisPrisonRef.transform.GetChild(2).gameObject.SetActive(false);
        spawnedNemesis = Instantiate(gameOverNemesisRef, nemesisPrisonRef.transform.position, Quaternion.Euler(new Vector3(0, -90, 0)));
        LightSource.AssignAllLightIndices();
        StartCoroutine(GrowGameOverNemesis());
    }

    IEnumerator GrowGameOverNemesis()
    {
        yield return new WaitForSeconds(5);
        growingGameOverNemesis = true;
        yield return new WaitForSeconds(3);
        // ENCENS DISAPPEAR
        Encense.PrisonBreak();
        //yield return new WaitForSeconds(0.3f);
        nemesisPrisonRef.transform.GetChild(0).gameObject.SetActive(false);
    }

    public void StartGameTimer()
    {
        startedTimer = true;
    }

    public void PauseTimer(bool status)
    {
        pausedTimer = status;
    }

    //// PLAYER ////
    ////////////////
    
    public GameObject GetPlayerRef()
    {
        return playerRef;
    }
    
    public void PlayerGotHit(float damage, GameObject sourceEnemy)
    {
        damage = 30;
        if (!currentlyTakingDamage)
        {
            currentlyTakingDamage = true;

            //SoundsLibrary.StopAllLooped();
            SoundZone.ResetAll();

            Debug.Log("Player lost " + damage + " seconds");
            if (timerFinished)
            {

                if (spawnedNemesis != null)
                    PlayerCameraControl.LookInDirWithoutRelease(((spawnedNemesis.transform.GetChild(1).position + new Vector3(0, 0f, 0)) - playerRef.transform.position).normalized, 0.2f);
                StartCoroutine(GameOverSequence());
            }
            else
            {


                //Disable Player
                playerRef.GetComponent<CharacterController>().enabled = false;
                playerRef.GetComponent<UnityStandardAssets.Characters.FirstPerson.FirstPersonController>().enabled = false;

                //Look at enemy
                if (sourceEnemy != null)
                    PlayerCameraControl.LookInDirWithoutRelease(((sourceEnemy.transform.position + new Vector3(0, 0.6f, 0)) - playerRef.transform.position).normalized, 0.2f);
                //Damage sequence.
                StartCoroutine(DamageSequence());

            }
        }
        
    }

    IEnumerator DamageSequence()
    {
        playerRef.GetComponent<SpiritMask>().forceCanUseMask(false);

        GameObject respawnIncense = GetClosestIncenseToPlayer();
        yield return new WaitForSeconds(0.5f);
        UI_Manager.main.fadeOutInstantly();
        yield return new WaitForSeconds(1);
        if (currentGamePhase == GamePhase.Spirit)
        {
            SetCurrentGamePhase(GamePhase.Real);
            playerRef.GetComponent<SpiritMask>().GetMaskAnimController().SetTrigger("RemoveMask");
        }

        yield return new WaitForSeconds(3.5f);
        LostTime(30);
        yield return new WaitForSeconds(1.5f);

        //Teleport player and switch to incense camera
        playerRef.transform.position = respawnIncense.transform.GetChild(2).position + new Vector3(0, 0.3f, 0);
        playerRef.transform.rotation = respawnIncense.transform.GetChild(4).transform.rotation;
        PlayerCameraControl.SetPitch(30);
        playerRef.GetComponent<UnityStandardAssets.Characters.FirstPerson.FirstPersonController>().GetMouseLook().Init(GameManager.main.GetPlayerRef().transform, GameManager.main.GetPlayerRef().transform.GetChild(0));
        respawnIncense.transform.GetChild(2).GetComponent<Camera>().enabled = true;
        playerRef.transform.GetChild(0).gameObject.SetActive(false);

        UI_Manager.main.FadeBlink();
        yield return new WaitForSeconds(4);
        //Encense.BurnOneMinute();
        yield return new WaitForSeconds(3.5f);

        // Switch back to player camera and release controls.
        UI_Manager.main.fadeOutFast();
        yield return new WaitForSeconds(3);
        respawnIncense.transform.GetChild(2).GetComponent<Camera>().enabled = false;
        playerRef.transform.GetChild(0).gameObject.SetActive(true);

        if (!(totalTimeLeft <= 15))
        {
            playerRef.GetComponent<CharacterController>().enabled = true;
            playerRef.GetComponent<UnityStandardAssets.Characters.FirstPerson.FirstPersonController>().enabled = true;
        } else
        {
            PlayerCameraControl.SetPitch(27f);
        }

        UI_Manager.main.QuitFadeOut();

        playerRef.GetComponent<SpiritMask>().forceCanUseMask(true);

        if (totalTimeLeft <= 15)
        {
            timerActive = false;
            showedNoTimeLeft = true;
            noTimeLeft();
        } else
        {
            currentlyTakingDamage = false;
        }
    }

    IEnumerator GameOverSequence()
    {
        // Death screen

        // Disable player movement
        playerRef.GetComponent<CharacterController>().enabled = false;
        playerRef.GetComponent<UnityStandardAssets.Characters.FirstPerson.FirstPersonController>().enabled = false;
        playerRef.GetComponent<SpiritMask>().forceCanUseMask(false);

        // Fade out and force spirit phase.
        yield return new WaitForSeconds(0.5f);
        UI_Manager.main.fadeOutInstantly();
        
        yield return new WaitForSeconds(1);
        SoundsLibrary.FadeIn("Hell", 7.5f);
        //TP player to game over zone
        playerRef.transform.position = transform.GetChild(2).position;
        playerRef.transform.rotation = transform.GetChild(2).rotation;
        PlayerCameraControl.SetPitch(0);
        if (currentGamePhase == GamePhase.Real)
        {
            SetCurrentGamePhase(GamePhase.Spirit);
        }


        //Activate void enemies
        Messenger.Broadcast<bool>("SetVoidEnemiesActive", true);

        yield return new WaitForSeconds(5);
        UI_Manager.main.QuitFadeOut();

        //Destroy masks and hide enemies.
        yield return new WaitForSeconds(7);
        playerRef.GetComponent<SpiritMask>().DestroyMask();
        yield return new WaitForSeconds(7);
        //Messenger.Broadcast<bool>("SetVoidEnemiesActive", false);

        //yield return new WaitForSeconds(5);
        //Messenger.Broadcast("GameOverEnemyRush");

        yield return new WaitForSeconds(1.75f);
        UI_Manager.main.fadeOutInstantly();

        yield return new WaitForSeconds(3f);
        UI_Manager.main.ActivateGameOverDisplay();
        yield return new WaitForSeconds(1);
        UI_Manager.main.ActivateGameOverConfirm();



    }

    public GameObject GetClosestIncenseToPlayer()
    {
        GameObject closestIncense = null;
        foreach(GameObject IncensePot in GameObject.FindGameObjectsWithTag("Encens"))
        {
            if (closestIncense == null)
            {
                closestIncense = IncensePot;
            } else
            {
                if (Vector3.Distance(playerRef.transform.position, IncensePot.transform.position) < Vector3.Distance(playerRef.transform.position, closestIncense.transform.position))
                {
                    closestIncense = IncensePot;
                }
            }
        }

        return closestIncense;
    }

    //// CAMERAS ////
    /////////////////
    
    public void SwitchToMainDoorsCamera()
    {
        transform.GetChild(0).gameObject.SetActive(true);
    }

    public void SwitchToPlayerCamera()
    {
        transform.GetChild(0).gameObject.SetActive(false);
    }
}
