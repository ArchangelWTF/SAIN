﻿using EFT;
using EFT.InventoryLogic;
using SAIN.Layers;
using SAIN.SAINComponent.Classes.EnemyClasses;
using SAIN.SAINComponent.Classes.Info;
using UnityEngine;

namespace SAIN.SAINComponent.Classes
{
    public class ShootDeciderClass : BotBase, IBotClass
    {
        public ShootDeciderClass(BotComponent bot) : base(bot)
        {
        }

        public void Init()
        {
        }

        public void Update()
        {
        }

        public void Dispose()
        {
        }

        public void CheckAimAndFire()
        {
            var weaponManager = BotOwner.WeaponManager;
            if (weaponManager == null)
                return;

            if (weaponManager.Selector.EquipmentSlot == EquipmentSlot.Holster
                && !weaponManager.HaveBullets
                && !weaponManager.Selector.TryChangeToMain())
            {
                selectWeapon();
            }

            if (!Bot.Aim.CanAim)
                return;

            if (_changeAimTimer < Time.time)
            {
                _changeAimTimer = Time.time + 0.5f;
                Bot.AimDownSightsController.UpdateADSstatus();
            }

            Bot.BotLight.HandleLightForEnemy();

            Vector3? target = getTarget();
            if (target != null && 
                aimAtTarget(target.Value) && 
                weaponManager.HaveBullets)
            {
                tryShoot();
            }
        }

        public void AllowUnpauseMove(bool value)
        {
            _shallUnpause = value;
        }

        private bool tryPauseForShoot(bool shallUnpause)
        {
            if (ShallPauseForShoot())
            {
                if (_nextPauseMoveTime < Time.time &&
                    !IsMovementPaused)
                {
                    _nextPauseMoveTime = Time.time + Random.Range(_pauseMoveFrequencyMin, _pauseMoveFrequencyMax);
                    Bot.Mover.PauseMovement(Random.Range(_pauseMoveDurationMin, _pauseMoveDurationMax));
                }
                if (!IsMovementPaused)
                {
                    BotOwner.AimingData?.LoseTarget();
                    return false;
                }
            }
            else if (IsMovementPaused && _shallUnpause)
            {
                //BotOwner.Mover.MovementResume();
            }
            return true;
        }

        public bool ShallPauseForShoot()
        {
            float maxPointFireDist = Bot.Info.FileSettings.Shoot.MaxPointFireDistance;
            return
                Bot.Enemy != null &&
                Bot.Enemy.RealDistance > maxPointFireDist &&
                IsAiming;
        }

        public bool IsAiming
        {
            get
            {
                return BotOwner?.ShootData?.ShootController?.IsAiming == true || BotOwner?.AimingData?.IsReady == true;
            }
        }

        private void selectWeapon()
        {
            EquipmentSlot optimalSlot = findOptimalWeaponForDistance(getDistance());
            if (currentSlot != optimalSlot)
            {
                tryChangeWeapon(optimalSlot);
            }
        }

        private EquipmentSlot currentSlot => BotOwner.WeaponManager.Selector.EquipmentSlot;

        private void tryChangeWeapon(EquipmentSlot slot)
        {
            if (_nextChangeWeaponTime < Time.time)
            {
                var selector = BotOwner?.WeaponManager?.Selector;
                if (selector != null)
                {
                    _nextChangeWeaponTime = Time.time + 1f;
                    switch (slot)
                    {
                        case EquipmentSlot.FirstPrimaryWeapon:
                            selector.TryChangeToMain();
                            break;

                        case EquipmentSlot.SecondPrimaryWeapon:
                            selector.ChangeToSecond();
                            break;

                        case EquipmentSlot.Holster:
                            selector.TryChangeWeapon(true);
                            break;

                        default:
                            break;
                    }
                }
            }
        }

        private float getDistance()
        {
            if (_nextGetDistTime < Time.time)
            {
                _nextGetDistTime = Time.time + 0.5f;
                Vector3? target = Bot.CurrentTargetPosition;
                if (target != null)
                {
                    _lastDistance = Bot.CurrentTargetDistance;
                }
            }
            return _lastDistance;
        }

        private EquipmentSlot findOptimalWeaponForDistance(float distance)
        {
            if (_nextCheckOptimalTime < Time.time)
            {
                _nextCheckOptimalTime = Time.time + 0.5f;

                var equipment = Bot.PlayerComponent.Equipment;

                float? primaryEngageDist = null;
                var primary = equipment.PrimaryWeapon;
                if (isWeaponDurableEnough(primary))
                {
                    primaryEngageDist = primary.EngagementDistance;
                }

                float? secondaryEngageDist = null;
                var secondary = equipment.SecondaryWeapon;
                if (isWeaponDurableEnough(secondary))
                {
                    secondaryEngageDist = secondary.EngagementDistance;
                }

                float? holsterEngageDist = null;
                var holster = equipment.HolsterWeapon;
                if (isWeaponDurableEnough(holster))
                {
                    holsterEngageDist = holster.EngagementDistance;
                }

                float minDifference = Mathf.Abs(distance - primaryEngageDist ?? 0);
                optimalSlot = EquipmentSlot.FirstPrimaryWeapon;

                float difference = Mathf.Abs(distance - secondaryEngageDist ?? 0);
                if (difference < minDifference)
                {
                    minDifference = difference;
                    optimalSlot = EquipmentSlot.SecondPrimaryWeapon;
                }

                if (!BotOwner.WeaponManager.HaveBullets)
                {
                    difference = Mathf.Abs(distance - holsterEngageDist ?? 0);
                    if (difference < minDifference)
                    {
                        minDifference = difference;
                        optimalSlot = EquipmentSlot.Holster;
                    }
                }
            }
            return optimalSlot;
        }

        private bool isWeaponDurableEnough(WeaponInfo info, float min = 0.5f)
        {
            return info != null &&
                info.Durability > min &&
                info.Weapon.ChamberAmmoCount > 0;
        }

        private bool aimAtTarget(Vector3 target)
        {
            var aimData = BotOwner.AimingData;
            AimStatus aimStatus = Bot.Aim.AimStatus;
            bool steerComplete = false;

            if (aimStatus == AimStatus.NoTarget)
            {
                if (!Bot.FriendlyFire.CheckFriendlyFire(target))
                {
                    BotOwner.ShootData.EndShoot();
                    return false;
                }
                steerComplete = checkSteerDirection(MIN_ANGLE_TO_START_AIM, target);
                if (!steerComplete)
                {
                    Bot.Steering.LookToPoint(target, TURN_SPEED_START_AIM);
                    return false;
                }
            }

            aimData.SetTarget(target);
            Vector3 aimTarget = aimData.EndTargetPoint;

            if (!steerComplete && 
                !checkSteerDirection(MIN_ANGLE_TO_KEEP_AIMING, aimTarget))
            {
                Bot.Steering.LookToPoint(aimTarget, TURN_SPEED_AIMING);
                return false;
            }

            aimData.NodeUpdate();

            if (!Bot.FriendlyFire.CheckFriendlyFire(aimTarget))
            {
                BotOwner.ShootData.EndShoot();
                return false;
            }
            if (Bot.NoBushESP.NoBushESPActive)
            {
                return false;
            }

            return aimData.IsReady;
        }

        private const float MIN_ANGLE_TO_START_AIM = 10f;
        private const float MIN_ANGLE_TO_KEEP_AIMING = 50f;
        private const float TURN_SPEED_START_AIM = 200f;
        private const float TURN_SPEED_AIMING = 250f;

        private bool checkSteerDirection(float maxAngle, Vector3 toPoint)
        {
            Vector3 lookdirection = Bot.LookDirection;
            Vector3 directionToTarget = (toPoint - Bot.Transform.WeaponRoot).normalized;
            float angle = Vector3.Angle(directionToTarget, lookdirection);
            return angle <= maxAngle;
        }

        private Vector3? blindShootTarget(Enemy enemy)
        {
            Vector3? result = null;
            if (!enemy.IsVisible
                    && enemy.Status.HeardRecently
                    && enemy.InLineOfSight)
            {
                EnemyPlace lastKnown = enemy.KnownPlaces.LastKnownPlace;
                if (lastKnown != null && lastKnown.CheckLineOfSight(BotOwner.LookSensor._headPoint, LayerMaskClass.HighPolyWithTerrainMask))
                {
                    result = lastKnown.Position + Vector3.up + UnityEngine.Random.onUnitSphere;
                }
            }
            return result;
        }

        private Vector3? getTarget()
        {
            Vector3? target = getAimTarget(Bot.Enemy);

            if (target == null)
            {
                target = getAimTarget(Bot.LastEnemy);
            }
            return target;
        }

        private Vector3? getAimTarget(Enemy enemy)
        {
            if (enemy != null && 
                enemy.IsVisible && 
                enemy.CanShoot)
            {
                Vector3? centerMass = findCenterMassPoint(enemy);
                Vector3? partToShoot = getEnemyPartToShoot(enemy.EnemyInfo);
                Vector3? modifiedTarget = checkYValue(centerMass, partToShoot);
                Vector3? finalTarget = modifiedTarget ?? partToShoot ?? centerMass;
                if (finalTarget != null)
                {
                    _targetEnemy = enemy;
                }
                return finalTarget;
            }
            return null;
        }

        private Vector3? checkYValue(Vector3? centerMass, Vector3? partTarget)
        {
            if (centerMass != null &&
                partTarget != null &&
                centerMass.Value.y < partTarget.Value.y)
            {
                Vector3 newTarget = partTarget.Value;
                newTarget.y = centerMass.Value.y;
                return new Vector3?(newTarget);
            }
            return null;
        }

        private Vector3? findCenterMassPoint(Enemy enemy)
        {
            if (enemy.IsAI)
            {
                return null;
            }
            if (!SAINPlugin.LoadedPreset.GlobalSettings.Aiming.AimCenterMassGlobal)
            {
                return null;
            }
            if (!Bot.Info.FileSettings.Aiming.AimCenterMass)
            {
                return null;
            }
            return enemy.CenterMass;
        }

        private Vector3? getEnemyPartToShoot(EnemyInfo enemy)
        {
            if (enemy != null)
            {
                Vector3 value;
                if (enemy.Distance < 6f)
                {
                    value = enemy.GetCenterPart();
                }
                else
                {
                    value = enemy.GetPartToShoot();
                }
                return new Vector3?(value);
            }
            return null;
        }

        private void tryShoot()
        {
            if (BotOwner.ShootData.Shoot())
            {
                _targetEnemy?.EnemyInfo?.SetLastShootTime();
            }
        }

        public bool IsMovementPaused => BotOwner?.Mover.Pause == true;
        private Enemy _targetEnemy;
        private EquipmentSlot optimalSlot;
        private float _nextCheckOptimalTime;
        private float _lastDistance;
        private float _nextGetDistTime;
        private float _nextChangeWeaponTime;
        private float _nextPauseMoveTime;
        private float _pauseMoveFrequencyMin = 2f;
        private float _pauseMoveFrequencyMax = 4f;
        private float _pauseMoveDurationMin = 0.5f;
        private float _pauseMoveDurationMax = 1f;
        private bool _shallUnpause = true;
        private float _changeAimTimer;
    }
}