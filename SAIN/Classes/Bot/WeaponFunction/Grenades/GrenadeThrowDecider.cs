using EFT;
using SAIN.Preset;
using SAIN.SAINComponent.Classes.EnemyClasses;
using UnityEngine;

namespace SAIN.SAINComponent.Classes.WeaponFunction;

public class GrenadeThrowDecider : BotSubClass<BotGrenadeManager>, IBotDecisionClass
{
    private const float MIN_THROW_DISPERSION = 0.5f;
    private const float MAX_THROW_DISPERSION = 5f;
    private const float MIN_THROW_DISTANCE_DISPERSION = 10f;
    private const float MAX_THROW_DISTANCE_DISPERSION = 50f;

    private float _nextSayNeedGrenadeTime;
    private float _minThrowDistPercent;
    private float _maxPower
    {
        get { return BotOwner.WeaponManager.Grenades.MaxPower; }
    }

    private float _nextPossibleAttempt;

    private static readonly AIGreandeAng[] _indoorAngles =
    [
        AIGreandeAng.ang5,
        AIGreandeAng.ang15,
        //AIGreandeAng.ang25,
        //AIGreandeAng.ang35,
    ];

    private static readonly AIGreandeAng[] _outdoorAngles =
    [
        AIGreandeAng.ang15,
        AIGreandeAng.ang25,
        AIGreandeAng.ang35,
        AIGreandeAng.ang45,
        //AIGreandeAng.ang55,
        //AIGreandeAng.ang65,
    ];

    private readonly AIGreanageThrowData[] _validThrowsBuffer = new AIGreanageThrowData[6];

    public GrenadeThrowDecider(BotGrenadeManager ThrowWeap)
        : base(ThrowWeap) { }

    protected override void UpdatePresetSettings(SAINPresetClass preset)
    {
        _grenadesEnabled = preset.GlobalSettings.General.BotsUseGrenades;

        var sainSettings = Bot.Info.FileSettings;
        _canThrowGrenades = sainSettings.Core.CanGrenade;
        _canThrowAtVisEnemies = sainSettings.Grenade.CAN_THROW_STRAIGHT_CONTACT;
        _canThrowWhileSprint = sainSettings.Grenade.CanThrowWhileSprinting;
        _minEnemyDistToThrow = sainSettings.Grenade.MinEnemyDistance;
        _minFriendlyDistToThrow = sainSettings.Grenade.MinFriendlyDistance;
        _minFriendlyDistToThrow_SQR = _minFriendlyDistToThrow * _minFriendlyDistToThrow;
        _throwGrenadeFreq = sainSettings.Grenade.ThrowGrenadeFrequency;
        _throwGrenadeFreqMax = sainSettings.Grenade.ThrowGrenadeFrequency_MAX;
        _minThrowDistPercent = 0.66f;

        _blindCornerDistToThrow = 5f;
        _blindCornerDistToLastKnown_Max_SQR = _blindCornerDistToThrow * _blindCornerDistToThrow;
        _checkThrowPos_HeightOffset = 0.25f;
    }

    private bool _grenadesEnabled = true;
    private bool _canThrowGrenades = true;
    private bool _canThrowAtVisEnemies = false;
    private bool _canThrowWhileSprint = false;
    private float _timeSinceSeenBeforeThrow = 3f;
    private float _maxTimeSinceUpdatedCanThrow = 120f;
    private float _minEnemyDistToThrow = 8f;
    private float _throwGrenadeFreq = 5f;
    private float _throwGrenadeFreqMax = 10f;
    private float _minFriendlyDistToThrow = 8f;
    private float _minFriendlyDistToThrow_SQR = 64;
    private float _blindCornerDistToThrow = 5f;
    private float _blindCornerDistToLastKnown_Max_SQR = 25f;
    private float _checkThrowPos_HeightOffset = 0.25f;
    private float _maxEnemyDistToCheckThrow = 75f;
    private float _friendlyCloseRecheckTime = 3f;
    private float _sayNeedGrenadeFreq = 10f;
    private float _sayNeedGrenadeChance = 5f;

    public bool GetDecision(Enemy enemy, out string reason)
    {
        if (enemy.IsAI && !GlobalSettings.General.BotVsBotGrenade)
        {
            reason = "noGoodTarget";
            return false;
        }
        if (!_grenadesEnabled || !_canThrowGrenades)
        {
            reason = "grenadesDisabled";
            return false;
        }
        if (BotOwner.WeaponManager?.Grenades.ThrowindNow == true)
        {
            reason = "throwingNow";
            return true;
        }
        if (!CheckCanThrow(out reason))
        {
            return false;
        }
        if (!CanThrowAtEnemy(enemy, out reason))
        {
            return false;
        }
        var grenades = BotOwner.WeaponManager.Grenades;
        if (!grenades.HaveGrenade)
        {
            _nextPossibleAttempt = Time.time + Random.Range(_throwGrenadeFreq, _throwGrenadeFreqMax);

            if (_nextSayNeedGrenadeTime < Time.time)
            {
                _nextSayNeedGrenadeTime = Time.time + _sayNeedGrenadeFreq;
                Bot.Talk.GroupSay(EPhraseTrigger.NeedFrag, null, true, _sayNeedGrenadeChance);
            }

            reason = "noNades";
            return false;
        }

        if (FindThrowTarget(enemy) && TryThrowGrenade())
        {
            _nextPossibleAttempt = Time.time + Random.Range(_throwGrenadeFreq, _throwGrenadeFreqMax);
            reason = "startThrow";
            return true;
        }
        reason = "noGoodTarget";
        return false;
    }

    private bool CheckCanThrow(out string reason)
    {
        var weaponManager = BotOwner.WeaponManager;
        if (weaponManager != null)
        {
            if (weaponManager.Selector.IsChanging)
            {
                reason = "changingWeapon";
                return false;
            }
            if (weaponManager.Reload.Reloading)
            {
                reason = "reloading";
                return false;
            }
        }

        if (_nextPossibleAttempt > Time.time)
        {
            reason = "nextAttemptTime";
            return false;
        }
        if (!_canThrowWhileSprint && (Player.IsSprintEnabled || Bot.Mover.Running))
        {
            reason = "running";
            return false;
        }
        if (Player.HandsController.IsInInteractionStrictCheck())
        {
            reason = "handsController Busy";
            return false;
        }
        reason = "canThrow";
        return true;
    }

    private bool CanThrowAtEnemy(Enemy enemy, out string reason)
    {
        if (!_canThrowAtVisEnemies)
        {
            if (enemy.IsVisible || enemy.InLineOfSight)
            {
                reason = "enemyVisible";
                return false;
            }
            if (enemy.TimeSinceSeen < _timeSinceSeenBeforeThrow)
            {
                reason = "enemySeenRecent";
                return false;
            }
        }
        if (enemy.TimeSinceLastKnownUpdated > _maxTimeSinceUpdatedCanThrow)
        {
            reason = "lastUpdatedTooLong";
            return false;
        }
        var lastKnown = enemy.KnownPlaces.LastKnownPlace;
        if (lastKnown == null)
        {
            reason = "nullLastKnown";
            return false;
        }
        if (lastKnown.DistanceToBot > _maxEnemyDistToCheckThrow)
        {
            reason = "tooFar";
            return false;
        }
        if (lastKnown.DistanceToBot < _minEnemyDistToThrow)
        {
            reason = "tooClose";
            return false;
        }
        reason = string.Empty;
        return true;
    }

    private bool FindThrowTarget(Enemy enemy)
    {
        EnemyPlace lastKnown = enemy.KnownPlaces.LastKnownPlace;
        if (lastKnown != null)
        {
            Vector3 lastKnownPos = lastKnown.Position;
            if (!CheckFriendlyDistances(lastKnownPos))
            {
                return false;
            }
            var angles = Bot.Memory.Location.IsIndoors ? _indoorAngles : _outdoorAngles;
            if (TryThrowToPos(lastKnownPos, "LastKnownPosition", lastKnown.DistanceToBot, angles))
            {
                return true;
            }
            if (CheckCanThrowBlindCorner(enemy, lastKnownPos))
            {
                return true;
            }
        }
        return false;
    }

    private bool CheckCanThrowBlindCorner(Enemy enemy, Vector3 lastKnownPos)
    {
        Vector3? blindCorner = enemy.VisiblePathPoint;
        if (blindCorner == null)
        {
            return false;
        }
        Vector3 blindCornerPos = blindCorner.Value;
        float sqrMag = (blindCornerPos - lastKnownPos).sqrMagnitude;
        if (sqrMag > _blindCornerDistToLastKnown_Max_SQR)
        {
            return false;
        }
        if (!CheckFriendlyDistances(blindCornerPos))
        {
            return false;
        }
        if (TryThrowToPos(blindCornerPos, "BlindCornerToEnemy", Mathf.Sqrt(sqrMag), AIGreandeAng.ang5))
        {
            return true;
        }
        return false;
    }

    private bool TryThrowToPos(Vector3 pos, string posString, float distance, params AIGreandeAng[] possibleAngles)
    {
        pos += Vector3.up * _checkThrowPos_HeightOffset;
        var weaponRoot = Bot.Transform.WeaponData.WeaponRoot;
        Vector3 throwDir = (pos - Bot.Position).normalized;
        float dispersion = GetThrowDispersion(distance);
        float heightJitter = GetHeightJitter(distance);
        Vector3 targetPos = Randomize(pos, throwDir, dispersion) + Vector3.up * heightJitter;

        return CanThrowAGrenade(weaponRoot, targetPos, possibleAngles);
    }

    private float GetThrowDispersion(float range)
    {
        float dispersionMin = MIN_THROW_DISPERSION;
        if (range <= MIN_THROW_DISTANCE_DISPERSION)
        {
            return dispersionMin;
        }
        float dispersionMax = MAX_THROW_DISPERSION;
        if (range >= MAX_THROW_DISTANCE_DISPERSION)
        {
            return dispersionMax;
        }
        range = Mathf.Clamp(range, MIN_THROW_DISTANCE_DISPERSION, MAX_THROW_DISTANCE_DISPERSION);
        float num = MAX_THROW_DISTANCE_DISPERSION - MIN_THROW_DISTANCE_DISPERSION;
        float num2 = range - MIN_THROW_DISTANCE_DISPERSION;
        float ratio = num2 / num;
        float result = Mathf.Lerp(dispersionMin, dispersionMax, ratio);
        return result;
    }

    private Vector3 Randomize(Vector3 target, Vector3 targetDirectionNormal, float dispersion)
    {
        Vector3 lateral = Vector3.Cross(targetDirectionNormal, Vector3.up).normalized;
        float lateralOffset = Random.Range(-dispersion * 0.5f, dispersion * 0.5f);
        float depthOffset = Random.Range(-dispersion, dispersion);

        return target + (targetDirectionNormal * depthOffset) + (lateral * lateralOffset);
    }

    private float GetHeightJitter(float distance)
    {
        float t = Mathf.InverseLerp(5f, 30f, distance);

        float maxJitter = Mathf.Lerp(0.05f, 0.40f, t);

        if (Bot.Memory.Location.IsIndoors)
        {
            maxJitter *= 0.5f;
        }

        float skill = Mathf.Clamp01(Bot.Info.Profile.DifficultyModifier);
        maxJitter *= Mathf.Lerp(1.1f, 0.85f, skill);

        return Random.Range(-maxJitter, maxJitter);
    }

    private bool TryThrowGrenade()
    {
        var grenades = BotOwner.WeaponManager.Grenades;
        if (!grenades.ReadyToThrow)
        {
            return false;
        }

        if (!grenades.AIGreanageThrowData.IsUpToDate())
        {
            return false;
        }

        if (grenades.DoThrow())
        {
            return true;
        }

        return false;
    }

    private bool CanThrowAGrenade(Vector3 from, Vector3 target, params AIGreandeAng[] possibleAngles)
    {
        if (_nextPossibleAttempt > Time.time)
        {
            return false;
        }

        if (!_canThrowWhileSprint && (Player.IsSprintEnabled || Bot.Mover.Running))
        {
            return false;
        }

        if (!CheckFriendlyDistances(target))
        {
            _nextPossibleAttempt = Time.time + _friendlyCloseRecheckTime;
            return false;
        }

        if (possibleAngles == null || possibleAngles.Length == 0)
        {
            return false;
        }

        int validCount = 0;

        for (int i = 0; i < possibleAngles.Length; i++)
        {
            AIGreandeAng angle = possibleAngles[i];
            AIGreanageThrowData data = AIGrenadeHelper.CanThrowGrenade2(from, target, _maxPower * 0.9f, angle, -1f, _minThrowDistPercent);

            if (data.CanThrow)
            {
                _validThrowsBuffer[validCount] = data;
                validCount++;
            }
        }

        if (validCount == 0)
        {
            return false;
        }

        int bestIndex = 0;
        float bestForce = _validThrowsBuffer[0].Force;

        for (int i = 1; i < validCount; i++)
        {
            if (_validThrowsBuffer[i].Force < bestForce)
            {
                bestForce = _validThrowsBuffer[i].Force;
                bestIndex = i;
            }
        }

        int chosenIndex;

        if (Random.value < GetThrowBestRange())
        {
            chosenIndex = bestIndex;
        }
        else
        {
            chosenIndex = Random.Range(0, validCount);
        }

        BotOwner.WeaponManager.Grenades.SetThrowData(_validThrowsBuffer[chosenIndex]);
        return true;
    }

    private bool CheckFriendlyDistances(Vector3 target)
    {
        var members = Bot.Squad.Members;
        if (members == null || members.Count <= 1)
        {
            return true;
        }

        foreach (var member in members.Values)
        {
            if (member != null && (member.Position - target).sqrMagnitude < _minFriendlyDistToThrow_SQR)
            {
                return false;
            }
        }

        return true;
    }

    private float GetThrowBestRange()
    {
        return Mathf.Lerp(0.40f, 0.80f, Bot.Info.Profile.DifficultyModifier);
    }
}
