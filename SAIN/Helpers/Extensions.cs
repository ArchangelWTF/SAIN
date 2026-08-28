using System.Collections.Generic;
using EFT;
using JetBrains.Annotations;
using SAIN.Preset.Shared.Enums;
using SAIN.SAINComponent.Classes.Mover;
using UnityEngine;

namespace SAIN.Helpers;

public static class Extensions
{
    public static float CalcPathLength([NotNull] this List<BotPathCorner> path)
    {
        float result = 0;
        for (int i = 0; i < path.Count; i++)
        {
            result += path[i].DirectionFromPrevious.Magnitude;
        }
        return result;
    }

    public static bool IsLegs(this EBodyPart part)
    {
        switch (part)
        {
            case EBodyPart.LeftLeg:
            case EBodyPart.RightLeg:
                return true;

            default:
                return false;
        }
    }

    public static bool IsPMC(this WildSpawnType type)
    {
        switch (type)
        {
            case WildSpawnType.pmcBEAR:
            case WildSpawnType.pmcUSEC:
                return true;

            default:
                return false;
        }
    }

    public static SAINSoundType Convert(this AISoundType aiSoundType)
    {
        switch (aiSoundType)
        {
            case AISoundType.silencedGun:
                return SAINSoundType.SuppressedShot;

            case AISoundType.gun:
                return SAINSoundType.Shot;

            default:
                return SAINSoundType.Generic;
        }
    }

    public static AISoundType Convert(this SAINSoundType sainSoundType)
    {
        switch (sainSoundType)
        {
            case SAINSoundType.SuppressedShot:
                return AISoundType.silencedGun;

            case SAINSoundType.Shot:
                return AISoundType.gun;

            default:
                return AISoundType.step;
        }
    }

    public static bool IsGunShot(this SAINSoundType sainSoundType)
    {
        switch (sainSoundType)
        {
            case SAINSoundType.SuppressedShot:
            case SAINSoundType.Shot:
                return true;

            default:
                return false;
        }
    }

    public static float Sqr(this float value)
    {
        return value * value;
    }

    public static float Scale0to1(this float value, float scalingFactor)
    {
        return value.Scale(0, 1f, 1f - scalingFactor, 1f + scalingFactor);
    }

    public static float Scale(this float value, float inputMin, float inputMax, float outputMin, float outputMax)
    {
        return outputMin + (outputMax - outputMin) * ((value - inputMin) / (inputMax - inputMin));
    }

    public static float Randomize(this float value, float a = 0.5f, float b = 2f)
    {
        return (value * Random(a, b)).Round100();
    }

    public static float Random(float a, float b)
    {
        return UnityEngine.Random.Range(a, b);
    }

    public static float Round(this float value, float round)
    {
        return Mathf.Round(value * round) / round;
    }

    public static float Round10(this float value)
    {
        return value.Round(10);
    }

    public static float Round100(this float value)
    {
        return value.Round(100);
    }

    public static float Round1000(this float value)
    {
        return value.Round(1000);
    }

    /// <summary>
    /// The body part SAIN's own vision data is keyed by.
    /// </summary>
    public static EBodyPart ToBodyPart(this EAimTargetPart part)
    {
        return part switch
        {
            EAimTargetPart.Head => EBodyPart.Head,
            EAimTargetPart.Chest => EBodyPart.Chest,
            EAimTargetPart.Stomach => EBodyPart.Stomach,
            EAimTargetPart.LeftArm => EBodyPart.LeftArm,
            EAimTargetPart.RightArm => EBodyPart.RightArm,
            EAimTargetPart.LeftLeg => EBodyPart.LeftLeg,
            _ => EBodyPart.RightLeg,
        };
    }

    /// <summary>
    /// The body part EFT's own aiming code is keyed by.
    /// </summary>
    public static BodyPartType ToBodyPartType(this EAimTargetPart part)
    {
        return part switch
        {
            EAimTargetPart.Head => BodyPartType.head,
            EAimTargetPart.LeftArm => BodyPartType.leftArm,
            EAimTargetPart.RightArm => BodyPartType.rightArm,
            EAimTargetPart.LeftLeg => BodyPartType.leftLeg,
            EAimTargetPart.RightLeg => BodyPartType.rightLeg,
            _ => BodyPartType.body,
        };
    }
}
