using EFT;
using EFT.Ballistics;
using SAIN.Components;
using SAIN.Helpers;
using SAIN.Preset.Shared.GlobalSettings;
using SAIN.Preset.Shared.GlobalSettings.Categories;
using UnityEngine;

namespace SAIN.SAINComponent.Classes;

public class AimHitEffectClass : BotBase
{
    private Vector3 _affectVector = Vector3.zero;
    private float _affectAmount;
    private float _finishDelay = 1f;
    private bool _affectActive;
    private float _timeFinished;

    private float EFFECT_MIN_ANGLE
    {
        get { return _settings.DAMAGE_BASE_MIN_ANGLE; }
    }

    private float EFFECT_MAX_ANGLE
    {
        get { return _settings.DAMAGE_BASE_MAX_ANGLE; }
    }

    private float DAMAGE_BASELINE
    {
        get { return _settings.DAMAGE_RECEIVED_BASELINE; }
    }

    private float DAMAGE_MIN_MOD
    {
        get { return _settings.DAMAGE_MIN_MOD; }
    }

    private float DAMAGE_MAX_MOD
    {
        get { return _settings.DAMAGE_MAX_MOD; }
    }

    private float DAMAGE_MANUAL_MODIFIER
    {
        get { return _settings.DAMAGE_MANUAL_MODIFIER; }
    }

    private bool DAMAGE_USE_HIT_OFFSET_DIR
    {
        get { return _settings.USE_HIT_POINT_DIRECTION; }
    }

    private float DAMAGE_HIT_OFFSET_BASE_DIST
    {
        get { return _settings.HIT_POINT_DIRECTION_BASE_DISTANCE; }
    }

    private HitEffectSettings _settings
    {
        get { return GlobalSettingsClass.Instance.Aiming.HitEffects; }
    }

    public AimHitEffectClass(BotComponent bot)
        : base(bot) { }

    public Vector3 ApplyEffect(Vector3 dir)
    {
        if (this._affectActive)
        {
            this.decayAffect();
            Vector3 affect = this._affectVector * this._affectAmount;
            Vector3 result = dir.normalized + affect;
            //DebugGizmos.Ray(Bot.Transform.WeaponRoot, result, Color.yellow, 1f, 0.05f, true, 5f);
            return result;
        }
        return dir;
    }

    private float calcDamageMod(DamageInfo DamageInfo)
    {
        float mod = DamageInfo.Damage / DAMAGE_BASELINE;
        mod = Mathf.Clamp(mod, DAMAGE_MIN_MOD, DAMAGE_MAX_MOD) * DAMAGE_MANUAL_MODIFIER;
        if (_affectActive)
        {
            mod *= 0.5f;
        }
        return mod;
    }

    private Vector3 getHitReactionDir(DamageInfo DamageInfo)
    {
        Vector3 hitPoint = DamageInfo.HitPoint;
        //DebugGizmos.Sphere(hitPoint, 0.25f, Color.red, true, 0.25f);
        Vector3 center = Bot.Transform.BodyPosition;
        //DebugGizmos.Sphere(center, 0.25f, Color.blue, true, 0.25f);
        Vector3 offset = hitPoint - center;
        Vector3 result = offset.normalized * DAMAGE_HIT_OFFSET_BASE_DIST;
        //result.x *= 1.5f;
        //result.z *= 1.5f;
        result.y *= 0.5f;
        return result;
    }

    public void GetHit(DamageInfo DamageInfo)
    {
        float mod = calcDamageMod(DamageInfo);
        Vector3 hitReactionDir;
        if (DAMAGE_USE_HIT_OFFSET_DIR)
        {
            hitReactionDir = getHitReactionDir(DamageInfo) * mod;
            _affectVector += hitReactionDir;
        }
        else
        {
            float minAngle = Mathf.Clamp(EFFECT_MIN_ANGLE * mod, 0f, 90f);
            float maxAngle = Mathf.Clamp(EFFECT_MAX_ANGLE * mod, 0f, 90f);
            float x = UnityEngine.Random.Range(-minAngle, -maxAngle) * 0.5f;
            float y = (float)EFTMath.RandomSing() * UnityEngine.Random.Range(minAngle, maxAngle);
            Vector3 lookDir = Bot.Transform.LookDirection;
            this._affectVector = Vector.Rotate(_affectVector + lookDir, x, y, 0) - lookDir;
        }

        var aimSettings = BotOwner.Settings.FileSettings.Aiming;
        this._affectActive = true;
        this._finishDelay = aimSettings.BASE_HIT_AFFECTION_DELAY_SEC * Mathf.Clamp(mod, 0f, 1.5f) * UnityEngine.Random.Range(0.8f, 1.2f);
        this._timeFinished = Time.time + this._finishDelay;
    }

    public void decayAffect()
    {
        if (this._affectActive)
        {
            float timeRemaining = this._timeFinished - Time.time;
            if (timeRemaining <= 0f)
            {
                this._affectActive = false;
                _affectVector = Vector3.zero;
                return;
            }
            this._affectAmount = timeRemaining / this._finishDelay;
        }
    }
}
