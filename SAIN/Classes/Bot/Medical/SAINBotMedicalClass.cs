using System.Collections.Generic;
using EFT;
using EFT.Ballistics;
using EFT.HealthSystem;
using EFT.InventoryLogic;
using SAIN.Components;
using UnityEngine;

namespace SAIN.SAINComponent.Classes;

public class SAINBotMedicalClass : BotComponentClassBase
{
    public SAINBotMedicalClass(BotComponent sain)
        : base(sain)
    {
        TickRequirement = ESAINTickState.OnlyNoSleep;
        Surgery = new BotSurgery(sain);
        HitReaction = new SAINBotHitReaction(sain);
        HitByEnemy = new BotHitByEnemyClass(sain);
    }

    public BotSurgery Surgery { get; private set; }
    public SAINBotHitReaction HitReaction { get; private set; }
    public BotHitByEnemyClass HitByEnemy { get; private set; }

    private static readonly EquipmentSlot[] allSlots =
    [
        EquipmentSlot.Pockets,
        EquipmentSlot.TacticalVest,
        EquipmentSlot.Backpack,
        EquipmentSlot.SecuredContainer,
    ];

    public void FindHealingItems(BotOwner botOwner)
    {
        if (_nextFullRefreshTime < Time.time)
        {
            _nextFullRefreshTime = Time.time + _fullRefreshFreq;
            Meds.Clear();
            _availableHealingItems.Clear();
            botOwner.GetPlayer.InventoryController.GetAcceptableItemsNonAlloc(allSlots, _availableHealingItems);
        }
        UpdateAvailableMeds(botOwner);
    }

    public bool TryUseMedItem()
    {
        if (Meds.Count == 0)
        {
            return false;
        }

        Meds item = SelectItemToUse();
        if (item == null)
        {
            return false;
        }
        BotOwner.Medecine.FirstAid.CurUsingMeds = item;
        BotOwner.Medecine.FirstAid._bodyPartToHeal = EBodyPart.Chest;
        BotOwner.Medecine.FirstAid.ApplyToSelf();
        return true;
    }

    private Meds SelectItemToUse()
    {
        Meds item;
        IHeavyBleeding heavyBleed = this.Player.HealthController.FindExistingEffect<IHeavyBleeding>(EBodyPart.Common);
        if (heavyBleed != null && Meds.TryGetValue(EDamageEffectType.HeavyBleeding, out item))
        {
            Meds.Remove(EDamageEffectType.HeavyBleeding);
            return item;
        }
        ILightBleeding lightBleed = this.Player.HealthController.FindExistingEffect<ILightBleeding>(EBodyPart.Common);
        if (lightBleed != null && Meds.TryGetValue(EDamageEffectType.LightBleeding, out item))
        {
            Meds.Remove(EDamageEffectType.LightBleeding);
            return item;
        }
        item = FindPainKiller();
        if (item != null)
        {
            return item;
        }
        return null;
    }

    private Meds FindPainKiller()
    {
        IPain pain = this.Player.HealthController.FindExistingEffect<IPain>(EBodyPart.Common);
        if (pain != null && Meds.TryGetValue(EDamageEffectType.Pain, out Meds item))
        {
            Meds.Remove(EDamageEffectType.Pain);
            return item;
        }
        return null;
    }

    public void OnHealthEffectStarted(IHealthEffect obj)
    {
        if (obj is IFracture)
        {
            EBodyPart bodyPart = obj.BodyPart;
            if (bodyPart - EBodyPart.LeftArm <= 1)
            {
                Bot?.Talk.GroupSay(EPhraseTrigger.HandBroken);
                return;
            }
            if (bodyPart - EBodyPart.LeftLeg > 1)
            {
                return;
            }
            Bot?.Talk.GroupSay(EPhraseTrigger.LegBroken);
            // leg broken
        }
    }

    private const float _fullRefreshFreq = 30f;
    private float _nextFullRefreshTime;

    private void UpdateAvailableMeds(BotOwner botOwner)
    {
        bool foundPainKiller = false;
        bool foundHeavyBleed = false;
        bool foundLightBleed = false;
        foreach (Meds med in _availableHealingItems)
        {
            if (med.TryGetItemComponent(out HealthEffectsComponent healthComponent))
            {
                if (
                    !foundHeavyBleed
                    && !Meds.ContainsKey(EDamageEffectType.HeavyBleeding)
                    && healthComponent.DamageEffects.ContainsKey(EDamageEffectType.HeavyBleeding)
                )
                {
                    Meds.Add(EDamageEffectType.HeavyBleeding, med);
                    //Logger.LogInfo($"[{botOwner.name}] Found Heavy Bleed Med: {med.Name}");
                }
                if (
                    !foundLightBleed
                    && !Meds.ContainsKey(EDamageEffectType.LightBleeding)
                    && healthComponent.DamageEffects.ContainsKey(EDamageEffectType.LightBleeding)
                )
                {
                    Meds.Add(EDamageEffectType.LightBleeding, med);
                    //Logger.LogInfo($"[{botOwner.name}] Found Light Bleed Med: {med.Name}");
                }
                if (
                    !foundPainKiller
                    && !Meds.ContainsKey(EDamageEffectType.Pain)
                    && healthComponent.DamageEffects.ContainsKey(EDamageEffectType.Pain)
                )
                {
                    Meds.Add(EDamageEffectType.Pain, med);
                    //Logger.LogInfo($"[{botOwner.name}] Found PainKiller Med: {med.Name}");
                }
            }
            if (foundPainKiller && foundHeavyBleed && foundLightBleed)
            {
                break; // We already have all the meds we need
            }
        }
    }

    public Dictionary<EDamageEffectType, Meds> Meds { get; } = [];

    private readonly List<Meds> _availableHealingItems = [];

    public void TryCancelHeal()
    {
        if (_nextCancelTime < Time.time)
        {
            _nextCancelTime = Time.time + _cancelFreq;
            //BotOwner.Medecine?.Stimulators?.CancelCurrent();
            //BotOwner.Medecine?.FirstAid?.CancelCurrent();
            //BotOwner.Medecine?.SurgicalKit?.CancelCurrent();
        }
    }

    private float _nextCancelTime;
    private float _cancelFreq = 1f;

    public override void Init()
    {
        Player.BeingHitAction += GetHit;
        BotOwner.Medecine.FirstAid.OnEndApply += FindHealingItems;
        FindHealingItems(BotOwner);
        Surgery.Init();
        HitReaction.Init();
        HitByEnemy.Init();
        base.Init();
    }

    public override void ManualUpdate()
    {
        Surgery.ManualUpdate();
        HitReaction.ManualUpdate();
        HitByEnemy.ManualUpdate();
        base.ManualUpdate();
    }

    public override void Dispose()
    {
        if (BotOwner != null && BotOwner.Medecine?.FirstAid != null)
        {
            BotOwner.Medecine.FirstAid.OnEndApply -= FindHealingItems;
        }

        if (Player != null)
        {
            Player.BeingHitAction -= GetHit;
        }

        Surgery?.Dispose();
        HitReaction?.Dispose();
        HitByEnemy?.Dispose();
        base.Dispose();
    }

    public void GetHit(DamageInfo DamageInfo, EBodyPart bodyPart, float floatVal)
    {
        TimeLastShot = Time.time;
        HitByEnemy.GetHit(DamageInfo, bodyPart, floatVal);
        HitReaction.GetHit(DamageInfo, bodyPart, floatVal);
        Bot.Cover.GetHit(DamageInfo, bodyPart, floatVal);
    }

    public float TimeLastShot { get; private set; }
    public float TimeSinceShot
    {
        get { return Time.time - TimeLastShot; }
    }
}
