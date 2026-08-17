using System;
using System.Collections.Generic;
using Comfort.Common;
using EFT;
using EFT.EnvironmentEffect;
using SAIN.BotController.Classes;
using SAIN.Components.BotController;
using SAIN.Components.BotControllerSpace.Classes;
using UnityEngine;

namespace SAIN.Components;

public class BotManagerComponent : MonoBehaviour
{
    public static BotManagerComponent Instance { get; private set; }

    public Dictionary<string, BotComponent> Bots
    {
        get { return BotSpawnController.Bots; }
    }

    public GameWorld GameWorld
    {
        get { return SAINGameWorld.GameWorld; }
    }

    public IBotGame BotGame
    {
        get { return Singleton<IBotGame>.Instance; }
    }

    public GlobalEventDispatcher BotEventHandler
    {
        get
        {
            if (_eventHandler == null)
            {
                _eventHandler = Singleton<GlobalEventDispatcher>.Instance;
                if (_eventHandler != null)
                {
                    GrenadeController.Subscribe(_eventHandler);
                }
            }
            return _eventHandler;
        }
    }

    private GlobalEventDispatcher _eventHandler;

    public GameWorldComponent SAINGameWorld { get; private set; }
    public BotsController DefaultController { get; set; }

    public BotSpawner BotSpawner
    {
        get { return _spawner; }
        set
        {
            BotSpawnController.Subscribe(value);
            _spawner = value;
        }
    }

    private BotSpawner _spawner;
    public GrenadeController GrenadeController { get; private set; }
    public BotJobsClass BotJobs { get; private set; }
    public BotExtractManager BotExtractManager { get; private set; }
    public TimeClass TimeVision { get; private set; }
    public SAINWeatherClass WeatherVision { get; private set; }
    public BotSpawnController BotSpawnController { get; private set; }
    public BotSquads BotSquads { get; private set; }
    public BotHearingClass BotHearing { get; private set; }

    public void PlayerEnviromentChanged(string profileID, IndoorTrigger trigger)
    {
        SAINGameWorld.PlayerTracker.GetPlayerComponent(profileID)?.AIData.PlayerLocation.UpdateEnvironment(trigger);
    }

    public void Activate(GameWorldComponent gameWorldComp)
    {
        Instance = this;
        SAINGameWorld = gameWorldComp;
        BotSpawnController = new BotSpawnController(this);
        BotExtractManager = new BotExtractManager(this);
        TimeVision = new TimeClass(this);
        WeatherVision = new SAINWeatherClass(this);
        BotSquads = new BotSquads(this);
        BotHearing = new BotHearingClass(this);
        BotJobs = new BotJobsClass(this);
        GrenadeController = new GrenadeController(this);
        GameWorld.OnDispose += Dispose;
    }

    public void ManualUpdate(float currentTime, float deltaTime)
    {
        BotSpawnController.ManualUpdate(currentTime, deltaTime);
        BotExtractManager.Update(currentTime, deltaTime);
        TimeVision.Update(currentTime, deltaTime);
        WeatherVision.Update(currentTime, deltaTime);
        BotSquads.Update(currentTime, deltaTime);

        HashSet<BotComponent> BotsArray = BotSpawnController.SAINBots;
        foreach (BotComponent BotComponent in BotsArray)
        {
            if (BotComponent != null)
            {
                BotComponent.ManualUpdate(currentTime, deltaTime);
            }
        }
    }

    public void Dispose()
    {
        try
        {
            GameWorld.OnDispose -= Dispose;
            StopAllCoroutines();
            BotJobs.Dispose();
            BotSpawnController.UnSubscribe();

            if (BotEventHandler != null)
            {
                GrenadeController.UnSubscribe(BotEventHandler);
            }

            if (Bots != null && Bots.Count > 0)
            {
                foreach (var bot in Bots.Values)
                {
                    bot?.Dispose();
                }
            }

            Bots?.Clear();
        }
        catch (Exception ex)
        {
            Logger.LogError($"Dispose SAIN BotController Error: {ex}");
        }

        Destroy(this);
    }

    public bool GetSAIN(BotOwner botOwner, out BotComponent bot)
    {
        bot = BotSpawnController.GetSAIN(botOwner);
        return bot != null;
    }
}
