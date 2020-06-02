using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimStart : MonoBehaviour
{
    private Animator anim;

    void Awake()
    {
        anim = GetComponent<Animator>();
    }

    void Start()
    {
        anim.SetFloat("Offset", Random.Range(0.0f, 1.0f));
        anim.SetFloat("SpeedMult", Random.Range(0.3f, 0.6f));
        anim.Play("LanternSway");
    }
}
