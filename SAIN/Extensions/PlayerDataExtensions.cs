using EFT;
using SAIN.Components.PlayerComponentSpace;
using SAIN.Helpers;
using SAIN.Models.PlayerData;
using UnityEngine;
using UnityEngine.AI;

namespace SAIN.Extensions;

public static class PlayerDataExtensions
{
    private const float ON_NAVMESH_BUFFER_INTERVAL = 0.15F;
    private const float NAV_SAMPLE_RANGE = 1f;
    private const float NAVMESH_CHECK_FREQUENCY = 0.5f;
    private const float NAVMESH_CHECK_FREQUENCY_AI = 1f / 60f;
    private const float NAVMESH_DISTANCE_ON = 0.35f;
    private const float NAVMESH_DISTANCE_CLOSE = 0.6f;
    private const float NAVMESH_DISTANCE_FAR = 1f;

    public static PlayerHeadData Update(this PlayerHeadData headData, Vector3 headPosition, Quaternion headRotation, Vector3 headUpVector)
    {
        headData.HeadPosition = headPosition;
        //headData.HeadLookDirection = headRotation * headUpVector;
        headData.HeadLookDirection = headUpVector;
        return headData;
    }

    public static PlayerWeaponData Update(this PlayerWeaponData weaponData, Vector3 playerPosition, Player player)
    {
        Vector3 weapRoot = player.WeaponRoot.position;
        weaponData.WeaponRoot = new Vector3(weapRoot.x, weapRoot.y, weapRoot.z);
        weaponData.WeaponRootOffset = new Vector3(
            weapRoot.x - playerPosition.x,
            weapRoot.y - playerPosition.y,
            weapRoot.z - playerPosition.z
        );
        weaponData.WeaponRootHeightOffset = new Vector3(0, weapRoot.y - playerPosition.y, 0);

        // Is the player holding a weapon?
        if (player.HandsController is Player.FirearmController firearmController && firearmController.CurrentFireport != null)
        {
            Vector3 firePort = firearmController.CurrentFireport.position;
            Vector3 pointDir = firearmController.CurrentFireport.Original.TransformDirection(player.LocalShotDirection);
            firearmController.AdjustShotVectors(ref firePort, ref pointDir);
            weaponData.FirePort = firePort;
            weaponData.PointDirection = pointDir.normalized;
        }
        else
        {
            // we failed to get fireport info, set the positions to a fallback
            weaponData.FirePort = weaponData.WeaponRoot;
            weaponData.PointDirection = player.LookDirection;
        }
        return weaponData;
    }

    public static PlayerVelocityData Update(this PlayerVelocityData playerVelocityData, Vector3 velocity)
    {
        const float TRANSFORM_MIN_VELOCITY = 0.25f;
        const float TRANSFORM_MAX_VELOCITY = 5f;

        playerVelocityData.Velocity = velocity;
        float sqrMag = playerVelocityData.Velocity.sqrMagnitude;
        if (sqrMag <= TRANSFORM_MIN_VELOCITY * TRANSFORM_MIN_VELOCITY)
        {
            playerVelocityData.VelocityMagnitude = 0f;
            playerVelocityData.VelocityMagnitudeNormal = 0f;
        }
        else if (sqrMag >= TRANSFORM_MAX_VELOCITY * TRANSFORM_MAX_VELOCITY)
        {
            playerVelocityData.VelocityMagnitude = TRANSFORM_MAX_VELOCITY;
            playerVelocityData.VelocityMagnitudeNormal = 1f;
        }
        else
        {
            float mag = Mathf.Sqrt(sqrMag);
            playerVelocityData.VelocityMagnitude = mag;
            float num = TRANSFORM_MAX_VELOCITY - TRANSFORM_MIN_VELOCITY;
            float num2 = mag - TRANSFORM_MIN_VELOCITY;
            playerVelocityData.VelocityMagnitudeNormal = num2 / num;
        }
        return playerVelocityData;
    }

    public static PlayerDirectionData GetUpdatedDirectionData(
        this PlayerDirectionData data,
        PlayerComponent Owner,
        PlayerComponent OtherPlayer
    )
    {
        var Transform = Owner.Transform;
        data.OwnerPosition = Transform.Position;
        data.OwnerLookDirection = Transform.LookDirection;
        data.OwnerViewPosition = Transform.EyePosition;
        data.MainDirectionData.Position = OtherPlayer.Position;
        return data;
    }

    public static PlayerNavData Update(this PlayerNavData playerNavData, Vector3 playerPosition, bool isAI)
    {
        float time = Time.time;
        if (playerNavData.NextCheckTime < time)
        {
            float delay = isAI ? NAVMESH_CHECK_FREQUENCY_AI : NAVMESH_CHECK_FREQUENCY;
            playerNavData.NextCheckTime = time + delay;

            if (NavMesh.SamplePosition(playerPosition, out var hit, NAV_SAMPLE_RANGE, -1))
            {
                playerNavData.LastValidHit = hit;
                playerNavData.Position = new Vector3(playerPosition.x, hit.position.y, playerPosition.z);
            }

            Vector3 offset = playerNavData.Position - playerPosition;
            offset.y = 0f; // Ignore vertical offset for nav mesh distance checks

            playerNavData.Status = CalcStatus(offset);
            if (playerNavData.Status == EPlayerNavMeshDistance.OnNavMesh)
            {
                playerNavData.TimeLastOnNavMesh = time;
            }
            //if (DebugSettings.Instance.Gizmos.DrawNavMeshSamplingGizmos)
            //{
            DrawDebugGizmos(playerNavData, playerPosition, delay);
            //}
        }
        playerNavData.IsOnNavMesh = time - playerNavData.TimeLastOnNavMesh < ON_NAVMESH_BUFFER_INTERVAL;
        return playerNavData;
    }

    private static void DrawDebugGizmos(PlayerNavData playerNavData, Vector3 playerPosition, float expireTime)
    {
        Color color;
        if (playerNavData.IsOnNavMesh)
        {
            color = Color.blue;
        }
        else
        {
            color = playerNavData.Status switch
            {
                EPlayerNavMeshDistance.OnNavMesh => Color.green,
                EPlayerNavMeshDistance.CloseToNavMesh => Color.magenta,
                EPlayerNavMeshDistance.FarFromNavMesh => Color.yellow,
                _ => Color.red,
            };
        }
        DebugGizmos.DrawSphere(playerNavData.Position, 0.1f, color, expireTime);
        DebugGizmos.DrawSphere(playerPosition, 0.1f, Color.white, expireTime);
        DebugGizmos.DrawLine(playerNavData.Position, playerPosition, color, 0.1f, expireTime);
    }

    private static EPlayerNavMeshDistance CalcStatus(Vector3 offset)
    {
        float sqrMag = offset.sqrMagnitude;
        if (sqrMag < NAVMESH_DISTANCE_ON * NAVMESH_DISTANCE_ON)
        {
            return EPlayerNavMeshDistance.OnNavMesh;
        }
        if (sqrMag < NAVMESH_DISTANCE_CLOSE * NAVMESH_DISTANCE_CLOSE)
        {
            return EPlayerNavMeshDistance.CloseToNavMesh;
        }
        if (sqrMag < NAVMESH_DISTANCE_FAR * NAVMESH_DISTANCE_FAR)
        {
            return EPlayerNavMeshDistance.FarFromNavMesh;
        }
        return EPlayerNavMeshDistance.OffNavMesh;
    }
}
