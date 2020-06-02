using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerShaderIdentity : MonoBehaviour
{
    private int positionPropId = Shader.PropertyToID("_PlayerPosition");

    void Start()
    {
        
    }
    
    void Update()
    {
        Shader.SetGlobalVector(positionPropId, transform.position);
    }
}
