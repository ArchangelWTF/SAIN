using SAIN.Components;
using SAIN.Models.Enums;
using UnityEngine;
using EFT.Ballistics;

namespace SAIN.SAINComponent.Classes;

public class BodyPartHitEffectClass : BotBase
{
    public EInjurySeverity LeftArmInjury { get; private set; }
    public EInjurySeverity RightArmInjury { get; private set; }
    public EHitReaction HitReaction { get; private set; }

    public BodyPartHitEffectClass(BotComponent bot)
        : base(bot) { }

    public override void ManualUpdate()
    {
        if (_updateHealthTime < Time.time)
        {
            checkArmInjuries();
        }
    }

    private void checkArmInjuries()
    {
        _updateHealthTime = Time.time + 1f;
        LeftArmInjury = Bot.Medical.HitReaction.BodyParts[EBodyPart.LeftArm].InjurySeverity;
        RightArmInjury = Bot.Medical.HitReaction.BodyParts[EBodyPart.RightArm].InjurySeverity;
    }

    public void GetHit(DamageInfo DamageInfo, EBodyPart bodyPart, float floatVal)
    {
        switch (bodyPart)
        {
            case EBodyPart.Head:
                GetHitInHead(DamageInfo);
                break;

            case EBodyPart.Chest:
            case EBodyPart.Stomach:
                GetHitInCenter(DamageInfo);
                break;

            case EBodyPart.LeftLeg:
            case EBodyPart.RightLeg:
                GetHitInLegs(DamageInfo);
                break;

            default:
                GetHitInArms(DamageInfo);
                break;
        }
    }

    private void GetHitInLegs(DamageInfo DamageInfo)
    {
        HitReaction = EHitReaction.Legs;
    }

    private void GetHitInArms(DamageInfo DamageInfo)
    {
        HitReaction = EHitReaction.Arms;
        checkArmInjuries();
    }

    private void GetHitInCenter(DamageInfo DamageInfo)
    {
        HitReaction = EHitReaction.Center;
    }

    private void GetHitInHead(DamageInfo DamageInfo)
    {
        HitReaction = EHitReaction.Head;
    }

    private float _updateHealthTime;
}
