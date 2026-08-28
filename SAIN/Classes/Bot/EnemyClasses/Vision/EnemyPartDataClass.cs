using System.Collections.Generic;
using EFT;
using SAIN.Models.Enums;
using SAIN.Models.Structs;
using UnityEngine;

namespace SAIN.SAINComponent.Classes.EnemyClasses;

public class EnemyPartDataClass
{
    private readonly RaycastResult[] _raycastResults =
    [
        new RaycastResult(), // LineofSight
        new RaycastResult(), // Shoot
        new RaycastResult(), // Vision
    ];

    public float TimeSeen { get; private set; }

    public bool CanBeSeen { get; private set; }
    public bool LineOfSight { get; private set; }
    public bool CanShoot { get; private set; }

    public Vector3? ShootPoint
    {
        get { return CanShoot ? _raycastResults[(int)ERaycastCheck.Shoot].GetSuccessPoint() : null; }
    }

    private readonly Dictionary<EBodyPartColliderType, BodyPartCollider> _colliderDictionary = [];

    public EnemyPartDataClass(EBodyPart bodyPart, BifacialTransform transform, List<BodyPartCollider> colliders)
    {
        BodyPart = bodyPart;
        Transform = transform;
        Colliders = colliders;
        _indexMax = colliders.Count - 1;

        foreach (BodyPartCollider collider in colliders)
        {
            if (!_colliderDictionary.ContainsKey(collider.BodyPartColliderType))
            {
                _colliderDictionary.Add(collider.BodyPartColliderType, collider);
            }
        }
    }

    public void Update(float currentTime)
    {
        const float SUCCESS_PERIOD = 0.25f;
        float lineOfSightSuccessTime = _raycastResults[(int)ERaycastCheck.LineofSight].TimeLastSuccess;
        LineOfSight = currentTime - lineOfSightSuccessTime <= SUCCESS_PERIOD;
        float shootSuccessTime = _raycastResults[(int)ERaycastCheck.Shoot].TimeLastSuccess;
        CanShoot = currentTime - shootSuccessTime <= SUCCESS_PERIOD;
        if (!LineOfSight)
        {
            CanBeSeen = false;
            TimeSeen = -1f;
            return;
        }
        float visionSuccessTime = _raycastResults[(int)ERaycastCheck.Vision].TimeLastSuccess;
        CanBeSeen = currentTime - visionSuccessTime <= SUCCESS_PERIOD;
        if (!CanBeSeen)
        {
            TimeSeen = -1f;
            return;
        }
        if (TimeSeen <= 0f)
        {
            TimeSeen = Time.time;
        }
    }

    public void SetLineOfSight(Vector3 castPoint, EBodyPartColliderType colliderType, RaycastHit raycastHit, ERaycastCheck type, float time)
    {
        _raycastResults[(int)type].Update(castPoint, _colliderDictionary[colliderType], raycastHit, time);
    }

    public SAINBodyPartRaycast GetRaycast()
    {
        BodyPartCollider collider = GetCollider();

        return new SAINBodyPartRaycast
        {
            CastPoint = GetCastPoint(collider),
            PartType = BodyPart,
            ColliderType = collider.BodyPartColliderType,
        };
    }

    public readonly EBodyPart BodyPart;
    public readonly List<BodyPartCollider> Colliders;
    public readonly BifacialTransform Transform;

    private BodyPartCollider GetCollider()
    {
        BodyPartCollider collider = Colliders[_index];
        _index++;
        if (_index > _indexMax)
        {
            _index = 0;
        }
        return collider;
    }

    private int _index;
    private readonly int _indexMax;

    private Vector3 GetCastPoint(BodyPartCollider collider)
    {
        float size = GetColliderMinSize(collider);
        //Logger.LogInfo(size);
        Vector3 random = UnityEngine.Random.insideUnitSphere * size;
        Vector3 result = collider.Collider.ClosestPoint(collider.transform.position + random);
        return result;
    }

    private float GetColliderMinSize(BodyPartCollider collider)
    {
        if (collider.Collider == null)
        {
            return 0f;
        }
        Vector3 bounds = collider.Collider.bounds.size;
        float lowest = bounds.x;
        if (bounds.y < lowest)
        {
            lowest = bounds.y;
        }
        if (bounds.z < lowest)
        {
            lowest = bounds.z;
        }
        return lowest;
    }
}
