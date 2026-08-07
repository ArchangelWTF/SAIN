using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

namespace SAIN.Helpers;

public static class Vector
{
    public static float FindFlatSignedAngle(Vector3 a, Vector3 b, Vector3 origin)
    {
        a.y = 0;
        b.y = 0;
        origin.y = 0;
        return Vector3.SignedAngle(a - origin, b - origin, Vector3.up);
    }

    public static bool Raycast(Vector3 start, Vector3 end, LayerMask mask)
    {
        Vector3 direction = end - start;
        return Physics.Raycast(start, direction.normalized, direction.magnitude, mask);
    }

    public static Vector3 DangerPoint(Vector3 position, Vector3 force, float mass)
    {
        force /= mass;

        Vector3 vector = CalculateForce(position, force);

        Vector3 midPoint = (position + vector) / 2f;

        CheckThreePoints(position, midPoint, vector, out Vector3 result);

        return result;
    }

    private static bool CheckThreePoints(Vector3 from, Vector3 midPoint, Vector3 target, out Vector3 hitPos)
    {
        Vector3 direction = midPoint - from;
        if (
            Physics.Raycast(
                new Ray(from, direction),
                out RaycastHit raycastHit,
                direction.magnitude,
                LayersMaskController.HighPolyWithTerrainMask
            )
        )
        {
            hitPos = raycastHit.point;
            return false;
        }

        Vector3 direction2 = midPoint - target;
        if (Physics.Raycast(new Ray(midPoint, direction2), out raycastHit, direction2.magnitude, LayersMaskController.HighPolyWithTerrainMask))
        {
            hitPos = raycastHit.point;
            return false;
        }

        hitPos = target;
        return true;
    }

    private static Vector3 CalculateForce(Vector3 from, Vector3 force)
    {
        Vector3 v = new(force.x, 0f, force.z);

        Vector2 vector = new(v.magnitude, force.y);

        float num = 2f * vector.x * vector.y / HelpersGClass.Gravity;

        if (vector.y < 0f)
        {
            num = -num;
        }

        return NormalizeFastSelf(v) * num + from;
    }

    public static bool CanShootToTarget(ShootToPoint shootToPoint, Vector3 firePos, LayerMask mask, bool doubleSide = false)
    {
        if (shootToPoint == null)
        {
            return false;
        }
        bool flag = false;
        Vector3 vector = shootToPoint.Point - firePos;
        Ray ray = new(firePos, vector);
        float magnitude = vector.magnitude;
        if (!Physics.Raycast(ray, out RaycastHit raycastHit, magnitude * shootToPoint.DistCoef, mask))
        {
            if (doubleSide)
            {
                if (!Physics.Raycast(new Ray(shootToPoint.Point, -vector), out raycastHit, magnitude, mask))
                {
                    flag = true;
                }
            }
            else
            {
                flag = true;
            }
        }
        return flag;
    }

    public static Vector3 Rotate(Vector3 direction, float degX, float degY, float degZ)
    {
        return Quaternion.Euler(degX, degY, degZ) * direction;
    }

    public static float RandomRange(float magnitude)
    {
        return Random.Range(-magnitude, magnitude);
    }

    public static bool IsAngLessNormalized(Vector3 a, Vector3 b, float cos)
    {
        return a.x * b.x + a.y * b.y + a.z * b.z > cos;
    }

    public static Vector3 NormalizeFastSelf(Vector3 v)
    {
        float num = (float)System.Math.Sqrt((double)(v.x * v.x + v.y * v.y + v.z * v.z));
        v.x /= num;
        v.y /= num;
        v.z /= num;
        return v;
    }

    public static Vector3 RotateOnAngUp(Vector3 b, float angDegree)
    {
        float f = angDegree * 0.017453292f;
        float num = Mathf.Sin(f);
        float num2 = Mathf.Cos(f);
        float x = b.x * num2 - b.z * num;
        float z = b.z * num2 + b.x * num;
        return new Vector3(x, 0f, z);
    }
}
