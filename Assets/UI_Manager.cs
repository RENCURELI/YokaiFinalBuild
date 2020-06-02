using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UI_Manager : MonoBehaviour
{
    public static UI_Manager main;

    public static bool playedIntro = false;

    bool maskConfirmActivated = false;
    bool maskHowToActivated = false;
    bool listeningForMaskUses = false;
    bool tutorialCancelled = false;
    bool listeningToGameOverConfirm = false;
    bool listeningToIntro1Confirm = false;
    bool listeningToIntro2Confirm = false;

    // Start is called before the first frame update
    private void Awake()
    {
        main = this;
    }
    void Start()
    {
        

    }

    // Update is called once per frame
    void Update()
    {
        if (maskConfirmActivated)
        {
            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter) || Input.GetKeyDown(KeyCode.Space))
            {
                maskConfirmActivated = false;
                transform.GetChild(2).gameObject.SetActive(false);

                Messenger.Broadcast("ConfirmedMaskHowTo");
            }
        }

        if (listeningForMaskUses)
        {
            if (Input.GetKeyDown(KeyCode.Mouse0) || Input.GetKeyDown(KeyCode.Mouse1))
            {
                tutorialCancelled = true;
                listeningForMaskUses = false;
            }
        }

        if (maskHowToActivated)
        {
            if (Input.GetKeyDown(KeyCode.Mouse0) || Input.GetKeyDown(KeyCode.Mouse1))
            {
                maskHowToActivated = false;
                DeactivateMaskHowTo();
            }
        }

        if (listeningToGameOverConfirm)
        {
            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter) || Input.GetKeyDown(KeyCode.Space))
            {
                listeningToGameOverConfirm = false;
                SoundsLibrary.StopAllLooped();
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
            }
        }

        if (listeningToIntro1Confirm)
        {
            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter) || Input.GetKeyDown(KeyCode.Space))
            {
                listeningToIntro1Confirm = false;
                StartCoroutine(ActivateIntroText2());
            }
        }

        if (listeningToIntro2Confirm)
        {
            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter) || Input.GetKeyDown(KeyCode.Space))
            {
                listeningToIntro2Confirm = false;
                Messenger.Broadcast("ConfirmedIntroFinish");
                UI_Manager.main.transform.GetChild(5).gameObject.SetActive(false);
            }
        }
    }

    public void fadeOut()
    {
        transform.GetChild(0).gameObject.SetActive(true);
        transform.GetChild(0).GetComponent<Animator>().SetTrigger("FadeOut");
    }

    public void fadeOutFast()
    {
        transform.GetChild(0).gameObject.SetActive(true);
        transform.GetChild(0).GetComponent<Animator>().SetTrigger("FadeOutFast");
    }

    public void fadeOutInstantly()
    {
        transform.GetChild(0).gameObject.SetActive(true);
        transform.GetChild(0).GetComponent<Animator>().SetTrigger("FadeOutInstant");
    }

    public void FadeBlink()
    {
        transform.GetChild(0).gameObject.SetActive(true);
        transform.GetChild(0).GetComponent<Animator>().SetTrigger("FadeBlink");
    }

    public void QuitFadeOut()
    {
        transform.GetChild(0).gameObject.SetActive(false);
    }

    public void ActivateMaskConfirm()
    {
        transform.GetChild(2).gameObject.SetActive(true);
        maskConfirmActivated = true;
    }

    public void IfUseMaskCancelTutorial()
    {
        listeningForMaskUses = true;
    }

    public void ActivateMaskHowTo()
    {
        if (!tutorialCancelled)
        {
            listeningForMaskUses = false;
            transform.GetChild(1).gameObject.SetActive(true);
            maskHowToActivated = true;
        }
    }

    void DeactivateMaskHowTo()
    {
        transform.GetChild(1).gameObject.SetActive(false);
    }

    public void ActivateGameOverDisplay()
    {
        transform.GetChild(3).gameObject.SetActive(true);
    }

    public void ActivateGameOverConfirm()
    {
        listeningToGameOverConfirm = true;
        transform.GetChild(4).gameObject.SetActive(true);
    }

    public void ActivateIntroDisplay()
    {
        if (playedIntro)
        {
            Messenger.Broadcast("ConfirmedIntroFinish");
            UI_Manager.main.transform.GetChild(5).gameObject.SetActive(false);
        }
        else
        {
            playedIntro = true;
            fadeOutInstantly();
            transform.GetChild(5).gameObject.SetActive(true);
            StartCoroutine(ActivateIntroText1());
        }
    }

    IEnumerator ActivateIntroText1()
    {
        yield return new WaitForSeconds(3);
        transform.GetChild(5).GetChild(0).gameObject.SetActive(true);
        yield return new WaitForSeconds(1);
        transform.GetChild(5).GetChild(0).GetChild(0).gameObject.SetActive(true);
        yield return new WaitForSeconds(1f);
        SoundsLibrary.PlaySoundEffect("Intro 1");
        yield return new WaitForSeconds(10);
        StartCoroutine(ActivateIntroText2());
        //transform.GetChild(5).GetChild(2).gameObject.SetActive(true);
        //listeningToIntro1Confirm = true;
    }

    IEnumerator ActivateIntroText2()
    {
        transform.GetChild(5).GetChild(1).gameObject.SetActive(true);
        transform.GetChild(5).GetChild(2).gameObject.SetActive(false);
        transform.GetChild(5).GetChild(0).gameObject.SetActive(false);
        yield return new WaitForSeconds(4.5f);

        //transform.GetChild(5).GetChild(1).gameObject.SetActive(true);
        //yield return new WaitForSeconds(1f);
        transform.GetChild(5).GetChild(1).GetChild(0).gameObject.SetActive(true);
        yield return new WaitForSeconds(1.2f);
        SoundsLibrary.PlaySoundEffect("Intro 2");
        yield return new WaitForSeconds(7f);
        SoundsLibrary.PlaySoundEffect("Intro 3");
        yield return new WaitForSeconds(3f);
        //transform.GetChild(5).GetChild(1).GetChild(0).gameObject.SetActive(true);
        //yield return new WaitForSeconds(2.5f);
        transform.GetChild(5).GetChild(2).gameObject.SetActive(true);
        listeningToIntro2Confirm = true;
    }
}
