using System.Collections;
using UnityEngine;

public class Movement : MonoBehaviour
{
    private enum Direction
    {
        Forward,
        Backward,
        Left,
        Right,
        Stop,
        None
    }

    [SerializeField]
    private Rigidbody leftWheelRigidbody;
    
    [SerializeField]
    private Rigidbody rightWheelRigidbody;
    
    [SerializeField]
    private Transform frameTransform;
    
    private Direction _direction = Direction.None;
    
    [SerializeField]
    private float maxForce;

    private void Update()
    {
        switch (_direction)
        {
            case Direction.Forward:
                MoveForward();
                break;
            case Direction.Backward:
                MoveBackward();
                break;
            case Direction.Left:
                TurnLeft();
                break;
            case Direction.Right:
                TurnRight();
                break;
            case Direction.Stop:
                StopMoving();
                break;
            case Direction.None:
                break;
        }
    }

    public void ShouldTurnRight()
    {
        _direction = Direction.Right;
    }
    
    public void ShouldStopMoving()
    {
        _direction = Direction.Stop;
    }
    
    private void StopMoving()
    {
        leftWheelRigidbody.angularVelocity = Vector3.zero;
        rightWheelRigidbody.angularVelocity = Vector3.zero;
        leftWheelRigidbody.velocity = Vector3.zero;
        rightWheelRigidbody.velocity = Vector3.zero;
        StartCoroutine(AfterStopMovingCoroutine());
    }
    
    private IEnumerator AfterStopMovingCoroutine()
    {
        yield return new WaitForSeconds(0.2f);
        if (_direction == Direction.Stop)
        {
            _direction = Direction.None;
        }
    }
    
    
    public void ShouldTurnLeft()
    {
        _direction = Direction.Left;
    }

    private void TurnRight()
    {
        leftWheelRigidbody.AddTorque(Vector3.up * Mathf.Clamp(10f * Time.time, 0f, 20f));
    }
    
    private void TurnLeft()
    {
        rightWheelRigidbody.AddTorque(Vector3.up * Mathf.Clamp(-10f * Time.time, -20f, 0f));
    }
    
    public void ShouldMoveForward()
    {
        _direction = Direction.Forward;
    }
    
    public void ShouldMoveBackward()
    {
        _direction = Direction.Backward;
    }

    private void MoveForward()
    {
        var force = frameTransform.forward * Mathf.Clamp(10f * Time.time, 0f, maxForce);
        rightWheelRigidbody.AddForce(force);
        leftWheelRigidbody.AddForce(force);
    }


    private void MoveBackward()
    {
        var force = -frameTransform.forward * Mathf.Clamp(10f * Time.time, 0f, maxForce);
        rightWheelRigidbody.AddForce(force);
        leftWheelRigidbody.AddForce(force);
    }
}
