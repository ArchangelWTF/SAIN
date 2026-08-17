using UnityEngine;

namespace SAIN.Types.Jobs;

public readonly struct RandomDir
{
    public RandomDir(float RandomMin, float RandomMax)
    {
        Magnitude = UnityEngine.Random.Range(RandomMin, RandomMax);
        DirectionNormal = UnityEngine.Random.onUnitSphere;
        Direction = DirectionNormal * Magnitude;
    }

    public RandomDir(float magnitude)
    {
        Magnitude = magnitude;
        DirectionNormal = UnityEngine.Random.onUnitSphere;
        Direction = DirectionNormal * Magnitude;
    }

    public RandomDir(float magnitude, Vector3 directionNormal)
    {
        Magnitude = magnitude;
        DirectionNormal = directionNormal;
        Direction = DirectionNormal * Magnitude;
    }

    public readonly Vector3 Direction;
    public readonly Vector3 DirectionNormal;
    public readonly float Magnitude;
}
