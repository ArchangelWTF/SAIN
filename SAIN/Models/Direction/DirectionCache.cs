using UnityEngine;

namespace SAIN.Models.Direction;

public sealed class DirectionCache
{
    public Vector3 Direction { get; private set; }
    public Vector3 DirectionNormalized { get; private set; }
    public float Magnitude { get; private set; }
    public float SqrMagnitude { get; private set; }

    public DirectionCache(Vector3 start, Vector3 end)
    {
        Vector3 dir = end - start;
        Direction = dir;
        DirectionNormalized = dir.normalized;
        float sqrMag = dir.sqrMagnitude;
        SqrMagnitude = sqrMag;
        Magnitude = Mathf.Sqrt(sqrMag);
    }

    public DirectionCache(Vector3 direction)
    {
        Direction = direction;
        DirectionNormalized = direction.normalized;
        float sqrMag = direction.sqrMagnitude;
        SqrMagnitude = sqrMag;
        Magnitude = Mathf.Sqrt(sqrMag);
    }
}
