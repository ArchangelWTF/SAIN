using System;
using System.Collections;
using EFT;
using SAIN.Components.PlayerComponentSpace;
using UnityEngine;
using EFT.Ballistics;

namespace SAIN.Components.BotControllerSpace.Classes;

public class BotHearingClass : BotManagerBase
{
    public event Action<EPhraseTrigger, ETagStatus, Player> PlayerTalk;

    public event Action<SAINSoundType, Vector3, PlayerComponent, float, float> AISoundPlayed;

    public event Action<Shot> BulletImpact;

    public BotHearingClass(BotManagerComponent botController)
        : base(botController) { }

    public void BulletImpacted(Shot bullet)
    {
        //Logger.LogInfo($"Shot By: {bullet.Player?.iPlayer?.Profile.Nickname} at Time: {Time.time}");
        //DebugGizmos.Sphere(bullet.CurrentPosition);
        BulletImpact?.Invoke(bullet);
    }

    public void PlayerTalked(EPhraseTrigger phrase, ETagStatus mask, Player player)
    {
        if (phrase == EPhraseTrigger.OnDeath)
        {
            return;
        }
        if (player == null || !player.HealthController.IsAlive)
        {
            return;
        }

        PlayerComponent playerComponent = SAINGameWorld.PlayerTracker.GetPlayerComponent(player);
        if (playerComponent != null)
        {
            var Range = phrase switch
            {
                EPhraseTrigger.OnBreath => 35,
                EPhraseTrigger.OnBeingHurt or EPhraseTrigger.OnAgony => 70,
                _ => (float)(mask == ETagStatus.Unaware ? 40 : 70),
            };
            playerComponent.PlayAISound(SAINSoundType.Conversation, player.Position, Range, 1, phrase, mask);
            PlayerTalk?.Invoke(phrase, mask, player);
        }
    }

    public void PlayAISound(string profileId, SAINSoundType soundType, Vector3 position, float range, float volume)
    {
        PlayerComponent playerComponent = SAINGameWorld.PlayerTracker.GetPlayerComponent(profileId);
        PlayAISound(playerComponent, soundType, position, range, volume, true);
    }

    public void PlayAISound(IPlayer Player, SAINSoundType soundType, Vector3 position, float range, float volume)
    {
        PlayerComponent playerComponent = SAINGameWorld.PlayerTracker.GetPlayerComponent(Player);
        PlayAISound(playerComponent, soundType, position, range, volume, true);
    }

    public void PlayAISound(
        PlayerComponent playerComponent,
        SAINSoundType soundType,
        Vector3 position,
        float range,
        float volume,
        bool limitFreq
    )
    {
        if (playerComponent == null)
        {
#if DEBUG
            Logger.LogError("Player Component Null");
#endif
            return;
        }
        if (!playerComponent.IsActive)
        {
            return;
        }
        if (!playerComponent.AIData.AISoundPlayer.ShallPlayAISound())
        {
            return;
        }

        /*
        if (!playerComponent.IsAI)
        {
            Logger.LogInfo(
                $"{playerComponent.Player.Profile.Nickname}: SoundType [{soundType}] FinalRange: {range * volume} Base Range {range} : Volume: {volume}"
            );
        }
        */

        playerComponent.PlayAISound(soundType, position, range, volume);
        AISoundPlayed?.Invoke(soundType, position, playerComponent, range, volume);
        
        BotController.StartCoroutine(WaitDelayThenPlayDefaultBotEvent(soundType, playerComponent, position, range, volume));
    }

    private IEnumerator WaitDelayThenPlayDefaultBotEvent(
        SAINSoundType soundType,
        PlayerComponent playerComponent,
        Vector3 position,
        float range,
        float volume,
        float delay = 0.1f
    )
    {
        yield return new WaitForSeconds(delay);
        if (playerComponent?.Player?.HealthController?.IsAlive == true && playerComponent.IsActive)
        {
            playBotEvent(playerComponent.Player, position, range * volume, soundType);
        }
    }

    private void playBotEvent(Player player, Vector3 position, float range, SAINSoundType soundType)
    {
        AISoundType baseSoundType = getBaseSoundType(soundType);
        BotController.GlobalEventDispatcher?.PlaySound(player, position, range, baseSoundType);
    }

    private AISoundType getBaseSoundType(SAINSoundType soundType)
    {
        AISoundType baseSoundType;
        switch (soundType)
        {
            case SAINSoundType.Shot:
                baseSoundType = AISoundType.gun;
                break;

            case SAINSoundType.SuppressedShot:
                baseSoundType = AISoundType.silencedGun;
                break;

            default:
                baseSoundType = AISoundType.step;
                break;
        }
        return baseSoundType;
    }
}
