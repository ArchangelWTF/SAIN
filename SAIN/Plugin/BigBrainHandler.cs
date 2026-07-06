using System;
using System.Collections.Generic;
using System.Reflection;
using DrakiaXYZ.BigBrain.Brains;
using EFT;
using SAIN.Layers;
using SAIN.Layers.Combat.Run;
using SAIN.Layers.Combat.Solo;
using SAIN.Layers.Combat.Squad;
using SAIN.Preset.GlobalSettings;
using SAIN.Preset.GlobalSettings.Categories;

namespace SAIN;

public static class BigBrainHandler
{
    public const bool INCLUDE_RAIDER_BRAIN_FOR_PMCS = true;

    private static readonly string[] _commonVanillaLayersToRemove =
    [
        //"FightReqNull",
        //"PeacecReqNull",
        //"Follow Player",
        //"HideHW", // Used by Scavs, Raiders, and Bloodhounds when zombies spawn
        //"Khorovod",
        //"GeneratorDef", // Used by Scavs and Killa
        "GroupForce", // Used by Cursed Scavs, Rogues, and Oni (event) Cultist
        "AdvAssaultTarget",
        "Simple Target",
        "Pmc", // Used by Raiders, Bloodhounds, and Scav groups
        "AssaultHaveEnemy",
        "PushAndSup", // Used by Scavs and Shadow of Tagilla's followers
        "Pursuit",
        "Kill logic",
    ];

    private static readonly string[] _commonVanillaBossAndFollowerLayersToRemove =
    [
        //"ExURequest", // Used by Rogues and Kaban's followers to warn neutral players/bots
        "BirdEyeFight", // Used by Birdeye and Ghost (event) Cultist
        "BossSanitarFight", // Used by Sanitar and Harbinger (event) Cultist
        "SanitarGoal", // Used by Sanitar and his followers
        "TagillaAmbush", // Used by Tagilla and his followers
        "TagillaMain", // Used by Tagilla and his followers
        "BoarGrenadeDanger", // Used by Kaban and his followers
        "HoldOrCoverF", // Used by Cultists and Gluhar's followers
        "SecurityGluhar", // Used by Gluhar and his followers
        "HoldNearBoss", // Used by Gluhar's and Kollontay's followers
        "Kln_NIMH", // Used by Kollontay and his followers
        "KlnTrg", // Used by Kollontay and his followers
        "Kojaniy Target", // Used by Shturman and his followers
    ];

    private static readonly List<Type> _SAINLayers = [];
    private static readonly List<string> _SAINLayerNames = [];

    public static List<string> SAINLayerNames
    {
        get { return FindAllSAINLayers(); }
    }

    public static List<Type> SAINLayers
    {
        get
        {
            if (_SAINLayers.Count == 0)
            {
                Type[] allTypes = typeof(SAINPlugin).Assembly.GetTypes();
                for (int i = 0; i < allTypes.Length; i++)
                {
                    Type type = allTypes[i];
                    if (type.IsSubclassOf(typeof(SAINLayer)))
                    {
                        _SAINLayers.Add(type);
                    }
                }
            }

            return _SAINLayers;
        }
    }

    public static void Init()
    {
        BrainAssignment.Init();
    }

    private static List<string> FindAllSAINLayers()
    {
        if (_SAINLayerNames.Count != 0)
        {
            return _SAINLayerNames;
        }

        foreach (Type layerType in SAINLayers)
        {
            FieldInfo nameFieldInfo = layerType.GetField("Name", BindingFlags.Public | BindingFlags.Static);
            if (nameFieldInfo == null)
            {
                Logger.LogError(
                    $"{layerType.Name} does not have a public static Name field. This is required for enabling vanilla layers!"
                );
                continue;
            }

            _SAINLayerNames.Add((string)nameFieldInfo.GetValue(null));
        }

        return _SAINLayerNames;
    }

    public static bool BigBrainInitialized;

    public static class BrainAssignment
    {
        public static void Init()
        {
            AddCustomLayersToPMCs();
            AddCustomLayersToScavs();
            AddCustomLayersToRaiders([WildSpawnType.pmcBot]);
            AddCustomLayersToRogues();
            AddCustomLayersToBloodHounds();
            AddCustomLayersToNormalBosses();
            AddCustomLayersToNormalFollowers();
            AddCustomLayersToGoons();
            AddCustomLayersToLabyrinthBots();
            AddCustomLayersToCultists();
            AddCustomLayersToSpecialBots();

            ToggleVanillaLayersForPMCs(false);
            ToggleVanillaLayersForAllBots();
        }

        public static void ToggleVanillaLayersForAllBots()
        {
            ToggleVanillaLayersForScavs(VanillaBotSettings.VanillaScavs);
            ToggleVanillaLayersForRogues(VanillaBotSettings.VanillaRogues);
            ToggleVanillaLayersForRaiders([WildSpawnType.pmcBot], VanillaBotSettings.VanillaRaiders);
            ToggleVanillaLayersForBloodHounds(VanillaBotSettings.VanillaBloodHounds);
            ToggleVanillaLayersForNormalBosses(VanillaBotSettings.VanillaBosses);
            ToggleVanillaLayersForNormalFollowers(VanillaBotSettings.VanillaFollowers);
            ToggleVanillaLayersForGoons(VanillaBotSettings.VanillaGoons);
            ToggleVanillaLayersForLaybrinthBots(VanillaBotSettings.VanillaLabyrinthBots);
            ToggleVanillaLayersForCultists(VanillaBotSettings.VanillaCultists);
            ToggleVanillaLayersForSpecialBots(VanillaBotSettings.VanillaSpecialBots);
        }

        public static void ToggleVanillaLayersForScavs(bool useVanillaLayers)
        {
            List<string> brainList = GetBrainList(AIBrains.Scavs);

            List<string> LayersToToggle =
            [
                "Help",
                "AssaultEnemyFar",
                .. _commonVanillaLayersToRemove,
            ];

            ToggleVanillaLayers(brainList, LayersToToggle, useVanillaLayers);

            ToggleVanillaLayersForRaiders([WildSpawnType.assaultGroup], useVanillaLayers);
        }

        public static void ToggleVanillaLayersForRaiders(List<WildSpawnType> roles, bool useVanillaLayers)
        {
            List<string> brainList = [nameof(EBrain.PMC)];

            List<string> LayersToToggle =
            [
                "AssaultEnemyFar", // Used by Raiders and Scav groups
                .. _commonVanillaLayersToRemove,
            ];

            ToggleVanillaLayers(brainList, LayersToToggle, roles, useVanillaLayers);
        }

        public static void ToggleVanillaLayersForSpecialBots(bool useVanillaLayers)
        {
            List<string> brainList = GetBrainList(AIBrains.SpecialBots);

            List<string> LayersToToggle =
            [
                "ObdolbosFight", // Used by crazy event Scavs
                .. _commonVanillaLayersToRemove,
            ];

            ToggleVanillaLayers(brainList, LayersToToggle, useVanillaLayers);
        }

        public static void ToggleVanillaLayersForRogues(bool useVanillaLayers)
        {
            List<string> brainList = [nameof(EBrain.ExUsec)];

            List<string> LayersToToggle =
            [
                .. _commonVanillaLayersToRemove,
            ];

            ToggleVanillaLayers(brainList, LayersToToggle, useVanillaLayers);
        }

        public static void ToggleVanillaLayersForBloodHounds(bool useVanillaLayers)
        {
            List<string> brainList = [nameof(EBrain.ArenaFighter)];

            List<string> LayersToToggle =
            [
                .. _commonVanillaLayersToRemove,
            ];

            ToggleVanillaLayers(brainList, LayersToToggle, useVanillaLayers);

            ToggleVanillaLayersForRaiders([WildSpawnType.arenaFighterEvent], useVanillaLayers);
        }

        public static void ToggleVanillaLayersForLaybrinthBots(bool useVanillaLayers)
        {
            List<string> brainList = GetBrainList(AIBrains.LabyrinthBots);

            List<string> LayersToToggle =
            [
                "KillaAgro", // Used by Vengeful Killa
                "TagillaAgro", // Used by Shadow of Tagilla
                .. _commonVanillaLayersToRemove,
            ];

            ToggleVanillaLayers(brainList, LayersToToggle, useVanillaLayers);
        }

        public static void ToggleVanillaLayersForCultists(bool useVanillaLayers)
        {
            List<string> brainList = GetBrainList(AIBrains.Cultists);

            List<string> LayersToToggle =
            [
                //"GrenSuicide", // Used by Cultist priests
                "Run&Strike", // Used by Cultist followers
                "MeleeS_IN", // Used by Cultist followers
                "R&H_OUT", // Used by Cultist priests
                "SupShootSect_IN", // Used by Cultist followers
                .. _commonVanillaLayersToRemove,
                .. _commonVanillaBossAndFollowerLayersToRemove,
            ];

            ToggleVanillaLayers(brainList, LayersToToggle, useVanillaLayers);
        }

        public static void ToggleVanillaLayersForNormalBosses(bool useVanillaLayers)
        {
            List<string> brainList = GetBrainList(AIBrains.NormalBosses);

            List<string> LayersToToggle =
            [
                //"PartisanMine", // Used by Partisan
                //"PartMineAll", // Used by Partisan
                "PrtFMN", // Used by Partisan
                "PrtPst", // Used by Partisan
                "PrtZrSvg", // Used by Partisan
                "PrtFight", // Used by Partisan
                "PrtBadTrg", // Used by Partisan
                "PrtMany", // Used by Partisan
                "PrtStalk", // Used by Partisan
                "HoldOrCoverT", // Used by Partisan
                "BossBoarFight", // Used by Kaban
                "BossGlFight", // Used by Gluhar
                "KojaniyB_Enemy", // Used by Shturman
                "Bully Layer", // Used by Reshala
                "KlnSolo", // Used by Kollontay
                "KolontayFight", // Used by Kollontay
                .. _commonVanillaLayersToRemove,
                .. _commonVanillaBossAndFollowerLayersToRemove,
            ];
            ToggleVanillaLayers(brainList, LayersToToggle, useVanillaLayers);
        }

        public static void ToggleVanillaLayersForNormalFollowers(bool useVanillaLayers)
        {
            List<string> brainList = GetBrainList(AIBrains.NormalFollowers);

            List<string> LayersToToggle =
            [
                "BoarStationary", // Used by Kaban's followers
                "BoarPatrol", // Used by Kaban's followers
                "BoarClPatrol", // Used by Kaban's followers
                "FBoarFght", // Used by Kaban's followers
                "SecurityKln", // Used by Kollontay's followers
                "KlnForceAtk", // Used by Kollontay's followers
                "KolontayAP", // Used by Kollontay's followers
                "FolKojEnemy", // Used by Shturman's followers
                "GluharKilla", // Used by Gluhar's followers
                "GluhAssKilla", // Used by Gluhar's followers
                "FlGlScout", // Used by Gluhar's followers
                "GlGoal", // Used by Gluhar's followers
                "FlSanFight", // Used by Sanitar's followers
                "TagillaFollower", // Used by Tagilla's followers
                "Follower bully", // Used by Reshala's followers
                .. _commonVanillaLayersToRemove,
                .. _commonVanillaBossAndFollowerLayersToRemove,
            ];

            ToggleVanillaLayers(brainList, LayersToToggle, useVanillaLayers);
        }

        public static void ToggleVanillaLayersForGoons(bool useVanillaLayers)
        {
            List<string> brainList = GetBrainList(AIBrains.Goons);

            List<string> LayersToToggle =
            [
                "KnightFight", // Used by Knight
                "Assault Building", // Used by Knight, Big Pipe, and Birdeye
                "Enemy Building", // Used by Knight, Big Pipe, and Birdeye
                "BirdHold", // Used by Birdeye
                .. _commonVanillaLayersToRemove,
                .. _commonVanillaBossAndFollowerLayersToRemove,
            ];

            ToggleVanillaLayers(brainList, LayersToToggle, useVanillaLayers);
        }

        public static void ToggleVanillaLayersForPMCs(bool useVanillaLayers)
        {
            List<string> brainList = GetBrainList(AIBrains.PMCs);

            List<string> LayersToToggle =
            [
                "PmcBear",
                "PmcUsec",
                .. _commonVanillaLayersToRemove,
            ];

            ToggleVanillaLayers(brainList, LayersToToggle, useVanillaLayers);

            if (INCLUDE_RAIDER_BRAIN_FOR_PMCS)
            {
                ToggleVanillaLayersForRaiders([WildSpawnType.pmcBEAR, WildSpawnType.pmcUSEC], useVanillaLayers);
            }
        }

        public static void ToggleVanillaLayersForBrains(List<string> brainList, List<string> layersToToggle, bool useVanillaLayers)
        {
            ToggleVanillaLayers(brainList, layersToToggle, useVanillaLayers);
        }

        public static void ToggleVanillaLayersForBrainsAndRoles(List<string> brainList, List<WildSpawnType> roles, List<string> layersToToggle, bool useVanillaLayers)
        {
            ToggleVanillaLayers(brainList, layersToToggle, roles, useVanillaLayers);
        }

        public static void AddCustomLayersToBrains(List<string> brainList, bool withExtract)
        {
            var settings = SAINPlugin.LoadedPreset.GlobalSettings.General.Layers;

            BrainManager.AddCustomLayer(typeof(DebugLayer), brainList, 99);
            BrainManager.AddCustomLayer(typeof(SAINAvoidThreatLayer), brainList, 80);
            BrainManager.AddCustomLayer(typeof(CombatSquadLayer), brainList, settings.SAINCombatSquadLayerPriority);
            BrainManager.AddCustomLayer(typeof(CombatSoloLayer), brainList, settings.SAINCombatSoloLayerPriority);

            if (withExtract)
            {
                BrainManager.AddCustomLayer(typeof(ExtractLayer), brainList, settings.SAINExtractLayerPriority);
            }
        }

        public static void AddCustomLayersToBrainsAndRoles(List<string> brainList, List<WildSpawnType> roles, bool withExtract)
        {
            var settings = SAINPlugin.LoadedPreset.GlobalSettings.General.Layers;

            BrainManager.AddCustomLayer(typeof(DebugLayer), brainList, 99, roles);
            BrainManager.AddCustomLayer(typeof(SAINAvoidThreatLayer), brainList, 80, roles);
            BrainManager.AddCustomLayer(typeof(CombatSquadLayer), brainList, settings.SAINCombatSquadLayerPriority, roles);
            BrainManager.AddCustomLayer(typeof(CombatSoloLayer), brainList, settings.SAINCombatSoloLayerPriority, roles);

            if (withExtract)
            {
                BrainManager.AddCustomLayer(typeof(ExtractLayer), brainList, settings.SAINExtractLayerPriority, roles);
            }
        }

        private static void ToggleVanillaLayers(List<string> brainNames, List<string> layerNames, bool useVanillaLayers)
        {
            if (useVanillaLayers)
            {
                BrainManager.RemoveLayers(SAINLayerNames, brainNames);
                BrainManager.RestoreLayers(layerNames, brainNames);
            }
            else
            {
                CheckExtractEnabled(layerNames);

                BrainManager.RestoreLayers(SAINLayerNames, brainNames);
                BrainManager.RemoveLayers(layerNames, brainNames);
            }
        }

        private static void ToggleVanillaLayers(
            List<string> brainNames,
            List<string> layerNames,
            List<WildSpawnType> roles,
            bool useVanillaLayers
        )
        {
            if (useVanillaLayers)
            {
                BrainManager.RemoveLayers(SAINLayerNames, brainNames, roles);
                BrainManager.RestoreLayers(layerNames, brainNames, roles);
            }
            else
            {
                CheckExtractEnabled(layerNames);

                BrainManager.RestoreLayers(SAINLayerNames, brainNames, roles);
                BrainManager.RemoveLayers(layerNames, brainNames, roles);
            }
        }

        private static void AddCustomLayersToPMCs()
        {
            List<string> pmcBrain = GetBrainList(AIBrains.PMCs);
            var settings = SAINPlugin.LoadedPreset.GlobalSettings.General.Layers;

            BrainManager.AddCustomLayer(typeof(DebugLayer), pmcBrain, 99);
            BrainManager.AddCustomLayer(typeof(SAINAvoidThreatLayer), pmcBrain, 80);
            BrainManager.AddCustomLayer(typeof(ExtractLayer), pmcBrain, settings.SAINExtractLayerPriority);
            BrainManager.AddCustomLayer(typeof(CombatSquadLayer), pmcBrain, settings.SAINCombatSquadLayerPriority);
            BrainManager.AddCustomLayer(typeof(CombatSoloLayer), pmcBrain, settings.SAINCombatSoloLayerPriority);

            if (INCLUDE_RAIDER_BRAIN_FOR_PMCS)
            {
                AddCustomLayersToRaiders([WildSpawnType.pmcBEAR, WildSpawnType.pmcUSEC]);
            }
        }

        private static void AddCustomLayersToScavs()
        {
            List<string> brainList = GetBrainList(AIBrains.Scavs);
            var settings = SAINPlugin.LoadedPreset.GlobalSettings.General.Layers;

            //BrainManager.AddCustomLayer(typeof(BotUnstuckLayer), stringList, 98);
            BrainManager.AddCustomLayer(typeof(DebugLayer), brainList, 99);
            BrainManager.AddCustomLayer(typeof(SAINAvoidThreatLayer), brainList, 80);
            BrainManager.AddCustomLayer(typeof(ExtractLayer), brainList, settings.SAINExtractLayerPriority);
            BrainManager.AddCustomLayer(typeof(CombatSquadLayer), brainList, settings.SAINCombatSquadLayerPriority);
            BrainManager.AddCustomLayer(typeof(CombatSoloLayer), brainList, settings.SAINCombatSoloLayerPriority);

            AddCustomLayersToRaiders([WildSpawnType.assaultGroup]);
        }

        private static void AddCustomLayersToRaiders(List<WildSpawnType> roles)
        {
            var settings = SAINPlugin.LoadedPreset.GlobalSettings.General.Layers;
            List<string> raiderBrain = [nameof(EBrain.PMC)];

            BrainManager.AddCustomLayer(typeof(DebugLayer), raiderBrain, 99, roles);
            BrainManager.AddCustomLayer(typeof(SAINAvoidThreatLayer), raiderBrain, 80, roles);
            BrainManager.AddCustomLayer(typeof(ExtractLayer), raiderBrain, settings.SAINExtractLayerPriority, roles);
            BrainManager.AddCustomLayer(typeof(CombatSquadLayer), raiderBrain, settings.SAINCombatSquadLayerPriority, roles);
            BrainManager.AddCustomLayer(typeof(CombatSoloLayer), raiderBrain, settings.SAINCombatSoloLayerPriority, roles);
        }

        private static void AddCustomLayersToSpecialBots()
        {
            List<string> brainList = GetBrainList(AIBrains.SpecialBots);

            var settings = SAINPlugin.LoadedPreset.GlobalSettings.General.Layers;
            //BrainManager.AddCustomLayer(typeof(BotUnstuckLayer), stringList, 98);
            BrainManager.AddCustomLayer(typeof(DebugLayer), brainList, 99);
            BrainManager.AddCustomLayer(typeof(SAINAvoidThreatLayer), brainList, 80);
            BrainManager.AddCustomLayer(typeof(ExtractLayer), brainList, settings.SAINExtractLayerPriority);
            BrainManager.AddCustomLayer(typeof(CombatSquadLayer), brainList, settings.SAINCombatSquadLayerPriority);
            BrainManager.AddCustomLayer(typeof(CombatSoloLayer), brainList, settings.SAINCombatSoloLayerPriority);
        }

        private static void AddCustomLayersToLabyrinthBots()
        {
            List<string> brainList = GetBrainList(AIBrains.LabyrinthBots);
            var settings = SAINPlugin.LoadedPreset.GlobalSettings.General.Layers;

            //BrainManager.AddCustomLayer(typeof(BotUnstuckLayer), stringList, 98);
            BrainManager.AddCustomLayer(typeof(DebugLayer), brainList, 99);
            BrainManager.AddCustomLayer(typeof(SAINAvoidThreatLayer), brainList, 80);
            BrainManager.AddCustomLayer(typeof(ExtractLayer), brainList, settings.SAINExtractLayerPriority);
            BrainManager.AddCustomLayer(typeof(CombatSquadLayer), brainList, settings.SAINCombatSquadLayerPriority);
            BrainManager.AddCustomLayer(typeof(CombatSoloLayer), brainList, settings.SAINCombatSoloLayerPriority);
        }

        private static void AddCustomLayersToCultists()
        {
            List<string> brainList = GetBrainList(AIBrains.Cultists);

            //BrainManager.AddCustomLayer(typeof(BotUnstuckLayer), stringList, 146);
            BrainManager.AddCustomLayer(typeof(DebugLayer), brainList, 150);
            BrainManager.AddCustomLayer(typeof(SAINAvoidThreatLayer), brainList, 118);
            BrainManager.AddCustomLayer(typeof(ExtractLayer), brainList, 108);
            BrainManager.AddCustomLayer(typeof(CombatSquadLayer), brainList, 104);
            BrainManager.AddCustomLayer(typeof(CombatSoloLayer), brainList, 102);
        }

        private static void AddCustomLayersToRogues()
        {
            List<string> brainList = [nameof(EBrain.ExUsec)];

            var settings = SAINPlugin.LoadedPreset.GlobalSettings.General.Layers;
            //BrainManager.AddCustomLayer(typeof(BotUnstuckLayer), stringList, 98);
            BrainManager.AddCustomLayer(typeof(DebugLayer), brainList, 99);
            BrainManager.AddCustomLayer(typeof(SAINAvoidThreatLayer), brainList, 80);
            BrainManager.AddCustomLayer(typeof(ExtractLayer), brainList, settings.SAINExtractLayerPriority);
            BrainManager.AddCustomLayer(typeof(CombatSquadLayer), brainList, settings.SAINCombatSquadLayerPriority);
            BrainManager.AddCustomLayer(typeof(CombatSoloLayer), brainList, settings.SAINCombatSoloLayerPriority);
        }

        private static void AddCustomLayersToBloodHounds()
        {
            List<string> brainList = [nameof(EBrain.ArenaFighter)];

            var settings = SAINPlugin.LoadedPreset.GlobalSettings.General.Layers;
            //BrainManager.AddCustomLayer(typeof(BotUnstuckLayer), stringList, 98);
            BrainManager.AddCustomLayer(typeof(DebugLayer), brainList, 99);
            BrainManager.AddCustomLayer(typeof(SAINAvoidThreatLayer), brainList, 80);
            BrainManager.AddCustomLayer(typeof(ExtractLayer), brainList, settings.SAINExtractLayerPriority);
            BrainManager.AddCustomLayer(typeof(CombatSquadLayer), brainList, settings.SAINCombatSquadLayerPriority);
            BrainManager.AddCustomLayer(typeof(CombatSoloLayer), brainList, settings.SAINCombatSoloLayerPriority);
        }

        private static void AddCustomLayersToNormalBosses()
        {
            List<string> brainList = GetBrainList(AIBrains.NormalBosses);

            //var settings = SAINPlugin.LoadedPreset.GlobalSettings.General;
            //BrainManager.AddCustomLayer(typeof(BotUnstuckLayer), stringList, 98);
            BrainManager.AddCustomLayer(typeof(DebugLayer), brainList, 99);
            BrainManager.AddCustomLayer(typeof(SAINAvoidThreatLayer), brainList, 80);
            BrainManager.AddCustomLayer(typeof(CombatSquadLayer), brainList, 70);
            BrainManager.AddCustomLayer(typeof(CombatSoloLayer), brainList, 69);
        }

        private static void AddCustomLayersToNormalFollowers()
        {
            List<string> brainList = GetBrainList(AIBrains.NormalFollowers);

            //var settings = SAINPlugin.LoadedPreset.GlobalSettings.General;
            //BrainManager.AddCustomLayer(typeof(BotUnstuckLayer), stringList, 98);
            BrainManager.AddCustomLayer(typeof(DebugLayer), brainList, 99);
            BrainManager.AddCustomLayer(typeof(SAINAvoidThreatLayer), brainList, 80);
            BrainManager.AddCustomLayer(typeof(CombatSquadLayer), brainList, 70);
            BrainManager.AddCustomLayer(typeof(CombatSoloLayer), brainList, 69);
        }

        private static void AddCustomLayersToGoons()
        {
            List<string> brainList = GetBrainList(AIBrains.Goons);

            BrainManager.AddCustomLayer(typeof(DebugLayer), brainList, 99);
            BrainManager.AddCustomLayer(typeof(SAINAvoidThreatLayer), brainList, 80);
            BrainManager.AddCustomLayer(typeof(CombatSquadLayer), brainList, 64);
            BrainManager.AddCustomLayer(typeof(CombatSoloLayer), brainList, 62);
        }

        private static void CheckExtractEnabled(List<string> layersToRemove)
        {
            if (GlobalSettingsClass.Instance.General.Extract.SAIN_EXTRACT_TOGGLE)
            {
                layersToRemove.Add("Exfiltration");
            }
        }

        private static List<string> GetBrainList(List<EBrain> brains)
        {
            List<string> brainList = [];
            for (int i = 0; i < brains.Count; i++)
            {
                brainList.Add(brains[i].ToString());
            }
            return brainList;
        }

        private static VanillaBotSettings VanillaBotSettings
        {
            get { return SAINPlugin.LoadedPreset.GlobalSettings.General.VanillaBots; }
        }
    }
}
