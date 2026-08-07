using System;
using System.Collections;
using CommonAssets.Scripts.Audio;
using EFT;
using SAIN.Components;
using SAIN.Components.PlayerComponentSpace;
using SAIN.SAINComponent;
using UnityEngine;

namespace SAIN.Classes;

public sealed class PlayerSoundController(PlayerComponent player) : PlayerComponentBase(player)
{
    private const float RunSurfaceRange = 40f;
    private const float SprintSurfaceRange = 60f;

    private Coroutine _runCoroutine;
    private Coroutine _sprintCoroutine;

    private float _lastStepTime = 0f;
    private bool _playedAtLeastOneStep;
    private float _lastAnimatorSign;

    public void HandleMovementState(EPlayerState previousState, EPlayerState nextState)
    {
        if (Player.IsAI && Player.AIData != null)
        {
            return;
        }

        if (BotManagerComponent.Instance == null)
        {
            return;
        }

        switch (previousState)
        {
            case EPlayerState.Idle:
            case EPlayerState.IdleZombieState:
            case EPlayerState.TurnZombieState:
                /*
                if (_idleCoroutine != null)
                {
                    player.StopCoroutine(_idleCoroutine);
                }
                */
                break;
            case EPlayerState.Run:
            case EPlayerState.MoveZombieState:
            case EPlayerState.StartMoveZombieState:
            case EPlayerState.EndMoveZombieState:
                if (_runCoroutine == null)
                {
                    break;
                }
                player.StopCoroutine(_runCoroutine);
                if (!_playedAtLeastOneStep && Player.SinceLastStep > 0.66f)
                {
                    if (Player.CheckSurface(RunSurfaceRange))
                    {
                        BotManagerComponent.Instance.BotHearing.PlayAISound(
                            Player.ProfileId,
                            SAINSoundType.FootStep,
                            Player.Position,
                            SAINPlugin.LoadedPreset.GlobalSettings.Hearing.BaseSoundRange_Footstep,
                            CalculateStepVolume()
                        );
                    }
                    _lastStepTime = Time.time;
                }
                break;
            case EPlayerState.Sprint:
                if (_sprintCoroutine != null)
                {
                    player.StopCoroutine(_sprintCoroutine);
                    if (!_playedAtLeastOneStep && Player.CheckSurface(SprintSurfaceRange))
                    {
                        BotManagerComponent.Instance.BotHearing.PlayAISound(
                            Player.ProfileId,
                            SAINSoundType.Sprint,
                            Player.Position,
                            SAINPlugin.LoadedPreset.GlobalSettings.Hearing.BaseSoundRange_Sprint,
                            Player.CalculateMovementVolumeDefaultMultiplier(EAudioMovementState.Sprint)
                        );
                    }
                }

                if (nextState == EPlayerState.Transition || nextState == EPlayerState.Idle)
                {
                    float num = 1f * Player.MovementContext.CovertMovementVolume;
                    num *= Player.CalculateMovementVolumeDefaultMultiplier(EAudioMovementState.Stop);
                    BotManagerComponent.Instance.BotHearing.PlayAISound(
                        Player.ProfileId,
                        SAINSoundType.Sprint,
                        Player.Position,
                        SAINPlugin.LoadedPreset.GlobalSettings.Hearing.BaseSoundRange_Sprint,
                        num
                    );
                }
                break;
        }
        switch (nextState)
        {
            case EPlayerState.Prone2Stand:
                BotManagerComponent.Instance.BotHearing.PlayAISound(
                    Player.ProfileId,
                    SAINSoundType.GearSound,
                    Player.Position,
                    SAINPlugin.LoadedPreset.GlobalSettings.Hearing.BaseSoundRange_Prone,
                    0.7f
                );
                break;
            case EPlayerState.Sprint:
                _sprintCoroutine = player.StartCoroutine(StartSprintCoroutine(nextState));
                break;
            case EPlayerState.Jump:
                float vol = Player.CalculateMovementVolumeDefaultMultiplier(EAudioMovementState.Jump);
                BotManagerComponent.Instance.BotHearing.PlayAISound(
                    Player.ProfileId,
                    SAINSoundType.Jump,
                    Player.Position,
                    SAINPlugin.LoadedPreset.GlobalSettings.Hearing.BaseSoundRange_Jump,
                    vol
                );
                break;
            case EPlayerState.Idle:
            case EPlayerState.IdleZombieState:
            case EPlayerState.TurnZombieState:
                //_idleCoroutine = StartCoroutine(method_72(nextstate));
                break;
            case EPlayerState.Run:
            case EPlayerState.MoveZombieState:
            case EPlayerState.StartMoveZombieState:
            case EPlayerState.EndMoveZombieState:
                _runCoroutine = player.StartCoroutine(StartRunCoroutine(nextState));
                break;
            case EPlayerState.Transit2Prone:
            {
                float volume = 0.7f * Player.MovementContext.CovertMovementVolume;
                if (previousState == EPlayerState.Sprint)
                {
                    BotManagerComponent.Instance.BotHearing.PlayAISound(
                        Player.ProfileId,
                        SAINSoundType.Prone,
                        Player.Position,
                        // Prone is slightly louder coming from a sprint into prone
                        SAINPlugin.LoadedPreset.GlobalSettings.Hearing.BaseSoundRange_Prone * 1.2f,
                        volume
                    );
                }
                else
                {
                    BotManagerComponent.Instance.BotHearing.PlayAISound(
                        Player.ProfileId,
                        SAINSoundType.Prone,
                        Player.Position,
                        SAINPlugin.LoadedPreset.GlobalSettings.Hearing.BaseSoundRange_Prone,
                        volume
                    );
                }
                break;
            }
        }
    }

    private float CalculateStepVolume()
    {
        EAudioMovementState movementState = ((Player.Pose != EPlayerPose.Duck) ? EAudioMovementState.Run : EAudioMovementState.Duck);
        float covertMovementVolumeBySpeed = Player.MovementContext.CovertMovementVolumeBySpeed;
        float speed = Player.CalculateStepSoundVolumeSpeedFactor();
        float state = Player.CalculateMovementVolumeDefaultMultiplier(movementState);
        float randomVolume = (Player.FirstPersonPointOfView || Player.IsAiUseSimpleSkeleton()) ? UnityEngine.Random.Range(0.75f, 1.1f) : 1f;

        float calc = covertMovementVolumeBySpeed * speed * state * randomVolume;

        return calc;
    }

    private IEnumerator StartRunCoroutine(EPlayerState state = EPlayerState.Run)
    {
        _playedAtLeastOneStep = false;
        while (Player.CurrentState.Name == state)
        {
            float single = Player.LeftStanceCurrentCurveValue;
            if (Math.Abs(_lastAnimatorSign - single) >= float.Epsilon)
            {
                _lastAnimatorSign = single;
                float sinceLastStep = Time.time - _lastStepTime;
                if (sinceLastStep > 0.2f && Player.MovementContext.FreefallTime < 1f)
                {
                    _lastStepTime = Time.time;
                    _playedAtLeastOneStep = true;
                    if (Player.CheckSurface(RunSurfaceRange))
                    {
                        BotManagerComponent.Instance?.BotHearing.PlayAISound(
                            Player.ProfileId,
                            SAINSoundType.FootStep,
                            Player.Position,
                            SAINPlugin.LoadedPreset.GlobalSettings.Hearing.BaseSoundRange_Footstep,
                            CalculateStepVolume()
                        );
                    }
                }
            }
            yield return null;
        }
    }

    private IEnumerator StartSprintCoroutine(EPlayerState state = EPlayerState.Sprint)
    {
        _playedAtLeastOneStep = false;
        while (Player.CurrentState.Name == state)
        {
            float single = Player.LeftStanceCurrentCurveValue;
            if (Math.Abs(_lastAnimatorSign - single) >= float.Epsilon)
            {
                _lastAnimatorSign = single;
                float sinceLastStep = Time.time - _lastStepTime;
                if (sinceLastStep > 0.2f && Player.MovementContext.FreefallTime < 0.6f)
                {
                    _lastStepTime = Time.time;
                    _playedAtLeastOneStep = true;
                    if (Player.CheckSurface(SprintSurfaceRange))
                    {
                        BotManagerComponent.Instance.BotHearing.PlayAISound(
                            Player.ProfileId,
                            SAINSoundType.Sprint,
                            Player.Position,
                            SAINPlugin.LoadedPreset.GlobalSettings.Hearing.BaseSoundRange_Sprint,
                            Player.CalculateMovementVolumeDefaultMultiplier(EAudioMovementState.Sprint)
                        );
                    }
                }
            }
            yield return null;
        }
    }

    public override void Dispose()
    {
        if (_runCoroutine != null)
        {
            player.StopCoroutine(_runCoroutine);
        }

        if (_sprintCoroutine != null)
        {
            player.StopCoroutine(_sprintCoroutine);
        }

        base.Dispose();
    }
}
