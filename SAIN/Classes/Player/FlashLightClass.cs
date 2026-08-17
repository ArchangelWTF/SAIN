using System;
using System.Collections.Generic;
using EFT;
using HarmonyLib;
using SAIN.Components.PlayerComponentSpace;
using SAIN.SAINComponent;
using UnityEngine;

namespace SAIN.Components;

public class FlashLightClass(PlayerComponent component) : PlayerComponentBase(component)
{
    public event Action<bool> OnLightToggle;

    public event Action<bool> OnLaserToggle;

    public List<TacticalComboVisualController> TacticalDevices { get; private set; }

    public bool UsingLight { get; private set; }
    public bool UsingLaser { get; private set; }

    public bool LaserOnly
    {
        get { return !WhiteLight && !IRLight && (Laser || IRLaser); }
    }

    public bool DeviceActive
    {
        get { return ActiveModes != 0; }
    }

    public bool IRLaser
    {
        get { return (ActiveModes & DeviceMode.IRLaser) != 0; }
    }

    public bool IRLight
    {
        get { return (ActiveModes & DeviceMode.IRLight) != 0; }
    }

    public bool Laser
    {
        get { return (ActiveModes & DeviceMode.VisibleLaser) != 0; }
    }

    public bool WhiteLight
    {
        get { return (ActiveModes & DeviceMode.WhiteLight) != 0; }
    }

    public LightDetectionClass LightDetection { get; } = new LightDetectionClass(component);

    public void Update() { }

    public void CheckDevice()
    {
        CheckUsingLightModes();

        bool wasUsingLight = UsingLight;
        UsingLight = (ActiveModes & (DeviceMode.WhiteLight | DeviceMode.IRLight)) == (DeviceMode.WhiteLight | DeviceMode.IRLight);
        if (wasUsingLight != UsingLight)
        {
            OnLightToggle?.Invoke(UsingLight);
        }

        bool wasUsingLaser = UsingLaser;
        UsingLaser = (ActiveModes & (DeviceMode.VisibleLaser | DeviceMode.IRLaser)) == (DeviceMode.VisibleLaser | DeviceMode.IRLaser);
        if (wasUsingLaser != UsingLaser)
        {
            OnLaserToggle?.Invoke(UsingLaser);
        }
    }

    private void CheckUsingLightModes()
    {
        ActiveModes = DeviceMode.None;
        Player player = Player;
        if (player == null)
        {
            return;
        }

        if (_tacticalModesField == null)
        {
#if DEBUG
            Logger.LogError("Could find not find _tacticalModesField");
#endif
            return;
        }

        // Get the firearmsController for the player, this will be their IsCurrentEnemy weapon
        Player.FirearmController firearmController = player.HandsController as Player.FirearmController;
        if (firearmController == null)
        {
#if DEBUG
            Logger.LogError("Could find not find firearmController");
#endif
            return;
        }

        // Get the list of tacticalComboVisualControllers for the current weapon (One should exist for every flashlight, laser, or combo device)
        Transform weaponRoot = firearmController.WeaponRoot;
        TacticalDevices = weaponRoot.GetComponentsInChildrenActiveIgnoreFirstLevel<TacticalComboVisualController>();
        if (TacticalDevices == null)
        {
#if DEBUG
            Logger.LogError("Could find not find tacticalComboVisualControllers");
#endif
            return;
        }

        // Loop through all of the tacticalComboVisualControllers, then its modes, then that modes children, and look for a light
        foreach (TacticalComboVisualController tacticalComboVisualController in TacticalDevices)
        {
            List<Transform> tacticalModes = _tacticalModesField(tacticalComboVisualController);
            foreach (var mode in tacticalModes)
            {
                // Skip disabled modes
                if (!mode.gameObject.activeInHierarchy)
                {
                    continue;
                }

                for (int i = 0; i < mode.childCount; i++)
                {
                    Transform child = mode.GetChild(i);
                    string name = child.name;
                    if (!WhiteLight && name.StartsWith("light_0", StringComparison.OrdinalIgnoreCase))
                    {
#if DEBUG
                        if (_debugMode)
                        {
                            Logger.LogDebug($"[{player.name}] Found WhiteLight : Name:{name}");
                        }
#endif
                        ActiveModes |= DeviceMode.WhiteLight;
                    }
                    if (!Laser && name.StartsWith("vis_0", StringComparison.OrdinalIgnoreCase))
                    {
#if DEBUG
                        if (_debugMode)
                        {
                            Logger.LogDebug($"[{player.name}] Found VisibleLaser : Name:{name}");
                        }
#endif
                        ActiveModes |= DeviceMode.VisibleLaser;
                    }
                    if (!IRLight && name.StartsWith("il_0", StringComparison.OrdinalIgnoreCase))
                    {
#if DEBUG
                        if (_debugMode)
                        {
                            Logger.LogDebug($"[{player.name}] Found IRLight : Name:{name}");
                        }
#endif
                        ActiveModes |= DeviceMode.IRLight;
                    }
                    if (!IRLaser && name.StartsWith("ir_0", StringComparison.OrdinalIgnoreCase))
                    {
#if DEBUG
                        if (_debugMode)
                        {
                            Logger.LogDebug($"[{player.name}] Found IRLaser : Name:{name}");
                        }
#endif
                        ActiveModes |= DeviceMode.IRLaser;
                    }
                }
            }
        }
    }

    private static bool _debugMode
    {
        get { return SAINPlugin.LoadedPreset.GlobalSettings.General.Flashlight.DebugFlash; }
    }

    public DeviceMode ActiveModes { get; set; }

    private static readonly AccessTools.FieldRef<TacticalComboVisualController, List<Transform>> _tacticalModesField =
        AccessTools.FieldRefAccess<TacticalComboVisualController, List<Transform>>("_ligthbeamsTransforms");
}
