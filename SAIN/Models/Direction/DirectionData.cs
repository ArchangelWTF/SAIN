using SAIN.SAINComponent.Classes.EnemyClasses;
using UnityEngine;

namespace SAIN.Models.Direction;

public sealed class DirectionData
{
    public Vector3 Position { get; set; }
    public Vector3 Direction { get; set; }
    public Vector3 DirectionNormalized { get; set; }
    public float Distance { get; set; }
    public float Dot { get; set; }
    public float HorizontalAngle { get; set; }
    public float VerticalAngle { get; set; }
    public float YDifference { get; set; }

    public void Update(Vector3 Origin)
    {
        Direction = Position - Origin;
        DirectionNormalized = Direction.normalized;
        Distance = Direction.magnitude;
    }

    public void UpdateDotProductAndCalcNormal(Vector3 Origin, Vector3 LookDirection)
    {
        UpdateDotProduct((Position - Origin).normalized, LookDirection);
    }

    public void UpdateDotProduct(Vector3 DirectionNormal, Vector3 LookDirection)
    {
        HorizontalAngle = EnemyAnglesClass.CalcHorizontalAngle(DirectionNormal, LookDirection);
        VerticalAngle = EnemyAnglesClass.CalcVerticalAngle(DirectionNormal, LookDirection, out float yDiff);
        YDifference = yDiff;
        Dot = Vector3.Dot(LookDirection, DirectionNormal);
    }

    public void UpdateDotProduct(Vector3 LookDirection)
    {
        Dot = Vector3.Dot(LookDirection, DirectionNormalized);
    }
}
