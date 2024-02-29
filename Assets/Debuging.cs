using System;
using UnityEngine;

public class Debuging : MonoBehaviour
{
    [SerializeField]
    private Rigidbody rb;
    private string _gameObjectName = null;

    private void Awake()
    {
        _gameObjectName = gameObject.name;
    }

    void Update()
    {
        Debug.Log($"{transform.position} - position");
        Debug.Log($"{rb.angularVelocity} - angular velocity");
        if (_gameObjectName.Equals("WheelL"))
        {
            rb.AddTorque(Vector3.up * Mathf.Clamp(10f * Time.time, 0f, 20f));
        }
        else if (_gameObjectName.Equals("WheelR"))
        {
            rb.AddTorque(Vector3.up * Mathf.Clamp(-10f * Time.time, -20f, 0f));
        }
    }
}
