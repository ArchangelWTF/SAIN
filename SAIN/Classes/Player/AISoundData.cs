using EFT;
using SAIN.Preset.Shared.Enums;
using SAIN.SAINComponent.Classes.EnemyClasses;
using UnityEngine;

namespace SAIN.Components.PlayerComponentSpace;

public struct AISoundData(SoundEvent InSound, BotComponent InBot, float InPlayerDistance, Enemy InEnemy)
{
    public bool Reported = false;
    public readonly SoundEvent Sound = InSound;
    public readonly BotComponent Bot = InBot;
    public readonly Enemy Enemy = InEnemy;
    public readonly float PlayerDistance = InPlayerDistance;
    public readonly float SoundTravelTime = InPlayerDistance / InSound.SoundSpeed;
    public readonly Player HeardPlayer
    {
        get { return Sound.GetPlayer(); }
    }

    public readonly PlayerComponent HeardPlayerComponent
    {
        get { return Sound.PlayerComponent; }
    }

    public readonly SAINSoundType SoundType
    {
        get { return Sound.SoundType; }
    }

    public readonly bool IsGunShot
    {
        get { return Sound.IsGunShot; }
    }

    public readonly string HeardProfileId
    {
        get { return Sound.ProfileId; }
    }

    public readonly bool IsAI
    {
        get { return Sound.IsAI; }
    }

    public readonly int EnvironmentId
    {
        get { return Sound.EnvironmentId; }
    }

    public Vector3 Position
    {
        get { return Sound.Position; }
    }

    public readonly bool CanReport(float ReactionDelay)
    {
        return Sound.IsValid() && Time.time - Sound.TimeCreated >= SoundTravelTime + ReactionDelay;
    }
}
