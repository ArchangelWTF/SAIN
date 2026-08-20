using System.Collections;
using System.Collections.Generic;
using SAIN.Helpers;
using SAIN.Models.Enums;
using SAIN.Models.Structs;
using SAIN.SAINComponent.Classes.EnemyClasses;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace SAIN.Components;

public class VisionRaycastJob : BotManagerBase
{
    private static readonly QueryParameters _losParams = new(LayersMaskController.HighPolyWithTerrainNoGrassMask);
    private static readonly QueryParameters _visParams = new(LayersMaskController.AI);
    private static readonly QueryParameters _shootParams = new(LayersMaskController.HighPolyWithTerrainMaskAI);

    private const float VISION_UPDATE_INTERVAL = 1f / 30f;
    private const float VISION_JOB_INTERVAL = 1f / 30f;
    private const int RAYCAST_CHECKS = 3;

    private bool _disposed = false;
    private readonly List<Enemy> _enemies = [];
    private readonly List<EBodyPartColliderType> _colliderTypes = [];
    private readonly List<Vector3> _castPoints = [];

    public VisionRaycastJob(BotManagerComponent botcontroller)
        : base(botcontroller)
    {
        botcontroller.StartCoroutine(EnemyVisionJob());
        botcontroller.StartCoroutine(UpdateEFTVision());
    }

    private IEnumerator EnemyVisionJob()
    {
        WaitForSeconds wait = new(VISION_JOB_INTERVAL);
        yield return wait;
        while (BotController != null && !_disposed)
        {
            HashSet<BotComponent> bots = BotController.BotSpawnController?.SAINBots;
            if (bots != null && bots.Count > 0)
            {
                FindEnemies(bots, _enemies);
                int enemyCount = _enemies.Count;
                if (enemyCount > 0)
                {
                    int totalParts = 0;
                    for (int i = 0; i < enemyCount; i++)
                    {
                        totalParts += _enemies[i].Vision.EnemyParts.PartsArray.Length;
                    }
                    int totalRaycasts = totalParts * RAYCAST_CHECKS;

                    NativeArray<RaycastHit> hits = new(totalRaycasts, Allocator.TempJob);
                    NativeArray<RaycastCommand> commands = new(totalRaycasts, Allocator.TempJob);

                    CreateCommands(commands, enemyCount);
                    _handle = RaycastCommand.ScheduleBatch(commands, hits, 32);

                    yield return null;

                    _handle.Complete();
                    AnalyzeHits(hits, commands, enemyCount);

                    hits.Dispose();
                    commands.Dispose();
                }
            }

            yield return wait;
        }
    }

    private IEnumerator UpdateEFTVision()
    {
        WaitForSeconds wait = new(VISION_UPDATE_INTERVAL);
        WaitForFixedUpdate waitForFixedUpdate = new();
        yield return wait;
        while (BotController != null)
        {
            yield return waitForFixedUpdate;
            HashSet<BotComponent> botGroup1 = BotController.BotSpawnController.BotGroup1;
            if (botGroup1.Count > 0)
            {
                float currentTime = Time.time;
                foreach (var bot in botGroup1)
                {
                    bot.Vision.BotLook.UpdateLook(currentTime);
                }
            }
            yield return null;

            yield return waitForFixedUpdate;
            HashSet<BotComponent> botGroup2 = BotController.BotSpawnController.BotGroup2;
            if (botGroup2.Count > 0)
            {
                float currentTime = Time.time;
                foreach (var bot in botGroup2)
                {
                    bot.Vision.BotLook.UpdateLook(currentTime);
                }
            }
            yield return null;
        }
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;

        if (!_handle.IsCompleted)
        {
            _handle.Complete();
        }

        _enemies.Clear();
        _colliderTypes.Clear();
        _castPoints.Clear();
    }

    private JobHandle _handle;

    private void CreateCommands(NativeArray<RaycastCommand> raycastCommands, int enemyCount)
    {
        _colliderTypes.Clear();
        _castPoints.Clear();

        int commands = 0;

        const float MinDist = 0.01f;
        const float Padding = 0.05f;

        for (int i = 0; i < enemyCount; i++)
        {
            var enemy = _enemies[i];
            var botTransform = enemy.Bot.Transform;

            Vector3 eyePosition = botTransform.EyePosition;
            Vector3 weaponFirePort = botTransform.WeaponData.FirePort;

            var parts = enemy.Vision.EnemyParts.PartsArray;

            for (int j = 0; j < parts.Length; j++)
            {
                var part = parts[j];

                SAINBodyPartRaycast raycastData = part.GetRaycast();
                Vector3 castPoint = raycastData.CastPoint;

                _colliderTypes.Add(raycastData.ColliderType);
                _castPoints.Add(castPoint);

                Vector3 eyeVec = castPoint - eyePosition;
                float eyeMag = eyeVec.magnitude;
                Vector3 eyeDir = eyeMag > 1e-6f ? (eyeVec / eyeMag) : Vector3.forward;
                float eyeDist = Mathf.Max(eyeMag, MinDist);

                Vector3 weaponVec = castPoint - weaponFirePort;
                float weaponMag = weaponVec.magnitude;
                Vector3 weaponDir = weaponMag > 1e-6f ? (weaponVec / weaponMag) : Vector3.forward;
                float weaponDist = Mathf.Max(weaponMag, MinDist);

                raycastCommands[commands++] = new RaycastCommand(eyePosition, eyeDir, _losParams, eyeDist + Padding);
                raycastCommands[commands++] = new RaycastCommand(eyePosition, eyeDir, _visParams, eyeDist + Padding);
                raycastCommands[commands++] = new RaycastCommand(weaponFirePort, weaponDir, _shootParams, weaponDist + Padding);
            }
        }
    }

    private void AnalyzeHits(NativeArray<RaycastHit> raycastHits, NativeArray<RaycastCommand> commands, int enemyCount)
    {
        float time = Time.time;
        int hits = 0;
        int colliderTypeCount = 0;

        for (int i = 0; i < enemyCount; i++)
        {
            var enemy = _enemies[i];
            var parts = enemy.Vision.EnemyParts.PartsArray;
            for (int j = 0; j < parts.Length; j++)
            {
                var part = parts[j];
                EBodyPartColliderType colliderType = _colliderTypes[colliderTypeCount];
                Vector3 castPoint = _castPoints[colliderTypeCount];
                colliderTypeCount++;

                if (SAINPlugin.LoadedPreset.GlobalSettings.General.Debug.Gizmos.DrawLineOfSightGizmos)
                {
                    var cmd = commands[hits];
                    var hit = raycastHits[hits];

                    Vector3 from = cmd.from;

                    if (hit.collider == null)
                    {
                        DebugGizmos.DrawSphere(castPoint, 0.03f, Color.yellow, 0.2f);
                        DebugGizmos.DrawLine(from, castPoint, Color.green, 0.025f, 0.2f);
                    }
                }

                part.SetLineOfSight(castPoint, colliderType, raycastHits[hits++], ERaycastCheck.LineofSight, time);
                part.SetLineOfSight(castPoint, colliderType, raycastHits[hits++], ERaycastCheck.Vision, time);
                part.SetLineOfSight(castPoint, colliderType, raycastHits[hits++], ERaycastCheck.Shoot, time);
            }
        }
    }

    private static void FindEnemies(HashSet<BotComponent> bots, List<Enemy> enemies)
    {
        float currentTime = Time.time;
        enemies.Clear();
        foreach (BotComponent bot in bots)
        {
            if (bot != null)
            {
                foreach (Enemy enemy in bot.EnemyController.EnemiesArray)
                {
                    if (enemy.ShallCheckLoS(currentTime))
                    {
                        enemies.Add(enemy);
                    }
                }
            }
        }
    }
}
