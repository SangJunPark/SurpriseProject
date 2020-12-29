using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoreMountains.TopDownEngine;

public class SPCharacterMovement : CharacterMovement
{
    Vector3 Magnitude = Vector3.zero;

    public float Mass = 1f;

    protected override void InitializeAnimatorParameters()
    {
        RegisterAnimatorParameter("velocity", AnimatorControllerParameterType.Float, out _speedAnimationParameter);
        RegisterAnimatorParameter("velocity", AnimatorControllerParameterType.Bool, out _walkingAnimationParameter);
        RegisterAnimatorParameter("Idle", AnimatorControllerParameterType.Bool, out _idleAnimationParameter);
    }
    protected override void SetMovement()
    {
        //_movementVector = Vector3.zero;
        _currentInput = Vector2.zero;

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

        //_movementVector.x += _lerpedInput.x;
        //_movementVector.y = 0f;
        //_movementVector.z += _lerpedInput.y;

        ApplyAccelation();
        //ApplyDeccelation();

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



        //if (_movementVector.magnitude > MovementSpeed)
        //{
        //    _movementVector = Vector3.ClampMagnitude(_movementVector, MovementSpeed);
        //}

        if ((_currentInput.magnitude <= IdleThreshold) && (_controller.CurrentMovement.magnitude < IdleThreshold))
        {
            _movementVector = Vector3.zero;
        }

        Debug.Log("accelation : " + _acceleration + " movementVec : " + _movementVector);
        Debug.Log("_currentInput : " + _currentInput + " _normalizedInput : " + _normalizedInput + " _lerperdInput : " + _lerpedInput);

        //Debug.Log(_movementVector);
        _controller.SetMovement(_movementVector);
    }

    void ApplyFriction()
    {

    }

    void ApplyAccelation()
    {
        ApplyForce(new Vector3(_normalizedInput.x, 0, _normalizedInput.y) * Acceleration * Time.fixedDeltaTime * 0.1F);
    }

    void ApplyDeccelation()
    {
        float GroundFriction = 1;

        float N = 1 * Mass * 1f; //수직 항력 ( m * g)
        float U = 0.01f * GroundFriction; // 마찰 계수
        Vector3 RevVel = -_movementVector.normalized;
        Vector3 Friction = RevVel * N * U * 1;

        ApplyForce(Friction);
    }

    void ApplyForce(Vector3 force)
    {
        //if (!base.hasAuthority) return;

        Vector3 f = force / Mass;
        Magnitude += f;
        Magnitude = Vector3.ClampMagnitude(Magnitude, 6);
    }
}
