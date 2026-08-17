using SAIN.Components;
using SAIN.Preset.Shared;
using SAIN.Preset.Shared.GlobalSettings.Categories;

namespace SAIN.Extensions;

public static class LocationSettingsExtensions
{
    public static DifficultySettings Current(this SAINLocationSettingsClass settings)
    {
        var gameworld = GameWorldComponent.Instance;
        if (gameworld == null || gameworld.Location == null)
        {
            Logger.LogError($"Gameworld or location is null");
            return null;
        }

        if (settings.LocationSettings.TryGetValue(gameworld.Location.Location, out var locationSettings))
        {
            return locationSettings;
        }

        Logger.LogError($"No settings for {gameworld.Location.Location}");

        return null;
    }
}
