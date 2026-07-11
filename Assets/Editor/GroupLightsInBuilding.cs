using UnityEngine;
using UnityEditor;

public class GroupLightsInBuilding
{
    [MenuItem("Tools/Group Lights In Selected Building")]
    static void GroupLights()
    {
        if (Selection.activeGameObject == null)
        {
            EditorUtility.DisplayDialog("No Selection", "Select a building GameObject first.", "OK");
            return;
        }

        GameObject building = Selection.activeGameObject;

        // Find or create Light Groups under the selected building
        Transform lightGroup = building.transform.Find("Light Groups");

        if (lightGroup == null)
        {
            GameObject group = new GameObject("Light Groups");
            Undo.RegisterCreatedObjectUndo(group, "Create Light Groups");
            group.transform.SetParent(building.transform, false);
            lightGroup = group.transform;
        }

        // Find every Light inside the selected building
        Light[] lights = building.GetComponentsInChildren<Light>(true);

        int moved = 0;

        foreach (Light light in lights)
        {
            // Don't move the Light Groups object itself if it somehow has a light
            if (light.transform == lightGroup)
                continue;

            // Skip if already inside Light Groups
            if (light.transform.parent == lightGroup)
                continue;

            Undo.SetTransformParent(light.transform, lightGroup, "Group Lights");
            moved++;
        }

        Debug.Log($"Moved {moved} lights into '{building.name}/Light Groups'.");
    }
}