using System;
using System.Collections;
using System.Collections.Generic;
using EFT;
using EFT.Ballistics;
using EFT.InventoryLogic;
using SAIN.Classes;
using SAIN.Components.BotController;
using SAIN.Components.BotControllerSpace.Classes.Raycasts;
using SAIN.Components.PlayerComponentSpace.Classes;
using SAIN.Components.PlayerComponentSpace.Classes.Equipment;
using SAIN.Components.PlayerComponentSpace.PersonClasses;
using SAIN.Helpers;
using SAIN.Preset.Shared.Enums;
using SAIN.SAINComponent.Classes.Info;
using UnityEngine;

namespace SAIN.Components.PlayerComponentSpace;

public class PlayerComponent : MonoBehaviour, IDisposable, ISPlayer
{
    public Vector3 NavMeshPosition
    {
        get { return Transform.NavData.Position; }
    }

    private const int MaxCachedSounds = 4;

    public event Action<WeaponInfo, Vector3> OnShoot;

    public event Action<PlayerComponent, Shot> OnBulletFlyBy;

    public event Action<PlayerComponent> OnComponentDestroyed;

    public string ProfileId { get; private set; }
    public FlashLightClass Flashlight { get; private set; }
    public SAINAIData AIData { get; private set; }
    public SAINEquipmentClass Equipment { get; private set; }
    public PlayerTransformClass Transform { get; } = new PlayerTransformClass();

    public bool IsActive
    {
        get { return ActivationClass.PlayerActive; }
    }

    public Vector3 Position
    {
        get { return Transform.Position; }
    }

    public Vector3 LookDirection
    {
        get { return Transform.LookDirection; }
    }

    public void InitializeBotOwner(BotOwner botOwner)
    {
        if (botOwner == null)
        {
#if DEBUG
            Logger.LogWarning($"{Name} : Null BotOwner, cannot Initialize!");
#endif
            return;
        }
        BotOwner = botOwner;
        IsAI = true;
        Name = botOwner.name;
    }

    public void InitializeBotComponent(BotComponent bot)
    {
        if (bot == null)
        {
#if DEBUG
            Logger.LogWarning($"{Name} : Null BotComponent, cannot Initialize!");
#endif
            return;
        }
        BotComponent = bot;
        IsSAINBot = true;
    }

    public string Name { get; private set; }
    public PersonActiveClass ActivationClass { get; private set; }
    public bool IsAI { get; private set; }
    public bool IsSAINBot { get; private set; }
    public BotOwner BotOwner { get; private set; }
    public BotComponent BotComponent { get; private set; }
    public Player Player { get; private set; }

    public PlayerMovementController CharacterController { get; private set; } = new();
    public PlayerTickData PlayerTickData { get; private set; }
    public OtherPlayersData OtherPlayersData { get; private set; }
    public BodyPartsClass BodyParts { get; private set; }
    public PlayerSoundController SoundController { get; private set; }

    public event Action<Weapon, Weapon> OnWeaponEquipped;

    public Weapon CurrentWeapon
    {
        get { return _currentWeapon; }
        private set
        {
            if (_currentWeapon != value)
            {
                Weapon LastWeapon = _currentWeapon;
#if DEBUG
                Logger.LogDebug(
                    $"[{Player?.Profile.Nickname}] Equipped Weapon [{value?.ShortName?.Localized()}] Last Weapon [{LastWeapon?.ShortName?.Localized()}]"
                );
#endif
                _currentWeapon = value;
                OnWeaponEquipped?.Invoke(value, LastWeapon);
            }
        }
    }

    public event Action<Item, Item> OnItemEquipped;

    public Item ItemInHands
    {
        get { return _currentItem; }
        private set
        {
            if (_currentItem != value)
            {
                Item LastItem = _currentItem;
#if DEBUG
                Logger.LogDebug(
                    $"[{Player?.Profile.Nickname}] Equipped Item [{value?.ShortName?.Localized()}] Last Item [{LastItem?.ShortName?.Localized()}]"
                );
#endif
                _currentItem = value;
                OnItemEquipped?.Invoke(value, LastItem);
            }
        }
    }

    public List<SoundEvent> AISoundCachedEvents { get; private set; } = [];

    public void SetItemEquippedInHands(Item Item)
    {
        ItemInHands = Item;
        CurrentWeapon = Item as Weapon;
    }

    public void PlayAISound(
        SAINSoundType InSoundType,
        Vector3 InPosition,
        float InRange,
        float InVolume,
        EPhraseTrigger Phrase = EPhraseTrigger.None,
        ETagStatus TagStatus = ETagStatus.Unaware
    )
    {
        if (IsActive && AIData.AISoundPlayer.ShallPlayAISound())
        {
#if DEBUG
            if (Player.IsYourPlayer)
            {
                Logger.LogDebug($"Sound Cached: [{InSoundType}, {InRange}, {InVolume}]");
            }
#endif
            AddCachedAISoundEvent(InSoundType, InPosition, InRange, InVolume, Phrase, TagStatus);
        }
    }

    protected void AddCachedAISoundEvent(
        SAINSoundType InSoundType,
        Vector3 InPosition,
        float InRange,
        float InVolume,
        EPhraseTrigger Phrase = EPhraseTrigger.None,
        ETagStatus TagStatus = ETagStatus.Unaware
    )
    {
        float SoundSpeed = 343;
        if (InSoundType.IsGunShot())
        {
            WeaponInfo Weapon = Equipment?.CurrentWeaponInfo;
            if (Weapon != null)
            {
                SoundSpeed = Weapon.BulletSpeed;
            }
            InVolume *= SAINPlugin.LoadedPreset.GlobalSettings.Hearing.GunshotAudioMultiplier;
        }
        else
        {
            InVolume *= SAINPlugin.LoadedPreset.GlobalSettings.Hearing.FootstepAudioMultiplier;
        }
        if (!AIData.PlayerLocation.InBunker)
        {
            var weather = SAINWeatherClass.Instance;
            if (weather != null)
            {
                if (Player.AIData.EnvironmentId == 0)
                {
                    InVolume *= weather.RainSoundModifierOutdoor;
                }
                else
                {
                    InVolume *= weather.RainSoundModifierIndoor;
                }
            }
        }
        int Count = AISoundCachedEvents.Count;
        if (Count >= MaxCachedSounds)
        {
            bool ShallInsert = false;
            float BaseRange = InRange * InVolume;
            for (int i = 0; i < Count; i++)
            {
                if (BaseRange < AISoundCachedEvents[i].BaseRangeWithVolume)
                {
                    continue;
                }
                ShallInsert = true;
                break;
            }
            if (!ShallInsert)
            {
                return;
            }
            AISoundCachedEvents.Add(new(InSoundType, InPosition, this, InRange, InVolume, SoundSpeed, Phrase, TagStatus));
            AISoundCachedEvents.Sort((a, b) => b.BaseRangeWithVolume.CompareTo(a.BaseRangeWithVolume));
            AISoundCachedEvents.RemoveAt(AISoundCachedEvents.Count - 1);
            return;
        }
        AISoundCachedEvents.Add(new(InSoundType, InPosition, this, InRange, InVolume, SoundSpeed, Phrase, TagStatus));
    }

    public float GetDistanceToPlayer(string ProfileId)
    {
        if (OtherPlayersData.DataDictionary.TryGetValue(ProfileId, out var Data))
        {
            return Data.DistanceData.Distance;
        }
        return float.MaxValue;
    }

    public bool IsPlayerInRange(string ProfileId, float maxDistance, out float playerDistance)
    {
        playerDistance = GetDistanceToPlayer(ProfileId);
        return playerDistance <= maxDistance;
    }

    public void ManualUpdate(float currentTime, float deltaTime)
    {
        ActivationClass.CheckActive(this);
        if (IsActive)
        {
            bool isAI = BotOwner != null;
            Transform.ManualUpdate(Player, isAI);
            if (!isAI || (BotOwner.BotState == EBotState.Active && BotOwner.StandBy.StandByType == BotStandByType.active))
            {
#if DEBUG
                drawTransformGizmos();
#endif
                Flashlight.Update();
                Equipment.Update();
            }
        }
    }

    public PlayerTickData GetPreparedTickData()
    {
        var data = PlayerTickData;
        data.Prepare(this);
        PlayerTickData = data;
        return data;
    }

    public void SetTickData(PlayerTickData data)
    {
        PlayerTickData = data;
    }

    public void RegisterFlyBy(PlayerComponent Source, Shot Bullet)
    {
        OnBulletFlyBy?.Invoke(Source, Bullet);
    }

    public void OnMakingShot(IWeapon Weapon, Vector3 Force)
    {
        if (IsActive)
        {
            WeaponInfo WeaponInfo = Equipment.CurrentWeaponInfo;
            if (WeaponInfo != null)
            {
                OnShoot?.Invoke(WeaponInfo, Force);
                PlayAISound(WeaponInfo.SoundType, Transform.WeaponData.FirePort, WeaponInfo.CalculatedAudibleRange, 1);
            }
        }
    }

    private void StartCoroutines()
    {
        _gearCoroutine ??= StartCoroutine(Equipment.GearInfo.GearUpdateLoop());
    }

    public bool Init(IPlayer iPlayer)
    {
        try
        {
            ProfileId = iPlayer.ProfileId;
            Player = iPlayer as Player;
            Name = Player.name;

            ActivationClass = new PersonActiveClass(this);

            OtherPlayersData = new OtherPlayersData(this);
            PlayerTickData = new PlayerTickData(this);
            BodyParts = new BodyPartsClass(this);
            Flashlight = new FlashLightClass(this);
            Equipment = new SAINEquipmentClass(this);
            AIData = new SAINAIData(Equipment.GearInfo, this);
            SoundController = new PlayerSoundController(this);
            Player.MovementContext.OnStateChanged += SoundController.HandleMovementState;
            ActivationClass.OnPlayerActiveChanged += HandleCoroutines;
            HandleCoroutines(true);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex);
            return false;
        }
        //Logger.LogDebug($"{Person.Nickname} Player Component Created");
        StartCoroutine(DelayInit());
        //StartCoroutine(voiceTest());
        return true;
    }

    private void HandleCoroutines(bool active)
    {
        if (active)
        {
            StartCoroutines();
        }
        else
        {
            StopAllCoroutines();
            _gearCoroutine = null;
        }
    }

    private IEnumerator DelayInit()
    {
        yield return null;
        Equipment.Init();
        if (Player.HandsController?.Item != null)
        {
            SetItemEquippedInHands(Player.HandsController.Item);
        }
    }

    private void OnDisable()
    {
        ActivationClass.Disable();
        StopAllCoroutines();
    }

    public void Dispose()
    {
#if DEBUG
        Logger.LogDebug(
            $"Destroying Playing Component for [Name: {Name} : Nickname: {Player?.Profile?.Nickname}, ProfileID: {ProfileId}, at time: {Time.time}]"
        );
#endif
        OnComponentDestroyed?.Invoke(this);
        StopAllCoroutines();
        ActivationClass.Disable();
        ActivationClass.OnPlayerActiveChanged -= HandleCoroutines;
        Equipment?.Dispose();
        OtherPlayersData?.Dispose();
        Player.MovementContext.OnStateChanged -= SoundController.HandleMovementState;
        SoundController.Dispose();
        Destroy(this);
    }

    public bool PlayVoiceLine(EPhraseTrigger phrase, ETagStatus mask, bool aggressive)
    {
        var speaker = Player.Speaker;
        if (speaker.Speaking || speaker.Busy)
        {
            return false;
        }

        //if (aggressive &&
        //    speaker.PhrasesBanks.TryGetValue(phrase, out var phrasesBank)) {
        //    _aggroIndexes.Clear();
        //    int count = phrasesBank.Clips.Length;
        //    for (int i = 0; i < count; i++) {
        //        if (phrasesBank.Clips[i].Clip.name.EndsWith("_bl")) {
        //            _aggroIndexes.Add(i);
        //        }
        //    }
        //
        //    if (_aggroIndexes.Count > 0) {
        //        int index = _aggroIndexes.PickRandom();
        //        speaker.PlayDirect(phrase, index);
        //        //Logger.LogInfo($"{phrase} :: {phrasesBank.Clips[index].Clip.name} :: {index}");
        //        return true;
        //    }
        //}

        return speaker.Play(phrase, mask, true, null) != null;
    }

    private void drawTransformGizmos()
    {
        if (SAINPlugin.DebugSettings.Gizmos.DrawTransformGizmos)
        {
            DebugGizmos.DrawSphere(Transform.EyePosition, 0.1f, Color.white, 0.1f, "Eye");
            DebugGizmos.Ray(Transform.EyePosition, Transform.LookDirection, Color.white, 1f, 0.025f, 0.1f);

            DebugGizmos.DrawSphere(Transform.WeaponData.WeaponRoot, 0.075f, Color.magenta, 0.1f, "WeaponRoot");
            DebugGizmos.Ray(Transform.WeaponData.WeaponRoot, Transform.LookDirection, Color.magenta, 1f, 0.025f, 0.1f);

            DebugGizmos.DrawSphere(Transform.HeadData.HeadPosition, 0.075f, Color.yellow, 0.1f, "Head");
            DebugGizmos.Ray(Transform.HeadData.HeadPosition, Transform.HeadData.HeadLookDirection, Color.yellow, 1f, 0.025f, 0.1f);

            DebugGizmos.DrawSphere(Transform.WeaponData.FirePort, 0.075f, Color.green, 0.1f, "FirePort");
            DebugGizmos.Ray(Transform.WeaponData.FirePort, Transform.WeaponData.PointDirection, Color.green, 1f, 0.05f, 0.1f);

            DebugGizmos.DrawSphere(Transform.BodyPosition, 0.1f, Color.blue, 0.1f, "Body");
        }
    }

    private Coroutine _gearCoroutine;
    private Item _currentItem = null;
    private Weapon _currentWeapon = null;
}
