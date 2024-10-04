using System;
using UnityEngine;

public enum OriginType
{
    Wheelchair,
    Camera
}

public class DisplayOrigin : MonoBehaviour
{
    [SerializeField]
    private Transform cameraPosition;

    [SerializeField]
    private Transform wheelChairPosition;

    private Transform _transform;

    private void Awake()
    {
        _transform = transform;
    }

    public OriginType CurrentOriginType { get; set; } = OriginType.Wheelchair;

    private void Update() 
    {
        switch (CurrentOriginType)
        {
            case OriginType.Wheelchair:
                _transform.position = wheelChairPosition.position;
                _transform.rotation = wheelChairPosition.rotation;
                break;
            case OriginType.Camera:
                _transform.position = cameraPosition.position;
                _transform.rotation = cameraPosition.rotation;
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
}
