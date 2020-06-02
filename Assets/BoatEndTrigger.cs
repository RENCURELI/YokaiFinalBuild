using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoatEndTrigger : MonoBehaviour
{
    bool active = true;
    GameObject playerRef;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player" && active)
        {
            active = false;

            GameManager.main.gameEnded = true;
            playerRef = other.gameObject;
            playerRef.GetComponent<SpiritMask>().forceCanUseMask(false);
            GameManager.main.SetCurrentGamePhase(GamePhase.Real);
            
            transform.parent.GetChild(0).GetChild(0).GetChild(0).gameObject.SetActive(true);

            playerRef.GetComponent<UnityStandardAssets.Characters.FirstPerson.FirstPersonController>().m_UseHeadBob = false;
            playerRef.GetComponent<CharacterController>().enabled = false;
            playerRef.transform.position = transform.position;
            playerRef.transform.rotation = transform.rotation;
            playerRef.GetComponent<UnityStandardAssets.Characters.FirstPerson.FirstPersonController>().GetMouseLook().Init(playerRef.transform, playerRef.transform.GetChild(0));

            playerRef.transform.parent = transform.parent;

            Invoke("StartEndCinematic", 2);

            Invoke("StartEndSpeech", 7);

            
            Invoke("EndTheGame", 18);
            Invoke("Quit", 24);
        }
    }

    void EndTheGame()
    {
        UI_Manager.main.fadeOut();
    }

    void Quit()
    {
        Application.Quit();
    }

    void StartEndCinematic()
    {
        transform.parent.GetComponent<Animator>().enabled = true;
    }

    void StartEndSpeech()
    {
        SoundsLibrary.PlaySoundEffect("Outro 1");
    }
}
