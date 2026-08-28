using UnityEngine;

namespace SAIN.SAINComponent.Classes.EnemyClasses;

public class RaycastResult
{
    public float TimeLastChecked { get; private set; }
    public float TimeLastSuccess { get; private set; }
    public RaycastHit LastRaycastHit { get; private set; }
    public BodyPartCollider LastSuccessBodyPart { get; private set; }

    private Vector3 _lastSuccessLocalPoint;

    public void Update(Vector3 castPoint, BodyPartCollider bodyPartCollider, RaycastHit raycastHit, float time)
    {
        TimeLastChecked = time;
        LastRaycastHit = raycastHit;

        if (raycastHit.collider == null)
        {
            LastSuccessBodyPart = bodyPartCollider;
            _lastSuccessLocalPoint = bodyPartCollider.transform.InverseTransformPoint(castPoint);
            TimeLastSuccess = time;
        }
    }

    public Vector3? GetSuccessPoint()
    {
        var collider = LastSuccessBodyPart;
        if (collider == null || collider.transform == null)
        {
            return null;
        }
        return collider.transform.TransformPoint(_lastSuccessLocalPoint);
    }
}
