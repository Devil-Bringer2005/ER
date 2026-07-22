using System;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class MoveForward : MonoBehaviour
{
    [SerializeField] private Vector3 velocity = new (0, 0, -5);
    private Rigidbody rb; 

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.AddForce(velocity, ForceMode.VelocityChange);
    }
}
