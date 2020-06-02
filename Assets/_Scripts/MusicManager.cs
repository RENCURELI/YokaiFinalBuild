using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicManager : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        //SoundsLibrary.StopLooped("BGM");
        //SoundsLibrary.PlayLooped("BGM");
        Messenger.AddListener("PlayMusic", StartBGMCallback);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void StartBGMCallback()
    {
        StartBGM();
    }

    public static void StartBGM()
    {
        SoundsLibrary.FadeIn("BGM", 3);
    }

    public static void StopBGM()
    {
        SoundsLibrary.FadeOut("BGM", 3);
    }
}
