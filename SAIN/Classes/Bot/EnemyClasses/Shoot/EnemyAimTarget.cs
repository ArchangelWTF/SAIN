using EFT;
using SAIN.Helpers;
using SAIN.Models.Enums;
using SAIN.Preset.Shared.Enums;
using SAIN.Preset.Shared.GlobalSettings.Categories;
using UnityEngine;

namespace SAIN.SAINComponent.Classes.EnemyClasses;

public class EnemyAimTarget(EnemyData enemyData) : EnemyBase(enemyData, enemyData.Enemy.Bot)
{
    private static readonly EAimTargetPart[] _parts =
    [
        EAimTargetPart.Head,
        EAimTargetPart.Chest,
        EAimTargetPart.Stomach,
        EAimTargetPart.LeftArm,
        EAimTargetPart.RightArm,
        EAimTargetPart.LeftLeg,
        EAimTargetPart.RightLeg,
    ];

    private readonly float[] _weights = new float[_parts.Length];

    private EAimTargetPart? _chosen;
    private float _repickTime;

    public EAimTargetPart? ChosenPart
    {
        get { return _chosen; }
    }

    private static AimTargetSettings Settings
    {
        get { return SAINPlugin.LoadedPreset.GlobalSettings.Aiming.AimTarget; }
    }
    
    public Vector3? GetPointToShoot(bool allowRepick = true)
    {
        if (!Settings.Enabled)
        {
            return null;
        }

        float distance = Enemy.RealDistance;
        if (distance < Settings.CloseRangeTorsoDistance)
        {
            _chosen = null;
            return TorsoPoint();
        }

        var parts = Enemy.Vision.EnemyParts;
        if (_chosen == null || _repickTime < Time.time || !CanShoot(_chosen.Value))
        {
            if (!allowRepick)
            {
                return null;
            }

            _chosen = Pick(distance);
            _repickTime = Time.time + Random.Range(Settings.RepickTimeMin, Settings.RepickTimeMax);
        }

        if (_chosen == null)
        {
            return null;
        }

        var part = parts.Parts[_chosen.Value.ToBodyPart()];
        Vector3? point = part.ShootPoint;
        if (point == null)
        {
            return null;
        }

        float spread = Settings.AimPointSpread;
        if (spread >= 1f)
        {
            return point;
        }

        return Vector3.Lerp(part.Transform.position, point.Value, spread);
    }

    private EAimTargetPart? Pick(float distance)
    {
        AimTargetWeights weights = BotWeights();
        float limbScale = LimbScale(distance);
        bool canHead = Bot.Info.FileSettings.Aiming.AimForHead && EFTMath.RandomBool(Bot.Info.FileSettings.Aiming.AimForHeadChance);

        float total = 0f;
        for (int i = 0; i < _parts.Length; i++)
        {
            EAimTargetPart part = _parts[i];
            float weight = 0f;

            if (CanShoot(part) && (canHead || part != EAimTargetPart.Head))
            {
                weight = weights.For(part);
                if (IsSmallTarget(part))
                {
                    weight *= limbScale;
                }
            }

            _weights[i] = weight;
            total += weight;
        }

        if (total <= 0f)
        {
            return null;
        }

        float roll = Random.Range(0f, total);
        EAimTargetPart? last = null;
        for (int i = 0; i < _parts.Length; i++)
        {
            if (_weights[i] <= 0f)
            {
                continue;
            }
            last = _parts[i];
            roll -= _weights[i];
            if (roll <= 0f)
            {
                return _parts[i];
            }
        }
        return last;
    }

    private AimTargetWeights BotWeights()
    {
        var botSettings = Bot.Info.FileSettings.Aiming;
        return botSettings.OverrideAimTargetWeights ? botSettings.AimTargetWeights : Settings.Weights;
    }

    private static float LimbScale(float distance)
    {
        float start = Settings.LimbFalloffStart;
        float end = Settings.LimbFalloffEnd;
        if (distance <= start)
        {
            return 1f;
        }
        if (distance >= end || end <= start)
        {
            return 0f;
        }
        return 1f - ((distance - start) / (end - start));
    }

    private static bool IsSmallTarget(EAimTargetPart part)
    {
        return part != EAimTargetPart.Chest && part != EAimTargetPart.Stomach;
    }

    private bool CanShoot(EAimTargetPart part)
    {
        return Enemy.Vision.EnemyParts.Parts.TryGetValue(part.ToBodyPart(), out var data) && data.CanShoot;
    }

    private Vector3? TorsoPoint()
    {
        var parts = Enemy.Vision.EnemyParts.Parts;
        return parts[EBodyPart.Chest].ShootPoint ?? parts[EBodyPart.Stomach].ShootPoint;
    }
}
