using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadLevel : MonoBehaviour
{
    public string[] loadOnStart;

    private void Awake()
    {
        foreach (var sceneName in loadOnStart)
        {
            if (!SceneManager.GetSceneByName(sceneName).isLoaded)
                SceneManager.LoadScene(sceneName, LoadSceneMode.Additive);
        }
        Destroy(gameObject);
    }
}
