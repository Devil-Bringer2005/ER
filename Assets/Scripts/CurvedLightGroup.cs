using UnityEngine;
using AmazingAssets.CurvedWorld;

public class CurvedLightGroup : MonoBehaviour
{
    public CurvedWorldController curvedWorld;

    CurvedLight[] lights;

    void Awake()
    {
        lights = GetComponentsInChildren<CurvedLight>(true);
    }

    void LateUpdate()
    {
        if (curvedWorld == null)
            return;

        foreach (CurvedLight light in lights)
            light.UpdatePosition(curvedWorld);
    }
}