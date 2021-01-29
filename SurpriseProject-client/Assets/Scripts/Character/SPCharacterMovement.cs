using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoreMountains.TopDownEngine;
using Mirror;
public class SPCharacterMovement : CharacterMovement
{
    [SerializeField] NetworkIdentity identity;
    Vector3 Magnitude = Vector3.zero;
    Vector3 _frameVelocity = Vector3.zero;

    [Header("Friction")]
    public float FrictionConstant = 1f;

    public float Mass = 1f;

    protected override void InitializeAnimatorParameters()
    {
        RegisterAnimatorParameter("velocity", AnimatorControllerParameterType.Float, out _speedAnimationParameter);
        RegisterAnimatorParameter("velocity", AnimatorControllerParameterType.Bool, out _walkingAnimationParameter);
        RegisterAnimatorParameter("Idle", AnimatorControllerParameterType.Bool, out _idleAnimationParameter);
    }
    protected override void SetMovement()
    {
        if(!identity.hasAuthority) return;
        //_movementVector = Vector3.zero;
        _currentInput = Vector2.zero;
        _frameVelocity = Vector3.zero;

        _currentInput.x = _horizontalMovement;
        _currentInput.y = _verticalMovement;

        _normalizedInput = _currentInput.normalized;

        if ((Acceleration == 0) || (Deceleration == 0))
        {
            _lerpedInput = _currentInput;
        }
        else
        {
            if (_normalizedInput.sqrMagnitude == 0)
            {
                _acceleration = Mathf.Lerp(_acceleration, 0f, Deceleration * Time.deltaTime);
                _lerpedInput = Vector2.Lerp(_lerpedInput, _lerpedInput * _acceleration, Time.deltaTime);
            }
            else
            {
                _acceleration = Mathf.Lerp(_acceleration, 1f, Acceleration * Time.deltaTime);
                _lerpedInput = AnalogInput ? Vector2.ClampMagnitude(_currentInput, _acceleration) : Vector2.ClampMagnitude(_normalizedInput, _acceleration);
            }
        }


        _acceleration = CalculateAccelation();
        float deceleration = CalculateDeceleration();

        Vector3 _friction = CalculateFriction();

        _frameVelocity.Set(_normalizedInput.x, 0, _normalizedInput.y);
        _frameVelocity *= (_acceleration) * Time.fixedDeltaTime;

        ApplyForce(_frameVelocity);
        ApplyForce(_friction);


        _movementVector = Magnitude;

        if (InterpolateMovementSpeed)
        {
            _movementSpeed = Mathf.Lerp(_movementSpeed, MovementSpeed * MovementSpeedMultiplier, _acceleration * Time.deltaTime);
        }
        else
        {
            _movementSpeed = MovementSpeed * MovementSpeedMultiplier;
        }

        _movementVector *= _movementSpeed;

        if (_movementVector.magnitude > MovementSpeed)
        {
            _movementVector = Vector3.ClampMagnitude(_movementVector, MovementSpeed);
        }

        if ((_currentInput.magnitude <= IdleThreshold) && (_controller.CurrentMovement.magnitude < IdleThreshold))
        {
            _movementVector = Vector3.zero;
        }
        //Debug.Log("Magnitude : " + Magnitude + " friction : " + _friction + " movevec : " + _movementVector);
        _controller.SetMovement(_movementVector);
        // Debug.Log(_movementVector);
    }

    Vector3 CalculateFriction()
    {
        float GroundFriction = 1;

        float N = FrictionConstant * Mass * 1f; //수직 항력 ( m * g)
        float U = 0.01f * GroundFriction; // 마찰 계수
        Vector3 RevVel = -_movementVector.normalized;
        Vector3 Friction = RevVel * N * U * 1;
        return Friction;
    }

    float CalculateAccelation()
    {
        if (_normalizedInput.sqrMagnitude > 0)
        {
            return Mathf.Lerp(0, Acceleration, Acceleration * Time.deltaTime);
        }
        return 0;
    }

    float CalculateDeceleration()
    {
        if (_normalizedInput.sqrMagnitude == 0)
        {
            return Mathf.Lerp(_acceleration, 0, Deceleration * Time.deltaTime);
        }
        return 0;
    }

    void ApplyForce(Vector3 force)
    {
        Vector3 f = force / Mass;
        Magnitude += f;
        Magnitude = Vector3.ClampMagnitude(Magnitude, 2);
    }
}
