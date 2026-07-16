using UnityEngine;
using System.Collections.Generic;
using AmazingAssets.CurvedWorld;

[ExecuteAlways]
public class CurvedWorldLightManager : MonoBehaviour
{
    public CurvedWorldController curvedWorld;

    public const int MaxLights = 32;

    public static readonly List<Light> Lights = new();

    private readonly Vector4[] positions = new Vector4[MaxLights];
    private readonly Vector4[] colors = new Vector4[MaxLights];

    public static void RegisterLight(Light light)
    {
        if (light == null)
            return;

        if (light.type != LightType.Point)
            return;

        if (!Lights.Contains(light))
            Lights.Add(light);
    }

    public static void UnregisterLight(Light light)
    {
        Lights.Remove(light);
    }

    void Update()
    {
        if (curvedWorld == null)
            return;

        // Remove destroyed lights
        for (int i = Lights.Count - 1; i >= 0; i--)
        {
            if (Lights[i] == null)
                Lights.RemoveAt(i);
        }

        int count = 0;

        for (int i = 0; i < Lights.Count && count < MaxLights; i++)
        {
            Light l = Lights[i];

            if (l == null)
                continue;

            if (!l.enabled)
                continue;

            if (!l.gameObject.activeInHierarchy)
                continue;

            if (l.lightmapBakeType == LightmapBakeType.Baked)
                continue;

            Vector3 curvedPos =
                curvedWorld.TransformPosition(l.transform.position);

            positions[count] = new Vector4(
                curvedPos.x,
                curvedPos.y,
                curvedPos.z,
                l.range
            );

            Color c = l.color * l.intensity;

            colors[count] = new Vector4(
                c.r,
                c.g,
                c.b,
                1
            );

            count++;
        }

        Shader.SetGlobalInt("_CurvedLightCount", count);
        Shader.SetGlobalVectorArray("_CurvedLightPositions", positions);
        Shader.SetGlobalVectorArray("_CurvedLightColors", colors);
    }
}