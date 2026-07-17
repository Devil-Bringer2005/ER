using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

[CustomEditor(typeof(VillageSpawner))]
public class VillageSpawnerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        VillageSpawner spawner = (VillageSpawner)target;

        GUILayout.Space(10);

        if (GUILayout.Button("Spawn Houses"))
        {
            Spawn(spawner);
        }

        if (GUILayout.Button("Clear Spawned Houses"))
        {
            Clear(spawner);
        }
    }

    void Spawn(VillageSpawner spawner)
    {
        Collider ground = spawner.GetComponent<Collider>();

        if (ground == null)
        {
            Debug.LogError("No Collider found on the terrain.");
            return;
        }

        if (spawner.prefabs == null || spawner.prefabs.Length == 0)
        {
            Debug.LogError("No prefabs assigned.");
            return;
        }

        Bounds bounds = ground.bounds;

        List<Vector3> usedPositions = new();

        Undo.IncrementCurrentGroup();
        Undo.SetCurrentGroupName("Spawn Houses");

        for (int i = 0; i < spawner.objectCount; i++)
        {
            bool placed = false;

            for (int attempt = 0; attempt < 50; attempt++)
            {
                Vector3 randomPoint = new Vector3(
                    Random.Range(bounds.min.x, bounds.max.x),
                    bounds.max.y + spawner.rayHeight,
                    Random.Range(bounds.min.z, bounds.max.z));

                if (!Physics.Raycast(
                        randomPoint,
                        Vector3.down,
                        out RaycastHit hit,
                        spawner.rayHeight * 2f,
                        spawner.groundLayer))
                {
                    continue;
                }

                // Skip road/path blockers
                if (Physics.CheckSphere(
                        hit.point + Vector3.up * 0.1f,
                        spawner.noSpawnCheckRadius,
                        spawner.noSpawnLayer,
                        QueryTriggerInteraction.Ignore))
                {
                    continue;
                }

                float slope = Vector3.Angle(hit.normal, Vector3.up);

                if (slope > spawner.maxSlope)
                    continue;

                bool overlap = false;

                foreach (Vector3 pos in usedPositions)
                {
                    if (Vector3.Distance(pos, hit.point) < spawner.minDistance)
                    {
                        overlap = true;
                        break;
                    }
                }

                if (overlap)
                    continue;

                GameObject prefab = spawner.prefabs[Random.Range(0, spawner.prefabs.Length)];

                GameObject obj = (GameObject)PrefabUtility.InstantiatePrefab(prefab);

                Undo.RegisterCreatedObjectUndo(obj, "Spawn House");

                Quaternion rotation = Quaternion.identity;

                if (spawner.alignToGround)
                    rotation = Quaternion.FromToRotation(Vector3.up, hit.normal);

                rotation *= Quaternion.Euler(
                    0f,
                    Random.Range(0f, spawner.randomYaw),
                    0f);

                rotation *= Quaternion.Euler(
                    Random.Range(-spawner.randomTilt, spawner.randomTilt),
                    0f,
                    Random.Range(-spawner.randomTilt, spawner.randomTilt));

                obj.transform.position = hit.point + hit.normal * spawner.heightOffset;
                obj.transform.rotation = rotation;
                obj.transform.SetParent(spawner.transform);

                spawner.spawnedObjects.Add(obj);
                usedPositions.Add(hit.point);

                placed = true;
                break;
            }

            if (!placed)
            {
                Debug.LogWarning($"Failed to place house {i + 1}");
            }
        }

        EditorUtility.SetDirty(spawner);
    }

    void Clear(VillageSpawner spawner)
    {
        for (int i = spawner.spawnedObjects.Count - 1; i >= 0; i--)
        {
            if (spawner.spawnedObjects[i] != null)
            {
                Undo.DestroyObjectImmediate(spawner.spawnedObjects[i]);
            }
        }

        spawner.spawnedObjects.Clear();

        EditorUtility.SetDirty(spawner);
    }
}