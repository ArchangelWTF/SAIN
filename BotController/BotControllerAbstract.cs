﻿using BepInEx.Logging;
using Comfort.Common;
using EFT;
using SAIN.Components;
using System.Collections.Generic;
using UnityEngine;
using SAIN.SAINComponent;
using SAIN.SAINComponent.Classes.Decision;
using SAIN.SAINComponent.Classes.Talk;
using SAIN.SAINComponent.Classes.WeaponFunction;
using SAIN.SAINComponent.Classes.Mover;
using SAIN.SAINComponent.Classes;
using SAIN.SAINComponent.SubComponents;

namespace SAIN
{
    public abstract class SAINControl
    {
        public void Awake()
        {
            BotController = GameWorld.GetOrAddComponent<SAINBotController>();
        }

        public SAINControl()
        {
        }

        public SAINBotController BotController { get; private set; }
        public Dictionary<string, BotComponent> Bots => BotController?.BotSpawnController?.SAINBotDictionary;
        public GameWorld GameWorld => Singleton<GameWorld>.Instance;
    }
}
