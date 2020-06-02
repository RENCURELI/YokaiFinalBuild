using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public static class PlayerCameraControl
{
    private static Camera playerCamera;
    private static CharacterController playerCharacter;
    private static UnityStandardAssets.Characters.FirstPerson.FirstPersonController fpc;

    private static bool initialized = false;
    private static void Initialize()
    {
        if (initialized) return;
        Player player = GameObject.FindObjectOfType<Player>();
        playerCamera = player.GetComponentInChildren<Camera>();
        playerCharacter = player.GetComponent<CharacterController>();
        fpc = player.GetComponent<UnityStandardAssets.Characters.FirstPerson.FirstPersonController>();
    }

    public static async void LookInDir(Vector3 direction, float duration)
    {
        Initialize();

        FreezeMouseLook();

        float deltaTime = 0.01f;
        float timer = duration;

        Quaternion startRotationChar = playerCharacter.transform.localRotation;
        Quaternion endRotationChar = Quaternion.LookRotation(direction, Vector3.up);

        while (timer > 0)
        {
            timer -= deltaTime;
            float t = 1 - timer / duration;
            playerCharacter.transform.localRotation = Quaternion.Slerp(startRotationChar, endRotationChar, t);
            await Task.Delay(System.TimeSpan.FromSeconds(deltaTime));
        }

        ReleaseMouseLook();
    }

    public static async void LookInDirWithoutRelease(Vector3 direction, float duration)
    {
        Initialize();

        FreezeMouseLook();

        float deltaTime = 0.01f;
        float timer = duration;

        Quaternion startRotationChar = playerCharacter.transform.localRotation;
        Quaternion endRotationChar = Quaternion.LookRotation(direction, Vector3.up);

        while (timer > 0)
        {
            timer -= deltaTime;
            float t = 1 - timer / duration;
            playerCharacter.transform.localRotation = Quaternion.Slerp(startRotationChar, endRotationChar, t);
            await Task.Delay(System.TimeSpan.FromSeconds(deltaTime));
        }
    }

    public static void SetPitch(float pitch)
    {
        Initialize();
        playerCharacter.transform.GetChild(0).localRotation = Quaternion.Euler(new Vector3(pitch, playerCharacter.transform.GetChild(0).localRotation.y, playerCharacter.transform.GetChild(0).localRotation.z));
    }

    public static void FreezeMouseLook()
    {
        Initialize();

        fpc.enabled = false;
    }

    public static void ReleaseMouseLook()
    {
        Initialize();

        fpc.GetMouseLook().Init(playerCharacter.transform, playerCamera.transform);
        /*
        Quaternion m_CharacterTargetRot = playerCharacter.transform.localRotation;
        Quaternion m_CameraTargetRot = playerCamera.transform.localRotation;

        float yRot = Input.GetAxis("Mouse X") * 2;
        float xRot = Input.GetAxis("Mouse Y") * 2;

        m_CharacterTargetRot *= Quaternion.Euler(0f, yRot, 0f);
        m_CameraTargetRot *= Quaternion.Euler(-xRot, 0f, 0f);
        */
        fpc.enabled = true;
    }
}
