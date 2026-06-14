using System;
using System.Collections.Generic;
using System.Reflection;
using EFT;
using SAIN.Attributes;
using SAIN.Components.BotController;
using SAIN.Helpers;
using SAIN.Preset.BotSettings.SAINSettings;
using static SAIN.Helpers.JsonUtility;

namespace SAIN.Preset.BotSettings;

public class SAINBotSettingsClass : BasePreset
{
    // Todo: convert to property in SPT 4.1, can't be done at the moment due to More Bots API
    public static Dictionary<WildSpawnType, float> DefaultDifficultyModifier = new()
    {
        { WildSpawnType.assault, 0.3f },
        { WildSpawnType.marksman, 0.3f },
        { WildSpawnType.crazyAssaultEvent, 0.35f },
        { WildSpawnType.cursedAssault, 0.35f },
        { WildSpawnType.assaultGroup, 0.35f },
        { WildSpawnType.bossBully, 0.75f },
        { WildSpawnType.bossBoar, 0.75f },
        { WildSpawnType.bossGluhar, 0.75f },
        { WildSpawnType.bossKilla, 0.75f },
        { WildSpawnType.bossKillaAgro, 0.75f },
        { WildSpawnType.bossSanitar, 0.75f },
        { WildSpawnType.bossKojaniy, 0.75f },
        { WildSpawnType.bossZryachiy, 0.75f },
        { WildSpawnType.bossTagilla, 0.75f },
        { WildSpawnType.bossKolontay, 0.75f },
        { WildSpawnType.sectantPriest, 0.75f },
        { WildSpawnType.bossPartisan, 0.75f },
        { WildSpawnType.bossKnight, 1f },
        { WildSpawnType.sectantWarrior, 0.7f },
        { WildSpawnType.followerBully, 0.55f },
        { WildSpawnType.followerGluharAssault, 0.55f },
        { WildSpawnType.followerGluharScout, 0.55f },
        { WildSpawnType.followerGluharSecurity, 0.55f },
        { WildSpawnType.followerGluharSnipe, 0.55f },
        { WildSpawnType.followerKojaniy, 0.55f },
        { WildSpawnType.followerSanitar, 0.55f },
        { WildSpawnType.followerTagilla, 0.55f },
        { WildSpawnType.tagillaHelperAgro, 0.55f },
        { WildSpawnType.followerZryachiy, 0.55f },
        { WildSpawnType.followerBoar, 0.55f },
        { WildSpawnType.followerBoarClose1, 0.55f },
        { WildSpawnType.followerBoarClose2, 0.55f },
        { WildSpawnType.bossBoarSniper, 0.55f },
        { WildSpawnType.followerKolontayAssault, 0.55f },
        { WildSpawnType.followerKolontaySecurity, 0.55f },
        { WildSpawnType.followerBigPipe, 1f },
        { WildSpawnType.followerBirdEye, 1f },
        { WildSpawnType.pmcBot, 0.66f },
        { WildSpawnType.exUsec, 0.66f },
        { WildSpawnType.arenaFighter, 0.66f },
        { WildSpawnType.arenaFighterEvent, 0.66f },
        { WildSpawnType.pmcUSEC, 1f },
        { WildSpawnType.pmcBEAR, 1f },
        { WildSpawnType.gifter, 1f },
    };

    // Todo: convert to property in SPT 4.1, can't be done at the moment due to More Bots API
    public Dictionary<WildSpawnType, SAINSettingsGroupClass> SAINSettings = [];

    // Todo: convert to property in SPT 4.1, can't be done at the moment due to More Bots API
    public Dictionary<WildSpawnType, EFTBotSettings> EFTSettings = [];

    public SAINBotSettingsClass(SAINPresetClass preset)
        : base(preset)
    {
        foreach (WildSpawnType type in BotTypeDefinitions.BotTypes.Keys)
        {
            if (!DefaultDifficultyModifier.ContainsKey(type))
            {
                DefaultDifficultyModifier.Add(type, 0.5f);
            }
        }

        LoadEFTSettings();
        LoadSAINSettings();
    }

    public void Update()
    {
        foreach (var settings in SAINSettings)
        {
            foreach (var group in settings.Value.Settings)
            {
                group.Value.Update();
            }
        }
    }

    public void Init()
    {
        foreach (var settings in SAINSettings)
        {
            foreach (var group in settings.Value.Settings)
            {
                group.Value.Init();
            }
        }
    }

    public void UpdateDefaults(SAINBotSettingsClass replacement)
    {
        foreach (var settings in SAINSettings)
        {
            var replacementSettings = replacement?.SAINSettings[settings.Key];
            foreach (var group in settings.Value.Settings)
            {
                var replacementGroup = replacementSettings?.Settings[group.Key];
                group.Value.UpdateDefaults(replacementGroup);
            }
        }
    }

    public void AddBotTypeToSettings(BotType botType, float defaultDifficultyModifier)
    {
        DefaultDifficultyModifier.Add(botType.WildSpawnType, defaultDifficultyModifier);

        string name = botType.Name;
        WildSpawnType wildSpawnType = botType.WildSpawnType;

        BotDifficulty[] Difficulties = EnumValues.Difficulties;
        if (
            Preset.Info.IsCustom == false
            || !SAINPresetClass.Import(out SAINSettingsGroupClass sainSettingsGroup, Preset.Info.Name, name, "BotSettings")
        )
        {
            sainSettingsGroup = new SAINSettingsGroupClass(Difficulties)
            {
                Name = name,
                WildSpawnType = wildSpawnType,
                DifficultyModifier = DefaultDifficultyModifier[wildSpawnType],
            };

            UpdateSAINSettingsToEFTDefault(wildSpawnType, sainSettingsGroup);

            if (Preset.Info.IsCustom == true)
            {
                SAINPresetClass.Export(sainSettingsGroup, Preset.Info.Name, name, "BotSettings");
            }
        }
        SAINSettings.Add(wildSpawnType, sainSettingsGroup);
    }

    private void LoadSAINSettings()
    {
        BotDifficulty[] Difficulties = EnumValues.Difficulties;
        foreach (var BotType in BotTypeDefinitions.BotTypesList)
        {
            string name = BotType.Name;
            WildSpawnType wildSpawnType = BotType.WildSpawnType;

            if (BotSpawnController.StrictExclusionList.Contains(wildSpawnType)) { }

            if (
                Preset.Info.IsCustom == false
                || !SAINPresetClass.Import(out SAINSettingsGroupClass sainSettingsGroup, Preset.Info.Name, name, "BotSettings")
            )
            {
                sainSettingsGroup = new SAINSettingsGroupClass(Difficulties)
                {
                    Name = name,
                    WildSpawnType = wildSpawnType,
                    DifficultyModifier = DefaultDifficultyModifier[wildSpawnType],
                };

                UpdateSAINSettingsToEFTDefault(wildSpawnType, sainSettingsGroup);

                if (Preset.Info.IsCustom == true)
                {
                    SAINPresetClass.Export(sainSettingsGroup, Preset.Info.Name, name, "BotSettings");
                }
            }
            SAINSettings.Add(wildSpawnType, sainSettingsGroup);
        }
    }

    private void UpdateSAINSettingsToEFTDefault(WildSpawnType wildSpawnType, SAINSettingsGroupClass sainSettingsGroup)
    {
        foreach (var keyPair in sainSettingsGroup.Settings)
        {
            SAINSettingsClass sainSettings = keyPair.Value;
            BotDifficulty Difficulty = keyPair.Key;

            // Get SAIN and EFT group for the given WildSpawnType and difficulties
            object eftSettings = GetEFTSettings(wildSpawnType, Difficulty);
            if (eftSettings != null)
            {
                CopyValuesAtoB(eftSettings, sainSettings, ShallUseEFTBotDefault);
            }
        }
    }

    private void CopyValuesAtoB(object A, object B, Func<FieldInfo, bool> shouldCopyFieldFunc = null)
    {
        // Get the names of the fields in EFT group
        FieldInfo[] aCatFieldArray = Reflection.GetFieldsInType(A.GetType());

        foreach (FieldInfo BCatField in Reflection.GetFieldsInType(B.GetType()))
        {
            // Check if the category inside SAIN GlobalSettings has a matching category in EFT group
            FieldInfo ACatField = null;
            for (int i = 0; i < aCatFieldArray.Length; i++)
            {
                if (aCatFieldArray[i].Name == BCatField.Name)
                {
                    ACatField = aCatFieldArray[i];
                    break;
                }
            }

            if (ACatField == null)
            {
                continue;
            }

            // Get the multiplier of the category from SAIN group
            object BCatObject = BCatField.GetValue(B);
            // Get the fields inside that category from SAIN group
            FieldInfo[] BVariableFieldArray = Reflection.GetFieldsInType(BCatField.FieldType);
            // Get the category of the matching sain category from EFT group
            if (ACatField != null)
            {
                // Get the value of the EFT group Category
                object ACatObject = ACatField.GetValue(A);
                // list the field names in that category
                FieldInfo[] aVarFieldArray = Reflection.GetFieldsInType(ACatObject.GetType());

                foreach (FieldInfo BVariableField in BVariableFieldArray)
                {
                    // Check if the sain variable is set to grab default EFT numbers and that it exists inside the EFT group category
                    FieldInfo AVariableField = null;
                    for (int i = 0; i < aVarFieldArray.Length; i++)
                    {
                        if (aVarFieldArray[i].Name == BVariableField.Name)
                        {
                            AVariableField = aVarFieldArray[i];
                            break;
                        }
                    }

                    if (AVariableField == null)
                    {
                        continue;
                    }

                    if (shouldCopyFieldFunc != null && !shouldCopyFieldFunc(BVariableField))
                    {
                        continue;
                    }

                    // Get the Variable from this category that matched
                    if (AVariableField != null)
                    {
                        // Get the final Rounding of the variable from EFT group, and set the SAIN Setting variable to that multiplier
                        object AValue = AVariableField.GetValue(ACatObject);
                        BVariableField.SetValue(BCatObject, AValue);
                        //Logger.LogWarning($"Set [{BVariableField.LayerName}] to [{AValue}]");
                    }
                }
            }
        }
    }

    private bool ShallUseEFTBotDefault(FieldInfo field)
    {
        return AttributesGUI.GetAttributeInfo(field)?.CopyValue == true;
    }

    public void LoadEFTSettings()
    {
        BotDifficulty[] Difficulties = EnumValues.Difficulties;
        foreach (var BotType in BotTypeDefinitions.BotTypesList)
        {
            string name = BotType.Name;
            WildSpawnType wildSpawnType = BotType.WildSpawnType;

            if (!EFTSettings.ContainsKey(wildSpawnType))
            {
                if (!Load.LoadObject(out EFTBotSettings eftSettings, name, "Default Bot Config Values") || eftSettings == null)
                {
                    Logger.LogError($"Failed to Import EFT Bot Settings for {name}");
                    eftSettings = new EFTBotSettings(name, wildSpawnType, Difficulties);
                    SaveObjectToJson(eftSettings, name, "Default Bot Config Values");
                }

                EFTSettings.Add(wildSpawnType, eftSettings);
            }
        }
    }

    public SAINSettingsClass GetSAINSettings(WildSpawnType type, BotDifficulty difficulty)
    {
        LoadEFTSettings();
        if (SAINSettings.TryGetValue(type, out var settingsGroup))
        {
            if (settingsGroup.Settings.TryGetValue(difficulty, out var settings))
            {
                return settings;
            }
            else
            {
                Logger.LogError($"[{difficulty}] does not exist in [{type}] SAIN Settings!");
            }
        }
        else
        {
            Logger.LogError($"[{type}] does not exist in SAINSettings Dictionary!");
        }
        return SAINSettings[WildSpawnType.pmcUSEC].Settings[BotDifficulty.normal];
    }

    // Todo: convert to BotsSettingsComponents in SPT 4.1, can't be done at the moment due to More Bots API
    public object GetEFTSettings(WildSpawnType type, BotDifficulty difficulty)
    {
        LoadEFTSettings();
        if (EFTSettings.TryGetValue(type, out var settingsGroup))
        {
            if (settingsGroup.Settings.TryGetValue(difficulty, out var settings))
            {
                return settings;
            }
            else
            {
                Logger.LogError($"[{difficulty}] does not exist in [{type}] Settings Group!");
            }
        }
        else
        {
            Logger.LogError($"[{type}] does not exist in EFTSettings Dictionary!");
        }
        return null;
    }
}
