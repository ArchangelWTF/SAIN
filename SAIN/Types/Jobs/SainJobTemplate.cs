using System.Collections;
using System.Collections.Generic;
using Comfort.Common;
using EFT;
using SAIN.Components;
using SAIN.Components.BotController;
using SAIN.Components.PlayerComponentSpace;
using UnityEngine;

namespace SAIN.Types.Jobs;

public interface ISainJob
{
    public void Start();

    public void Stop();
}

public abstract class SainJobTemplate(string InName, MonoBehaviour InOwner, bool InLooping = true, float InLoopInterval = 1.0f / 30.0f)
    : ISainJob
{
    protected readonly string Name = InName;
    protected readonly bool Looping = InLooping;
    protected readonly float LoopInterval = InLoopInterval;
    protected readonly MonoBehaviour Owner = InOwner;

    public bool Active
    {
        get { return Coroutine != null; }
    }

    protected Coroutine Coroutine;

    protected virtual IEnumerator Loop()
    {
#if DEBUG
        Logger.LogDebug($"Starting Job: [{Name}]");
#endif
        WaitForSeconds Wait = new(LoopInterval);
        while (LoopCondition())
        {
            if (!CanProceed())
            {
                yield return Wait;
                continue;
            }
            yield return PrimaryFunction();
            yield return Wait;
        }
#if DEBUG
        Logger.LogDebug($"Job Ended [{Name}]");
#endif
    }

    protected virtual IEnumerator PrimaryFunction()
    {
        yield return null;
    }

    protected virtual bool LoopCondition()
    {
        return true;
    }

    protected virtual bool CanProceed()
    {
        return true;
    }

    public virtual void Start()
    {
        if (!Active)
        {
            if (Owner == null)
            {
#if DEBUG
                Logger.LogError("Owner Null. Cannot Start.");
#endif
            }
            else
            {
                Coroutine = Owner.StartCoroutine(Loop());
            }
        }
    }

    public virtual void Stop()
    {
        if (Active)
        {
            if (Owner == null)
            {
#if DEBUG
                Logger.LogError("Owner Null. Cannot Stop Coroutine.");
#endif
            }
            else
            {
                Owner.StopCoroutine(Coroutine);
            }
        }
    }

    protected static GameWorld GameWorld
    {
        get { return Singleton<GameWorld>.Instance; }
    }

    protected static IBotGame BotGame
    {
        get { return Singleton<IBotGame>.Instance; }
    }

    protected static GameWorldComponent SAINGameWorld
    {
        get { return GameWorldComponent.Instance; }
    }

    protected static BotManagerComponent SAINBotController
    {
        get { return BotManagerComponent.Instance; }
    }

    protected static Dictionary<string, PlayerComponent> AlivePlayers
    {
        get { return GameWorldComponent.Instance?.PlayerTracker?.AlivePlayersDictionary; }
    }

    protected static List<IPlayer> DeadPlayers
    {
        get { return GameWorldComponent.Instance?.PlayerTracker?.DeadPlayers; }
    }

    protected static Dictionary<string, BotComponent> AliveBots
    {
        get { return BotSpawnController.Instance?.BotDictionary; }
    }

    protected static bool GameActive
    {
        get
        {
            return GameStatus switch
            {
                GameStatus.Running or GameStatus.Runned or GameStatus.Starting or GameStatus.Started => true,
                _ => false,
            };
        }
    }

    protected static GameStatus GameStatus
    {
        get
        {
            if (BotGame == null)
            {
                return GameStatus.Stopped;
            }
            return BotGame.Status;
        }
    }
}
