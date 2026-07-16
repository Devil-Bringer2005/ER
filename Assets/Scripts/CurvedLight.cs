using UnityEngine;
using AmazingAssets.CurvedWorld;

[ExecuteAlways]
[RequireComponent(typeof(Light))]
public class CurvedLight : MonoBehaviour
{
    Vector3 originalLocalPosition;
    Transform parent;

    void Awake()
    {
        parent = transform.parent;
        originalLocalPosition = transform.localPosition;
    }

    public void UpdatePosition(CurvedWorldController curvedWorld)
    {
        if (curvedWorld == null)
            return;

        Vector3 flatWorldPosition;

        if (parent != null)
            flatWorldPosition = parent.TransformPoint(originalLocalPosition);
        else
            flatWorldPosition = originalLocalPosition;

        transform.position =
            curvedWorld.TransformPosition(flatWorldPosition);
    }
}