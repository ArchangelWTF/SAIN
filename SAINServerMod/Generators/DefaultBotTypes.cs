using SAIN.Preset.Shared.BotSettings;
using SAIN.Preset.Shared.Enums;

namespace SAINServerMod.Generators;

public static class DefaultBotTypes
{
    public static List<BotTypeInfo> All()
    {
        return
        [
            new("Scav", ESainWildSpawnType.assault, 0.3f, "Scavs", "Scavs!"),
            new("Scav Group", ESainWildSpawnType.assaultGroup, 0.35f, "Scavs", "Scavs in a Group!"),
            new("Crazy Scav Event", ESainWildSpawnType.crazyAssaultEvent, 0.35f, "Scavs", "Scavs!"),
            new("Usec", ESainWildSpawnType.pmcUSEC, 1f, "PMCs", "A PMC of the Usec Faction"),
            new("Bear", ESainWildSpawnType.pmcBEAR, 1f, "PMCs", "A PMC of the Bear Faction"),
            new("Scav Sniper", ESainWildSpawnType.marksman, 0.3f, "Scavs", "The Scav Snipers that spawn on rooftops on certain maps"),
            new(
                "Tagged and Cursed Scav",
                ESainWildSpawnType.cursedAssault,
                0.35f,
                "Scavs",
                "The type a scav is assigned when the player is marked as Tagged and Cursed"
            ),
            new(
                "Knight",
                ESainWildSpawnType.bossKnight,
                1f,
                "Goons",
                "Goons leader. Close proximity to the goons has been noted to cause smashed keyboards"
            ),
            new(
                "BigPipe",
                ESainWildSpawnType.followerBigPipe,
                1f,
                "Goons",
                "Goons follower. Close proximity to the goons has been noted to cause smashed keyboards\""
            ),
            new(
                "BirdEye",
                ESainWildSpawnType.followerBirdEye,
                1f,
                "Goons",
                "Goons follower. Close proximity to the goons has been noted to cause smashed keyboards\""
            ),
            new(
                "Rogue",
                ESainWildSpawnType.exUsec,
                0.66f,
                "Other",
                "Ex Usec Personel on Lighthouse usually found around the water treatment plant"
            ),
            new("Raider", ESainWildSpawnType.pmcBot, 0.66f, "Other", "Heavily armed scavs typically found on reserve and Labs by default"),
            new(
                "Bloodhound",
                ESainWildSpawnType.arenaFighterEvent,
                0.66f,
                "Other",
                "From the Live Event, nearly identical to raiders except with different voicelines and better gear. Found in"
            ),
            new("Cultist Priest", ESainWildSpawnType.sectantPriest, 0.75f, "Other", "Found on Customs, Woods, Factory, Shoreline at night"),
            new("Cultist", ESainWildSpawnType.sectantWarrior, 0.7f, "Other", "Found on Customs, Woods, Factory, Shoreline at night"),
            new("Killa", ESainWildSpawnType.bossKilla, 0.75f, "Bosses", "He shoot. Found on Interchange and Streets"),
            new("Rashala", ESainWildSpawnType.bossBully, 0.75f, "Bosses", "Customs Boss"),
            new("Rashala Guard", ESainWildSpawnType.followerBully, 0.55f, "Followers", "Customs Boss Follower"),
            new("Shturman", ESainWildSpawnType.bossKojaniy, 0.75f, "Bosses", "Woods Boss"),
            new("Shturman Guard", ESainWildSpawnType.followerKojaniy, 0.55f, "Followers", "Woods Boss Follower"),
            new("Tagilla", ESainWildSpawnType.bossTagilla, 0.75f, "Bosses", "He Smash"),
            new("Tagilla Guard", ESainWildSpawnType.followerTagilla, 0.55f, "Followers", "They Smash Too?"),
            new("Shadow of Tagilla", ESainWildSpawnType.bossTagillaAgro, 0.5f, "Bosses", "He Smash"),
            new("Vengeful Killa", ESainWildSpawnType.bossKillaAgro, 0.75f, "Bosses", "He no Smash"),
            new("Shadow of Tagilla Guard", ESainWildSpawnType.tagillaHelperAgro, 0.55f, "Followers", "Labyrinth Boss Follower"),
            new("Sanitar", ESainWildSpawnType.bossSanitar, 0.75f, "Bosses", "Shoreline Boss"),
            new("Sanitar Guard", ESainWildSpawnType.followerSanitar, 0.55f, "Followers", "Shoreline Boss Follower"),
            new("Gluhar", ESainWildSpawnType.bossGluhar, 0.75f, "Bosses", "Reserve Boss. Also can be found on Streets."),
            new("Gluhar Guard Snipe", ESainWildSpawnType.followerGluharSnipe, 0.55f, "Followers", "Reserve Boss Follower"),
            new("Gluhar Guard Scout", ESainWildSpawnType.followerGluharScout, 0.55f, "Followers", "Reserve Boss Follower"),
            new("Gluhar Guard Security", ESainWildSpawnType.followerGluharSecurity, 0.55f, "Followers", "Reserve Boss Follower"),
            new("Gluhar Guard Assault", ESainWildSpawnType.followerGluharAssault, 0.55f, "Followers", "Reserve Boss Follower"),
            new("Zryachiy", ESainWildSpawnType.bossZryachiy, 0.75f, "Bosses", "Lighthouse Island Sniper Boss"),
            new("Zryachiy Guard", ESainWildSpawnType.followerZryachiy, 0.55f, "Followers", "Lighthouse Island Sniper Boss Follower"),
            new("Kaban", ESainWildSpawnType.bossBoar, 0.75f, "Bosses", "Streets Gangster"),
            new("Kaban Guard", ESainWildSpawnType.followerBoar, 0.55f, "Followers", "Gangster Cannon Fodder"),
            new("Basmach", ESainWildSpawnType.followerBoarClose1, 0.55f, "Followers", "Gangster 1"),
            new("Gus", ESainWildSpawnType.followerBoarClose2, 0.55f, "Followers", "Gangster 2"),
            new("Kaban Sniper", ESainWildSpawnType.bossBoarSniper, 0.55f, "Followers", "Gangster Sniper"),
            new("Kollontay", ESainWildSpawnType.bossKolontay, 0.75f, "Bosses", "Crooked Cop"),
            new("Kollantay Assault", ESainWildSpawnType.followerKolontayAssault, 0.55f, "Followers", "Aggressive Guard"),
            new("Kollantay Security", ESainWildSpawnType.followerKolontaySecurity, 0.55f, "Followers", "Defensive Guard"),
            new("Partisan", ESainWildSpawnType.bossPartisan, 0.75f, "Bosses", "A scav legend.. who scavs hate"),
            new("BTR", ESainWildSpawnType.shooterBTR, 0.5f, "Other", "Zoom. Zoom. Bang. Bang."),
            new("Santa Claus", ESainWildSpawnType.gifter, 1f, "Bosses", "Event boss, he will jingle your bells if you try to kill him"),
        ];
    }

    public static List<ESainWildSpawnType> StrictExclusionList { get; } =
    [
        ESainWildSpawnType.bossZryachiy,
        ESainWildSpawnType.followerZryachiy,
        ESainWildSpawnType.peacefullZryachiyEvent,
        ESainWildSpawnType.ravangeZryachiyEvent,
        ESainWildSpawnType.shooterBTR,
        ESainWildSpawnType.marksman,
        ESainWildSpawnType.infectedAssault,
        ESainWildSpawnType.infectedCivil,
        ESainWildSpawnType.infectedLaborant,
        ESainWildSpawnType.infectedPmc,
        ESainWildSpawnType.infectedTagilla,
    ];
}
