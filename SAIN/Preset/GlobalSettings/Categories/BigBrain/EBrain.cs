using System;
using System.Collections.Generic;
using System.Linq;

namespace SAIN.Preset.GlobalSettings.Categories;

public enum EBrain
{
    ArenaFighter,
    BossBully,
    BossGluhar,
    BossBoar,
    BossPartisan,
    Knight,
    BossKojaniy,
    BossSanitar,
    BossKolontay,
    Tagilla,
    TagillaAgro,
    BossTest,

    Obdolbs,
    ExUsec,
    BigPipe,
    BirdEye,
    FollowerBully,
    FollowerGluharAssault,
    FollowerGluharProtect,
    FollowerGluharScout,
    FollowerKojaniy,
    FollowerSanitar,
    FlBoar,
    FlBoarCl,
    FlBoarSt,
    FlKlnAslt,
    KolonSec,
    TagillaFollower,
    HelperAgro,

    Gifter,
    Killa,
    KillaAgro,
    Marksman,
    BoarSniper,
    PMC,
    SectantPriest,
    SctPredvst,
    PrizrakSt,
    Oni,
    SectantWarrior,
    CursAssault,
    Assault,
    PmcBear,
    PmcUsec,
    InfectedSlow,
}

public static class AIBrains
{
    private static IReadOnlyCollection<string> _allowedPlayerScavBrains;

    public static IReadOnlyCollection<string> AllowedPlayerScavBrains
    {
        get
        {
            if (_allowedPlayerScavBrains == null)
            {
                List<string> combinedBrains = [];

                foreach (var brain in AllowedPMCBrains)
                {
                    if (!combinedBrains.Contains(brain))
                    {
                        combinedBrains.Add(brain);
                    }
                }

                foreach (var brain in AllowedScavBrains)
                {
                    if (!combinedBrains.Contains(brain))
                    {
                        combinedBrains.Add(brain);
                    }
                }

                _allowedPlayerScavBrains = combinedBrains.AsReadOnly();
            }

            return _allowedPlayerScavBrains;
        }
    }

    public static IReadOnlyCollection<string> AllowedPMCBrains
    {
        get
        {
            if (_allowedPMCBrains == null)
            {
                List<EBrain> brains = [.. PMCs];
                if (BigBrainHandler.INCLUDE_RAIDER_BRAIN_FOR_PMCS)
                {
                    brains.Add(EBrain.PMC);
                }
                _allowedPMCBrains = brains.ConvertAll(brain => brain.ToString()).AsReadOnly();
            }
            return _allowedPMCBrains;
        }
    }

    private static IReadOnlyCollection<string> _allowedPMCBrains;

    public static IReadOnlyCollection<string> AllowedScavBrains
    {
        get
        {
            if (_allowedScavBrains == null)
            {
                // PMC brain is needed for assaultGroup scavs
                List<EBrain> brains = [EBrain.PMC, .. Scavs];
                _allowedScavBrains = brains.ConvertAll(brain => brain.ToString()).AsReadOnly();
            }
            return _allowedScavBrains;
        }
    }

    private static IReadOnlyCollection<string> _allowedScavBrains;

    public static readonly List<EBrain> PMCs = [EBrain.PmcBear, EBrain.PmcUsec];

    public static readonly List<EBrain> Scavs = [EBrain.CursAssault, EBrain.Assault];

    public static readonly List<EBrain> Goons = [EBrain.Knight, EBrain.BirdEye, EBrain.BigPipe];

    public static readonly List<EBrain> LabyrinthBots = [EBrain.TagillaAgro, EBrain.KillaAgro, EBrain.HelperAgro];

    public static readonly List<EBrain> SpecialBots = [EBrain.Obdolbs, EBrain.Gifter];

    public static readonly List<EBrain> Cultists =
    [
        EBrain.SectantWarrior,
        EBrain.SectantPriest,
        EBrain.SctPredvst,
        EBrain.PrizrakSt,
        EBrain.Oni,
    ];

    public static readonly List<EBrain> NormalBosses =
    [
        EBrain.BossBully,
        EBrain.BossGluhar,
        EBrain.BossKojaniy,
        EBrain.BossSanitar,
        EBrain.Tagilla,
        EBrain.BossTest,
        EBrain.Killa,
        EBrain.BossBoar,
        EBrain.BossKolontay,
        EBrain.BossPartisan,
    ];

    public static readonly List<EBrain> NormalFollowers =
    [
        EBrain.FollowerBully,
        EBrain.FollowerGluharAssault,
        EBrain.FollowerGluharProtect,
        EBrain.FollowerGluharScout,
        EBrain.FollowerKojaniy,
        EBrain.FollowerSanitar,
        EBrain.TagillaFollower,
        EBrain.FlBoar,
        EBrain.FlBoarCl,
        EBrain.FlBoarSt,
        EBrain.FlKlnAslt,
        EBrain.KolonSec,
    ];

    public static readonly EBrain[] AllBrains = Enum.GetValues(typeof(EBrain)).Cast<EBrain>().ToArray();
}
