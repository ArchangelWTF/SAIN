using UnityEngine;

namespace SAIN.Types.PlayerSmoothing;

public class PredictivePositionSmoother
{
    private Vector3 _smoothedPosition;
    private Vector3 _targetVelocity;
    private Vector3 _currentVelocity; // For SmoothDamp
    private Vector3 _velocitySmoothing; // For velocity estimation SmoothDamp
    private bool _initialized;

    // Smoothing parameters
    public float SmoothTime { get; set; } = 0.35f; // Time to reach target (lower = faster)

    public float PredictionStrength { get; set; } = 1f; // How much to compensate for lag
    public float VelocitySmoothTime { get; set; } = 0.1f; // Velocity estimation smoothing time
    public float MaxPredictionDistance { get; set; } = 5f; // Clamp prediction to reasonable bounds

    /// <summary>
    /// Current smoothed position
    /// </summary>
    public Vector3 Position
    {
        get { return _smoothedPosition; }
    }

    /// <summary>
    /// Estimated target velocity
    /// </summary>
    public Vector3 Velocity
    {
        get { return _targetVelocity; }
    }

    /// <summary>
    /// Initialize or reset the smoother with a starting position
    /// </summary>
    /// <param name="initialPosition">Starting position</param>
    public void Initialize(Vector3 initialPosition)
    {
        _smoothedPosition = initialPosition;
        _targetVelocity = Vector3.zero;
        _currentVelocity = Vector3.zero;
        _velocitySmoothing = Vector3.zero;
        _initialized = true;
    }

    /// <summary>
    /// Update the smoother with a new target position
    /// </summary>
    /// <param name="targetPosition">Current target position</param>
    /// <param name="targetVelocity">Current target position</param>
    /// <param name="deltaTime">Time since last update</param>
    /// <returns>New smoothed position</returns>
    public Vector3 Update(Vector3 targetPosition, Vector3 targetVelocity, float deltaTime)
    {
        if (!_initialized)
        {
            Initialize(targetPosition);
            return _smoothedPosition;
        }

        if (deltaTime <= 0f)
        {
            return _smoothedPosition;
        }

        // Calculate target velocity with SmoothDamp
        _targetVelocity = Vector3.SmoothDamp(
            _targetVelocity,
            targetVelocity,
            ref _velocitySmoothing,
            VelocitySmoothTime,
            Mathf.Infinity,
            deltaTime
        );

        // Predict target position with lag compensation
        var lagCompensation = SmoothTime * PredictionStrength;
        var predictedTarget = targetPosition + _targetVelocity * lagCompensation;

        // Clamp prediction
        var predictionOffset = predictedTarget - targetPosition;
        if (predictionOffset.magnitude > MaxPredictionDistance)
        {
            predictionOffset = predictionOffset.normalized * MaxPredictionDistance;
            predictedTarget = targetPosition + predictionOffset;
        }

        // Smooth towards predicted position using SmoothDamp
        _smoothedPosition = Vector3.SmoothDamp(
            _smoothedPosition,
            predictedTarget,
            ref _currentVelocity,
            SmoothTime,
            Mathf.Infinity,
            deltaTime
        );

        // Convergence guarantee
        var distanceToTarget = Vector3.Distance(_smoothedPosition, targetPosition);
        var velocityMagnitude = _targetVelocity.magnitude;

        if (!(distanceToTarget < 0.001f) || !(velocityMagnitude < 0.01f))
        {
            return _smoothedPosition;
        }

        _smoothedPosition = targetPosition;
        _currentVelocity = Vector3.zero;

        return _smoothedPosition;
    }

    /// <summary>
    /// Force immediate convergence to target position
    /// </summary>
    /// <param name="targetPosition">Position to snap to</param>
    public void Snap(Vector3 targetPosition)
    {
        _smoothedPosition = targetPosition;
        _targetVelocity = Vector3.zero;
        _currentVelocity = Vector3.zero;
        _velocitySmoothing = Vector3.zero;
    }
}
