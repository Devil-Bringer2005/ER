using System.Collections.Generic;
using UnityEngine;

public class VillageSpawner : MonoBehaviour
{
    [Header("Ground")]
    public LayerMask groundLayer;
    public LayerMask noSpawnLayer;
    public float rayHeight = 100f;

    [Header("Prefabs")]
    public GameObject[] prefabs;

    [Header("Spawn Settings")]
    public int objectCount = 100;
    public float heightOffset = 0.02f;

    [Header("Rotation")]
    public bool alignToGround = true;
    public float randomYaw = 360f;
    public float randomTilt = 2f;

    [Header("Placement")]
    public float minDistance = 8f;

    [Range(0, 90)]
    public float maxSlope = 20f;

    [Tooltip("Radius used to check NoSpawn colliders.")]
    public float noSpawnCheckRadius = 0.5f;

    [HideInInspector]
    public List<GameObject> spawnedObjects = new();
}