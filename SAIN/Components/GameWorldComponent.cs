using System;
using System.Collections;
using System.Collections.Generic;
using EFT;
using EFT.Ballistics;
using EFT.Game.Spawning;
using EFT.InventoryLogic;
using SAIN.Components.PlayerComponentSpace;
using SAIN.Helpers;
using UnityEngine;

namespace SAIN.Components;

public class GameWorldComponent : MonoBehaviour
{
    public static bool TryGetPlayerComponent(IPlayer Player, out PlayerComponent PlayerComponent)
    {
        if (Player == null)
        {
            //Logger.LogError("Player Null");
            PlayerComponent = null;
            return false;
        }
        PlayerSpawnTracker PlayerTracker = Instance?.PlayerTracker;
        if (PlayerTracker == null)
        {
            //Logger.LogError("GameWorld Component Null, can't get Player Component");
            PlayerComponent = null;
            return false;
        }
        PlayerComponent = PlayerTracker.GetPlayerComponent(Player);
        return PlayerComponent != null;
    }

    public void RegisterShot(Player Player, Shot Bullet, Item Weapon)
    {
        if (TryGetPlayerComponent(Player, out PlayerComponent PlayerComponent))
        {
            StartCoroutine(TrackBullet(PlayerComponent, Bullet));
            //var OtherPlayerData = PlayerComponent.OtherPlayersData.DataHashSet;
            //Vector3 PlayerLookDir = PlayerComponent.LookDirection;
            //
            //// Add any other AI Controlled players that are in the direction this shot is going
            //_tempOtherPlayerCache.AddRange(from Data in OtherPlayerData
            //                               let OtherPlayerDirNormal = Data.DistanceData.DirectionNormal
            //                               let BotPlayerComponent = Data.PlayerComponent
            //                               where BotPlayerComponent != null && BotPlayerComponent.IsAI && BotPlayerComponent.IsActive && Vector3.Dot(OtherPlayerDirNormal, PlayerLookDir) > 0.75f
            //                               select Data);
            //
            //if (_tempOtherPlayerCache.Count > 0)
            //{
            //    List<OtherPlayerData> RelevantPlayers = [];
            //    RelevantPlayers.AddRange(_tempOtherPlayerCache);
            //    _tempOtherPlayerCache.Clear();
            //    ActiveBullets.Add(new(Bullet, PlayerComponent, RelevantPlayers));
            //}
        }
    }

    private IEnumerator TrackBullet(PlayerComponent Player, Shot Bullet)
    {
        //Vector3 LastPosition = Bullet.StartPosition;
        var OtherPlayerData = Player.OtherPlayersData.DataDictionary;
        Vector3 PlayerLookDir = Player.LookDirection;

        List<OtherPlayerData> PlayersToCheck = [];
        foreach (var Data in OtherPlayerData)
        {
            OtherPlayerData data = Data.Value;
            PlayerComponent PlayerComponent = data.OtherPlayerComponent;
            if (
                PlayerComponent?.IsAI == true
                && PlayerComponent.IsActive
                && Vector3.Dot(data.DistanceData.DirectionNormal, PlayerLookDir) > 0.75f
            )
            {
                PlayersToCheck.Add(data);
            }
        }

        const float MaxFlyByDistSqr = 10 * 10;

        if (PlayersToCheck.Count > 0)
        {
            while (!Bullet.IsShotFinished && Player?.IsActive == true && PlayersToCheck.Count > 0)
            {
                Vector3 BulletPosition = Bullet.CurrentPosition;
                //DebugGizmos.Sphere(BulletPosition, 0.3f, Color.red, true, 3.0f);
                //DebugGizmos.Line(BulletPosition, LastPosition, Color.red, 0.05f, true, 3, true);

                for (int i = PlayersToCheck.Count - 1; i >= 0; i--)
                {
                    OtherPlayerData Data = PlayersToCheck[i];
                    if (Data?.OtherPlayerComponent?.IsActive == false)
                    {
                        PlayersToCheck.RemoveAt(i);
                        continue;
                    }
                    Vector3 PlayerPosition = Data.DistanceData.Position;
                    float BulletDistSqr = (PlayerPosition - BulletPosition).sqrMagnitude;
                    if (BulletDistSqr < MaxFlyByDistSqr)
                    {
                        Data.OtherPlayerComponent.RegisterFlyBy(Player, Bullet);
                        PlayersToCheck.RemoveAt(i);
                        continue;
                    }
                }
                //LastPosition = BulletPosition;
                yield return null;
            }
        }
    }

    public static GameWorldComponent Instance { get; private set; }
    public GameWorld GameWorld { get; private set; }
    public PlayerSpawnTracker PlayerTracker { get; private set; }
    public BotManagerComponent SAINBotController { get; private set; }
    public Extract.ExtractFinderComponent ExtractFinder { get; private set; }
    public DoorHandler Doors { get; private set; }
    public LocationClass Location { get; private set; }
    public SpawnPointMarker[] SpawnPointMarkers { get; private set; }
    public JobManager JobManager { get; private set; }

    public void WorldTick(float deltaTime)
    {
        float currentTime = Time.time;
        ManualUpdate(currentTime, deltaTime);
        SAINBotController.ManualUpdate(currentTime, deltaTime);
    }

    protected void ManualUpdate(float CurrentTime, float DeltaTime)
    {
        if (_activated)
        {
            ExtractFinder.ManualUpdate(CurrentTime, DeltaTime);
            Doors.ManualUpdate(CurrentTime, DeltaTime);
            Location.ManualUpdate(CurrentTime, DeltaTime);
            findSpawnPointMarkers();
            HashSet<PlayerComponent> players = PlayerTracker?.AlivePlayerArray;
            if (players != null && players.Count > 0)
            {
                foreach (PlayerComponent Player in players)
                {
                    if (Player != null)
                    {
                        Player.ManualUpdate(CurrentTime, DeltaTime);
                    }
                }

                TickSoundCaches(players, CurrentTime);
            }
        }
    }

    private const float _Sounds_PlayerCache_Interval = 1f / 30f;
    private const float _Sounds_BotCache_Interval = 1f / 15f;

    protected void TickSoundCaches(HashSet<PlayerComponent> PlayerComponents, float CurrentTime)
    {
        if (_Sounds_PlayerCache_Time < CurrentTime)
        {
            _Sounds_PlayerCache_Time = CurrentTime + _Sounds_PlayerCache_Interval;

            foreach (PlayerComponent Player in PlayerComponents)
            {
                if (Player != null)
                {
                    UpdatePlayerSoundCache(Player);
                }
            }
        }

        if (_Sounds_BotCache_Time < CurrentTime)
        {
            _Sounds_BotCache_Time = CurrentTime + _Sounds_BotCache_Interval;

            foreach (var playerComponent in PlayerComponents)
            {
                BotComponent Bot = playerComponent?.BotComponent;
                if (Bot != null && Bot.BotOwner?.BotState == EBotState.Active)
                {
                    Bot.Hearing.HearingInput.ProcessAISoundCache();
                }
            }
        }
    }

    protected static void UpdatePlayerSoundCache(PlayerComponent Player)
    {
        List<SoundEvent> events = Player.AISoundCachedEvents;
        if (Player.IsActive)
        {
            foreach (OtherPlayerData OtherPlayerData in Player.OtherPlayersData.DataHashSet)
            {
                PlayerComponent OtherPlayer = OtherPlayerData?.OtherPlayerComponent;
                if (OtherPlayer != null && OtherPlayer.IsActive && OtherPlayer.IsSAINBot)
                {
                    BotComponent Bot = OtherPlayer.BotComponent;
                    if (Bot != null && Bot.BotOwner?.BotState == EBotState.Active)
                    {
                        bool InFootstepRadius = OtherPlayerData.IsInHearingRadius_Footsteps;
                        bool InGunfireRadius = OtherPlayerData.IsInHearingRadius_GunFire;
                        float Distance = OtherPlayerData.DistanceData.Distance;
                        foreach (var soundEvent in events)
                        {
                            bool isGunshot = soundEvent.IsGunShot;
                            if (isGunshot && !InGunfireRadius)
                            {
                                continue;
                            }

                            if (!isGunshot && !InFootstepRadius)
                            {
                                continue;
                            }

                            OtherPlayer.BotComponent?.Hearing.HearingInput.CheckAddSoundToCache(soundEvent, Distance);
                        }
                    }
                }
            }
        }
        events.Clear();
    }

    private void findSpawnPointMarkers()
    {
        if ((SpawnPointMarkers != null) || (Camera.main == null))
        {
            return;
        }

        SpawnPointMarkers = UnityEngine.Object.FindObjectsOfType<SpawnPointMarker>();

#if DEBUG
        if (SAINPlugin.DebugMode)
        {
            Logger.LogInfo($"Found {SpawnPointMarkers.Length} spawn point markers");
        }
#endif
    }

    public IEnumerable<Vector3> GetAllSpawnPointPositionsOnNavMesh()
    {
        if (SpawnPointMarkers == null)
        {
            return [];
        }

        List<Vector3> spawnPointPositions = [];
        foreach (SpawnPointMarker spawnPointMarker in SpawnPointMarkers)
        {
            // Try to find a point on the NavMesh nearby the spawn point
            Vector3? spawnPointPosition = NavMeshHelpers.GetNearbyNavMeshPoint(spawnPointMarker.Position, 2);
            if (spawnPointPosition.HasValue && !spawnPointPositions.Contains(spawnPointPosition.Value))
            {
                spawnPointPositions.Add(spawnPointPosition.Value);
            }
        }

        return spawnPointPositions;
    }

    public void Activate(BotsController botsController)
    {
        SAINBotController.DefaultController = botsController;
        SAINBotController.BotSpawner = botsController.BotSpawner;
        _activated = true;
        JobManager.Start();
        // StartCoroutine(CalcPathsJobs());
    }

    private bool _activated = false;

    public void Init(GameWorld gameWorld, BotManagerComponent sainBotController)
    {
        Instance = this;
        GameWorld = gameWorld;
        if (GameWorld == null)
        {
#if DEBUG
            Logger.LogWarning("GameWorld Null, cannot Init SAIN Gameworld! Check 2. Disposing Component...");
#endif
            DestroyComponent();
            return;
        }

        SAINBotController = sainBotController;
        PlayerTracker = new PlayerSpawnTracker(this);
        Doors = new DoorHandler(this);
        Location = new LocationClass(this);
        ExtractFinder = this.GetOrAddComponent<Extract.ExtractFinderComponent>();
        JobManager = new JobManager(this);
        GameWorld.OnDispose += DestroyComponent;

        try
        {
            EFTCoreSettings.UpdateCoreSettings();
        }
        catch (Exception e)
        {
            Logger.LogError(e);
        }

        //Logger.LogDebug("SAIN GameWorld Created.");

        Doors.Init();
        Location.Init();
    }

    public void DestroyComponent()
    {
        Instance = null;
        try
        {
            PlayerTracker?.Dispose();
            Doors?.Dispose();
            Location?.Dispose();
            JobManager?.Dispose();
        }
        catch (Exception e)
        {
            Logger.LogError($"Dispose GameWorld Component Class Error: {e}");
        }

        try
        {
            ComponentHelpers.DestroyComponent(SAINBotController);
        }
        catch (Exception e)
        {
            Logger.LogError($"Dispose GameWorld SubComponent Error: {e}");
        }

        StopAllCoroutines();
        Instance = null;
        GameWorld.OnDispose -= DestroyComponent;
        Destroy(this);
        //Logger.LogDebug("SAIN GameWorld Destroyed.");
    }

    private float _Sounds_PlayerCache_Time;
    private float _Sounds_BotCache_Time;
}
