using System;
using System.Collections.Generic;
using System.Linq;
using EFT;
using SAIN.Preset.Shared.Enums;

namespace SAIN.Helpers;

internal static class EnumValues
{
    internal static class WildSpawn
    {
        static WildSpawn()
        {
            Bosses = CheckAdd("boss");
            Followers = CheckAdd("follower");
        }

        private static List<WildSpawnType> CheckAdd(string search)
        {
            var list = new List<WildSpawnType>();
            foreach (WildSpawnType type in GetEnum<WildSpawnType>())
            {
                if (type.ToString().ToLower().StartsWith(search))
                {
                    list.Add(type);
                }
            }
            return list;
        }

        public static bool IsScav(WildSpawnType type)
        {
            return Scavs.Contains(type);
        }

        public static bool IsGoons(WildSpawnType type)
        {
            return Goons.Contains(type);
        }

        public static bool IsNormalBoss(WildSpawnType type)
        {
            return NormalBosses.Contains(type);
        }

        public static bool IsNormalFollower(WildSpawnType type)
        {
            return NormalFollowers.Contains(type);
        }

        public static bool IsLabyrinthBot(WildSpawnType type)
        {
            return LabyrinthBots.Contains(type);
        }

        public static bool IsSpecialBot(WildSpawnType type)
        {
            return SpecialBots.Contains(type);
        }

        public static WildSpawnType[] Scavs =
        [
            WildSpawnType.assault,
            WildSpawnType.assaultGroup,
            WildSpawnType.crazyAssaultEvent,
            WildSpawnType.cursedAssault,
            WildSpawnType.marksman,
        ];

        public static WildSpawnType[] Goons = [WildSpawnType.bossKnight, WildSpawnType.followerBigPipe, WildSpawnType.followerBirdEye];

        public static WildSpawnType[] LabyrinthBots =
        [
            WildSpawnType.bossTagillaAgro,
            WildSpawnType.bossKillaAgro,
            WildSpawnType.tagillaHelperAgro,
        ];

        public static WildSpawnType[] SpecialBots = [WildSpawnType.crazyAssaultEvent, WildSpawnType.gifter];

        public static List<WildSpawnType> Bosses;
        public static List<WildSpawnType> Followers;

        public static WildSpawnType[] NormalBosses =
        [
            WildSpawnType.bossBully,
            WildSpawnType.bossGluhar,
            WildSpawnType.bossKojaniy,
            WildSpawnType.bossSanitar,
            WildSpawnType.bossTagilla,
            WildSpawnType.bossTest,
            WildSpawnType.bossKilla,
            WildSpawnType.bossBoar,
            WildSpawnType.bossKolontay,
            WildSpawnType.bossPartisan,
        ];

        public static WildSpawnType[] NormalFollowers =
        [
            WildSpawnType.followerBully,
            WildSpawnType.followerGluharAssault,
            WildSpawnType.followerGluharSecurity,
            WildSpawnType.followerGluharScout,
            WildSpawnType.followerKojaniy,
            WildSpawnType.followerSanitar,
            WildSpawnType.followerTagilla,
            WildSpawnType.followerBoar,
            WildSpawnType.followerBoarClose1,
            WildSpawnType.followerBoarClose2,
            WildSpawnType.followerKolontayAssault,
            WildSpawnType.followerKolontaySecurity,
        ];
    }

    public static T Parse<T>(string value)
    {
        return (T)Enum.Parse(typeof(T), value);
    }

    public static readonly BotDifficulty[] Difficulties =
    [
        BotDifficulty.easy,
        BotDifficulty.normal,
        BotDifficulty.hard,
        BotDifficulty.impossible,
    ];

    public static EWeaponClass ParseWeaponClass(string weaponClass)
    {
        if (Enum.TryParse(weaponClass, out EWeaponClass result))
        {
            return result;
        }
        Logger.LogError($"Weapon Class [{weaponClass}] does not exist in IWeaponClass Enum!");
        return EWeaponClass.Default;
    }

    public static T TryParse<T>(string _string)
        where T : struct, Enum
    {
        if (Enum.TryParse(_string, out T result))
        {
            return result;
        }
        Logger.LogError($"[{_string}] does not exist in [{typeof(T)}] Enum!");
        return default;
    }

    public static T[] GetEnum<T>()
    {
        return EnumCache<T>.Values;
    }

    private static class EnumCache<T>
    {
        public static readonly T[] Values = (T[])Enum.GetValues(typeof(T));
    }
}
