using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class PearlFocus : MonoBehaviour
{
    public static PearlFocus main;

    public static List<PearlFocus> all = new List<PearlFocus>();

    private static int pearlFocusPointId = Shader.PropertyToID("_PearlFocusPoint");
    private static int pearlFocusCountId = Shader.PropertyToID("_PearlFocusCount");

    // Start is called before the first frame update
    void Start()
    {
        if (!main) main = this;
        all.Add(this);
    }

    // Update is called once per frame
    void Update()
    {
        if (!main) main = this;
        if (!all.Contains(this)) all.Add(this);
        if (main == this)
        {
            List<Vector4> points = new List<Vector4>();
            for (int i = 0; i < 8; i++)
            {
                if (i < all.Count)
                {
                    if (all[i] == null) all.RemoveAt(i); 
                    else points.Add(all[i].transform.position);
                }
                else
                {
                    points.Add(Vector4.zero);
                }
            }
            Shader.SetGlobalVectorArray(pearlFocusPointId, points.ToArray());
            Shader.SetGlobalInt(pearlFocusCountId, all.Count);
        }
    }
}
