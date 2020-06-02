using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CandleTrigger : MonoBehaviour
{

    public static List<CandleTrigger> all = new List<CandleTrigger>();

    private void OnEnable()
    {
        all.Add(this);
    }

    private void OnDisable()
    {
        all.Remove(this);
    }

    private void OnDestroy()
    {
        OnDisable();
    }
}
