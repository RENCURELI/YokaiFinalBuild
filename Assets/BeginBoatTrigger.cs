using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class BeginBoatTrigger : MonoBehaviour
{
    bool active = true;
    GameObject playerRef;

    public Transform boat;

    private Vector3 boatBeginPosition;
    private Quaternion boatBeginRotation;

    private Vector3 startPosition;
    private Quaternion startRotation;

    private AsyncProcessId mainProcess = new AsyncProcessId();

    // Start is called before the first frame update
    private void Start()
    {
        UI_Manager.main.ActivateIntroDisplay();
        Messenger.AddListener("ConfirmedIntroFinish", StartBoatCutscene);
    }

    void StartBoatCutscene()
    {
        active = false;
        mainProcess = mainProcess.GetNew();
        playerRef = FindObjectOfType<Player>().gameObject;

        StartCutscene(mainProcess);

        Invoke("EnableCutsceneSkip", 5);

        //EndCutscene();
    }

    void EnableCutsceneSkip()
    {
        RegisterSkipTask(mainProcess);
    }

    private async void StartCutscene(AsyncProcessId process)
    {
        SoundsLibrary.StopAllLooped();
        await AsyncProcessId.Wait(2);

        startPosition = playerRef.transform.position;
        startRotation = playerRef.transform.rotation;
        boatBeginPosition = boat.position;
        boatBeginRotation = boat.rotation;

        playerRef.GetComponent<UnityStandardAssets.Characters.FirstPerson.FirstPersonController>().m_UseHeadBob = false;
        playerRef.GetComponent<CharacterController>().enabled = false;
        playerRef.transform.position = transform.position;
        playerRef.transform.rotation = transform.rotation;
        playerRef.transform.parent = transform.parent;
        PlayerCameraControl.SetPitch(7);

        transform.parent.GetComponent<Animator>().enabled = true;
        transform.parent.GetComponent<Animator>().SetTrigger("Begin");

        SoundsLibrary.FadeIn("BoatWaves", 1);
        SoundsLibrary.FadeIn("WindOutLoop", 1);

        PlayerCameraControl.LookInDirWithoutRelease(Vector3.forward, .2f);

        await AsyncProcessId.Wait(0.5f);
        UI_Manager.main.QuitFadeOut();

        await AsyncProcessId.Wait(20);
        if (process.Canceled) return;

        PlayerCameraControl.LookInDirWithoutRelease(Vector3.forward, 2);

        await AsyncProcessId.Wait(5);
        if (process.Canceled) return;

        EndCutscene();
    }

    private async void RegisterSkipTask(AsyncProcessId process)
    {
        const float deltaTime = 0.01f;
        while (process.valid)
        {
            await AsyncProcessId.Wait(deltaTime);
            if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter) || Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1))
            {
                UI_Manager.main.fadeOutInstantly();
                await AsyncProcessId.Wait(0.3f);
                process.Cancel();
                EndCutscene();
                await AsyncProcessId.Wait(0.4f);
                //playerRef.GetComponent<CharacterController>().enabled = true;
                
                UI_Manager.main.QuitFadeOut();
                //playerRef.GetComponent<UnityStandardAssets.Characters.FirstPerson.FirstPersonController>().enabled = true;
                return;
            }
        }
    }

    private void EndCutscene()
    {
        SoundsLibrary.FadeOut("BoatWaves", 1);
        SoundsLibrary.FadeOut("WindOutLoop", 1);

        
        playerRef.transform.parent = null;
        playerRef.transform.position = startPosition;
        playerRef.transform.rotation = startRotation;
        playerRef.GetComponent<UnityStandardAssets.Characters.FirstPerson.FirstPersonController>().GetMouseLook().Init(playerRef.transform, playerRef.transform.GetChild(0));

        playerRef.GetComponent<UnityStandardAssets.Characters.FirstPerson.FirstPersonController>().m_UseHeadBob = true;

        transform.parent.GetComponent<Animator>().enabled = false;
        transform.parent.GetComponent<Animator>().SetTrigger("End");

        boat.position = boatBeginPosition;
        boat.rotation = boatBeginRotation;
        //PlayerCameraControl.LookInDir(Vector3.right, 0.1f);
        playerRef.GetComponent<UnityStandardAssets.Characters.FirstPerson.FirstPersonController>().enabled = true;
        playerRef.GetComponent<CharacterController>().enabled = true;
        mainProcess.Cancel();
    }
}
